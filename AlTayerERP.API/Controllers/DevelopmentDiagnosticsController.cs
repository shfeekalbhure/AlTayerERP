using AlTayerERP.API.Services;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers;

[ApiController]
[Authorize]
[Route("api/diagnostics")]
public sealed class DevelopmentDiagnosticsController(AppDbContext db, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet("session-context")]
    public async Task<IActionResult> GetSessionContext(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
            return NotFound();

        if (HttpContext.Items["ServerSession"] is not ServerSession session)
            return Unauthorized(new { message = "انتهت الجلسة أو أنها غير صالحة." });

        if (!await db.Database.CanConnectAsync(cancellationToken))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { sessionValid = true });

        return Ok(new
        {
            sessionValid = true,
            companyId = session.Company_ID,
            branchId = session.Branch_ID,
            fiscalYearId = session.Year_ID,
            databaseName = db.Database.GetDbConnection().Database
        });
    }
}
