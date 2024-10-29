using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using QFSW.QC;

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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        UnityServices.InitializeAsync();
        Debug.Log("AuthManager Created!");
    }

    public async Task<MessageResult> SignUpWithUsernamePasswordAsync(string eamil, string password)
    {
        MessageResult result = new MessageResult();
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(eamil, password);
            Debug.Log("Membership registration successful");
            result.Message = "Membership registration successful";
            result.State = FirebaseState.success;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{ex.ErrorCode}");

            result.ErrorCode = ex.ErrorCode;
            result.Message = ex.Message;
            result.State = FirebaseState.Failed;
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            result.State = FirebaseState.Failed;
            result.ErrorCode = ex.ErrorCode;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task<MessageResult> SignInWithUsernamePasswordAsync(string eamil, string password)
    {
        MessageResult result = new MessageResult();
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                result.Message = "The player is already signed in.";
                result.ErrorCode = AuthenticationErrorCodes.AccountAlreadyLinked;
                result.State = FirebaseState.Failed;
                return result;
            }
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(eamil, password);
            result.State = FirebaseState.success;
            result.Message = "Login successful";
            Debug.Log("Login successful");
        }
        catch(AuthenticationException ex)
        {
            Debug.LogException(ex);
            Debug.Log(ex.ErrorCode);

            result.State = FirebaseState.Failed;
            result.ErrorCode = ex.ErrorCode;
            result.Message = ex.Message;
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            result.State = FirebaseState.Failed;
            result.ErrorCode = ex.ErrorCode;
            result.Message = ex.Message;
        }

        return result;
    }

    public async Task SignInAnonymously(string eamil, string password)
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public void Logout()
    {
        AuthenticationService.Instance.SignOut();
    }

    [Command]
    public async Task<bool> CheckIfLoggedIn()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("User is logged in.");
            return true;
        }
        else
        {
            Debug.Log("User is not logged in.");
            return false;
        }
    }
}
