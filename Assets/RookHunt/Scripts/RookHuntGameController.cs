using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using static ScriptKing;
using UnityEditor;

public class RookHuntGameController : MonoBehaviour
{
    [NonSerialized] public int CurrentRang = 0;
    [NonSerialized] public GameObject MapGO;
    [NonSerialized] private MapScript MapCS;
    [NonSerialized] public List<WayCreator> Ways;
    [NonSerialized] public List<WayCreator> WaysDef;
    [NonSerialized] private WayCreator SnipersWay;
    [NonSerialized] public ScriptKing _SK;
    [SerializeField] private GameObject[] MapsPF;
    [SerializeField] public List<GameObject> EnemyPF;
    [SerializeField] private List<GameObject> SpecialEnemyPF;
    [SerializeField] private List<GameObject> EnemyDefPF;
    [SerializeField] private GameObject TheAshOne;
    [SerializeField] public GameObject DynamicCanvas;
    [SerializeField] public GameObject Alibi;
    [NonSerialized] public List<GameObject> Enemies = new();
    [NonSerialized] public bool InvincibleEnemies;
    [NonSerialized] public double Shots = 3;
    [NonSerialized] public int KillStreak = 0;
    [NonSerialized] public int Score;
    [SerializeField] public enum _CurrentMode { Menu, GameOver, Ranked, Infinite }
    [SerializeField] public _CurrentMode CurrentMode = _CurrentMode.Menu;
    [NonSerialized] public int ShootTimesPerMatch;
    [NonSerialized] public float InfModeSpeedScaler = 1;
    [NonSerialized] public float InfModeSpawnSpeedScaler = 1;
    [Header("UI")]
    [SerializeField] public GameObject MenuCanvasGO;
    [SerializeField] public Image CurrentRangImg;
    [SerializeField] public TextMeshProUGUI TopRecordText;
    [SerializeField] public GameObject CavPortraitGO;
    [SerializeField] public GameObject CavLaughsGO;
    [SerializeField] public GameObject CavLaughsHeadGO;
    [SerializeField] public GameObject CavNotBadGO;
    [SerializeField] public Collider2D CavRestartColl;
    [SerializeField] public TextMeshProUGUI ShootToRestartTXT;
    [SerializeField] public TextMeshProUGUI CavNewRecordCallerTXT;
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
    [SerializeField] public GameObject LoadingScreenCanvas; // LS - LoadingScreenCanvas
    [SerializeField] public TextMeshProUGUI LSRounds;
    [SerializeField] public TextMeshProUGUI LSTeamScore;
    [SerializeField] public TextMeshProUGUI LSTeamRole;
    [SerializeField] public GameObject LSTeamIconCenter;
    [SerializeField] public GameObject LSTeam1IcoGO;
    [SerializeField] public GameObject LSTeam2IcoGO;
    [SerializeField] public Image LSTeam1IcoImg;
    [SerializeField] public Image LSTeam2IcoImg;
    [SerializeField] public Sprite LSDefendSpr;
    [SerializeField] public Sprite LSAtackSpr;
    [SerializeField] public GameObject Outro;
    [SerializeField] public TextMeshProUGUI OutroRoundStatusText;
    [SerializeField] public TextMeshProUGUI OutroReasonOfEndText;
    [SerializeField] public GameObject RewardScreenCanvas; // RS
    [SerializeField] public Animator RSAnimator;
    [SerializeField] public Image[] RSRangs;
    [SerializeField] public Sprite[] RangImages;
    [SerializeField] public Image RSGradiend;
    [SerializeField] public TextMeshProUGUI RSMatchReport;
    [SerializeField] public GameObject RSNextButton;
    [NonSerialized] public int KillStreakPerRound = 0;
    [NonSerialized] private bool IsDefender = true;
    [NonSerialized] private int Round = 0;
    [NonSerialized] private int[] TeamScore = { 0, 0 };
    [NonSerialized] public int StatsMaxKillStreak;
    [NonSerialized] public int StatsShotsMissed;
    [NonSerialized] public int StatsEnemyMissed;
    [Header("Audios")]
    [SerializeField] public AudioClip MainMenuAudioAC;
    [NonSerialized] public GameObject MainMenuAudioGO;
    [SerializeField] public GameObject TVAudioSource;
    [SerializeField] public AudioClip ShotSound;
    [SerializeField] public AudioClip EnemySpotAC;
    [SerializeField] public AudioClip RunningSound;
    [SerializeField] public AudioClip HitSound;
    [SerializeField] public AudioClip EnemyHitSound;
    [SerializeField] public AudioClip EnemyMissedSound;
    [SerializeField] public AudioClip StartRoundMusic;
    [SerializeField] public AudioClip DefeatMusic;
    [SerializeField] public AudioClip WinMusic;
    [SerializeField] public AudioClip WinMusic_Perfect;
    [SerializeField] public AudioClip CavLaughAC;
    [SerializeField] public AudioClip AlibiKillingAC;
    [SerializeField] public AudioClip IanaMissingAC;
    [SerializeField] public AudioClip OsasShieldCrashAC;
    [SerializeField] public AudioClip YingFlash;


