using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetcodeUtill 
{
    private static NetcodeUtill instance = null;
    public static NetcodeUtill Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new NetcodeUtill();
            }

            return instance;
        }
    }

    private Dictionary<string, object> objects = new Dictionary<string, object>();

    public void SendValue<T>(string key, T value, ulong clientID)
    {
        ReceiveValueRpc(key, (object)value, NetworkManager.Singleton.RpcTarget.Single(clientID, RpcTargetUse.Temp));
    }


    [Rpc(SendTo.SpecifiedInParams)]
    private void ReceiveValueRpc(string key, object value, RpcParams rpcParams)
    {
        objects[key] = value;
    }
    
    public T GetValue<T>(string key)
    {
        if (objects.ContainsKey(key))
        {
            return (T)objects[key];
        }
        else
        {
            Debug.LogWarning($"The value of {key} does not exist");
            return default;
        }
    }
}
