using System.Net.Http.Json;

namespace Client;

public static class AccessTokenProvider
{
    private static readonly HttpClient httpClient = new();

    public static async Task<string> GetAccessToken(string username)
    {
        var request = new RequestModel(username, "Qwerty1@");
        var responseMessage = await httpClient.PostAsJsonAsync("http://localhost:5123/api/login", request);
        var responseModel = await responseMessage.Content.ReadFromJsonAsync<ResponseModel>();
        ArgumentNullException.ThrowIfNull(responseModel);
        return responseModel.AccessToken;
    }

    public sealed record RequestModel(string Username, string Password);

    public sealed record ResponseModel(string AccessToken);
}
