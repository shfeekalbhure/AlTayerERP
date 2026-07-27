using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office;

public partial class NewVoucherPage
{
    private async void OnRefreshReferencesClicked(object? sender, EventArgs e)
    {
        _loaded = false;
        await LoadReferencesAsync();
    }

    private void ApplyReferencesDiagnostics()
    {
        if (_references == null)
        {
            ReferencesStatusLabel.Text = "لم تصل بيانات القوائم من الخادم.";
            ReferencesStatusLabel.TextColor = Color.FromArgb("#B42318");
            return;
        }

        var sources = _references.Sources?.Count ?? 0;
        var accounts = _references.Accounts?.Count ?? 0;
        var centers = _references.CostCenters?.Count ?? 0;
        var currencies = _references.Currencies?.Count ?? 0;
        var parties = _references.Parties?.Count ?? 0;
        var methods = _references.PaymentMethods?.Count ?? 0;

        ReferencesStatusLabel.Text =
            $"الصناديق/البنوك: {sources} | الحسابات: {accounts} | العملات: {currencies} | الأطراف: {parties} | طرق السداد: {methods} | مراكز التكلفة: {centers}";

        var allEmpty = sources == 0 && accounts == 0 && currencies == 0 && parties == 0 && methods == 0 && centers == 0;
        ReferencesStatusLabel.TextColor = allEmpty
            ? Color.FromArgb("#B42318")
            : Color.FromArgb("#18794E");

        if (sources == 1 && SourcePicker.SelectedItem == null)
        {
            SourcePicker.SelectedItem = _references.Sources[0];
            SourceHelpLabel.Text = "تم اختيار الصندوق أو البنك الوحيد تلقائيًا.";
            SourceHelpLabel.TextColor = Color.FromArgb("#18794E");
        }

        if (sources == 0)
        {
            SourceHelpLabel.Text = _references.SourceMessage ?? "لا توجد صناديق أو بنوك متاحة للفرع الحالي.";
            SourceHelpLabel.TextColor = Color.FromArgb("#B42318");
        }
    }
}
