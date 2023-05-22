using System.Collections;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class Enemy : MonoBehaviour
{
    [NonSerialized] public int id;
    [NonSerialized] public RookHuntGameController HRGC;
    [NonSerialized] private int Step;
    [NonSerialized] public float[] Perspective = { 1.77f, 0.2f}; // default values
    [NonSerialized] public WayCreator _WayCreator;
    [SerializeField] public enum _EnemyType { Standart, Sniper, Blitz, Ying, Osa, Iana, IanaClone, Alibi }
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
        StartCoroutine(Animation());
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
                EnemyCloneCS.HRGC = HRGC;
                HRGC.Enemies.Add(clone);
                WalkType = _WalkType.Stop;
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
                    if (EnemyType != _EnemyType.Sniper)
                    {
                        if (EnemyType != _EnemyType.IanaClone)
                        {
                            HRGC.Shoots--;
                            HRGC.StatsEnemyMissed++;
                        }
                        HRGC.Enemies.Remove(gameObject);
                        HRGC.MagazineUpdate();
                        Destroy(gameObject);
                    }
                    else if (EnemyType == _EnemyType.Sniper)
                    {
                        RB2D.velocity = Vector3.zero;
                        StartCoroutine(YouShouldKillUrSelf(10));
                        StartCoroutine(EnemyShoot());
                        WalkType = _WalkType.Stop;
                    }
                }
                else if (WalkType == _WalkType.BetweenPoints && _WayCreator.ShootingMoment + 1 == Step)
                {
                    if (!ShootingStarted)
                    {
                        if (EnemyType == _EnemyType.Blitz)
                        {
                            ShieldGO.transform.localPosition = new Vector3(-0.11f, 0.13f, ShieldGO.transform.position.z);
                            ShieldGO.transform.localRotation = Quaternion.Euler(0, 0, 66);
                            HitCollider.enabled = true;
                            AnimID += 2;
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
                ShieldGO.transform.localPosition = new Vector3(0, 0, ShieldGO.transform.position.z);
                ShieldGO.transform.localRotation = Quaternion.Euler(0, 0, 0);
                HitCollider.enabled = false;
                AnimID -= 2;
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
        AlertGO.SetActive(true);
        yield return new WaitForSeconds(ShootingDelay);
        HRGC.Shoots -= Damage;
        HRGC.MagazineUpdate();
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
        HRGC.Enemies.Remove(gameObject);
        Destroy(gameObject);
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
        HRGC.FlashScreenAnim.SetTrigger("FlashNOW");
    }
    public IEnumerator GoAfterXSec(float time)
    {
        yield return new WaitForSeconds(time);
        WalkType = _WalkType.Straigh;
    }
}