using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Net.Http.Json;
using System.Text.Json;

namespace AlTayerERP.Desktop;

/// <summary>طلب صرف مستقل: لا ينشئ قيداً ولا يعرض أي أمر ترحيل.</summary>
public sealed class FrmPaymentRequest : BaseForm
{
    private readonly TextBox _beneficiary=new(){Width=220,PlaceholderText="المستفيد"};
    private readonly TextBox _party=new(){Width=120,PlaceholderText="معرف الطرف"};
    private readonly TextBox _reference=new(){Width=150,PlaceholderText="مرجع الرأس"};
    private readonly TextBox _description=new(){Width=260,PlaceholderText="البيان"};
    private readonly TextBox _reason=new(){Width=200,PlaceholderText="سبب إلزامي"};
    private readonly TextBox _cash=new(){Width=170,PlaceholderText="حساب الصندوق/البنك لإنشاء السند"};
    private readonly DataGridView _lines=new(){Dock=DockStyle.Fill,AutoGenerateColumns=false,AllowUserToAddRows=true};
    private long _id;
    public FrmPaymentRequest()
    {
        Text="طلب الصرف";ApplyBaseFormStyle();RightToLeft=RightToLeft.Yes;RightToLeftLayout=true;KeyPreview=true;Build();
        KeyDown+=async(_,e)=>{if(e.KeyCode==Keys.F2)NewRequest();if(e.Control&&e.KeyCode==Keys.S)await SaveAsync();if(e.KeyCode==Keys.Escape)Close();};
    }
    private void Build()
    {
        var head=new FlowLayoutPanel{Dock=DockStyle.Top,Height=80,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(10),WrapContents=true};
        head.Controls.AddRange(new Control[]{L("المستفيد"),_beneficiary,L("الطرف"),_party,L("المرجع"),_reference,L("البيان"),_description,_reason,_cash});
        var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=44,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(8,5,8,5)};
        bar.Controls.AddRange(new Control[]{B("جديد F2",(_,_)=>NewRequest()),B("حفظ Ctrl+S",async(_,_)=>await SaveAsync()),B("إرسال للمراجعة",async(_,_)=>await ActionAsync("submit",false)),B("مراجعة",async(_,_)=>await ActionAsync("review",true)),B("اعتماد",async(_,_)=>await ActionAsync("approve",true)),B("رفض",async(_,_)=>await ActionAsync("reject",true)),B("إرجاع",async(_,_)=>await ActionAsync("return",true)),B("إنشاء سند الصرف",async(_,_)=>await CreateVoucherAsync())});
        foreach(var x in new[]{("Account","الحساب"),("CostCenter","مركز التكلفة"),("Currency","العملة"),("Rate","سعر الصرف"),("Foreign","أجنبي"),("Local","محلي"),("Reference","المرجع"),("Description","الوصف")})_lines.Columns.Add(new DataGridViewTextBoxColumn{Name=x.Item1,HeaderText=x.Item2,Width=125});
        Controls.Add(_lines);Controls.Add(bar);Controls.Add(head);NewRequest();
    }
    private static Label L(string x)=>new(){Text=x,AutoSize=true,Margin=new Padding(7,8,2,2)};private static Button B(string x,EventHandler h){var b=new Button{Text=x,AutoSize=true};b.Click+=h;return b;}
    private void NewRequest(){_id=0;_beneficiary.Clear();_party.Clear();_reference.Clear();_description.Clear();_reason.Clear();_cash.Clear();_lines.Rows.Clear();_lines.Rows.Add();_lines.Rows.Add();}
    private static decimal D(DataGridViewRow r,string name)=>decimal.TryParse(r.Cells[name].Value?.ToString(),out var x)?x:0;
    private object Payload()=>new{request_Date=DateTime.Today,beneficiary_Name=_beneficiary.Text.Trim(),party_ID=string.IsNullOrWhiteSpace(_party.Text)?null:_party.Text.Trim(),header_Reference_No=string.IsNullOrWhiteSpace(_reference.Text)?null:_reference.Text.Trim(),description=_description.Text.Trim(),lines=_lines.Rows.Cast<DataGridViewRow>().Where(r=>!r.IsNewRow&&!string.IsNullOrWhiteSpace(r.Cells["Account"].Value?.ToString())).Select(r=>new{account_ID=r.Cells["Account"].Value!.ToString(),cost_Center_ID=r.Cells["CostCenter"].Value?.ToString(),currency_ID=int.TryParse(r.Cells["Currency"].Value?.ToString(),out var c)?c:0,exchange_Rate=D(r,"Rate"),foreign_Amount=D(r,"Foreign"),local_Amount=D(r,"Local"),reference_No=r.Cells["Reference"].Value?.ToString(),description=r.Cells["Description"].Value?.ToString()}).ToList()};
    private async Task SaveAsync(){if(string.IsNullOrWhiteSpace(_beneficiary.Text)){MessageBox.Show("المستفيد مطلوب.");return;}var response=_id==0?await ApiService.Client.PostAsJsonAsync("payment-requests",Payload()):await ApiService.Client.PutAsJsonAsync($"payment-requests/{_id}",Payload());var text=await response.Content.ReadAsStringAsync();if(!response.IsSuccessStatusCode){MessageBox.Show(text,"طلب الصرف");return;}using var d=JsonDocument.Parse(text);_id=d.RootElement.GetProperty("payment_Request_ID").GetInt64();MessageBox.Show("تم حفظ طلب الصرف كمسودة.");}
    private async Task ActionAsync(string action,bool reason){if(_id<=0){MessageBox.Show("احفظ الطلب أولاً.");return;}if(reason&&string.IsNullOrWhiteSpace(_reason.Text)){MessageBox.Show("السبب إلزامي.");return;}var r=await ApiService.Client.PostAsJsonAsync($"payment-requests/{_id}/{action}",new{reason=_reason.Text});MessageBox.Show(r.IsSuccessStatusCode?"تم تنفيذ العملية.":await r.Content.ReadAsStringAsync());}
    private async Task CreateVoucherAsync(){if(_id<=0||string.IsNullOrWhiteSpace(_cash.Text)){MessageBox.Show("احفظ الطلب ثم أدخل حساب الصندوق/البنك.");return;}var r=await ApiService.Client.PostAsJsonAsync($"payment-requests/{_id}/create-payment-voucher",new{cash_Account_ID=_cash.Text.Trim()});MessageBox.Show(r.IsSuccessStatusCode?"تم إنشاء سند الصرف المرتبط.":await r.Content.ReadAsStringAsync());}
}