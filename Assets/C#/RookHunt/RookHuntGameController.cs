using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class RookHuntGameController : MonoBehaviour
{
    [SerializeField] private GameObject MenuGO;
    [SerializeField] private GameObject[] MapsPF;
    [SerializeField] public WayCreator[] Ways;
    [NonSerialized] private bool IsKaliWayExist;
    [SerializeField] private List<GameObject> EnemyPF;
    [SerializeField] private List<GameObject> SpecialEnemyPF;
    [SerializeField] private float SpawnSpeed;
    [SerializeField] public double Shoots;
    [SerializeField] public int KillStreak;
    [SerializeField] public int Score;
    [SerializeField] private GameObject CavLaughsGO;
    [Header("UI")]
    [SerializeField] public Image[] BulletsImg;
    [SerializeField] public TextMeshProUGUI MultiplierText;
    [SerializeField] public TextMeshProUGUI ScoreText;
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
            IsKaliWayExist = Ways[a].KaliWay;
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
            GameObject _EnemyGO = Instantiate(EnemyPF[UnityEngine.Random.Range(0, EnemyPF.Count)], Ways[wayID].transform.position, Quaternion.identity);

            Enemy _EnemyCS = _EnemyGO.GetComponent<Enemy>();
            _EnemyCS.StartCoroutine(_EnemyCS.Animation());
            _EnemyCS._WayCreator = Ways[wayID];
        }
        else
        {
            int enemyID = UnityEngine.Random.Range(0, SpecialEnemyPF.Count); 
            int wayID = UnityEngine.Random.Range(enemyID == 0 ? (IsKaliWayExist == true ? Ways.Length - 1 : 0) : 0, Ways.Length - (IsKaliWayExist? 1 : 0));
            GameObject _EnemyGO = Instantiate(SpecialEnemyPF[enemyID], Ways[wayID].transform.position, Quaternion.identity);

            Enemy _EnemyCS = _EnemyGO.GetComponent<Enemy>();
            _EnemyCS._WayCreator = Ways[wayID];
        }

        yield return new WaitForSeconds(SpawnSpeed);
        StartCoroutine(EnemySpawn());
    }
    #endregion

    #region UpdateValues
    public void StatUpdate()
    {
        MagazineUpdate();
        MultiplierText.text = KillStreak + " STREAK (" + (1 + KillStreak * 0.2f) + "x Multiplier)";
        ScoreText.text = Score.ToString();
    }

    public void MagazineUpdate()
    {
        Shoots = math.clamp(Shoots, -3, 1);
        for (int i = 0; i < BulletsImg.Length; i++)
        {
            BulletsImg[i].fillAmount = i + (float)Shoots;
        }

        if (Shoots == -3)
        {
            StopAllCoroutines();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                go.GetComponentInParent<Enemy>().Speed = 0;
                go.GetComponentInParent<Enemy>().StopAllCoroutines();
            }
            CavLaughsGO.transform.localPosition = new Vector3(0,600,-50);
            CavLaughsGO.SetActive(true);
        }
    }
    #endregion
}