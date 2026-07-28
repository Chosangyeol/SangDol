using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_Bishop : MonoBehaviour
{
    public float damagePercent = 0.1f;

    private bool damaging = false;
    private float prog = 0f;
    private CharacterModel model;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            model = other.GetComponent<CharacterModel>();

            if (model != null)
            {
                damaging = true;
                prog = 0f;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            prog += Time.deltaTime;
            
            if (damaging && prog >= 1)
            {
                model.Damaged(damagePercent, true);
                prog = 0f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            damaging = false;
            prog = 0f;
            model = null;
        }
    }

}
