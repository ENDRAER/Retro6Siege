using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class RookHuntGameController : MonoBehaviour
{
    [SerializeField] public GameObject MenuGO;
    [NonSerialized] public GameObject MapGO;
    [NonSerialized] private MapScript MapCS;
    [NonSerialized] public WayCreator[] Ways;
    [NonSerialized] private bool IsKaliWayExist;
    [SerializeField] private GameObject[] MapsPF;
    [SerializeField] private List<GameObject> EnemyPF;
    [SerializeField] private List<GameObject> SpecialEnemyPF;
    [SerializeField] public List<GameObject> Enemies;
    [SerializeField] private float SpawnSpeed;
    [SerializeField] public double Shoots;
    [SerializeField] public int KillStreak;
    [SerializeField] public int Score;
    [SerializeField] public GameObject CavLaughsGO;
    [SerializeField] public Animator CavLaughsAnimator;
    [SerializeField] public bool GameStarted;
    [SerializeField] private bool GameOver;
    [Header("UI")]
    [SerializeField] public Image[] BulletsImg;
    [SerializeField] public TextMeshProUGUI MultiplierText;
    [SerializeField] public TextMeshProUGUI ScoreText;
    [SerializeField] public TextMeshProUGUI TopRecordText;
    [Header("Ranked")]
    [NonSerialized] private int Round = 1;
    [NonSerialized] public List<GameObject> OutedEnemies;
    [NonSerialized] public List<WayCreator> OutedWays;
    [NonSerialized] public int[] RatioOfOperatives = { 5, 0 };
    [SerializeField] public GameObject RoundCallerGO;
    [SerializeField] public Animator RoundCallerAnimator;


    private void Start()
    {
        TopRecordText.text = "TOP SCORE = " + PlayerPrefs.GetInt("TopScore");
        MenuGO.SetActive(true);
    }

    private void MapCreator()
    {
        MapGO = Instantiate(MapsPF[UnityEngine.Random.Range(0, MapsPF.Length - 1)], new Vector3(50, 1.5f, 0), Quaternion.identity);
        MapCS = MapGO.GetComponent<MapScript>();
        Ways = MapCS.Ways;
        IsKaliWayExist = MapCS.IsKaliWayExist;
        MenuGO.SetActive(false);
        GameStarted = true;
    }

    #region Ranked
    public void RankedGameStart()
    {
        MapCreator();
    }
    #endregion

    #region Infinite Mode
    public void InfiniteModeStart()
    {
        MapCreator();
        StartCoroutine(EnemySpawn());
    }

    private IEnumerator EnemySpawn()
    {
        GameObject _EnemyGO;
        int wayID;
        if (UnityEngine.Random.Range(0, 100) < 80)
        {
            wayID = UnityEngine.Random.Range(0, Ways.Length - (IsKaliWayExist? 1 : 0));
            _EnemyGO = Instantiate(EnemyPF[UnityEngine.Random.Range(0, EnemyPF.Count)], Ways[wayID].transform.position, Quaternion.identity);
        }
        else
        {
            int enemyID = UnityEngine.Random.Range(0, SpecialEnemyPF.Count); 
            wayID = UnityEngine.Random.Range(enemyID == 0 ? (IsKaliWayExist == true ? Ways.Length - 1 : 0) : 0, Ways.Length - (IsKaliWayExist? 1 : 0));
            _EnemyGO = Instantiate(SpecialEnemyPF[enemyID], Ways[wayID].transform.position, Quaternion.identity);
        }
        Enemy _EnemyCS = _EnemyGO.GetComponent<Enemy>();
        _EnemyCS.HRGC = this;
        _EnemyCS._WayCreator = Ways[wayID];
        Enemies.Add(_EnemyGO);

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

        if (Shoots == -3 && !GameOver)
        {
            GameStarted = false;
            GameOver = true;
            StopAllCoroutines();
            foreach (GameObject go in Enemies)
            {
                go.GetComponentInParent<Enemy>().Speed = 0;
                go.GetComponentInParent<Enemy>().StopAllCoroutines();
            }
            if(Score > PlayerPrefs.GetInt("TopScore"))
                PlayerPrefs.SetInt("TopScore", Score);
            CavLaughsGO.transform.localPosition = new Vector3(0,-1200,-50);
            CavLaughsGO.SetActive(true);
        }
    }

    public void ResetAllVallues()
    {
        MenuGO.SetActive(true);
        TopRecordText.text = "TOP SCORE = " + PlayerPrefs.GetInt("TopScore");
        GameOver = false;
        Shoots = 1;
        Score = 0;

        foreach (GameObject go in Enemies)
        {
            Destroy(go);
        }
        Enemies.Clear();

        CavLaughsAnimator.SetTrigger("Restart");
        CavLaughsGO.SetActive(false);

        Destroy(MapGO);
    }
    #endregion
}