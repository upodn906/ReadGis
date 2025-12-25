using Microsoft.Extensions.Logging;
using Reader.Infrastructures.Clients._Common.Base;

namespace Reader.Infrastructures.Clients;

public class EzriClient : EdsabClient
{
    private static string? _token = null;
    public EzriClient(HttpClient client, GisClientConfiguration configuration, ILogger<BaseGisClient> logger) : base(client, configuration, logger)
    {
        if(configuration.Token != null)
            _token = configuration.Token;
    }
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (_token == null)
        {
            var authUrl =
                $"{Configuration.AuthenticationAddress}?request=getToken&username={Configuration.Username}&password={Configuration.Password}&expires=1569473358584";
            _token = await Client.GetStringAsync(authUrl);
            Logger.LogInformation("Authenticated with token {token}.", _token);
        }
        else
        {
            Logger.LogInformation("Skipped authentication with toke {token}.", _token);
        }
    }
    public override string AuthenticateUrl(in string url)
    {
        if (url.Contains('?'))
        {
            return $"{url}&token={_token}";
        }
        return $"{url}?token={_token}";
    }
}