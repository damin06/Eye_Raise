using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


[RequireComponent(typeof(Volume))]
public class VolumeManager : NetCodeSingleton<VolumeManager>
{

    private Volume volume;
    private Vignette vignette;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        base.Awake();
        volume = GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
    }

    public void SetEyeClosure(float closureAmount)
    {
        vignette.intensity.value = closureAmount;
    }

    public void AnimateEyeClosure(float targetAmount, float duration)
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            return;
        }

        blinkCoroutine = StartCoroutine(EyeClosureRoutine(targetAmount, duration));
    }

    private IEnumerator EyeClosureRoutine(float targetAmount, float duration)
    {
        float startAmount = vignette.intensity.value;
        float elapsed = 0f;
        float t;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            t = elapsed / duration;
            vignette.intensity.value = Mathf.Lerp(startAmount, targetAmount, t);
            yield return null;
        }
        vignette.intensity.value = targetAmount;
    }
}
