using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D;

public class Eye_Animation : NetworkBehaviour
{
    [SerializeField] private Material material;
    private Material fillMaterial;

    [Header("pupil")]
    [SerializeField] private float pupilmoveSpeed = 2f;
    [SerializeField] private float pupilmoveDistance = 0.15f;

    [Space]

    [Header("Blink")]
    [SerializeField] private float BlinkSpeed = 4.5f;
    [SerializeField] private float minBlinkDelay = 2f;
    [SerializeField] private float maxBlinkDelay = 5f;

    private float blinkDelay = 0;
    private Vector2 moveDir = Vector2.zero;

    private void Awake()
    {
        if (transform.Find("Visual").TryGetComponent(out SpriteShapeRenderer _sprite))
        {
            _sprite.materials[0] = Instantiate(material);
            fillMaterial = _sprite.materials[0];
        }

        //if(TryGetComponent(out SpriteRenderer _renderer))
        //{
        //    _renderer.materials[0] = Instantiate(material);
        //    fillMaterial = _renderer.materials[0];
        //}
    }

    public override void OnNetworkSpawn()
    {
        

    }

    private void Update()
    {
        RePlaceMovementAnimation();
    }

    [Command("SetEyeColor")]
    public void SetEyeColor(Color _newColor)
    {
        fillMaterial.SetColor("_IrisColor", _newColor);
    }

    [ClientRpc]
    public void InputMovementAnimationClientRpc(Vector2 _dir)
    {
        _dir *= pupilmoveDistance;

        moveDir = _dir;
    }

    private void ResetMovementAnimation()
    {
        moveDir = Vector2.zero;
    }

    Vector3 elid;
    public void SetEyelid(float _eyelidValue)
    {
        elid = fillMaterial.GetVector("_EyelidScale");
        elid.y = _eyelidValue;
        fillMaterial.SetVector("_EyelidScale", elid);
    }

    private void RePlaceMovementAnimation()
    {
        Vector2 irisPos = (Vector2)fillMaterial.GetVector("_IrisPos");

        if (moveDir + irisPos == Vector2.zero)
            return;

        Vector2 learp = Vector2.Lerp(irisPos, moveDir, Time.deltaTime * pupilmoveSpeed);
        learp.x = Mathf.Clamp(learp.x, -pupilmoveDistance, pupilmoveDistance);
        learp.y = Mathf.Clamp(learp.y, -pupilmoveDistance, pupilmoveDistance);


        fillMaterial.SetVector("_IrisPos", learp);

        if ((learp - moveDir).magnitude < 0.0001f)
        {
            if (moveDir != Vector2.zero)
            {
                moveDir = Vector2.zero;
            }
            else
            {
                //fillMaterial.SetVector("_IrisPos", Vector2.zero);
            }

        }
    }
}
