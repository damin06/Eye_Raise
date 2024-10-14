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
using Util;

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
    private TMP_InputField nameInput;
    private TextMeshProUGUI title;

    [SerializeField] private string[] forbiddenWords;
    [SerializeField] ArrowPos[] arrowPos;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Material eyeMaterial;
    [SerializeField] private float minRange = -0.15f;
    [SerializeField] private float maxRange = 0.15f;
    private Vector2 eyeScreenPoint;

    protected override void Init()
    {
        base.Init();
        
        eye = Get<Button>("Button_Eye");
        nameInput = Get<TMP_InputField>("InputField_Name");
        title = Get<TextMeshProUGUI>("TMP_Title");

        nameInput.characterLimit = 10;
        nameInput.onValueChanged.AddListener(HandleOnNameInputChanged);

        inputReader.AimPositionEvent += HandleAimPosition;
        eyeScreenPoint = Camera.main.ScreenToViewportPoint(RectTransformUtility.WorldToScreenPoint(Camera.main, eye.transform.position));

        for(int i = 1; i <= 4; i++)
        {
            Image _arrow = Get<Image>($"Image_Arrow{i}");
            BindEvent(_arrow.gameObject, HandleOnEnterArrow, Define.ClickType.Enter);
            BindEvent(_arrow.gameObject, HandleOnExitArrow, Define.ClickType.Exit);
        }

        title.text = "OH MY\nEYES!";
        title.fontSize = 196f;
        BindEvent(title.gameObject, HandleOnEnterArrow, Define.ClickType.Enter);
        BindEvent(title.gameObject, HandleOnExitArrow, Define.ClickType.Exit);

        eye.onClick.AddListener(HandleClickEye);
        eyeMaterial.SetFloat("_EyeVeins", 1);
    }




    private void SetEyeVeins(bool EyeVeins)
    {
        DOTween.To(() => eyeMaterial.GetFloat("_EyeVeins"), x => eyeMaterial.SetFloat("_EyeVeins", x), EyeVeins ? 0 : 1, 1.5f);
    }

    private void BlinkEyeVeins()
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(DOTween.To(() => eyeMaterial.GetFloat("_EyeVeins"), x => eyeMaterial.SetFloat("_EyeVeins", x), 1, 1f));
        sequence.Append(DOTween.To(() => eyeMaterial.GetFloat("_EyeVeins"), x => eyeMaterial.SetFloat("_EyeVeins", x), 0, 1f));
    }

    float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return from2 + (value - from1) * (to2 - from2) / (to1 - from1);
    }


    private void HideIntro()
    {
        for (int i = 0; i < arrowPos.Length; i++)
        {
            Get<Image>(arrowPos[i].ObjectName).rectTransform.DOAnchorPos(arrowPos[i].EndPos, 0.4f).SetEase(Ease.InBack);
        }
        Get<TextMeshProUGUI>("TMP_Title").rectTransform.DOAnchorPosY(287f, 0.4f).SetEase(Ease.InBack);
    }

    private void ShowNameInput()
    {
        for (int i = 0; i < arrowPos.Length; i++)
        {
            Get<Image>(arrowPos[i].ObjectName).rectTransform.DOAnchorPos(arrowPos[i].OriginPos, 0.4f).SetEase(Ease.Linear);
        }

        title.text = "Enter your name!";
        title.fontSize = 161f;
        title.rectTransform.DOAnchorPosY(-242f, 0.4f).SetEase(Ease.InBack);
    }

    #region Handle

    private void HandleClickEye()
    {
        BlinkEyeVeins();
        HideIntro();
        Invoke("ShowNameInput", 0.5f);
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
                Remap(input.x, 0f, 1f, minRange, maxRange),
                Remap(input.y, 0f, 1f, minRange, maxRange)
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
}
