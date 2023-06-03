using System.Collections;
using Unity.Mathematics;
using UnityEngine.Audio;
using UnityEngine;
using System;

public class ScriptKing : MonoBehaviour
{
    [Header("Bridge")]
    [SerializeField] public GameObject WorldCanvas;
    [NonSerialized] public RookHuntGameController BF_RHGC;
    [NonSerialized] public static ScriptKing MainBridge;
    [Header("Camera")]
    [SerializeField] private bool ReadyToShoot = true;
    [SerializeField] private GameObject HitColiderGO;
    [SerializeField] private GameObject Camera;
    [SerializeField] private Transform ScreenPos;
    [SerializeField] private Vector3 TVGamesPos = new Vector3(50,0);
    [SerializeField] private float RotSpeed;
    [SerializeField] private float MinRot;
    [SerializeField] private float MaxRot;
    [SerializeField] private int maxFPS;
    [Header("RookHunt")]
    [SerializeField] private GameObject RookHuntMenuPF;
    [SerializeField] private GameObject RookHuntMenu;
    [Header("Audio")]
    [SerializeField] private AudioMixerGroup UnivrsalAM;
    [SerializeField] private AudioSource LightGunAS;
    [SerializeField] private AudioClip LightGunClick;
    [SerializeField] private AudioClip LightGunUnClick;

    public enum _defaultPos { Custom, TV };

    private void Awake()
    {
        MainBridge = this;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        Camera.transform.eulerAngles += new Vector3(-Input.GetAxis("Mouse Y") * RotSpeed, Input.GetAxis("Mouse X") * RotSpeed);
        Camera.transform.eulerAngles = new Vector3(Mathf.Clamp((Camera.transform.eulerAngles.x > 200 ? -(360 - Camera.transform.eulerAngles.x) : Camera.transform.eulerAngles.x), MinRot, MaxRot), Camera.transform.eulerAngles.y);

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
                if (Physics.Raycast(Camera.transform.position, Camera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
                {
                    switch (hit.transform.gameObject.name)
                    {
                        case "Screen":
                            if (RookHuntMenu != null)
                                Instantiate(HitColiderGO, new Vector3((hit.point.x - ScreenPos.position.x) * 3.365f + TVGamesPos.x, (hit.point.y - ScreenPos.position.y) * 3.365f + TVGamesPos.y, -2), new Quaternion(0, 0, 0, 0));
                            break;
                        case "ResetConcole":
                            if (RookHuntMenu != null)
                            {
                                Destroy(RookHuntMenu);
                            }
                            RookHuntMenu = Instantiate(RookHuntMenuPF, TVGamesPos, new Quaternion(0, 0, 0, 0));
                            BF_RHGC = RookHuntMenu.GetComponent<RookHuntGameController>();
                            break;
                        case "TV_VolUp":
                            {
                                UnivrsalAM.audioMixer.GetFloat("TV", out float curentVol);
                                print(math.clamp(curentVol + 20, -80, 20));
                                UnivrsalAM.audioMixer.SetFloat("TV", math.clamp(curentVol + 20,-80,20));
                            }
                            break;
                        case "TV_VolDowm":
                            {
                                UnivrsalAM.audioMixer.GetFloat("TV", out float curentVol);
                                print(math.clamp(curentVol - 20, -80, 20));
                                UnivrsalAM.audioMixer.SetFloat("TV", math.clamp(curentVol - 20, -80, 20));
                            }//dont delete these {}
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
                AU.transform.position = new Vector3();
                break;
        }
        AU.GetComponent<SoundPlayer>().enabled = shouldKillUrSelf;
        AU.GetComponent<AudioSource>().clip = audioClip;
        AU.GetComponent<AudioSource>().Play();
        return AU;
    }
}