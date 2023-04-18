using System.Collections;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int Step;
    [SerializeField] public WayCreator _WayCreator;
    [SerializeField] private GameObject BalancerGO;
    [SerializeField] private Rigidbody2D RB2D;
    [SerializeField] private Collider2D HitCollider;
    [SerializeField] private enum _WalkType { Straigh, Stop, BetweenPoints }
    [SerializeField] private _WalkType WalkType = _WalkType.Straigh;
    [SerializeField] private float Speed;

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
    }

    private void Update()
    {
        if (WalkType != _WalkType.Stop && _WayCreator)
        {
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(_WayCreator.PathPoints[Step].x - transform.position.x, -(_WayCreator.PathPoints[Step].y - transform.position.y)) * Mathf.Rad2Deg);
            RB2D.AddForce(-transform.up * Speed * Time.deltaTime);
            if (Math.Round(transform.position.x, 1) == Math.Round(_WayCreator.PathPoints[Step].x, 1) && Math.Round(transform.position.y, 1) == Math.Round(_WayCreator.PathPoints[Step].y, 1))
            {
                Step++;
                if (Step == _WayCreator.PathPoints.Length)
                {
                    if (gameObject.name.StartsWith("Kali") && gameObject.name.StartsWith("WayTaster"))
                        Destroy(gameObject);
                    else if (gameObject.name.StartsWith("Kali") && _WayCreator.KaliWay)
                    {
                        StartCoroutine(YouShouldKillUrSelf());
                        WalkType = _WalkType.Stop;
                        StartCoroutine(EnemyShoot());
                    }
                    else
                    {
                        transform.position = _WayCreator.transform.position;
                        Step = 0;
                    }
                }
            }
            else if (WalkType == _WalkType.BetweenPoints && _WayCreator.ShootingMoment + 1 == Step)
            {
                if (!ShootingStarted)
                {
                    if (gameObject.name.StartsWith("Blitz"))
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
        transform.localScale = new Vector3(1, 1, 1) * (1.77f - (5.08f / 23.6f * transform.position.y));
        BalancerGO.transform.rotation = Quaternion.Euler(0, 0, 0);
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
            if (gameObject.name.StartsWith("Blitz"))
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
        RookHuntGameController _RHC = GameObject.Find("CenterForUI").GetComponent<RookHuntGameController>();
        _RHC.Shoots--;
        _RHC.MagazineUpdate();
        AlertGO.SetActive(false);
        yield return new WaitForSeconds(PostShootingDelay);
    }

    public IEnumerator YouShouldKillUrSelf()
    {
        yield return new WaitForSeconds(8);
        Destroy(gameObject);
    }

    public IEnumerator Animation()
    {
        _SpriteRenderer.sprite = SpritesAnim[0 + AnimID];
        yield return new WaitForSeconds(AnimDelay);
        _SpriteRenderer.sprite = SpritesAnim[1 + AnimID];
        yield return new WaitForSeconds(AnimDelay);
        StartCoroutine(Animation());
    }
}