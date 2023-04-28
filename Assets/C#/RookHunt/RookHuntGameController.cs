using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using Unity.VisualScripting;

public class RookHuntGameController : MonoBehaviour
{
    [NonSerialized] public GameObject MapGO;
    [NonSerialized] private MapScript MapCS;
    [NonSerialized] public List<WayCreator> Ways;
    [SerializeField] private GameObject[] MapsPF;
    [SerializeField] private List<GameObject> EnemyPF;
    [SerializeField] private List<GameObject> SpecialEnemyPF;
    [NonSerialized] public List<GameObject> Enemies = new List<GameObject>();
    [SerializeField] public GameObject CavLaughsGO;
    [SerializeField] public GameObject CavLaughsHeadGO;
    [SerializeField] public Collider2D CavLaughsRestartColl;
    [NonSerialized] private bool IsKaliWayExist;
    [NonSerialized] public double Shoots = 3;
    [NonSerialized] public int KillStreak = 1;
    [NonSerialized] public int Score;
    [NonSerialized] public bool GameStarted;
    [NonSerialized] private bool GameOver;
    [Header("UI")]
    [SerializeField] public Image[] BulletsImg;
    [SerializeField] public TextMeshProUGUI MultiplierText;
    [SerializeField] public TextMeshProUGUI ScoreText;
    [SerializeField] public GameObject MenuGO;
    [SerializeField] public TextMeshProUGUI TopRecordText;
    [SerializeField] public GameObject LoadingScreen;
    [SerializeField] public TextMeshProUGUI LSRounds;
    [SerializeField] public TextMeshProUGUI LSTeamRole;
    [SerializeField] public TextMeshProUGUI LSRoundsForChangeDuty;
    [SerializeField] public GameObject LSTeamIconCenter;
    [SerializeField] public GameObject LSDefendersIcon;
    [SerializeField] public GameObject LSAtatckersIcon;
    [Header("Ranked")]
    [NonSerialized] private bool IsDefender = true;
    [NonSerialized] private int Round;
    [NonSerialized] public List<GameObject> OutedEnemies;
    [NonSerialized] public List<GameObject> OutedSpecialEnemies;
    [NonSerialized] public List<WayCreator> OutedWays;
    [NonSerialized] public double RatioOfOperatives = 5.0;


    private void Start()
    {
        TopRecordText.text = "TOP SCORE = " + PlayerPrefs.GetInt("TopScore");
        MenuGO.transform.localPosition = Vector2.zero;
    }

    private void MapCreator()
    {
        MapGO = Instantiate(MapsPF[UnityEngine.Random.Range(0, MapsPF.Length - 1)], new Vector3(50, 1.5f, 0), Quaternion.identity);
        MapCS = MapGO.GetComponent<MapScript>();
        Ways = MapCS.Ways;
        IsKaliWayExist = MapCS.IsKaliWayExist;
        MenuGO.transform.localPosition = new Vector2(0, 2000);
    }

    #region Ranked
    public void RankedGameStart()
    {
        MapCreator();
        StartCoroutine(RankedRoundLauncher());
    }

    public IEnumerator RankedRoundLauncher()
    {
        Round++;
        LSRounds.text = "round " + Round;
        LoadingScreen.transform.localPosition = Vector2.zero;
        if (Round % 2 == 1 && Round != 1)
        {
            yield return new WaitForSeconds(1);
            for (int i = 0; i != 18; i++)
            {
                LSTeamIconCenter.transform.Rotate(0, 0, 10);
                LSDefendersIcon.transform.rotation = Quaternion.identity;
                LSAtatckersIcon.transform.rotation = Quaternion.identity;
                yield return new WaitForSeconds(0.12f);
            }
            IsDefender = !IsDefender;
        }
        else
        {
            LSRoundsForChangeDuty.text = Round == 1 ? "2" : "0";
        }
        LSTeamRole.text = IsDefender == true ? "defend" : "atack";
        yield return new WaitForSeconds(3);
        LoadingScreen.transform.localPosition = new Vector2(0, 2000);
        GameStarted = true;

        for (int a = 0; a != 5; a++)
        {
            GameObject _EnemyGO;
            int wayID;
            if ((int)RatioOfOperatives != 0)
            {
                int enemyID = UnityEngine.Random.Range(0, SpecialEnemyPF.Count);
                wayID = UnityEngine.Random.Range(0, Ways.Count - (IsKaliWayExist ? 1 : 0));
                _EnemyGO = Instantiate(EnemyPF[enemyID], Ways[wayID].transform.position, Quaternion.identity);

                OutedWays.Add(Ways[wayID]);
                Ways.Remove(Ways[wayID]);

                OutedEnemies.Add(EnemyPF[enemyID]);
                EnemyPF.Remove(EnemyPF[enemyID]);
            }
            else
            {
                int enemyID = UnityEngine.Random.Range(0, SpecialEnemyPF.Count);
                wayID = UnityEngine.Random.Range(enemyID == 0 ? (IsKaliWayExist == true ? Ways.Count - 1 : 0) : 0, Ways.Count - (IsKaliWayExist ? 1 : 0));
                _EnemyGO = Instantiate(SpecialEnemyPF[enemyID], Ways[wayID].transform.position, Quaternion.identity);

                OutedWays.Add(Ways[wayID]);
                Ways.Remove(Ways[wayID]);

                OutedSpecialEnemies.Add(SpecialEnemyPF[enemyID]);
                SpecialEnemyPF.Remove(SpecialEnemyPF[enemyID]);
            }
            Enemy _EnemyCS = _EnemyGO.GetComponent<Enemy>();
            _EnemyCS.HRGC = this;
            _EnemyCS._WayCreator = Ways[wayID];
            Enemies.Add(_EnemyGO);
        }
    }
    #endregion

