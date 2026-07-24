using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// محرك التفويض للخادم. السياسة Default Deny: غياب صلاحية الدور أو استثناء المستخدم
    /// يعني الرفض. استثناء المستخدم SCREEN/<Screen_Code> يحل محل صلاحية دوره لتلك الشاشة.
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
            if (session.Is_System_Admin)
                return true;

            var normalizedScreen = screenCode.Trim();

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
