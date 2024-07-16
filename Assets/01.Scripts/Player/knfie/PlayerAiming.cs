using System;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    public NetworkVariable<int> _curDir = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private Transform _handTrm;
    private float _maxRotateSpeed = 180;
    public NetworkVariable<float> _rotateSpeed = new NetworkVariable<float>(180, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float curZRoate;
    private PlayerAnimation _playerAnimation;
    private Knife _Knife;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;
        _Knife.knifeScale.OnValueChanged += (float previousValue, float newValue) =>
        {
            _rotateSpeed.Value = _maxRotateSpeed -= newValue * 17;
        };
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
            return;
        _Knife.knifeScale.OnValueChanged -= (float previousValue, float newValue) =>
        {
            _rotateSpeed.Value = _maxRotateSpeed -= newValue * 17;
        };
    }

    private void Awake()
    {
        _playerAnimation = GetComponent<PlayerAnimation>();
        _Knife = _handTrm.GetComponentInChildren<Knife>();
    }

    public void ChangeDir()
    {
        if (!IsServer)
            return;
        _playerAnimation.AttackAnimationClientRpc();
        _curDir.Value *= -1;
    }
    
    private void Update()
    {
        if (!IsOwner) return;

        curZRoate += Time.deltaTime;

        //_handTrm.transform.right = new Vector3(0, 0, transform.rotation.z + Time.deltaTime);
        _handTrm.transform.rotation = Quaternion.Euler(0, 0, _rotateSpeed.Value * curZRoate * _curDir.Value);
        if (curZRoate * _rotateSpeed.Value >= 360)
            curZRoate = 0;
    }
}
