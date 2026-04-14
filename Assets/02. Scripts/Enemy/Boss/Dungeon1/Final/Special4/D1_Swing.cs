using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_Swing : MonoBehaviour
{
    public float speed;
    public CharacterModel model;
    public bool isActive = false;

    public void Init()
    {
        StartCoroutine(Swing());
    }

    private void Update()
    {
        if (isActive && model != null)
            model.transform.position = transform.position;
    }

    private IEnumerator Swing()
    {
        float Timer = 0f;

        while (Timer < 6f)
        {
            Timer += Time.deltaTime;
            transform.position += transform.forward * speed * Time.deltaTime;
            yield return null;
        }

        if (model != null && isActive)
        {
            isActive = false;
            model.SetCanMove();
            model.Navmesh.enabled = true;
        }

        GetComponent<BoxCollider>().enabled = false;

        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            model = other.GetComponent<CharacterModel>();

            if (model != null && !isActive && model.canMove)
            {
                model.canMove = false;
                isActive = true;
                model.PlayerController.StopMove();

                model.Navmesh.enabled = false;
            }
        }
    }
}
