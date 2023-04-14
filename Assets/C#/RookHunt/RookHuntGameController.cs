using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class RookHuntGameController : MonoBehaviour
{
    [SerializeField] private GameObject MenuGO;
    [SerializeField] private GameObject[] MapsPF;
    [SerializeField] private WayCreator[] Ways;
    [SerializeField] private bool IsKaliWayExist;
    [SerializeField] private Dictionary<string, WayCreator> WaysKey;
    [SerializeField] private GameObject[] EnemyPF;
    [SerializeField] private GameObject[] SpecialEnemyPF;
    [SerializeField] private float SpawnSpeed;
    [SerializeField] public double Shoots;

    [Header("UI")]
    [SerializeField] public GameObject[] ShootsUI;


    [Header("Ranked")]
    [SerializeField] private GameObject[] Enemies;
    [SerializeField] private int Round;


    private void Start()
    {
        MenuGO.SetActive(true);
    }

    #region Ranked
    public void RankedGameStart()
    {

    }
    #endregion

    #region Infinite Mode
    public void InfiniteModeStart()
    {
        //Instantiate(MapsPF[UnityEngine.Random.Range(1, 1)], new Vector3(50, 0, 0), Quaternion.identity);
        #region FindAllSpawns
        int a = 0;
        GameObject[] _GO = GameObject.FindGameObjectsWithTag("Spawn");
        Ways = new WayCreator[_GO.Length];
        foreach (GameObject obj in _GO)
        {
            Ways[a] = obj.GetComponent<WayCreator>();
            a++;
        }
        #endregion
        MenuGO.SetActive(false);
        StartCoroutine(EnemySpawn());
    }

    private IEnumerator EnemySpawn()
    {
        if (UnityEngine.Random.Range(0, 100) < 80)
        {
            int wayID = UnityEngine.Random.Range(0, Ways.Length - (IsKaliWayExist? 1 : 0));
            GameObject _EnemyGO = Instantiate(EnemyPF[UnityEngine.Random.Range(0, EnemyPF.Length)], Ways[wayID].transform.position, Quaternion.identity);

            Enemy _EnemyCS = _EnemyGO.GetComponent<Enemy>();
            _EnemyCS.StartCoroutine(_EnemyCS.Animation());
            _EnemyCS._WayCreator = Ways[wayID];
        }
        else
        {
            int enemyID = UnityEngine.Random.Range(0, SpecialEnemyPF.Length); 
            int wayID = UnityEngine.Random.Range(enemyID == 0 ? (IsKaliWayExist == true ? Ways.Length - 1 : 0) : 0, Ways.Length - (IsKaliWayExist? 1 : 0));
            GameObject _EnemyGO = Instantiate(SpecialEnemyPF[enemyID], Ways[wayID].transform.position, Quaternion.identity);

            Enemy _EnemyCS = _EnemyGO.GetComponent<Enemy>();
            _EnemyCS.StartCoroutine(_EnemyCS.Animation());
            _EnemyCS._WayCreator = Ways[wayID];
        }

        yield return new WaitForSeconds(SpawnSpeed);
        StartCoroutine(EnemySpawn());
    }
    #endregion

    public void MagazineUpdate()
    {

    }
}