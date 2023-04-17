using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameObject GOTexture;
    [SerializeField] private Rigidbody2D RB2D;
    [SerializeField] public WayCreator _WayCreator;
    [SerializeField] private enum _WalkType { Straigh, Stop, BetweenPoints }
    [SerializeField] private _WalkType WalkType = _WalkType.Straigh;
    [SerializeField] private bool ShootingStarted;
    [SerializeField] private bool Invincible;
    [SerializeField] private int Step;
    [SerializeField] private float Speed;
    [SerializeField] private bool RepeatAfterDone;

    [Header("Animation")]
    [SerializeField] private SpriteRenderer _SpriteRenderer;
    [SerializeField] private Sprite[] SpritesAnim;
    [SerializeField] private byte BackWalkTimes;
    [SerializeField] private byte AnimID;
    [SerializeField] private float AnimDelay;

    [Header("Shooting")]
    [SerializeField] private GameObject AlertGO;
    [SerializeField] private float ShootingDelay;
    [SerializeField] private float PostShootingDelay;


    private void Start()
    {
        if (!RepeatAfterDone)
            StartCoroutine(YouShouldKillUrSelf());
        StartCoroutine(Animation());
    }

    private void FixedUpdate()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -1 + 1 / 6.5f * transform.position.y);
        transform.localScale = new Vector3(1, 1, 1) * (1.77f - (5.08f / 23.6f * transform.position.y));
        AlertGO.transform.rotation = Quaternion.Euler(0, 0, 0);

        if (WalkType != _WalkType.Stop)
        {
            if (!_WayCreator) return;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(_WayCreator.PathPoints[Step].x - transform.position.x, -(_WayCreator.PathPoints[Step].y - transform.position.y)) * Mathf.Rad2Deg);
            GOTexture.transform.rotation = Quaternion.Euler(0, 0, 0);
            RB2D.AddForce(-transform.up * Speed);
            if (Math.Round(transform.position.x, 2) == Math.Round(_WayCreator.PathPoints[Step].x, 2) && Math.Round(transform.position.y, 2) == Math.Round(_WayCreator.PathPoints[Step].y, 2))
            {
                transform.position = _WayCreator.PathPoints[Step];
                Step++;
                if (Step == _WayCreator.PathPoints.Length)
                {
                    if (gameObject.name != "Kali(Clone)" && !RepeatAfterDone)
                        Destroy(gameObject);
                    else if (gameObject.name == "Kali(Clone)" && _WayCreator.KaliWay)
                    {
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
                    if (gameObject.name == "Blitz(Clone)")
                    {
                        AnimID += 2;
                        Invincible = false;
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
    }

    public IEnumerator GoBackWhenISay()
    {
        BackWalkTimes--;
        if (BackWalkTimes != 0)
        {
            yield return new WaitForSeconds(Speed / 15);
            Step--;
            WalkType = _WalkType.BetweenPoints;
        }
        else if (gameObject.name == "Blitz(Clone)")
        {
            AnimID -= 2;
            Invincible = true;
            Speed /= 1.5f;
        }
        else
        {
            Speed *= 2;
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
        yield return new WaitForSeconds(10);
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