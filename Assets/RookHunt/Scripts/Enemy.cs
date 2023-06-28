using System.Collections;
using UnityEngine;
using System;
using static ScriptKing;

public class Enemy : MonoBehaviour
{
    [NonSerialized] public int id;
    [NonSerialized] public RookHuntGameController RHGC;
    [NonSerialized] private int Step;
    [NonSerialized] public float[] Perspective = { 1.77f, 0.2f}; // default values
    [NonSerialized] public WayCreator _WayCreator;
    [NonSerialized] public bool IsOnKaliWay;
    [SerializeField] public enum _EnemyType { Standart, Sniper, Blitz, Ying, Osa, Iana, IanaClone, Alibi, Ash}
    [SerializeField] public _EnemyType EnemyType;
    [SerializeField] private GameObject BalancerGO;
    [SerializeField] private Rigidbody2D RB2D;
    [SerializeField] public Collider2D HitCollider;
    [SerializeField] public enum _WalkType { Straigh, Stop, BetweenPoints }
    [SerializeField] public _WalkType WalkType = _WalkType.Straigh;
    [SerializeField] public float Speed;
    [SerializeField] public Sprite IcoSprite;

    [Header("Animation")]
    [SerializeField] private SpriteRenderer _SpriteRenderer;
    [SerializeField] private Sprite[] SpritesAnim;
    [SerializeField] private byte BackWalkTimes;
    [NonSerialized] public byte AnimID;
    [SerializeField] public Coroutine WalkAnimCor;
    [NonSerialized] public float AnimDelay = 0.2f;

    [Header("Shooting")]
    [SerializeField] private GameObject AlertGO;
    [SerializeField] public float Damage;
    [SerializeField] public float ShootingDelay;
    [SerializeField] public float PostShootingDelay;
    [NonSerialized] private bool ShootingStarted;

    [Header("Shielders")]
    [SerializeField] private GameObject ShieldGO;
    [SerializeField] public Collider2D ShieldCol;
    [SerializeField] public bool ShieldDestroyed;


    private void Start()
    {
        WalkAnimCor = StartCoroutine(Animation());
        switch (EnemyType)
        {
            case _EnemyType.Ying:
                StartCoroutine(StartYingFlashingThrough());
                break;
            case _EnemyType.Iana:
                GameObject clone = Instantiate(gameObject, transform.position, Quaternion.identity);
                Enemy EnemyCloneCS = clone.GetComponent<Enemy>();
                EnemyCloneCS._WayCreator = _WayCreator;
                EnemyCloneCS.EnemyType = _EnemyType.IanaClone;
                EnemyCloneCS._WayCreator = _WayCreator;
                EnemyCloneCS.RHGC = RHGC;
                EnemyCloneCS.transform.SetParent(new GameObject().AddComponent<AudioSource>().transform);
                RHGC.Enemies.Add(clone);
                WalkType = _WalkType.Stop;
                gameObject.GetComponentInParent<AudioSource>().Stop();
                StartCoroutine(GoAfterXSec(3));
                break;
        }
    }

