using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_Bullet : MonoBehaviour
{
    public float speed = 5f;

    private float damage = 0f;
    private CharacterModel _model;

    private bool isActive = true;

    public void Init(float damage,float speed, CharacterModel model, bool isActive)
    {
        this.damage = damage;
        this.speed = speed;
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

        if (!isActive)
        {
            if (other.gameObject.CompareTag("Boss"))
            {
                BossModel _boss = other.GetComponent<D1_FinalBoss>();
                _boss.StartCoroutine(_boss.Counter(10f));
                Destroy(this.gameObject);
            }
        }  
    }
}
