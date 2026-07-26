diff --git a/AlTayerERP.Desktop/CountriesForm.Designer.cs b/AlTayerERP.Desktop/CountriesForm.Designer.cs
index d57902b..86f7b2b 100644
--- a/AlTayerERP.Desktop/CountriesForm.Designer.cs
+++ b/AlTayerERP.Desktop/CountriesForm.Designer.cs
@@ -115,6 +115,7 @@ namespace AlTayerERP.Desktop.Forms
             this.btnNew = new System.Windows.Forms.Button();     // أداة زر: [+ جديد] لإنشاء سجل جديد
             this.btnSave = new System.Windows.Forms.Button();    // أداة زر: [✔ حفظ] لحفظ البيانات المدخلة
             this.btnEdit = new System.Windows.Forms.Button();    // أداة زر: [✎ تعديل] لتعديل السجل الحالي
+            this.btnDeactivate = new System.Windows.Forms.Button(); // أداة زر: إيقاف أو إعادة تفعيل السجل الحالي
             this.btnCancel = new System.Windows.Forms.Button();  // أداة زر: [✖ إلغاء] للتراجع عن العمليات
             this.btnReset = new System.Windows.Forms.Button();   // أداة زر: [↺ إعادة] لتفريغ الحقول
             this.btnRefresh = new System.Windows.Forms.Button(); // أداة زر: [↻ تحديث] لجلب البيانات من القاعدة
@@ -156,6 +157,10 @@ namespace AlTayerERP.Desktop.Forms
             this.lblCurrency = new System.Windows.Forms.Label();      // أداة عنوان النص: "العملة الرسمية"
             this.cmbCurrency = new System.Windows.Forms.ComboBox();   // أداة قائمة منسدلة لاختيار العملة (مثل YER, SAR)
 
+            // --- حقل: اسم الجنسية بالعربية ---
+            this.lblNationality = new System.Windows.Forms.Label();   // أداة عنوان النص: "اسم الجنسية بالعربية"
+            this.txtNationality = new System.Windows.Forms.TextBox(); // أداة مربع نص لإدخال الجنسية المرتبطة بالدولة
+
             // --- حقل: ترتيب الظهور ---
             this.lblDisplayOrder = new System.Windows.Forms.Label();          // أداة عنوان النص: "ترتيب الظهور"
             this.numDisplayOrder = new System.Windows.Forms.NumericUpDown(); // أداة خانة أرقام تنازلية/تصاعدية للترتيب
@@ -215,8 +220,8 @@ namespace AlTayerERP.Desktop.Forms
             // -------------------------------------------------------------------------
             this.RightToLeft = System.Windows.Forms.RightToLeft.Yes; // ضبط اتجاه الواجهة بالكامل من اليمين إلى اليسار (عربي)
             this.RightToLeftLayout = true;
-            this.ClientSize = new System.Drawing.Size(980, 620);     // الأبعاد القياسية المريحة للشاشة
-            this.MinimumSize = new System.Drawing.Size(900, 550);    // الحد الأدنى لمقاس النافذة
+            this.ClientSize = new System.Drawing.Size(1080, 720);    // مساحة كافية للحقول والجدول وبطاقات التدقيق
+            this.MinimumSize = new System.Drawing.Size(920, 650);    // يمنع قص حقول الإدخال والتذييل
             this.Text = "نظام الطائر السعيد - إدارة الدول";       // عنوان النافذة في الشريط العلوي
             this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252))))); // خلفية رمادي فاتح مريح
             this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
@@ -230,7 +235,7 @@ namespace AlTayerERP.Desktop.Forms
             this.mainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
             this.mainTableLayout.RowCount = 5;
             this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));  // الصف 0: شريط الأزرار العلوية
-            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 245F)); // الصف 1: 🟢 بطاقة البيانات الأساسية (+1 سم توسعة)
+            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 285F)); // الصف 1: بطاقة البيانات الأساسية بما فيها الجنسية
             this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 85F));  // الصف 2: 🟢 إطار البحث والتصفية (+1 سم توسعة)
             this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F)); // الصف 3: 🔴 جدول عرض الدول (تم تقليصه تلقائياً 2 سم)
             this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 75F));  // الصف 4: بطاقات التدقيق والتذييل السفلي
@@ -239,25 +244,27 @@ namespace AlTayerERP.Desktop.Forms
             // [إعدادات وتنسيق لوحة الأزرار العلوية - Top Toolbar Panel]
             // -------------------------------------------------------------------------
             this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
