using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

/// <summary>
/// Firebase Authentication을 관리하는 싱글톤 클래스입니다.
/// 이메일/비밀번호 로그인, 익명 로그인 등의 인증 기능을 제공합니다.
/// </summary>
public class FirebaseAuthManager
{
    #region Singleton
    private static FirebaseAuthManager instance;

    /// <summary>
    /// FirebaseAuthManager의 싱글톤 인스턴스를 반환합니다.
    /// 인스턴스가 없는 경우 새로 생성합니다.
    /// </summary>
    public static FirebaseAuthManager Instance => instance ??= new FirebaseAuthManager();
    #endregion

    #region Properties
    private FirebaseAuth auth;
    private FirebaseUser user;

    /// <summary>
    /// 현재 사용자의 인증 상태를 반환합니다.
    /// </summary>
    public bool IsAuthenticated => auth?.CurrentUser != null;

    /// <summary>
    /// 현재 인증된 사용자의 고유 ID를 반환합니다.
    /// </summary>
    public string UserId => auth?.CurrentUser?.UserId;
    #endregion

    #region Events
    /// <summary>
    /// 인증 상태가 변경될 때 발생하는 이벤트입니다.
    /// </summary>
    public event Action<FirebaseUser> OnAuthStateChanged;
    #endregion

    #region Initialization
    /// <summary>
    /// Unity 실행 시 자동으로 호출되어 Firebase 인증을 초기화합니다.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Instance.auth = FirebaseAuth.DefaultInstance;
        Instance.auth.StateChanged += Instance.OnChangedState;
    }
    #endregion

    #region Authentication Methods
    /// <summary>
    /// 이메일과 비밀번호를 사용하여 새 계정을 생성합니다.
    /// </summary>
    /// <param name="email">사용자 이메일</param>
    /// <param name="password">사용자 비밀번호</param>
    /// <returns>인증 결과를 포함한 MessageResult 객체</returns>
    public async Task<MessageResult> SignUpWithEmailPasswordAsync(string email, string password)
    {
        MessageResult result = new MessageResult();
        try
        {
            await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            Debug.Log("Membership registration successful");

            await SignInWithEmailPasswordAsync(email, password);
            result.Message = "Membership registration successful";
            result.State = FirebaseState.success;
        }
        catch (FirebaseException ex)
        {
            Debug.LogException(ex);
            result.ErrorCode = ex.ErrorCode;
            result.Message = GetLocalizedErrorMessage(ex.ErrorCode);
            result.State = FirebaseState.Failed;
        }

        return result;
    }

    /// <summary>
    /// 이메일과 비밀번호로 로그인을 시도합니다.
    /// </summary>
    /// <param name="email">사용자 이메일</param>
    /// <param name="password">사용자 비밀번호</param>
    /// <returns>로그인 결과를 포함한 MessageResult 객체</returns>
    public async Task<MessageResult> SignInWithEmailPasswordAsync(string email, string password)
    {
        MessageResult result = new MessageResult();
        try
        {
            await auth.SignInWithEmailAndPasswordAsync(email, password);
            result.State = FirebaseState.success;
            result.Message = "Login successful";
        }
        catch (FirebaseException ex)
        {
            result.State = FirebaseState.Failed;
            result.ErrorCode = ex.ErrorCode;
            result.Message = GetLocalizedErrorMessage(ex.ErrorCode);
            Debug.LogError($"Firebase Auth Error: {ex.ErrorCode} - {ex.Message}");
        }
        catch (Exception ex)
        {
            result.State = FirebaseState.Failed;
            result.Message = "Unknown error occurred";
            Debug.LogError($"Unexpected Error: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// 익명 로그인을 수행합니다.
    /// </summary>
    public async void LoginAnonymously()
    {
        await auth.SignInAnonymouslyAsync();
    }

    /// <summary>
    /// 현재 로그인된 사용자를 로그아웃합니다.
    /// </summary>
    public void Logout()
    {
        auth.SignOut();
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Firebase 인증 상태 변경을 처리하는 이벤트 핸들러입니다.
    /// </summary>
    private void OnChangedState(object sender, EventArgs e)
    {
        bool wasSignedIn = user != null;
        bool isSignedIn = auth.CurrentUser != null;

        user = auth.CurrentUser;
        OnAuthStateChanged?.Invoke(user);

        if (!wasSignedIn && isSignedIn)
        {
            Debug.Log($"User signed in: {user.UserId}");
        }
        else if (wasSignedIn && !isSignedIn)
        {
            Debug.Log("User signed out");
        }
    }
    #endregion

    #region Utility Methods
    /// <summary>
    /// Firebase 에러 코드에 따른 현지화된 에러 메시지를 반환합니다.
    /// </summary>
    /// <param name="errorCode">Firebase 에러 코드</param>
    /// <returns>현지화된 에러 메시지</returns>
    private string GetLocalizedErrorMessage(int errorCode)
    {
        switch (errorCode)
        {
            case 1:
                return "The email address is already in use by another account.";
            case 2:
                return "The email address is badly formatted.";
            case 3:
                return "The password is invalid or the user does not have a password.";
            case 4:
                return "The email address is not found. Please check your email address and try again.";
            case 5:
                return "The password is incorrect. Please try again.";
            case 10:
                return "The email address is invalid.";
            case 17:
                return "The password is too weak. Please try again.";
            case 20:
                return "The email address is already in use by another account.";
            default:
                return "An unknown error occurred.";
        }
    }

    /// <summary>
    /// 현재 로그인된 사용자의 ID를 반환합니다.
    /// </summary>
    /// <returns>사용자 ID</returns>
    public string GetUserID()
    {
        return auth.CurrentUser.UserId;
    }
    #endregion
}
