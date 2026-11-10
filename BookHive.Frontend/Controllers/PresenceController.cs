using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Frontend.Controllers;

[Authorize]
public class PresenceController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PresenceController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Token()
    {
        string? token = await HttpContext.GetTokenAsync("access_token");
        Uri? backendAddress = _httpClientFactory.CreateClient("BackendApi").BaseAddress;

        if (string.IsNullOrWhiteSpace(token) || backendAddress == null)
        {
            return Unauthorized();
        }

        return Json(new
        {
            token,
            hubUrl = new Uri(backendAddress, "hubs/presence").ToString()
        });
    }
}