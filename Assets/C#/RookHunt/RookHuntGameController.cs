using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class RookHuntGameController : MonoBehaviour
{
    [NonSerialized] public GameObject MapGO;
    [NonSerialized] private MapScript MapCS;
    [NonSerialized] public List<WayCreator> Ways; 
    [NonSerialized] public List<WayCreator> WaysDef;
    [NonSerialized] private WayCreator SnipersWay;
    [SerializeField] private GameObject[] MapsPF;
    [SerializeField] private List<GameObject> EnemyPF;
    [SerializeField] private List<GameObject> SpecialEnemyPF;
    [SerializeField] private List<GameObject> EnemyDefPF;
    [NonSerialized] public List<GameObject> Enemies = new List<GameObject>();
    [NonSerialized] public bool InvincibleEnemies;
    [NonSerialized] public double Shoots = 3;
    [NonSerialized] public int KillStreak = 1;
    [NonSerialized] public int Score;
    [SerializeField] public enum _CurrentMode { Menu, GameOver, Ranked, Infinite }
    [SerializeField] public _CurrentMode CurrentMode = _CurrentMode.Menu;
    [Header("UI")]
    [SerializeField] public GameObject MenuGO;
    [SerializeField] public TextMeshProUGUI TopRecordText;
    [SerializeField] public GameObject CavLaughsGO;
    [SerializeField] public GameObject CavLaughsHeadGO;
    [SerializeField] public Collider2D CavLaughsRestartColl;
    [SerializeField] public Image[] BulletsImg;
    [SerializeField] public TextMeshProUGUI MultiplierText;
    [SerializeField] public TextMeshProUGUI ScoreText;
    [SerializeField] public Animator FlashScreenAnim;
    [Header("Ranked")]
    [SerializeField] public TextMeshProUGUI TeamScoreText;
    [NonSerialized] private bool IsDefender = true;
    [NonSerialized] private int Round = 0;
    [NonSerialized] private int[] TeamScore = { 0, 0 };
    [NonSerialized] public int SpecialOp = 3;
    [SerializeField] public GameObject LoadingScreen; // LS - LoadingScreen
    [SerializeField] public TextMeshProUGUI LSRounds;
    [SerializeField] public TextMeshProUGUI LSTeamRole;
    [SerializeField] public TextMeshProUGUI LSRoundsForChangeDuty;
    [SerializeField] public GameObject LSTeamIconCenter;
    [SerializeField] public GameObject LSDefendersIcon;
    [SerializeField] public GameObject LSAtatckersIcon;
    [SerializeField] public GameObject Outro;
    [SerializeField] public TextMeshProUGUI OutroRoundStatusText;
    [SerializeField] public TextMeshProUGUI OutroReasonOfEndText;


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
        WaysDef = MapCS.WaysDef;
        SnipersWay = MapCS.SnipersWay;
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
                LSRoundsForChangeDuty.text = "1";
                LSTeamIconCenter.transform.Rotate(0, 0, 10);
                LSDefendersIcon.transform.rotation = Quaternion.identity;
                LSAtatckersIcon.transform.rotation = Quaternion.identity;
                yield return new WaitForSeconds(0.06f);
            }
            IsDefender = !IsDefender;
        }
        LSTeamRole.text = IsDefender == true ? "defend" : "atack";
        yield return new WaitForSeconds(3);
        LoadingScreen.transform.localPosition = new Vector2(0, 2000);
        StartCoroutine(TurnOfRoundTeamScoreStats());

        #region defend
        if (IsDefender)
        {
            MapCS.Inside.SetActive(true);
            MapCS.Outside.SetActive(false);
            TeamScoreText.text = TeamScore[0] + ":" + TeamScore[1];

            int wayID = 0;
            Enemy _EnemyCS;
            GameObject _EnemyGO;
            List<WayCreator> OutedWays = new List<WayCreator>();
            List<GameObject> OutedEnemies = new List<GameObject>();
            List<GameObject> OutedSpecialEnemies = new List<GameObject>();
            for (int a = 0; a != 5; a++)
            {
                if (SpecialOp == 0)
                {
                    int enemyID = UnityEngine.Random.Range(0, EnemyPF.Count - 1);
                    wayID = UnityEngine.Random.Range(0, Ways.Count - 1);
                    _EnemyGO = Instantiate(EnemyPF[enemyID], Ways[wayID].transform.position, Quaternion.identity);

                    OutedEnemies.Add(EnemyPF[enemyID]);
                    EnemyPF.Remove(EnemyPF[enemyID]);
                }
                else
                {
                    int enemyID = UnityEngine.Random.Range(0, SpecialEnemyPF.Count - 1);
                    _EnemyGO = Instantiate(SpecialEnemyPF[enemyID], new Vector3(50, 50), Quaternion.identity);
                    if ((_EnemyGO.name.StartsWith("Kali") || _EnemyGO.name.StartsWith("Glaz")) && SnipersWay != null)
                    {
                        _EnemyGO.transform.position = SnipersWay.transform.position;
                        _EnemyCS = _EnemyGO.GetComponent<Enemy>();
                        _EnemyCS._WayCreator = SnipersWay;
                        SnipersWay = null;
                    }
                    else
                    {
                        wayID = UnityEngine.Random.Range(0, Ways.Count - 1);
                        _EnemyGO.transform.position = Ways[wayID].transform.position;
                    }
                    SpecialOp--;

                    OutedSpecialEnemies.Add(SpecialEnemyPF[enemyID]);
                    SpecialEnemyPF.Remove(SpecialEnemyPF[enemyID]);
                }

                _EnemyCS = _EnemyGO.GetComponent<Enemy>();
                if (_EnemyCS._WayCreator == null)
                {
                    _EnemyCS._WayCreator = Ways[wayID];
                    OutedWays.Add(Ways[wayID]);
                    Ways.Remove(Ways[wayID]);
                }
                _EnemyCS.HRGC = this;
                _EnemyCS.Perspective = MapCS.Perspective;
                Enemies.Add(_EnemyGO);
            }
            SpecialOp = 3;
            SnipersWay = MapCS.SnipersWay;
            Ways.AddRange(OutedWays);
            EnemyPF.AddRange(OutedEnemies);
            SpecialEnemyPF.AddRange(OutedSpecialEnemies);

            CurrentMode = _CurrentMode.Ranked;
            InvincibleEnemies = false;
        }
        #endregion

        #region atack
        else
        {
            Shoots = 0;
            MagazineUpdate();
            MapCS.Inside.SetActive(false);
            MapCS.Outside.SetActive(true);
            TeamScoreText.text = TeamScore[0] + ":" + TeamScore[1];

            yield return new WaitForSeconds(UnityEngine.Random.Range(0.7f, 8));

            List<GameObject> OutedEnemies = new List<GameObject>();
            List<WayCreator> OutedWays = new List<WayCreator>();
            GameObject _EnemyGO;
            int wayID = 0;
            for (int i = 0; i != 1; i++)
            {
                int enemyID = UnityEngine.Random.Range(0, EnemyDefPF.Count - 1);
                wayID = UnityEngine.Random.Range(0, WaysDef.Count - 1);
                _EnemyGO = Instantiate(EnemyDefPF[enemyID], WaysDef[wayID].transform.position, Quaternion.identity);

                Enemy _EnemyCS = _EnemyGO.GetComponent<Enemy>();
                _EnemyCS.HRGC = this;
                _EnemyCS._WayCreator = WaysDef[wayID];
                _EnemyCS.Perspective = MapCS.PerspectiveDef;

                OutedEnemies.Add(EnemyPF[enemyID]);
                EnemyDefPF.Remove(EnemyPF[enemyID]);
                Enemies.Add(_EnemyGO);
            }
            WaysDef.AddRange(OutedWays);
            EnemyPF.AddRange(OutedEnemies);

            CurrentMode = _CurrentMode.Ranked;
            InvincibleEnemies = false;
        }
        #endregion
    }
    public IEnumerator TurnOfRoundTeamScoreStats()
    {
        yield return new WaitForSeconds(0.7f);
        TeamScoreText.text = null;
    }

    public IEnumerator EndOfTheRankedRound(bool IsWinner)
    {
        InvincibleEnemies = true;
        CurrentMode = _CurrentMode.Menu;
        foreach (GameObject go in Enemies)
        {
            go.GetComponentInParent<Enemy>().Speed = 0;
            go.GetComponentInParent<Enemy>().StopAllCoroutines();
        }
        OutroRoundStatusText.text = IsWinner ? "ROUND " + Round + " WON" : "ROUND " + Round + " LOST";
        OutroReasonOfEndText.text = IsWinner ? "Enemies eliminated" : "no ammunition left";
        TeamScore[IsWinner ? 0 : 1]++;
        for (int i = 1; Outro.transform.localScale.y < 1; i++)
        {
            Outro.transform.localScale = new Vector3(1, (float)i /10,1);
            yield return new WaitForSeconds(0.06f);
        }
        Outro.transform.localScale = new Vector3(1, 1, 1);
        yield return new WaitForSeconds(2);
        Outro.transform.localScale = new Vector3(1, 0, 1);

        ResetAllVallues();
        StartCoroutine(RankedRoundLauncher());
    }
    #endregion

    #region Infinite Mode
    public void InfiniteModeStart()
    {
        MapCreator();
        StartCoroutine(EnemySpawn());
        CurrentMode = _CurrentMode.Infinite;
    }

    private IEnumerator EnemySpawn()
    {
        GameObject _EnemyGO;
        int wayID = 0;
        Enemy _EnemyCS;
        if (UnityEngine.Random.Range(0, 100) < 80)
        {
            int enemyID = UnityEngine.Random.Range(0, EnemyPF.Count - 1);
            wayID = UnityEngine.Random.Range(0, Ways.Count - 1);
            _EnemyGO = Instantiate(EnemyPF[enemyID], Ways[wayID].transform.position, Quaternion.identity);
        }
        else
        {
            int enemyID = UnityEngine.Random.Range(0, SpecialEnemyPF.Count - 1);
            if ((SpecialEnemyPF[enemyID].name.StartsWith("Kali") || SpecialEnemyPF[enemyID].name.StartsWith("Kali")) && SnipersWay != null)
            {
                _EnemyGO = Instantiate(SpecialEnemyPF[enemyID], SnipersWay.transform.position, Quaternion.identity);
                _EnemyCS = _EnemyGO.GetComponent<Enemy>();
                _EnemyCS._WayCreator = SnipersWay;
                goto SkipWaySetter_Infinite; // from here
            }
            else
            {
                wayID = UnityEngine.Random.Range(0, Ways.Count - 1);
                _EnemyGO = Instantiate(SpecialEnemyPF[enemyID], Ways[wayID].transform.position, Quaternion.identity);
            }
        }
        _EnemyCS = _EnemyGO.GetComponent<Enemy>();
        _EnemyCS._WayCreator = Ways[wayID];
        SkipWaySetter_Infinite: // to there
        _EnemyCS.HRGC = this;
        _EnemyCS.Perspective = MapCS.Perspective;
        Enemies.Add(_EnemyGO);

        yield return new WaitForSeconds(1.5f);
        StartCoroutine(EnemySpawn());
    }
    #endregion

    #region Update Values
    public void StatUpdate()
    {
        MagazineUpdate();
        MultiplierText.text = KillStreak + " STREAK (" + (1 + KillStreak * 0.2f) + "x Multiplier)";
        ScoreText.text = Score.ToString();
    }

    public void MagazineUpdate()
    {
        Shoots = math.clamp(Shoots, -2, 1);
        for (int i = 0; i < BulletsImg.Length; i++)
        {
            BulletsImg[i].fillAmount = i + (float)Shoots;
        }

        if (CurrentMode == _CurrentMode.Infinite && Shoots < -1)
        {
            CurrentMode = _CurrentMode.GameOver;
            TeamScoreText.text = null;
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

        if (CurrentMode == _CurrentMode.Ranked)
        {
            if (Enemies.Count == 0)
                StartCoroutine(EndOfTheRankedRound(true));
            else if (Shoots < -1)
                StartCoroutine(EndOfTheRankedRound(false));
        }
    }

    public void ExitInMainMenu()
    {
        TopRecordText.text = "TOP SCORE = " + PlayerPrefs.GetInt("TopScore");
        CurrentMode = _CurrentMode.Menu;
        ResetAllVallues(); 
        InvincibleEnemies = false;
        CavLaughsRestartColl.enabled = false;
        CavLaughsGO.GetComponentInChildren<TextMeshProUGUI>().fontSize = 0;
        CavLaughsGO.transform.localPosition = new Vector2(0, -1200);

        StopAllCoroutines();
        Destroy(MapGO);
        MenuGO.transform.localPosition = Vector2.zero;
    }

    public void ResetAllVallues()
    {
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
        StatUpdate();
    }
    #endregion

    #region Animations
    private IEnumerator CavLaughANIM()
    {
        InvincibleEnemies = true;
        yield return new WaitForSeconds(1);
        while (CavLaughsGO.transform.localPosition.y < -502)
        {
            yield return new WaitForSeconds(0.03f);
            CavLaughsGO.transform.localPosition += new Vector3(0, 5000 * Time.deltaTime);
        }
        CavLaughsGO.transform.localPosition = new Vector3(0, -501);
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
