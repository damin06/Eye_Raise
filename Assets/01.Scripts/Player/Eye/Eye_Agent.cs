using DG.Tweening;
using Mono.CSharp.yyParser;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

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

    public bool CanMerge => creationTime.Value + eyeBrain.MergeableTime < Time.time;
    private Vector2 movementInput;

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
            await new WaitUntil(() => transform.GetComponentInParent<Eye_Brain>() != null);

            eyeBrain = transform.GetComponentInParent<Eye_Brain>();

            eyeBrain.username.OnValueChanged += HandleNameChanged;
            eyeBrain.eyeColor.OnValueChanged += HandleEyeColorChanged;

            HandleNameChanged("", eyeBrain.username.Value);
            HandleEyeColorChanged(eyeBrain.eyeColor.Value, eyeBrain.eyeColor.Value);
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
            //eyeBrain.username.OnValueChanged -= HandleNameChanged;
            //eyeBrain.eyeColor.OnValueChanged -= HandleEyeColorChanged;
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            eyeAnimation.InputMovementAnimationClientRpc(movementInput.normalized);

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
            rb.AddForce(movementInput, ForceMode2D.Force);
        }
    }

    private void HandleNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        nameLabel.text = newValue.ToString();
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
        movementInput = dir;
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
