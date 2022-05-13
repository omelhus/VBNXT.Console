using ConsoleDemo.VismaConnect;
using ConsoleTables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using VBNXT.Client;

// Read the configuration from user secrets
/* Format:
{
  "VismaConnect": {
    "ClientId": "isv_client_id",
    "ClientSecret": "client secret from Visma Connect"
  },
  "RefreshToken": "refresh token from already authenticated session"
}
*/
var config = new ConfigurationBuilder()
               .AddUserSecrets<Program>()
               .Build();

// Create a service collection and configure our services
var serviceCollection = new ServiceCollection();
serviceCollection
    .AddHttpClient()
    .AddTransient<VismaConnectService>()
    .AddSingleton<JwtSecurityTokenHandler>()
    .Configure<VismaConnectCredentials>(options =>
    {
        options.ClientId = config["VismaConnect:ClientId"];
        options.ClientSecret = config["VismaConnect:ClientSecret"];
    })
    .AddVismaBusinessNXTClient()
    .ConfigureHttpClient((serviceProvider, client) =>
    {
        // Read audience from the JWT token and use that as base address for the API.
        // https://business.visma.net/api/graphql is used for "authorization code"-flow and
        // https://business.visma.net/api/graphql-service for "client credentials"-flow.
        var decodedToken = serviceProvider.GetRequiredService<JwtSecurityTokenHandler>().ReadJwtToken(VismaConnectService.Token);
        if (decodedToken.Audiences?.FirstOrDefault(x => x.Contains("graphql")) is string endpoint)
        {
            client.BaseAddress = new Uri(endpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", VismaConnectService.Token);
        }
    });

// Create a service provider
var serviceProvider = serviceCollection.BuildServiceProvider();

using (var scope = serviceProvider.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<IVismaBusinessNXTClient>();
    var vismaConnect = scope.ServiceProvider.GetRequiredService<VismaConnectService>();

    // Use vismaConnect.CreateWithClientCredentials(); if you want to use the Visma Business NXT GraphQL service with client credentials.
    var accessToken = await vismaConnect.CreateFromRefreshToken(config["RefreshToken"]);

    // This will set the access token in an AsyncLocal<string>, which ensures it's only available in this scope.
    using (VismaConnectService.UseAccessToken(accessToken))
    {
        // Fetch available companies
        var companies = await client.GetAvailableCompanies.ExecuteAsync();
        if (companies.Errors?.Count > 0)
        {
            Console.WriteLine(ConsoleTable.From(companies.Errors).ToString());
        }
        else
        {
            Console.WriteLine(ConsoleTable.From(companies.Data.AvailableCompanies.Items).ToString());
        }

        var firstCompanyId = (int)companies.Data?.AvailableCompanies?.Items?[0]?.VismaNetCompanyId;

        // List all customers from the first company
        var customers = await client.GetCustomers.ExecuteAsync(firstCompanyId);
        if (customers.Errors?.Count > 0)
        {
            Console.WriteLine(ConsoleTable.From(customers.Errors).ToString());
        }
        else
        {
            Console.WriteLine(ConsoleTable.From(customers.Data?.UseCompany?.Associate?.Items?.Select(x => new
            {
                x.CustomerNo,
                Name = x.Name?.Trim(),
                x.CompanyNo,
                Country = x.Country?.Name
            })).ToString());
        }
    }
}
Console.ReadKey();