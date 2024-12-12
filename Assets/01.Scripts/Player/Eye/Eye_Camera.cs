using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
public class Eye_Camera : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Transform followTarget;
    [SerializeField] private float followSpeed = 5f;

    [Header("Zoom Settings")]
    [SerializeField] private float minOrthoSize = 10f;
    [SerializeField] private float maxOrthoSize = 50f;

    [Header("Reference")]
    [SerializeField] private MapRange mapRange;

    private float currentOrthoSize = 10f;

    private Eye_Brain eyeBrain;

    private void Awake()
    {
        eyeBrain = GetComponent<Eye_Brain>();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 targetPosition = eyeBrain.GetCenterOfAgents();
        followTarget.position = Vector3.Lerp(
            followTarget.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            CinemachineConfiner2D Confiner = virtualCamera.GetComponentInChildren<CinemachineConfiner2D>();
            if (Confiner != null)
            {
                Confiner.m_BoundingShape2D = mapRange.MapColider;
            }

            virtualCamera.Priority = 15;
            eyeBrain.TotalScore.OnValueChanged += (int prevValue, int newValue) =>
            {
                currentOrthoSize = Mathf.Clamp((float)((double)newValue / (double)150) * 10, minOrthoSize, float.MaxValue);
                AdjustOthoSize(currentOrthoSize);
            };
        }
    }

    private void AdjustOthoSize(float newValue)
    {
        virtualCamera.m_Lens.OrthographicSize = newValue;
    }
}
