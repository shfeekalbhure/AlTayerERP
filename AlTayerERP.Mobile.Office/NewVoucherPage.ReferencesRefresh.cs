using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office;

public partial class NewVoucherPage
{
    private async void OnRefreshReferencesClicked(object? sender, EventArgs e)
    {
        RefreshReferencesButton.IsEnabled = false;
        ReferencesStatusLabel.Text = "جارٍ تحميل القوائم...";
        ReferencesStatusLabel.TextColor = Color.FromArgb("#0B6B87");

        try
        {
            _loaded = false;
            await LoadReferencesAsync();
            ApplyReferencesDiagnostics();
        }
        finally
        {
            RefreshReferencesButton.IsEnabled = true;
        }
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

        var counts = $"الصناديق/البنوك: {sources} | الحسابات: {accounts} | العملات: {currencies} | الأطراف: {parties} | طرق السداد: {methods} | مراكز التكلفة: {centers}";
        ReferencesStatusLabel.Text = _references.PermissionDiagnostics.Count == 0
            ? counts
            : string.Join(Environment.NewLine, _references.PermissionDiagnostics.Append(counts));

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

        var missingLists = new List<string>();
        if (accounts == 0) missingLists.Add("الحسابات المقابلة");
        if (currencies == 0) missingLists.Add("العملات");
        if (methods == 0) missingLists.Add("طرق السداد");

        if (missingLists.Count > 0)
        {
            ShowStatus($"القوائم التالية فارغة: {string.Join("، ", missingLists)}. أكمل تهيئتها ثم اضغط تحديث القوائم.");
        }
    }
}
