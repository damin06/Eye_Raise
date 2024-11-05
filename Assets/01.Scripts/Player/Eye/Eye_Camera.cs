using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
public class Eye_Camera : NetworkBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cam;
    [SerializeField] private Transform followTarget;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float minFov = 1f;
    [SerializeField] private float maxFov = 5f;
    [SerializeField] private float minOthoSize = 10;
    private float currentOthoSize = 10;

    private Eye_Brain eyeBrain;

    private void Awake()
    {
        eyeBrain = GetComponent<Eye_Brain>();
    }

    private void FixedUpdate()
    {
        Vector3 targetPosition = eyeBrain.GetCenterOfAgents(); // 목표 위치
        followTarget.transform.position = Vector3.Lerp(followTarget.transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) 
        {
            cam.Priority = 15;
            eyeBrain.TotalScore.OnValueChanged += (int prevValue, int newValue) =>
            {
                currentOthoSize = Mathf.Clamp((float)((double)newValue / (double)150) * 10, minOthoSize, float.MaxValue);
                AdjustOthoSize(newValue);
            };
        }
    }

    private void AdjustOthoSize(int newValue)
    {
        cam.m_Lens.OrthographicSize = newValue;
    }
}
