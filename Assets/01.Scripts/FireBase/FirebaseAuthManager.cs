using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Google;
using QFSW.QC;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UIElements;


public class FirebaseAuthManager
{
    private static FirebaseAuthManager instance = null;
    public static FirebaseAuthManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new FirebaseAuthManager();
            }

            return instance;
        }
    }

    private FirebaseAuth auth = null;
    private FirebaseUser user = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        Instance.auth = FirebaseAuth.DefaultInstance;
        Instance.auth.StateChanged += Instance.OnChangedState;
    }

    private void OnChangedState(object sender, EventArgs e)
    {
        if(auth.CurrentUser != user)
        {
            bool signed = (auth.CurrentUser != user && auth.CurrentUser != null);
            if(!signed && user != null)
            {
                Debug.Log("Logout");
                return;
            }

            user = auth.CurrentUser;


            if (signed)
            {
                Debug.Log("Login");
                return;
            }
        }
    }

    public string GetUserID()
    {
        return auth.CurrentUser.UserId;
    }

    public async void LoginAnonymousl()
    {
        await auth.SignInAnonymouslyAsync();
    }

    //Result
    public async Task<MessageResult> SignUpWithEmailPasswordAsync(string eamail, string password)
    {
        MessageResult result = new MessageResult();
        try
        {
            await auth.CreateUserWithEmailAndPasswordAsync(eamail, password);
            Debug.Log("Membership registration successful");

            await SignInWithEmailPasswordAsync(eamail, password);
            result.Message = "Membership registration successful";
            result.State = FirebaseState.success;
        }
        catch (FirebaseException ex)
        {
            Debug.LogException(ex);
            
            //if (ex.ErrorCode == 8)
            //    SignInWithEmailPasswordAsync(eamail, password);

            Debug.Log(ex.Message);
            result.ErrorCode = ex.ErrorCode;
            result.Message = ex.Message;
            result.State = FirebaseState.Failed;
        }

        return result;
    }

    public async Task<MessageResult> SignInWithEmailPasswordAsync(string eamail, string password)
    {
        MessageResult result = new MessageResult();
        try
        {
            await auth.SignInWithEmailAndPasswordAsync(eamail, password);

            result.State = FirebaseState.success;
            result.Message = "Login successful";
            Debug.Log("Login successful");
        }
        catch (FirebaseException ex)
        {
            Debug.LogException(ex);
            Debug.Log(ex.Message);
            result.State = FirebaseState.Failed;
            result.ErrorCode = ex.ErrorCode;
            result.Message = ex.Message;
        }

        return result;
    }

    public void Logout()
    {
        auth.SignOut();
    }
}
