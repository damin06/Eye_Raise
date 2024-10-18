using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;
using Utill = Util.Util;
using System.Threading.Tasks;
using Firebase;
using UnityEngine.Purchasing;


[Serializable]
struct ArrowPos
{
    public Vector2 OriginPos;
    public Vector2 EndPos;
    public string ObjectName;
}

public class Panel_Intro : UI_Panel
{
    private Button eye;
    private TextMeshProUGUI title;
    private TMP_InputField nameInput;
    private TMP_InputField emailInput;
    private TMP_InputField passwordInput;

    [SerializeField] private string[] forbiddenWords;
    [SerializeField] ArrowPos[] arrowPos;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Material eyeMaterial;
    [SerializeField] private float minRange = -0.15f;
    [SerializeField] private float maxRange = 0.15f;

    protected override void Init()
    {
        base.Init();
        
        eye = Get<Button>("Button_Eye");
        title = Get<TextMeshProUGUI>("TMP_Title");
        nameInput = Get<TMP_InputField>("InputField_Name");
        emailInput = Get<TMP_InputField>("InputField_Email");
        passwordInput = Get<TMP_InputField>("InputField_Password");

        nameInput.characterLimit = 10;
        nameInput.onValueChanged.AddListener(HandleOnNameInputChanged);

        inputReader.AimPositionEvent += HandleAimPosition;

        for(int i = 1; i <= 4; i++)
        {
            Image _arrow = Get<Image>($"Image_Arrow{i}");
            BindEvent(_arrow.gameObject, HandleOnEnterArrow, Define.ClickType.Enter);
            BindEvent(_arrow.gameObject, HandleOnExitArrow, Define.ClickType.Exit);
        }

        BindEvent(title.gameObject, HandleOnExitArrow, Define.ClickType.Exit);
        BindEvent(title.gameObject, HandleOnEnterArrow, Define.ClickType.Enter);
        eyeMaterial.SetFloat("_EyeVeins", 1);

        ShowIntro();
    }

    #region Intro

    private void ShowIntro()
    {
        for (int i = 0; i < arrowPos.Length; i++)
        {
            Get<Image>(arrowPos[i].ObjectName).rectTransform.DOAnchorPos(arrowPos[i].OriginPos, 0.4f).SetEase(Ease.InBack);
        }

        Get<TextMeshProUGUI>("TMP_Title").rectTransform.DOAnchorPosY(-242, 0.4f).SetEase(Ease.InBack);
        title.text = "OH MY\nEYE!!!";
        title.fontSize = 196f;
        eye.onClick.AddListener(HandleClickEye);
        SetEyeVeins(false);
    }

    private void HideIntro()
    {
        for (int i = 0; i < arrowPos.Length; i++)
        {
            Get<Image>(arrowPos[i].ObjectName).rectTransform.DOAnchorPos(arrowPos[i].EndPos, 0.4f).SetEase(Ease.InBack);
        }
        Get<TextMeshProUGUI>("TMP_Title").rectTransform.DOAnchorPosY(287f, 0.4f).SetEase(Ease.InBack);
        (nameInput.transform as RectTransform).DOAnchorPosY(800f, 0.3f).SetEase(Ease.InBack);
        (emailInput.transform as RectTransform).DOAnchorPosY(1200f, 0.3f).SetEase(Ease.InBack);
        (passwordInput.transform as RectTransform).DOAnchorPosY(995f, 0.3f).SetEase(Ease.InBack);
    }

    #endregion

    #region NameInput

    private void ShowNameInput()
    {
        for (int i = 0; i < arrowPos.Length; i++)
        {
            Get<Image>(arrowPos[i].ObjectName).rectTransform.DOAnchorPos(arrowPos[i].OriginPos, 0.4f).SetEase(Ease.Linear);
        }

        (nameInput.transform as RectTransform).DOAnchorPosY(0, 0.3f).SetEase(Ease.InBack);

        title.text = "Enter your name!";
        title.fontSize = 161f;
        title.rectTransform.DOAnchorPosY(-242f, 0.4f).SetEase(Ease.InBack);

        eye.onClick.AddListener(async () =>
        {
            try
            {
                FirebaseResult databaseResult = await DatabaseManager.Instance.GetUserDataBase(FirebaseAuthManager.Instance.GetUserID());
                UserInfo userInfo = (UserInfo)databaseResult.Result;

                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { "UserName", nameInput.text }
                };
                await DatabaseManager.Instance.UpdateUserData(FirebaseAuthManager.Instance.GetUserID(), updates).ContinueWith(async task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        eye.onClick.RemoveAllListeners();
                        await UIManager_Lobby.Instance.ChangePanel(gameObject.name, "Panel_Lobby");
                    }
                });
            }
            catch (FirebaseException ex)
            {
                Debug.LogException(ex);
            }
        });
    }

    private void HideNameInput()
    {
        for (int i = 0; i < arrowPos.Length; i++)
        {
            Get<Image>(arrowPos[i].ObjectName).rectTransform.DOAnchorPos(arrowPos[i].EndPos, 0.4f).SetEase(Ease.Linear);
        }

        (nameInput.transform as RectTransform).DOAnchorPosY(800f, 0.3f).SetEase(Ease.InBack);
    }

