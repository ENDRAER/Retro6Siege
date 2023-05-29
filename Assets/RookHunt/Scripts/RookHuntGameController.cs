using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using static ScriptKing;
using System.Runtime.CompilerServices;

public class RookHuntGameController : MonoBehaviour
{
    [NonSerialized] public int CurrentRang = 0;
    [NonSerialized] public GameObject MapGO;
    [NonSerialized] private MapScript MapCS;
    [NonSerialized] public List<WayCreator> Ways;
    [NonSerialized] public List<WayCreator> WaysDef;
    [NonSerialized] private WayCreator SnipersWay;
    [NonSerialized] public ScriptKing _ScriptKing;
    [SerializeField] private GameObject[] MapsPF;
    [SerializeField] public List<GameObject> EnemyPF;
    [SerializeField] private List<GameObject> SpecialEnemyPF;
    [SerializeField] private List<GameObject> EnemyDefPF;
    [SerializeField] public GameObject Alibi;
    [NonSerialized] public List<GameObject> Enemies = new List<GameObject>();
    [NonSerialized] public bool InvincibleEnemies;
    [NonSerialized] public double Shoots = 3;
    [NonSerialized] public int KillStreak = 1;
    [NonSerialized] public int Score;
    [SerializeField] public enum _CurrentMode { Menu, GameOver, Ranked, Infinite }
    [SerializeField] public _CurrentMode CurrentMode = _CurrentMode.Menu;
    [Header("UI")]
    [SerializeField] public GameObject MenuGO;
    [SerializeField] public Image CurrentRangImg;
    [SerializeField] public TextMeshProUGUI TopRecordText;
    [SerializeField] public GameObject CavLaughsGO;
    [SerializeField] public GameObject CavLaughsHeadGO;
    [SerializeField] public Collider2D CavLaughsRestartColl;
    [SerializeField] public Image[] BulletsImg;
    [SerializeField] public TextMeshProUGUI MultiplierText;
    [SerializeField] public TextMeshProUGUI ScoreText;
    [SerializeField] public Animator FlashScreenAnim;
    [SerializeField] public Animator BloodFrameAnim;
    [Header("Ranked")]
    [SerializeField] public Image[] OpIcos;
    [SerializeField] public Sprite KilledIcon;
    [SerializeField] public Sprite SpaceSprite;
    [SerializeField] public TextMeshProUGUI TeamScoreText;
    [SerializeField] public GameObject LoadingScreen; // LS - LoadingScreen
    [SerializeField] public TextMeshProUGUI LSRounds;
    [SerializeField] public TextMeshProUGUI LSTeamScore;
    [SerializeField] public TextMeshProUGUI LSTeamRole;
    [SerializeField] public GameObject LSTeamIconCenter;
    [SerializeField] public GameObject LSDefendersIcon;
    [SerializeField] public GameObject LSAtatckersIcon;
    [SerializeField] public GameObject Outro;
    [SerializeField] public TextMeshProUGUI OutroRoundStatusText;
    [SerializeField] public TextMeshProUGUI OutroReasonOfEndText;
    [SerializeField] public GameObject RewardScreen; // RS
    [SerializeField] public Animator RSAnimator;
    [SerializeField] public Image[] RSRangs;
    [SerializeField] public Sprite[] RangImages;
    [SerializeField] public Image RSGradiend;
    [SerializeField] public TextMeshProUGUI RSMatchReport;
    [SerializeField] public GameObject RSNextButton;
    [NonSerialized] private bool IsDefender = true;
    [NonSerialized] private int Round = 0;
    [NonSerialized] private int[] TeamScore = { 0, 0 };
    [NonSerialized] public int StatsMaxKillStreak;
    [NonSerialized] public int StatsShootsMissed;
    [NonSerialized] public int StatsEnemyMissed;
    [Header("Audios")]
    [SerializeField] public AudioClip MainMenuAudioAC;
    [NonSerialized] public GameObject MainMenuAudioGO;
    [SerializeField] public GameObject TVAudioSource;
    [SerializeField] public AudioClip ShootSound;
    [SerializeField] public AudioClip RunningSound;
    [SerializeField] public AudioClip HitSound;
    [SerializeField] public AudioClip EnemyHitSound;
    [SerializeField] public AudioClip StartRoundMusic;
    [SerializeField] public AudioClip DefeatMusic;
    [SerializeField] public AudioClip WinMusic;
    [SerializeField] public AudioClip WinMusic_Perfect;
    [SerializeField] public AudioClip CavLaughAC;
    [SerializeField] public AudioClip AlibiKillingAC;


