namespace Reader.Infrastructures.Clients._Common.Base;

//public abstract class BaseGisClientWithAuthentication : BaseGisClient
//{
//    public bool Authenticated { get; private set; }
//    private string? _token = null;
//    protected BaseGisClientWithAuthentication(HttpClient client, GisClientConfiguration configuration,
//        ILogger<BaseGisClientWithAuthentication> logger) : base(client, configuration, logger)
//    {
//    }
//    public override async Task InitializeAsync()
//    {
//        await AuthenticateAsync();
//    }
//    protected override Task<T> GetAsync<T>(string url, params string[] paths)
//    {
//        var authUrl = AuthenticateUrl(url);
//        return base.GetAsync<T>(authUrl, paths);
//    }
//    protected virtual async Task AuthenticateAsync()
//    {
//        //var requestUrl =
//        //    $"{Configuration.Address}/tokens/?request=getToken&username={Configuration.Username}&password={Configuration.Password}&expires=1569473358584";
//        using var response = await Client.GetAsync(CreateAuthenticationUrl());
//        var responseString = await response.Content.ReadAsStringAsync();
//        if (response.StatusCode != HttpStatusCode.OK)
//        {
//            Logger.LogCritical($"Fail to authenticate with response {responseString}.");
//            throw new FailToAuthenticateException($"Fail to authenticate with response {responseString}.");
//        }

//        _token = responseString;
//        Authenticated = true;
//        Logger.LogDebug("Client authenticated with token {token}.", _token);
//    }

//    protected abstract string CreateAuthenticationUrl();
//}