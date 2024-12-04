using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Point : NetworkBehaviour
{
    public NetworkVariable<int> Score = new NetworkVariable<int>();
    public NetworkVariable<ulong> PointOwner = new NetworkVariable<ulong>();
    public NetworkVariable<Color> PointColor = new NetworkVariable<Color>(Color.blue);

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            PointColor.OnValueChanged += HandleColorChanged;
            HandleColorChanged(PointColor.Value, PointColor.Value);
        }

        if (IsServer)
        {

        }
    }



    private void HandleColorChanged(Color previousValue, Color newValue)
    {
        if (TryGetComponent(out SpriteRenderer _sprite))
        {
            _sprite.color = newValue;
        }
    }

    [ClientRpc]
    public void ActiveClientRpc(bool active)
    {
        gameObject.SetActive(active);
    }

    public void OnEnable()
    {
        //color.Value = Random.ColorHSV(0f, 1f, 0.9f, 1f, 0.9f, 1f);
        //point.Value = Random.Range(1, 6);


        //if (IsServer)
        //{
        //    HandlePointOwnerChanged(isUserPoint.Value, isUserPoint.Value);
        //}
    }

    //public void SetUserPoint(int newPoint, Color newColor)
    //{
    //    point.Value = newPoint;
    //    isUserPoint.Value = true;
    //    color.Value = newColor;
    //}
}