    private void Start()
    {
        _ScriptKing = MainBridge;
        CurrentRang = math.clamp(PlayerPrefs.GetInt("CurrentRang"), 0, RangImages.Length - 1);
        CurrentRang = RangImages.Length - 1;
        CurrentRangImg.sprite = RangImages[CurrentRang];
        TopRecordText.text = "TOP SCORE = " + PlayerPrefs.GetInt("TopScore");
        MenuGO.transform.localPosition = Vector2.zero;
        MainMenuAudioGO = MainBridge.CreateSoundGetGO(TVAudioSource, MainMenuAudioAC, _defaultPos.TV, true);
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
        if(MainMenuAudioGO != null)
            Destroy(MainMenuAudioGO);
        CurrentRang--;
        PlayerPrefs.SetInt("CurrentRang", CurrentRang);
        IsDefender = UnityEngine.Random.Range(0, 2) == 0 ? true : false;
        MapCreator();
        StartCoroutine(RankedRoundLauncher());
    }

    public IEnumerator RankedRoundLauncher()
    {
        MainBridge.CreateSoundGetGO(TVAudioSource, StartRoundMusic, _defaultPos.TV, true);
        Round++;
        LSRounds.text = "round " + Round;
        LSTeamScore.text = TeamScore[0] + ":" + TeamScore[1];
        LoadingScreen.transform.localPosition = Vector2.zero;
        float WaitingToStart = 4;
        if (Round == 4 || Round >= 7)
        {
            yield return new WaitForSeconds(1);
            for (int i = 0; i != 18; i++)
            {
                LSTeamIconCenter.transform.Rotate(0, 0, 10);
                LSDefendersIcon.transform.rotation = Quaternion.identity;
                LSAtatckersIcon.transform.rotation = Quaternion.identity;
                yield return new WaitForSeconds(0.06f);
                WaitingToStart -= 0.06f;
            }
            IsDefender = !IsDefender;
        }
        LSTeamRole.text = IsDefender == true ? "defend" : "atack";
        yield return new WaitForSeconds(WaitingToStart);
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
            int SpecialOp;
            switch (CurrentRang)
            {
                case < 5:
                    SpecialOp = 0;
                    break;
                case < 15:
                    SpecialOp = 1;
                    break;
                case < 20:
                    SpecialOp = 2;
                    break;
                case < 25:
                    SpecialOp = 3;
                    break;
                case < 30:
                    SpecialOp = 4;
                    break;
                default:
                    SpecialOp = 5;
                    break;
            }
            if(UnityEngine.Random.Range(0,100) == 100)
            {
                WayCreator WC = MapCS.AlibiWay;
                _EnemyGO = Instantiate(Alibi, WC.transform.position, Quaternion.identity);
                _EnemyGO.GetComponent<Enemy>()._WayCreator = WC;
                _EnemyGO.GetComponent<Enemy>().RHGC = this;
                Enemies.Add(_EnemyGO);

                GameObject AUGO = _ScriptKing.CreateSoundGetGO(TVAudioSource, RunningSound, ScriptKing._defaultPos.TV, false);
                AudioSource AUAU = AUGO.GetComponent<AudioSource>();
                AUAU.GetComponent<AudioSource>().clip = RunningSound;
                _EnemyGO.gameObject.transform.SetParent(AUAU.transform);
            }
            for (int id = 0; id != 5; id++)
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
                _EnemyCS.id = id;
                OpIcos[id].sprite = _EnemyCS.IcoSprite;
                _EnemyCS.RHGC = this;
                _EnemyCS.Perspective = MapCS.Perspective;

                GameObject AUGO = _ScriptKing.CreateSoundGetGO(TVAudioSource, RunningSound, ScriptKing._defaultPos.TV, false);
                AudioSource AUAU = AUGO.GetComponent<AudioSource>();
                AUAU.GetComponent<AudioSource>().clip = RunningSound;
                _EnemyGO.gameObject.transform.SetParent(AUAU.transform);

                float _Speed = UnityEngine.Random.Range(0.7f, 0.9f) + (0.6f / (RangImages.Length - 1) * CurrentRang);
                _EnemyCS.Speed *= _Speed;
                _EnemyCS.AnimDelay /= _Speed;
                _EnemyCS.ShootingDelay *= 1 - (0.4f / (RangImages.Length - 1) * CurrentRang);

                Enemies.Add(_EnemyGO);
            }
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
            Shoots = CurrentRang >= 10 ? 0 : 1;
            MagazineUpdate();
            MapCS.Inside.SetActive(false);
            MapCS.Outside.SetActive(true);
            CurrentMode = _CurrentMode.Ranked;
            TeamScoreText.text = TeamScore[0] + ":" + TeamScore[1];

