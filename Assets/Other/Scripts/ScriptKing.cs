using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Audio;
using UnityEngine;
using Unity.Mathematics;
using TMPro;

public class ScriptKing : MonoBehaviour
{
    [Header("Bridge")]
    [NonSerialized] public RookHuntGameController BF_RHGC;
    [NonSerialized] public static ScriptKing MainBridge;
    [Header("Other")]
    [SerializeField] private GameObject LampLight;
    [SerializeField] private GameObject OtsideLight;
    [Header("Paper")]
    [SerializeField] public TextMeshProUGUI[] ModText;
    [SerializeField] public GameObject[] CheckBoxes;
    [SerializeField] public GameObject[] CheckMarks;
    [SerializeField] private Animator PaperAnim;
    [SerializeField] public enum _ObjectType { Screen, ResetConcole, TV_VolUp, LightSwitch, PaperWithModifers, ModButton, Settings, Exit };
    [NonSerialized] public bool InfiniteAmmo;
    [NonSerialized] public bool FullAutoShoting;
    [NonSerialized] public bool NoOpLeft;
    [NonSerialized] public bool LargeBullet;
    [NonSerialized] public bool AllEnemyAreAsh;
    [NonSerialized] public bool NoMoreShieldHitBox;
    [NonSerialized] public bool NoMoreLosingKillStreak;
    [NonSerialized] public bool glock;
    [NonSerialized] public bool doom1993;
    [Header("Camera")]
    [SerializeField] private Vector3 DeffaultCamRot;
    [SerializeField] private SpriteRenderer LaserMark;
    [SerializeField] private SpriteRenderer LaserHalo;
    [SerializeField] private GameObject CameraGO;
    [SerializeField] private GameObject PistolTrigger;
    [SerializeField] private Animator CameraAnimator;
    [SerializeField] private Transform ScreenPos;
    [SerializeField] private Vector3 TVGamesPos = new(50, 0);
    [SerializeField] private float RotSpeed;
    [SerializeField] private float MinRot;
    [SerializeField] private float MaxRot;
    [NonSerialized] private bool ReadyToShot = true;
    [NonSerialized] private bool Focused = false;
    [Header("RookHunt")]
    [SerializeField] private GameObject RookHuntMenuPF;
    [SerializeField] private GameObject HitColiderGO;
    [NonSerialized] private GameObject RookHuntMenu;
    [Header("Audio")]
    [SerializeField] private AudioMixerGroup UnivrsalAM;
    [SerializeField] private AudioSource LightGunAS;
    [SerializeField] private AudioClip LightGunClick;
    [SerializeField] private AudioClip LightGunUnClick;
    [Header("SettingsUI")]
    [SerializeField] private GameObject SettCanvasGO;
    [SerializeField] private UnityEngine.UI.Toggle FullScreenToggle;
    [SerializeField] private UnityEngine.UI.Toggle VSyncToggle;
    [SerializeField] private TMP_InputField MaxFPSIF;
    [SerializeField] private TMP_InputField SensitivityIF;
    [SerializeField] private TMP_Dropdown ResolutionDD;
    [NonSerialized] private List<Resolution> AllResolutions = new ();

    public enum _defaultPos { Custom, TV };

    private void Awake()
    {
        RookHuntMenu = Instantiate(RookHuntMenuPF, TVGamesPos, new Quaternion(0, 0, 0, 0));
        BF_RHGC = RookHuntMenu.GetComponent<RookHuntGameController>();
        MainBridge = this;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        #region CheckModifersProgress
        BuffersCounter(0, "ShotTimes", 100, 0, "Shoot 100 times\n", "Infinite ammo");
        BuffersCounter(1, "ShotTimes", 1000, 0, "shoot 1000 times\n", "Full auto");
        BuffersCounter(2, "MissedEnemies", 20, 0, "Let 20 attackers escape\n", "Don't lose ammo when attackers escape");
        BuffersCounter(3, "MultipleKills", 5, 0, "hit two attackers with one shot 5 times", "Big bullets");
        BuffersCounter(4, "AshKills", 20, 0, "Kill Ash 20 times \n", "Ashpocalipse\n(infinite game mode only)");
        BuffersCounter(5, "ShieldHits", 25, 0, "Shoot any shield 25 times", "Instant shield break");
        BuffersCounter(6, "KillSteakEarned", 1, 0, "Get a 20 kill streak", "Don't lose killstreak on miss");
        BuffersCounter(7, "ChampionEarned", 1, 0, "Get champion rank", "GLOCK");
        BuffersCounter(8, "DoomUnlocked", 1, 0, "Secret", "Back to the 90's");
        #endregion

        #region SetSettings
        if (PlayerPrefs.GetInt("FirstStart") == 0)
        {
            PlayerPrefs.SetInt("VSync", 1);
            PlayerPrefs.SetInt("MaxFPS", 60);
            PlayerPrefs.SetFloat("Sensitivity", 0.8f);
            SettCanvasGO.SetActive(true);
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            PlayerPrefs.SetInt("FirstStart", 1);
        }
        FullScreenToggle.isOn = Screen.fullScreen;

        int VS = PlayerPrefs.GetInt("VSync");
        VSyncToggle.isOn = VS == 1;
        QualitySettings.vSyncCount = VS;
        int FPasS = PlayerPrefs.GetInt("MaxFPS");
        MaxFPSIF.text = FPasS.ToString();
        Application.targetFrameRate = FPasS;
        float SensaEbat = PlayerPrefs.GetFloat("Sensitivity");
        SensitivityIF.text = SensaEbat.ToString();
        RotSpeed = SensaEbat;
        #endregion
    }

