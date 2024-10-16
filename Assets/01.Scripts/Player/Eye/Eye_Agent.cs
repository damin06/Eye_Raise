using QFSW.QC;
using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Eye_Agent : NetworkBehaviour
{
    [Header("Reference")]
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private TextMeshPro nameLabel;
    private NetworkVariable<Vector2> movementInput = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> creationTime = new NetworkVariable<float>();

    private Rigidbody2D rb;
    private Eye_Animation eyeAnimation;
    private Eye_Physics eyePhysics;

    public NetworkVariable<int> score = new NetworkVariable<int>();
    private Eye_Brain eyeBrain = null;
    public bool canMerge => creationTime.Value + eyeBrain.MergeableTime < Time.time;

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

        if (IsServer)
        {
            creationTime.Value = Time.time;
        }

        if (IsClient)
        {
            //nameLabel.font = Instantiate(font);
            //nameLabel.font.material.SetColor(ShaderUtilities.ID_OutlineColor, eyeBrain.eyeColor.Value);
            eyeBrain.username.OnValueChanged += HandleNameChanged;
            eyeBrain.eyeColor.OnValueChanged += HandleEyeColorChanged;

            HandleNameChanged("", eyeBrain.username.Value);
            HandleEyeColorChanged(Color.blue, eyeBrain.eyeColor.Value);
        }
    }

    private void HandleEyeColorChanged(Color previousValue, Color newValue)
    {
        eyeAnimation.SetEyeColor(newValue);
    }

    public override void OnNetworkDespawn()
    {
        score.OnValueChanged -= HandleScoreChanged;

        if (IsClient)
        {
            eyeBrain.username.OnValueChanged -= HandleNameChanged;
            eyeBrain.eyeColor.OnValueChanged -= HandleEyeColorChanged;
        }
        Debug.Log($"{NetworkObjectId} is Destroyed!");
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

        if (IsOwner)
        {
            Vector2 _viewport = Camera.main.WorldToViewportPoint(transform.position);
            Vector2 _input = movementInput.Value;

            //float t = CalculateMappedValue(eyeBrain.Fov, eyeBrain.minFov, eyeBrain.maxFov, 0, 1f);

            if (_viewport.x > 1)
                _input.x = 1;
            if (_viewport.x < 0)
                _input.x = 0;
            if (_viewport.y > 1)
                _input.y = 1;
            if (_viewport.y < 0)
                _input.y = 0;


            //if (eyeBrain != null && Vector2.Distance(eyeBrain.mainAgent.transform.position, transform.position) > 10)
            //    return;

            //rb.velocity = movementInput.Value;
            rb.AddForce(_input, ForceMode2D.Force);
        }
    }

    private float CalculateMappedValue(float input, float minInput, float maxInput, float minOutput, float maxOutput)
    {
        float normalized = (input - minInput) / (maxInput - minInput);
        return Mathf.Lerp(maxOutput, minOutput, normalized);
    }

    private void HandleNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        nameLabel.text = newValue.ToString();
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
        transform.localScale = new Vector3(newScale, newScale, newScale);
        eyePhysics.RePlaceCircles();
    }

    private void HandleScoreChanged(int previousValue, int newValue)
    {
        if (IsOwner)
        {
            float newScale = (float)((double)newValue / (double)100);
            SetScale(newScale);
            rb.mass = (float)((double)newValue / (double) 500);
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

    private void MergeAgent(Eye_Agent agent)
    {
        if(agent.OwnerClientId != OwnerClientId)
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

        if(collision.TryGetComponent(out Point _point))
        {
            Debug.Log(_point.name + "Hit!" + _point.point.Value.ToString());
            score.Value += _point.point.Value;

            ScoreManager.Instance.ReturnPoint(collision.GetComponent<NetworkObject>());
        }
        
        if(collision.TryGetComponent(out Eye_Agent _agent))
        {
            if (_agent.score.Value > score.Value)
                return;

            if(_agent.OwnerClientId != OwnerClientId)
            {
                Debug.Log($"{_agent.OwnerClientId} Is Deady By {OwnerClientId}");
                MergeAgent(_agent);
            }
            else if(canMerge && _agent.canMerge && _agent.OwnerClientId == OwnerClientId)
            {
                MergeAgent(_agent);
                Debug.Log($"{_agent.OwnerClientId} Is Merged!");
            }
        }
    }
}
