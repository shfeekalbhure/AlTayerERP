using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace AlTayerERP.Desktop;

/// <summary>طلب صرف مستقل: لا ينشئ قيداً ولا يعرض أي أمر ترحيل.</summary>
public sealed class FrmPaymentRequest : BaseForm
{
    private readonly TextBox _number=new(){Width=150,PlaceholderText="رقم الطلب أو جزء منه"};
    private readonly TextBox _beneficiary=new(){Width=220,PlaceholderText="المستفيد"};
    private readonly TextBox _party=new(){Width=120,PlaceholderText="معرف الطرف"};
    private readonly TextBox _reference=new(){Width=150,PlaceholderText="مرجع الرأس"};
    private readonly TextBox _description=new(){Width=260,PlaceholderText="البيان"};
    private readonly TextBox _reason=new(){Width=200,PlaceholderText="سبب إلزامي"};
    private readonly TextBox _cash=new(){Width=170,PlaceholderText="حساب الصندوق/البنك لإنشاء السند"};
    private readonly DataGridView _lines=new(){Dock=DockStyle.Fill,AutoGenerateColumns=false,AllowUserToAddRows=true};
    private long _id;
    private string _status = "DRAFT";
    private long? _paymentVoucherId;
    private Button? _btnSubmit;
    private Button? _btnReview;
    private Button? _btnApprove;
    private Button? _btnReject;
    private Button? _btnReturn;
    private Button? _btnCreateVoucher;
    public FrmPaymentRequest()
    {
        Text="طلب الصرف";ApplyBaseFormStyle();RightToLeft=RightToLeft.Yes;RightToLeftLayout=true;KeyPreview=true;Build();
        KeyDown+=async(_,e)=>{if(e.KeyCode==Keys.F2)NewRequest();if(e.Control&&e.KeyCode==Keys.S)await SaveAsync();if(e.Control&&e.KeyCode==Keys.F){_number.Focus();e.Handled=true;}if(e.KeyCode==Keys.Escape)Close();};
    }
    private void Build()
    {
        var head=new FlowLayoutPanel{Dock=DockStyle.Top,Height=80,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(10),WrapContents=true};
        head.Controls.AddRange(new Control[]{L("رقم الطلب"),_number,L("المستفيد"),_beneficiary,L("الطرف"),_party,L("المرجع"),_reference,L("البيان"),_description,_reason,_cash});
        var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=44,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(8,5,8,5)};
        _btnSubmit=B("إرسال للمراجعة",async(_,_)=>await ActionAsync("submit",false));
        _btnReview=B("مراجعة",async(_,_)=>await ActionAsync("review",false));
        _btnApprove=B("اعتماد",async(_,_)=>await ActionAsync("approve",true));
        _btnReject=B("رفض",async(_,_)=>await ActionAsync("reject",true));
        _btnReturn=B("إرجاع",async(_,_)=>await ActionAsync("return",true));
        _btnCreateVoucher=B("إنشاء سند الصرف",async(_,_)=>await CreateVoucherAsync());
        bar.Controls.AddRange(new Control[]{B("جديد F2",(_,_)=>NewRequest()),B("حفظ Ctrl+S",async(_,_)=>await SaveAsync()),B("بحث",async(_,_)=>await SearchAsync()),_btnSubmit,_btnReview,_btnApprove,_btnReject,_btnReturn,B("مرفقات",async(_,_)=>await UploadAttachmentAsync()),_btnCreateVoucher});
        foreach(var x in new[]{("Account","الحساب"),("CostCenter","مركز التكلفة"),("Currency","العملة"),("Rate","سعر الصرف"),("Foreign","أجنبي"),("Local","محلي"),("Reference","المرجع"),("Description","الوصف")})_lines.Columns.Add(new DataGridViewTextBoxColumn{Name=x.Item1,HeaderText=x.Item2,Width=125});
        Controls.Add(_lines);Controls.Add(bar);Controls.Add(head);NewRequest();
    }
    private static Label L(string x)=>new(){Text=x,AutoSize=true,Margin=new Padding(7,8,2,2)};private static Button B(string x,EventHandler h){var b=new Button{Text=x,AutoSize=true};b.Click+=h;return b;}
    private void NewRequest(){_id=0;_status="DRAFT";_paymentVoucherId=null;ApplyWorkflowState();_number.Clear();_beneficiary.Clear();_party.Clear();_reference.Clear();_description.Clear();_reason.Clear();_cash.Clear();_lines.Rows.Clear();_lines.Rows.Add();_lines.Rows.Add();}
    private static decimal D(DataGridViewRow r,string name)=>decimal.TryParse(r.Cells[name].Value?.ToString(),out var x)?x:0;
    private object Payload()=>new{request_Date=DateTime.Today,beneficiary_Name=_beneficiary.Text.Trim(),party_ID=string.IsNullOrWhiteSpace(_party.Text)?null:_party.Text.Trim(),header_Reference_No=string.IsNullOrWhiteSpace(_reference.Text)?null:_reference.Text.Trim(),description=_description.Text.Trim(),lines=_lines.Rows.Cast<DataGridViewRow>().Where(r=>!r.IsNewRow&&!string.IsNullOrWhiteSpace(r.Cells["Account"].Value?.ToString())).Select(r=>new{account_ID=r.Cells["Account"].Value!.ToString(),cost_Center_ID=r.Cells["CostCenter"].Value?.ToString(),currency_ID=int.TryParse(r.Cells["Currency"].Value?.ToString(),out var c)?c:0,exchange_Rate=D(r,"Rate"),foreign_Amount=D(r,"Foreign"),local_Amount=D(r,"Local"),reference_No=r.Cells["Reference"].Value?.ToString(),description=r.Cells["Description"].Value?.ToString()}).ToList()};
    private async Task SaveAsync(){if(string.IsNullOrWhiteSpace(_beneficiary.Text)){MessageBox.Show("المستفيد مطلوب.");return;}var response=_id==0?await ApiService.Client.PostAsJsonAsync("payment-requests",Payload()):await ApiService.Client.PutAsJsonAsync($"payment-requests/{_id}",Payload());var text=await response.Content.ReadAsStringAsync();if(!response.IsSuccessStatusCode){MessageBox.Show(text,"طلب الصرف");return;}using var d=JsonDocument.Parse(text);_id=d.RootElement.GetProperty("payment_Request_ID").GetInt64();if(d.RootElement.TryGetProperty("request_No",out var no))_number.Text=no.GetString()??"";if(d.RootElement.TryGetProperty("status",out var status))_status=status.GetString()??"DRAFT";_paymentVoucherId=d.RootElement.TryGetProperty("payment_Voucher_ID",out var paymentVoucher)&&paymentVoucher.ValueKind!=JsonValueKind.Null?paymentVoucher.GetInt64():null;ApplyWorkflowState();MessageBox.Show("تم حفظ طلب الصرف كمسودة.");}
    private async Task SearchAsync(){if(string.IsNullOrWhiteSpace(_number.Text)){_number.Focus();return;}var raw=await ApiService.Client.GetStringAsync($"payment-requests?requestNo={Uri.EscapeDataString(_number.Text.Trim())}");using var list=JsonDocument.Parse(raw);var first=list.RootElement.EnumerateArray().FirstOrDefault();if(first.ValueKind==JsonValueKind.Undefined){MessageBox.Show("لم يتم العثور على الطلب.");return;}_id=first.GetProperty("payment_Request_ID").GetInt64();var r=await ApiService.Client.GetAsync($"payment-requests/{_id}");if(!r.IsSuccessStatusCode){MessageBox.Show(await r.Content.ReadAsStringAsync());return;}using var doc=JsonDocument.Parse(await r.Content.ReadAsStringAsync());var x=doc.RootElement;_number.Text=x.GetProperty("request_No").GetString()??"";_beneficiary.Text=x.GetProperty("beneficiary_Name").GetString()??"";_party.Text=x.TryGetProperty("party_ID",out var p)?p.GetString()??"":"";_reference.Text=x.TryGetProperty("header_Reference_No",out var rf)?rf.GetString()??"":"";_description.Text=x.TryGetProperty("description",out var de)?de.GetString()??"":"";_lines.Rows.Clear();foreach(var line in x.GetProperty("details").EnumerateArray())_lines.Rows.Add(line.GetProperty("account_ID").GetString(),line.TryGetProperty("cost_Center_ID",out var cc)?cc.GetString():null,line.GetProperty("currency_ID").GetInt32(),line.GetProperty("exchange_Rate").GetDecimal(),line.GetProperty("foreign_Amount").GetDecimal(),line.GetProperty("local_Amount").GetDecimal(),line.TryGetProperty("reference_No",out var rr)?rr.GetString():null,line.TryGetProperty("description",out var dd)?dd.GetString():null);_status=x.TryGetProperty("status",out var status)?status.GetString()??"DRAFT":"DRAFT";_paymentVoucherId=x.TryGetProperty("payment_Voucher_ID",out var paymentVoucher)&&paymentVoucher.ValueKind!=JsonValueKind.Null?paymentVoucher.GetInt64():null;ApplyWorkflowState();}

    private async Task ActionAsync(string action,bool reason)
    {
        if(_id<=0){MessageBox.Show("احفظ الطلب أولاً.");return;}
        if(reason&&string.IsNullOrWhiteSpace(_reason.Text)){MessageBox.Show("السبب إلزامي.");return;}
        var r=await ApiService.Client.PostAsJsonAsync($"payment-requests/{_id}/{action}",new{reason=_reason.Text});
        var responseText=await r.Content.ReadAsStringAsync();
        if(!r.IsSuccessStatusCode){MessageBox.Show(responseText);return;}
        using var document=JsonDocument.Parse(responseText);
        if(document.RootElement.TryGetProperty("status",out var status)) _status=status.GetString()??_status;
        ApplyWorkflowState();
        MessageBox.Show("تم تنفيذ العملية.");
    }
    private async Task UploadAttachmentAsync(){if(_id<=0){MessageBox.Show("احفظ الطلب أولاً.");return;}using var dialog=new OpenFileDialog();if(dialog.ShowDialog(this)!=DialogResult.OK)return;using var form=new MultipartFormDataContent();using var stream=File.OpenRead(dialog.FileName);using var file=new StreamContent(stream);form.Add(file,"file",Path.GetFileName(dialog.FileName));var r=await ApiService.Client.PostAsync($"payment-requests/{_id}/attachments",form);MessageBox.Show(r.IsSuccessStatusCode?"تم حفظ المرفق.":await r.Content.ReadAsStringAsync());}
    private async Task CreateVoucherAsync()
    {
        if(!string.Equals(_status,"APPROVED",StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("لا يمكن إنشاء سند الصرف إلا من طلب معتمد.");
            return;
        }
        if(_id<=0||string.IsNullOrWhiteSpace(_cash.Text)){MessageBox.Show("احفظ الطلب ثم أدخل حساب الصندوق/البنك.");return;}
        var r=await ApiService.Client.PostAsJsonAsync($"payment-requests/{_id}/create-payment-voucher",new{cash_Account_ID=_cash.Text.Trim()});
        var responseText=await r.Content.ReadAsStringAsync();
        if(!r.IsSuccessStatusCode){MessageBox.Show(responseText);return;}
        using var document=JsonDocument.Parse(responseText);
        _paymentVoucherId=document.RootElement.TryGetProperty("voucherId",out var voucherId)?voucherId.GetInt64():-1;
        ApplyWorkflowState();
        MessageBox.Show("تم إنشاء سند الصرف المرتبط.");
    }

    private void ApplyWorkflowState()
    {
        var canSubmit=_id>0&&string.Equals(_status,"DRAFT",StringComparison.OrdinalIgnoreCase);
        var canReview=_id>0&&string.Equals(_status,"PENDING_REVIEW",StringComparison.OrdinalIgnoreCase);
        var canDecide=_id>0&&string.Equals(_status,"PENDING_APPROVAL",StringComparison.OrdinalIgnoreCase);
        if(_btnSubmit is not null) _btnSubmit.Enabled=canSubmit;
        if(_btnReview is not null) _btnReview.Enabled=canReview;
        if(_btnApprove is not null) _btnApprove.Enabled=canDecide;
        if(_btnReject is not null) _btnReject.Enabled=canDecide;
        if(_btnReturn is not null) _btnReturn.Enabled=canDecide;
        if(_btnCreateVoucher is not null)
        {
            _btnCreateVoucher.Enabled=_id>0&&_paymentVoucherId is null&&string.Equals(_status,"APPROVED",StringComparison.OrdinalIgnoreCase);
            _btnCreateVoucher.Visible=_btnCreateVoucher.Enabled;
        }
    }
}