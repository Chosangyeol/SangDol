using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_Knife : MonoBehaviour
{
    public float damage = 0f;
    public float multi = 0f;
    public float speed = 5f;
    public LayerMask wallMask;

    public void Init(float damage, float multi)
    {
        this.damage = damage;
        this.multi = multi;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self); ;
    }

}
