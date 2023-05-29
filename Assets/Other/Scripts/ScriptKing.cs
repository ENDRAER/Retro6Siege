using System.Collections;
using UnityEngine;

public class ScriptKing : MonoBehaviour
{
    [Header("Bridge")]
    [SerializeField] public static ScriptKing MainBridge;
    [SerializeField] public RookHuntGameController BF_RHGC;
    [SerializeField] public GameObject Canvas2D;
    [SerializeField] public GameObject WorldCanvas;
    [Header("Camera")]
    [SerializeField] private bool ReadyToShoot = true;
    [SerializeField] private GameObject HitColiderGO;
    [SerializeField] private GameObject Camera;
    [SerializeField] private Transform Zero1;
    [SerializeField] private Transform Zero2;
    [SerializeField] private float RotSpeed;
    [SerializeField] private float MinRot;
    [SerializeField] private float MaxRot;
    [SerializeField] private int maxFPS;
    [Header("Audio")]
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
                    if (hit.transform.gameObject.tag == "Screen")
                    {
                        Instantiate(HitColiderGO, new Vector3((hit.point.x - Zero1.position.x) * 3.365f + Zero2.position.x, (hit.point.y - Zero1.position.y) * 3.365f + Zero2.position.y, -2), new Quaternion(0, 0, 0, 0));
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

    public GameObject CreateSoundGetGO(GameObject AudioSource, AudioClip audioClip, _defaultPos defaultPos, bool shouldKillUrSelf, Vector3 Position = new Vector3())
    {
        Vector3 v3 = Position;
        switch (defaultPos)
        {
            case _defaultPos.TV:
                new Vector3(0, 0, 0);
                break;
        }
        GameObject AU = Instantiate(AudioSource, v3, Quaternion.identity);
        AU.GetComponent<AudioSource>().clip = audioClip;
        if (shouldKillUrSelf)
        {
            AU.GetComponent<SoundPlayer>().enabled = true;
        }
        AU.GetComponent<AudioSource>().Play();
        return AU;
    }
}