using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elder_N2_Stone : MonoBehaviour
{
    [Header("피해량")]
    public float damage = 0.2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterModel>(out CharacterModel model))
        {
            model.Damaged(damage, true);
        }
    }
}
