using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elder_N3_Orb : MonoBehaviour
{
    [Header("데미지")]
    public float damagePercent = 0.2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterModel>(out var character))
        {
            character.Damaged(damagePercent, true);
        }
    }

}
