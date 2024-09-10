using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D;

public class Eye_Physics : MonoBehaviour
{
    [SerializeField] private float circleRadius = 1f;
    private float splineOffset = 0.5f;

    //[SerializeField] private SpriteMask spriteMask;
    [SerializeField] private SpriteShapeController spriteShape;
    [SerializeField] private Transform[] points;

    private void Awake()
    {
        RePlaceCircles();
    }

    private void Update()
    {
        //UpdateVerticies();
    }


    private void UpdateVerticies()
    {
        for(int i = 0; i < points.Length - 1; i++)
        {
            Vector2 _vertex = points[i].localPosition;

            Vector2 _towardCenter = (Vector2.zero - _vertex).normalized;

            float _coliderRadius = points[i].gameObject.GetComponent<CircleCollider2D>().radius;

            try
            {
                spriteShape.spline.SetPosition(i, (_vertex - _towardCenter * _coliderRadius));
            }
            catch
            {
                spriteShape.spline.SetPosition(i, (_vertex - _towardCenter * (_coliderRadius + splineOffset)));
            }

            Vector2 _lt = spriteShape.spline.GetLeftTangent(i);

            Vector2 _newRt = Vector2.Perpendicular(_towardCenter) * _lt.magnitude;
            Vector2 _newLt = Vector2.zero - (_newRt);

            spriteShape.spline.SetRightTangent(i, _newRt);
            spriteShape.spline.SetLeftTangent(i, _newLt  );
        }
    }

    [ContextMenu("RePlaceCricle")]
    public void RePlaceCircles()
    {
        Transform _circles = transform.Find("Circles");

        for (int i = 0; i < _circles.childCount; i++)
        {
            float _angle = i * Mathf.PI * 2 / _circles.childCount;

            float _x = Mathf.Cos(_angle) * circleRadius;
            float _y = Mathf.Sin(_angle) * circleRadius;

            Vector3 _position = new Vector3(_x, _y, 0);
            _circles.GetChild(i).transform.localPosition = _position;

            if (_circles.GetChild(i).TryGetComponent(out DistanceJoint2D _joint))
            {
                _joint.distance = transform.localScale.x;
            }
        }

        UpdateVerticies();
    }
}
