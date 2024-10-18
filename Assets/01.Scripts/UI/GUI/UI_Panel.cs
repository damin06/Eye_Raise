using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Mono.CSharp.yyParser;

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

    public async Task ActiveWithMotion()
    {
        gameObject.SetActive(true);
        await ActiveSceneRoutine().ToTask(this);
    }

    public async Task DeactiveWithMotion()
    {
        await DeactiveSceneRoutine().ToTask(this);
        gameObject.SetActive(false);
    }

    protected abstract IEnumerator ActiveSceneRoutine();

    protected abstract IEnumerator DeactiveSceneRoutine();

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
