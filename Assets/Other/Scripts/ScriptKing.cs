using System.Collections;
using UnityEngine.Audio;
using UnityEngine;
using System;

public class ScriptKing : MonoBehaviour
{
    [Header("Bridge")]
    [NonSerialized] public RookHuntGameController BF_RHGC;
    [NonSerialized] public static ScriptKing MainBridge;
    [Header("Other")]
    [SerializeField] private GameObject TVVolCircle;
    [Header("CameraGO")]
    [SerializeField] private bool ReadyToShoot = true;
    [SerializeField] private GameObject HitColiderGO;
    [SerializeField] private GameObject CameraGO;
    [SerializeField] private Animator CameraAnimator;
    [SerializeField] private Transform ScreenPos;
    [SerializeField] private Vector3 TVGamesPos = new Vector3(50,0);
    [SerializeField] private float RotSpeed;
    [SerializeField] private float MinRot;
    [SerializeField] private float MaxRot;
    [SerializeField] private int maxFPS;
    [Header("RookHunt")]
    [SerializeField] private GameObject RookHuntMenuPF;
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
    }

    void LateUpdate()
    {
        CameraGO.transform.eulerAngles += new Vector3(-Input.GetAxis("Mouse Y") * RotSpeed, Input.GetAxis("Mouse X") * RotSpeed);
        CameraGO.transform.eulerAngles = new Vector3(Mathf.Clamp((CameraGO.transform.eulerAngles.x > 200 ? -(360 - CameraGO.transform.eulerAngles.x) : CameraGO.transform.eulerAngles.x), MinRot, MaxRot), CameraGO.transform.eulerAngles.y);

        Application.targetFrameRate = maxFPS;//nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            LightGunAS.clip = LightGunClick;
            LightGunAS.Play();
            if (ReadyToShoot)
            {
                ReadyToShoot = false;
                StartCoroutine(ShootCD());
                RaycastHit hit;
                if (Physics.Raycast(CameraGO.transform.position, CameraGO.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
                {
                    switch (hit.transform.gameObject.name)
                    {
                        case "Screen":
                            if (RookHuntMenu != null)
                                Instantiate(HitColiderGO, new Vector3((hit.point.x - ScreenPos.position.x) * 16.7f + TVGamesPos.x, (hit.point.y - ScreenPos.position.y) * 16.7f + TVGamesPos.y, -2), new Quaternion(0, 0, 0, 0));
                            break;
                        case "ResetConcole":
                            Destroy(RookHuntMenu);
                            RookHuntMenu = Instantiate(RookHuntMenuPF, TVGamesPos, new Quaternion(0, 0, 0, 0));
                            BF_RHGC = RookHuntMenu.GetComponent<RookHuntGameController>();
                            break;
                        case "TV_VolUp":
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
                                TVVolCircle.transform.eulerAngles = new Vector3(0, 0, (-curentVol * 3) - 100);
                                UnivrsalAM.audioMixer.SetFloat("TVVol", curentVol);
                                PlayerPrefs.SetFloat("TVVol", curentVol);
                            }
                            break;
                    }
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            LightGunAS.clip = LightGunUnClick;
            LightGunAS.Play();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            CameraAnimator.SetTrigger("ChangeFov");
        }
    }
    private IEnumerator ShootCD()
    {
        yield return new WaitForSeconds(0.1f);
        ReadyToShoot = true;
    }

    public GameObject CreateSoundGetGO(GameObject AudioSource, AudioClip audioClip, _defaultPos defaultPos, Transform ParentTrans = null, bool shouldKillUrSelf = true, Vector3 Position = new Vector3())
    {
        GameObject AU = Instantiate(AudioSource, transform.position, Quaternion.identity);
        AU.transform.SetParent(ParentTrans);
        switch (defaultPos)
        {
            case _defaultPos.TV:
                AU.transform.position = new Vector3(0, 0.025f, -8);
                break;
        }
        AU.GetComponent<SoundPlayer>().enabled = shouldKillUrSelf;
        AU.GetComponent<AudioSource>().clip = audioClip;
        AU.GetComponent<AudioSource>().Play();
        return AU;
    }
}