    private void Start()
    {
        _SK = MainBridge;
        MenuCanvasGO.SetActive(true);
        CurrentRang = math.clamp(PlayerPrefs.GetInt("CurrentRang"), 0, RangImages.Length - 1);
        CurrentRangImg.sprite = RangImages[CurrentRang];
        TopRecordText.text = "TOP SCORE = " + PlayerPrefs.GetInt("TopScore");
        MainMenuAudioGO = MainBridge.CreateSoundGetGO(TVAudioSource, MainMenuAudioAC, _defaultPos.TV, transform);
    }

    private void MapCreator()
    {
        MapGO = Instantiate(MapsPF[UnityEngine.Random.Range(0, MapsPF.Length - 1)], new(50, 1.5f, 0), Quaternion.identity);
        MapGO.transform.SetParent(transform);
        MapCS = MapGO.GetComponent<MapScript>();
        Ways = MapCS.Ways;
        WaysDef = MapCS.WaysDef;
        SnipersWay = MapCS.SnipersWay;
        MenuCanvasGO.SetActive(false);
    }

    #region Infinite Mode
    public void InfiniteModeStart()
    {
        InfModeSpeedScaler = 1;
        InfModeSpawnSpeedScaler = 1;
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
        int enemyID = UnityEngine.Random.Range(0, EnemyPF.Count - 1);
        wayID = UnityEngine.Random.Range(0, Ways.Count - 1);
        _EnemyGO = Instantiate(_SK.AllEnemyAreAsh ? TheAshOne : EnemyPF[enemyID], Ways[wayID].transform.position, Quaternion.identity);
        _EnemyCS = _EnemyGO.GetComponent<Enemy>();

        GameObject AUGO = _SK.CreateSoundGetGO(TVAudioSource, RunningSound, ScriptKing._defaultPos.TV, transform, false);
        AudioSource AUAU = AUGO.GetComponent<AudioSource>();
        AUAU.GetComponent<AudioSource>().clip = RunningSound;
        _EnemyGO.transform.SetParent(AUAU.transform);

        _EnemyCS.Speed *= InfModeSpeedScaler;
        _EnemyCS.AnimDelay *= InfModeSpeedScaler;
        if (InfModeSpeedScaler <= 1.4)
            InfModeSpeedScaler += 0.01f;
        if (InfModeSpawnSpeedScaler <= 2)
            InfModeSpawnSpeedScaler += 0.01f;

        if (_EnemyCS._WayCreator == null)
            _EnemyCS._WayCreator = Ways[wayID];
        _EnemyCS.RHGC = this;
        _EnemyCS.Perspective = MapCS.Perspective;
        Enemies.Add(_EnemyGO);

        yield return new WaitForSeconds(1.5f / InfModeSpawnSpeedScaler);
        StartCoroutine(EnemySpawn());

    }
    #endregion

    #region Update Values
    public void StatUpdate()
    {
        MagazineUpdate();
        MultiplierText.text = KillStreak + " STREAK (" + (double)math.clamp(1 + KillStreak * 0.2, 0, 20) + "x Multiplier)";
        ScoreText.text = Score.ToString();
    }

