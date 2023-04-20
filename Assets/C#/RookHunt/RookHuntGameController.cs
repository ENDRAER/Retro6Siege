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
    [SerializeField] private float SpawnSpeed;
    [SerializeField] public double Shoots;
    [SerializeField] public int KillStreak;
    [SerializeField] public int Score;
    [SerializeField] public GameObject CavLaughsGO;
    [SerializeField] public Animator CavLaughsAnimator;
    [SerializeField] private bool GameOver;
    [Header("UI")]
    [SerializeField] public Image[] BulletsImg;
    [SerializeField] public TextMeshProUGUI MultiplierText;
    [SerializeField] public TextMeshProUGUI ScoreText;
    [SerializeField] public TextMeshProUGUI TopRecordText;
    [Header("Ranked")]
    [SerializeField] private List<GameObject> Enemies;
    [SerializeField] private int Round;


    private void Start()
    {
        TopRecordText.text = "TOP SCORE = " + PlayerPrefs.GetInt("TopScore");
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
        MapGO = Instantiate(MapsPF[UnityEngine.Random.Range(0, MapsPF.Length - 1)], new Vector3(50, 1.5f, 0), Quaternion.identity);
        MapCS = MapGO.GetComponent<MapScript>();
        Ways = MapCS.Ways;
        IsKaliWayExist = MapCS.IsKaliWayExist;
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
            _EnemyCS.HRGC = this;
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

        if (Shoots == -3 && !GameOver)
        {
            GameOver = true;
            StopAllCoroutines();
            Enemies.Remove(gameObject4);
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
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
    #endregion

    public void ResetAllVallues()
    {
        CavLaughsAnimator.SetTrigger("Restart");
        CavLaughsGO.SetActive(false);

        MenuGO.SetActive(true);
        TopRecordText.text = "TOP SCORE = " + PlayerPrefs.GetInt("TopScore");
        Shoots = 1;
        Score = 0;

        Destroy(MapGO);
    }
}