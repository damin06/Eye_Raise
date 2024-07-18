using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D;

public class SoftBodyPhysics : MonoBehaviour
{
    private float splineOffset = 0.5f;

    //[SerializeField] private SpriteMask spriteMask;
    [SerializeField] private SpriteShapeController spriteShape;
    [SerializeField] private Transform[] points;

    private void Update()
    {
        //if (!IsOwner)
        //    return;
        UpdateVerticies();
        UpdateSpriteMask();
    }

    private void UpdateSpriteMask()
    {
        //Sprite _newSprite = Sprite.Create(null, spriteRenderer.);
        //Sprite _newSprite = spriteShape.sp;
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
}
