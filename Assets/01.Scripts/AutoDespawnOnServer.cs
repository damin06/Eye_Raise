using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AutoDespawnOnServer : NetworkBehaviour
{
    [SerializeField] private float lifeTime = 3f;
    void Start()
    {
        if(IsServer)
        Invoke("OnDestroy", lifeTime); 
    }

    private void OnDestroy()
    {
        GetComponent<NetworkObject>().Despawn(true);
        Destroy(gameObject);
    }
}
