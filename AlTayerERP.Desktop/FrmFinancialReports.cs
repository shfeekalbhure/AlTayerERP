using AlTayerERP.Desktop.Common;
using AlTayerERP.Desktop.Services;
using System.Data;
using System.Text.Json;

namespace AlTayerERP.Desktop;

/// <summary>مركز تقارير المرحلة الأولى: ميزان مراجعة وأستاذ عام، عبر API فقط.</summary>
public sealed class FrmFinancialReports : BaseForm
{
    private readonly TabControl _tabs=new(){Dock=DockStyle.Fill};
    private readonly DataGridView _trialGrid=Grid(); private readonly DataGridView _ledgerGrid=Grid();
    private readonly DateTimePicker _trialDate=new(){Format=DateTimePickerFormat.Short};
    private readonly DateTimePicker _from=new(){Format=DateTimePickerFormat.Short};private readonly DateTimePicker _to=new(){Format=DateTimePickerFormat.Short};
    private readonly TextBox _account=new(){Width=190,PlaceholderText="معرف الحساب"};
    public FrmFinancialReports()
    {
        Text="التقارير المالية";ApplyBaseFormStyle();RightToLeft=RightToLeft.Yes;RightToLeftLayout=true;
        var trial=new TabPage("ميزان المراجعة");var trialBar=Bar();trialBar.Controls.AddRange(new Control[]{B("عرض الميزان",async(_,_)=>await TrialAsync()),_trialDate,new Label{Text="حتى تاريخ",AutoSize=true}});trial.Controls.Add(_trialGrid);trial.Controls.Add(trialBar);
        var ledger=new TabPage("الأستاذ العام");var ledgerBar=Bar();ledgerBar.Controls.AddRange(new Control[]{B("عرض الأستاذ",async(_,_)=>await LedgerAsync()),_account,_from,_to});ledger.Controls.Add(_ledgerGrid);ledger.Controls.Add(ledgerBar);
        _tabs.TabPages.Add(trial);_tabs.TabPages.Add(ledger);Controls.Add(_tabs);Load+=async(_,_)=>await TrialAsync();
    }
    private static DataGridView Grid()=>new(){Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
    private static FlowLayoutPanel Bar()=>new(){Dock=DockStyle.Top,Height=42,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(8,5,8,5)};
    private static Button B(string text,EventHandler action){var b=new Button{Text=text,AutoSize=true};b.Click+=action;return b;}
    private async Task TrialAsync(){await BindAsync(_trialGrid,$"accounting-reports/trial-balance?toDate={_trialDate.Value:yyyy-MM-dd}","rows");}
    private async Task LedgerAsync(){if(string.IsNullOrWhiteSpace(_account.Text)){MessageBox.Show("أدخل معرف الحساب.");return;}await BindAsync(_ledgerGrid,$"accounting-reports/general-ledger?accountId={Uri.EscapeDataString(_account.Text.Trim())}&fromDate={_from.Value:yyyy-MM-dd}&toDate={_to.Value:yyyy-MM-dd}","rows");}
    private static async Task BindAsync(DataGridView grid,string url,string property)
    {
        var response=await ApiService.Client.GetAsync(url);var raw=await response.Content.ReadAsStringAsync();if(!response.IsSuccessStatusCode){MessageBox.Show(raw,"التقارير",MessageBoxButtons.OK,MessageBoxIcon.Warning);return;}
        using var doc=JsonDocument.Parse(raw);var table=new DataTable();foreach(var item in doc.RootElement.GetProperty(property).EnumerateArray()){if(table.Columns.Count==0)foreach(var p in item.EnumerateObject())table.Columns.Add(p.Name);var row=table.NewRow();foreach(var p in item.EnumerateObject())row[p.Name]=p.Value.ValueKind==JsonValueKind.Null?DBNull.Value:p.Value.ToString();table.Rows.Add(row);}grid.DataSource=table;
    }
}