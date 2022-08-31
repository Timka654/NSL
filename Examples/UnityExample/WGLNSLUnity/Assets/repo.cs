using NSL.RestExtensions;
using NSL.RestExtensions.RESTContentProcessors;
using NSL.RestExtensions.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;

public class repo : UnityWebRequestRepository
{
    public override string GetBaseDomain()
        => "http://localhost:7190";

    public override HttpMethod GetHttpMethod()
        => HttpMethod.Post;

    private const string ApiLoginUrl = "Account/APILogin";

    private static repo Instanse;

    private static IRestContentProcessor contentConverter => Instanse.HttpContentConverter;

    static repo()
    {
        Instanse = new repo();
    }

    public static void APILogin(LoginViewModel data, WebResponseDelegate<LoginResult> onResult)
    {
        Instanse.SafeRequest<LoginResult>(
            ApiLoginUrl,
            onResult,
            request => contentConverter.SetContent(request, data));
    }
}



public class LoginViewModel
{
    public string Email { get; set; }

    public string Password { get; set; }

    public bool RememberMe { get; set; }

    public string GameID { get; set; }

}
public class LoginResult
{
    public LoginStatusCode Status { get; set; }
    public DateTime Date { get; set; }
    public string UID { get; set; }

}

public enum LoginStatusCode
{
    Success,
    Failure,
    LockedOut,
    RequiresVerification
}