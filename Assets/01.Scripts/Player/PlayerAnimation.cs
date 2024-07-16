using Unity.Netcode;
using UnityEngine;
//using MoreMountains.Feedbacks;

public class PlayerAnimation : NetworkBehaviour
{
    private Animator _animator;
    //private MMF_Player _MMF_Player;
    [SerializeField]private GameObject _postProcess;

    private readonly string Eye_Attack = "Attack";
    private readonly string Eye_Hurt = "Hurt";
    private readonly string Eye_Close50 = "50";
    private readonly string Eye_Close20 = "20";

    public override void OnNetworkSpawn()
    {
      
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        //_MMF_Player = GetComponentInChildren<MMF_Player>();
    }

    [ServerRpc]
    public void AttackAnimationServerRpc()
    {
        _animator.SetTrigger(Eye_Attack);
    }

    [ClientRpc]
    public void AttackAnimationClientRpc()
    {
        _animator.SetTrigger(Eye_Attack);
        if(IsOwner)
            AttackAnimationServerRpc();
    }

    [ServerRpc]
    public void HurtAnimationServerRpc()
    {
        _animator.SetTrigger(Eye_Hurt);
    }

    [ClientRpc]
    public void HurtAnimationClientRpc()
    {
        _animator.SetTrigger(Eye_Hurt);
        //if (IsOwner)
            //_MMF_Player?.PlayFeedbacks();
        if (IsOwner)
            HurtAnimationServerRpc();
    }

    [ServerRpc]
    public void ChangeEyeCloseServerRpc()
    {
        _animator.SetBool(Eye_Close20, true);
        _animator.SetBool(Eye_Close50, true);
        ChangeEyeCloseClientRpc();
    }

    [ClientRpc]
    public void ChangeEyeCloseClientRpc()
    {
        _animator.SetBool(Eye_Close20, true);
        _animator.SetBool(Eye_Close50, true);
        if (IsOwner)
            _postProcess.gameObject.SetActive(true);
    }
}