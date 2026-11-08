using System.Collections.Concurrent;
using System.Net;
using System.Text;
using BookHive.Services;

namespace BookHive.Tests.Helpers;


// Fake HTTP handler so OpenLibraryClient can be tested without the network.

public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;
    private readonly ConcurrentQueue<Uri> _requests = new();

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        _respond = respond;
    }

    public IReadOnlyCollection<Uri> Requests => _requests;

    public OpenLibraryClient CreateClient()
    {
        var httpClient = new HttpClient(this)
        {
            BaseAddress = new Uri("https://openlibrary.org/")
        };

        return new OpenLibraryClient(httpClient);
    }

    public static HttpResponseMessage Json(
        string json,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _requests.Enqueue(request.RequestUri!);

        return Task.FromResult(_respond(request));
    }
}