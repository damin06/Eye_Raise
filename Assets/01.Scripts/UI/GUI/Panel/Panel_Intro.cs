using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class Panel_Intro : UI_Panel
{
    [SerializeField] private InputReader inputReader;
    private Image eye => Get<Image>("Image_Eye");
    private Vector2 eyeScreenPoint;
    [SerializeField] private Material eyeMaterial;
    [SerializeField] private float minRange = -0.15f;
    [SerializeField] private float maxRange = 0.15f;

    protected override void Init()
    {
        base.Init();

        inputReader.AimPositionEvent += HandleAimPosition;

        eyeScreenPoint = Camera.main.ScreenToViewportPoint(RectTransformUtility.WorldToScreenPoint(Camera.main, eye.transform.position));

        for(int i = 1; i <= 4; i++)
        {
            Image _arrow = Get<Image>($"Image_Arrow{i}");
            BindEvent(_arrow.gameObject, HandleOnEnterArrow, Define.ClickType.Enter);
            BindEvent(_arrow.gameObject, HandleOnExitArrow, Define.ClickType.Exit);
        }

        BindEvent(eye.gameObject, HandleClickEye, Define.ClickType.Click);
        eyeMaterial.SetFloat("EyeVeins", 1);
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

    private void SetEyeVeins(bool EyeVeins)
    {
        DOTween.To(() => eyeMaterial.GetFloat("_EyeVeins"), x => eyeMaterial.SetFloat("_EyeVeins", x), EyeVeins ? 0 : 1, 1.5f);
    }

    float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return from2 + (value - from1) * (to2 - from2) / (to1 - from1);
    }

    private void HandleClickEye(PointerEventData data, Transform transform)
    {
        SetEyeVeins(true);
    }

    private void HandleOnEnterArrow(PointerEventData data, Transform transform)
    {
        transform.DOScale(1.1f, 0.5f);
    }

    private void HandleOnExitArrow(PointerEventData data, Transform transform)
    {
        transform.DOScale(1f, 0.5f);
    }
}
