using AlTayerERP.Desktop.Services;
using AlTayerERP.Desktop.Accounting.ReceiptVoucher;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    /// <summary>
    /// أزرار التعديل والحذف والطباعة.
    /// </summary>
    public partial class FrmReceiptVoucher
    {
        #region === متغيرات الطباعة ===

        // وثيقة الطباعة الخاصة بالسند
        private readonly PrintDocument _receiptPrintDocument = new PrintDocument();
        // مؤشر السطر الحالي أثناء الطباعة
        private int _printRowIndex;

        #endregion

        #region === تسجيل الأحداث ===

        // دالة لتسجيل أحداث أزرار العمليات (تعديل، حذف، طباعة) وتجنب التكرار
        private void RegisterVoucherActionEvents()
        {
            btnEdit.Click -= btnEdit_Click;
            btnEdit.Click += btnEdit_Click;

            btnDelete.Click -= btnDelete_Click;
            btnDelete.Click += btnDelete_Click;

            btnPrint.Click -= btnPrint_Click;
            btnPrint.Click += btnPrint_Click;

            btnRefresh.Click -= btnRefresh_Click;
            btnRefresh.Click += btnRefresh_Click;

            btnAttachments.Click -= btnAttachments_Click;
            btnAttachments.Click += btnAttachments_Click;

            btnViewJournalEntry.Click -= btnViewJournalEntry_Click;
            btnViewJournalEntry.Click += btnViewJournalEntry_Click;
            // للتراجع
            btnUndo.Click -= btnUndo_Click;
            btnUndo.Click += btnUndo_Click;

            btnApprove.Click -= btnApprove_Click;
            btnApprove.Click += btnApprove_Click;

            btnCancelApprove.Click -= btnCancelApprove_Click;
            btnCancelApprove.Click += btnCancelApprove_Click;

            btnPost.Click -= btnPost_Click;
            btnPost.Click += btnPost_Click;

            btnUnPost.Click -= btnUnPost_Click;
            btnUnPost.Click += btnUnPost_Click;

            // استخدمنا زري الاستيراد والتصدير السابقين لدورة المراجعة.
            btnImport.Click -= btnReview_Click;
            btnImport.Click += btnReview_Click;

            btnExport.Click -= btnReturnForCorrection_Click;
            btnExport.Click += btnReturnForCorrection_Click;



        }

        #endregion

        #region === نماذج الاعتماد ===

        private sealed class VoucherActionRequest
        {
            public string User_ID { get; set; } = string.Empty;

            public string Action_Channel { get; set; } = "DESKTOP";

            public string? Device_Name { get; set; }

            public string? Notes { get; set; }
        }

        private sealed class VoucherReasonActionRequest
        {
            public string User_ID { get; set; } = string.Empty;
            public string Reason { get; set; } = string.Empty;
            public string Action_Channel { get; set; } = "DESKTOP";
            public string? Device_Name { get; set; }
        }

        #endregion

        #region === المرفقات واستعراض القيد ===

        private void btnAttachments_Click(object? sender, EventArgs e)
        {
            if (_selectedVoucherId <= 0)
            {
                MessageBox.Show("احفظ أو ابحث عن سند القبض أولاً.", "المرفقات",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FrmVoucherAttachments(_selectedVoucherId);
            form.ShowDialog(this);
        }

        private void btnViewJournalEntry_Click(object? sender, EventArgs e)
        {
            string voucherNo = txtVoucherNo.Text.Trim();
            if (_selectedVoucherId <= 0 || string.IsNullOrWhiteSpace(voucherNo))
            {
                MessageBox.Show("احفظ أو ابحث عن سند القبض أولاً.", "استعراض القيد",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FrmJournalEntryInquiry(voucherNo);
            form.ShowDialog(this);
        }

        #endregion

        #region === زر التعديل ===

        /// <summary>
        /// فتح السند الحالي في وضع التعديل.
        /// </summary>
        private void btnEdit_Click(object? sender, EventArgs e)
        {
            SetEditMode();
        }


        #endregion


        #region === زر الحذف ===

        // الحدث الخاص بالنقر على زر الحذف
        private async void btnDelete_Click(object? sender, EventArgs e)
        {
            // التحقق من اختيار سند أولاً
            if (_selectedVoucherId <= 0)
            {
                MessageBox.Show("ابحث عن السند المراد حذفه أولًا.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // منع الحذف إذا كان السند مرحلاً
            if (chkPosted.Checked)
            {
                MessageBox.Show("لا يمكن حذف سند مرحل.\nيجب إلغاء الترحيل أولًا.", "السند مرحل",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // عرض رسالة تحذيرية لتأكيد الحذف
            DialogResult confirmation = MessageBox.Show(
                $"هل أنت متأكد من حذف سند القبض رقم:\n{txtVoucherNo.Text}؟\n\nلا يمكن التراجع عن هذه العملية.",
                "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmation != DialogResult.Yes) return;

            try
            {
                // تغيير شكل المؤشر وتجميد الزر
                UseWaitCursor = true;
                btnDelete.Enabled = false;

                // تجهيز اسم المستخدم وإرسال طلب الحذف إلى الـ API
                string userName = Uri.EscapeDataString(CurrentSession.User_ID.ToString());
                HttpResponseMessage response = await _client.DeleteAsync(
                    $"{_baseUrl}FinancialVoucher/{_selectedVoucherId}?deletedBy={userName}");

                // التحقق من استجابة الـ API
                if (!response.IsSuccessStatusCode)
                {
                    await ShowVoucherApiErrorAsync(response, "تعذر حذف سند القبض.");
                    return;
                }

                // تأكيد الحذف وتصفير الواجهة لتجهيز سند جديد
                MessageBox.Show("تم حذف سند القبض بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NewVoucher();
                await GenerateVoucherNumberAsync();
            }
            catch (Exception ex)
            {
                // معالجة الأخطاء الاستثنائية
                MessageBox.Show($"حدث خطأ أثناء حذف سند القبض.\n\n{ex.Message}", "خطأ الحذف",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // إعادة تفعيل الزر وإرجاع شكل المؤشر
                if (_screenMode == VoucherScreenMode.View)
                {
                    UpdateWorkflowButtonsState();
                }
                UseWaitCursor = false;
            }
        }

        #endregion
        #region === زر التحديث ===

        /// <summary>
        /// إعادة تحميل بيانات سند القبض الظاهر حاليًا من قاعدة البيانات.
        /// </summary>
        private async void btnRefresh_Click(object? sender, EventArgs e)
        {
            string voucherNumber = txtVoucherNo.Text.Trim();

            if (string.IsNullOrWhiteSpace(voucherNumber))
            {
                MessageBox.Show(
                    "رقم السند غير موجود.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                UseWaitCursor = true;
                btnRefresh.Enabled = false;

                await SearchVoucherAsync(voucherNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ أثناء تحديث بيانات السند.\n\n{ex.Message}",
                    "خطأ التحديث",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
                UseWaitCursor = false;
            }
        }

        #endregion


        #region === زر التراجع ===

        /// <summary>
        /// إلغاء السند الجديد أو التعديلات غير المحفوظة،
        /// ثم العودة إلى وضع العرض.
        /// </summary>
        private async void btnUndo_Click(
            object? sender,
            EventArgs e)
        {
            DialogResult confirmation = MessageBox.Show(
                "هل تريد إلغاء التغييرات غير المحفوظة؟",
                "تأكيد التراجع",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            try
            {
                UseWaitCursor = true;
                btnUndo.Enabled = false;

                if (_screenMode == VoucherScreenMode.Edit &&
                    _selectedVoucherId > 0)
                {
                    string voucherNumber =
                        txtVoucherNo.Text.Trim();

                    if (!string.IsNullOrWhiteSpace(voucherNumber))
                    {
                        await SearchVoucherAsync(voucherNumber);
                    }
                }
                else if (_screenMode == VoucherScreenMode.New)
                {
                    NewVoucher();
                }

                SetViewMode();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ أثناء التراجع.\n\n{ex.Message}",
                    "خطأ التراجع",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        #endregion

        #region === زر الطباعة ===

        // الحدث الخاص بالنقر على زر الطباعة والمعاينة
        private async void btnPrint_Click(object? sender, EventArgs e)
        {
            // التحقق من اختيار السند
            if (_selectedVoucherId <= 0)
            {
                MessageBox.Show("ابحث عن السند المراد طباعته أولًا.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // تهيئة إعدادات المستند والصفحة
                _printRowIndex = 0;
                _receiptPrintDocument.DocumentName = $"سند قبض - {txtVoucherNo.Text}";
                _receiptPrintDocument.DefaultPageSettings.Landscape = false;
                _receiptPrintDocument.PrintPage -= ReceiptPrintDocument_PrintPage;
                _receiptPrintDocument.PrintPage += ReceiptPrintDocument_PrintPage;

                // تجهيز نافذة معاينة الطباعة وضبط خصائص العرض والدعم العربي
                using PrintPreviewDialog preview = new PrintPreviewDialog
                {
                    Document = _receiptPrintDocument,
                    Width = 1200,
                    Height = 800,
                    RightToLeft = RightToLeft.Yes,
                    RightToLeftLayout = true
                };

                // عرض نافذة المعاينة
                preview.ShowDialog(this);

                await RecordPrintOperationAsync();
            }
            catch (Exception ex)
            {
                // معالجة خطأ فشل عرض المعاينة
                MessageBox.Show($"تعذر عرض معاينة الطباعة.\n\n{ex.Message}", "خطأ الطباعة",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
        #region === زر اعتماد السند ===

        /// <summary>
        /// اعتماد سند القبض الحالي.
        /// </summary>
        private async void btnApprove_Click(
            object? sender,
            EventArgs e)
        {
            // إذا كان السند غير محفوظ
            // فإن النظام يمنع الاعتماد.
            if (_selectedVoucherId <= 0)
            {
                MessageBox.Show(
                    "ابحث عن السند المراد اعتماده أولًا.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // إذا كان السند مرحلًا
            // فإن النظام يمنع الاعتماد.
            if (chkPosted.Checked)
            {
                MessageBox.Show(
                    "لا يمكن اعتماد سند مرحل محاسبيًا.",
                    "السند مرحل",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // إذا كان السند لا يحتاج اعتمادًا
            // فإن النظام لا يرسل طلب الاعتماد.
            if (!checkBox2.Checked)
            {
                MessageBox.Show(
                    "هذا السند غير محدد كمعلّق ولا يتطلب اعتمادًا.",
                    "الاعتماد",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            if (_currentReviewStatus != 2)
            {
                MessageBox.Show(
                    "يجب الضغط على (تمت المراجعة) قبل اعتماد السند.",
                    "المراجعة مطلوبة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmation =
                MessageBox.Show(
                    $"هل تريد اعتماد سند القبض رقم:\n" +
                    $"{txtVoucherNo.Text.Trim()}؟",
                    "تأكيد الاعتماد",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            try
            {
                UseWaitCursor = true;
                btnApprove.Enabled = false;

                var request = new VoucherActionRequest
                {
                    User_ID = CurrentSession.User_ID.ToString(),
                    Action_Channel = "DESKTOP",
                    Device_Name = Environment.MachineName,
                    Notes = "تم اعتماد سند القبض من شاشة سطح المكتب."
                };

                using HttpResponseMessage response =
                    await _client.PostAsJsonAsync(
                        $"{_baseUrl}FinancialVoucher/" +
                        $"{_selectedVoucherId}/approve",
                        request);

                if (!response.IsSuccessStatusCode)
                {
                    await ShowVoucherApiErrorAsync(
                        response,
                        "تعذر اعتماد سند القبض.");

                    return;
                }

                MessageBox.Show(
                    "تم اعتماد سند القبض بنجاح.",
                    "نجاح الاعتماد",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // إعادة جلب السند بعد الاعتماد
                // حتى تظهر حالته الجديدة من قاعدة البيانات.
                string voucherNumber =
                    txtVoucherNo.Text.Trim();

                if (!string.IsNullOrWhiteSpace(voucherNumber))
                {
                    await SearchVoucherAsync(voucherNumber);
                }

                SetViewMode();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"تعذر الاتصال بالـ API أثناء اعتماد السند.\n\n" +
                    $"{ex.Message}",
                    "خطأ الاتصال",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ أثناء اعتماد سند القبض.\n\n" +
                    $"{ex.Message}",
                    "خطأ الاعتماد",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;

                if (_screenMode == VoucherScreenMode.View)
                {
                    UpdateWorkflowButtonsState();
                }
            }
        }

        #endregion

        #region === المراجعة والاعتماد والترحيل ===

        private async void btnReview_Click(object? sender, EventArgs e)
        {
            if (_selectedVoucherId <= 0 || chkPosted.Checked || _currentApprovalStatus == 2)
            {
                return;
            }

            DialogResult confirmation = MessageBox.Show(
                $"هل تؤكد أنك راجعت جميع بيانات السند رقم:\n{txtVoucherNo.Text.Trim()}؟",
                "تأكيد المراجعة",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirmation != DialogResult.Yes) return;

            var request = new VoucherActionRequest
            {
                User_ID = CurrentSession.User_ID.ToString(),
                Action_Channel = "DESKTOP",
                Device_Name = Environment.MachineName,
                Notes = "تمت مراجعة بيانات سند القبض كاملة من شاشة سطح المكتب."
            };

            await ExecuteVoucherActionAsync(
                "review",
                request,
                "تعذر تأكيد مراجعة السند.",
                "تمت مراجعة السند بنجاح.");
        }

        private async void btnReturnForCorrection_Click(object? sender, EventArgs e)
        {
            if (_selectedVoucherId <= 0 || chkPosted.Checked || _currentApprovalStatus == 2)
            {
                return;
            }

            string reason = Microsoft.VisualBasic.Interaction.InputBox(
                "اكتب سبب إعادة السند للتصحيح:",
                "إعادة للتصحيح",
                string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("سبب الإعادة مطلوب.", "إعادة للتصحيح",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var request = BuildReasonRequest(reason);
            await ExecuteVoucherActionAsync(
                "return-for-correction",
                request,
                "تعذر إعادة السند للتصحيح.",
                "تمت إعادة السند للتصحيح.");
        }

        private async void btnCancelApprove_Click(object? sender, EventArgs e)
        {
            if (_selectedVoucherId <= 0 || chkPosted.Checked || _currentApprovalStatus != 2)
            {
                return;
            }

            string reason = Microsoft.VisualBasic.Interaction.InputBox(
                "اكتب سبب إلغاء اعتماد السند:",
                "إلغاء الاعتماد",
                string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("سبب إلغاء الاعتماد مطلوب.", "إلغاء الاعتماد",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await ExecuteVoucherActionAsync(
                "cancel-approval",
                BuildReasonRequest(reason),
                "تعذر إلغاء اعتماد السند.",
                "تم إلغاء اعتماد السند بنجاح.");
        }

        private async void btnPost_Click(object? sender, EventArgs e)
        {
            if (_selectedVoucherId <= 0 || chkPosted.Checked)
            {
                return;
            }

            if (_currentReviewStatus != 2)
            {
                MessageBox.Show("يجب إتمام المراجعة قبل الترحيل.", "المراجعة مطلوبة",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (checkBox2.Checked && _currentApprovalStatus != 2)
            {
                MessageBox.Show("هذا السند معلّق ويجب اعتماده قبل الترحيل.", "الاعتماد مطلوب",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmation = MessageBox.Show(
                $"هل تريد ترحيل السند رقم:\n{txtVoucherNo.Text.Trim()} وإنشاء القيد المحاسبي؟",
                "تأكيد الترحيل",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirmation != DialogResult.Yes) return;

            var request = new VoucherActionRequest
            {
                User_ID = CurrentSession.User_ID.ToString(),
                Action_Channel = "DESKTOP",
                Device_Name = Environment.MachineName,
                Notes = "تم ترحيل سند القبض من شاشة سطح المكتب."
            };

            await ExecuteVoucherActionAsync(
                "post",
                request,
                "تعذر ترحيل السند.",
                "تم ترحيل السند وإنشاء القيد المحاسبي بنجاح.");
        }

        private async void btnUnPost_Click(object? sender, EventArgs e)
        {
            if (_selectedVoucherId <= 0 || !chkPosted.Checked)
            {
                return;
            }

            string reason = Microsoft.VisualBasic.Interaction.InputBox(
                "اكتب سبب إلغاء ترحيل السند:",
                "إلغاء الترحيل",
                string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("سبب إلغاء الترحيل مطلوب.", "إلغاء الترحيل",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await ExecuteVoucherActionAsync(
                "unpost",
                BuildReasonRequest(reason),
                "تعذر إلغاء ترحيل السند.",
                "تم إلغاء ترحيل السند بنجاح.");
        }

        private VoucherReasonActionRequest BuildReasonRequest(string reason)
        {
            return new VoucherReasonActionRequest
            {
                User_ID = CurrentSession.User_ID.ToString(),
                Reason = reason,
                Action_Channel = "DESKTOP",
                Device_Name = Environment.MachineName
            };
        }

        private async Task<bool> ExecuteVoucherActionAsync<TRequest>(
            string action,
            TRequest request,
            string defaultError,
            string successMessage)
        {
            try
            {
                UseWaitCursor = true;
                using HttpResponseMessage response = await _client.PostAsJsonAsync(
                    $"{_baseUrl}FinancialVoucher/{_selectedVoucherId}/{action}",
                    request);

                if (!response.IsSuccessStatusCode)
                {
                    await ShowVoucherApiErrorAsync(response, defaultError);
                    return false;
                }

                MessageBox.Show(successMessage, "سند القبض",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                string voucherNumber = txtVoucherNo.Text.Trim();
                if (!string.IsNullOrWhiteSpace(voucherNumber))
                {
                    await SearchVoucherAsync(voucherNumber);
                }

                SetViewMode();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{defaultError}\n\n{ex.Message}", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private async Task RecordPrintOperationAsync()
        {
            var request = new VoucherActionRequest
            {
                User_ID = CurrentSession.User_ID.ToString(),
                Action_Channel = "DESKTOP",
                Device_Name = Environment.MachineName,
                Notes = "تم فتح معاينة طباعة سند القبض."
            };

            using HttpResponseMessage response = await _client.PostAsJsonAsync(
                $"{_baseUrl}FinancialVoucher/{_selectedVoucherId}/record-print",
                request);

            if (!response.IsSuccessStatusCode)
            {
                await ShowVoucherApiErrorAsync(response, "تعذر تسجيل عملية الطباعة.");
                return;
            }

            string voucherNumber = txtVoucherNo.Text.Trim();
            if (!string.IsNullOrWhiteSpace(voucherNumber))
            {
                await SearchVoucherAsync(voucherNumber);
            }
        }

        #endregion

        #region === تصميم صفحة الطباعة ===

        // رسم محتويات صفحة الطباعة
        private void ReceiptPrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Rectangle page = e.MarginBounds;

            // إنشاء الخطوط والأقلام وأدوات التنسيق بالنص العربي
            using Font companyFont = new Font("Arial", 15, FontStyle.Bold);
            using Font titleFont = new Font("Arial", 18, FontStyle.Bold);
            using Font headerFont = new Font("Arial", 10, FontStyle.Bold);
            using Font normalFont = new Font("Arial", 10, FontStyle.Regular);
            using Pen borderPen = new Pen(Color.Black, 1);
            using StringFormat rightFormat = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.DirectionRightToLeft
            };
            using StringFormat centerFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.DirectionRightToLeft
            };

            float y = page.Top;

            // طباعة رأس التقرير (اسم الشركة والعنوان)
            string companyName = string.IsNullOrWhiteSpace(CurrentSession.Company_Name)
                ? "شركة الطائر السعيد للنقل" : CurrentSession.Company_Name;

            graphics.DrawString(companyName, companyFont, Brushes.Black,
                new RectangleF(page.Left, y, page.Width, 30), centerFormat);
            y += 35;

            graphics.DrawString("سند قبض", titleFont, Brushes.Black,
                new RectangleF(page.Left, y, page.Width, 35), centerFormat);
            y += 45;

            // رسم خط فاصل
            graphics.DrawLine(borderPen, page.Left, y, page.Right, y);
            y += 10;

            // طباعة بيانات السند الأساسية في حقول مزدوجة
            DrawPrintField(graphics, normalFont, rightFormat, page, ref y,
                "رقم السند", txtVoucherNo.Text, "تاريخ السند", dtVoucherDate.Value.ToString("yyyy/MM/dd"));
            DrawPrintField(graphics, normalFont, rightFormat, page, ref y,
                "الفرع", cmbBranch.Text, "حالة السند", cmbStatus.Text);
            DrawPrintField(graphics, normalFont, rightFormat, page, ref y,
                "الصندوق أو البنك", cmbCashAccount.Text, "الطرف", cmbParty.Text);
            DrawPrintField(graphics, normalFont, rightFormat, page, ref y,
                "العملة", cmbCurrency.Text, "سعر الصرف", numExchangeRate.Value.ToString("N6"));
            DrawPrintField(graphics, normalFont, rightFormat, page, ref y,
                "المبلغ", numAmount.Value.ToString("N2"), "المبلغ المحلي", numLocalAmount.Value.ToString("N2"));
            DrawPrintField(graphics, normalFont, rightFormat, page, ref y,
                "المبلغ الأجنبي", numForeignAmount.Value.ToString("N2"), "طريقة السداد", cmbPaymentMethod.Text);

            // طباعة حقل البيان/الملاحظات
            graphics.DrawString($"البيان: {txtHeaderNotes.Text}", normalFont, Brushes.Black,
                new RectangleF(page.Left, y, page.Width, 45), rightFormat);
            y += 50;

            // تهيئة عناوين جدول تفاصيل الحسابات وأبعاد الأعمدة
            float tableLeft = page.Left;
            float[] columnWidths = { 55, 105, 190, 85, 75, 80, 90, 90 };
            string[] headers = { "م", "رقم الحساب", "اسم الحساب", "مركز التكلفة", "العملة", "سعر الصرف", "المبلغ الأجنبي", "المبلغ المحلي" };
            float tableWidth = columnWidths.Sum();
            float headerHeight = 32;
            float x = tableLeft;

            // رسم خلايا ترويسة الجدول
            for (int i = 0; i < headers.Length; i++)
            {
                RectangleF rect = new RectangleF(x, y, columnWidths[i], headerHeight);
                graphics.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width, rect.Height);
                graphics.DrawString(headers[i], headerFont, Brushes.Black, rect, centerFormat);
                x += columnWidths[i];
            }
            y += headerHeight;

            // رسم أسطر الجدول وتعبئتها بالبيانات من الـ DataGridView
            float rowHeight = 28;
            while (_printRowIndex < dgvVoucherDetails.Rows.Count)
            {
                DataGridViewRow row = dgvVoucherDetails.Rows[_printRowIndex];
                if (row.IsNewRow)
                {
                    _printRowIndex++;
                    continue;
                }

                // التحقق من المساحة المتبقية في الصفحة لإنشاء صفحة جديدة عند الحاجة
                if (y + rowHeight > page.Bottom - 100)
                {
                    e.HasMorePages = true;
                    return;
                }

                // جلب القيم من خلايا السطر الحالي
                string accountId = Convert.ToString(row.Cells[colAccountCode.Name].Value) ?? string.Empty;
                string accountName = Convert.ToString(row.Cells[colAccountName.Name].FormattedValue) ?? string.Empty;
                string costCenter = Convert.ToString(row.Cells[colCostCenter.Name].FormattedValue) ?? string.Empty;
                string currency = Convert.ToString(row.Cells[colCurrency.Name].FormattedValue) ?? string.Empty;
                decimal exchangeRate = GetGridDecimalValue(row.Cells[colExchangeRate.Name].Value);
                decimal foreignAmount = GetGridDecimalValue(row.Cells[colForeignAmount.Name].Value);
                decimal localAmount = GetGridDecimalValue(row.Cells[colLocalAmount.Name].Value);

                // ترتيب القيم لطباعتها في الأعمدة المقابلة لها
                string[] values =
                {
                    (_printRowIndex + 1).ToString(),
                    accountId,
                    accountName,
                    costCenter,
                    currency,
                    exchangeRate.ToString("N6"),
                    foreignAmount.ToString("N2"),
                    localAmount.ToString("N2")
                };

                x = tableLeft;
                for (int i = 0; i < values.Length; i++)
                {
                    RectangleF rect = new RectangleF(x, y, columnWidths[i], rowHeight);
                    graphics.DrawRectangle(borderPen, rect.X, rect.Y, rect.Width, rect.Height);
                    graphics.DrawString(values[i], normalFont, Brushes.Black, rect, centerFormat);
                    x += columnWidths[i];
                }

                y += rowHeight;
                _printRowIndex++;
            }

            // طباعة الإجماليات والتوقيعات أسفل الجدول
            y += 15;
            graphics.DrawString($"إجمالي المبلغ المحلي: {numLocalAmount.Value:N2}", headerFont, Brushes.Black,
                new RectangleF(page.Left, y, page.Width, 25), rightFormat);
            y += 28;
            graphics.DrawString($"إجمالي المبلغ الأجنبي: {numForeignAmount.Value:N2}", headerFont, Brushes.Black,
                new RectangleF(page.Left, y, page.Width, 25), rightFormat);
            y += 50;

            // رسم خطوط التوقيع للمستلم والمحاسب
            graphics.DrawString("المستلم: ____________________", normalFont, Brushes.Black, page.Right - 240, y);
            graphics.DrawString("المحاسب: ____________________", normalFont, Brushes.Black, page.Left, y);
            y += 35;

            // طباعة اسم المستخدم من الجلسة الحالية
            graphics.DrawString($"المستخدم: {CurrentSession.Full_Name}", normalFont, Brushes.Black,
                new RectangleF(page.Left, y, page.Width, 25), rightFormat);

            // تصفير المؤشر وإنهاء الصفحات
            _printRowIndex = 0;
            e.HasMorePages = false;
        }

        #endregion

        #region === أدوات رسم الطباعة ===

        // دالة مساعدة لطباعة حقلين متجاورين في سطر واحد لتوفير المساحة وتنسيق المظهر
        private static void DrawPrintField(Graphics graphics, Font font, StringFormat format, Rectangle page,
            ref float y, string firstLabel, string firstValue, string secondLabel, string secondValue)
        {
            float halfWidth = page.Width / 2f;
            graphics.DrawString($"{firstLabel}: {firstValue}", font, Brushes.Black,
                new RectangleF(page.Left + halfWidth, y, halfWidth, 25), format);
            graphics.DrawString($"{secondLabel}: {secondValue}", font, Brushes.Black,
                new RectangleF(page.Left, y, halfWidth, 25), format);
            y += 28;
        }

        #endregion

        #region === التحقق قبل العمليات ===

        // دالة للتحقق من سلامة واكتمال البيانات والقيود المفروضة قبل الحفظ أو التعديل
        private bool ValidateVoucherBeforeAction()
        {
            // التأكد من وجود رقم السند
            if (string.IsNullOrWhiteSpace(txtVoucherNo.Text))
            {
                MessageBox.Show("رقم السند غير موجود.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // التأكد من اختيار حساب الصندوق أو البنك
            if (cmbCashAccount.SelectedValue == null)
            {
                MessageBox.Show("اختر حساب الصندوق أو البنك.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCashAccount.Focus();
                return false;
            }

            // التأكد من تحديد عملة السند
            if (cmbCurrency.SelectedValue == null)
            {
                MessageBox.Show("اختر العملة.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbCurrency.Focus();
                return false;
            }

            // التأكد من أن قيمة مبلغ السند أكبر من صفر
            if (numAmount.Value <= 0)
            {
                MessageBox.Show("أدخل مبلغ السند.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numAmount.Focus();
                return false;
            }

            // فحص تفاصيل السند والتأكد من إدخال سطر حقيقي واحد على الأقل يحتوي حساب ومبلغ
            int detailCount = 0;
            foreach (DataGridViewRow row in dgvVoucherDetails.Rows)
            {
                if (row.IsNewRow) continue;
                string accountId = Convert.ToString(row.Cells[colAccountCode.Name].Value) ?? string.Empty;
                decimal amount = GetGridDecimalValue(row.Cells[colAmount.Name].Value);
                if (!string.IsNullOrWhiteSpace(accountId) && amount > 0) detailCount++;
            }

            // التنبيه في حال خلو الجدول من التفاصيل الصحيحة
            if (detailCount == 0)
            {
                MessageBox.Show("أدخل سطرًا واحدًا على الأقل في تفاصيل السند.", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dgvVoucherDetails.Focus();
                return false;
            }

            return true;
        }

        #endregion

        #region === معالجة أخطاء API ===

        // دالة لقراءة رسائل الخطأ الراجعة من خادم الـ API وعرضها للمستخدم بشكل مفهوم
        private static async Task ShowVoucherApiErrorAsync(HttpResponseMessage response, string defaultMessage)
        {
            string errorMessage = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                try
                {
                    using JsonDocument document = JsonDocument.Parse(errorMessage);
                    if (document.RootElement.TryGetProperty("message", out JsonElement messageElement))
                    {
                        errorMessage = messageElement.GetString() ?? errorMessage;
                    }
                }
                catch (JsonException)
                {
                    // الاستجابة نص عادي وليست JSON.
                }
            }
            if (string.IsNullOrWhiteSpace(errorMessage))
                errorMessage = $"{defaultMessage}\nرمز الخطأ: {(int)response.StatusCode}";

            MessageBox.Show(errorMessage, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion
    }
}
