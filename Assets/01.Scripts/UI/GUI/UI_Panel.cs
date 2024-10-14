using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class UI_Panel : UI_Base
{
    protected override void Init()
    {
        base.Init();

        Bind<Image>();
        Bind<TextMeshProUGUI>();
        Bind<TMP_InputField>();
        Bind<Button>();
        Bind<Slider>();
        Bind<RawImage>();
        Bind<ScrollRect>();
    }

    protected virtual void OnActive() { }
    protected virtual void OnDeactive() { }


    public delegate void SceneRoutineCallback();

    SceneRoutineCallback SceneActiveCallback;
    SceneRoutineCallback SceneDeactiveCallback;

    public virtual void ActiveWithMotion(SceneRoutineCallback callback = default)
    {
        SceneActiveCallback = callback;
        gameObject.SetActive(true);
        StartCoroutine(ActiveSceneRoutine());
    }

    public virtual void DeactiveWithMotion(SceneRoutineCallback callback = default)
    {
        SceneDeactiveCallback = callback;
        StartCoroutine(DeactiveSceneRoutine());
    }

    /// <summary>
    /// yield return StartCoroutine(base.ActiveSceneRoutine()); must be added at the end.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator ActiveSceneRoutine()
    {
        if (SceneActiveCallback != null)
            SceneActiveCallback();
        yield return null;
    }

    /// <summary>
    /// yield return StartCoroutine(base.DeactiveSceneRoutine()); must be added at the end.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator DeactiveSceneRoutine()
    {
        if (SceneDeactiveCallback != null)
            SceneDeactiveCallback();

        gameObject.SetActive(false);
        yield return null;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        OnActive();
    }

    protected virtual void OnDisable()
    {
        OnDeactive();
    }
}
