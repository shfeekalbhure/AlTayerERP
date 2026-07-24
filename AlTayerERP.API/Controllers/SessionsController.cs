using AlTayerERP.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlTayerERP.API.Controllers
{
    /// <summary>
    /// مراقبة الجلسات الحية وإبطالها. لا تعرض هذه الواجهة Access/Refresh Token مطلقاً.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class SessionsController : ControllerBase
    {
        private readonly ServerSessionService _sessions;
        private readonly TokenService _tokens;
        private readonly AuditTrailService _audit;

        public SessionsController(ServerSessionService sessions, TokenService tokens, AuditTrailService audit)
        {
            _sessions = sessions;
            _tokens = tokens;
            _audit = audit;
        }

        [HttpGet]
        public IActionResult Get()
        {
            if (HttpContext.Items["ServerSession"] is not ServerSession current)
                return Unauthorized();

            var rows = _sessions.GetActiveSessions()
                .Where(x => current.Is_System_Admin || x.User_ID == current.User_ID)
                .Select(x => new
                {
                    x.Session_ID, x.User_ID, x.Role_ID, x.Company_ID, x.Branch_ID,
                    x.Year_ID, x.Device_ID, x.Issued_At, x.Expires_At
                });
            return Ok(rows);
        }

        [HttpPost("{sessionId}/revoke")]
        public async Task<IActionResult> Revoke(string sessionId, CancellationToken cancellationToken)
        {
            if (HttpContext.Items["ServerSession"] is not ServerSession current)
                return Unauthorized();

            if (!_sessions.TryGet(sessionId, out var target))
                return NotFound("الجلسة غير موجودة أو انتهت.");

            if (!current.Is_System_Admin && target.User_ID != current.User_ID)
                return Forbid();

            await _tokens.RevokeSessionRefreshTokensAsync(sessionId, "ADMIN_SESSION_REVOKE", cancellationToken);
            _sessions.Remove(sessionId);
            _audit.Add(current, HttpContext, "sessions", sessionId, "SESSION_REVOKE",
                newValues: new { target.User_ID, target.Device_ID });
            await _tokens.SaveChangesAsync(cancellationToken);
            return Ok(new { message = "تم إبطال الجلسة." });
        }
    }
}
