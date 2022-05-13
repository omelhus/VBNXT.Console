# Visma Business NXT GraphQL API Demo
This is a simple demo connecting to the Visma Business GraphQL API using .net

Start with installing the [StrawberryShake Extension for Visual Studio](https://marketplace.visualstudio.com/items?itemName=ChilliCream.strawberryshake-visualstudio). This contains a GraphQL language server that will give you autocomplete when you write queries and mutations.

This also requires you to run the latest version of Visual Studio 2022 (17.2).

## Obtain client id and secret

Follow these guides on how to obtain a client id and secret - [Authorization Flow](https://docs.business.visma.net/docs/authentication/web/setup_web) or [Service Flow](https://docs.business.visma.net/docs/authentication/service/setup_service).

**Tip:** For your redirect url domain you can use localtest.me (or any subdomain of localtest.me), as this domain points to localhost. You won't be able to create an official certificate for it, but it's enough to get going.

When you have your client id and secret you can right click the VBNXT.Console-project and select "Manage user secrets". This will open secrets.json and allow you to add your client id and secret.

```json
{
  "VismaConnect": {
    "ClientId": "isv_test_application",
    "ClientSecret": "...."
  },
  "RefreshToken": "..."
}
```

Note that if you have a service to service integration set up you won't need the refresh token, and you don't need to enter it in the user secrets.

## Create a refresh token
In order to create a refresh token you need to setup [Banana Cake Pop](https://chillicream.com/docs/bananacakepop/install) or a similar OAuth2 compatible client. You'll need to know the following settings.

| Field | Description |
| ----- | ------------ |
| Type | Oauth 2 |
| Grant Type | Authorization Code |
| Authorization URL | https://connect.visma.com/connect/authorize |
| Access Token URL | https://connect.visma.com/connect/token |
| Client Id | (the one provided from Visma Connect) |
| Client Secret | (the one provided from Visma Connect) |
| Use PKCE | Yes | 
| Code Challenge Method | SHA-256 |
| Redirect URL | https://app.localtest.me/whatever/you/decided |
| Scope | openid profile email business-graphql-api:access-group-based offline_access |
| Credentials | As Basic Auth Header |

When you select "Fetch token" you'll get an id token, access token and a refresh token. The refresh token is what you'll set in the user secrets file.

## Adding queries and mutations

GraphQL queries and mutations are added to the VBNXT.Client project under Queries. The queries and mutations will then be source generated into c#-code, so all you need to do is add what you want and compile the project. The method will then be available on the VismaBusinessNXTClient.
