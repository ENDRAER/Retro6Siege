using System.Collections;
using UnityEngine.Audio;
using UnityEngine;
using System;
using TMPro;
using System.Linq;

public class ScriptKing : MonoBehaviour
{
    [Header("Bridge")]
    [NonSerialized] public RookHuntGameController BF_RHGC;
    [NonSerialized] public static ScriptKing MainBridge;
    [Header("Other")]
    [SerializeField] private GameObject TVVolCircle;
    [SerializeField] private Light LampLight;
    [Header("Paper")]
    [SerializeField] public TextMeshProUGUI[] ModText;
    [SerializeField] public GameObject[] CheckBoxes;
    [SerializeField] public GameObject[] CheckMarks;
    [SerializeField] private Animator PaperAnim;
    [SerializeField] public enum _ObjectType { Screen, ResetConcole, TV_VolUp, LightSwitch, PaperWithModifers, ModButton };
    [NonSerialized] public bool InfiniteAmmo;
    [NonSerialized] public bool FullAutoShooting;
    [NonSerialized] public bool NoOpLeft;
    [NonSerialized] public bool BigBullet;
    [NonSerialized] public bool AllEnemyAreAsh;
    [NonSerialized] public bool NoMoreShieldHitBox;
    [NonSerialized] public bool NoMoreLosingKillStreak;
    [NonSerialized] public bool glock;
    [NonSerialized] public bool doom1993;
    [Header("CameraGO")]
    [SerializeField] private bool ReadyToShoot = true;
    [SerializeField] private SpriteRenderer LaserMark;
    [SerializeField] private GameObject CameraGO;
    [SerializeField] private Animator CameraAnimator;
    [SerializeField] private Transform ScreenPos;
    [SerializeField] private Vector3 TVGamesPos = new(50, 0);
    [SerializeField] private float RotSpeed;
    [SerializeField] private float MinRot;
    [SerializeField] private float MaxRot;
    [Header("RookHunt")]
    [SerializeField] private GameObject RookHuntMenuPF;
    [SerializeField] private GameObject HitColiderGO;
    [NonSerialized] private GameObject RookHuntMenu;
    [Header("Audio")]
    [SerializeField] private AudioMixerGroup UnivrsalAM;
    [SerializeField] private AudioSource LightGunAS;
    [SerializeField] private AudioClip LightGunClick;
    [SerializeField] private AudioClip LightGunUnClick;

    public enum _defaultPos { Custom, TV };

    private void Awake()
    {
        RookHuntMenu = Instantiate(RookHuntMenuPF, TVGamesPos, new Quaternion(0, 0, 0, 0));
        BF_RHGC = RookHuntMenu.GetComponent<RookHuntGameController>();
        UnivrsalAM.audioMixer.SetFloat("TVVol", PlayerPrefs.GetFloat("TVVol"));
        TVVolCircle.transform.eulerAngles = new Vector3(0, 0, (-PlayerPrefs.GetFloat("TVVol") * 3) - 100);
        MainBridge = this;
        Cursor.lockState = CursorLockMode.Locked;
        #region CheckModifersProgress
        BuffersCounter(0, "ShootTimes", 100, 0, "Shoot for 100 times\n", "infinite ammo");
        BuffersCounter(1, "ShootTimes", 1000, 1000, "Shoot for 1000 times\n", "full auto shooting");
        BuffersCounter(2, "MissedEnemies", 20, 0, "miss 20 atackers\n", "missing enemies do not stole ammo");
        BuffersCounter(4, "AshKills", 20, 0, "kill Ash for 20 times \n", "all enemies are Ash now\n(infinite game mode only)");
        BuffersCounter(5, "ShieldHits", 25, 0, "shoot to the shield for 25 times ", "no more shield hitbox");
        BuffersCounter(6, "KillSteakEarned", 1, 0, "Get Streak of 20 kills", "no more losing kill streak");
        BuffersCounter(7, "ChampionEarned", 1, 0, "beat champion", "GLOCK");
        BuffersCounter(8, "DoomUnlocked", 1, 0, "secret", "back to the 1993");
        #endregion
    }

    void LateUpdate()
    {
        CameraGO.transform.eulerAngles += new Vector3(-Input.GetAxis("Mouse Y") * RotSpeed, Input.GetAxis("Mouse X") * RotSpeed);
        CameraGO.transform.eulerAngles = new Vector3(Mathf.Clamp((CameraGO.transform.eulerAngles.x > 200 ? -(360 - CameraGO.transform.eulerAngles.x) : CameraGO.transform.eulerAngles.x), MinRot, MaxRot), CameraGO.transform.eulerAngles.y);

        if (Physics.Raycast(CameraGO.transform.position, CameraGO.transform.TransformDirection(Vector3.forward), out RaycastHit hit, Mathf.Infinity) && hit.transform.GetComponent<InteractableObjects>() != null)
        {
            InteractableObjects _object = hit.transform.GetComponent<InteractableObjects>();
            #region ColorOfPointer
            if (_object.ObjectType != _ObjectType.Screen)
                LaserMark.color = new Color(0, 1, 0, 0.6f);
            else
                LaserMark.color = new Color(1, 0, 0, 0.6f);
            #endregion
            if (ReadyToShoot && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse0) && FullAutoShooting && hit.transform.GetComponent<InteractableObjects>().ObjectType == _ObjectType.Screen))
            {
                ReadyToShoot = false;
                StartCoroutine(ShootCD());
                switch (hit.transform.GetComponent<InteractableObjects>().ObjectType)
                {
                    case _ObjectType.Screen:
                        Instantiate(HitColiderGO, new((hit.point.x - ScreenPos.position.x) * 16.66f + TVGamesPos.x, (hit.point.y - ScreenPos.position.y) * 16.66f + TVGamesPos.y, -2), new Quaternion(0, 0, 0, 0)); break;
                    case _ObjectType.ResetConcole:
                        Destroy(RookHuntMenu);
                        RookHuntMenu = Instantiate(RookHuntMenuPF, TVGamesPos, new Quaternion(0, 0, 0, 0));
                        BF_RHGC = RookHuntMenu.GetComponent<RookHuntGameController>();
                        break;
                    case _ObjectType.TV_VolUp:
                        {
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
                            TVVolCircle.transform.eulerAngles = new(0, 0, (-curentVol * 3) - 100);
                            UnivrsalAM.audioMixer.SetFloat("TVVol", curentVol);
                            PlayerPrefs.SetFloat("TVVol", curentVol);
                        }
                        break;
                    case _ObjectType.LightSwitch:
                        LampLight.intensity = LampLight.intensity == 1 ? 0 : 1;
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
                                FullAutoShooting = !FullAutoShooting;
                                CheckMarks[1].SetActive(FullAutoShooting);
                                break;
                            case 2:
                                NoOpLeft = !NoOpLeft;
                                CheckMarks[2].SetActive(NoOpLeft);
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
                }
            }

        }
        else
            LaserMark.color = new Color(1, 0, 0, 0.6f);

        #region ClickSound
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            LightGunAS.clip = LightGunClick;
            LightGunAS.Play();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            LightGunAS.clip = LightGunUnClick;
            LightGunAS.Play();
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            CameraAnimator.SetTrigger("ChangeFov");
        }
    }
    private IEnumerator ShootCD()
    {
        yield return new WaitForSeconds(FullAutoShooting ? 0.08f : 0.16f);
        ReadyToShoot = true;
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
}