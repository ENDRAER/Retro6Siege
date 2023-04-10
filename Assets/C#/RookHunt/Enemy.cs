using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [SerializeField] public WayCreator _WayCreator;
    [SerializeField] private GameObject GOTexture;
    [SerializeField] private Rigidbody2D RB2D;
    [SerializeField] private Collider2D[] Colliders;
    [SerializeField] private int Step;
    [SerializeField] private float Speed;
    [SerializeField] private bool RepeatAfterDone;

    [Header("Animation")]
    [SerializeField] private SpriteRenderer _SpriteRenderer;
    [SerializeField] private Sprite[] SpritesAnim;
    [SerializeField] private float AnimDelay;

    private void FixedUpdate()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -0.2f + 0.15f / 5f * transform.position.y);
        transform.localScale = new Vector3(1, 1, 1) * (1.53f - (0.7f / 2.91f * transform.position.y));
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
                if (!RepeatAfterDone)
                    Destroy(gameObject);
                else
                {
                    transform.position = _WayCreator.transform.position;
                    Step = 0;
                }
            }
        }
    }

    public IEnumerator Animation()
    {
        _SpriteRenderer.sprite = SpritesAnim[0];
        yield return new WaitForSeconds(AnimDelay);
        _SpriteRenderer.sprite = SpritesAnim[1];
        yield return new WaitForSeconds(AnimDelay);
        StartCoroutine(Animation());
    }
}