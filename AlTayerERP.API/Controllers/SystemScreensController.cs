using AlTayerERP.Core.Entities;
using AlTayerERP.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlTayerERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemScreensController : ControllerBase
    {
        private