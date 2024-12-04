using System.Threading.Tasks;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

/// <summary>
/// UI 패널의 기본 클래스입니다.
/// 화면에 표시되는 개별 UI 패널의 기능을 정의합니다.
/// </summary>
public abstract class UI_Panel : UI_Base
{
    #region Initialization
    /// <summary>
    /// 패널의 기본 UI 컴포넌트들을 초기화합니다.
    /// </summary>
    protected override void Init()
    {
        base.Init();

        // 기본 UI 컴포넌트들을 바인딩
        Bind<Image>();
        Bind<TextMeshProUGUI>();
        Bind<TMP_InputField>();
        Bind<Button>();
        Bind<Slider>();
        Bind<RawImage>();
        Bind<ScrollRect>();
    }
    #endregion

    #region Panel Lifecycle
    /// <summary>
    /// 패널이 활성화될 때 호출됩니다.
    /// </summary>
    protected virtual void OnActive() { }

    /// <summary>
    /// 패널이 비활성화될 때 호출됩니다.
    /// </summary>
    protected virtual void OnDeactive() { }

    protected override void OnEnable()
    {
        base.OnEnable();
        OnActive();
    }

    protected virtual void OnDisable()
    {
        OnDeactive();
    }
    #endregion

    #region Animation Control
    /// <summary>
    /// 패널을 애니메이션과 함께 활성화합니다.
    /// </summary>
    public async Task ActiveWithMotion()
    {
        gameObject.SetActive(true);
        await ActiveSceneRoutine().ToTask(this);
    }

    /// <summary>
    /// 패널을 애니메이션과 함께 비활성화합니다.
    /// </summary>
    public async Task DeactiveWithMotion()
    {
        await DeactiveSceneRoutine().ToTask(this);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 패널 활성화 애니메이션을 정의합니다.
    /// </summary>
    protected abstract IEnumerator ActiveSceneRoutine();

    /// <summary>
    /// 패널 비활성화 애니메이션을 정의합니다.
    /// </summary>
    protected abstract IEnumerator DeactiveSceneRoutine();
    #endregion
}
