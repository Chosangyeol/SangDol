using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_Bullet : MonoBehaviour
{
    public float speed = 5f;

    private float damage = 0f;
    private CharacterModel _model;

    private bool isActive = true;

    public void Init(float damage, CharacterModel model, bool isActive)
    {
        this.damage = damage;
        _model = model;
        this.isActive = isActive;
    }

    private void Start()
    {
        Destroy(gameObject,4f);
    }    

    private void Update()
    {
        UpdatePos();


    }

    private void UpdatePos()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!isActive) return;

            if (_model == null) return;

            _model.Damaged(damage, true);

            Destroy(this.gameObject);
        }
    }
}
