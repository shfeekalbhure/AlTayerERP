using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace AlTayerERP.Desktop;

/// <summary>سند صرف مستقل؛ لا يرث سند القبض ولا يملك اتصالاً مباشراً بقاعدة البيانات.</summary>
public sealed class FrmPaymentVoucher : BaseForm
{
    private readonly ComboBox _type = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 175 };
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 145 };
    private readonly ComboBox _cash = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 210 };
    private readonly ComboBox _party = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 210 };
    private readonly ComboBox _method = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly ComboBox _currency = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short, Width = 120 };
    private readonly NumericUpDown _rate = new() { DecimalPlaces = 6, Minimum = .000001m, Maximum = 999999m, Value = 1, Width = 110 };
    private readonly TextBox _reference = new() { Width = 150, PlaceholderText = "رقم شيك/تحويل/مرجع" };
    private readonly TextBox _beneficiary = new() { Width = 210, PlaceholderText = "المستفيد" };
    private readonly TextBox _narration = new() { Width = 330, PlaceholderText = "البيان المحاسبي" };
    private readonly TextBox _reason = new() { Width = 210, PlaceholderText = "سبب إلزامي لفك الترحيل" };
    private readonly TextBox _number = new() { Width = 150, ReadOnly = true };
    private readonly DataGridView _lines = new() { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = true };
    private readonly Label _totals = new() { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(10) };
    private long _voucherId;
    private List<TypeRow> _types = new(); private List<StatusRow> _statuses = new(); private List<CurrencyRow> _currencies = new();
    private List<CashRow> _cashBoxes = new(); private List<PartyRow> _parties = new(); private List<MethodRow> _methods = new();

    public FrmPaymentVoucher()
    {
        Text = "سند الصرف"; ApplyBaseFormStyle(); RightToLeft = RightToLeft.Yes; RightToLeftLayout = true; KeyPreview = true;
        Build(); Load += async (_, _) => await LoadLookupsAsync();
        KeyDown += async (_, e) => { if (e.KeyCode == Keys.F2) NewVoucher(); if (e.Control && e.KeyCode == Keys.S) await SaveAsync(); if (e.KeyCode == Keys.F5) await LoadLookupsAsync(); if (e.KeyCode == Keys.Escape) Close(); };
    }

    private void Build()
    {
        var header = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 130, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(10), WrapContents = true };
        header.Controls.AddRange(new Control[] { L("رقم السند"), _number, L("التاريخ"), _date, L("النوع"), _type, L("الحالة"), _status, L("الصندوق/البنك الدائن"), _cash, L("المستفيد"), _party, _beneficiary, L("طريقة السداد"), _method, L("العملة"), _currency, L("سعر الصرف"), _rate, _reference, _narration, _reason });
        var commands = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 42, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(8, 5, 8, 5) };
        commands.Controls.AddRange(new Control[] { B("جديد F2", (_, _) => NewVoucher()), B("حفظ Ctrl+S", async (_, _) => await SaveAsync()), B("بحث", async (_, _) => await SearchAsync()), B("مرفقات", (_, _) => OpenAttachments()), B("مراجعة", async (_, _) => await ActionAsync("review", false)), B("اعتماد", async (_, _) => await ActionAsync("approve", false)), B("ترحيل", async (_, _) => await ActionAsync("post", false)), B("فك ترحيل", async (_, _) => await ActionAsync("unpost", true)), B("عرض القيد", async (_, _) => await ViewJournalAsync()) });
        _lines.Columns.AddRange(new DataGridViewColumn[] { C("Account","الحساب"), C("CostCenter","مركز التكلفة"), C("Currency","العملة"), C("Rate","سعر الصرف"), C("Foreign","أجنبي"), C("Local","محلي"), C("Debit","مدين"), C("Credit","دائن"), C("Description","الوصف", 220), C("Notes","ملاحظات", 180) });
        _lines.CellValueChanged += (_, _) => Totals(); _lines.RowsRemoved += (_, _) => Totals();
        Controls.Add(_lines); Controls.Add(_totals); Controls.Add(commands); Controls.Add(header);
    }
    private static Label L(string text) => new() { Text = text, AutoSize = true, Margin = new Padding(7, 8, 2, 2) };
    private static Button B(string text, EventHandler click) { var b = new Button { Text = text, AutoSize = true, Height = 29 }; b.Click += click; return b; }
    private static DataGridViewTextBoxColumn C(string name, string header, int width = 120) => new() { Name = name, HeaderText = header, Width = width };

    private async Task LoadLookupsAsync()
    {
        try {
            var x = await ApiService.Client.GetFromJsonAsync<Lookup>($"FinancialVoucherLookups?companyId={Uri.EscapeDataString(CurrentSession.Company_ID)}&branchId={CurrentSession.Branch_ID}") ?? throw new InvalidOperationException("تعذر تحميل القوائم المرجعية.");
            _types = x.VoucherTypes.Where(v => v.Is_Active && v.Voucher_Type_Code.Equals("PAYMENT", StringComparison.OrdinalIgnoreCase)).ToList(); _statuses = x.VoucherStatuses.Where(v => v.Is_Active).ToList(); _currencies = x.Currencies.Where(v => v.Is_Active).ToList(); _cashBoxes = x.CashBoxes; _parties = x.Parties; _methods = x.PaymentMethods;
            Bind(_type,_types,nameof(TypeRow.Voucher_Type_Name_AR),nameof(TypeRow.Voucher_Type_ID)); Bind(_status,_statuses,nameof(StatusRow.Voucher_Status_Name_AR),nameof(StatusRow.Voucher_Status_ID)); Bind(_currency,_currencies,nameof(CurrencyRow.Currency_Name_AR),nameof(CurrencyRow.Currency_ID)); Bind(_cash,_cashBoxes,nameof(CashRow.Display),nameof(CashRow.Account_ID)); Bind(_party,_parties,nameof(PartyRow.Display),nameof(PartyRow.Party_ID)); Bind(_method,_methods,nameof(MethodRow.Payment_Method_Name_AR),nameof(MethodRow.Payment_Method_ID));
            var draft=_statuses.FirstOrDefault(s=>s.Voucher_Status_Code.Equals("DRAFT",StringComparison.OrdinalIgnoreCase)); if(draft!=null)_status.SelectedValue=draft.Voucher_Status_ID; var local=_currencies.FirstOrDefault(c=>c.Is_Local_Currency)??_currencies.FirstOrDefault(); if(local!=null)_currency.SelectedValue=local.Currency_ID; NewVoucher();
        } catch(Exception ex) { MessageBox.Show(ex.Message,"سند الصرف",MessageBoxButtons.OK,MessageBoxIcon.Warning); }
    }
    private static void Bind<T>(ComboBox box,List<T> list,string display,string value){ box.DataSource=null; box.DisplayMember=display; box.ValueMember=value; box.DataSource=list; }
    private void NewVoucher(){_voucherId=0;_number.Clear();_reference.Clear();_beneficiary.Clear();_narration.Clear();_reason.Clear();_lines.Rows.Clear();_lines.Rows.Add();_lines.Rows.Add();Totals();}
    private decimal D(DataGridViewRow r,string n)=>decimal.TryParse(r.Cells[n].Value?.ToString(),NumberStyles.Any,CultureInfo.CurrentCulture,out var x)?x:0;
    private void Totals(){var rows=_lines.Rows.Cast<DataGridViewRow>().Where(r=>!r.IsNewRow).ToList();var debit=rows.Sum(r=>D(r,"Debit"));var credit=rows.Sum(r=>D(r,"Credit"));_totals.Text=$"إجمالي المدين: {debit:N2} | إجمالي الدائن: {credit:N2} | الفرق: {(debit-credit):N2}";}

    private async Task SaveAsync()
    {
        var rows=_lines.Rows.Cast<DataGridViewRow>().Where(r=>!r.IsNewRow&&!string.IsNullOrWhiteSpace(r.Cells["Account"].Value?.ToString())).ToList(); var debit=rows.Sum(r=>D(r,"Debit")); var credit=rows.Sum(r=>D(r,"Credit"));
        if(_type.SelectedValue==null||_status.SelectedValue==null||_cash.SelectedValue==null||_currency.SelectedValue==null||string.IsNullOrWhiteSpace(_narration.Text)||string.IsNullOrWhiteSpace(_beneficiary.Text)||rows.Count==0||debit<=0||Math.Round(credit,2)!=Math.Round(debit,2)){MessageBox.Show("أكمل بيانات الصرف، والمستفيد، والأسطر المدينة المتوازنة.");return;}
        var currencyId=Convert.ToInt32(_currency.SelectedValue); var currency=_currencies.First(c=>c.Currency_ID==currencyId); var rate=currency.Is_Local_Currency?1m:_rate.Value;
        var cashId=_cash.SelectedValue?.ToString()??""; var dto=new { Voucher_Type_ID=Convert.ToInt32(_type.SelectedValue),Voucher_Status_ID=Convert.ToInt32(_status.SelectedValue),Voucher_Date=_date.Value.Date,Transaction_Date=_date.Value,Cash_Account_ID=cashId,Party_ID=string.IsNullOrWhiteSpace(_party.SelectedValue?.ToString())?null:_party.SelectedValue!.ToString(),Received_From_Name=_beneficiary.Text.Trim(),Payment_Method_ID=Convert.ToInt32(_method.SelectedValue),Currency_ID=currencyId,Exchange_Rate=rate,Amount=debit,Foreign_Total=currency.Is_Local_Currency?0m:debit,Local_Total=debit,Reference_No=string.IsNullOrWhiteSpace(_reference.Text)?null:_reference.Text.Trim(),Reference_Date=_date.Value.Date,Against_Text=_narration.Text.Trim(),Description=_narration.Text.Trim(),Notes=(string?)null,Requires_Approval=true,Details=rows.Select((r,i)=>new {Line_No=i+2,Account_ID=r.Cells["Account"].Value!.ToString(),Cost_Center_ID=r.Cells["CostCenter"].Value?.ToString(),Currency_ID=currencyId,Exchange_Rate=rate,Foreign_Amount=currency.Is_Local_Currency?0m:D(r,"Debit"),Local_Amount=D(r,"Debit"),Debit_Amount=D(r,"Debit"),Credit_Amount=0m,Line_Type=(byte)2,Description=r.Cells["Description"].Value?.ToString(),Notes=r.Cells["Notes"].Value?.ToString()}).Prepend(new {Line_No=1,Account_ID=cashId,Cost_Center_ID=(string?)null,Currency_ID=currencyId,Exchange_Rate=rate,Foreign_Amount=currency.Is_Local_Currency?0m:credit,Local_Amount=credit,Debit_Amount=0m,Credit_Amount=credit,Line_Type=(byte)1,Description=_narration.Text,Notes=(string?)null}).ToList()};
        var response=await ApiService.Client.PostAsJsonAsync("FinancialVoucher",dto);var text=await response.Content.ReadAsStringAsync();if(!response.IsSuccessStatusCode){MessageBox.Show(text,"تعذر الحفظ",MessageBoxButtons.OK,MessageBoxIcon.Warning);return;}using var doc=JsonDocument.Parse(text);_voucherId=doc.RootElement.GetProperty("voucher_ID").GetInt64();_number.Text=doc.RootElement.GetProperty("voucher_No").GetString()??"";MessageBox.Show("تم حفظ سند الصرف كمسودة.","سند الصرف");
    }
    private async Task SearchAsync(){if(string.IsNullOrWhiteSpace(_number.Text)){_number.ReadOnly=false;_number.Focus();return;}var response=await ApiService.Client.GetAsync($"FinancialVoucher/ByNumber?voucherNumber={Uri.EscapeDataString(_number.Text)}&voucherTypeId={Convert.ToInt32(_type.SelectedValue)}");var text=await response.Content.ReadAsStringAsync();if(!response.IsSuccessStatusCode){MessageBox.Show(text);return;}using var d=JsonDocument.Parse(text);var data=d.RootElement.GetProperty("data");_voucherId=data.GetProperty("voucher_ID").GetInt64();_number.Text=data.GetProperty("voucher_No").GetString()??"";}
    private async Task ActionAsync(string action,bool reason){if(_voucherId<=0){MessageBox.Show("احفظ أو ابحث عن السند أولاً.");return;}if(reason&&string.IsNullOrWhiteSpace(_reason.Text)){MessageBox.Show("سبب فك الترحيل إلزامي.");return;}HttpResponseMessage response=reason?await ApiService.Client.PostAsJsonAsync($"FinancialVoucher/{_voucherId}/{action}",new {Reason=_reason.Text,Action_Channel="DESKTOP"}):await ApiService.Client.PostAsJsonAsync($"FinancialVoucher/{_voucherId}/{action}",new {Notes=_narration.Text,Action_Channel="DESKTOP"});MessageBox.Show(response.IsSuccessStatusCode?"تم تنفيذ العملية.":await response.Content.ReadAsStringAsync());}
    private void OpenAttachments(){if(_voucherId<=0){MessageBox.Show("احفظ سند الصرف أولاً.");return;}new FrmVoucherAttachments(_voucherId).ShowDialog(this);}
    private async Task ViewJournalAsync(){if(string.IsNullOrWhiteSpace(_number.Text))return;var r=await ApiService.Client.GetAsync($"FinancialVoucher/journal-entry/{Uri.EscapeDataString(_number.Text)}?voucherTypeId={Convert.ToInt32(_type.SelectedValue)}");MessageBox.Show(await r.Content.ReadAsStringAsync(),"القيد الناتج");}

    private sealed class Lookup{public List<TypeRow> VoucherTypes{get;set;}=new();public List<StatusRow> VoucherStatuses{get;set;}=new();public List<CurrencyRow>Currencies{get;set;}=new();public List<CashRow>CashBoxes{get;set;}=new();public List<PartyRow>Parties{get;set;}=new();public List<MethodRow>PaymentMethods{get;set;}=new();}
    private sealed class TypeRow{public int Voucher_Type_ID{get;set;}public string Voucher_Type_Code{get;set;}="";public string Voucher_Type_Name_AR{get;set;}="";public bool Is_Active{get;set;}=true;}
    private sealed class StatusRow{public int Voucher_Status_ID{get;set;}public string Voucher_Status_Code{get;set;}="";public string Voucher_Status_Name_AR{get;set;}="";public bool Is_Active{get;set;}=true;}
    private sealed class CurrencyRow{public int Currency_ID{get;set;}public string Currency_Name_AR{get;set;}="";public decimal Exchange_Rate{get;set;}=1;public bool Is_Local_Currency{get;set;}public bool Is_Active{get;set;}=true;}
    private sealed class CashRow{public string Account_ID{get;set;}="";public string Cash_Box_Name{get;set;}="";public string Cash_Box_Code{get;set;}="";public string Display=>$"{Cash_Box_Code} - {Cash_Box_Name}";}
    private sealed class PartyRow{public string Party_ID{get;set;}="";public string Party_Code{get;set;}="";public string Party_Name_AR{get;set;}="";public string Display=>$"{Party_Code} - {Party_Name_AR}";}
    private sealed class MethodRow{public int Payment_Method_ID{get;set;}public string Payment_Method_Name_AR{get;set;}="";}
}