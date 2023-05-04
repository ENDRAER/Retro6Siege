using System.Collections;
using UnityEngine;
using System;

public class WayTester : MonoBehaviour
{
    [SerializeField] private int Step;
    [SerializeField] public WayCreator _WayCreator;
    [SerializeField] private GameObject BalancerGO;
    [SerializeField] private Rigidbody2D RB2D;
    [SerializeField] private float Speed;

    [Header("Animation")]
    [SerializeField] private SpriteRenderer _SpriteRenderer;
    [SerializeField] private Sprite[] SpritesAnim;
    [SerializeField] private byte AnimID;


    private void Start()
    {
        StartCoroutine(Animation());
    }

    private void Update()
    {
        if (_WayCreator)
            return;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(_WayCreator.PathPoints[Step].x - transform.position.x, -(_WayCreator.PathPoints[Step].y - transform.position.y)) * Mathf.Rad2Deg);
        RB2D.AddForce(-transform.up * Speed * Time.deltaTime);
        if (Math.Round(transform.position.x, 1) == Math.Round(_WayCreator.PathPoints[Step].x, 1) && Math.Round(transform.position.y, 1) == Math.Round(_WayCreator.PathPoints[Step].y, 1))
        {
            Step++;
            if (Step == _WayCreator.PathPoints.Length)
            {
                transform.position = _WayCreator.transform.position;
                Step = 0;
            }
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, -1 + (1 / 6.5f * transform.position.y));
        transform.localScale = new Vector3(1, 1, 1) * (1.77f - (5.08f / 23.6f * transform.position.y));
        BalancerGO.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public IEnumerator Animation()
    {
        _SpriteRenderer.sprite = SpritesAnim[0 + AnimID];
        yield return new WaitForSeconds(0.2f);
        _SpriteRenderer.sprite = SpritesAnim[1 + AnimID];
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(Animation());
    }
}