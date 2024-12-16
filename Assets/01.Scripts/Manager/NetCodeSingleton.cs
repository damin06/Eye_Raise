using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 네트워크 싱글톤을 구현하는 추상 클래스입니다.
/// NetworkBehaviour를 상속받아 네트워크 기능을 제공합니다.
/// </summary>
/// <typeparam name="T">싱글톤으로 구현할 클래스 타입</typeparam>
public abstract class NetCodeSingleton<T> : NetworkBehaviour where T : Component
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    GameObject go = new GameObject($"[{typeof(T).Name}]");
                    instance = go.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (instance == null)
        {
            instance = this as T;
            //DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
