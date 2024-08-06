using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Eye_Animation : MonoBehaviour
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
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        MovementAnimation(dir);
        RePlaceMovementAnimation();

        if (blinkDelay <= 0)
        {
            blinkDelay = 0;
            Blink();
        }
        else
        {
            blinkDelay -= Time.deltaTime;
        }
    }

    public void MovementAnimation(Vector2 _dir)
    {
        _dir *= pupilmoveDistance;

        moveDir = _dir; 
    }

    float curValue = 0;
    private void Blink()
    {
        curValue += Time.deltaTime * Random.Range(BlinkSpeed - 0.5f, BlinkSpeed + 0.5f);

        float curEyelidValue = 1 + Mathf.Cos(curValue);
        Vector3 elid = fillMaterial.GetVector("_EyelidScale");
        elid.y = curEyelidValue;
        fillMaterial.SetVector("_EyelidScale", elid);

        if(curValue >= Mathf.PI * 2)
        {
            blinkDelay = Random.Range(minBlinkDelay, maxBlinkDelay);
            curValue = 0;
        }
        
    }

    private void MovementAnimtation()
    {

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

        if ((learp - moveDir).magnitude < 0.001f)
        {
            if (moveDir != Vector2.zero)
            {
                moveDir = Vector2.zero;
            }
            else
            {
                fillMaterial.SetVector("_IrisPos", Vector2.zero);
            }

        }
    }
}
