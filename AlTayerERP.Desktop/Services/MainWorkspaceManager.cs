using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// مدير تبويبات مساحة العمل في FrmMain. يمنع فتح نفس الشاشة أو نفس السجل
    /// مرتين، ويجعل جميع النوافذ الفرعية داخل pnlWorkspace وفق عقد عرض موحد.
    /// </summary>
    public sealed class MainWorkspaceManager : IDisposable
    {
        private const string HomeKey = "__HOME__";
        private readonly TabControl _tabs;
        private readonly Dictionary<string, TabPage> _pages =
            new(StringComparer.OrdinalIgnoreCase);

        public MainWorkspaceManager(Panel host)
        {
            host.SuspendLayout();
            host.AutoScroll = false;
            host.Padding = Padding.Empty;

            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Alignment = TabAlignment.Top,
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                DrawMode = TabDrawMode.Normal,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Padding = new Point(18, 6),
                Margin = Padding.Empty
            };

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
            host.ResumeLayout(true);
        }

        public void ShowHome(Control dashboard) => SetHome(dashboard, activate: true);

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

            dashboard.Margin = Padding.Empty;
            dashboard.Dock = DockStyle.Fill;
            page.Controls.Add(dashboard);
            if (activate)
                _tabs.SelectedTab = page;
        }

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
            PrepareHostedForm(form, page);

            form.FormClosed += (_, _) => RemovePage(key, page);
            page.Resize += (_, _) => RefreshHostedFormLayout(form, page);

            page.Controls.Add(form);
            _tabs.TabPages.Add(page);
            _pages[key] = page;
            _tabs.SelectedTab = page;

            form.Show();
            RefreshHostedFormLayout(form, page);
            return true;
        }

        public void ActivateHome() => ShowExistingOrHome();

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

        private static void PrepareHostedForm(Form form, TabPage page)
        {
            form.SuspendLayout();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.StartPosition = FormStartPosition.Manual;
            form.WindowState = FormWindowState.Normal;
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.AutoScroll = false;
            form.MinimumSize = Size.Empty;
            form.MaximumSize = Size.Empty;
            form.Margin = Padding.Empty;
            form.Padding = Padding.Empty;
            form.Dock = DockStyle.Fill;

            ScaleHostedFormToWorkspace(form, page);
            UnifiedScreenLayoutService.Apply(form);
            form.ResumeLayout(true);
        }

        /// <summary>
        /// يوحد حدود الشاشة المستضافة مع مساحة التبويب قبل تطبيق القالب الموحد.
        /// </summary>
        private static void ScaleHostedFormToWorkspace(Form form, TabPage page)
        {
            if (form.IsDisposed || page.IsDisposed)
                return;

            NormalizeRootControls(form);
            ApplyWorkspaceBounds(form, page);
        }

        private static void NormalizeRootControls(Form form)
        {
            var visibleRoots = form.Controls
                .Cast<Control>()
                .Where(control => control.Visible && control is not MenuStrip && control is not StatusStrip)
                .ToList();

            if (visibleRoots.Count == 1)
            {
                visibleRoots[0].Margin = Padding.Empty;
                visibleRoots[0].Dock = DockStyle.Fill;
                return;
            }

            foreach (var control in visibleRoots)
            {
                if (control.Dock == DockStyle.None && control.Anchor == AnchorStyles.Top &&
                    control.Width >= form.ClientSize.Width * 0.85 &&
                    control.Height >= form.ClientSize.Height * 0.75)
                {
                    control.Margin = Padding.Empty;
                    control.Dock = DockStyle.Fill;
                }
            }
        }

        private static void RefreshHostedFormLayout(Form form, TabPage page)
        {
            ApplyWorkspaceBounds(form, page);
            UnifiedScreenLayoutService.Apply(form);
        }

        private static void ApplyWorkspaceBounds(Form form, TabPage page)
        {
            if (form.IsDisposed || page.IsDisposed)
                return;

            form.MinimumSize = Size.Empty;
            form.MaximumSize = Size.Empty;
            form.Dock = DockStyle.Fill;
            form.Location = Point.Empty;

            if (page.ClientSize.Width > 0 && page.ClientSize.Height > 0)
                form.Bounds = page.ClientRectangle;

            form.PerformLayout();
            form.Invalidate(true);
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
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                AutoScroll = false
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