    public void MagazineUpdate()
    {
        Shots = math.clamp(Shots, -2, 1);
        for (int i = 0; i < BulletsImg.Length; i++)
        {
            BulletsImg[i].fillAmount = i + (float)Shots;
        }

        if (CurrentMode == _CurrentMode.Infinite && Shots < -1)
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
    }

    public void ExitInMainMenu()
    {
        Shots = 1;
        Score = 0;
        KillStreak = 0;
        Round = 0;
        TeamScore[0] = 0;
        TeamScore[1] = 0;
        StatsEnemyMissed = 0;
        StatsShotsMissed = 0;
        StatsMaxKillStreak = 0;
        if (Enemies != null)
        {
            foreach (GameObject go in Enemies)
            {
                Destroy(go.transform.parent.gameObject);
            }
            Enemies.Clear();
        }
        InvincibleEnemies = false;
        CurrentRangImg.sprite = RangImages[CurrentRang];

        RSNextButton.transform.localPosition = new(1232, -486.71f);
        RewardScreenCanvas.SetActive(false);
        CavRestartColl.enabled = false;
        ShootToRestartTXT.fontSize = 0;
        CavPortraitGO.transform.localPosition = new(0, -1200, -0.1f);
        CavLaughsGO.transform.localScale = new(1, 0);
        CavNotBadGO.transform.localScale = new(1, 0);
        CavNewRecordCallerTXT.text = "";

        StatUpdate();
        StopAllCoroutines();
        Destroy(MapGO);
        MenuCanvasGO.SetActive(true);
        CurrentMode = _CurrentMode.Menu;
        MainMenuAudioGO = MainBridge.CreateSoundGetGO(TVAudioSource, MainMenuAudioAC, _defaultPos.TV, transform);
    }
    #endregion

    #region Animations
    private IEnumerator CavLaughANIM(float posX, float posY, float scale, bool isRanked, bool notBad)
    {
        if (!isRanked && Score >= PlayerPrefs.GetInt("TopScore"))
        {
            notBad = true;
            TopRecordText.text = "TOP SCORE = " + Score;
        }
        _SK.BuffersCounter(0, "ShotTimes", 100, ShootTimesPerMatch, "Shoot 100 times\n", "Infinite ammo");
        _SK.BuffersCounter(1, "ShotTimes", 1000, 0, "shoot 1000 times\n", "Full auto");

        CavPortraitGO.transform.localPosition = new(posX, -1200, -0.1f);
        CavPortraitGO.transform.localScale = new(scale, scale);
        yield return new WaitForSeconds(1);
        if (notBad)
        {
            MainBridge.CreateSoundGetGO(TVAudioSource, WinMusic_Perfect, _defaultPos.TV, transform);
            CavNotBadGO.transform.localScale = new Vector2(1, 1);
        }
        else
        {
            MainBridge.CreateSoundGetGO(TVAudioSource, CavLaughAC, _defaultPos.TV, transform);
            CavLaughsGO.transform.localScale = new Vector2(1, 1);
        }

        while (CavPortraitGO.transform.localPosition.y <= posY)
        {
            yield return new WaitForSeconds(0.03f);
            CavPortraitGO.transform.localPosition += new Vector3(0, 50, -0.1f);
        }
        CavPortraitGO.transform.localPosition = new(posX, posY, -0.1f);
        if (!isRanked && notBad)
            CavNewRecordCallerTXT.text = "NEW RECORD: " + Score + "!";
        int a = 0;
        while (true)
        {
            a++;
            if (a == 8)
            {
                if (isRanked)
                    RSNextButton.transform.localPosition = new(540, -486.71f);
                else
                {
                    ShootToRestartTXT.fontSize = 0.001f;
                    CavRestartColl.enabled = true;
                }
                a++;
            }
            if (!isRanked && a % 3 == 0)
                ShootToRestartTXT.fontSize = ShootToRestartTXT.fontSize == 0.001f ? 60 : 0.001f;
            CavLaughsHeadGO.transform.localPosition = new(0, CavLaughsHeadGO.transform.localPosition.y == 280 ? 300 : 280);
            yield return new WaitForSeconds(0.12f);
        }
    }
    #endregion
}
