using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace BookHive.Tests.Helpers;

public sealed class StubBackendHandler(
    params (string Method, string PathAndQuery, HttpStatusCode StatusCode, string Body)[] responses)
    : HttpMessageHandler
{
    private readonly IReadOnlyDictionary<(string Method, string PathAndQuery), (HttpStatusCode StatusCode, string Body)> _responses =
        responses.ToDictionary(
            response => (response.Method, response.PathAndQuery),
            response => (response.StatusCode, response.Body));

    public ConcurrentQueue<RecordedRequest> Requests { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string method = request.Method.Method;
        string pathAndQuery = request.RequestUri?.PathAndQuery
            ?? throw new InvalidOperationException("Backend request has no URI.");
        string body = request.Content == null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken);

        Requests.Enqueue(new RecordedRequest(
            method,
            pathAndQuery,
            body,
            request.Headers.Authorization?.ToString()));

        if (!_responses.TryGetValue((method, pathAndQuery), out var response))
        {
            throw new InvalidOperationException(
                $"Unexpected backend request: {method} {pathAndQuery}. " +
                "Add it to the StubBackendHandler responses.");
        }

        return new HttpResponseMessage(response.StatusCode)
        {
            Content = new StringContent(response.Body, Encoding.UTF8, "application/json")
        };
    }
}
