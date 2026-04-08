using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_Box : MonoBehaviour
{
    [Header("깜짝 박스 설정")]
    public BuffSO stunDeBuffSO;
    public SphereCollider SphereCollider;
    public GameObject spawnObject;
    public GameObject activeObject;

    private SBuff sBuff;
    private CharacterModel _model;

    private bool isActive = false;


    public void Init(CharacterModel model)
    {
        _model = model;

        sBuff = new SBuff(
            this.gameObject,
            _model.gameObject,
            new StunDeBuff(_model, stunDeBuffSO, 1.5f)
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

            if (_model == null)
            {
                _model = other.gameObject.GetComponent<CharacterModel>();
                Init(_model);
            }
            
            _model.Buff.AddBuff(sBuff);
            SphereCollider.enabled = false;
            Destroy(this.gameObject, 2f);
        }

    }
}
