using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private float _movementSpeed;

    [Header("EYE")]
    [SerializeField][Min(0.1f)] private float _eyeMoveSpeed = 2f;
    [SerializeField] private float _eyeMaxDist = 0.2f;
    [SerializeField] private Transform _pupilPos;
    [SerializeField] private Transform _irisPos;

    private Vector2 _movementInput;
    private Rigidbody2D _rigidbody2D;


    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _inputReader.MovementEvent += HandleMovement;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        _inputReader.MovementEvent -= HandleMovement;
    }

    private void HandleMovement(Vector2 movementInput)
    {
        _movementInput = movementInput;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        _irisPos.transform.localPosition = Vector3.Lerp(_irisPos.transform.localPosition, _movementInput, Time.deltaTime * _eyeMoveSpeed);
        _rigidbody2D.velocity = _movementInput * _movementSpeed;
    }

    [ServerRpc]
    public void OnHitServerRPC(Vector3 vec)
    {
        _rigidbody2D.AddForce(vec);
    }
}
