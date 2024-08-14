using QFSW.QC;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;

public class Eye_Agent : NetworkBehaviour
{
    [Header("Status")]
    //[SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Color eyeColor = Color.black;
    //private Vector2 movementInput = Vector2.zero;
    private NetworkVariable<Vector2> movementInput = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Rigidbody2D rb = default;
    private Eye_Animation eyeAnimation = default;
    private Eye_Physics eyePhysics = default;

    private NetworkVariable<int> score = new NetworkVariable<int>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        eyeAnimation = GetComponent<Eye_Animation>();
        eyePhysics = GetComponent<Eye_Physics>();
    }

    public override void OnNetworkSpawn()
    {
        score.OnValueChanged += HandleScoreChanged;
        SetScale(1);
    }

    public override void OnNetworkDespawn()
    {
        score.OnValueChanged -= HandleScoreChanged;
    }

    public void Init()
    {
        
    }

    private void Update()
    {
        if (IsOwner)
        {
            rb.velocity = movementInput.Value;
        }

        if (IsClient)
        {
            eyeAnimation.InputMovementAnimation(movementInput.Value.normalized);
        }
    }

    [Command]
    public void SetEyeColor(Color newColor)
    {
        eyeAnimation.SetEyeColor(newColor);
    }

    [Command("AgentMove")]
    public void MoveInput(Vector2 dir)
    {
        movementInput.Value = dir;
    }

    [Command]
    public void SetScale(int newScale)
    {
        newScale = Mathf.Clamp(newScale, 1, int.MaxValue);
        transform.localScale = new Vector2(newScale, newScale);
        eyePhysics.RePlaceCircles();
    }

    private void HandleScoreChanged(int previousValue, int newValue)
    {
        SetScale(newValue / 10);
    }
}