#endregion

    #region Login

    private void ShowLoginInput()
    {
        (emailInput.transform as RectTransform).DOAnchorPosY(220f, 0.3f).SetEase(Ease.InBack);
        (passwordInput.transform as RectTransform).DOAnchorPosY(0, 0.3f).SetEase(Ease.InBack);
        eye.onClick.RemoveAllListeners();

        eye.onClick.AddListener(async () =>
        {
            FirebaseResult signUpResult = await FirebaseAuthManager.Instance.SignUpWithEmailPasswordAsync(emailInput.text, passwordInput.text);

            if (signUpResult.ErrorCode == 8 && signUpResult.State == FirebaseState.Failed)
            {
                FirebaseResult signIpResult = await FirebaseAuthManager.Instance.SignInWithEmailPasswordAsync(emailInput.text, passwordInput.text);
                ShowErrorMessage(signIpResult.Message);

                if (signIpResult.State == FirebaseState.success)
                {
                    HideLoginInput();
                    eye.onClick.RemoveAllListeners();
                    await UIManager_Lobby.Instance.ChangePanel(gameObject.name, "Panel_Lobby");
                }
            }
            else if (signUpResult.State == FirebaseState.success)
            {
                ShowErrorMessage(signUpResult.Message);
                UserInfo userInfo = new UserInfo("NONAME", 0);
                FirebaseResult databaseResult = await DatabaseManager.Instance.CreateUserDatabase(FirebaseAuthManager.Instance.GetUserID(), userInfo);
                await FirebaseAuthManager.Instance.SignInWithEmailPasswordAsync(emailInput.text, passwordInput.text).ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        HideLoginInput();
                        Invoke("ShowNameInput", 0.5f);
                        eye.onClick.RemoveAllListeners();
                    }

                });
            }

        });
    }

    private void HideLoginInput()
    {
        (emailInput.transform as RectTransform).DOAnchorPosY(1200f, 0.3f).SetEase(Ease.InBack);
        (passwordInput.transform as RectTransform).DOAnchorPosY(995f, 0.3f).SetEase(Ease.InBack);
    }

    #endregion

    #region Handle

    private void HandleClickEye()
    {
        BlinkEyeVeins();
        HideIntro();
        Invoke("ShowLoginInput", 0.5f);
        //Invoke("ShowNameInput", 0.5f);
    }

    private void HandleOnNameInputChanged(string arg0)
    {
        string newText = null;

        if (arg0.Contains(@"[^0-9a-zA-Z°¡-ÆR]"))
        {
            newText = "Æ¯¼ö¹®ÀÚ¸¦ Æ÷ÇÔÇÒ ¼ö ¾ø½À´Ï´Ù.";
            nameInput.text = arg0.Replace(@"[^0-9a-zA-Z°¡-ÆR]", "");
        }

        for (int i = 0; i < forbiddenWords.Length; i++)
        {
            if (arg0.Contains(forbiddenWords[i]))
            {
                newText = "±ÝÁöµÈ ¾ð¾î°¡ Æ÷ÇÔµÇ¾î ÀÖ½À´Ï´Ù.";
                nameInput.text = newText.Replace(forbiddenWords[i], "");
            }
        }

        if (String.IsNullOrEmpty(newText))
            return;

        string prevText = title.text;
        title.text = newText;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(title.transform.DOScale(1.15f, 0.3f).SetEase(Ease.InBack));
        sequence.Append(title.transform.DOScale(1f, 0.3f).SetEase(Ease.InBack));
        sequence.OnComplete(() =>
        {
            title.text = prevText;
        });
    }

    private void HandleAimPosition(Vector2 vector)
    {
        //Vector2 dir = (eyeScreenPoint - vector).normalized;
        //Vector2 dir = ((Vector2)Camera.main.ScreenToViewportPoint(vector) - eyeScreenPoint).normalized;
        Vector2 input = Camera.main.ScreenToViewportPoint(vector);
        input.y += 0.5f;
        Vector2 dir = new Vector2
            (
                Utill.Remap(input.x, 0f, 1f, minRange, maxRange),
                Utill.Remap(input.y, 0f, 1f, minRange, maxRange)
            );
        eyeMaterial.SetVector("_IrisPos", dir);
    }

    private void HandleOnEnterArrow(PointerEventData data, Transform transform)
    {
        transform.DOScale(1.1f, 0.35f);
    }

    private void HandleOnExitArrow(PointerEventData data, Transform transform)
    {
        transform.DOScale(1f, 0.35f);
    }

    #endregion

    private void ShowErrorMessage(string message)
    {
        Debug.Log(message);
        title.text = message;
        title.fontSize = 55;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(title.rectTransform.DOAnchorPosY(-144f, 0.3f));
        sequence.Insert(0.5f, title.rectTransform.DOShakeAnchorPos(0.5f));
    }

    private void SetEyeVeins(bool EyeVeins)
    {
        DOTween.To(() => eyeMaterial.GetFloat("_EyeVeins"), x => eyeMaterial.SetFloat("_EyeVeins", x), EyeVeins ? 0 : 1, 1.5f);
    }

    private void BlinkEyeVeins()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(() => eyeMaterial.GetFloat("_EyeVeins"), x => eyeMaterial.SetFloat("_EyeVeins", x), 0, 1f));
        sequence.Append(DOTween.To(() => eyeMaterial.GetFloat("_EyeVeins"), x => eyeMaterial.SetFloat("_EyeVeins", x), 1, 1f));
    }

    protected override IEnumerator ActiveSceneRoutine()
    {
        ShowIntro();
        yield return new WaitForSeconds(0.5f);
    }

    protected override IEnumerator DeactiveSceneRoutine()
    {
        HideIntro();
        HideNameInput();
        HideLoginInput();
        (eye.transform as RectTransform).DOAnchorPosY(-700, 0.3f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(0.6f);
    }
}
