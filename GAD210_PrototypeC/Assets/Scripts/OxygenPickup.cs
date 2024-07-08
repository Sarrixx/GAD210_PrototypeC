using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class OxygenPickup : MonoBehaviour
{
    [SerializeField] [Range(0.001f, 1f)] private float oxygenSurplus = 0.25f;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") == true && other.TryGetComponent(out OxygenHandler oxygen) == true)
        {
            oxygen.AddOxygen(oxygenSurplus);
            gameObject.SetActive(false);
        }
    }
}
