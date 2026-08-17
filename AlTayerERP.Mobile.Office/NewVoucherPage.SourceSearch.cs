using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office;

public partial class NewVoucherPage
{
    private async void OnSelectSourceClicked(object? sender, EventArgs e)
    {
        HideStatus();

        if (_references == null)
        {
            ShowStatus("لم يتم تحميل بيانات الصناديق والبنوك بعد.");
            return;
        }

        if (_references.Sources.Count == 0)
        {
            SourceHelpLabel.Text = "لا توجد صناديق أو بنوك فعالة مرتبطة بحساب مالي لهذا الفرع.";
            SourceHelpLabel.TextColor = Color.FromArgb("#B42318");
            ShowStatus("أنشئ صندوقاً أو حساباً بنكياً فعالاً واربطه بحساب مالي ثم أعد فتح الشاشة.");
            return;
        }

        var page = new SearchableVoucherSourcePage(_references.Sources);
        await Navigation.PushModalAsync(new NavigationPage(page)
        {
            FlowDirection = FlowDirection.RightToLeft,
            BarBackgroundColor = Color.FromArgb("#17324D"),
            BarTextColor = Colors.White
        });

        VoucherEntrySourceDto? selected = await page.WaitForSelectionAsync();
        if (selected == null)
            return;

        SourcePicker.SelectedItem = _references.Sources.FirstOrDefault(x =>
            string.Equals(x.AccountId, selected.AccountId, StringComparison.Ordinal) &&
            string.Equals(x.SourceType, selected.SourceType, StringComparison.OrdinalIgnoreCase));

        SourceSearchButton.Text = $"{selected.DisplayName}  ✓";
        SourceHelpLabel.Text = selected.SourceType.Equals("BANK", StringComparison.OrdinalIgnoreCase)
            ? "تم اختيار حساب بنكي. اضغط للتغيير أو البحث من جديد."
            : "تم اختيار صندوق. اضغط للتغيير أو البحث من جديد.";
        SourceHelpLabel.TextColor = Color.FromArgb("#18794E");
    }
}
