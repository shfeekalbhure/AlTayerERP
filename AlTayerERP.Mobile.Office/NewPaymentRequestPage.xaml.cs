using System.Collections.ObjectModel;
using System.Globalization;
using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlTayerERP.Mobile.Office;

public partial class NewPaymentRequestPage : ContentPage
{
    private readonly PaymentRequestService _service;
    private readonly PaymentRequestReferenceService _referenceService;
    private readonly SessionStorageService _sessionStorage;
    private readonly ObservableCollection<PaymentRequestDraftLine> _lines = [];
    private readonly PaymentRequestListItemDto? _editingRequest;
    private PaymentRequestReferencesDto? _references;
    private bool _referencesLoaded;
    private bool _hasOpenPeriod;

    public NewPaymentRequestPage(
        PaymentRequestService service,
        PaymentRequestListItemDto? editingRequest = null)
    {
        InitializeComponent();
        _service = service;
        _editingRequest = editingRequest;
        _referenceService = IPlatformApplication.Current.Services
            .GetRequiredService<PaymentRequestReferenceService>();
        _sessionStorage = IPlatformApplication.Current.Services
            .GetRequiredService<SessionStorageService>();

        RequestDatePicker.Date = editingRequest?.Request_Date ?? DateTime.Today;
        LinesList.ItemsSource = _lines;

        if (editingRequest != null)
        {
            Title = $"تعديل {editingRequest.Request_No}";
            SaveDraftButton.Text = "حفظ التعديلات";
            SaveAndSubmitButton.Text = "حفظ وإرسال";
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSessionContextAsync();
        if (!_referencesLoaded)
            await LoadReferencesAsync();
    }

    private async Task LoadSessionContextAsync()
    {
        var context = await _sessionStorage.GetPaymentRequestContextAsync();
        CompanyContextLabel.Text = context?.Company ?? "الشركة: غير محددة";
        BranchContextLabel.Text = context?.Branch ?? "الفرع: غير محدد";
        FiscalYearContextLabel.Text = context?.FiscalYear ?? "السنة المالية: غير محددة";
    }

    private async Task LoadReferencesAsync()
    {
        SetBusy(true);
        HideStatus();
        try
        {
            var result = await _referenceService.GetAsync();
            if (!result.IsSuccess || result.References == null)
            {
                _hasOpenPeriod = false;
                OpenPeriodLabel.Text = "تعذر التحقق من الفترة المالية المفتوحة.";
                OpenPeriodLabel.TextColor = Color.FromArgb("#B42318");
                ShowStatus(result.UserMessage);
                return;
            }

            _references = result.References;
            AccountPicker.ItemsSource = _references.Accounts;
            CostCenterPicker.ItemsSource = _references.CostCenters;
            CurrencyPicker.ItemsSource = _references.Currencies;
            PaymentMethodPicker.ItemsSource = _references.PaymentMethods;

            CurrencyPicker.SelectedItem = _references.Currencies.FirstOrDefault(x => x.IsDefault)
                                          ?? _references.Currencies.FirstOrDefault(x => x.IsLocal)
                                          ?? _references.Currencies.FirstOrDefault();

            if (_references.PaymentMethods.Count == 1)
                PaymentMethodPicker.SelectedItem = _references.PaymentMethods[0];

            ApplyOpenPeriods();
            if (_references.Accounts.Count == 0)
                ShowStatus("لا توجد حسابات نشطة قابلة للترحيل متاحة لطلب الصرف.");

            if (_editingRequest != null)
                PopulateForEdit();

            _referencesLoaded = true;
        }
        catch (Exception)
        {
            _hasOpenPeriod = false;
            OpenPeriodLabel.Text = "تعذر التحقق من الفترة المالية المفتوحة.";
            OpenPeriodLabel.TextColor = Color.FromArgb("#B42318");
            ShowStatus("تعذر تحميل بيانات طلب الصرف.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ApplyOpenPeriods()
    {
        var periods = _references?.OpenPeriods.OrderBy(x => x.StartDate).ToList() ?? [];
        _hasOpenPeriod = periods.Count > 0;
        RequestDatePicker.IsEnabled = _hasOpenPeriod;

        if (!_hasOpenPeriod)
        {
            OpenPeriodLabel.Text = "لا توجد فترة مالية مفتوحة للفرع والسنة المالية المحددين.";
            OpenPeriodLabel.TextColor = Color.FromArgb("#B42318");
            return;
        }

        RequestDatePicker.MinimumDate = periods.Min(x => x.StartDate.Date);
        RequestDatePicker.MaximumDate = periods.Max(x => x.EndDate.Date);

        var requestedDate = (_editingRequest?.Request_Date ?? DateTime.Today).Date;
        var matchingPeriod = periods.FirstOrDefault(x => x.Contains(requestedDate));
        var selectedPeriod = matchingPeriod
                             ?? periods.FirstOrDefault(x => x.Contains(DateTime.Today))
                             ?? periods[0];

        RequestDatePicker.Date = matchingPeriod != null
            ? requestedDate
            : selectedPeriod.Contains(DateTime.Today)
                ? DateTime.Today.Date
                : selectedPeriod.StartDate.Date;

        OpenPeriodLabel.Text = periods.Count == 1
            ? $"الفترة المفتوحة: {periods[0].DisplayName}"
            : $"الفترات المفتوحة: {string.Join(" | ", periods.Select(x => x.DisplayName))}";
        OpenPeriodLabel.TextColor = Color.FromArgb("#198754");
    }

    private bool IsDateInOpenPeriod(DateTime date) =>
        _references?.OpenPeriods.Any(x => x.Contains(date)) == true;

    private void PopulateForEdit()
    {
        if (_editingRequest == null || _references == null)
            return;

        BeneficiaryEntry.Text = _editingRequest.Beneficiary_Name;
        PartyIdEntry.Text = _editingRequest.Party_ID;
        ReferenceEntry.Text = _editingRequest.Header_Reference_No;
        DescriptionEditor.Text = _editingRequest.Description;
        PaymentMethodPicker.SelectedItem = _references.PaymentMethods
            .FirstOrDefault(x => x.Id == _editingRequest.Payment_Method_ID);

        _lines.Clear();
        foreach (var line in _editingRequest.Details.OrderBy(x => x.Line_No))
        {
            var account = _references.Accounts.FirstOrDefault(x => x.Id == line.Account_ID);
            var costCenter = _references.CostCenters.FirstOrDefault(x => x.Id == line.Cost_Center_ID);
            var currency = _references.Currencies.FirstOrDefault(x => x.Id == line.Currency_ID);

            _lines.Add(new PaymentRequestDraftLine
            {
                AccountId = line.Account_ID,
                AccountDisplay = account?.DisplayName ?? line.Account_ID,
                CostCenterId = line.Cost_Center_ID,
                CostCenterDisplay = costCenter?.DisplayName ?? "بدون مركز تكلفة",
                CurrencyId = line.Currency_ID,
                CurrencyDisplay = currency?.DisplayName ?? line.Currency_ID.ToString(),
                ExchangeRate = line.Exchange_Rate,
                ForeignAmount = line.Foreign_Amount,
                LocalAmount = line.Local_Amount,
                Description = line.Description
            });
        }

        UpdateTotal();
    }

    private async void OnSaveDraftClicked(object? sender, EventArgs e) => await SaveAsync(false);
    private async void OnSaveAndSubmitClicked(object? sender, EventArgs e) => await SaveAsync(true);

    private void OnCurrencyChanged(object? sender, EventArgs e)
    {
        if (CurrencyPicker.SelectedItem is not PaymentRequestCurrencyDto currency)
            return;

        ExchangeRateEntry.Text = currency.IsLocal
            ? "1"
            : currency.ExchangeRate.ToString(CultureInfo.InvariantCulture);
        ExchangeRateEntry.IsReadOnly = currency.IsLocal;
        ForeignAmountEntry.IsEnabled = !currency.IsLocal;
        LocalAmountEntry.IsReadOnly = !currency.IsLocal;

        if (currency.IsLocal)
            ForeignAmountEntry.Text = "0";
        else
            RecalculateLocalAmount();
    }

    private void OnForeignAmountChanged(object? sender, TextChangedEventArgs e) => RecalculateLocalAmount();

    private void RecalculateLocalAmount()
    {
        if (CurrencyPicker.SelectedItem is not PaymentRequestCurrencyDto currency || currency.IsLocal)
            return;

        if (TryDecimal(ForeignAmountEntry.Text, out var foreignAmount) &&
            TryDecimal(ExchangeRateEntry.Text, out var exchangeRate) &&
            foreignAmount >= 0 && exchangeRate > 0)
        {
            LocalAmountEntry.Text = decimal.Round(foreignAmount * exchangeRate, 2)
                .ToString(CultureInfo.InvariantCulture);
        }
    }

    private void OnAddLineClicked(object? sender, EventArgs e)
    {
        HideStatus();
        if (!TryBuildCurrentLine(out var line, out var message))
        {
            ShowStatus(message);
            return;
        }

        _lines.Add(line!);
        ClearLineEditor();
        UpdateTotal();
    }

    private void OnRemoveLineClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is PaymentRequestDraftLine line)
        {
            _lines.Remove(line);
            UpdateTotal();
        }
    }

    private bool TryBuildCurrentLine(out PaymentRequestDraftLine? line, out string message)
    {
        line = null;
        message = string.Empty;

        if (AccountPicker.SelectedItem is not PaymentRequestReferenceItemDto account)
        {
            message = "اختر الحساب المدين.";
            return false;
        }

        if (CurrencyPicker.SelectedItem is not PaymentRequestCurrencyDto currency)
        {
            message = "اختر العملة.";
            return false;
        }

        if (!TryDecimal(ExchangeRateEntry.Text, out var exchangeRate) || exchangeRate <= 0)
        {
            message = "سعر الصرف غير صحيح.";
            return false;
        }

        if (!TryDecimal(ForeignAmountEntry.Text, out var foreignAmount) || foreignAmount < 0)
        {
            message = "المبلغ الأجنبي غير صحيح.";
            return false;
        }

        if (!TryDecimal(LocalAmountEntry.Text, out var localAmount) || localAmount <= 0)
        {
            message = "المبلغ المحلي يجب أن يكون أكبر من صفر.";
            return false;
        }

        if (currency.IsLocal && (foreignAmount != 0 || exchangeRate != 1))
        {
            message = "في العملة المحلية يجب أن يكون المبلغ الأجنبي صفراً وسعر الصرف 1.";
            return false;
        }

        var costCenter = CostCenterPicker.SelectedItem as PaymentRequestReferenceItemDto;
        line = new PaymentRequestDraftLine
        {
            AccountId = account.Id,
            AccountDisplay = account.DisplayName,
            CostCenterId = costCenter?.Id,
            CostCenterDisplay = costCenter?.DisplayName ?? "بدون مركز تكلفة",
            CurrencyId = currency.Id,
            CurrencyDisplay = currency.DisplayName,
            ExchangeRate = exchangeRate,
            ForeignAmount = foreignAmount,
            LocalAmount = localAmount,
            Description = Clean(LineDescriptionEditor.Text)
        };
        return true;
    }

    private async Task SaveAsync(bool submitAfterSave)
    {
        HideStatus();

        if (!_hasOpenPeriod)
        {
            ShowStatus("لا يمكن الحفظ قبل فتح فترة مالية للفرع والسنة الحالية.");
            return;
        }

        var requestDate = RequestDatePicker.Date ?? DateTime.Today;
        if (!IsDateInOpenPeriod(requestDate))
        {
            ShowStatus("تاريخ الطلب لا يقع ضمن أي فترة مالية مفتوحة.");
            return;
        }

        if (string.IsNullOrWhiteSpace(BeneficiaryEntry.Text))
        {
            ShowStatus("اسم المستفيد مطلوب.");
            return;
        }

        if (PaymentMethodPicker.SelectedItem is not PaymentRequestMethodDto paymentMethod)
        {
            ShowStatus("اختر طريقة السداد.");
            return;
        }

        if (_lines.Count == 0)
        {
            ShowStatus("أضف سطر صرف واحداً على الأقل.");
            return;
        }

        var dto = new CreatePaymentRequestDto
        {
            Request_Date = requestDate,
            Beneficiary_Name = BeneficiaryEntry.Text.Trim(),
            Party_ID = Clean(PartyIdEntry.Text),
            Payment_Method_ID = paymentMethod.Id,
            Header_Reference_No = Clean(ReferenceEntry.Text),
            Description = Clean(DescriptionEditor.Text),
            Lines = _lines.Select(x => new CreatePaymentRequestLineDto
            {
                Account_ID = x.AccountId,
                Cost_Center_ID = x.CostCenterId,
                Currency_ID = x.CurrencyId,
                Exchange_Rate = x.ExchangeRate,
                Foreign_Amount = x.ForeignAmount,
                Local_Amount = x.LocalAmount,
                Description = x.Description
            }).ToList()
        };

        SetBusy(true);
        try
        {
            var saved = _editingRequest == null
                ? await _service.CreateAsync(dto)
                : await _service.UpdateAsync(_editingRequest.Payment_Request_ID, dto);

            if (submitAfterSave)
                await _service.SubmitAsync(saved.Payment_Request_ID);

            await DisplayAlert(
                "تمت العملية",
                submitAfterSave
                    ? $"تم حفظ طلب الصرف {saved.Request_No} وإرساله للمراجعة."
                    : $"تم حفظ طلب الصرف {saved.Request_No}.",
                "موافق");

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            ShowStatus(ex.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ClearLineEditor()
    {
        AccountPicker.SelectedItem = null;
        CostCenterPicker.SelectedItem = null;
        CurrencyPicker.SelectedItem = _references?.Currencies.FirstOrDefault(x => x.IsDefault)
                                      ?? _references?.Currencies.FirstOrDefault(x => x.IsLocal)
                                      ?? _references?.Currencies.FirstOrDefault();
        ForeignAmountEntry.Text = "0";
        LocalAmountEntry.Text = string.Empty;
        LineDescriptionEditor.Text = string.Empty;
    }

    private void UpdateTotal() =>
        TotalLabel.Text = $"الإجمالي: {_lines.Sum(x => x.LocalAmount):N2}";

    private static bool TryDecimal(string? value, out decimal result) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result) ||
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result);

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void SetBusy(bool busy)
    {
        BusyIndicator.IsVisible = busy;
        BusyIndicator.IsRunning = busy;
        SaveDraftButton.IsEnabled = !busy && _hasOpenPeriod;
        SaveAndSubmitButton.IsEnabled = !busy && _hasOpenPeriod;
        AddLineButton.IsEnabled = !busy && _hasOpenPeriod;
        AccountPicker.IsEnabled = !busy;
        CostCenterPicker.IsEnabled = !busy;
        CurrencyPicker.IsEnabled = !busy;
        PaymentMethodPicker.IsEnabled = !busy;
        RequestDatePicker.IsEnabled = !busy && _hasOpenPeriod;
    }

    private void ShowStatus(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.IsVisible = true;
    }

    private void HideStatus()
    {
        StatusLabel.Text = string.Empty;
        StatusLabel.IsVisible = false;
    }

    private sealed class PaymentRequestDraftLine
    {
        public string AccountId { get; init; } = string.Empty;
        public string AccountDisplay { get; init; } = string.Empty;
        public string? CostCenterId { get; init; }
        public string CostCenterDisplay { get; init; } = string.Empty;
        public int CurrencyId { get; init; }
        public string CurrencyDisplay { get; init; } = string.Empty;
        public decimal ExchangeRate { get; init; }
        public decimal ForeignAmount { get; init; }
        public decimal LocalAmount { get; init; }
        public string? Description { get; init; }
    }
}
