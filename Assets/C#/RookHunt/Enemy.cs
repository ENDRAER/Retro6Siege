using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameObject GOTexture;
    [SerializeField] private Rigidbody2D RB2D;
    [SerializeField] private Collider2D[] Colliders;
    [SerializeField] public WayCreator _WayCreator;
    [SerializeField] private enum _WalkType { Straigh, Stop, BetweenPoints }
    [SerializeField] private _WalkType WalkType = _WalkType.Straigh;
    [SerializeField] private bool Invincible;
    [SerializeField] private int Step;
    [SerializeField] private float Speed;
    [SerializeField] private bool RepeatAfterDone;

    [Header("Animation")]
    [SerializeField] private SpriteRenderer _SpriteRenderer;
    [SerializeField] private Sprite[] SpritesAnim;
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
                        AlertGO.transform.rotation = Quaternion.Euler(0, 0, 0);
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
            else if (WalkType == _WalkType.BetweenPoints && _WayCreator.ShootingMoment == Step)
            {
                Speed /= 2;
                StartCoroutine(GoBackWhenISay());
                WalkType = _WalkType.Straigh;
            }
        }
    }

    public IEnumerator GoBackWhenISay()
    {
        yield return new WaitForSeconds(0.5f);
        Step--;
        WalkType = _WalkType.BetweenPoints;
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

    public IEnumerator Animation()
    {
        _SpriteRenderer.sprite = SpritesAnim[0];
        yield return new WaitForSeconds(AnimDelay);
        _SpriteRenderer.sprite = SpritesAnim[1];
        yield return new WaitForSeconds(AnimDelay);
        StartCoroutine(Animation());
    }

    public IEnumerator YouShouldKillUrSelf()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }
}