using QFSW.QC;
using System;
using Unity.Netcode;
using UnityEngine;

public class Eye_Agent : NetworkBehaviour
{
    [Header("Status")]
    [SerializeField] private Color eyeColor = Color.black;
    private NetworkVariable<Vector2> movementInput = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private Rigidbody2D rb;
    private Eye_Animation eyeAnimation;
    private Eye_Physics eyePhysics;

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
        eyeBrain = transform.GetComponentInParent<Eye_Brain>();

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
            if (eyeBrain != null && Vector2.Distance(eyeBrain.mainAgent.transform.position, transform.position) > 10)
                return;

                //rb.velocity = movementInput.Value;
            rb.AddForce(movementInput.Value, ForceMode2D.Force);
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
            rb.mass = (float)((double)newValue / (double)1000);
        }

        if (IsServer)
        {
            if(eyeBrain == null)
            {
                eyeBrain = transform.GetComponentInParent<Eye_Brain>();
            }
            Debug.Log($"is Brain is null? {eyeBrain == null}");
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
