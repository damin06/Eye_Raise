using System;
using Unity.Netcode;
using UnityEngine;

public class ShootKnife : NetworkBehaviour
{
    [Header("����������")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _shootPositionTrm; //�̰��� right��������
    [SerializeField] private GameObject _serverKnifePrefab;
    [SerializeField] private GameObject _clientKnifePrefab;
    [SerializeField] private Collider2D _playerCollider;

    //������ �������� �ʿ��ϰ�
    //Ŭ���̾�Ʈ�� �������� �ʿ��ϴ�.
    // �ڱ��ڽŰ� �浹������ ���ؼ� �÷��̾� �ö��̴��� �ʿ��ϴ�.


    [Header("���ð���")]
    [SerializeField] private float _knifeSpeed;
    [SerializeField] private int _knifeDamage;
    [SerializeField] private float _throwCooltime;

    private float _lastThrowTime;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _inputReader.ShootEvent += HandleShootKnife;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        _inputReader.ShootEvent -= HandleShootKnife;
    }

    private void HandleShootKnife()
    {
        //��Ÿ���� ���ƿԴٸ� �߻� �ϴ°ž�
        if (Time.time < _lastThrowTime + _throwCooltime) return;
        //Ŭ�� ���
        Vector3 pos = _shootPositionTrm.position;
        Vector3 direction = _shootPositionTrm.right;
        _lastThrowTime = Time.time;
        SpawnDummyKnife(pos, direction);

        //���� RPC������
        SpawnKnifeServerRpc(pos, direction);

        
        //�ڱ��ڽ��� �ȸ�����ְ� ������ Ŭ��� �����.
        //��� �������� ���鶧 
        //Physics2D.IgnoreCollision �� �̿��ؼ� �ڱ��ڽŰ��� �浹���� �ʰ� �����.
    }

    [ServerRpc]
    private void SpawnKnifeServerRpc(Vector3 pos, Vector3 dir)
    {
        //�� ��� ������Ʈ�� ������ �༮�� id�� �̸��� ��Ÿ��� �˾Ƴ��°ž�.
        UserData user = ServerSingleton.Instance.NetServer.GetUserDataByClientID(OwnerClientId);

        Debug.Log($"{user.username} : launch knife pos: {pos}, dir : {dir}");

        GameObject instance = Instantiate(_serverKnifePrefab, pos, Quaternion.identity);
        instance.transform.right = dir;

        Physics2D.IgnoreCollision(_playerCollider, instance.GetComponent<Collider2D>());

        if(instance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rigidbody))
        {
            rigidbody.velocity = dir * _knifeSpeed;
        }

        if(instance.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact damage))
        {
            damage.SetDamage(_knifeDamage);
            damage.SetOwner(OwnerClientId);
        }


        SpawnDummyKnifeClientRpc(pos, dir);
    }

    [ClientRpc]
    private void SpawnDummyKnifeClientRpc(Vector3 pos, Vector3 dir)
    {
        if (IsOwner) return;
        SpawnDummyKnife(pos, dir);
    }

    private void SpawnDummyKnife(Vector3 pos, Vector3 dir)
    {
        GameObject instance = Instantiate(_clientKnifePrefab, pos, Quaternion.identity);
        instance.transform.right = dir;

        Physics2D.IgnoreCollision(_playerCollider, instance.GetComponent<Collider2D>());

        if (instance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rigidbody))
        {
            rigidbody.velocity = dir * _knifeSpeed;
        }
    }
}
