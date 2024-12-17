using DG.Tweening;
using QFSW.QC;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Eye_Agent : NetworkBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private Eye_Animation eyeAnimation;
    private Eye_Physics eyePhysics;
    private Eye_Brain eyeBrain;

    [Header("Network Variables")]
    public NetworkVariable<int> score = new NetworkVariable<int>();
    private NetworkVariable<float> creationTime = new NetworkVariable<float>();

    [Header("UI")]
    [SerializeField] private TextMeshPro nameLabel;
    [SerializeField] private TMP_FontAsset font;

    [Header("Reference")]
    [SerializeField] private MapRange mapRange;
    private PolygonCollider2D mapColider;

    public bool CanMerge => creationTime.Value + eyeBrain.MergeableTime < Time.time;
    private NetworkVariable<Vector2> movementInput = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        eyeAnimation = GetComponent<Eye_Animation>();
        eyePhysics = GetComponent<Eye_Physics>();
    }

    public override async void OnNetworkSpawn()
    {
        score.OnValueChanged += HandleScoreChanged;

        if (IsServer)
        {
            creationTime.Value = Time.time;
        }


        if (IsClient)
        {
            await new WaitUntil(() => (eyeBrain = transform.GetComponentInParent<Eye_Brain>()) != null);
        }

        if (IsOwner)
        {
            mapColider = GameManager.Instance.MapColider;
        }
    }

    private void HandleEyeColorChanged(Color previousValue, Color newValue)
    {
        Log.Message($"{eyeBrain.name}'s Colror {newValue}");
        eyeAnimation.SetEyeColor(newValue);
    }

    public override void OnNetworkDespawn()
    {
        score.OnValueChanged -= HandleScoreChanged;

        if (IsClient)
        {
            //eyeBrain.username.OnValueChanged -= HandleNameChanged;
            //eyeBrain.eyeColor.OnValueChanged -= HandleEyeColorChanged;
        }
    }

    private void FixedUpdate()
    {
        if (IsClient)
        {
            eyeAnimation.InputMovementAnimationClientRpc(movementInput.Value.normalized);
        }

        if (IsOwner)
        {

            // Vector2 _viewport = Camera.main.WorldToViewportPoint(transform.position);
            //if (_viewport.x > 1)
            //    _input.x = 1;
            //if (_viewport.x < 0)
            //    _input.x = 0;
            //if (_viewport.y > 1)
            //    _input.y = 1;
            //if (_viewport.y < 0)
            //    _input.y = 0;


            //if (eyeBrain != null && Vector2.Distance(eyeBrain.mainAgent.transform.position, transform.position) > 10)
            //    return;

            //rb.velocity = movementInput.Value;
            rb.AddForce(ClampMovementToBounds(movementInput.Value), ForceMode2D.Force);
        }
    }

    private Vector2 ClampMovementToBounds(Vector2 input)
    {
        if(transform.position.x >= mapRange.MaxSpawnPos.x && input.x > 0)
        {
            input.x = 0;
        }

        if(transform.position.x <= mapRange.MinSpawnPos.x && input.x < 0) 
        {
            input.x = 0;
        }

        if(transform.position.y >= mapRange.MinSpawnPos.y && input.y > 0)
        {
            input.y = 0;
        }

        if(transform.position.y <= mapRange.MaxSpawnPos.y && input.y < 0)
        {
            input.y = 0;
        }

        return input;
    }

    public void SetNameLabel(string name)
    {
        nameLabel.text = name;
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
        transform.DOScale(new Vector3(newScale, newScale, newScale), 0.3f);
        eyePhysics.RePlaceCircles();
    }

    private void HandleScoreChanged(int previousValue, int newValue)
    {
        if (IsOwner)
        {
            float newScale = (float)((double)newValue / (double)120);
            SetScale(newScale);
            rb.mass = (float)((double)newValue / (double)530);
        }

        if (IsServer)
        {
            if (eyeBrain == null)
            {
                eyeBrain = transform.GetComponentInParent<Eye_Brain>();
            }

            eyeBrain.ModifySocre(NetworkObjectId, newValue);
        }
    }

    private void MergeAgent(Eye_Agent agent)
    {
        if (agent.OwnerClientId != OwnerClientId)
        {
            eyeBrain.LastHitDealerID = agent.OwnerClientId;
        }

        agent.eyeBrain.RemoveAgent(agent.NetworkObjectId);
        score.Value += agent.score.Value;
        agent.NetworkObject.Despawn(true);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;

        if (collision.TryGetComponent(out Point _point))
        {
            Debug.Log(_point.name + "Hit!" + _point.Score.Value.ToString());
            score.Value += _point.Score.Value;

            ScoreManager.Instance.ReturnPoint(collision.GetComponent<NetworkObject>());

            if (GameManager.Instance.ServerId != _point.OwnerClientId && _point.OwnerClientId != OwnerClientId)
            {
                eyeBrain.CloseEye(5f);
                Log.Message($"{_point.OwnerClientId}, {OwnerClientId}");
            }
        }

        if (collision.TryGetComponent(out Eye_Agent _agent))
        {
            if (_agent.score.Value > score.Value)
                return;

            if (_agent.OwnerClientId != OwnerClientId)
            {
                Debug.Log($"{_agent.OwnerClientId} Is Deady By {OwnerClientId}");
                MergeAgent(_agent);
            }
            else if (CanMerge && _agent.CanMerge && _agent.OwnerClientId == OwnerClientId)
            {
                MergeAgent(_agent);
                Debug.Log($"{_agent.OwnerClientId} Is Merged!");
            }
        }
    }
}
