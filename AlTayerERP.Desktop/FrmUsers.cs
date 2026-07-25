// استدعاء ملف الخدمات المركزي المسؤول عن توفير كائن HttpClient وإعدادات الاتصال بالسيرفر
using AlTayerERP.Desktop.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlTayerERP.Desktop
{
    // تعريف كلاس الشاشة الرئيسي كـ partial class ليتكامل مع ملف الـ Designer
    public partial class FrmUsers : Form
    {
        // --- المتغيرات العامة على مستوى الشاشة ---

        // كائن الاتصال بالشبكة المأخوذ من الملف المركزي لإرسال الطلبات للسيرفر
        private readonly HttpClient _client = ApiService.Client;

        // الرابط الأساسي لـ API المنظومة مأخوذ من الملف المركزي
        private readonly string _baseUrl = ApiService.BaseUrl;

        // متغير يحمل الرقم المعرف الفرعي (ID) للمستخدم المحدد حالياً في الجدول (0 تعني وضع إضافة مستخدم جديد)
        private int _selectedUserId = 0;

        // ذاكرة كاش مؤقتة لحفظ قائمة المستخدمين القادمة من السيرفر لسرعة المعالجة والفلترة الفورية
        private List<UserListModel> _usersCache = new();

        // ذاكرة كاش مؤقتة لحفظ أسماء شاشات النظام المجلوبة من قاعدة البيانات لغرض بناء جدول صلاحيات الوظائف
        private List<ScreenPermissionModel> _screensCache = new();

        // ذاكرة كاش مؤقتة لحفظ الصلاحيات الفعلية المجلوبة للدور (Role) المحدد حالياً
        private List<RolePermissionModel> _rolePermissionsCache = new();

        // متغير العلم والحارس (Flag): يستخدم لمنع الـ WinForms من إطلاق أحداث برمجية مكررة أو متداخلة أثناء تفريغ أو تعبئة الحقول
        private bool _isBinding = false;

        /// <summary>
        /// مشيد الشاشة الرئيسي (Constructor) - يتم فيه بناء عناصر الواجهة وربط الأحداث بشكل آمن ومستقر 10/10
        /// </summary>
        public FrmUsers()
        {
            // دالة النظام الأساسية لبناء ورسم عناصر الواجهة المعرفة في الـ Designer
            InitializeComponent();
            ApplyApprovedUsersLayout();

            // توحيد شكل الشاشة القديمة والاختصارات العربية دون تغيير منطقها.
// --- ربط الأحداث الأساسية للشاشة والأزرار مع إلغاء الاشتراك أولاً لمنع التكرار ---
            this.Load -= FrmUsers_Load;
            this.Load += FrmUsers_Load;

            this.btnSave.Click -= btnSave_Click;
            this.btnSave.Click += btnSave_Click;

            this.dgvUsers.SelectionChanged -= dgvUsers_SelectionChanged;
            this.dgvUsers.SelectionChanged += dgvUsers_SelectionChanged;

            this.cmbRole.SelectedIndexChanged -= cmbRole_SelectedIndexChanged;
            this.cmbRole.SelectedIndexChanged += cmbRole_SelectedIndexChanged;

            // ربط حدث كومبو بوكس البحث في الصلاحيات الجديد والمحمى للفحص الآمن
            if (this.cmbPermissionSearch != null)
            {
                this.cmbPermissionSearch.SelectedIndexChanged -= cmbPermissionSearch_SelectedIndexChanged;
                this.cmbPermissionSearch.SelectedIndexChanged += cmbPermissionSearch_SelectedIndexChanged;
            }

            // ربط حدث مربع اختيار (تحديد الكل) الجديد لمنع تضارب الرسوم
            if (this.chkSelectAll != null)
            {
                this.chkSelectAll.CheckedChanged -= chkSelectAll_CheckedChanged;
                this.chkSelectAll.CheckedChanged += chkSelectAll_CheckedChanged;
            }

            // ربط أزرار التحكم القياسية بالشاشة بعد التوثق من وجودها بالـ Designer
            if (this.btnEdit != null) this.btnEdit.Click += btnEdit_Click;
            if (this.btnNew != null) this.btnNew.Click += btnNew_Click;
            if (this.btnDelete != null) this.btnDelete.Click += btnDelete_Click;
            if (this.btnClose != null) this.btnClose.Click += btnClose_Click;
            if (this.btnRefresh != null) this.btnRefresh.Click += btnRefresh_Click;
            if (this.btnSearch != null) this.btnSearch.Click += btnSearch_Click;
        }

        /// <summary>
        /// حدث تحميل الشاشة لأول مرة (Load Event) - تهيئة البيانات واستدعاء الـ API
        /// </summary>
        private async void FrmUsers_Load(object sender, EventArgs e)
        {
            // تهيئة أعمدة جدول المستخدمين السفلي
            SetupUsersGrid();

            // تثبيت ألوان جدول صلاحيات الوظائف برمجياً لضمان ثبات الواجهة
            FixFunctionPermissionsGridStyle();

            // تعبئة خيارات كومبو بوكس الحالة وتحديد الخيار الافتراضي
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("نشط");
            cmbStatus.Items.Add("موقوف");
            cmbStatus.Text = "نشط";

            chkIsActive.Checked = true;
            chkChangePassword.Checked = true;

            // استدعاء دوال جلب البيانات الأساسية من الـ API بشكل متزامن ومرتب هندسياً
            await LoadBranchesAsync();
            InitializeApprovedUserFilters();
            await LoadRolesAsync();
            await LoadUsersAsync();
            await LoadFunctionPermissionsAsync();

            // ✨ التعديل والفلترة حسب طلبك: تم الإبقاء فقط على دالة جلب صلاحيات البيانات الحقيقية لجدول dgvDataPermissions
            await LoadDataPermissionsAsync();

            // بناء وفلترة كومبو بوكس البحث العلوي وتنظيف الفورم بالكامل
            SetupSearchAndFilters();
            ClearForm();
        }

        /// <summary>
        /// دالة بناء وتسمية أعمدة جدول عرض المستخدمين الرئيسي برمجياً
        /// </summary>
        private void SetupUsersGrid()
        {
            dgvUsers.Columns.Clear();
            dgvUsers.AllowUserToAddRows = false;
            dgvUsers.ReadOnly = true;
            dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvUsers.Columns.Add("User_ID", "رقم المستخدم");
            dgvUsers.Columns.Add("Login_Name", "اسم المستخدم");
            dgvUsers.Columns.Add("Full_Name", "الاسم الكامل");
            dgvUsers.Columns.Add("Role_Name", "الدور");
            dgvUsers.Columns.Add("Company_ID", "الشركة");
            dgvUsers.Columns.Add("Branch_ID", "الفرع");
            dgvUsers.Columns.Add("Is_Active", "الحالة");
            dgvUsers.Columns.Add("Last_Login_At", "آخر دخول");
            dgvUsers.Columns.Add("Locked_Until", "مقفل");
        }

        /// <summary>
        /// دالة تثبيت ثيم وألوان جدول صلاحيات الوظائف لمنع اختفاء النصوص أو تأثرها بالويندوز
        /// </summary>
        private void FixFunctionPermissionsGridStyle()
        {
            dgvFunctionPermissions.DefaultCellStyle.ForeColor = Color.Black;
            dgvFunctionPermissions.DefaultCellStyle.BackColor = Color.White;
            dgvFunctionPermissions.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvFunctionPermissions.DefaultCellStyle.SelectionBackColor = Color.RoyalBlue;

            dgvFunctionPermissions.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvFunctionPermissions.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;
            dgvFunctionPermissions.EnableHeadersVisualStyles = false;
        }

        /// <summary>
        /// المحرك العام لجدول صلاحيات الوظائف (قراءة كاش الشاشات وزرع الأسطر)
        /// </summary>
        private async Task FillPermissionGridAsync(DataGridView grid, List<ScreenPermissionModel> data)
        {
            grid.SuspendLayout();
            try
            {
                grid.Rows.Clear();
                int no = 1;
                foreach (var item in data)
                {
                    int row = grid.Rows.Add();
                    grid.Rows[row].Cells[0].Value = no++;
                    grid.Rows[row].Cells[1].Value = item.Screen_Name;
                    for (int i = 2; i <= 7; i++) grid.Rows[row].Cells[i].Value = false;
                    grid.Rows[row].Tag = item.Screen_ID;
                }
            }
            finally { grid.ResumeLayout(); }
            await Task.CompletedTask;
        }

        /// <summary>
        /// المحرك العام والموحد لجدول صلاحيات البيانات (قراءة نموذج الـ SystemPermissionModel الجديد)
        /// </summary>
        private async Task FillSystemPermissionGridAsync(DataGridView grid, List<SystemPermissionModel> data)
        {
            grid.SuspendLayout();
            try
            {
                grid.Rows.Clear();
                int no = 1;
                foreach (var item in data)
                {
                    int rowIndex = grid.Rows.Add();
                    var row = grid.Rows[rowIndex];
                    row.Cells[0].Value = no++;
                    row.Cells[1].Value = item.Permission_Name;
                    for (int i = 2; i < row.Cells.Count; i++) row.Cells[i].Value = false;
                    row.Tag = item.Permission_ID;
                }
            }
            finally { grid.ResumeLayout(); }
            await Task.CompletedTask;
        }

        /// <summary>
        /// دالة جلب قائمة الشاشات من الـ API وتعبئة جدول التبويب الأول (صلاحيات الوظائف)
        /// </summary>
        private async Task LoadFunctionPermissionsAsync()
        {
            try
            {
                var screens = await _client.GetFromJsonAsync<List<ScreenPermissionModel>>($"{_baseUrl}RolePermissions/GetScreens");
                _screensCache = screens ?? new();
                await FillPermissionGridAsync(dgvFunctionPermissions, _screensCache);
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تحميل شاشات الصلاحيات:\n" + ex.Message, "خطأ بالتحميل", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// دالة جلب صلاحيات البيانات (نوع Data) من الـ API وتعبئة جدول dgvDataPermissions الموجود حقيقة بالـ Designer
        /// </summary>
        private async Task LoadDataPermissionsAsync()
        {
            try
            {
                var data = await _client.GetFromJsonAsync<List<SystemPermissionModel>>($"{_baseUrl}SystemPermissions/GetByType/Data");
                await FillSystemPermissionGridAsync(dgvDataPermissions, data ?? new List<SystemPermissionModel>());
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء استدعاء صلاحيات البيانات:\n" + ex.Message, "تنبيه الشبكة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// دالة ربط وتعبئة كومبو بوكس البحث العلوية بكاش الشاشات بحماية حارس الأحداث
        /// </summary>
        private void SetupSearchAndFilters()
        {
            if (cmbPermissionSearch == null) return;
            _isBinding = true;
            try
            {
                cmbPermissionSearch.DataSource = _screensCache.ToList();
                cmbPermissionSearch.DisplayMember = "Screen_Name";
                cmbPermissionSearch.ValueMember = "Screen_ID";
                cmbPermissionSearch.SelectedIndex = -1;
            }
            catch (Exception) { }
            finally { _isBinding = false; }
        }

        /// <summary>
        /// 🔍 حدث البحث والفلترة الذكية داخل جدول الصلاحيات برمجياً لإخفاء وإظهار الأسطر حسب الكلمة المكتوبة
        /// </summary>
        private void cmbPermissionSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isBinding) return;
            string search = cmbPermissionSearch.Text.Trim();
            dgvFunctionPermissions.SuspendLayout();
            try
            {
                foreach (DataGridViewRow row in dgvFunctionPermissions.Rows)
                {
                    if (row.Cells[1].Value == null) continue;
                    if (string.IsNullOrEmpty(search)) row.Visible = true;
                    else row.Visible = row.Cells[1].Value.ToString().Contains(search, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception) { }
            finally { dgvFunctionPermissions.ResumeLayout(); }
        }

        /// <summary>
        /// 🏁 حدث اختيار أو إلغاء تحديد الكل لكافة خانات جدول الصلاحيات الستة دفعة واحدة
        /// </summary>
        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSelectAll == null) return;
            foreach (DataGridViewRow row in dgvFunctionPermissions.Rows)
            {
                for (int i = 2; i <= 7; i++) row.Cells[i].Value = chkSelectAll.Checked;
            }
        }

        /// <summary>
        /// حدث إطلاق جلب البيانات عند قيام المستخدم بتغيير الدور (Role) من الواجهة
        /// </summary>
        private async void cmbRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isBinding) return;
            if (cmbRole.SelectedValue == null) return;
            if (!int.TryParse(cmbRole.SelectedValue.ToString(), out int roleId)) return;
            await LoadRolePermissionsAsync(roleId);
        }

        /// <summary>
        /// دالة سحب الصلاحيات المخزنة للدور المحدد وعكس خانات الصح والخطأ بالجدول تلقائياً
        /// </summary>
        private async Task LoadRolePermissionsAsync(int roleId)
        {
            try
            {
                var permissions = await _client.GetFromJsonAsync<List<RolePermissionModel>>($"{_baseUrl}RolePermissions/GetRolePermissions/{roleId}");
                _rolePermissionsCache = permissions ?? new();
                dgvFunctionPermissions.SuspendLayout();
                foreach (DataGridViewRow row in dgvFunctionPermissions.Rows)
                {
                    for (int i = 2; i <= 7; i++) row.Cells[i].Value = false;
                }
                foreach (DataGridViewRow row in dgvFunctionPermissions.Rows)
                {
                    if (row.Tag == null) continue;
                    int screenId = Convert.ToInt32(row.Tag);
                    var permission = _rolePermissionsCache.FirstOrDefault(x => x.Screen_ID == screenId);
                    if (permission == null) continue;
                    row.Cells[2].Value = permission.Can_View;
                    row.Cells[3].Value = permission.Can_Add;
                    row.Cells[4].Value = permission.Can_Edit;
                    row.Cells[5].Value = permission.Can_Delete;
                    row.Cells[6].Value = permission.Can_Print;
                    row.Cells[7].Value = permission.Can_Approve;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء تحميل الصلاحيات:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { dgvFunctionPermissions.ResumeLayout(); }
        }

        /// <summary>
        /// دالة تجميع وقراءة أسطر جدول الصلاحيات وإرسالها كـ قائمة List متكاملة إلى الـ API لحفظها
        /// </summary>
        private async Task SaveFunctionPermissionsAsync()
        {
            try
            {
                if (cmbRole.SelectedValue == null) return;
                int roleId = Convert.ToInt32(cmbRole.SelectedValue);
                List<RolePermissionModel> permissions = new();
                foreach (DataGridViewRow row in dgvFunctionPermissions.Rows)
                {
                    if (row.Tag == null) continue;
                    permissions.Add(new RolePermissionModel
                    {
                        Role_ID = roleId,
                        Screen_ID = Convert.ToInt32(row.Tag),
                        Can_View = Convert.ToBoolean(row.Cells[2].Value ?? false),
                        Can_Add = Convert.ToBoolean(row.Cells[3].Value ?? false),
                        Can_Edit = Convert.ToBoolean(row.Cells[4].Value ?? false),
                        Can_Delete = Convert.ToBoolean(row.Cells[5].Value ?? false),
                        Can_Print = Convert.ToBoolean(row.Cells[6].Value ?? false),
                        Can_Approve = Convert.ToBoolean(row.Cells[7].Value ?? false)
                    });
                }
                var response = await _client.PostAsJsonAsync($"{_baseUrl}RolePermissions/SaveRolePermissions", permissions);
                if (!response.IsSuccessStatusCode) MessageBox.Show(await response.Content.ReadAsStringAsync(), "خطأ حفظ الصلاحيات", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ تجميع وحفظ الصلاحيات:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// دالة سحب قائمة المستخدمين وتمريرها للمحرك الرئيسي
        /// </summary>
        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _client.GetFromJsonAsync<List<UserListModel>>($"{_baseUrl}Users");
                _usersCache = users ?? new();
                DisplayUsersInGrid(_usersCache);
            }
            catch (Exception ex) { MessageBox.Show("فشل جلب قائمة المستخدمين:\n" + ex.Message, "خطأ بالشبكة", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        /// <summary>
        /// دالة تفريغ وتعبئة الجدول الرئيسي للمستخدمين في أسفل الشاشة
        /// </summary>
        private void DisplayUsersInGrid(List<UserListModel> usersList)
        {
            _isBinding = true;
            try
            {
                dgvUsers.Rows.Clear();
                foreach (var user in usersList)
                {
                    dgvUsers.Rows.Add(
                        user.User_ID,
                        user.Login_Name,
                        user.Full_Name,
                        user.Role_Name,
                        user.Company_ID,
                        user.Branch_ID,
                        user.Is_Active ? "نشط" : "موقوف",
                        user.Last_Login_At?.ToLocalTime().ToString("yyyy/MM/dd HH:mm") ?? "—",
                        user.Locked_Until is { } locked && locked > DateTime.UtcNow ? "نعم" : "لا");
                }
            }
            finally { _isBinding = false; }
        }
        // هذا كود الصلاحيات عامة 
     //   private async Task LoadBranchesAsync()
      //  {
     //       try
     //       {
     //           var branches = await _client.GetFromJsonAsync<List<BranchLookupModel>>($"{_baseUrl}Users/GetBranchesLookup");
     //           cmbBranch.DataSource = branches; cmbBranch.DisplayMember = "Branch_Name"; cmbBranch.ValueMember = "Branch_ID"; cmbBranch.SelectedIndex = -1;
     //       }
      //      catch (Exception ex) { MessageBox.Show(ex.Message); }
      //  }


        // هذا كود صلاحيات الجلسة
        private async Task LoadBranchesAsync()
        {
            try
            {
                var branches = await _client.GetFromJsonAsync<List<BranchLookupModel>>(
                    $"{_baseUrl}Users/GetBranchesLookup?companyId={CurrentSession.Company_ID}");

                cmbBranch.DataSource = branches ?? new List<BranchLookupModel>();
                cmbBranch.DisplayMember = "Branch_Name";
                cmbBranch.ValueMember = "Branch_ID";
                cmbBranch.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تحميل الفروع:\n" + ex.Message);
            }
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _client.GetFromJsonAsync<List<RoleLookupModel>>($"{_baseUrl}Users/GetRolesLookup");
                _isBinding = true;
                cmbRole.DataSource = roles; cmbRole.DisplayMember = "Role_Name"; cmbRole.ValueMember = "Role_ID"; cmbRole.SelectedIndex = -1;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { _isBinding = false; }
        }

        /// <summary>
        /// ➕ حدث حفظ مستخدم جديد واستدعاء الصلاحيات المحدثة لحفظها معاً فوراً بالتزامن
        /// </summary>
        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs(isInputsForUpdate: false)) return;
            var request = BuildUserRequestObject(isUpdate: false);
            var response = await _client.PostAsJsonAsync($"{_baseUrl}Users", request);
            if (response.IsSuccessStatusCode)
            {
                await SaveFunctionPermissionsAsync();
                MessageBox.Show("تم حفظ المستخدم بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadUsersAsync();
                ClearForm();
            }
            else MessageBox.Show(await response.Content.ReadAsStringAsync(), "فشل الحفظ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private async void btnEdit_Click(object sender, EventArgs e) { await ExecuteUpdateAsync(); }
        private void btnNew_Click(object sender, EventArgs e) { ClearForm(); }
        private async void btnDelete_Click(object sender, EventArgs e) { await ExecuteDeleteAsync(); }
        private void btnClose_Click(object sender, EventArgs e) { this.Close(); }
        private async void btnRefresh_Click(object sender, EventArgs e) { await LoadUsersAsync(); ClearForm(); }

        /// <summary>
        /// 🔍 تفعيل ميزة البحث والفلترة السريعة لجدول المستخدمين داخل الذاكرة كاش لتسريع الأداء وحماية السيرفر
        /// </summary>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            // مربع البحث يعتمد اسم الدخول، وهو الحقل الموحد الموجود في ملف التصميم.
            string searchText = txtLoginName.Text.Trim();
            if (string.IsNullOrEmpty(searchText)) DisplayUsersInGrid(_usersCache);
            else
            {
                var filtered = _usersCache.Where(u => u.Full_Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) || u.Login_Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
                DisplayUsersInGrid(filtered);
            }
        }

        /// <summary>
        /// 🛠️ دالة تنفيذ التعديل وإرسال بيانات الحساب الأساسية والصلاحيات المحدثة فوراً للسيرفر
        /// </summary>
        private async Task ExecuteUpdateAsync()
        {
            if (_selectedUserId == 0) { MessageBox.Show("يرجى اختيار مستخدم للتعديل."); return; }
            if (!ValidateInputs(isInputsForUpdate: true)) return;
            var request = BuildUserRequestObject(isUpdate: true);
            var response = await _client.PutAsJsonAsync($"{_baseUrl}Users/{_selectedUserId}", request);
            if (response.IsSuccessStatusCode)
            {
                await SaveFunctionPermissionsAsync();
                MessageBox.Show("تم التعديل بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadUsersAsync();
                ClearForm();
            }
            else MessageBox.Show(await response.Content.ReadAsStringAsync(), "فشل التعديل", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// ❌ دالة حذف مستخدم نهائياً بعد التأكيد الأمني لحماية النظام من الحذف العشوائي
        /// </summary>
        private async Task ExecuteDeleteAsync()
        {
            if (_selectedUserId == 0) { MessageBox.Show("يرجى اختيار مستخدم أولاً."); return; }
            var dialogResult = MessageBox.Show("هل أنت متأكد من الحذف؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                var response = await _client.DeleteAsync($"{_baseUrl}Users/{_selectedUserId}");
                if (response.IsSuccessStatusCode) { MessageBox.Show("تم الحذف بنجاح."); await LoadUsersAsync(); ClearForm(); }
                else MessageBox.Show(await response.Content.ReadAsStringAsync(), "فشل الحذف", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// حدث النقر بالماوس واختيار سطر مستخدم من الجدول السفلي لتعبئة بياناته وإضاءة صلاحياته الحقيقية
        /// </summary>
        private async void dgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (_isBinding) return;
            if (dgvUsers.SelectedRows.Count > 0)
            {
                var currentRow = dgvUsers.SelectedRows[0];
                if (currentRow.Cells["User_ID"].Value != null)
                {
                    _selectedUserId = Convert.ToInt32(currentRow.Cells["User_ID"].Value);
                    await GetUserDetailsAsync(_selectedUserId);
                }
            }
        }

        /// <summary>
        /// دالة جلب تفاصيل حساب الموظف وفك التشفير وعكس الصلاحيات صراحة عند حدث النقر
        /// </summary>
        private async Task GetUserDetailsAsync(int userId)
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}Users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var user = System.Text.Json.JsonSerializer.Deserialize<UserDetailModel>(jsonString, options);
                    if (user != null)
                    {
                        _isBinding = true; // قفل لمنع التفاف أو تكرار استدعاء الكومبوهات أثناء التعبئة
                        txtFullName.Text = user.Full_Name; txtLoginName.Text = user.Login_Name; txtPhone.Text = user.Phone; txtEmail.Text = user.Email; txtNotes.Text = user.Notes; cmbBranch.SelectedValue = user.Branch_ID; cmbRole.SelectedValue = user.Role_ID; cmbStatus.Text = user.Is_Active ? "نشط" : "موقوف"; chkIsActive.Checked = user.Is_Active; chkChangePassword.Checked = user.Must_Change_Password; txtPassword.Text = string.Empty; txtConfirmPassword.Text = string.Empty;
                        _isBinding = false;

                        // 👈 الاستدعاء الصريح المباشر لتأشير علامات الصلاحيات فور النقر
                        await LoadRolePermissionsAsync(user.Role_ID);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { _isBinding = false; }
        }

        private bool ValidateInputs(bool isInputsForUpdate)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text)) { MessageBox.Show("يرجى إدخال الاسم."); return false; }
            if (string.IsNullOrWhiteSpace(txtLoginName.Text)) { MessageBox.Show("يرجى إدخال اسم الدخول."); return false; }
            bool passwordProvided = !string.IsNullOrWhiteSpace(txtPassword.Text) || !string.IsNullOrWhiteSpace(txtConfirmPassword.Text);
            if (!isInputsForUpdate && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("يرجى إدخال كلمة المرور.");
                return false;
            }
            if (passwordProvided && txtPassword.Text != txtConfirmPassword.Text)
            {
                MessageBox.Show("كلمتا المرور غير متطابقتين.");
                return false;
            }
            if (cmbBranch.SelectedValue == null || cmbRole.SelectedValue == null) { MessageBox.Show("يرجى اختيار الفرع والدور."); return false; }
            return true;
        }

        private object BuildUserRequestObject(bool isUpdate)
        {
            return new { Company_ID = CurrentSession.Company_ID, Branch_ID = Convert.ToInt32(cmbBranch.SelectedValue), Role_ID = Convert.ToInt32(cmbRole.SelectedValue), User_Code = string.Empty, Full_Name = txtFullName.Text.Trim(), Login_Name = txtLoginName.Text.Trim(), Password = txtPassword.Text.Trim(), Phone = txtPhone.Text.Trim(), Email = txtEmail.Text.Trim(), Notes = txtNotes.Text.Trim(), Must_Change_Password = chkChangePassword.Checked, Is_Active = cmbStatus.Text == "نشط" };
        }

        /// <summary>
        /// يطبّق التصميم العربي الموحد للشاشة ويمنع قص الحقول عند تغيير مقاس النافذة.
        /// </summary>
        private void ApplyApprovedUsersLayout()
        {
            SuspendLayout();
            try
            {
                RightToLeft = RightToLeft.Yes;
                RightToLeftLayout = true;
                Font = new Font("Tahoma", 10F, FontStyle.Regular);
                MinimumSize = new Size(1150, 720);
                StartPosition = FormStartPosition.CenterScreen;
                BackColor = Color.FromArgb(245, 247, 250);

                pnlToolbar.Height = 74;
                pnlToolbar.Padding = new Padding(12, 12, 12, 10);
                pnlToolbar.BackColor = Color.White;
                pnlToolbar.BorderStyle = BorderStyle.FixedSingle;

                ArrangeToolbarButtons();
                pnlToolbar.Resize -= pnlToolbar_Resize;
                pnlToolbar.Resize += pnlToolbar_Resize;

                pnlGrid.Height = 250;
                pnlGrid.Padding = new Padding(10, 8, 10, 10);
                pnlGrid.BackColor = Color.White;
                pnlGrid.BorderStyle = BorderStyle.None;

                dgvUsers.RightToLeft = RightToLeft.Yes;
                dgvUsers.BackgroundColor = Color.White;
                dgvUsers.BorderStyle = BorderStyle.None;
                dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvUsers.RowTemplate.Height = 32;
                dgvUsers.ColumnHeadersHeight = 36;
                dgvUsers.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 64, 112);
                dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvUsers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
                dgvUsers.DefaultCellStyle.SelectionForeColor = Color.FromArgb(17, 24, 39);
                dgvUsers.EnableHeadersVisualStyles = false;

                grpUserData.Width = 500;
                grpUserData.Padding = new Padding(10, 12, 10, 10);
                grpUserData.BackColor = Color.FromArgb(245, 247, 250);
                grpUserData.ForeColor = Color.FromArgb(30, 64, 112);
                grpUserData.Font = new Font("Tahoma", 11F, FontStyle.Bold);

                panel1.BackColor = Color.White;
                panel1.Padding = new Padding(16, 12, 16, 12);
                panel1.Font = new Font("Tahoma", 10F);
                ArrangeUserFields();

                grpPermissions.Padding = new Padding(10, 12, 10, 10);
                grpPermissions.BackColor = Color.FromArgb(245, 247, 250);
                grpPermissions.ForeColor = Color.FromArgb(30, 64, 112);
                grpPermissions.Font = new Font("Tahoma", 11F, FontStyle.Bold);
                pnlPermissionHeader.Height = 94;
                pnlPermissionHeader.BackColor = Color.White;
                pnlPermissionHeader.Padding = new Padding(12, 10, 12, 10);
                ArrangePermissionFilters();
                pnlPermissionHeader.Resize -= pnlPermissionHeader_Resize;
                pnlPermissionHeader.Resize += pnlPermissionHeader_Resize;

                foreach (var grid in new[] { dgvFunctionPermissions, dgvDataPermissions, dataGridView1, dataGridView2 })
                {
                    grid.RightToLeft = RightToLeft.Yes;
                    grid.RowTemplate.Height = 30;
                    grid.ColumnHeadersHeight = 34;
                    grid.EnableHeadersVisualStyles = false;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 64, 112);
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void ArrangeToolbarButtons()
        {
            var buttons = new[] { btnNew, btnSave, btnEdit, btnDelete, btnPrint, btnSearch, btnRefresh, btnClose };
            int x = pnlToolbar.ClientSize.Width - 12;
            foreach (var button in buttons)
            {
                button.Size = new Size(102, 42);
                button.Location = new Point(x - button.Width, 14);
                button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                button.RightToLeft = RightToLeft.Yes;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = Color.FromArgb(191, 219, 254);
                button.BackColor = button == btnSave
                    ? Color.FromArgb(30, 64, 112)
                    : button == btnClose ? Color.FromArgb(254, 242, 242) : Color.FromArgb(239, 246, 255);
                button.ForeColor = button == btnSave ? Color.White : Color.FromArgb(30, 64, 112);
                x -= button.Width + 8;
            }
        }

        private void ArrangeUserFields()
        {
            int labelRight = panel1.ClientSize.Width - 22;
            int fieldWidth = Math.Max(230, panel1.ClientSize.Width - 190);
            int fieldLeft = 20;
            int y = 12;

            ArrangeField(label11, txtFullName, labelRight, fieldLeft, fieldWidth, ref y);
            ArrangeField(label1, txtUserName, labelRight, fieldLeft, fieldWidth, ref y);
            ArrangeField(label2, txtLoginName, labelRight, fieldLeft, fieldWidth, ref y);
            ArrangeField(label3, txtPassword, labelRight, fieldLeft, fieldWidth, ref y);
            ArrangeField(label4, txtConfirmPassword, labelRight, fieldLeft, fieldWidth, ref y);
            ArrangeField(label7, cmbBranch, labelRight, fieldLeft, fieldWidth, ref y);
            ArrangeField(label6, cmbRole, labelRight, fieldLeft, fieldWidth, ref y);
            ArrangeField(label5, cmbStatus, labelRight, fieldLeft, fieldWidth, ref y);
            ArrangeField(label10, txtPhone, labelRight, fieldLeft, fieldWidth, ref y);
            ArrangeField(label9, txtEmail, labelRight, fieldLeft, fieldWidth, ref y);
            ArrangeField(label8, txtNotes, labelRight, fieldLeft, fieldWidth, ref y);

            chkChangePassword.Location = new Point(fieldLeft, y + 2);
            chkChangePassword.AutoSize = true;
            chkIsActive.Location = new Point(fieldLeft + 245, y + 2);
            chkIsActive.AutoSize = true;
        }

        private static void ArrangeField(Label label, Control input, int labelRight, int fieldLeft, int fieldWidth, ref int y)
        {
            label.AutoSize = false;
            label.TextAlign = ContentAlignment.MiddleRight;
            label.Location = new Point(labelRight - 140, y + 4);
            label.Size = new Size(140, 28);
            input.Location = new Point(fieldLeft, y);
            input.Size = new Size(fieldWidth, 32);
            input.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            y += 38;
        }

        private void ArrangePermissionFilters()
        {
            int x = pnlPermissionHeader.ClientSize.Width - 12;
            ArrangePermissionFilter(label14, cmbPermissionSearch, ref x);
            ArrangePermissionFilter(label12, cmbPermissionRole, ref x);
            ArrangePermissionFilter(label13, cmbPermissionModule, ref x);
            ArrangePermissionFilter(label15, cmbPermissionScreen, ref x);
            ArrangePermissionFilter(label16, cmbPermissionType, ref x);
            chkSelectAll.Location = new Point(18, 56);
            chkSelectAll.AutoSize = true;
        }

        private static void ArrangePermissionFilter(Label label, ComboBox combo, ref int x)
        {
            const int width = 155;
            x -= width;
            combo.Location = new Point(x, 42);
            combo.Size = new Size(width, 31);
            combo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label.AutoSize = false;
            label.TextAlign = ContentAlignment.MiddleRight;
            label.Location = new Point(x, 12);
            label.Size = new Size(width, 24);
            label.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            x -= 10;
        }

        private void pnlToolbar_Resize(object? sender, EventArgs e) => ArrangeToolbarButtons();
        private void pnlPermissionHeader_Resize(object? sender, EventArgs e) => ArrangePermissionFilters();

        private void ClearForm()
        {
            _isBinding = true;
            try
            {
                _selectedUserId = 0; txtFullName.Text = string.Empty; txtLoginName.Text = string.Empty; txtPassword.Text = string.Empty; txtConfirmPassword.Text = string.Empty; txtPhone.Text = string.Empty; txtEmail.Text = string.Empty; txtNotes.Text = string.Empty; cmbBranch.SelectedIndex = -1; cmbRole.SelectedIndex = -1; cmbStatus.Text = "نشط"; chkIsActive.Checked = true; chkChangePassword.Checked = true;
                if (dgvUsers.SelectedRows.Count > 0) dgvUsers.ClearSelection();
                foreach (DataGridViewRow row in dgvFunctionPermissions.Rows) { for (int i = 2; i <= 7; i++) row.Cells[i].Value = false; }
                if (cmbPermissionSearch != null) cmbPermissionSearch.SelectedIndex = -1;
                if (chkSelectAll != null) chkSelectAll.Checked = false;
                txtFullName.Focus();
            }
            finally { _isBinding = false; }
        }

        // 🛡️ أحداث احتياطية فارغة لإرضاء ملف الـ Designer والتخلص من خطأ الـ Build تماماً
        private void grpUserData_Enter(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void tabPage1_Click(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void dgvDataPermissions_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }

    // --- الـ Models المستقرة النظيفة الموقعة داخل الـ Namespace لحل أخطاء التجميع ---

    // ✨ الموديل المشترك والموحد الجديد لقراءة صلاحيات جدول system_permissions حسب النوع لـ .NET 10
    public class SystemPermissionModel
    {
        public int Permission_ID { get; set; }
        public string Permission_Code { get; set; } = string.Empty;
        public string Permission_Name { get; set; } = string.Empty;
        public string Permission_Type { get; set; } = string.Empty;
        public string? Module_Name { get; set; }
    }

    public class UserListModel
    {
        public int User_ID { get; set; }
        public string User_Code { get; set; } = string.Empty;
        public string Company_ID { get; set; } = string.Empty;
        public int Branch_ID { get; set; }
        public int Role_ID { get; set; }
        public string Role_Name { get; set; } = string.Empty;
        public string Full_Name { get; set; } = string.Empty;
        public string Login_Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool Is_Active { get; set; }
        public DateTime? Last_Login_At { get; set; }
        public DateTime? Locked_Until { get; set; }
    }
    public class UserDetailModel { public int User_ID { get; set; } public string Company_ID { get; set; } = string.Empty; public int Branch_ID { get; set; } public int Role_ID { get; set; } public string User_Code { get; set; } = string.Empty; public string Full_Name { get; set; } = string.Empty; public string Login_Name { get; set; } = string.Empty; public string? Phone { get; set; } public string? Email { get; set; } public string? Notes { get; set; } public bool Must_Change_Password { get; set; } public bool Is_Active { get; set; } }
    public class BranchLookupModel { public int Branch_ID { get; set; } public string Branch_Name { get; set; } = string.Empty; }
    public class RoleLookupModel { public int Role_ID { get; set; } public string Role_Name { get; set; } = string.Empty; }
    public class ScreenPermissionModel { public int Screen_ID { get; set; } public string Screen_Code { get; set; } = string.Empty; public string Screen_Name { get; set; } = string.Empty; public string Module_Name { get; set; } = string.Empty; }
    public class RolePermissionModel { public int Permission_ID { get; set; } public int Role_ID { get; set; } public int Screen_ID { get; set; } public bool Can_View { get; set; } public bool Can_Add { get; set; } public bool Can_Edit { get; set; } public bool Can_Delete { get; set; } public bool Can_Print { get; set; } public bool Can_Approve { get; set; } }
}