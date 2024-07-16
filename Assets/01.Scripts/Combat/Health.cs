using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using QFSW.QC;

public class Health : NetworkBehaviour
{
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth;
    public UnityEvent<int, int, float> OnHealthChanged;

    private bool _isDead = false;
    
    public Action<Health> OnDie;

    public ulong LastHitDealerID { get; private set; }

    [SerializeField] private GameObject coinPrefab;
    private PlayerAnimation _playerAnimation;

    private void Awake()
    {
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            currentHealth.OnValueChanged += HandleChangeHealth;
            HandleChangeHealth(0, maxHealth); //처음 시작
        }

        if (!IsServer) return;
        currentHealth.Value = maxHealth; //서버만
        OnDie += SpawnCoin;
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            currentHealth.OnValueChanged -= HandleChangeHealth;
        }
    }

    private void HandleChangeHealth(int prev, int newValue)
    {
        OnHealthChanged?.Invoke(prev, newValue, (float)newValue / maxHealth);
    }

    public void TakeDamage(int damageValue, ulong dealerID)
    {
        LastHitDealerID = dealerID;
        ModifyHealth(-damageValue);
        DebugCurHealthClientRpc();
        _playerAnimation.HurtAnimationClientRpc();
    }

    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    public void ModifyHealth(int value)
    {
        if (_isDead) return;

        currentHealth.Value = Mathf.Clamp(currentHealth.Value + value, 0, maxHealth);

        if(currentHealth.Value == 0)
        {
            OnDie?.Invoke(this);
            _isDead = true;
        }
    }

    [Command("DebugHealth")]
    [ClientRpc]
    private void DebugCurHealthClientRpc()
    {
        if (!IsOwner)
            return;
        Debug.Log($"{GetComponent<Player>().GetUserName()} : CurHealth = {currentHealth.Value}");
    }

    private void SpawnCoin(Health health)
    {
        GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
        coin.GetComponent<NetworkObject>().Spawn(true);
    }
}
