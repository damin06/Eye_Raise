using UnityEngine;

public class KnifeBase : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 2;
    private float _currentLifeTime;
    private Knife Knife;

    private void Awake()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        _currentLifeTime += Time.deltaTime;
        if(_currentLifeTime >= _lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
