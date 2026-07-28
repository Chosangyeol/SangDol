using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elder_N3_Center : MonoBehaviour
{

    [Header("데미지 판정")]
    public float damagePercent = 0.2f;
    public float tick = 0.5f;
    public float timer = 0f;
    public bool isActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterModel>(out CharacterModel player))
        {
            isActive = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActive) return;

        if (timer < 0.5f)
        {
            timer += Time.deltaTime;
            return;
        }

        if (other.TryGetComponent<CharacterModel>(out CharacterModel player))
        {
            player.Damaged(damagePercent, true);
            timer = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CharacterModel>(out CharacterModel player))
        {
            isActive = false;
            timer = 0f;
        }
    }
}
