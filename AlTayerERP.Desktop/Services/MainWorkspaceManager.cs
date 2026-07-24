using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// مدير تبويبات مساحة العمل في FrmMain. يمنع فتح نفس الشاشة أو نفس السجل
    /// مرتين، ويجعل جميع النوافذ الفرعية داخل pnlWorkspace.
    /// </summary>
    public sealed class MainWorkspaceManager : IDisposable
    {
        private const string HomeKey = "__HOME__";
        private readonly TabControl _tabs;
        private readonly Dictionary<string, TabPage> _pages =
            new(StringComparer.OrdinalIgnoreCase);

        public MainWorkspaceManager(Panel host)
        {
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Alignment = TabAlignment.Top,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                DrawMode = TabDrawMode.Normal,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Padding = new Point(18, 6)
            };

            // زر الإغلاق متاح من القائمة السياقية؛ تبويب الرئيسية لا يغلق.
            var menu = new ContextMenuStrip();
            var closeItem = new ToolStripMenuItem("إغلاق التبويب الحالي");
            closeItem.Click += (_, _) => CloseActivePage();
            menu.Items.Add(closeItem);
            _tabs.ContextMenuStrip = menu;
            _tabs.MouseUp += (_, args) =>
            {
                if (args.Button != MouseButtons.Right)
                    return;

                for (var index = 0; index < _tabs.TabCount; index++)
                {
                    if (_tabs.GetTabRect(index).Contains(args.Location))
                    {
                        _tabs.SelectedIndex = index;
                        break;
                    }
                }
            };

            host.Controls.Clear();
            host.Controls.Add(_tabs);
        }

        /// <summary>يعرض لوحة الملخص الثابتة ولا يسمح بإغلاقها.</summary>
        public void ShowHome(Control dashboard)
        {
            SetHome(dashboard, activate: true);
        }

        /// <summary>يحدث محتوى لوحة الملخص بعد تحميل الصلاحيات، دون إغلاق بقية التبويبات.</summary>
        public void SetHome(Control dashboard, bool activate = false)
        {
            if (!_pages.TryGetValue(HomeKey, out var page))
            {
                page = CreatePage(HomeKey, "الرئيسية");
                _tabs.TabPages.Insert(0, page);
                _pages[HomeKey] = page;
            }

            foreach (Control control in page.Controls)
                control.Dispose();
            page.Controls.Clear();

            dashboard.Dock = DockStyle.Fill;
            page.Controls.Add(dashboard);
            if (activate)
                _tabs.SelectedTab = page;
        }

        /// <summary>
        /// يفتح الشاشة بمفتاح ثابت. استخدم recordKey عند فتح محررات سجلات مستقلة
        /// مستقبلاً؛ يمنع المفتاح نفسه من فتح نسخة ثانية.
        /// </summary>
        public bool Open(string screenCode, string caption, Func<Form> factory, string? recordKey = null)
        {
            var key = string.IsNullOrWhiteSpace(recordKey)
                ? screenCode
                : $"{screenCode}:{recordKey}";

            if (_pages.TryGetValue(key, out var existing))
            {
                _tabs.SelectedTab = existing;
                existing.Focus();
                return false;
            }

            var form = factory();
            var page = CreatePage(key, caption);
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;
            form.FormClosed += (_, _) => RemovePage(key, page);

            page.Controls.Add(form);
            _tabs.TabPages.Add(page);
            _pages[key] = page;
            _tabs.SelectedTab = page;
            form.Show();
            return true;
        }

        /// <summary>ينتقل إلى التبويب الموجود أو يعرض لوحة الملخص.</summary>
        public void ActivateHome() => ShowExistingOrHome();

        /// <summary>يحاول إغلاق التبويب النشط بعد سؤال الشاشة عن التعديلات غير المحفوظة.</summary>
        public bool CloseActivePage()
        {
            var page = _tabs.SelectedTab;
            if (page == null || string.Equals(page.Name, HomeKey, StringComparison.OrdinalIgnoreCase))
                return false;

            var form = FindWorkspaceForm(page);
            if (form is IWorkspaceDirtyAware dirtyAware &&
                dirtyAware.HasUnsavedChanges &&
                !dirtyAware.ConfirmWorkspaceClose())
            {
                return false;
            }

            form?.Close();
            if (_pages.ContainsKey(page.Name))
                RemovePage(page.Name, page);
            return true;
        }

        /// <summary>يغلق كل التبويبات بعد التحقق من كل شاشة قابلة للتعديل.</summary>
        public bool TryCloseAll()
        {
            var pages = new List<TabPage>(_pages.Values);
            foreach (var page in pages)
            {
                if (string.Equals(page.Name, HomeKey, StringComparison.OrdinalIgnoreCase))
                    continue;

                _tabs.SelectedTab = page;
                if (!CloseActivePage())
                    return false;
            }

            return true;
        }

        public void Dispose()
        {
            _tabs.Dispose();
            _pages.Clear();
        }

        private void ShowExistingOrHome()
        {
            if (_pages.TryGetValue(HomeKey, out var page))
                _tabs.SelectedTab = page;
        }

        private static Form? FindWorkspaceForm(TabPage page)
        {
            foreach (Control control in page.Controls)
            {
                if (control is Form form)
                    return form;
            }

            return null;
        }

        private static TabPage CreatePage(string key, string caption) =>
            new()
            {
                Name = key,
                Text = caption,
                BackColor = Color.FromArgb(249, 250, 252),
                Padding = new Padding(0)
            };

        private void RemovePage(string key, TabPage page)
        {
            _pages.Remove(key);
            _tabs.TabPages.Remove(page);
            page.Dispose();
            ShowExistingOrHome();
        }
    }
}