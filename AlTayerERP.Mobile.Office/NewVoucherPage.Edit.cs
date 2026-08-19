using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class NewVoucherPage
{
    private long _editVoucherId;
    private ReceiptVoucherService? _receiptVoucherService;
    private PaymentVoucherService? _paymentVoucherService;
    private bool _editDataLoaded;
    private ReceiptVoucherHeaderDto? _editHeader;
    private PaymentVoucherHeaderDto? _paymentEditHeader;

    public NewVoucherPage(
        VoucherEntryService service,
        ReceiptVoucherService receiptVoucherService,
        long voucherId)
        : this(service, "RECEIPT")
    {
        if (voucherId <= 0)
            throw new ArgumentOutOfRangeException(nameof(voucherId));

        _editVoucherId = voucherId;
        _receiptVoucherService = receiptVoucherService;
        Title = "تعديل سند القبض";
        SaveButton.Text = "حفظ التعديلات";
        SaveButton.Clicked -= OnSaveClicked;
        SaveButton.Clicked += OnEditSaveClicked;

        Appearing += async (_, _) => await EnsureEditDataLoadedAsync();
    }

    public NewVoucherPage(
        VoucherEntryService service,
        PaymentVoucherService paymentVoucherService,
        long voucherId)
        : this(service, "PAYMENT")
    {
        if (voucherId <= 0) throw new ArgumentOutOfRangeException(nameof(voucherId));
        _editVoucherId = voucherId;
        _paymentVoucherService = paymentVoucherService;
        Title = "تعديل سند الصرف";
        SaveButton.Text = "حفظ التعديلات";
        SaveButton.Clicked -= OnSaveClicked;
        SaveButton.Clicked += OnEditSaveClicked;
        Appearing += async (_, _) => await EnsureEditDataLoadedAsync();
    }

    private async Task EnsureEditDataLoadedAsync()
    {
        if (_editVoucherId <= 0 || _editDataLoaded)
            return;

        if (_paymentVoucherService != null)
        {
            await EnsurePaymentEditDataLoadedAsync();
            return;
        }
        if (_receiptVoucherService == null) return;

        while (!_loaded)
            await Task.Delay(100);

        SetBusy(true);
        try
        {
            var data = await _receiptVoucherService.GetByIdAsync(_editVoucherId);
            var header = data.Header;

            if (header.IsPosted)
                throw new InvalidOperationException("لا يمكن تعديل سند قبض مرحّل. فك الترحيل أولاً.");
            if (header.ApprovalStatus == 2)
                throw new InvalidOperationException("لا يمكن تعديل سند قبض معتمد. ألغِ الاعتماد أولاً.");
            if (header.ReviewStatus == 2)
                throw new InvalidOperationException("لا يمكن تعديل سند تمت مراجعته. أعده للتصحيح أولاً.");

            _editHeader = header;
            VoucherDatePicker.Date = header.VoucherDate.Date;
            ReferenceEntry.Text = header.ReferenceNo;
            DescriptionEditor.Text = header.Description;
            PartyNameEntry.Text = header.ReceivedFromName;

            var source = _references!.Sources.FirstOrDefault(x =>
                string.Equals(x.AccountId, header.CashAccountId, StringComparison.Ordinal));
            if (source == null)
            {
                source = new VoucherEntrySourceDto
                {
                    AccountId = header.CashAccountId,
                    SourceType = "CASH",
                    DisplayName = header.CashAccountDisplay
                };
                _references.Sources.Add(source);
                SourcePicker.ItemsSource = null;
                SourcePicker.ItemsSource = _references.Sources;
            }
            SourcePicker.SelectedItem = source;
            SourceSearchButton.Text = $"{source.DisplayName}  ✓";
            SourceHelpLabel.Text = "تم تحميل الصندوق أو البنك المحفوظ. اضغط للتغيير.";
            SourceHelpLabel.TextColor = Color.FromArgb("#18794E");

            PartyPicker.SelectedItem = _references.Parties.FirstOrDefault(x => x.Id == header.PartyId);
            PaymentMethodPicker.SelectedItem = _references.PaymentMethods.FirstOrDefault(x => x.Id == header.PaymentMethodId);
            RefreshLookupButtonTexts();

            _lines.Clear();
            foreach (var line in data.Details.Where(x => x.LineType != 1).OrderBy(x => x.LineNo))
            {
                _lines.Add(new VoucherDraftLine
                {
                    AccountId = line.AccountId,
                    AccountDisplay = line.AccountDisplay,
                    CostCenterId = line.CostCenterId,
                    CostCenterDisplay = line.CostCenterDisplay ?? "بدون مركز تكلفة",
                    CurrencyId = line.CurrencyId,
                    CurrencyDisplay = line.CurrencyDisplay,
                    ExchangeRate = line.ExchangeRate,
                    ForeignAmount = line.ForeignAmount,
                    LocalAmount = line.LocalAmount,
                    Description = line.Description
                });
            }

            UpdateTotal();
            _editDataLoaded = true;
        }
        catch (Exception ex)
        {
            ShowStatus(ex.Message);
            SaveButton.IsEnabled = false;
            AddLineButton.IsEnabled = false;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task EnsurePaymentEditDataLoadedAsync()
    {
        while (!_loaded) await Task.Delay(100);
        SetBusy(true);
        try
        {
            var data = await _paymentVoucherService!.GetByIdAsync(_editVoucherId);
            var header = data.Header;
            if (header.IsPosted) throw new InvalidOperationException("لا يمكن تعديل سند صرف مرحّل. فك الترحيل أولاً.");
            if (header.ApprovalStatus == 2) throw new InvalidOperationException("لا يمكن تعديل سند صرف معتمد. ألغِ الاعتماد أولاً.");
            if (header.ReviewStatus == 2) throw new InvalidOperationException("لا يمكن تعديل سند تمت مراجعته. أعده للتصحيح أولاً.");

            _paymentEditHeader = header;
            VoucherDatePicker.Date = header.VoucherDate.Date;
            ReferenceEntry.Text = header.ReferenceNo;
            DescriptionEditor.Text = header.Description;
            PartyNameEntry.Text = header.BeneficiaryName;
            var source = _references!.Sources.FirstOrDefault(x => string.Equals(x.AccountId, header.CashAccountId, StringComparison.Ordinal));
            if (source == null)
            {
                source = new VoucherEntrySourceDto { AccountId = header.CashAccountId, SourceType = "CASH", DisplayName = header.CashAccountDisplay };
                _references.Sources.Add(source);
                SourcePicker.ItemsSource = null;
                SourcePicker.ItemsSource = _references.Sources;
            }
            SourcePicker.SelectedItem = source;
            SourceSearchButton.Text = $"{source.DisplayName}  ✓";
            SourceHelpLabel.Text = "تم تحميل الصندوق أو البنك المحفوظ. اضغط للتغيير.";
            SourceHelpLabel.TextColor = Color.FromArgb("#18794E");
            PartyPicker.SelectedItem = _references.Parties.FirstOrDefault(x => x.Id == header.PartyId);
            PaymentMethodPicker.SelectedItem = _references.PaymentMethods.FirstOrDefault(x => x.Id == header.PaymentMethodId);
            RefreshLookupButtonTexts();
            _lines.Clear();
            foreach (var line in data.Details.Where(x => x.LineType != 1).OrderBy(x => x.LineNo))
                _lines.Add(new VoucherDraftLine { AccountId = line.AccountId, AccountDisplay = line.AccountDisplay, CostCenterId = line.CostCenterId, CostCenterDisplay = line.CostCenterDisplay ?? "بدون مركز تكلفة", CurrencyId = line.CurrencyId, CurrencyDisplay = line.CurrencyDisplay, ExchangeRate = line.ExchangeRate, ForeignAmount = line.ForeignAmount, LocalAmount = line.LocalAmount, Description = line.Description });
            UpdateTotal();
            _editDataLoaded = true;
        }
        catch (Exception ex)
        {
            ShowStatus(ex.Message);
            SaveButton.IsEnabled = AddLineButton.IsEnabled = false;
        }
        finally { SetBusy(false); }
    }

    private async void OnEditSaveClicked(object? sender, EventArgs e)
    {
        HideStatus();
        if (_references == null || (_editHeader == null && _paymentEditHeader == null))
        {
            ShowStatus(_type == "RECEIPT" ? "لم يتم تحميل بيانات سند القبض للتعديل." : "لم يتم تحميل بيانات سند الصرف للتعديل.");
            return;
        }

        if (SourcePicker.SelectedItem is not VoucherEntrySourceDto source)
        {
            ShowStatus("اختر الصندوق أو البنك.");
            return;
        }
        if (string.IsNullOrWhiteSpace(PartyNameEntry.Text))
        {
            ShowStatus(_type == "RECEIPT" ? "اسم المستلم منه مطلوب." : "اسم المستفيد مطلوب.");
            return;
        }
        if (PaymentMethodPicker.SelectedItem is not VoucherEntryPaymentMethodDto method)
        {
            ShowStatus("اختر طريقة السداد.");
            return;
        }
        var accountingText = Clean(DescriptionEditor.Text);
        if (accountingText == null)
        {
            ShowStatus("البيان المحاسبي مطلوب.");
            return;
        }
        if (_lines.Count == 0)
        {
            ShowStatus("أضف سطراً محاسبياً واحداً على الأقل.");
            return;
        }
        if (_lines.Any(x => string.Equals(x.AccountId, source.AccountId, StringComparison.Ordinal)))
        {
            ShowStatus("لا يمكن استخدام حساب الصندوق أو البنك نفسه كحساب مقابل.");
            return;
        }

        var voucherDate = VoucherDatePicker.Date ?? DateTime.Today;
        if (!_references.OpenPeriods.Any(x => voucherDate.Date >= x.StartDate.Date && voucherDate.Date <= x.EndDate.Date))
        {
            ShowStatus("تاريخ السند لا يقع داخل فترة مالية مفتوحة.");
            return;
        }

        var localCurrency = _references.Currencies.FirstOrDefault(x => x.IsLocal);
        if (localCurrency == null)
        {
            ShowStatus("العملة المحلية غير مهيأة.");
            return;
        }

        var total = _lines.Sum(x => x.LocalAmount);
        var details = new List<UpdateMobileVoucherLineDto>
        {
            new()
            {
                Voucher_Detail_ID = 0,
                Line_No = 1,
                Account_ID = source.AccountId,
                Currency_ID = localCurrency.Id,
                Exchange_Rate = 1m,
                Foreign_Amount = 0m,
                Local_Amount = total,
                Debit_Amount = _type == "RECEIPT" ? total : 0m,
                Credit_Amount = _type == "PAYMENT" ? total : 0m,
                Description = accountingText,
                Line_Type = 1
            }
        };

        details.AddRange(_lines.Select((x, index) => new UpdateMobileVoucherLineDto
        {
            Voucher_Detail_ID = 0,
            Line_No = index + 2,
            Account_ID = x.AccountId,
            Cost_Center_ID = x.CostCenterId,
            Currency_ID = x.CurrencyId,
            Exchange_Rate = x.ExchangeRate,
            Foreign_Amount = x.ForeignAmount,
            Local_Amount = x.LocalAmount,
            Debit_Amount = _type == "PAYMENT" ? x.LocalAmount : 0m,
            Credit_Amount = _type == "RECEIPT" ? x.LocalAmount : 0m,
            Description = x.Description,
            Line_Type = 2
        }));

        if (details.Sum(x => x.Debit_Amount) != details.Sum(x => x.Credit_Amount))
        {
            ShowStatus(_type == "RECEIPT" ? "سند القبض غير متوازن محاسبياً." : "سند الصرف غير متوازن محاسبياً.");
            return;
        }

        var party = PartyPicker.SelectedItem as VoucherEntryPartyDto;
        var dto = new UpdateMobileVoucherDto
        {
            Voucher_ID = _editVoucherId,
            Voucher_Type_ID = _editHeader?.VoucherTypeId ?? _paymentEditHeader!.VoucherTypeId,
            Voucher_Status_ID = _editHeader?.VoucherStatusId ?? _paymentEditHeader!.VoucherStatusId,
            Voucher_Date = voucherDate.Date,
            Transaction_Date = voucherDate.Date,
            Cash_Account_ID = source.AccountId,
            Party_ID = party?.Id,
            Received_From_Name = PartyNameEntry.Text.Trim(),
            Payment_Method_ID = method.Id,
            Currency_ID = localCurrency.Id,
            Exchange_Rate = 1m,
            Amount = total,
            Foreign_Total = 0m,
            Local_Total = total,
            Reference_No = Clean(ReferenceEntry.Text),
            Against_Text = accountingText,
            Description = accountingText,
            Requires_Approval = true,
            Details = details
        };

        SetBusy(true);
        try
        {
            await _service.UpdateAsync(dto);
            await DisplayAlertAsync("تم التعديل", _type == "RECEIPT" ? "تم حفظ تعديلات سند القبض وإعادته إلى بداية دورة المراجعة." : "تم حفظ تعديلات سند الصرف وإعادته إلى بداية دورة المراجعة.", "موافق");
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
}
