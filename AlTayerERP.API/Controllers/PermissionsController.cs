using AlTayerERP.API.DTOs;
using AlTayerERP.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlTayerERP.API.Controllers;

/// <summary>
/// واجهة تقييم الصلاحيات. لا تقبل User_ID أو Role_ID من العميل؛ الجلسة هي مصدر الهوية.
/// </summary>
[ApiController]
[Route("api/permissions")]
public class PermissionsController : ControllerBase
{
    private readonly PermissionService _permissionService;

    public PermissionsController(PermissionService permissionService) =>
        _permissionService = permissionService;

    /// <summary>يفحص صلاحية عملية ونطاقها وحد الاعتماد من الخادم.</summary>
    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate([FromBody] PermissionEvaluationRequestDto dto)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Permission_Code))
            return BadRequest("رمز الصلاحية مطلوب.");

        if (HttpContext.Items["ServerSession"] is not ServerSession session)
            return Unauthorized("انتهت الجلسة أو أنها غير صالحة.");

        var result = await _permissionService.EvaluateAsync(
            session, dto.Permission_Code.Trim(), dto.Company_ID, dto.Branch_ID,
            dto.Amount_Local, dto.Voucher_Type_Code);

        return Ok(result);
    }
}