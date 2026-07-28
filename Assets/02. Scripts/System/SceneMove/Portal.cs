using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public string targetSceneName;
    public string targetPointName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneChanger.instance.LoadScene(targetSceneName, targetPointName);
        }
    }
}
