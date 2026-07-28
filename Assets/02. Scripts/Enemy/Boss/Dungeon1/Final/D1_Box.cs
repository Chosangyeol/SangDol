using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_Box : MonoBehaviour
{
    [Header("깜짝 박스 설정")]
    public SphereCollider SphereCollider;
    public GameObject spawnObject;
    public GameObject activeObject;

    private BuffSO stunDeBuffSO;
    private SBuff sBuff;
    private float stunDuration;
    private CharacterModel _model;

    private bool isActive = false;


    public void Init(CharacterModel model, BuffSO debuffSO, float stunDuration)
    {
        _model = model;
        stunDeBuffSO = debuffSO;
        this.stunDuration = stunDuration;

        sBuff = new SBuff(
            this.gameObject,
            _model.gameObject,
            new StunDeBuff(_model, stunDeBuffSO, stunDuration)
            );
        StartCoroutine(Invisable());
    }

    IEnumerator Invisable()
    {
        spawnObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        spawnObject.SetActive(false);

        isActive = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.gameObject.CompareTag("Player"))
        {
            activeObject.SetActive(true);

            AudioManager.instance.PlaySFX(C_Enums.SFX_List.D1_Final_Box);

            _model.Buff.AddBuff(sBuff);
            SphereCollider.enabled = false;
            Destroy(this.gameObject, 2f);
        }

    }
}