-            this.panelButtons.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
-            this.panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight; // رصف الأزرار من اليمين
-            this.panelButtons.Padding = new System.Windows.Forms.Padding(4);
+            // FlowDirection وحده يثبت أول زر عند أقصى اليمين داخل واجهة RTL.
+            this.panelButtons.RightToLeft = System.Windows.Forms.RightToLeft.No;
+            this.panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
+            this.panelButtons.Padding = new System.Windows.Forms.Padding(8, 6, 8, 4);
 
             System.Windows.Forms.Button[] buttons = {
-                btnNew, btnSave, btnEdit, btnCancel, btnReset, btnRefresh, btnSearch, btnPrint, btnClose
+                btnNew, btnSave, btnEdit, btnDeactivate, btnPrint, btnSearch, btnRefresh, btnCancel, btnReset, btnClose
             };
-            string[] titles = { "+ جديد", "✔ حفظ", "✎ تعديل", "✖ إلغاء", "↺ إعادة", "↻ تحديث", "🔍 بحث", "🖨 طباعة", "🚪 إغلاق" };
+            string[] titles = { "+ جديد", "✔ حفظ", "✎ تعديل", "إيقاف", "🖨 طباعة", "🔍 بحث", "↻ تحديث", "✖ إلغاء", "↺ إعادة", "🚪 إغلاق" };
 
             // تخصيص الألوان المعاصرة والحديثة (ERP Modern Palette) لكل زر على حدة
             System.Drawing.Color[] bgColors = {
                 System.Drawing.Color.FromArgb(13, 148, 136),   // زر جديد (أخضر تركوازي Teal)
                 System.Drawing.Color.FromArgb(37, 99, 235),    // زر حفظ (أزرق ملكي Royal Blue)
                 System.Drawing.Color.FromArgb(217, 119, 6),    // زر تعديل (برتقالي دافئ Amber)
+                System.Drawing.Color.FromArgb(220, 38, 38),    // زر إيقاف (أحمر)
+                System.Drawing.Color.FromArgb(124, 58, 237),   // زر طباعة (بنفسجي غامق Purple)
+                System.Drawing.Color.FromArgb(5, 150, 105),    // زر بحث (أخضر زمردي Emerald)
+                System.Drawing.Color.FromArgb(2, 132, 199),    // زر تحديث (أزرق سماوي Sky Blue)
                 System.Drawing.Color.FromArgb(100, 116, 139),  // زر إلغاء (رمادي Slate Gray)
                 System.Drawing.Color.FromArgb(79, 70, 229),    // زر إعادة (بنفسجي Indigo)
-                System.Drawing.Color.FromArgb(2, 132, 199),    // زر تحديث (أزرق سماوي Sky Blue)
-                System.Drawing.Color.FromArgb(5, 150, 105),    // زر بحث (أخضر زمردي Emerald)
-                System.Drawing.Color.FromArgb(124, 58, 237),   // زر طباعة (بنفسجي غامق Purple)
                 System.Drawing.Color.FromArgb(220, 38, 38)     // زر إغلاق (أحمر مميز Rose Red)
             };
 
@@ -294,16 +301,16 @@ namespace AlTayerERP.Desktop.Forms
             this.tblDataCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));  // 🟢 عمود فاصل مجوف لمنع الالتصاق
             this.tblDataCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 115F)); // عناوين العمود الثاني
             this.tblDataCard.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));  // حقول إدخال العمود الثاني
-            this.tblDataCard.RowCount = 5;
+            this.tblDataCard.RowCount = 6;
 
             // إعطاء ارتفاع 42px لكل صف إدخال ليكون واسعاً ومريحاً جداً للمستخدم
