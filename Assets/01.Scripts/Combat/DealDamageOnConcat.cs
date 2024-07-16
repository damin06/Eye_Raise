using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    private int _damage;
    private ulong _ownerClientID;  //���� ų�� ����, ������ �����

    public void SetDamage(int knifeDamage)
    {
        _damage = knifeDamage;
    }

    public void SetOwner(ulong ownerClientId)
    {
        _ownerClientID = ownerClientId;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null) return;

        if (collision.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            //Debug.Log($"{collision.gameObject.name} is hit");
            health.TakeDamage(_damage, _ownerClientID);
        }
    }

}
