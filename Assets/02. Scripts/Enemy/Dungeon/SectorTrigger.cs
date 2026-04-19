using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorTrigger : MonoBehaviour
{
    public SectorController targetSector;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetSector.ActivateSector();
            gameObject.SetActive(false);
        }
    }
}