-            for (int r = 0; r < 5; r++)
+            for (int r = 0; r < 6; r++)
                 this.tblDataCard.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
 
             // تنسيق جميع عناوين الحقول لتكون بارزة وغامقة (Bold Labels) وواضحة للعين
             System.Windows.Forms.Label[] labels = {
                 lblCountryCode, lblCountryNameAr, lblCountryNameEn, lblPhoneKey,
-                lblIso2, lblIso3, lblCurrency, lblDisplayOrder, lblNotes
+                lblIso2, lblIso3, lblCurrency, lblDisplayOrder, lblNationality, lblNotes
             };
             foreach (var lbl in labels)
             {
@@ -316,7 +323,7 @@ namespace AlTayerERP.Desktop.Forms
             // ضبط هوامش مربعات النص والقوائم لتقليص أطوالها وجعلها متناسقة
             System.Windows.Forms.Control[] inputs = {
                 txtCountryCode, txtCountryNameAr, txtCountryNameEn, txtPhoneKey,
-                txtIso2, txtIso3, cmbCurrency, numDisplayOrder, txtNotes
+                txtIso2, txtIso3, cmbCurrency, numDisplayOrder, txtNationality, txtNotes
             };
             foreach (var input in inputs)
             {
@@ -364,11 +371,18 @@ namespace AlTayerERP.Desktop.Forms
             this.tblDataCard.Controls.Add(lblDisplayOrder, 3, 3); // إضافة عنوان ترتيب الظهور
             this.tblDataCard.Controls.Add(numDisplayOrder, 4, 3); // إضافة حقل ترتيب الظهور
 
-            // --- تعبئة الصف الخامس (4): الملاحظات (ممتدة أفقياً عبر 4 أعمدة) ---
+            // --- تعبئة الصف الخامس (4): اسم الجنسية بالعربية ---
+            lblNationality.Text = "اسم الجنسية بالعربية:";
+            txtNationality.Dock = System.Windows.Forms.DockStyle.Fill;
+            this.tblDataCard.Controls.Add(lblNationality, 0, 4);
+            this.tblDataCard.Controls.Add(txtNationality, 1, 4);
+            this.tblDataCard.SetColumnSpan(txtNationality, 4);
+
+            // --- تعبئة الصف السادس (5): الملاحظات (ممتدة أفقياً عبر 4 أعمدة) ---
             lblNotes.Text = "ملاحظات:";
             txtNotes.Dock = System.Windows.Forms.DockStyle.Fill;
-            this.tblDataCard.Controls.Add(lblNotes, 0, 4);  // إضافة عنوان الملاحظات
-            this.tblDataCard.Controls.Add(txtNotes, 1, 4);  // إضافة حقل الملاحظات
+            this.tblDataCard.Controls.Add(lblNotes, 0, 5);  // إضافة عنوان الملاحظات
+            this.tblDataCard.Controls.Add(txtNotes, 1, 5);  // إضافة حقل الملاحظات
             this.tblDataCard.SetColumnSpan(txtNotes, 4);    // مد مربع الملاحظات على عرض الأعمدة بالكامل
 
             // -------------------------------------------------------------------------
@@ -514,13 +528,13 @@ namespace AlTayerERP.Desktop.Forms
         // =========================================================================
         private System.Windows.Forms.TableLayoutPanel mainTableLayout; // الحاوية الرئيسية
         private System.Windows.Forms.FlowLayoutPanel panelButtons;     // لوحة الأزرار
-        private System.Windows.Forms.Button btnNew, btnSave, btnEdit, btnCancel, btnReset, btnRefresh, btnSearch, btnPrint, btnClose; // الأزرار التسعة
+        private System.Windows.Forms.Button btnNew, btnSave, btnEdit, btnDeactivate, btnCancel, btnReset, btnRefresh, btnSearch, btnPrint, btnClose;
         private System.Windows.Forms.GroupBox grpDataCard;             // إطار البيانات الأساسية
         private System.Windows.Forms.TableLayoutPanel tblDataCard;      // جدول حقول البيانات
 
         // أدوات العناوين وحقول الإدخال الخاصة بالدول
-        private System.Windows.Forms.Label lblCountryCode, lblCountryNameAr, lblCountryNameEn, lblDisplayOrder, lblIso2, lblIso3, lblPhoneKey, lblCurrency, lblNotes;
-        private System.Windows.Forms.TextBox txtCountryCode, txtCountryNameAr, txtCountryNameEn, txtIso2, txtIso3, txtPhoneKey, txtNotes;
+        private System.Windows.Forms.Label lblCountryCode, lblCountryNameAr, lblCountryNameEn, lblDisplayOrder, lblIso2, lblIso3, lblPhoneKey, lblCurrency, lblNationality, lblNotes;
+        private System.Windows.Forms.TextBox txtCountryCode, txtCountryNameAr, txtCountryNameEn, txtIso2, txtIso3, txtPhoneKey, txtNationality, txtNotes;
         private System.Windows.Forms.NumericUpDown numDisplayOrder;
         private System.Windows.Forms.ComboBox cmbCurrency;
 
@@ -538,4 +552,4 @@ namespace AlTayerERP.Desktop.Forms
         private System.Windows.Forms.GroupBox grpCreationData, grpModificationData, grpCounters;
         private System.Windows.Forms.Label lblCreatedBy, lblCreatedAt, lblModifiedBy, lblModifiedAt, lblEditCount, lblPrintCount;
     }
-}
\ No newline at end of file
+}
