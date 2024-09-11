using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColiderBridge : MonoBehaviour
{
    [SerializeField] private UnityEvent<Collider2D> OnTriggerEnter;
    [SerializeField] private UnityEvent<Collision2D> OnCollisionEnter;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
