using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeManager : MonoBehaviour
{
    public static VolumeManager Instance;

    private Volume volume;

    private Vignette vignette;
    public Vignette Vignette => vignette;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        volume = GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
    }
}
