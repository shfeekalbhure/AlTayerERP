using AlTayerERP.Mobile.Office.DTOs;
using AlTayerERP.Mobile.Office.Services;

namespace AlTayerERP.Mobile.Office;

public partial class NewVoucherPage
{
    private long _editVoucherId;
    private ReceiptVoucherService? _receiptVoucherService;
    private bool _editDataLoaded;
    private ReceiptVoucherHeaderDto? _editHeader;

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

    private async Task EnsureEditDataLoadedAsync()
    {
        if (_editVoucherId <= 0 || _editDataLoaded || _receiptVoucherService == null)
            return;

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

    private async void OnEditSaveClicked(object? sender, EventArgs e)
    {
        HideStatus();
        if (_references == null || _editHeader == null)
        {
            ShowStatus("لم يتم تحميل بيانات سند القبض للتعديل.");
            return;
        }

        if (SourcePicker.SelectedItem is not VoucherEntrySourceDto source)
        {
            ShowStatus("اختر الصندوق أو البنك.");
            return;
        }
        if (string.IsNullOrWhiteSpace(PartyNameEntry.Text))
        {
            ShowStatus("اسم المستلم منه مطلوب.");
            return;
        }
        if (_lines.Count == 0)
        {
            ShowStatus("أضف سطراً محاسبياً واحداً على الأقل.");
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
                Debit_Amount = total,
                Credit_Amount = 0m,
                Description = Clean(DescriptionEditor.Text),
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
            Debit_Amount = 0m,
            Credit_Amount = x.LocalAmount,
            Description = x.Description,
            Line_Type = 2
        }));

        if (details.Sum(x => x.Debit_Amount) != details.Sum(x => x.Credit_Amount))
        {
            ShowStatus("سند القبض غير متوازن محاسبياً.");
            return;
        }

        var party = PartyPicker.SelectedItem as VoucherEntryPartyDto;
        var method = PaymentMethodPicker.SelectedItem as VoucherEntryPaymentMethodDto;
        var dto = new UpdateMobileVoucherDto
        {
            Voucher_ID = _editVoucherId,
            Voucher_Type_ID = _editHeader.VoucherTypeId,
            Voucher_Status_ID = _editHeader.VoucherStatusId,
            Voucher_Date = voucherDate.Date,
            Transaction_Date = voucherDate.Date,
            Cash_Account_ID = source.AccountId,
            Party_ID = party?.Id,
            Received_From_Name = PartyNameEntry.Text.Trim(),
            Payment_Method_ID = method?.Id,
            Currency_ID = localCurrency.Id,
            Exchange_Rate = 1m,
            Amount = total,
            Foreign_Total = 0m,
            Local_Total = total,
            Reference_No = Clean(ReferenceEntry.Text),
            Description = Clean(DescriptionEditor.Text),
            Requires_Approval = true,
            Details = details
        };

        SetBusy(true);
        try
        {
            await _service.UpdateAsync(dto);
            await DisplayAlert("تم التعديل", "تم حفظ تعديلات سند القبض وإعادته إلى بداية دورة المراجعة.", "موافق");
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
