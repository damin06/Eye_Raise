using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Eye_Agent : NetworkBehaviour
{
    [Header("Status")]
    [SerializeField] private float moveSpeed = 3f;

    private Rigidbody2D rb;
    private Eye_Animation eyeAnimation;
    private Eye_Physics eyePhysics;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        eyeAnimation = GetComponent<Eye_Animation>();
        eyePhysics = GetComponent<Eye_Physics>();
    }

    void Update()
    {
    }

    [Command]
    public void Move(Vector2 dir)
    {
        rb.velocity = dir.normalized * moveSpeed;
        eyeAnimation.InputMovementAnimation(dir);
    }

    [Command]
    public void SetScale(float newScale)
    {
        transform.localScale = new Vector2(newScale, newScale);
        eyePhysics.RePlaceCircles();
    }
}
