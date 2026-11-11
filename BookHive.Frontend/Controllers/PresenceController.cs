using System.Net;
using BookHive.Models;
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
    public async Task<IActionResult> Ticket()
    {
        HttpClient client = _httpClientFactory.CreateClient("BackendApi");

        HttpResponseMessage response =
            await client.PostAsync("api/presence/ticket", content: null);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return Unauthorized();
        }

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode(StatusCodes.Status502BadGateway);
        }

        var ticket = await response.Content.ReadFromJsonAsync<PresenceTicketResponse>();

        if (ticket == null || client.BaseAddress == null)
        {
            return StatusCode(StatusCodes.Status502BadGateway);
        }

        return Json(new
        {
            token = ticket.Ticket,
            hubUrl = new Uri(client.BaseAddress, "hubs/presence").ToString()
        });
    }
}