using System;
using System.Collections.Generic;
using System.Linq;

namespace AlTayerERP.Desktop.Services
{
    /// <summary>
    /// يحتفظ ببيانات جلسة المستخدم الحالية والصلاحيات الفعلية التي أعادها الخادم.
    /// لا تعد صلاحيات الواجهة بديلاً عن تحقق الـ API.
    /// </summary>
    public static class CurrentSession
    {
        public static string Company_ID { get; set; } = "";
        public static string Company_Name { get; set; } = "";

        public static int Branch_ID { get; set; }
        public static string Branch_Name { get; set; } = "";

        public static int Year_ID { get; set; }
        public static string Year_Name { get; set; } = "";
        public static DateTime? FiscalYear_StartDate { get; set; }
        public static DateTime? FiscalYear_EndDate { get; set; }

        public static int User_ID { get; set; }
        public static int Role_ID { get; set; }
        public static string Username { get; set; } = "";
        public static string Full_Name { get; set; } = "";
        public static bool Is_System_Admin { get; set; }

        public static string Currency_Code { get; set; } = "YER";
        public static string Language { get; set; } = "AR";
        public static DateTime Login_Time { get; set; } = DateTime.Now;
        public static string Device_Name { get; } = Environment.MachineName;

        private static readonly Dictionary<string, ScreenPermissionState> _screenPermissions =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, bool> _resourcePermissions =
            new(StringComparer.OrdinalIgnoreCase);

        public static bool IsLoggedIn =>
            User_ID > 0 &&
            !string.IsNullOrWhiteSpace(Company_ID) &&
            Branch_ID > 0 &&
            Year_ID > 0;

        public static void SetScreenPermissions(IEnumerable<ScreenPermissionState>? permissions)
        {
            _screenPermissions.Clear();

            if (permissions == null)
                return;

            foreach (ScreenPermissionState permission in permissions)
            {
                if (!string.IsNullOrWhiteSpace(permission.Screen_Code))
                    _screenPermissions[permission.Screen_Code] = permission;
            }
        }

        public static void SetResourcePermissions(IEnumerable<ResourcePermissionState>? permissions)
        {
            _resourcePermissions.Clear();
            if (permissions == null)
                return;

            foreach (ResourcePermissionState permission in permissions)
            {
                if (!string.IsNullOrWhiteSpace(permission.Screen_Code) &&
                    !string.IsNullOrWhiteSpace(permission.Resource_Kind) &&
                    !string.IsNullOrWhiteSpace(permission.Resource_Code) &&
                    !string.IsNullOrWhiteSpace(permission.Permission_Code))
                {
                    _resourcePermissions[ResourceKey(
                        permission.Screen_Code,
                        permission.Resource_Kind,
                        permission.Resource_Code,
                        permission.Permission_Code)] = permission.Effect;
                }
            }
        }

        public static bool CanResourceExecute(
            string screenCode,
            string resourceKind,
            string resourceCode,
            string permissionCode)
        {
            return Is_System_Admin ||
                (_resourcePermissions.TryGetValue(
                    ResourceKey(screenCode, resourceKind, resourceCode, permissionCode),
                    out bool effect) && effect);
        }

        private static string ResourceKey(
            string screenCode,
            string resourceKind,
            string resourceCode,
            string permissionCode) =>
            screenCode + "|" + resourceKind + "|" + resourceCode + "|" + permissionCode;

        public static bool CanViewScreen(string screenCode) =>
            Is_System_Admin ||
            (_screenPermissions.TryGetValue(screenCode, out ScreenPermissionState? permission) &&
             permission.Can_View);

        public static bool CanExecute(string screenCode, string actionCode)
        {
            if (Is_System_Admin)
                return true;

            if (!_screenPermissions.TryGetValue(screenCode, out ScreenPermissionState? permission))
                return false;

            return actionCode.ToUpperInvariant() switch
            {
                "VIEW" => permission.Can_View,
                "ADD" => permission.Can_Add,
                "EDIT" => permission.Can_Edit,
                "DELETE" => permission.Can_Delete,
                "PRINT" => permission.Can_Print,
                "EXPORT" => permission.Can_Export,
                "IMPORT" => permission.Can_Import,
                "APPROVE" => permission.Can_Approve,
                "UNAPPROVE" => permission.Can_UnApprove,
                _ => false
            };
        }

        public static IReadOnlyCollection<ScreenPermissionState> GetScreenPermissions() =>
            _screenPermissions.Values.ToList().AsReadOnly();

        public static void Clear()
        {
            Company_ID = "";
            Company_Name = "";

            Branch_ID = 0;
            Branch_Name = "";

            Year_ID = 0;
            Year_Name = "";

            FiscalYear_StartDate = null;
            FiscalYear_EndDate = null;

            User_ID = 0;
            Role_ID = 0;

            Username = "";
            Full_Name = "";

            Currency_Code = "YER";
            Language = "AR";
            Is_System_Admin = false;
            Login_Time = DateTime.Now;

            _screenPermissions.Clear();
            _resourcePermissions.Clear();
        }

        public sealed class ResourcePermissionState
        {
            public string Screen_Code { get; set; } = "";
            public string Resource_Kind { get; set; } = "";
            public string Resource_Code { get; set; } = "";
            public string Permission_Code { get; set; } = "";
            public bool Effect { get; set; }
        }

        public sealed class ScreenPermissionState
        {
            public string Screen_Code { get; set; } = "";
            public bool Can_View { get; set; }
            public bool Can_Add { get; set; }
            public bool Can_Edit { get; set; }
            public bool Can_Delete { get; set; }
            public bool Can_Print { get; set; }
            public bool Can_Export { get; set; }
            public bool Can_Import { get; set; }
            public bool Can_Approve { get; set; }
            public bool Can_UnApprove { get; set; }
        }
    }
}