    void LateUpdate()
    {
        if (SettCanvasGO.activeSelf) return;
        if (Application.platform == RuntimePlatform.Android)
            CameraGO.transform.rotation = Quaternion.Lerp(transform.rotation, new quaternion(Input.acceleration.y * 90, Input.acceleration.x * -90, 0, 0), 0.5f  * Time.deltaTime);
        else
            CameraGO.transform.eulerAngles += new Vector3(-Input.GetAxis("Mouse Y") * (RotSpeed / (Focused ? 2 : 1)), Input.GetAxis("Mouse X") * (RotSpeed / (Focused ? 2 : 1)));
        CameraGO.transform.eulerAngles = new Vector3(Mathf.Clamp((CameraGO.transform.eulerAngles.x > 200 ? -(360 - CameraGO.transform.eulerAngles.x) : CameraGO.transform.eulerAngles.x), MinRot, MaxRot), CameraGO.transform.eulerAngles.y);

        if (Physics.Raycast(CameraGO.transform.position, CameraGO.transform.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity) && hit.transform.GetComponent<InteractableObjects>() != null)
        {
            InteractableObjects _object = hit.transform.GetComponent<InteractableObjects>();
            #region ColorOfPointer
            if (_object.ObjectType != _ObjectType.Screen)
            {
                LaserHalo.color = new Color(0, 1, 0, 0.6f);
                LaserMark.color = new Color(0, 1, 0, 0.6f);
            }
            else
            {
                LaserHalo.color = new Color(1, 0, 0, 0.6f);
                LaserMark.color = new Color(1, 0, 0, 0.6f);
            }
            #endregion
            if (ReadyToShot && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse0) && FullAutoShoting && hit.transform.GetComponent<InteractableObjects>().ObjectType == _ObjectType.Screen))
            {
                ReadyToShot = false;
                StartCoroutine(ShotCD());
                switch (hit.transform.GetComponent<InteractableObjects>().ObjectType)
                {
                    case _ObjectType.Screen:
                        Instantiate(HitColiderGO, new((hit.point.x - ScreenPos.position.x) * (13.333333f / ScreenPos.localScale.x) + TVGamesPos.x, (hit.point.y - ScreenPos.position.y) * (10 / ScreenPos.localScale.y) + TVGamesPos.y, -2), new Quaternion(0, 0, 0, 0)); break;
                    case _ObjectType.ResetConcole:
                        Destroy(RookHuntMenu);
                        RookHuntMenu = Instantiate(RookHuntMenuPF, TVGamesPos, new Quaternion(0, 0, 0, 0));
                        BF_RHGC = RookHuntMenu.GetComponent<RookHuntGameController>();
                        break;
                    case _ObjectType.TV_VolUp:
                        {
                            /*
                            UnivrsalAM.audioMixer.GetFloat("TVVol", out float curentVol);
                            switch (curentVol)
                            {
                                case -80:
                                    curentVol = -40;
                                    break;
                                case -40:
                                    curentVol = -20;
                                    break;
                                case -20:
                                    curentVol = -10;
                                    break;
                                case -10:
                                    curentVol = -5;
                                    break;
                                case -5:
                                    curentVol = 0;
                                    break;
                                case 0:
                                    curentVol = -80;
                                    break;
                            }
                            //TVVolCircle.transform.eulerAngles = new(0, 0, (-curentVol * 3) - 100);
                            UnivrsalAM.audioMixer.SetFloat("TVVol", curentVol);
                            PlayerPrefs.SetFloat("TVVol", curentVol);
                            */
                        }
                        break;
                    case _ObjectType.LightSwitch:
                        LampLight.SetActive(!LampLight.activeSelf);
                        OtsideLight.SetActive(!OtsideLight.activeSelf);
                        hit.collider.transform.localEulerAngles = new Vector3 (0, LampLight.activeSelf ? -10 : 10, 0);
                        break;
                    case _ObjectType.PaperWithModifers:
                        PaperAnim.SetBool("Focussed", _object.modifer == 0);
                        break;
                    case _ObjectType.ModButton:
                        switch (_object.modifer)
                        {
                            case 0:
                                InfiniteAmmo = !InfiniteAmmo;
                                CheckMarks[0].SetActive(InfiniteAmmo);
                                break;
                            case 1:
                                FullAutoShoting = !FullAutoShoting;
                                CheckMarks[1].SetActive(FullAutoShoting);
                                break;
                            case 2:
                                NoOpLeft = !NoOpLeft;
                                CheckMarks[2].SetActive(NoOpLeft);
                                break;
                            case 3:
                                LargeBullet = !LargeBullet;
                                CheckMarks[3].SetActive(LargeBullet);
                                break;
                            case 4:
                                AllEnemyAreAsh = !AllEnemyAreAsh;
                                CheckMarks[4].SetActive(AllEnemyAreAsh);
                                break;
                            case 5:
                                NoMoreShieldHitBox = !NoMoreShieldHitBox;
                                CheckMarks[5].SetActive(NoMoreShieldHitBox);
                                break;
                            case 6:
                                NoMoreLosingKillStreak = !NoMoreLosingKillStreak;
                                CheckMarks[6].SetActive(NoMoreLosingKillStreak);
                                break;
                        }
                        break;
                    case _ObjectType.Settings:
                        UnityEngine.Cursor.lockState = CursorLockMode.None;
                        SettCanvasGO.SetActive(!SettCanvasGO.activeSelf);
                        ResolutionDD.ClearOptions();
                        AllResolutions.AddRange(Screen.resolutions);
                        List<string> res = new();
                        foreach(Resolution r in AllResolutions)
                            res.Add(r.ToString());
                        ResolutionDD.AddOptions(res);
                        break;
                    case _ObjectType.Exit:
                        Application.Quit();
                        break;
                }
            }

        }
        else
        {
            LaserHalo.color = new Color(1, 0, 0, 0.6f);
            LaserMark.color = new Color(1, 0, 0, 0.6f);
        }

        #region ClickSound
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            PistolTrigger.transform.localEulerAngles = new Vector3(0, 0, 15);
            LightGunAS.clip = LightGunClick;
            LightGunAS.Play();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            PistolTrigger.transform.localEulerAngles = new Vector3(0, 0, 0);
            LightGunAS.clip = LightGunUnClick;
            LightGunAS.Play();
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            CameraAnimator.SetTrigger("ChangeFov");
        }
    }
    private IEnumerator ShotCD()
    {
        yield return new WaitForSeconds(FullAutoShoting ? 0.08f : 0.16f);
        ReadyToShot = true;
    }
    public void SetFocused(int boolean)
    {
        Focused = boolean == 1;
    }

    public GameObject CreateSoundGetGO(GameObject AudioSource, AudioClip audioClip, _defaultPos defaultPos, Transform ParentTrans = null, bool shouldKillUrSelf = true, Vector3 _Position = new())
    {
        GameObject AU = Instantiate(AudioSource, transform.position, Quaternion.identity);
        AU.transform.SetParent(ParentTrans);
        switch (defaultPos)
        {
            case _defaultPos.TV:
                AU.transform.position = new(0, 0.025f, -8);
                break;
            case _defaultPos.Custom:
                AU.transform.position = _Position;
                break;
        }
        AU.GetComponent<SoundPlayer>().enabled = shouldKillUrSelf;
        AU.GetComponent<AudioSource>().clip = audioClip;
        AU.GetComponent<AudioSource>().Play();
        return AU;
    }

    public void BuffersCounter(int BuffId, string PlPrName, float HowMuch, int add, string CounterText, string EarnedText)
    {
        float Counter = PlayerPrefs.GetInt(PlPrName) + add;
        if (add != 0)
            PlayerPrefs.SetInt(PlPrName, (int)(Counter));
        if (Counter >= HowMuch)
        {
            CheckBoxes[BuffId].SetActive(true);
            ModText[BuffId].text = EarnedText;
        }
        else if (HowMuch != 1) 
            ModText[BuffId].text = CounterText + new string('x', (int)(Counter / (HowMuch / 10))) + new string('-', (int)(10 - Counter / (HowMuch / 10)));
    }

    #region SettingsUI
    public void SetVsync(bool _VSync)
    {
        QualitySettings.vSyncCount = _VSync? 1 : 0;
        PlayerPrefs.SetInt("VSync", QualitySettings.vSyncCount);
    }
    public void SetFullScreen(bool _FullScreen)
    {
        Screen.fullScreen = _FullScreen;
    }
    public void SetMaxFPS(string _MaxFPSstr)
    {
        int FPasS = int.Parse(_MaxFPSstr);
        MaxFPSIF.text = FPasS.ToString();
        if (FPasS < 1)
            FPasS = 1;
        Application.targetFrameRate = FPasS;
        PlayerPrefs.SetInt("MaxFPS", FPasS);
    }
    public void SetSensitivity(string _Sensitivity_str)
    {
        float SensaEbat = math.clamp(float.Parse(_Sensitivity_str), 0.05f, 20);
        SensitivityIF.text = SensaEbat.ToString();
        RotSpeed = SensaEbat;
        PlayerPrefs.SetFloat("Sensitivity", SensaEbat);
    }
    public void SetRes(int ResolID)
    {
        Screen.SetResolution(AllResolutions[ResolID].width, AllResolutions[ResolID].height, Screen.fullScreen);
    }
    public void CloseSettingsUI()
    {
        SettCanvasGO.SetActive(false);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion
}