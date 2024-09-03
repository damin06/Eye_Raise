using QFSW.QC;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Eye_Agent : NetworkBehaviour
{
    [Header("Status")]
    [SerializeField] private Color eyeColor = Color.black;
    private NetworkVariable<Vector2> movementInput = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Rigidbody2D rb = default;
    private Eye_Animation eyeAnimation = default;
    private Eye_Physics eyePhysics = default;

    public NetworkVariable<int> score = new NetworkVariable<int>();
    private Eye_Brain eyeBrain = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        eyeAnimation = GetComponent<Eye_Animation>();
        eyePhysics = GetComponent<Eye_Physics>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"is Brain is null? {eyeBrain == null}");

        score.OnValueChanged += HandleScoreChanged;
    }

    public override void OnNetworkDespawn()
    {
        score.OnValueChanged -= HandleScoreChanged;
    }

    public void Init()
    {
        eyeBrain = transform.GetComponentInParent<Eye_Brain>();
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
        //Debug.Log($"NewScale {newScale}");
        
        transform.localScale = new Vector3(newScale, newScale, newScale);
        eyePhysics.RePlaceCircles();
    }

    private void HandleScoreChanged(int previousValue, int newValue)
    {
        if (IsOwner)
        {
            float newScale = (float)((double)newValue / (double)100);
            SetScale(newScale);
        }

        if (IsServer)
        {
            if(eyeBrain == null)
            {
                eyeBrain = transform.GetComponentInParent<Eye_Brain>();
            }

            eyeBrain.ModifySocre(NetworkObjectId, newValue);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;

        if(collision.TryGetComponent(out Point _point))
        {
            Debug.Log(_point.name + "Hit!" + _point.point.Value.ToString());
            score.Value += _point.point.Value;

            ScoreManager.Instance.ReturnPoint(collision.GetComponent<NetworkObject>());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
