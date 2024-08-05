using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Eye_Animation : MonoBehaviour
{
    [SerializeField] private Material material;
    public Material fillMaterial;
    private SpriteShapeRenderer spriteShapeRenderer;

    private void Awake()
    {
        if (transform.Find("Visual").TryGetComponent(out SpriteShapeRenderer _sprite))
        {
            var _newMaterial = Instantiate(material);
            fillMaterial = _newMaterial;
            _sprite.materials[0] = fillMaterial;
            //spriteShapeRenderer = _sprite;
            //_sprite.
        }
    }

    private void Update()
    {
        Debug.Log(fillMaterial.GetFloat("_PupilRadisu"));
        //fillMaterial.SetFloat("_PupilRadisu", 1);
        //Vector2 _vec = _a.GetVector("_IrisPos");
        //Debug.Log(_vec);
    }

    public void MovementAnimation(Vector2 _dir = default)
    {
        CancelInvoke("RePlaceMovementAnimation");
        _dir *= 1.5f;

        //material.GetFloat()
    }

    private void RePlaceMovementAnimation()
    {

    }
}
