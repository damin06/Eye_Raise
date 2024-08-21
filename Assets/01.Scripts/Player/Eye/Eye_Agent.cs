using QFSW.QC;
using System;
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

    public NetworkVariable<int> score = new NetworkVariable<int>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        eyeAnimation = GetComponent<Eye_Animation>();
        eyePhysics = GetComponent<Eye_Physics>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            score.OnValueChanged += HandleScoreChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner) 
        {
            score.OnValueChanged -= HandleScoreChanged;
        }
    }

    public void Init()
    {
        
    }

    private void Update()
    {
        if (IsClient)
        {
            eyeAnimation.InputMovementAnimation(movementInput.Value.normalized);
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            rb.velocity = movementInput.Value;
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
    public void SetScale(float newScale)
    {
        //newScale = Mathf.Clamp(newScale, 1, int.MaxValue);
        Debug.Log($"NewScale {newScale}");
        
        transform.localScale = new Vector3(newScale, newScale, newScale);
        eyePhysics.RePlaceCircles();
    }

    private void HandleScoreChanged(int previousValue, int newValue)
    {
        float newScale = (float)((double)newValue / (double)100);
        SetScale(newScale);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;

        if(collision.TryGetComponent(out Point _point))
        {
            Debug.Log(_point.name + "Hit!" + _point.point.Value.ToString());
            score.Value += _point.point.Value;

            ScoreManager.Instance.ReturnPoint(_point.GetComponent<NetworkObject>());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
