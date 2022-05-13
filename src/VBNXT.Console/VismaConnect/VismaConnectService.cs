using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace ConsoleDemo.VismaConnect;

public class VismaConnectCredentials
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}

public class VismaConnectService
{
    private readonly HttpClient httpClient;
    private readonly VismaConnectCredentials credentials;

    public VismaConnectService(HttpClient httpClient, IOptions<VismaConnectCredentials> credentials)
    {
        this.httpClient = httpClient;
        this.credentials = credentials.Value;
    }

    public static string? Token { get => asyncLocal.Value; }

    public readonly static AsyncLocal<string?> asyncLocal = new();

    public static IDisposable UseAccessToken(string accessToken)
    {
        asyncLocal.Value = accessToken;
        return new Disposable(() => asyncLocal.Value = null);
    }

    public async Task<string> CreateFromRefreshToken(string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://connect.visma.com/connect/token")
        {
            Content = new FormUrlEncodedContent(new[]
        {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
            })
        };
        request.Headers.Authorization = CreateAuthenticationHeaderValue();
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseObject = await response.Content.ReadFromJsonAsync<OAuth2TokenResponse>();
        return responseObject?.access_token;
    }

    public async Task<string> CreateWithClientCredentials()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://connect.visma.com/connect/token")
        {
            Content = new FormUrlEncodedContent(new[]
        {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "business-graphql-service-api:access-group-based")
            })
        };
        request.Headers.Authorization = CreateAuthenticationHeaderValue();
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseObject = await response.Content.ReadFromJsonAsync<OAuth2TokenResponse>();
        return responseObject?.access_token;
    }

    private AuthenticationHeaderValue CreateAuthenticationHeaderValue()
    {
        return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.ClientId}:{credentials.ClientSecret}")));
    }
}
