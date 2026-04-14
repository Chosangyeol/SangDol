using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_PanicBullet : MonoBehaviour
{
    private SBuff sBuff1;
    private SBuff sBuff2;
    private BuffSO panicBuffSO;
    private BuffSO stunBuffSO;
    private float speed;

    public void Init(BuffSO panicBuffSO, BuffSO stunBuffSO, float speed)
    {
        this.panicBuffSO = panicBuffSO;
        this.stunBuffSO = stunBuffSO;
        this.speed = speed;

        StartCoroutine(Shot());
    }

    private IEnumerator Shot()
    {
        float Timer = 0;

        while (Timer < 10f)
        {
            Timer += Time.deltaTime;
            transform.position += transform.forward * speed * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterModel model = other.GetComponent<CharacterModel>();

            if (model != null )
            {
                sBuff1 = new SBuff(
                    this.gameObject,
                    model.gameObject,
                    new PanicDeBuff(model, panicBuffSO, 5f)
                    );

                sBuff2 = new SBuff(
                    this.gameObject,
                    model.gameObject,
                    new StunDeBuff(model, stunBuffSO, 0.5f)
                    );

                model.Buff.AddBuff(sBuff1);
                model.Buff.AddBuff(sBuff2);

                Destroy(gameObject);
            }
        }
    }
}
