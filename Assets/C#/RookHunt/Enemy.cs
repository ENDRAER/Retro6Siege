using System.Collections;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameObject BalancerGO;
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
        StartCoroutine(Animation());
    }

    private void Update()
    {
        if (WalkType != _WalkType.Stop && _WayCreator)
        {
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(_WayCreator.PathPoints[Step].x - transform.position.x, -(_WayCreator.PathPoints[Step].y - transform.position.y)) * Mathf.Rad2Deg);
            RB2D.AddForce(-transform.up * Speed * Time.deltaTime);
            if (Math.Round(transform.position.x, 1) == _WayCreator.PathPoints[Step].x && Math.Round(transform.position.y, 1) == _WayCreator.PathPoints[Step].y)
            {
                Step++;
                if (Step == _WayCreator.PathPoints.Length)
                {
                    if (gameObject.name != "Kali(Clone)" && !RepeatAfterDone)
                        Destroy(gameObject);
                    else if (gameObject.name == "Kali(Clone)" && _WayCreator.KaliWay)
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
        transform.position = new Vector3(transform.position.x, transform.position.y, -1 + (1 / 6.5f * transform.position.y));
        transform.localScale = new Vector3(1, 1, 1) * (1.77f - (5.08f / 23.6f * transform.position.y));
        BalancerGO.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public IEnumerator GoBackWhenISay()
    {
        BackWalkTimes--;
        if (BackWalkTimes != 0) yield break;
        if (gameObject.name == "Blitz(Clone)")
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