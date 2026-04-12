using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_Hat : BossModel
{
    public GameObject redPrefab;
    public GameObject purplePrefab;

    public GameObject spawnChip;

    bool isReal = false;

    private D1_Yabawe parent;

    protected override void Start()
    {
        base.Start();

        normalPatterns.Add(new D1_King_Stand());
    }

    public void Init(bool isReal, D1_Yabawe parent)
    {
        this.parent = parent;

        if (isReal)
        {
            spawnChip = Instantiate(redPrefab, transform.position, Quaternion.identity);
            StartCoroutine(DropChip(spawnChip));
            this.isReal = true;
        }
        else
        {
            spawnChip = Instantiate(purplePrefab, transform.position, Quaternion.identity);
            StartCoroutine(DropChip(spawnChip));
            this.isReal = false;
        }
    }

    public void DropHatStart()
    {
        StartCoroutine(DropHat());
    }

    private IEnumerator DropHat()
    {
        while (transform.position.y > 0)
        {
            transform.position += Vector3.down * 40f * Time.deltaTime;
            yield return null;
        }

        Vector3 lastPos = transform.position;
        lastPos.y = 0;

        transform.position = lastPos;

        spawnChip.transform.parent = transform;
    }

    private IEnumerator DropChip(GameObject chip)
    {
        while (chip.transform.position.y > 0)
        {
            chip.transform.position += Vector3.down * 40f * Time.deltaTime;
            yield return null;
        }

        Vector3 lastPos = chip.transform.position;
        lastPos.y = 0;

        chip.transform.position = lastPos;
    }

    protected override void Die(GameObject source = null)
    {
        base.Die(source);

        spawnChip.transform.parent = null;

        StartCoroutine(CheckResult());

    }

    private IEnumerator CheckResult()
    {
        while (transform.position.y < 5)
        {
            transform.position += Vector3.up * 40 * Time.deltaTime;
            yield return null;
        }

        if (isReal)
            parent.Success();
        else
            parent.Fail();
    }
}