            List<GameObject> OutedEnemies = new List<GameObject>();
            List<WayCreator> OutedWays = new List<WayCreator>();
            List<Enemy> AllCreatedEnemyCS = new List<Enemy>();
            GameObject _EnemyGO;
            int wayID = 0;
            for (int i = 0; i != (CurrentRang >= 25 ? 2 : 1); i++)
            {
                int enemyID = UnityEngine.Random.Range(0, EnemyDefPF.Count - 1);
                wayID = UnityEngine.Random.Range(0, WaysDef.Count - 1);
                _EnemyGO = Instantiate(EnemyDefPF[enemyID], WaysDef[wayID].transform.position, Quaternion.identity);

                Enemy _EnemyCS = _EnemyGO.GetComponent<Enemy>();
                _EnemyCS.WalkType = Enemy._WalkType.Stop;
                _EnemyCS.RHGC = this;
                _EnemyCS.id = i;
                _EnemyCS._WayCreator = WaysDef[wayID];
                _EnemyCS.Perspective = MapCS.PerspectiveDef;

                _EnemyCS.Speed *= UnityEngine.Random.Range(0.7f, 0.9f) + (0.6f / (RangImages.Length - 1) * CurrentRang);
                _EnemyCS.ShootingDelay *= 1 - (0.4f / (RangImages.Length - 1) * CurrentRang);
                _EnemyCS.PostShootingDelay *= 1 - (0.2f / (RangImages.Length - 1) * CurrentRang);

                AllCreatedEnemyCS.Add(_EnemyCS);

                OutedEnemies.Add(EnemyDefPF[enemyID]);
                EnemyDefPF.Remove(EnemyDefPF[enemyID]);
                OutedWays.Add(WaysDef[wayID]);
                WaysDef.Remove(WaysDef[wayID]);
                Enemies.Add(_EnemyGO);
                _EnemyGO.transform.SetParent(new GameObject().AddComponent<AudioSource>().transform);
                OpIcos[i].sprite = _EnemyCS.IcoSprite;
            }
            WaysDef.AddRange(OutedWays);
            EnemyDefPF.AddRange(OutedEnemies);
            InvincibleEnemies = false;

