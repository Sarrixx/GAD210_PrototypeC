using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenVolume : MonoBehaviour
{
    [SerializeField] private bool depleteOxygen = false;
    [SerializeField] private Transform respawnPoint;

    private static PlayerController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") == true)
        {
            if (other.TryGetComponent(out OxygenHandler oxygen) == true)
            {
                oxygen.ToggleDepleting(depleteOxygen);
                oxygen.SetSpawnTarget(new OxygenHandler.OxygenDepletedDelegate(RespawnPlayer));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player") == true)
        {
            if (other.TryGetComponent(out OxygenHandler oxygen) == true)
            {
                oxygen.ToggleDepleting(!depleteOxygen);
            }
            if (controller == null)
            {
                other.TryGetComponent(out controller);
            }
        }
    }

    private void RespawnPlayer()
    {
        if(controller != null && respawnPoint != null)
        {
            controller.TeleportToPosition(respawnPoint.position);
        }
    }
}
