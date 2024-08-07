using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Eye_Agent : NetworkBehaviour
{
    [Header("Status")]
    //[SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Color eyeColor = Color.black;
    private Vector2 movementInput = Vector2.zero;

    private Rigidbody2D rb = default;
    private Eye_Animation eyeAnimation = default;
    private Eye_Physics eyePhysics = default;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        eyeAnimation = GetComponent<Eye_Animation>();
        eyePhysics = GetComponent<Eye_Physics>();
    }

    public void Init()
    {
        
    }

    private void Update()
    {
        rb.velocity = movementInput;
        eyeAnimation.InputMovementAnimation(movementInput);

    }

    [Command]
    public void SetEyeColor(Color newColor)
    {
        eyeAnimation.SetEyeColor(newColor);
    }

    [Command("AgentMove")]
    public void MoveInput(Vector2 dir)
    {
        movementInput = dir;
    }

    [Command]
    public void SetScale(float newScale)
    {
        transform.localScale = new Vector2(newScale, newScale);
        eyePhysics.RePlaceCircles();
    }
}
