using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ArtisanalVCS.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
