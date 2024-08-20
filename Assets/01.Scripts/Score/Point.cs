using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Point : NetworkBehaviour
{
    public NetworkVariable<int> point = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isUserPoint = new NetworkVariable<bool>();
    public NetworkVariable<Color> color = new NetworkVariable<Color>(Random.ColorHSV());

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            color.OnValueChanged += HandleColorChanged;
        }

        if (IsServer)
        {
            isUserPoint.OnValueChanged += HandlePointOwnerChanged;
        }
    }

    private void HandlePointOwnerChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            transform.localScale = Vector3.one;
        }
        else
        {
            float newScale = Random.Range(0.6f, 0.75f);
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }

    private void HandleColorChanged(Color previousValue, Color newValue)
    {
        if(TryGetComponent(out SpriteRenderer _sprite))
        {
            _sprite.color = newValue;
        }
    }

    public void OnEnable()
    {
        //color.Value = Random.ColorHSV(0f, 1f, 0.9f, 1f, 0.9f, 1f);
        //point.Value = Random.Range(1, 6);


        if (IsServer)
        {
            HandlePointOwnerChanged(isUserPoint.Value, isUserPoint.Value);
        }
    }

    //public void SetUserPoint(int newPoint, Color newColor)
    //{
    //    point.Value = newPoint;
    //    isUserPoint.Value = true;
    //    color.Value = newColor;
    //}
}
