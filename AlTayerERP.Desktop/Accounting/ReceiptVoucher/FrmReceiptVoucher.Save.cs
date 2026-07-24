using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// التحقق من البيانات وحفظ سند القبض.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        #region === نماذج إرسال البيانات ===

        // نموذج بيانات طلب إنشاء السند المالي الرئيسي
        private sealed class CreateFinancialVoucherRequest
        {
            public int Voucher_Type_ID { get; set; } // معرف نوع السند
            public int Voucher_Status_ID { get; set; } // معرف حالة السند
            public string Branch_ID { get; set; } = string.Empty; // معرف الفرع
            public int Fiscal_Year_ID { get; set; } // معرف السنة المالية
            public DateTime Voucher_Date { get; set; } // تاريخ السند
            public DateTime Transaction_Date { get; set; } // تاريخ الحركة المالية
            public string Cash_Account_ID { get; set; } = string.Empty; // معرف حساب الصندوق أو البنك
            public string? Party_ID { get; set; } // معرف العميل أو المورد أو الحساب المرتبط
            public string? Received_From_Name { get; set; } // اسم الشخص المستلم منه كما ظهر في السند
            public int? Payment_Method_ID { get; set; } // معرف طريقة الدفع
            public int Currency_ID { get; set; } // معرف العملة
            public decimal Exchange_Rate { get; set; } // سعر صرف العملة
            public decimal Amount { get; set; } // المبلغ الأصلي المدخل بعملة السند
            public decimal Foreign_Total { get; set; } // إجمالي المبلغ بالعملة الأجنبية
            public decimal Local_Total { get; set; } // إجمالي المبلغ بالعملة المحلية
            public string? Reference_No { get; set; } // رقم المرجع
            public DateTime? Reference_Date { get; set; } // تاريخ المرجع
            public string? Against_Text { get; set; } // نص "وذلك مقابل"
            public string? Description { get; set; } // الوصف العام للسند
            public string? Notes { get; set; } // ملاحظات إضافية
            public int? Module_ID { get; set; } // معرف الوحدة أو النظام الفرعي
            public int? Document_Type_ID { get; set; } // معرف نوع المستند المرتبط
            public long? Document_ID { get; set; } // معرف المستند المرتبط
            public string? Source_Document_No { get; set; } // رقم مستند المصدر
            public bool Requires_Approval { get; set; } // هل يتطلب السند موافقة واعتماد؟
            public string Created_By { get; set; } = string.Empty; // المستخدم الذي قام بإنشاء السند
            public long Voucher_ID { get; set; } // معرف السند المالي
            public string Updated_By { get; set; } = string.Empty; // المستخدم الذي قام بتحديث السند
            public List<CreateFinancialVoucherDetailRequest> Details { get; set; } = new(); // قائمة تفاصيل أسطر السند المحاسبية
            public List<CreateDocumentAllocationRequest> Allocations { get; set; } = new(); // قائمة توزيع السند وتخصيصه على الفواتير
        }

        // نموذج بيانات طلب إنشاء تفاصيل السند المالي (الأطراف المقابلة)
        private sealed class CreateFinancialVoucherDetailRequest
        {
            public int Line_No { get; set; } // رقم السطر المحاسبي في الجدول
            public string Account_ID { get; set; } = string.Empty; // معرف الحساب المالي المقابل
            public string? Description { get; set; } // البيان أو الشرح الخاص بالسطر
            public string? Cost_Center_ID { get; set; } // معرف مركز التكلفة المرتبط بالسطر

            
            public string? Project_ID { get; set; }  // معرف المشروع المرتبط بالسطر
            public string? Reference_Type { get; set; }
            public string? Reference_No { get; set; }
            public string? Reference_Name { get; set; }
            public DateTime? Reference_Date { get; set; }
            public int Currency_ID { get; set; } // معرف العملة الخاصة بالسطر
            public decimal Exchange_Rate { get; set; } // سعر صرف العملة للسطر
            public decimal Foreign_Amount { get; set; } // المبلغ بالعملة الأجنبية للسطر
            public decimal Local_Amount { get; set; } // المبلغ بالعملة المحلية للسطر
            public decimal Debit_Amount { get; set; } // قيمة المبلغ المدين (يكون للصندوق/البنك)
            public decimal Credit_Amount { get; set; } // قيمة المبلغ الدائن (يكون للحسابات المقابلة)
            public byte Line_Type { get; set; } // نوع السطر (مثل 1 للمدين، 2 للدائن)
            public string? Notes { get; set; } // ملاحظات السطر المحاسبي
        }

        // نموذج بيانات تخصيص وتوزيع السند على المستندات والفواتير الأخرى
        private sealed class CreateDocumentAllocationRequest
        {
            public int Module_ID { get; set; } // معرف النظام الفرعي للمستند الموزع عليه
            public int Document_Type_ID { get; set; } // معرف نوع المستند (مثل فاتورة مبيعات)
            public long Document_ID { get; set; } // معرف المستند الفعلي
            public string Document_No { get; set; } = string.Empty; // رقم المستند المراد تخصيصه
            public string? Party_ID { get; set; } // معرف العميل أو الطرف المرتبط بالمستند
            public int Currency_ID { get; set; } // معرف العملة الخاصة بالمستند
            public decimal Exchange_Rate { get; set; } // سعر صرف عملة المستند
            public decimal Document_Total { get; set; } // إجمالي قيمة المستند الأصلية
            public decimal Collected_Before { get; set; } // المبالغ التي تم تحصيلها مسبقاً من هذا المستند
            public decimal Collected_Now { get; set; } // المبلغ الذي سيتم تحصيله الآن وتخصيصه من هذا السند
            public decimal Remaining_Balance { get; set; } // الرصيد المتبقي من المستند بعد التحصيل الحالي
            public string? Notes { get; set; } // ملاحظات التخصيص والتوزيع
        }

        // نموذج استجابة واجهة برمجة التطبيقات (API) عند حفظ السند المالي
        private sealed class FinancialVoucherApiResponse
        {
            public bool Success { get; set; } // حالة نجاح العملية (true/false)
            public string Message { get; set; } = string.Empty; // رسالة الاستجابة القادمة من السيرفر
            public long? Voucher_ID { get; set; }
            public string? Voucher_No { get; set; }
        }

        #endregion

        #region === حدث زر الحفظ ===

        /// <summary>
        /// حفظ سند جديد أو حفظ تعديلات سند موجود حسب حالة الشاشة.
        /// </summary>
        private async void btnSave_Click(object? sender, EventArgs e)
        {
            // لا يسمح بالحفظ في وضع العرض.
            if (_screenMode == VoucherScreenMode.View)
            {
                MessageBox.Show(
                    "اضغط زر جديد لإنشاء سند، أو افتح سندًا ثم اضغط تعديل.",
                    "سند القبض",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            bool isNewVoucher =
                _screenMode == VoucherScreenMode.New;

            bool isEditingVoucher =
                _screenMode == VoucherScreenMode.Edit;

            if (!isNewVoucher && !isEditingVoucher)
            {
                return;
            }

            // تثبيت قيمة الخلية التي يكتب فيها المستخدم قبل بدء التحقق والحفظ.
            Validate();
            dgvVoucherDetails.EndEdit();

            // عند التعديل يجب أن يكون السند محفوظًا.
            if (isEditingVoucher && _selectedVoucherId <= 0)
            {
                MessageBox.Show(
                    "معرف السند غير موجود.\nأعد البحث عن السند ثم حاول مرة أخرى.",
                    "تعذر التعديل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // منع تعديل السند المرحل.
            if (isEditingVoucher && chkPosted.Checked)
            {
                MessageBox.Show(
                    "لا يمكن تعديل سند مرحل.\nيجب إلغاء الترحيل أولًا.",
                    "السند مرحل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }
      //      شرط مبلغ االسند
            if (!ValidateVoucherBeforeSave(
                    out string validationMessage))
            {
                MessageBox.Show(
                    validationMessage,
                    "بيانات غير مكتملة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string operationName =
                isNewVoucher ? "حفظ" : "حفظ تعديلات";

            DialogResult confirmation = MessageBox.Show(
                $"هل تريد {operationName} سند القبض رقم:\n\n" +
                $"{txtVoucherNo.Text.Trim()} ؟",
                isNewVoucher ? "تأكيد الحفظ" : "تأكيد التعديل",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            try
            {
                btnSave.Enabled = false;
                btnUndo.Enabled = false;
                UseWaitCursor = true;

                lblStatusApi.Text = isNewVoucher
                    ? "API: جاري حفظ السند..."
                    : "API: جاري حفظ التعديلات...";

                CreateFinancialVoucherRequest request =
                   BuildCreateVoucherRequest();

                FinancialVoucherApiResponse response;

                if (isNewVoucher)
                {
                    response =
                        await SendCreateVoucherAsync(request);
                }
                else
                {
                    request.Voucher_ID =
                        _selectedVoucherId;

                    request.Updated_By =
                        CurrentSession.User_ID.ToString(
                            CultureInfo.InvariantCulture);

                    response =
                        await SendUpdateVoucherAsync(
                            _selectedVoucherId,
                            request);
                }

                if (!response.Success)
                {
                    lblStatusApi.Text =
                        "API: متصل - فشلت العملية";

                    MessageBox.Show(
                        response.Message,
                        isNewVoucher
                            ? "تعذر حفظ السند"
                            : "تعذر تعديل السند",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                string voucherNumber =
                    !string.IsNullOrWhiteSpace(response.Voucher_No)
                        ? response.Voucher_No.Trim()
                        : txtVoucherNo.Text.Trim();

                txtVoucherNo.Text = voucherNumber;

                // إعادة تحميل السند من قاعدة البيانات للحصول على
                // معرفه الحقيقي ورقم القيد وبقية البيانات.
                await SearchVoucherAsync(voucherNumber);

                lblStatusApi.Text = "API: متصل";
                lblStatusDatabase.Text =
                    "قاعدة البيانات: متصلة";

                MessageBox.Show(
                    response.Message,
                    isNewVoucher
                        ? "تم الحفظ"
                        : "تم التعديل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // العودة إلى وضع العرض وقفل الحقول.
                SetViewMode();
            }
            catch (HttpRequestException ex)
            {
                lblStatusApi.Text = "API: غير متصل";

                MessageBox.Show(
                    $"تعذر الاتصال بالـ API.\n\n{ex.Message}",
                    "خطأ في الاتصال",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (TaskCanceledException)
            {
                lblStatusApi.Text =
                    "API: انتهت مهلة الاتصال";

                MessageBox.Show(
                    "انتهت مهلة الاتصال بالـ API.",
                    "انتهاء مهلة الاتصال",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ أثناء تنفيذ العملية:\n\n{ex.Message}",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;

                if (_screenMode != VoucherScreenMode.View)
                {
                    btnSave.Enabled = true;
                    btnUndo.Enabled = true;
                }
            }
        }

        #endregion


        #region === التحقق من البيانات ===

        // دالة التحقق من صحة واكتمال بيانات السند قبل عملية الحفظ وتوليد رسالة خطأ عند وجود نقص
        private bool ValidateVoucherBeforeSave(out string errorMessage)
        {
            errorMessage = string.Empty;

            // التحقق من أن المستخدم مسجل دخوله حالياً في النظام
            if (!CurrentSession.IsLoggedIn)
            {
                errorMessage = "جلسة المستخدم غير صالحة.\nيرجى تسجيل الدخول من جديد.";
                return false;
            }

            // التحقق من تعيين معرف الشركة في الجلسة الحالية
            if (string.IsNullOrWhiteSpace(CurrentSession.Company_ID))
            {
                errorMessage = "لم يتم تحديد الشركة في جلسة المستخدم.";
                return false;
            }

            // التحقق من صحة تعيين معرف الفرع في الجلسة الحالية
            if (CurrentSession.Branch_ID <= 0)
            {
                errorMessage = "لم يتم تحديد الفرع في جلسة المستخدم.";
                return false;
            }

            // التحقق من صحة تعيين معرف السنة المالية في الجلسة الحالية
            if (CurrentSession.Year_ID <= 0)
            {
                errorMessage = "لم يتم تحديد السنة المالية.";
                return false;
            }

            // التحقق من أن رقم السند تم توليده بشكل صحيح ولا يحتوي على نصوص أخطاء أو حالة جاري التحميل
            if (string.IsNullOrWhiteSpace(txtVoucherNo.Text)
                || txtVoucherNo.Text.Contains("جاري", StringComparison.OrdinalIgnoreCase)
                || txtVoucherNo.Text.Contains("تعذر", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "رقم السند غير جاهز.\nاضغط زر جديد لتوليد رقم صحيح.";
                txtVoucherNo.Focus();
                return false;
            }

            // التحقق من اختيار نوع السند المالي من القائمة المنسدلة
            if (cmbVoucherType.SelectedValue == null)
            {
                errorMessage = "يرجى اختيار نوع السند.";
                cmbVoucherType.Focus();
                return false;
            }

            // التحقق من اختيار حالة السند من القائمة المنسدلة
            if (cmbStatus.SelectedValue == null)
            {
                errorMessage = "يرجى اختيار حالة السند.";
                cmbStatus.Focus();
                return false;
            }

            // التحقق من اختيار حساب الصندوق أو البنك الرئيسي الذي سيتم القبض فيه
            if (cmbCashAccount.SelectedValue == null)
            {
                errorMessage = "يرجى اختيار حساب الصندوق أو البنك.";
                cmbCashAccount.Focus();
                return false;
            }

            string receivedFromName = GetReceivedFromName();
            if (string.IsNullOrWhiteSpace(receivedFromName) ||
                receivedFromName.Equals("بدون طرف محدد", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "يرجى إدخال اسم الشخص في حقل استلمت من السيد.";
                cmbParty.Focus();
                return false;
            }

            // التحقق من اختيار عملة السند من القائمة المنسدلة
            if (cmbCurrency.SelectedValue == null)
            {
                errorMessage = "يرجى اختيار عملة السند.";
                cmbCurrency.Focus();
                return false;
            }

            if (cmbPaymentMethod.SelectedValue == null)
            {
                errorMessage = "يرجى اختيار طريقة السداد.";
                cmbPaymentMethod.Focus();
                return false;
            }

            // التحقق من أن سعر الصرف المدخل أكبر من الصفر
            if (numExchangeRate.Value <= 0m)
            {
                errorMessage = "سعر الصرف يجب أن يكون أكبر من صفر.";
                numExchangeRate.Focus();
                return false;
            }

            // التحقق من أن مبلغ السند بالعملة الأجنبية أكبر من الصفر

            if (numAmount.Value <= 0m)
            {
                errorMessage =
                    "يرجى إدخال المبلغ المقبوض.";

                numAmount.Focus();
                return false;
            }

            // علقت بسبب ان العمله صفر

            /*      if (numForeignAmount.Value <= 0m)
            {
                errorMessage = "يرجى إدخال المبلغ المقبوض.";
                numForeignAmount.Focus();
                return false;
            }
      */


            // التحقق من أن قيمة السند بالعملة المحلية أكبر من الصفر
            if (numLocalAmount.Value <= 0m)
            {
                errorMessage = "المبلغ بالعملة المحلية يجب أن يكون أكبر من صفر.";
                numLocalAmount.Focus();
                return false;
            }

            // التحقق من إدخال الشرح أو البيان الرئيسي للسند في حقل "وذلك مقابل"
            if (string.IsNullOrWhiteSpace(txtAgainst.Text))
            {
                errorMessage = "يرجى إدخال البيان في حقل وذلك مقابل.";
                txtAgainst.Focus();
                return false;
            }

            // جلب الأسطر الصالحة والمملوءة بالبيانات من جدول التفاصيل المحاسبية لتدقيقها
            var validRows = GetValidVoucherRows();
            if (validRows.Count == 0)
            {
                errorMessage = "يجب إدخال حساب مقابل واحد على الأقل داخل جدول التوزيع المحاسبي.";
                dgvVoucherDetails.Focus();
                return false;
            }

            decimal totalCredit = 0m; // متغير لتجميع إجمالي المبالغ الدائنة للأسطر
            foreach (DataGridViewRow row in validRows)
            {
                int visibleRowNo = row.Index + 1; // تحديد رقم السطر الفعلي لعرضه في رسالة الخطأ إن وجدت
                string accountId = GetCellString(row, colAccountCode.Name);

                // التأكد من عدم ترك خلية الحساب المالي فارغة في السطر الحالي
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    errorMessage = $"الحساب المالي مطلوب في السطر رقم {visibleRowNo}.";
                    dgvVoucherDetails.CurrentCell = row.Cells[colAccountCode.Name];
                    return false;
                }

                string cashAccountId = cmbCashAccount.SelectedValue?.ToString()?.Trim() ?? string.Empty;
                if (string.Equals(accountId, cashAccountId, StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = $"لا يمكن اختيار حساب الصندوق نفسه كحساب مقابل في السطر رقم {visibleRowNo}.";
                    dgvVoucherDetails.CurrentCell = row.Cells[colAccountCode.Name];
                    return false;
                }

                // جلب المبالغ وأسعار الصرف الخاصة بالسطر الحالي لحساب القيمة المحلية واختبارها
                decimal enteredAmount = GetRowEnteredAmount(row);
                decimal exchangeRate = GetRowExchangeRate(row);
                decimal localAmount = GetRowLocalAmount(row, enteredAmount, exchangeRate);

                // المبلغ المدخل مطلوب سواء كانت العملة محلية أو أجنبية.
                if (enteredAmount <= 0m)
                {
                    errorMessage = $"المبلغ يجب أن يكون أكبر من صفر في السطر رقم {visibleRowNo}.";
                    dgvVoucherDetails.CurrentCell = row.Cells[colAmount.Name];
                    return false;
                }

                // التأكد من أن سعر صرف السطر الحالي أكبر من الصفر
                if (exchangeRate <= 0m)
                {
                    errorMessage = $"سعر الصرف يجب أن يكون أكبر من صفر في السطر رقم {visibleRowNo}.";
                    dgvVoucherDetails.CurrentCell = row.Cells[colExchangeRate.Name];
                    return false;
                }

                // التأكد من أن القيمة المحلية الناتجة للسطر أكبر من الصفر
                if (localAmount <= 0m)
                {
                    errorMessage = $"المبلغ المحلي يجب أن يكون أكبر من صفر في السطر رقم {visibleRowNo}.";
                    return false;
                }

                string referenceDateText = GetCellString(row, colReferenceDate.Name);
                if (!string.IsNullOrWhiteSpace(referenceDateText) &&
                    !DateTime.TryParse(referenceDateText, CultureInfo.CurrentCulture, DateTimeStyles.None, out _) &&
                    !DateTime.TryParse(referenceDateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    errorMessage = $"تاريخ المرجع غير صحيح في السطر رقم {visibleRowNo}.";
                    dgvVoucherDetails.CurrentCell = row.Cells[colReferenceDate.Name];
                    return false;
                }

                totalCredit += localAmount; // إضافة القيمة المحلية للسطر الحالي إلى إجمالي الطرف الدائن
            }

            // تقريب إجمالي الطرف المدين (الرئيسي) والطرف الدائن (مجموع الأسطر) إلى خانتين عشريتين لضمان دقة المقارنة
            decimal totalDebit = decimal.Round(numLocalAmount.Value, 2, MidpointRounding.AwayFromZero);
            totalCredit = decimal.Round(totalCredit, 2, MidpointRounding.AwayFromZero);
            decimal difference = decimal.Round(totalDebit - totalCredit, 2, MidpointRounding.AwayFromZero); // حساب الفارق بين الطرفين

            // عرض قيم الإجماليات والفارق في الحقول المخصصة لها على الشاشة للمستخدم
            txtTotalAmount.Text = totalDebit.ToString("N2", CultureInfo.CurrentCulture);
            txtTotalForeignAmount.Text = numForeignAmount.Value.ToString("N2", CultureInfo.CurrentCulture);
            txtDifference.Text = difference.ToString("N2", CultureInfo.CurrentCulture);

            // التحقق من توازن السند (يجب أن يكون الفارق بين إجمالي المدين وإجمالي الدائن مساوياً للصفر تماماً)
            if (difference != 0m)
            {
                errorMessage = $"السند غير متوازن.\n\nإجمالي المدين: {totalDebit:N2}\nإجمالي الدائن: {totalCredit:N2}\nالفارق: {difference:N2}";
                return false;
            }

            return true; // إرجاع true في حال اجتياز كافة فحوصات الأمان وصحة البيانات
        }

        #endregion

        #region === بناء طلب الحفظ ===

        // دالة تقوم بتجميع البيانات من عناصر واجهة المستخدم وبناء كائن الطلب النهائي لإرساله إلى السيرفر
        private CreateFinancialVoucherRequest BuildCreateVoucherRequest()
        {
            // تحويل وجلب القيم المحددة من عناصر الواجهة المختلفة مع تنظيف النصوص
            int voucherTypeId = Convert.ToInt32(cmbVoucherType.SelectedValue, CultureInfo.InvariantCulture);
            bool isPaymentVoucher = string.Equals(_voucherTypeCode, "PAYMENT", StringComparison.OrdinalIgnoreCase);
            int voucherStatusId = Convert.ToInt32(cmbStatus.SelectedValue, CultureInfo.InvariantCulture);
            string cashAccountId = cmbCashAccount.SelectedValue?.ToString()?.Trim() ?? string.Empty;
            string? partyId = cmbParty.SelectedValue?.ToString()?.Trim();
            string receivedFromName = GetReceivedFromName();
            if (_screenMode == VoucherScreenMode.Edit &&
                string.IsNullOrWhiteSpace(partyId) &&
                string.Equals(receivedFromName, _loadedReceivedFromName, StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(_loadedPartyId))
            {
                partyId = _loadedPartyId;
            }
            int? paymentMethodId = TryConvertNullableInt(cmbPaymentMethod.SelectedValue);
            int currencyId = Convert.ToInt32(cmbCurrency.SelectedValue, CultureInfo.InvariantCulture);
            string? defaultCostCenterId = cmbCostCenter.SelectedValue?.ToString()?.Trim();
            string referenceNo = !string.IsNullOrWhiteSpace(txtReference.Text) ? txtReference.Text.Trim() : txtReferenceNo.Text.Trim();
            string branchId =
                _screenMode == VoucherScreenMode.Edit && !string.IsNullOrWhiteSpace(_loadedVoucherBranchId)
                    ? _loadedVoucherBranchId
                    : CurrentSession.Branch_ID.ToString(CultureInfo.InvariantCulture);
            int fiscalYearId =
                _screenMode == VoucherScreenMode.Edit && _loadedFiscalYearId > 0
                    ? _loadedFiscalYearId
                    : CurrentSession.Year_ID;

            // إنشاء وتعبئة كائن الطلب الرئيسي لبيانات رأس السند المالي
            var request = new CreateFinancialVoucherRequest
            {
                Voucher_Type_ID = voucherTypeId,
                Voucher_Status_ID = voucherStatusId,
                Branch_ID = branchId,
                Fiscal_Year_ID = fiscalYearId,
                Voucher_Date = dtVoucherDate.Value.Date,
                Transaction_Date = dtVoucherDate.Value,
                Cash_Account_ID = cashAccountId,
                Party_ID = string.IsNullOrWhiteSpace(partyId) ? null : partyId,
                Received_From_Name = receivedFromName,
                Payment_Method_ID = paymentMethodId,
                Currency_ID = currencyId,
                Exchange_Rate = decimal.Round(numExchangeRate.Value, 6, MidpointRounding.AwayFromZero),
                Amount = decimal.Round(numAmount.Value, 2, MidpointRounding.AwayFromZero),
                Foreign_Total = decimal.Round(numForeignAmount.Value, 2, MidpointRounding.AwayFromZero),
                Local_Total = decimal.Round(numLocalAmount.Value, 2, MidpointRounding.AwayFromZero),
                Reference_No = string.IsNullOrWhiteSpace(referenceNo) ? null : referenceNo,
                Reference_Date = string.IsNullOrWhiteSpace(referenceNo) ? null : dtReferenceDate.Value.Date,
                Against_Text = NullIfWhiteSpace(txtAgainst.Text),
                Description = NullIfWhiteSpace(txtAgainst.Text),
                Notes = NullIfWhiteSpace(txtHeaderNotes.Text),
                Module_ID = null,
                Document_Type_ID = null,
                Document_ID = null,
                Source_Document_No = null,
                

                Requires_Approval = checkBox2.Checked,
                Created_By = CurrentSession.User_ID.ToString(CultureInfo.InvariantCulture)
            };

            // قيد الصندوق أو البنك (الطرف المدين الرئيسي للسند) - يحمل رقم السطر 1 ونوع السطر 1
            request.Details.Add(new CreateFinancialVoucherDetailRequest
            {
                Line_No = 1,
                Account_ID = cashAccountId,
                Description = NullIfWhiteSpace(txtAgainst.Text),
                Cost_Center_ID = string.IsNullOrWhiteSpace(defaultCostCenterId) ? null : defaultCostCenterId,
                Project_ID = null,
                Currency_ID = currencyId,
                Exchange_Rate = decimal.Round(numExchangeRate.Value, 6, MidpointRounding.AwayFromZero),
                Foreign_Amount = decimal.Round(numForeignAmount.Value, 2, MidpointRounding.AwayFromZero),
                Local_Amount = decimal.Round(numLocalAmount.Value, 2, MidpointRounding.AwayFromZero),
                Debit_Amount = isPaymentVoucher ? 0m : decimal.Round(numLocalAmount.Value, 2, MidpointRounding.AwayFromZero),
                Credit_Amount = isPaymentVoucher ? decimal.Round(numLocalAmount.Value, 2, MidpointRounding.AwayFromZero) : 0m,
                Line_Type = 1,
                Notes = NullIfWhiteSpace(txtHeaderNotes.Text)
            });

            // توليد الحسابات المقابلة (الطرف الدائن للسند) من خلال الدوران على أسطر جدول البيانات وتعبئتها بكائن الطلب
            int lineNo = 2; // أسطر التوزيع تبدأ من الرقم 2 تتابعاً

            foreach (DataGridViewRow row in GetValidVoucherRows())
            {
                string accountId =
                    GetCellString(row, colAccountCode.Name);

                string description =
                    GetCellString(row, colDescription.Name);

                string costCenterId =
                    GetCellString(row, colCostCenter.Name);

                int rowCurrencyId =
                    GetRowCurrencyId(row, currencyId);

                decimal exchangeRate =
                    GetRowExchangeRate(row);

                decimal enteredAmount =
                    GetRowEnteredAmount(row);

                decimal foreignAmount =
                    GetRowForeignAmount(row);

                decimal localAmount =
                    GetRowLocalAmount(
                        row,
                        enteredAmount,
                        exchangeRate);

                string detailReferenceNo =
                    GetCellString(row, colReferenceNo.Name);

                string detailReferenceType =
                    GetCellString(row, colReferenceType.Name);

                string detailReferenceName =
                    GetCellString(row, colReferenceName.Name);

                string detailReferenceDateText =
                    GetCellString(row, colReferenceDate.Name);

                DateTime? detailReferenceDate = null;

                if (DateTime.TryParse(
                        detailReferenceDateText,
                        CultureInfo.CurrentCulture,
                        DateTimeStyles.None,
                        out DateTime parsedReferenceDate))
                {
                    detailReferenceDate =
                        parsedReferenceDate.Date;
                }

                string notes =
                    GetCellString(row, colNotes.Name);

                request.Details.Add(
                    new CreateFinancialVoucherDetailRequest
                    {
                        Line_No = lineNo++,
                        Account_ID = accountId,

                        Description =
                            string.IsNullOrWhiteSpace(description)
                                ? NullIfWhiteSpace(txtAgainst.Text)
                                : description,

                        Cost_Center_ID =
                            string.IsNullOrWhiteSpace(costCenterId)
                                ? defaultCostCenterId
                                : costCenterId,

                        Project_ID = null,

                        Reference_Type =
                            NullIfWhiteSpace(detailReferenceType),

                        Reference_No =
                            NullIfWhiteSpace(detailReferenceNo),

                        Reference_Name =
                            NullIfWhiteSpace(detailReferenceName),

                        Reference_Date =
                            detailReferenceDate,

                        Currency_ID = rowCurrencyId,

                        Exchange_Rate =
                            decimal.Round(
                                exchangeRate,
                                6,
                                MidpointRounding.AwayFromZero),

                        Foreign_Amount =
                            decimal.Round(
                                foreignAmount,
                                2,
                                MidpointRounding.AwayFromZero),

                        Local_Amount =
                            decimal.Round(
                                localAmount,
                                2,
                                MidpointRounding.AwayFromZero),

                        Debit_Amount = isPaymentVoucher
                            ? decimal.Round(localAmount, 2, MidpointRounding.AwayFromZero)
                            : 0m,

                        Credit_Amount = isPaymentVoucher
                            ? 0m
                            : decimal.Round(localAmount, 2, MidpointRounding.AwayFromZero),

                        Line_Type = 2,

                        Notes =
                NullIfWhiteSpace(notes)
                    });
            }

            return request;
        }

        #endregion

        #region === إرسال طلب الحفظ ===

        // دالة تقوم بإرسال طلب الحفظ المبني إلى عنوان الـ API بشكل غير متزامن وقراءة الاستجابة الناتجة وتحليلها
        private async Task<FinancialVoucherApiResponse> SendCreateVoucherAsync(CreateFinancialVoucherRequest request)
        {
            string requestUrl = $"{_baseUrl}FinancialVoucher"; // دمج العنوان الأساسي مع اسم المورد المالي للـ API

            // إرسال كائن الطلب بتنسيق JSON عبر طريقة HTTP Post إلى العنوان المحدد والانتظار حتى انتهاء الاستجابة
            using HttpResponseMessage httpResponse = await _client.PostAsJsonAsync(requestUrl, request);
            string rawMessage = await httpResponse.Content.ReadAsStringAsync(); // قراءة محتوى الاستجابة النصي الخام القادم من السيرفر

            FinancialVoucherApiResponse? apiResponse = null;

            try
            {
                // محاولة فك تشفير النص الخام القادم وتحويله إلى نموذج الاستجابة المعتمد لدينا كـ JSON
                if (!string.IsNullOrWhiteSpace(rawMessage))
                {
                    apiResponse = System.Text.Json.JsonSerializer.Deserialize<FinancialVoucherApiResponse>(rawMessage,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch { } // تجاهل أخطاء فك التشفير في حال كانت الاستجابة ليست بتنسيق JSON المتوقع

            if (apiResponse != null) return apiResponse; // إرجاع الكائن المحلل في حال نجاح عملية فك التشفير

            // بناء كائن استجابة افتراضي يدوي في حال فشل تحليل رد السيرفر أو في حال عدم وجود رد نصي مفصل
            return new FinancialVoucherApiResponse
            {
                Success = httpResponse.IsSuccessStatusCode, // تحديد النجاح بناءً على رمز الحالة الخاص بـ HTTP (مثل 200 OK)
                Message = string.IsNullOrWhiteSpace(rawMessage)
                    ? (httpResponse.IsSuccessStatusCode ? "تم حفظ سند القبض بنجاح." : $"فشل حفظ سند القبض. رمز الاستجابة: {(int)httpResponse.StatusCode}")
                    : rawMessage
            };
        }

        /// <summary>
        /// إرسال تعديلات سند قبض محفوظ إلى الـ API.
        /// </summary>
        private async Task<FinancialVoucherApiResponse>
            SendUpdateVoucherAsync(
                long voucherId,
                CreateFinancialVoucherRequest request)
        {
            string requestUrl =
                $"{_baseUrl}FinancialVoucher/{voucherId}";

            using HttpResponseMessage httpResponse =
                await _client.PutAsJsonAsync(
                    requestUrl,
                    request);

            string rawMessage =
                await httpResponse.Content.ReadAsStringAsync();

            FinancialVoucherApiResponse? apiResponse = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(rawMessage))
                {
                    apiResponse =
                        System.Text.Json.JsonSerializer
                            .Deserialize<FinancialVoucherApiResponse>(
                                rawMessage,
                                new System.Text.Json
                                    .JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                }
            }
            catch
            {
                // إذا لم تكن الاستجابة JSON،
                // سيتم استخدام النص الخام في الأسفل.
            }

            if (apiResponse != null)
            {
                return apiResponse;
            }

            return new FinancialVoucherApiResponse
            {
                Success = httpResponse.IsSuccessStatusCode,

                Message = string.IsNullOrWhiteSpace(rawMessage)
                    ? httpResponse.IsSuccessStatusCode
                        ? "تم تعديل سند القبض بنجاح."
                        : $"فشل تعديل سند القبض. " +
                          $"رمز الاستجابة: " +
                          $"{(int)httpResponse.StatusCode}"
                    : rawMessage
            };
        }

        #endregion






        /// <summary>
        /// يعيد الاسم الظاهر في حقل استلمت من السيد، سواء اختير طرف مسجل
        /// أو كتب المستخدم اسمًا يدويًا.
        /// </summary>
        private string GetReceivedFromName()
        {
            if (cmbParty.SelectedItem is PartyLookupModel selectedParty &&
                !string.IsNullOrWhiteSpace(selectedParty.Party_ID) &&
                !string.IsNullOrWhiteSpace(selectedParty.Party_Name_AR))
            {
                return selectedParty.Party_Name_AR.Trim();
            }

            return cmbParty.Text?.Trim() ?? string.Empty;
        }

        #region === قراءة الصفوف ===

        // دالة لجلب كافة الأسطر الصالحة من جدول البيانات مع استبعاد سطر الإضافة الجديد والأسطر الفارغة تماماً
        private List<DataGridViewRow> GetValidVoucherRows()
        {
            return dgvVoucherDetails.Rows.Cast<DataGridViewRow>()
                .Where(row => !row.IsNewRow && RowContainsData(row))
                .ToList();
        }

        // دالة للتحقق مما إذا كان السطر الحالي يحتوي على بيانات حقيقية (معرف حساب مالي أو قيمة مبلغ أكبر من الصفر)
        private bool RowContainsData(DataGridViewRow row)
        {
            string accountId = GetCellString(row, colAccountCode.Name);
            decimal amount = GetRowEnteredAmount(row);
            return !string.IsNullOrWhiteSpace(accountId) || amount > 0m;
        }

        // المبلغ الذي أدخله المستخدم في عمود المبلغ، مع بدائل آمنة عند تحميل سند قديم.
        private decimal GetRowEnteredAmount(DataGridViewRow row)
        {
            decimal amount = GetCellDecimal(row, colAmount.Name);
            if (amount > 0m) return amount;

            decimal foreignAmount = GetCellDecimal(row, colForeignAmount.Name);
            if (foreignAmount > 0m) return foreignAmount;

            return GetCellDecimal(row, colLocalAmount.Name);
        }

        // العملة المحلية لا تحمل مبلغًا أجنبيًا في قاعدة البيانات.
        private decimal GetRowForeignAmount(DataGridViewRow row)
        {
            int defaultCurrencyId = Convert.ToInt32(
                cmbCurrency.SelectedValue,
                CultureInfo.InvariantCulture);

            int currencyId = GetRowCurrencyId(row, defaultCurrencyId);
            CurrencyLookupModel? currency =
                _currencyLookups.FirstOrDefault(x => x.Currency_ID == currencyId);

            if (currency != null &&
                (currency.Is_Local_Currency ||
                 currency.Currency_Code.Equals("YER", StringComparison.OrdinalIgnoreCase)))
            {
                return 0m;
            }

            decimal foreignAmount = GetCellDecimal(row, colForeignAmount.Name);
            return foreignAmount > 0m ? foreignAmount : GetRowEnteredAmount(row);
        }

        // دالة لجلب سعر الصرف للسطر الحالي، وتعتمد سعر صرف السند الرئيسي في حال لم يحدد سعر خاص بالسطر
        private decimal GetRowExchangeRate(DataGridViewRow row)
        {
            decimal exchangeRate = GetCellDecimal(row, colExchangeRate.Name);
            return exchangeRate > 0m ? exchangeRate : numExchangeRate.Value;
        }

        // دالة لحساب أو جلب قيمة المبلغ بالعملة المحلية للسطر، وتقريب الناتج النهائي إلى خانتين عشريتين
        private decimal GetRowLocalAmount(DataGridViewRow row, decimal enteredAmount, decimal exchangeRate)
        {
            decimal localAmount = GetCellDecimal(row, colLocalAmount.Name);
            if (localAmount > 0m)
                return decimal.Round(localAmount, 2, MidpointRounding.AwayFromZero);

            int defaultCurrencyId = Convert.ToInt32(
                cmbCurrency.SelectedValue,
                CultureInfo.InvariantCulture);
            int currencyId = GetRowCurrencyId(row, defaultCurrencyId);
            CurrencyLookupModel? currency =
                _currencyLookups.FirstOrDefault(x => x.Currency_ID == currencyId);

            if (currency != null &&
                (currency.Is_Local_Currency ||
                 currency.Currency_Code.Equals("YER", StringComparison.OrdinalIgnoreCase)))
            {
                return decimal.Round(enteredAmount, 2, MidpointRounding.AwayFromZero);
            }

            return decimal.Round(enteredAmount * exchangeRate, 2, MidpointRounding.AwayFromZero);
        }

        // دالة لجلب معرف العملة الخاص بالسطر الحالي وتعتمد معرف العملة الرئيسي كخيار افتراضي في حال عدم التحديد
        private int GetRowCurrencyId(DataGridViewRow row, int defaultCurrencyId)
        {
            object? value = row.Cells[colCurrency.Name].Value;
            if (value == null) return defaultCurrencyId;
            if (value is int integerValue) return integerValue;

            return int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int currencyId)
                ? currencyId : defaultCurrencyId;
        }

        // دالة مساعدة لقراءة القيمة النصية من خلية معينة داخل السطر وإزالة المسافات الزائدة منها بأمان
        private static string GetCellString(DataGridViewRow row, string columnName)
        {
            return row.Cells[columnName].Value?.ToString()?.Trim() ?? string.Empty;
        }

        // دالة مساعدة لقراءة القيم الرقمية العشارية من خلايا الجدول ومعالجة كافة أنواع البيانات والأشكال المحتملة للرقم بأمان وتحويلها إلى decimal
        private static decimal GetCellDecimal(DataGridViewRow row, string columnName)
        {
            object? value = row.Cells[columnName].Value;
            if (value == null || value == DBNull.Value) return 0m;
            if (value is decimal decimalValue) return decimalValue;
            if (value is int integerValue) return integerValue;
            if (value is long longValue) return longValue;
            if (value is double doubleValue) return Convert.ToDecimal(doubleValue, CultureInfo.InvariantCulture);

            string text = value.ToString()?.Trim() ?? string.Empty;
            // محاولة التحقق من الصيغة الرقمية للثقافة الحالية للجهاز أو الثقافة العالمية المحايدة لمنع حدوث أخطاء تحويل
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal currentResult))
                return currentResult;
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal invariantResult))
                return invariantResult;

            return 0m;
        }

        #endregion
    }
}
