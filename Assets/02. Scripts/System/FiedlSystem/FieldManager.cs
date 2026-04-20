using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    public static FieldManager instance;

    [Header("몬스터 스포너 설정")]
    [SerializeField] List<EnemySpawner> spawners;
    [SerializeField] float limitDistance = 20f;

    private CharacterModel _model;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);


    }

    private void Start()
    {
        _model = FindAnyObjectByType<CharacterModel>();

        StartCoroutine(CheckSpawnerActivate());
    }

    private IEnumerator CheckSpawnerActivate()
    {
        var wait = new WaitForSeconds(0.5f);

        while (true)
        {
            if (_model == null) yield return null;

            foreach (var spawner in spawners)
            {
                float dis = Vector3.Distance(
                    new Vector3(spawner.transform.position.x, 0, spawner.transform.position.z),
                    new Vector3(_model.transform.position.x, 0, _model.transform.position.z)
                );

                // 거리 조건에 따른 활성화/비활성화 제어
                if (dis <= limitDistance)
                {
                    spawner.ActiveSpawner();
                }
                else if (dis >= limitDistance + 5f)
                {
                    spawner.DeactiveSpawner();
                }
            }
            yield return wait;
        }
    }
}
