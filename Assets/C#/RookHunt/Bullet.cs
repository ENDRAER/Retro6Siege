using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float waitTime;
    [SerializeField] private BridgeForLinks _BridgeForLinks;

    private Collider2D[] CollidersInZone;

    void Start()
    {
        _BridgeForLinks = GameObject.Find("3D Camera").gameObject.GetComponent<BridgeForLinks>();
        RookHuntGameController RHControllerCS = _BridgeForLinks.BF_RookHuntGameController;
        StartCoroutine(WaitDelete());
        
        CollidersInZone = Physics2D.OverlapCircleAll(transform.position, 0.015f);
        if (CollidersInZone .Length==0  ) return;
        switch (CollidersInZone[0].gameObject.tag)
        {
            case "RankedPlayButton":
                RHControllerCS.RankedGameStart();
                break;
            case "InfModePlayButton":
                RHControllerCS.InfiniteModeStart();
                break;

            default:
                if (CollidersInZone == null) return;

                RHControllerCS.Shoots--;
                Collider2D cover = null;
                foreach (Collider2D _coll in CollidersInZone)
                {
                    if (!_coll.gameObject.CompareTag("Enemy"))
                    {
                        cover = _coll;
                    }
                }
                foreach (Collider2D _coll in CollidersInZone)
                {
                    if (_coll.tag == "Enemy" && _coll.transform.position.z < (cover ? cover.transform.position.z : 0))
                        Destroy(_coll.gameObject);
                }
                break;
        }
    }

    private IEnumerator WaitDelete()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject.GetComponent<CircleCollider2D>());
        yield return new WaitForSeconds(waitTime - 0.2f);
        Destroy(gameObject);
    }
}
