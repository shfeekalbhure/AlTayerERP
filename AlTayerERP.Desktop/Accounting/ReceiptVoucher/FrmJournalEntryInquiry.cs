using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Accounting.ReceiptVoucher
{
    public partial class FrmJournalEntryInquiry : Form
    {
        private readonly HttpClient _client = ApiService.Client;
        private readonly string _baseUrl = ApiService.BaseUrl;

        private readonly string _voucherNo;

        public FrmJournalEntryInquiry(string voucherNo)
        {
            InitializeComponent();

            _voucherNo = voucherNo?.Trim() ?? string.Empty;

            Load += FrmJournalEntryInquiry_Load;
            btnClose.Click += btnClose_Click;
            btnPrint.Click += btnPrint_Click;
        }

        private async void FrmJournalEntryInquiry_Load(
            object? sender,
            EventArgs e)
        {
            ConfigureScreen();

            if (string.IsNullOrWhiteSpace(_voucherNo))
            {
                MessageBox.Show(
                    "رقم السند غير موجود.",
                    "تنبيه",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                Close();
                return;
            }

            txtDocumentNo.Text = _voucherNo;

            await LoadJournalEntryAsync();
        }

        private void ConfigureScreen()
        {
            txtDocumentNo.ReadOnly = true;
            txtDocumentName.ReadOnly = true;
            txtTotalDebit.ReadOnly = true;
            txtTotalCredit.ReadOnly = true;

            dgvJournalDetails.AutoGenerateColumns = false;
            dgvJournalDetails.AllowUserToAddRows = false;
            dgvJournalDetails.AllowUserToDeleteRows = false;
            dgvJournalDetails.ReadOnly = true;
            dgvJournalDetails.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            SetColumn(
                "colNo",
                "No");

            SetColumn(
                "colAccountCode",
                "Account_Code");

            SetColumn(
                "colAccountName",
                "Account_Name");

            SetColumn(
                "colDebit",
                "Debit");

            SetColumn(
                "colCredit",
                "Credit");

            SetColumn(
                "colCurrency",
                "Currency_Name");

            SetColumn(
                "colExchangeRate",
                "Exchange_Rate");

            SetColumn(
                "colLocalDebit",
                "Local_Debit");

            SetColumn(
                "colLocalCredit",
                "Local_Credit");

            SetColumn(
                "colForeignAmount",
                "Foreign_Amount");

            SetColumn(
                "colCostCenter",
                "Cost_Center_Name");

            SetColumn(
                "colDescription",
                "Description");
        }

        private void SetColumn(
            string columnName,
            string dataPropertyName)
        {
            if (dgvJournalDetails.Columns.Contains(columnName))
            {
                dgvJournalDetails.Columns[columnName]
                    .DataPropertyName = dataPropertyName;
            }
        }

        private async Task LoadJournalEntryAsync()
        {
            try
            {
                UseWaitCursor = true;

                string baseUrl =
                    _baseUrl.TrimEnd('/');

                string encodedVoucherNo =
                    Uri.EscapeDataString(_voucherNo);

                string url =
                    $"{baseUrl}/FinancialVoucher/" +
                    $"journal-entry/{encodedVoucherNo}";

                using HttpResponseMessage response =
                    await _client.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    MessageBox.Show(
                        "لا يوجد قيد محاسبي لهذا السند.",
                        "استعراض القيد",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "تعذر جلب القيد المحاسبي.",
                        "خطأ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                JournalEntryApiResponse? result =
                    await response.Content.ReadFromJsonAsync
                    <JournalEntryApiResponse>(options);

                if (result == null ||
                    !result.Success ||
                    result.Data == null)
                {
                    MessageBox.Show(
                        result?.Message ??
                        "لم يتم العثور على بيانات القيد.",
                        "تنبيه",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                DisplayJournalEntry(result.Data);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "تعذر الاتصال بخادم النظام.\n\n" +
                    ex.Message,
                    "خطأ اتصال",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء تحميل القيد:\n\n" +
                    ex.Message,
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void DisplayJournalEntry(
            JournalEntryData data)
        {
            txtDocumentNo.Text = data.Voucher_No;
            txtDocumentName.Text = data.Voucher_Type_Name;

            dgvJournalDetails.DataSource = null;
            dgvJournalDetails.DataSource = data.Details;

            txtTotalDebit.Text =
                data.Total_Debit.ToString("N2");

            txtTotalCredit.Text =
                data.Total_Credit.ToString("N2");
        }

        private void btnClose_Click(
            object? sender,
            EventArgs e)
        {
            Close();
        }

        private void btnPrint_Click(
            object? sender,
            EventArgs e)
        {
            MessageBox.Show(
                "سنضيف الطباعة بعد التأكد من عرض القيد.",
                "الطباعة",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private sealed class JournalEntryApiResponse
        {
            public bool Success { get; set; }

            public string Message { get; set; } = string.Empty;

            public JournalEntryData? Data { get; set; }
        }

        private sealed class JournalEntryData
        {
            public string Voucher_No { get; set; } = string.Empty;

            public string Voucher_Type_Name { get; set; } = string.Empty;

            public decimal Total_Debit { get; set; }

            public decimal Total_Credit { get; set; }

            public List<JournalEntryLine> Details { get; set; }
                = new List<JournalEntryLine>();
        }

        private sealed class JournalEntryLine
        {
            public int Line_No { get; set; }

            public int No => Line_No;

            public string Account_Code { get; set; } = string.Empty;

            public string Account_Name { get; set; } = string.Empty;

            public decimal Debit_Amount { get; set; }

            public decimal Credit_Amount { get; set; }

            public decimal Debit => Debit_Amount;

            public decimal Credit => Credit_Amount;

            public string Currency_Name { get; set; } = string.Empty;

            public decimal Exchange_Rate { get; set; }

            public decimal Local_Debit { get; set; }

            public decimal Local_Credit { get; set; }

            public decimal Foreign_Amount { get; set; }

            public string Cost_Center_Name { get; set; } = string.Empty;

            public string? Description { get; set; }
        }
    }
}