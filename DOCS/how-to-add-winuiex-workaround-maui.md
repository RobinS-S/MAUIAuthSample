### Setting Up the WinUIEx Workaround for the Windows Native URL Handler Bug

Follow the steps below to configure the WinUIEx workaround:

1. **Open your project.**

2. **Add the latest [WinUIEx](https://www.nuget.org/packages/WinUIEx/) package to your MAUI project.** At the time of writing, the latest version is 2.2.0. To do this, open your MAUI .csproj file located within the project folder. Add the following within the `<Project>` tags. This will include the WinUIEx package exclusively for Windows.

    ```xml
    <ItemGroup>
        <PackageReference Include="WinUIEx" Version="2.2.0" Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'" />
    </ItemGroup>
    ```

    [View the example file](../Clients/MauiXamlClient/MauiXamlClient.csproj#L69)

3. **Modify `Platforms\Windows\App.xaml.cs`.** You might need to expand the App.xaml dropdown. After `public App() {`, add:

    ```csharp
    if (WebAuthenticator.CheckOAuthRedirectionActivation())
        return;
    ```

    [View the example file](../Clients/MauiXamlClient/Platforms/Windows/App.xaml.cs#L19)

    This step ensures your application will redirect to the running instance upon launch if one is running and expecting a login/logout/refresh result.

4. **Adjust your authentication code to include an exception for Windows.** Do this in the section where you're using `WebAuthenticator`.

    ```csharp
    #if WINDOWS
    using WebAuthenticator = WinUIEx.WebAuthenticator;
    #endif

    ...

    var authResult =
    #if WINDOWS
        await WebAuthenticator
            .AuthenticateAsync(new Uri(options.StartUrl), new Uri(ApiConfig.Instance.CustomProtocolRedirectUri));
    #else
            await WebAuthenticator.Default
        .AuthenticateAsync(new Uri(options.StartUrl), new Uri(ApiConfig.Instance.CustomProtocolRedirectUri));
    #endif
    ```

    [View the example file](../Clients/MauiXamlClient/Auth/AuthBrowser.cs#L16)

    Now, you have added the Windows workaround. Remember to handle the different return types for `authResult` correctly!

    [View the example file](../Clients/MauiXamlClient/Auth/AuthBrowser.cs#L24)
