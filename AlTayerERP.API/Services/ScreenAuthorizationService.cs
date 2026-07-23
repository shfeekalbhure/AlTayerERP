using AlTayerERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Services
{
    /// <summary>
    /// يفرض صلاحيات الأدوار من الخادم قبل تنفيذ عمليات الشاشات الحساسة.
    /// لا يعتمد على إخفاء الأزرار في سطح المكتب.
    /// </summary>
    public sealed class ScreenAuthorizationService
    {
        private readonly AppDbContext _context;

        public ScreenAuthorizationService(AppDbContext context) => _context = context;

        public async Task<bool> IsAllowedAsync(
            ServerSession session,
            string screenCode,
            ScreenOperation operation)
        {
            if (session.Is_System_Admin)
                return true;

            var permission = await (
                from rolePermission in _context.RolePermissions.AsNoTracking()
                join screen in _context.SystemScreens.AsNoTracking()
                    on rolePermission.Screen_ID equals screen.Screen_ID
                where rolePermission.Role_ID == session.Role_ID
                      && screen.Is_Active
                      && screen.Screen_Code == screenCode
                select rolePermission
            ).FirstOrDefaultAsync();

            if (permission == null)
                return false;

            return operation switch
            {
                ScreenOperation.View => permission.Can_View,
                ScreenOperation.Add => permission.Can_View && permission.Can_Add,
                ScreenOperation.Edit => permission.Can_View && permission.Can_Edit,
                ScreenOperation.Delete => permission.Can_View && permission.Can_Delete,
                ScreenOperation.Print => permission.Can_View && permission.Can_Print,
                ScreenOperation.Approve => permission.Can_View && permission.Can_Approve,
                ScreenOperation.Unapprove => permission.Can_View && permission.Can_UnApprove,
                _ => false
            };
        }
    }

    public enum ScreenOperation
    {
        View,
        Add,
        Edit,
        Delete,
        Print,
        Approve,
        Unapprove
    }
}
