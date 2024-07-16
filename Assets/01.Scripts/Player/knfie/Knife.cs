using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Knife : NetworkBehaviour
{
    [SerializeField] private PlayerAnimation _playerAnimation;
    [SerializeField] private PlayerAiming _playerAiming;
    [SerializeField] private int _damage = 20;
    public NetworkVariable<int> knifeSocre = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> knifeScale = new NetworkVariable<float>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private float stretchFactor = 1f;

    public void AddknifeSocre(int socre)
    {
        ModifyKnifeSocre(socre);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;
        knifeSocre.OnValueChanged += (int previousValue, int newValue) => 
        {
            RankBoardBehaviour.Instance.onUserScoreChanged?.Invoke(OwnerClientId, newValue);
        };
    }

    public override void OnNetworkDespawn()
    {
        knifeSocre.OnValueChanged -= (int previousValue, int newValue) =>
        {
            RankBoardBehaviour.Instance.onUserScoreChanged?.Invoke(OwnerClientId, newValue);
        };
    }

    private void Update()
    {
        if (!IsOwner)
            return;
        transform.localRotation = Quaternion.Euler(0, 0, -90);
        //transform.rotation = Quaternion.Euler(0, 0, -90);

    }

    private void ModifyKnifeSocre(int socre)
    {
        knifeSocre.Value += socre;
        SetKnifeScaleClientRpc();
    }

    [ServerRpc]
    private void SetScaleValueServerRpc(float scale)
    {
        knifeScale.Value = scale;
    }

    [ClientRpc]
    private void SetKnifeScaleClientRpc()
    {
        if (!IsOwner)
            return;

        Vector3 currentScale = transform.localScale;
        Vector3 currentPosition = transform.position;

        currentScale.x = Mathf.Clamp(currentScale.x + stretchFactor, 1, 5);
        currentScale.y = Mathf.Clamp(currentScale.y + stretchFactor, 1, 5);

        currentPosition.y += (currentScale.y - transform.localScale.y) * 0.5f;

        if (currentScale.x >= 2.5f)
            _playerAnimation.ChangeEyeCloseServerRpc();

        transform.localScale = currentScale;
        transform.position = currentPosition;

        //transform.rotation = Quaternion.Euler(0, 0, -90);
        SetScaleValueServerRpc(currentScale.x);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;

        if (collision.TryGetComponent<Health>(out Health health))
            health.TakeDamage(_damage, OwnerClientId);

        if (collision.TryGetComponent<Knife>(out Knife knife))
        {
            knife._playerAiming.ChangeDir();
            DebugToClientRpc(OwnerClientId, knife.OwnerClientId, knife.GetComponentInParent<Player>().GetUserName());
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (!IsServer)
    //        return;
    //    if (collision.attachedRigidbody.TryGetComponent<Health>(out Health health))
    //        health.TakeDamage(_damage, OwnerClientId);

    //    if (collision.TryGetComponent<Knife>(out Knife knife))
    //    {
    //        knife._playerAiming.ChangeDir();
    //        DebugToClientRpc(OwnerClientId, knife.OwnerClientId, knife.GetComponentInParent<Player>().GetUserName());
    //    }

    //    if(collision.attachedRigidbody.TryGetComponent<PlayerMovement>(out PlayerMovement player))
    //    {
    //        //Vector3 inVec = collision.GetContacts(0)
    //    }
    //}

    [ClientRpc]
    private void DebugToClientRpc(ulong owner,ulong hit, FixedString32Bytes hitName)
    {
        if (OwnerClientId != owner || OwnerClientId != hit)
            return;

        Debug.Log($"{GetComponentInParent<Player>().GetUserName()} -> {hitName}");
    }
}
