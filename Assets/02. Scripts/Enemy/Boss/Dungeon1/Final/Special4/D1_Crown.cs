using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class D1_Crown : MonoBehaviour
{
    private GameObject bulletPrefab;
    private float bulletSpeed;
    private BuffSO panicBuffSO;
    private BuffSO stunBuffSO;

    public CharacterModel target;

    public void Init(GameObject bullet,float bulletSpeed, BuffSO panicBuffSO
        , BuffSO stunBuffSO, CharacterModel target)
    {
        this.bulletPrefab = bullet;
        this.bulletSpeed = bulletSpeed;
        this.panicBuffSO = panicBuffSO;
        this.stunBuffSO = stunBuffSO;
        this.target = target;

        StartCoroutine(ShotPanicBullet());
    }

    private IEnumerator ShotPanicBullet()
    {
        while (transform.position.y > -0.5f)
        {
            transform.position += Vector3.down * 20f * Time.deltaTime;
            yield return null;
        }

        float timer = 0f;

        while (timer < 2f)
        {

            timer += Time.deltaTime;
            Vector3 lookTarget = target.transform.position;
            lookTarget.y = 0f;
            transform.LookAt(lookTarget);

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        bullet.GetComponent<D1_PanicBullet>().Init(panicBuffSO, stunBuffSO, bulletSpeed);
        yield return new WaitForSeconds(0.5f);

        while (transform.position.y < 20f)
        {
            transform.position += Vector3.up * 20f * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

}
