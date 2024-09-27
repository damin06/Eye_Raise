using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Eye_Camera : NetworkBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cam;
    [SerializeField] private Transform followTarget;
    [SerializeField] private float minFov = 1f;
    [SerializeField] private float maxFov = 5f;
    private float maxOthoSize = 10;

    private Eye_Brain eyeBrain;

    private void Awake()
    {
        eyeBrain = GetComponent<Eye_Brain>();
    }

    private void LateUpdate()
    {
        followTarget.transform.position = eyeBrain.GetCenterOfAgents();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) 
        {
            cam.Priority = 15;
            eyeBrain.TotalScore.OnValueChanged += (int prevValue, int newValue) =>
            {
                maxOthoSize = (float)((double)newValue / (double)150) * 10;
                AdjustOthoSize(newValue);
            };
        }
    }

    private void AdjustOthoSize(int newValue)
    {
        cam.m_Lens.OrthographicSize = (float)((double)newValue / (double)150) * 10;
    }
}