    private void FixedUpdate()
    {
        if (WalkType != _WalkType.Stop && _WayCreator)
        {
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(_WayCreator.PathPoints[Step].x - transform.position.x, -(_WayCreator.PathPoints[Step].y - transform.position.y)) * Mathf.Rad2Deg);
            RB2D.AddForce(-transform.up * Speed);
            if (Math.Round(transform.position.x, 1) == Math.Round(_WayCreator.PathPoints[Step].x, 1) && Math.Round(transform.position.y, 1) == Math.Round(_WayCreator.PathPoints[Step].y, 1))
            {
                Step++;
                if (Step == _WayCreator.PathPoints.Length)
                {
                    if (!IsOnKaliWay)
                    {
                        if (EnemyType != _EnemyType.IanaClone)
                        {
                            if(!RHGC._SK.NoOpLeft)
                                RHGC.Shoots--;
                            RHGC.StatsEnemyMissed++;

                            RHGC._SK.BuffersCounter(2, "MissedEnemies", 20, 1, "miss 20 atackers\n", "missing enemies do not stole ammo");
                            MainBridge.CreateSoundGetGO(RHGC.TVAudioSource, RHGC.EnemyMissedSound, _defaultPos.TV, RHGC.transform);
                        }
                        RHGC.Enemies.Remove(gameObject);
                        RHGC.MagazineUpdate();
                        Destroy(gameObject.transform.parent.gameObject);
                    }
                    else
                    {
                        RB2D.velocity = Vector3.zero;
                        StartCoroutine(YouShouldKillUrSelf(10));
                        if(EnemyType == _EnemyType.Sniper)
                            StartCoroutine(EnemyShoot());
                        WalkType = _WalkType.Stop;
                        gameObject.GetComponentInParent<AudioSource>().Stop();
                    }
                }
                else if (WalkType == _WalkType.BetweenPoints && _WayCreator.ShootingMoment + 1 == Step)
                {
                    if (!ShootingStarted)
                    {
                        if (EnemyType == _EnemyType.Blitz)
                        {
                            ShieldGO.transform.SetLocalPositionAndRotation(new Vector3(-0.11f, 0.13f, ShieldGO.transform.position.z), Quaternion.Euler(0, 0, 66));
                            HitCollider.enabled = true;
                            AnimID += 2;
                            StopCoroutine(WalkAnimCor);
                            WalkAnimCor = StartCoroutine(Animation());
                            Speed *= 1.5f;
                        }
                        else
                            Speed *= 1.25f;
                        ShootingStarted = true;
                        StartCoroutine(EnemyShoot());
                    }
                    StartCoroutine(GoBackWhenISay());
                    WalkType = _WalkType.Straigh;
                }
            }
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, -1 + (1 / 6.5f * transform.position.y));
        transform.localScale = new Vector3(1, 1, 1) * (Perspective[0] - (Perspective[1] * transform.position.y));
        BalancerGO.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public IEnumerator GoBackWhenISay()
    {
        BackWalkTimes--;
        if (BackWalkTimes != 0)
        {
            yield return new WaitForSeconds(0.6f);
            Step = _WayCreator.ShootingMoment;
            WalkType = _WalkType.BetweenPoints;
        }
        else
        {
            if (EnemyType == _EnemyType.Blitz)
            {
                ShieldGO.transform.SetLocalPositionAndRotation(new Vector3(0, 0, ShieldGO.transform.position.z), Quaternion.Euler(0, 0, 0));
                HitCollider.enabled = false;
                AnimID -= 2;
                StopCoroutine(WalkAnimCor);
                WalkAnimCor = StartCoroutine(Animation());
                Speed /= 1.5f;
            }
            else
            {
                Speed /= 1.25f;
            }
        }
    }

    public IEnumerator EnemyShoot()
    {
        MainBridge.CreateSoundGetGO(RHGC.TVAudioSource, RHGC.EnemySpotAC, _defaultPos.TV, RHGC.transform);
        AlertGO.SetActive(true);
        yield return new WaitForSeconds(ShootingDelay);
        MainBridge.CreateSoundGetGO(RHGC.TVAudioSource, RHGC.EnemyHitSound, _defaultPos.TV, RHGC.transform);
        RHGC.BloodFrameAnim.SetTrigger("Reactive");
        RHGC.Shoots -= Damage;
        RHGC.MagazineUpdate();
        AlertGO.SetActive(false);
        if (PostShootingDelay != 0)
        {
            yield return new WaitForSeconds(PostShootingDelay);
            StartCoroutine(EnemyShoot());
        }
    }

    public IEnumerator YouShouldKillUrSelf(float time)
    {
        yield return new WaitForSeconds(time);
        RHGC.Enemies.Remove(gameObject);
        Destroy(gameObject.transform.parent.gameObject);
    }

    public IEnumerator Animation()
    {
        _SpriteRenderer.sprite = SpritesAnim[AnimID];
        yield return new WaitForSeconds(AnimDelay);
        _SpriteRenderer.sprite = SpritesAnim[AnimID + 1];
        yield return new WaitForSeconds(AnimDelay);
        StartCoroutine(Animation());
    }

    public IEnumerator StartYingFlashingThrough()
    {
        yield return new WaitForSeconds(1);
        RHGC.FlashScreenAnim.SetTrigger("FlashNOW");
    }
    public IEnumerator GoAfterXSec(float time)
    {
        yield return new WaitForSeconds(time);
        WalkType = _WalkType.Straigh;
        gameObject.GetComponentInParent<AudioSource>().Play();
    }
}