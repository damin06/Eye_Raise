using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [Header("참조형 변수들")]
    [SerializeField] private Transform _barTrm;

    public void HandleHealthChanged(int oldHealth, int newHealth, float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1f);
        _barTrm.localScale = new Vector3(ratio, 1, 1);
    }
}