    #region Infinite Mode
    public void InfiniteModeStart()
    {
        MapCreator();
        StartCoroutine(EnemySpawn());
        GameStarted = true;
    }

    private IEnumerator EnemySpawn()
    {
        GameObject _EnemyGO;
        int wayID;
        if (UnityEngine.Random.Range(0, 100) < 80)
        {
            wayID = UnityEngine.Random.Range(0, Ways.Count - (IsKaliWayExist ? 1 : 0));
            _EnemyGO = Instantiate(EnemyPF[UnityEngine.Random.Range(0, EnemyPF.Count)], Ways[wayID].transform.position, Quaternion.identity);
        }
        else
        {
            int enemyID = UnityEngine.Random.Range(0, SpecialEnemyPF.Count);
            wayID = UnityEngine.Random.Range(enemyID == 0 ? (IsKaliWayExist == true ? Ways.Count - 1 : 0) : 0, Ways.Count - (IsKaliWayExist ? 1 : 0));
            _EnemyGO = Instantiate(SpecialEnemyPF[enemyID], Ways[wayID].transform.position, Quaternion.identity);
        }
        Enemy _EnemyCS = _EnemyGO.GetComponent<Enemy>();
        _EnemyCS.HRGC = this;
        _EnemyCS._WayCreator = Ways[wayID];
        Enemies.Add(_EnemyGO);

        yield return new WaitForSeconds(1.5f);
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
            if (Score > PlayerPrefs.GetInt("TopScore"))
                PlayerPrefs.SetInt("TopScore", Score);
            CavLaughsGO.SetActive(true);
            StartCoroutine(CavLaughANIM());
        }
    }

    public void ResetAllVallues()
    {
        MenuGO.transform.localPosition = Vector2.zero;
        TopRecordText.text = "TOP SCORE = " + PlayerPrefs.GetInt("TopScore");
        GameOver = false;
        Shoots = 1;
        Score = 0;

        if (Enemies != null)
        {
            foreach (GameObject go in Enemies)
            {
                Destroy(go);
            }
            Enemies.Clear();
        }

        CavLaughsGO.GetComponent<BoxCollider2D>().enabled = false;
        CavLaughsRestartColl.enabled = false;
        CavLaughsGO.GetComponentInChildren<TextMeshProUGUI>().fontSize = 0;
        CavLaughsGO.transform.localPosition = new Vector2(0, -1200);

        StopAllCoroutines();
        Destroy(MapGO);
    }
    #endregion

    #region Animations
    private IEnumerator CavLaughANIM()
    {
        CavLaughsGO.GetComponent<BoxCollider2D>().enabled = true;
        yield return new WaitForSeconds(1);
        while (true)
        {
            CavLaughsGO.transform.localPosition += new Vector3(0, 5000 * Time.deltaTime);
            if (CavLaughsGO.transform.localPosition.y >= -501)
            {
                CavLaughsGO.transform.localPosition = new Vector3(0, -501);
                goto CavLaughANIM_exit;
            }
            else
                yield return new WaitForSeconds(0.03f);
        }
    CavLaughANIM_exit:
        int a = 0;
        while (true)
        {
            a++;
            if (a == 8)
            {
                CavLaughsRestartColl.enabled = true;
                CavLaughsGO.GetComponentInChildren<TextMeshProUGUI>().fontSize = 44;
                a++;
            }
            CavLaughsHeadGO.transform.localPosition = new Vector3(0, CavLaughsHeadGO.transform.localPosition.y == 280 ? 300 : 280);
            yield return new WaitForSeconds(0.12f);
        }
    }
    #endregion
}