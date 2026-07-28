using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// محرك التفويض للخادم. السياسة Default Deny: غياب صلاحية الدور أو استثناء المستخدم
    /// يعني الرفض. مدير النظام يتجاوز صلاحيات الشاشات العادية فقط، بينما العمليات المالية
    /// الحساسة التي تستعمل IsExplicitlyAllowedAsync تبقى خاضعة لصلاحية صريحة.
    /// </summary>
    public sealed class ScreenAuthorizationService
    {
        private readonly AppDbContext _context;

        public ScreenAuthorizationService(AppDbContext context) => _context = context;

        public async Task<bool> IsAllowedAsync(
            ServerSession session,
            string screenCode,
            ScreenOperation operation,
            CancellationToken cancellationToken = default)
        {
            var normalizedScreen = screenCode.Trim();

            // مدير النظام يملك جميع عمليات الشاشات العادية. توجد دالة مستقلة
            // IsExplicitlyAllowedAsync للعمليات المالية الحساسة التي لا يجوز تجاوزها.
            if (session.Is_System_Admin)
                return true;

            // الاستثناء المباشر للمستخدم أعلى أولوية من الدور؛ الصف ذو القيم false
            // يستخدم كمنع صريح ولا يسقط إلى صلاحية الدور.
            var directOverride = await _context.User_Permissions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.User_ID == session.User_ID &&
                    x.Permission_Category == "SCREEN" &&
                    x.Permission_Name == normalizedScreen, cancellationToken);

            if (directOverride != null)
                return IsOperationAllowed(directOverride, operation);

            var permission = await (
                from rolePermission in _context.RolePermissions.AsNoTracking()
                join screen in _context.SystemScreens.AsNoTracking()
                    on rolePermission.Screen_ID equals screen.Screen_ID
                where rolePermission.Role_ID == session.Role_ID
                      && screen.Is_Active
                      && screen.Screen_Code == normalizedScreen
                select rolePermission
            ).FirstOrDefaultAsync(cancellationToken);

            return permission != null && IsOperationAllowed(permission, operation);
        }

        /// <summary>تفويض صريح للعمليات المالية الحساسة: لا يتجاوز مدير النظام Default Deny.</summary>
        public async Task<bool> IsExplicitlyAllowedAsync(ServerSession session, string screenCode, ScreenOperation operation, CancellationToken cancellationToken = default)
        {
            var normalizedScreen = screenCode.Trim();
            var directOverride = await _context.User_Permissions.AsNoTracking().FirstOrDefaultAsync(x => x.User_ID == session.User_ID && x.Permission_Category == "SCREEN" && x.Permission_Name == normalizedScreen, cancellationToken);
            if (directOverride != null) return IsOperationAllowed(directOverride, operation);
            var permission = await (from rolePermission in _context.RolePermissions.AsNoTracking()
                                    join screen in _context.SystemScreens.AsNoTracking() on rolePermission.Screen_ID equals screen.Screen_ID
                                    where rolePermission.Role_ID == session.Role_ID && screen.Is_Active && screen.Screen_Code == normalizedScreen
                                    select rolePermission).FirstOrDefaultAsync(cancellationToken);
            return permission != null && IsOperationAllowed(permission, operation);
        }

        private static bool IsOperationAllowed(RolePermission permission, ScreenOperation operation) =>
            operation switch
            {
                ScreenOperation.View => permission.Can_View,
                ScreenOperation.Add => permission.Can_View && permission.Can_Add,
                ScreenOperation.Edit => permission.Can_View && permission.Can_Edit,
                ScreenOperation.Delete => permission.Can_View && permission.Can_Delete,
                ScreenOperation.Deactivate => permission.Can_View && permission.Can_Delete,
                ScreenOperation.Reactivate => permission.Can_View && permission.Can_Edit,
                ScreenOperation.Print => permission.Can_View && permission.Can_Print,
                ScreenOperation.Export => permission.Can_View && permission.Can_Export,
                ScreenOperation.Import => permission.Can_View && permission.Can_Import,
                ScreenOperation.Approve => permission.Can_View && permission.Can_Approve,
                ScreenOperation.Unapprove => permission.Can_View && permission.Can_UnApprove,
                _ => false
            };

        private static bool IsOperationAllowed(UserPermission permission, ScreenOperation operation) =>
            operation switch
            {
                ScreenOperation.View => permission.Can_View,
                ScreenOperation.Add => permission.Can_View && permission.Can_Add,
                ScreenOperation.Edit => permission.Can_View && permission.Can_Edit,
                ScreenOperation.Delete => permission.Can_View && permission.Can_Delete,
                ScreenOperation.Deactivate => permission.Can_View && permission.Can_Delete,
                ScreenOperation.Reactivate => permission.Can_View && permission.Can_Edit,
                ScreenOperation.Print => permission.Can_View && permission.Can_Print,
                ScreenOperation.Export => permission.Can_View && permission.Can_Export,
                ScreenOperation.Import => permission.Can_View && permission.Can_Import,
                ScreenOperation.Approve => permission.Can_View && permission.Can_Approve,
                ScreenOperation.Unapprove => permission.Can_View && permission.Can_UnApprove,
                _ => false
            };
    }

    public enum ScreenOperation
    {
        View,
        Add,
        Edit,
        Delete,
        /// <summary>إيقاف منطقي؛ يحكمه حق الحذف المنطقي.</summary>
        Deactivate,
        /// <summary>إعادة تفعيل سجل موقوف؛ يحكمها حق التعديل.</summary>
        Reactivate,
        Print,
        Export,
        Import,
        Approve,
        Unapprove
    }
}