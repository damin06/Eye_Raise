using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Eye_Animation : MonoBehaviour
{
    [SerializeField] private Material material;
    private Material fillMaterial;

    private void Awake()
    {
        if (transform.Find("Visual").TryGetComponent(out SpriteShapeRenderer _sprite))
        {
            _sprite.materials[0] = Instantiate(material);
            fillMaterial = _sprite.materials[0];
        }
    }

    private void Start()
    {
        //Debug.Log(spriteShapeRenderer.GetComponent<Renderer>().materials[0].name);
    }

    private void Update()
    {
        //Debug.Log(spriteShapeRenderer.materials[1]);
        //Debug.Log(fillMaterial.GetFloat("_PupilRadisu"));
        //fillMaterial.SetFloat("_PupilRadisu", 1);
        //Debug.Log(_vec);


    }

    public void MovementAnimation(Vector2 _dir = default)
    {
        CancelInvoke("RePlaceMovementAnimation");
        _dir *= 1.5f;

        //fillMaterial.get
        //InvokeRepeating("RePlaceMovementAnimation", 0, Time.deltaTime);
    }

    private void RePlaceMovementAnimation()
    {

    }
}
