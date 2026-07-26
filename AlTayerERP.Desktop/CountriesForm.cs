diff --git a/AlTayerERP.Desktop/CountriesForm.cs b/AlTayerERP.Desktop/CountriesForm.cs
index b523512..2b81e03 100644
--- a/AlTayerERP.Desktop/CountriesForm.cs
+++ b/AlTayerERP.Desktop/CountriesForm.cs
@@ -13,6 +13,7 @@ public partial class CountriesForm : BaseForm
     private readonly BindingSource _rows = new();
     private List<CountryRow> _allRows = new();
     private int _selectedId;
+    private bool _selectedIsActive;
     private string? _selectedCurrencyCode;
     // حقل الجنسية غير ظاهر في التصميم المعتمد؛ نحتفظ بالقيمة السابقة عند تعديل دولة قائمة.
     private string? _selectedNationality;
@@ -42,6 +43,7 @@ public partial class CountriesForm : BaseForm
             if (_editorMode == EditorMode.New) await CancelChangesAsync();
             else BeginEdit();
         };
+        btnDeactivate.Click += async (_, _) => await ChangeStatusAsync(_selectedIsActive);
         btnCancel.Click += async (_, _) => await CancelChangesAsync();
         btnReset.Click += (_, _) => ResetCurrentInput();
         btnRefresh.Click += async (_, _) => await LoadRowsAsync();
@@ -119,6 +121,7 @@ public partial class CountriesForm : BaseForm
     {
         if (dgvCountries.CurrentRow?.DataBoundItem is not CountryRow row) return;
         _selectedId = row.Country_ID;
+        _selectedIsActive = row.Is_Active;
         _selectedCurrencyCode = row.Currency_Code;
         _selectedNationality = row.Nationality_Name_AR;
         txtCountryCode.Text = row.Country_Code; txtCountryNameAr.Text = row.Country_Name_AR; txtCountryNameEn.Text = row.Country_Name_EN ?? string.Empty;
@@ -186,7 +189,7 @@ public partial class CountriesForm : BaseForm
 
     private void StartNew()
     {
-        _selectedId = 0; _selectedCurrencyCode = null; _selectedNationality = null;
+        _selectedId = 0; _selectedIsActive = false; _selectedCurrencyCode = null; _selectedNationality = null;
         ClearInputFields();
         dgvCountries.ClearSelection();
         ClearAudit();
@@ -202,7 +205,7 @@ public partial class CountriesForm : BaseForm
 
     private void ClearEditor()
     {
-        _selectedId = 0; _selectedCurrencyCode = null; _selectedNationality = null;
+        _selectedId = 0; _selectedIsActive = false; _selectedCurrencyCode = null; _selectedNationality = null;
         ClearInputFields();
         dgvCountries.ClearSelection();
         ClearAudit();
@@ -228,12 +231,15 @@ public partial class CountriesForm : BaseForm
         btnReset.Enabled = editable;
         btnRefresh.Enabled = mode == EditorMode.View;
         btnPrint.Enabled = mode == EditorMode.View && _selectedId > 0;
+        btnDeactivate.Enabled = mode == EditorMode.View && _selectedId > 0;
+        btnDeactivate.Text = _selectedIsActive ? "إيقاف" : "إعادة تفعيل";
         dgvCountries.Enabled = mode == EditorMode.View;
     }
 
-    private async Task ChangeStatusAsync(bool reactivate)
+    private async Task ChangeStatusAsync(bool isCurrentlyActive)
     {
         if (_selectedId <= 0) { MessageBox.Show("اختر دولة أولاً."); return; }
+        var reactivate = !isCurrentlyActive;
         var action = reactivate ? "إعادة تفعيل" : "إيقاف";
         var reason = Microsoft.VisualBasic.Interaction.InputBox($"أدخل سبب {action} الدولة:", action, string.Empty).Trim();
         if (string.IsNullOrWhiteSpace(reason)) { MessageBox.Show("السبب إلزامي للتدقيق."); return; }
