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
        MinimumSize = new Size(980, 690);
        Build(); Load += async (_, _) => await LoadLookupsAsync();
        KeyDown += async (_, e) => { if (e.KeyCode == Keys.F2) NewVoucher(); if (e.Control && e.KeyCode == Keys.S) await SaveAsync(); if (e.KeyCode == Keys.F5) await LoadLookupsAsync(); if (e.KeyCode == Keys.Escape) Close(); };
    }

    private void Build()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6,
            Padding = new Padding(12), BackColor = Color.FromArgb(244, 247, 251)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 214));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));

        root.Controls.Add(new BrandHeaderControl("سند الصرف (Payment Voucher)"), 0, 0);
        root.Controls.Add(BuildCommandBar(), 0, 1);
        root.Controls.Add(BuildHeaderCard(), 0, 2);
        root.Controls.Add(BuildDetailsCard(), 0, 3);
        root.Controls.Add(CreateAuditInfoPanel(), 0, 4);
        root.Controls.Add(CreateSessionStatusStrip(), 0, 5);

        _lines.Columns.AddRange(new DataGridViewColumn[] { C("Account","الحساب"), C("CostCenter","مركز التكلفة"), C("Currency","العملة"), C("Rate","سعر الصرف"), C("Foreign","أجنبي"), C("Local","محلي"), C("Debit","مدين"), C("Credit","دائن"), C("Description","الوصف", 220), C("Notes","ملاحظات", 180) });
        _lines.RightToLeft = RightToLeft.Yes;
        _lines.RowHeadersVisible = false;
        _lines.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _lines.MultiSelect = false;
        _lines.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        _lines.CellValueChanged += (_, _) => Totals(); _lines.RowsRemoved += (_, _) => Totals();
        _lines.DefaultValuesNeeded += (_, e) => ApplyLineDefaults(e.Row);
        _lines.CellEndEdit += (_, e) => CalculateLine(_lines.Rows[e.RowIndex]);
        Controls.Add(root);
    }

    private Control BuildCommandBar()
    {
        var commands = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false, AutoScroll = true, Padding = new Padding(5, 6, 5, 4), BackColor = Color.White
        };
        commands.Controls.AddRange(new Control[]
        {
            B("جديد F2", (_, _) => NewVoucher()),
            B("حفظ Ctrl+S", async (_, _) => await SaveAsync()),
            B("بحث", async (_, _) => await SearchAsync()),
            B("تحديث F5", async (_, _) => await LoadLookupsAsync()),
            B("مراجعة", async (_, _) => await ActionAsync("review", false)),
            B("اعتماد", async (_, _) => await ActionAsync("approve", false)),
            B("ترحيل", async (_, _) => await ActionAsync("post", false)),
            B("فك ترحيل", async (_, _) => await ActionAsync("unpost", true)),
            B("عرض القيد", async (_, _) => await ViewJournalAsync()),
            B("مرفقات", (_, _) => OpenAttachments()),
            B("إغلاق", (_, _) => Close())
        });
        return commands;
    }

    private Control BuildHeaderCard()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
        var title = new Label { Text = "بيانات سند الصرف", Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112), TextAlign = ContentAlignment.MiddleRight };
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4, Padding = new Padding(0, 4, 0, 0) };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        for (var row = 0; row < 4; row++) fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        AddField(fields, 0, 0, "رقم السند", _number);
        AddField(fields, 1, 0, "تاريخ السند", _date);
        AddField(fields, 2, 0, "النوع", _type);
        AddField(fields, 3, 0, "الحالة", _status);
        AddField(fields, 0, 1, "الصندوق/البنك الدائن *", _cash);
        AddField(fields, 1, 1, "الطرف المالي", _party);
        AddField(fields, 2, 1, "اسم المستفيد *", _beneficiary);
        AddField(fields, 3, 1, "طريقة السداد *", _method);
        AddField(fields, 0, 2, "عملة الصندوق/البنك", _currency);
        AddField(fields, 1, 2, "سعر الصرف", _rate);
        AddField(fields, 2, 2, "رقم المرجع", _reference);
        AddField(fields, 3, 2, "البيان المحاسبي *", _narration);
        AddField(fields, 0, 3, "سبب فك الترحيل", _reason);
        fields.SetColumnSpan(_reason.Parent!, 4);

        card.Controls.Add(fields);
        card.Controls.Add(title);
        return card;
    }

    private Control BuildDetailsCard()
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
        var title = new Label { Text = "تفاصيل الحسابات المدينة", Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(8, 55, 112), TextAlign = ContentAlignment.MiddleRight };
        _totals.Dock = DockStyle.Bottom;
        _totals.Height = 34;
        _totals.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _totals.ForeColor = Color.FromArgb(8, 55, 112);
        _totals.TextAlign = ContentAlignment.MiddleRight;
        _totals.BackColor = Color.FromArgb(242, 247, 253);
        card.Controls.Add(_lines);
        card.Controls.Add(_totals);
        card.Controls.Add(title);
        return card;
    }

    private static void AddField(TableLayoutPanel table, int column, int row, string caption, Control input)
    {
        var field = new Panel { Dock = DockStyle.Fill, Margin = new Padding(4), Padding = new Padding(4), BorderStyle = BorderStyle.FixedSingle };
        var label = new Label { Text = caption, Dock = DockStyle.Top, Height = 16, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold), ForeColor = Color.FromArgb(48, 70, 96), TextAlign = ContentAlignment.MiddleRight };
        input.Dock = DockStyle.Fill;
        input.Margin = Padding.Empty;
        field.Controls.Add(input);
        field.Controls.Add(label);
        table.Controls.Add(field, column, row);
    }

    private static Button B(string text, EventHandler click) { var b = new Button { Text = text, Width = 104, Height = 32, Margin = new Padding(3) }; b.Click += click; return b; }
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
    private void NewVoucher(){_voucherId=0;_number.ReadOnly=true;_number.Clear();_date.Value=DateTime.Today;_reference.Clear();_beneficiary.Clear();_narration.Clear();_reason.Clear();_lines.Rows.Clear();_lines.Rows.Add();_lines.Rows.Add();Totals();}
    private void ApplyLineDefaults(DataGridViewRow row)
    {
        if (_currency.SelectedValue == null) return;
        row.Cells["Currency"].Value = Convert.ToInt32(_currency.SelectedValue);
        var local = _currencies.FirstOrDefault(x => x.Is_Local_Currency);
        var currencyId = Convert.ToInt32(_currency.SelectedValue);
        row.Cells["Rate"].Value = local != null && currencyId == local.Currency_ID ? 1m : _rate.Value;
        row.Cells["Foreign"].Value = 0m; row.Cells["Local"].Value = 0m;
    }
    private void CalculateLine(DataGridViewRow row)
    {
        if (row.IsNewRow || _currency.SelectedValue == null) return;
        var localId = _currencies.FirstOrDefault(x => x.Is_Local_Currency)?.Currency_ID;
        var currencyId = int.TryParse(row.Cells["Currency"].Value?.ToString(), out var parsed) ? parsed : Convert.ToInt32(_currency.SelectedValue);
        var isLocal = localId.HasValue && currencyId == localId.Value;
        var foreign = D(row,"Foreign"); var rate = D(row,"Rate");
        if (isLocal) { row.Cells["Foreign"].Value=0m; row.Cells["Rate"].Value=1m; row.Cells["Local"].Value=D(row,"Debit")+D(row,"Credit"); row.Cells["Foreign"].ReadOnly=true; row.Cells["Rate"].ReadOnly=true; }
        else { if(rate<=0) rate=_rate.Value; row.Cells["Rate"].Value=rate; row.Cells["Foreign"].ReadOnly=false; row.Cells["Rate"].ReadOnly=false; row.Cells["Local"].Value=Math.Round(foreign*rate,4); }
        Totals();
    }
    private decimal D(DataGridViewRow r,string n)=>decimal.TryParse(r.Cells[n].Value?.ToString(),NumberStyles.Any,CultureInfo.CurrentCulture,out var x)?x:0;
    private void Totals(){var rows=_lines.Rows.Cast<DataGridViewRow>().Where(r=>!r.IsNewRow).ToList();var debit=rows.Sum(r=>D(r,"Debit"));var credit=rows.Sum(r=>D(r,"Credit"));_totals.Text=$"إجمالي المدين: {debit:N2} | إجمالي الدائن: {credit:N2} | الفرق: {(debit-credit):N2}";}

    private async Task SaveAsync()
    {
        var local = _currencies.FirstOrDefault(c => c.Is_Local_Currency) ?? throw new InvalidOperationException("العملة المحلية غير مهيأة.");
        var rows=_lines.Rows.Cast<DataGridViewRow>().Where(r=>!r.IsNewRow&&!string.IsNullOrWhiteSpace(r.Cells["Account"].Value?.ToString())).ToList();
        var normalized=new List<PaymentVoucherDetailDto>(); decimal debitTotal=0m;
        foreach(var r in rows)
        {
            if(!int.TryParse(r.Cells["Currency"].Value?.ToString(),out var currencyId) || !_currencies.Any(c=>c.Currency_ID==currencyId&&c.Is_Active)){MessageBox.Show("عملة كل سطر مطلوبة ونشطة.");return;}
            var isLocal=currencyId==local.Currency_ID;var rate=isLocal?1m:D(r,"Rate");var foreign=isLocal?0m:D(r,"Foreign");var localAmount=isLocal?D(r,"Debit")+D(r,"Credit"):Math.Round(foreign*rate,4);var debit=D(r,"Debit");var credit=D(r,"Credit");
            if(rate<=0||foreign<0||localAmount<=0||debit<=0||credit!=0){MessageBox.Show("أسطر سند الصرف يجب أن تكون مدينة فقط، مع سعر صرف ومبلغ صالحين.");return;}
            debitTotal+=localAmount; normalized.Add(new PaymentVoucherDetailDto {Account_ID=r.Cells["Account"].Value!.ToString(),Cost_Center_ID=r.Cells["CostCenter"].Value?.ToString(),Currency_ID=currencyId,Exchange_Rate=rate,Foreign_Amount=foreign,Local_Amount=localAmount,Debit_Amount=localAmount,Credit_Amount=0m,Line_Type=(byte)2,Description=r.Cells["Description"].Value?.ToString(),Notes=r.Cells["Notes"].Value?.ToString()});
        }
        if(_type.SelectedValue==null||_status.SelectedValue==null||_cash.SelectedValue==null||_currency.SelectedValue==null||_method.SelectedValue==null||string.IsNullOrWhiteSpace(_narration.Text)||string.IsNullOrWhiteSpace(_beneficiary.Text)||normalized.Count==0||debitTotal<=0){MessageBox.Show("أكمل بيانات الصرف والمستفيد والأسطر المدينة.");return;}
        var cashCurrencyId=Convert.ToInt32(_currency.SelectedValue);var cashIsLocal=cashCurrencyId==local.Currency_ID;var cashRate=cashIsLocal?1m:_rate.Value;if(cashRate<=0){MessageBox.Show("سعر صرف الصندوق/البنك غير صالح.");return;}
        var dto=new { Voucher_Type_ID=Convert.ToInt32(_type.SelectedValue),Voucher_Status_ID=Convert.ToInt32(_status.SelectedValue),Voucher_Date=_date.Value.Date,Transaction_Date=_date.Value,Cash_Account_ID=_cash.SelectedValue!.ToString(),Party_ID=string.IsNullOrWhiteSpace(_party.SelectedValue?.ToString())?null:_party.SelectedValue!.ToString(),Received_From_Name=_beneficiary.Text.Trim(),Payment_Method_ID=Convert.ToInt32(_method.SelectedValue),Currency_ID=cashCurrencyId,Exchange_Rate=cashRate,Amount=debitTotal,Foreign_Total=cashIsLocal?0m:Math.Round(debitTotal/cashRate,4),Local_Total=debitTotal,Reference_No=string.IsNullOrWhiteSpace(_reference.Text)?null:_reference.Text.Trim(),Reference_Date=_date.Value.Date,Against_Text=_narration.Text.Trim(),Description=_narration.Text.Trim(),Notes=(string?)null,Requires_Approval=true,Details=normalized.Select((x,i)=>new PaymentVoucherDetailDto {Line_No=i+2,Account_ID=x.Account_ID,Cost_Center_ID=x.Cost_Center_ID,Currency_ID=x.Currency_ID,Exchange_Rate=x.Exchange_Rate,Foreign_Amount=x.Foreign_Amount,Local_Amount=x.Local_Amount,Debit_Amount=x.Debit_Amount,Credit_Amount=x.Credit_Amount,Line_Type=x.Line_Type,Description=x.Description,Notes=x.Notes}).Prepend(new PaymentVoucherDetailDto {Line_No=1,Account_ID=_cash.SelectedValue!.ToString(),Cost_Center_ID=(string?)null,Currency_ID=cashCurrencyId,Exchange_Rate=cashRate,Foreign_Amount=cashIsLocal?0m:Math.Round(debitTotal/cashRate,4),Local_Amount=debitTotal,Debit_Amount=0m,Credit_Amount=debitTotal,Line_Type=(byte)1,Description=_narration.Text,Notes=(string?)null}).ToList() };
        var response=await ApiService.Client.PostAsJsonAsync("FinancialVoucher",dto);var text=await response.Content.ReadAsStringAsync();if(!response.IsSuccessStatusCode){MessageBox.Show(text,"تعذر الحفظ",MessageBoxButtons.OK,MessageBoxIcon.Warning);return;}using var doc=JsonDocument.Parse(text);_voucherId=doc.RootElement.GetProperty("voucher_ID").GetInt64();_number.Text=doc.RootElement.GetProperty("voucher_No").GetString()??"";MessageBox.Show("تم حفظ سند الصرف كمسودة.","سند الصرف");
    }
    private async Task SearchAsync(){if(string.IsNullOrWhiteSpace(_number.Text)){_number.ReadOnly=false;_number.Focus();return;}var response=await ApiService.Client.GetAsync($"FinancialVoucher/ByNumber?voucherNumber={Uri.EscapeDataString(_number.Text)}&voucherTypeId={Convert.ToInt32(_type.SelectedValue)}");var text=await response.Content.ReadAsStringAsync();if(!response.IsSuccessStatusCode){MessageBox.Show(text);return;}using var d=JsonDocument.Parse(text);var data=d.RootElement.GetProperty("data");_voucherId=data.GetProperty("voucher_ID").GetInt64();_number.Text=data.GetProperty("voucher_No").GetString()??"";}
    private async Task ActionAsync(string action,bool reason){if(_voucherId<=0){MessageBox.Show("احفظ أو ابحث عن السند أولاً.");return;}if(reason&&string.IsNullOrWhiteSpace(_reason.Text)){MessageBox.Show("سبب فك الترحيل إلزامي.");return;}HttpResponseMessage response=reason?await ApiService.Client.PostAsJsonAsync($"FinancialVoucher/{_voucherId}/{action}",new {Reason=_reason.Text,Action_Channel="DESKTOP"}):await ApiService.Client.PostAsJsonAsync($"FinancialVoucher/{_voucherId}/{action}",new {Notes=_narration.Text,Action_Channel="DESKTOP"});MessageBox.Show(response.IsSuccessStatusCode?"تم تنفيذ العملية.":await response.Content.ReadAsStringAsync());}
    private void OpenAttachments(){if(_voucherId<=0){MessageBox.Show("احفظ سند الصرف أولاً.");return;}new FrmVoucherAttachments(_voucherId).ShowDialog(this);}
    private async Task ViewJournalAsync(){if(string.IsNullOrWhiteSpace(_number.Text))return;var r=await ApiService.Client.GetAsync($"FinancialVoucher/journal-entry/{Uri.EscapeDataString(_number.Text)}?voucherTypeId={Convert.ToInt32(_type.SelectedValue)}");MessageBox.Show(await r.Content.ReadAsStringAsync(),"القيد الناتج");}

    private sealed class PaymentVoucherDetailDto
    {
        public int Line_No { get; set; }
        public string? Account_ID { get; set; }
        public string? Cost_Center_ID { get; set; }
        public int Currency_ID { get; set; }
        public decimal Exchange_Rate { get; set; }
        public decimal Foreign_Amount { get; set; }
        public decimal Local_Amount { get; set; }
        public decimal Debit_Amount { get; set; }
        public decimal Credit_Amount { get; set; }
        public byte Line_Type { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
    }

    private sealed class Lookup{public List<TypeRow> VoucherTypes{get;set;}=new();public List<StatusRow> VoucherStatuses{get;set;}=new();public List<CurrencyRow>Currencies{get;set;}=new();public List<CashRow>CashBoxes{get;set;}=new();public List<PartyRow>Parties{get;set;}=new();public List<MethodRow>PaymentMethods{get;set;}=new();}
    private sealed class TypeRow{public int Voucher_Type_ID{get;set;}public string Voucher_Type_Code{get;set;}="";public string Voucher_Type_Name_AR{get;set;}="";public bool Is_Active{get;set;}=true;}
    private sealed class StatusRow{public int Voucher_Status_ID{get;set;}public string Voucher_Status_Code{get;set;}="";public string Voucher_Status_Name_AR{get;set;}="";public bool Is_Active{get;set;}=true;}
    private sealed class CurrencyRow{public int Currency_ID{get;set;}public string Currency_Name_AR{get;set;}="";public decimal Exchange_Rate{get;set;}=1;public bool Is_Local_Currency{get;set;}public bool Is_Active{get;set;}=true;}
    private sealed class CashRow{public string Account_ID{get;set;}="";public string Cash_Box_Name{get;set;}="";public string Cash_Box_Code{get;set;}="";public string Display=>$"{Cash_Box_Code} - {Cash_Box_Name}";}
    private sealed class PartyRow{public string Party_ID{get;set;}="";public string Party_Code{get;set;}="";public string Party_Name_AR{get;set;}="";public string Display=>$"{Party_Code} - {Party_Name_AR}";}
    private sealed class MethodRow{public int Payment_Method_ID{get;set;}public string Payment_Method_Name_AR{get;set;}="";}
}