            yield return new WaitForSeconds(UnityEngine.Random.Range(0.7f, 8));
            foreach (Enemy e in AllCreatedEnemyCS)
            {
                e.WalkType = Enemy._WalkType.BetweenPoints;
            }
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
        MainBridge.CreateSoundGetGO(TVAudioSource, IsWinner? KillStreak > 5 ? WinMusic_Perfect : WinMusic : DefeatMusic, _defaultPos.TV, true);
        InvincibleEnemies = true;
        CurrentMode = _CurrentMode.Menu;
        foreach (GameObject go in Enemies)
        {
            go.GetComponentInParent<Enemy>().Speed = 0;
            go.GetComponentInParent<AudioSource>().Stop();
            go.GetComponentInParent<Enemy>().StopAllCoroutines();
        }
        OutroRoundStatusText.text = IsWinner ? "ROUND " + Round + " WON" : "ROUND " + Round + " LOST";
        OutroReasonOfEndText.text = IsWinner ? "Enemies eliminated" : "no ammunition left";
        TeamScore[IsWinner ? 0 : 1]++;
        for (int i = 1; Outro.transform.localScale.y < 1; i++)
        {
            Outro.transform.localScale = new Vector3(1, (float)i / 10, 1);
            yield return new WaitForSeconds(0.06f);
        }
        Outro.transform.localScale = new Vector3(1, 1, 1);
        yield return new WaitForSeconds(2);
        Outro.transform.localScale = new Vector3(1, 0, 1);
        foreach (Image icon in OpIcos)
        {
            icon.sprite = SpaceSprite;
        }

