using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office;

public partial class NewVoucherPage
{
    private async void OnSelectPartyClicked(object? sender, EventArgs e)
    {
        if (_references == null) { ShowStatus("لم يتم تحميل الأطراف بعد."); return; }
        var selected = await SelectOptionAsync("اختيار الطرف", _references.Parties.Select(x => new VoucherOption(x.Id, x.DisplayName)), true);
        if (selected == null) return;
        if (string.IsNullOrWhiteSpace(selected.Id))
        {
            PartyPicker.SelectedItem = null;
            PartySearchButton.Text = "اختيار الطرف - اختياري 🔍";
            return;
        }
        var party = _references.Parties.FirstOrDefault(x => x.Id == selected.Id);
        PartyPicker.SelectedItem = party;
        PartySearchButton.Text = party?.DisplayName ?? selected.DisplayName;
    }

    private async void OnSelectPaymentMethodClicked(object? sender, EventArgs e)
    {
        if (_references == null) { ShowStatus("لم يتم تحميل طرق السداد بعد."); return; }
        var selected = await SelectOptionAsync("اختيار طريقة السداد", _references.PaymentMethods.Select(x => new VoucherOption(x.Id.ToString(), x.DisplayName)), true);
        if (selected == null) return;
        if (string.IsNullOrWhiteSpace(selected.Id))
        {
            PaymentMethodPicker.SelectedItem = null;
            PaymentMethodSearchButton.Text = "اختيار طريقة السداد 🔍";
            return;
        }
        var method = _references.PaymentMethods.FirstOrDefault(x => x.Id.ToString() == selected.Id);
        PaymentMethodPicker.SelectedItem = method;
        PaymentMethodSearchButton.Text = method?.DisplayName ?? selected.DisplayName;
    }

    private async void OnSelectAccountClicked(object? sender, EventArgs e)
    {
        if (_references == null) { ShowStatus("لم يتم تحميل الحسابات بعد."); return; }
        var selected = await SelectOptionAsync("اختيار الحساب المقابل", _references.Accounts.Select(x => new VoucherOption(x.Id, x.DisplayName)));
        if (selected == null) return;
        var account = _references.Accounts.FirstOrDefault(x => x.Id == selected.Id);
        AccountPicker.SelectedItem = account;
        AccountSearchButton.Text = account?.DisplayName ?? selected.DisplayName;
    }

    private async void OnSelectCostCenterClicked(object? sender, EventArgs e)
    {
        if (_references == null) { ShowStatus("لم يتم تحميل مراكز التكلفة بعد."); return; }
        var selected = await SelectOptionAsync("اختيار مركز التكلفة", _references.CostCenters.Select(x => new VoucherOption(x.Id, x.DisplayName)), true);
        if (selected == null) return;
        if (string.IsNullOrWhiteSpace(selected.Id))
        {
            CostCenterPicker.SelectedItem = null;
            CostCenterSearchButton.Text = "اختيار مركز التكلفة - اختياري 🔍";
            return;
        }
        var center = _references.CostCenters.FirstOrDefault(x => x.Id == selected.Id);
        CostCenterPicker.SelectedItem = center;
        CostCenterSearchButton.Text = center?.DisplayName ?? selected.DisplayName;
    }

    private async void OnSelectCurrencyClicked(object? sender, EventArgs e)
    {
        if (_references == null) { ShowStatus("لم يتم تحميل العملات بعد."); return; }
        var selected = await SelectOptionAsync("اختيار العملة", _references.Currencies.Select(x => new VoucherOption(x.Id.ToString(), x.DisplayName)));
        if (selected == null) return;
        var currency = _references.Currencies.FirstOrDefault(x => x.Id.ToString() == selected.Id);
        CurrencyPicker.SelectedItem = currency;
        CurrencySearchButton.Text = currency?.DisplayName ?? selected.DisplayName;
    }

    private async Task<VoucherOption?> SelectOptionAsync(string title, IEnumerable<VoucherOption> options, bool allowClear = false)
    {
        HideStatus();
        var list = options.ToList();
        if (list.Count == 0)
        {
            ShowStatus($"لا توجد بيانات متاحة في {title}.");
            return null;
        }

        var page = new SearchableVoucherOptionPage(title, list, allowClear);
        await Navigation.PushModalAsync(new NavigationPage(page)
        {
            FlowDirection = FlowDirection.RightToLeft,
            BarBackgroundColor = Color.FromArgb("#17324D"),
            BarTextColor = Colors.White
        });
        return await page.WaitForSelectionAsync();
    }

    private void RefreshLookupButtonTexts()
    {
        if (PartyPicker.SelectedItem is VoucherEntryPartyDto party)
            PartySearchButton.Text = party.DisplayName;
        else
            PartySearchButton.Text = "اختيار الطرف - اختياري 🔍";

        if (PaymentMethodPicker.SelectedItem is VoucherEntryPaymentMethodDto method)
            PaymentMethodSearchButton.Text = method.DisplayName;
        else
            PaymentMethodSearchButton.Text = "اختيار طريقة السداد 🔍";

        if (AccountPicker.SelectedItem is VoucherEntryLookupDto account)
            AccountSearchButton.Text = account.DisplayName;
        else
            AccountSearchButton.Text = "اختيار الحساب المقابل 🔍";

        if (CostCenterPicker.SelectedItem is VoucherEntryLookupDto center)
            CostCenterSearchButton.Text = center.DisplayName;
        else
            CostCenterSearchButton.Text = "اختيار مركز التكلفة - اختياري 🔍";

        if (CurrencyPicker.SelectedItem is VoucherEntryCurrencyDto currency)
            CurrencySearchButton.Text = currency.DisplayName;
        else
            CurrencySearchButton.Text = "اختيار العملة 🔍";
    }
}
