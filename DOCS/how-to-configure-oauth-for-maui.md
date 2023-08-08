## How to setup OAuth for MAUI

**If your provider does not support or allow custom URL protocol handlers as RedirectUrl or PostLogoutRedirectUrls, you must create a HTTPS page that gets the token result and opens the native URL handler along with the result. Or you could use IdentityServer as used in the example project.**

1. Register your application:

In order to use OAuth2 with your application, you first need to register your application with the service you want to authenticate with.

2. Create an OAuth 2.0 client ID:

Name your OAuth 2.0 client and set your application's authorized JavaScript origins.

For the redirect URIs, add the RedirectUrl "authtest://processauth" and PostLogoutRedirectUrl "authtest://processauth". Add default scopes 'openid' and 'profile'.

Click on "Create".

Your new OAuth 2.0 client ID and secret will now appear in the list of IDs for your client.

3. Setting up your Maui application:

Now, move to the Maui application where you want to setup OAuth. Set the ClientId, scopes, RedirectUrl and PostLogoutRedirectUrl in the ApiConfig.cs for the MAUI project.

That's all!