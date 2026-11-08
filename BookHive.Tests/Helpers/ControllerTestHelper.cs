using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookHive.Tests.Helpers;

public static class ControllerTestHelper
{
    public static TController WithUser<TController>(
        this TController controller, int? userId)
        where TController : ControllerBase
    {
        ClaimsIdentity identity = userId == null
            ? new ClaimsIdentity()
            : new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) },
                authenticationType: "Test");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }
}