        Shoots = 1;
        if (Enemies != null)
        {
            foreach (GameObject go in Enemies)
            {
                Destroy(go.transform.parent.gameObject);
            }
            Enemies.Clear();
        }
        StatUpdate();
        if (TeamScore[0] == 4 && TeamScore[1] < 3 || TeamScore[0] == 5)
            EndOfTheRankedMatch(true);
        else if (TeamScore[1] == 4 && TeamScore[0] < 3 || TeamScore[1] == 5)
            EndOfTheRankedMatch(false);
        else
            StartCoroutine(RankedRoundLauncher());
    }

    public void EndOfTheRankedMatch(bool IsWinner)
    {
        CurrentRang += IsWinner ? 2 : 0;
        RSGradiend.color = IsWinner ? new Color(0, 0.6f, 1) : new Color(1, 0.2441347f, 0);
        bool HasntMoved = false;
        if (CurrentRang < 0 || CurrentRang > 35)
        {
            CurrentRang = math.clamp(CurrentRang, 0, RangImages.Length - 1);
            RSAnimator.SetTrigger("HasNotMoved");
            HasntMoved = true;
        }
        else
        {
            RSAnimator.SetTrigger(IsWinner ? "Win" : "Lose");
        }
        PlayerPrefs.SetInt("CurrentRang", CurrentRang);
        int RangsRange = HasntMoved ? -2 : (IsWinner ? -3 : -2);
        foreach (Image img in RSRangs)
        {
            try
            {
                img.sprite = RangImages[CurrentRang + RangsRange];
            }
            catch
            {
                img.sprite = SpaceSprite;
            }
            RangsRange++;
        }
        RewardScreen.transform.localPosition = Vector3.zero;
        StartCoroutine(CavLaughANIM(-428, -530, 0.7f, true, IsWinner));
        RSMatchReport.text =
            TeamScore[0] + " : " + TeamScore[1] +
            "\r\nSCORE: " + Score +
            "\r\nMax killstreak: " + (StatsMaxKillStreak > KillStreak ? StatsMaxKillStreak : KillStreak) +
            "\r\nenemy missed: " + StatsEnemyMissed +
            "\r\nshoots missed: " + StatsShootsMissed;
    }
    #endregion

    #region Infinite Mode
    public void InfiniteModeStart()
    {
        if (MainMenuAudioGO != null)
            Destroy(MainMenuAudioGO);
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
            }
            else
            {
                wayID = UnityEngine.Random.Range(0, Ways.Count - 1);
                _EnemyGO = Instantiate(SpecialEnemyPF[enemyID], Ways[wayID].transform.position, Quaternion.identity);
            }
        }
        _EnemyCS = _EnemyGO.GetComponent<Enemy>();

        GameObject AUGO = _ScriptKing.CreateSoundGetGO(TVAudioSource, RunningSound, ScriptKing._defaultPos.TV, false);
        AudioSource AUAU = AUGO.GetComponent<AudioSource>();
        AUAU.GetComponent<AudioSource>().clip = RunningSound;
        _EnemyGO.gameObject.transform.SetParent(AUAU.transform);

        if (_EnemyCS._WayCreator == null)
            _EnemyCS._WayCreator = Ways[wayID];
        _EnemyCS.RHGC = this;
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
                go.GetComponentInParent<AudioSource>().Stop();
                go.GetComponentInParent<Enemy>().Speed = 0;
                go.GetComponentInParent<Enemy>().StopAllCoroutines();
            }
            if (Score > PlayerPrefs.GetInt("TopScore"))
                PlayerPrefs.SetInt("TopScore", Score);
            InvincibleEnemies = true;
            StartCoroutine(CavLaughANIM(0, -500, 1, false, false));
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
        Shoots = 1;
        Score = 0;
        KillStreak = 1;
        Round = 0;
        TeamScore[0] = 0;
        TeamScore[1] = 0;
        if (Enemies != null)
        {
            foreach (GameObject go in Enemies)
            {
                Destroy(go.transform.parent.gameObject.gameObject);
            }
            Enemies.Clear();
        }
        StatsEnemyMissed = 0;
        StatsShootsMissed = 0;
        StatsMaxKillStreak = 0;
        InvincibleEnemies = false;
        CurrentRangImg.sprite = RangImages[CurrentRang];

        RSNextButton.transform.localPosition = new Vector3(1232, -486.71f);
        RewardScreen.transform.localPosition = new Vector3(0, 2000);
        CavLaughsRestartColl.enabled = false;
        CavLaughsGO.GetComponentInChildren<TextMeshProUGUI>().fontSize = 0;
        CavLaughsGO.transform.localPosition = new Vector2(0, -1200);

        StatUpdate();
        StopAllCoroutines();
        Destroy(MapGO);
        MenuGO.transform.localPosition = Vector2.zero;
        CurrentMode = _CurrentMode.Menu;
        MainMenuAudioGO = MainBridge.CreateSoundGetGO(TVAudioSource, MainMenuAudioAC, _defaultPos.TV, true);
    }
    #endregion

    #region Animations
    private IEnumerator CavLaughANIM(float posX, float posY, float scale, bool isRanked, bool notBad)
    {
        CavLaughsGO.transform.localPosition = new Vector3(posX, -1200);
        CavLaughsGO.transform.localScale = new Vector3(scale, scale);
        yield return new WaitForSeconds(1);
        if(!notBad)
            MainBridge.CreateSoundGetGO(TVAudioSource, CavLaughAC, _defaultPos.TV, true);
        while (CavLaughsGO.transform.localPosition.y <= posY)
        {
            yield return new WaitForSeconds(0.03f);
            CavLaughsGO.transform.localPosition += new Vector3(0, 5000 * Time.deltaTime);
        }
        CavLaughsGO.transform.localPosition = new Vector3(posX, posY);
        int a = 0;
        while (true)
        {
            a++;
            if (a == 8)
            {
                if (isRanked)
                    RSNextButton.transform.localPosition = new Vector3(540, -486.71f);
                else
                {
                    CavLaughsGO.GetComponentInChildren<TextMeshProUGUI>().fontSize = 44;
                    CavLaughsRestartColl.enabled = true;
                }
                a++;
            }
            CavLaughsHeadGO.transform.localPosition = new Vector3(0, CavLaughsHeadGO.transform.localPosition.y == 280 ? 300 : 280);
            yield return new WaitForSeconds(0.12f);
        }
    }
    #endregion
}
