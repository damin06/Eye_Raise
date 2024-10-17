using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class AuthManager
{
    private static AuthManager instance = null;
    public static AuthManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AuthManager();
            }

            return instance;
        }
    }

    AuthManager()
    {
        UnityServices.InitializeAsync();
        Debug.Log("AuthManager Created!");
    }

    public async void SignUpWithUsernamePasswordAsync(string eamil, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(eamil, password);
            Debug.Log("SignUp is successful.");
            await SignInWithUsernamePasswordAsync(eamil, password);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{ex.ErrorCode}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task SignInWithUsernamePasswordAsync(string eamil, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(eamil, password);
            Debug.Log("SignIn is successful");
        }
        catch(AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task SignInAnonymously(string eamil, string password)
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public void Logout()
    {
        AuthenticationService.Instance.SignOut();
    }
}
