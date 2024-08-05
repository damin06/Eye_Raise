using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePlacement : MonoBehaviour
{
    public float radius = 5.0f;

    private void Awake()
    {
        PlaceObjectsInCircle();
    }

    [ContextMenu("RePlaceCricle")]
    private void PlaceObjectsInCircle()
    {
        Transform _circles = transform.Find("Circles");

        for (int i = 0; i < _circles.childCount; i++)
        {
            float _angle = i * Mathf.PI * 2 / _circles.childCount;
            
            float _x = Mathf.Cos(_angle) * radius;
            float _y = Mathf.Sin(_angle) * radius;
            
            Vector3 _position = new Vector3(_x,_y, 0);
            _circles.GetChild(i).transform.localPosition = _position;

            foreach(SpringJoint2D _spring in _circles.GetComponents<SpringJoint2D>())
            {
                //_spring.distance = _angle * transform.localScale.x;
                //_spring.autoConfigureConnectedAnchor = true;
                //_spring.autoConfigureDistance = true;

                //_spring.autoConfigureConnectedAnchor = false;
                //_spring.autoConfigureDistance = false;
            }
        }
    }
}
