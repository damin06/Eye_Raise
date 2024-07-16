using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Coin : NetworkBehaviour
{
    private NetworkVariable<int> _coinSocre = new NetworkVariable<int>(1,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private ParticleSystem _particleSystem;
    public UnityEvent OnDie;

    private void Awake()
    {
        _particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    public int GetCoinScoreS()
    {
        return _coinSocre.Value;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;

        if (collision.TryGetComponent<Knife>(out Knife knife))
        {
            knife.AddknifeSocre(_coinSocre.Value);
            StartCoroutine(OnHitCo());
            OnDie?.Invoke();
        }
    }

    private IEnumerator OnHitCo()
    {
        _particleSystem.Play();
        yield return new WaitForSeconds(1.1f);
        GetComponent<NetworkObject>().Despawn(true);
        Destroy(gameObject);
    }
}
