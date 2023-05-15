using System.Collections;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [NonSerialized] public int id;
    [NonSerialized] public RookHuntGameController HRGC;
    [NonSerialized] private int Step;
    [NonSerialized] public float[] Perspective = { 1.77f, 0.2f}; // default values
    [NonSerialized] public WayCreator _WayCreator;
    [SerializeField] public enum _EnemyType { Standart, Sniper, Blitz, Ying, Finka }
    [SerializeField] public _EnemyType EnemyType;
    [SerializeField] private GameObject BalancerGO;
    [SerializeField] private Rigidbody2D RB2D;
    [SerializeField] private Collider2D HitCollider;
    [SerializeField] public enum _WalkType { Straigh, Stop, BetweenPoints }
    [SerializeField] public _WalkType WalkType = _WalkType.Straigh;
    [SerializeField] public float Speed;
    [SerializeField] public Sprite IcoSprite;

    [Header("Animation")]
    [SerializeField] private SpriteRenderer _SpriteRenderer;
    [SerializeField] private Sprite[] SpritesAnim;
    [SerializeField] private byte BackWalkTimes;
    [SerializeField] private byte AnimID;
    [SerializeField] private float AnimDelay;

    [Header("Shooting")]
    [SerializeField] private GameObject AlertGO;
    [SerializeField] private bool ShootingStarted;
    [SerializeField] private float ShootingDelay;
    [SerializeField] private float PostShootingDelay;

    [Header("Shielders")]
    [SerializeField] private GameObject ShieldGO;


    private void Start()
    {
        StartCoroutine(Animation());
        if (EnemyType == _EnemyType.Ying)
        {
            StartCoroutine(StartYingFlashingThrough());
        }
    }

    private void FixedUpdate()
    {
        if (WalkType != _WalkType.Stop && _WayCreator)
        {
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(_WayCreator.PathPoints[Step].x - transform.position.x, -(_WayCreator.PathPoints[Step].y - transform.position.y)) * Mathf.Rad2Deg);
            RB2D.AddForce(-transform.up * Speed);
            if (Math.Round(transform.position.x, 2) == Math.Round(_WayCreator.PathPoints[Step].x, 2) && Math.Round(transform.position.y, 2) == Math.Round(_WayCreator.PathPoints[Step].y, 2))
            {
                Step++;
                if (Step == _WayCreator.PathPoints.Length)
                {
                    if (EnemyType != _EnemyType.Sniper)
                    {
                        HRGC.Shoots--;
                        HRGC.MagazineUpdate();
                        HRGC.StatsEnemyMissed++;
                        YouShouldKillUrSelfNOW(false);
                    }
                    else if (EnemyType == _EnemyType.Sniper)
                    {
                        StartCoroutine(YouShouldKillUrSelf());
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
                            Speed /= 2;
                        ShootingStarted = true;
                        StartCoroutine(EnemyShoot());
                    }
                    StartCoroutine(GoBackWhenISay());
                    WalkType = _WalkType.Straigh;
                }
            }
            transform.position = new Vector3(transform.position.x, transform.position.y, -1 + (1 / 6.5f * transform.position.y));
            transform.localScale = new Vector3(1, 1, 1) * (Perspective[0] - (Perspective[1] * transform.position.y));
            BalancerGO.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public IEnumerator GoBackWhenISay()
    {
        BackWalkTimes--;
        if (BackWalkTimes != 0)
        {
            yield return new WaitForSeconds(1);
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
                Speed /= 2;
            }
        }
    }

    public IEnumerator EnemyShoot()
    {
        AlertGO.SetActive(true);
        yield return new WaitForSeconds(ShootingDelay);
        HRGC.Shoots--;
        HRGC.MagazineUpdate();
        AlertGO.SetActive(false);
        if (PostShootingDelay != 0)
        {
            yield return new WaitForSeconds(PostShootingDelay);
            StartCoroutine(EnemyShoot());
        }
    }

    public IEnumerator YouShouldKillUrSelf()
    {
        yield return new WaitForSeconds(10); 
        YouShouldKillUrSelfNOW(false);
    }

    public void YouShouldKillUrSelfNOW(bool setkillico)
    {
        if(HRGC.CurrentMode == RookHuntGameController._CurrentMode.Ranked && setkillico)
            HRGC.OpIcos[id].sprite = HRGC.KilledIcon;
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
}