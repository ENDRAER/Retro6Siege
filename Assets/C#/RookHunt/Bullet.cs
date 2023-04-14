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
        RHControllerCS.Shoots--;
        RHControllerCS.MagazineUpdate();
        StartCoroutine(WaitDelete());
        
        CollidersInZone = Physics2D.OverlapCircleAll(transform.position, 0.005f);
        if (CollidersInZone.Length == 0)
        {
            switch (CollidersInZone[0].gameObject.tag)
            {
                case "RankedPlayButton":
                    RHControllerCS.RankedGameStart();
                    break;
                case "InfModePlayButton":
                    RHControllerCS.InfiniteModeStart();
                    break;

                default:
                    Collider2D cover = null;
                    foreach (Collider2D _coll in CollidersInZone)
                    {
                        if (_coll.gameObject.CompareTag("Cover"))
                            cover = cover == null ? _coll : cover.transform.position.z < _coll.transform.position.z? cover : _coll;
                    }
                    foreach (Collider2D _coll in CollidersInZone)
                    {
                        if (_coll.GetComponentInParent<Enemy>() != null && _coll.transform.position.z < (cover == null ? 0 : cover.transform.position.z))
                        {
                            Destroy(_coll.gameObject.GetComponentInParent<Enemy>().gameObject);
                            RHControllerCS.Shoots += 1.5;
                        }
                    }
                    break;
            }
        }
        RHControllerCS.MagazineUpdate();
    }

    private IEnumerator WaitDelete()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject.GetComponent<CircleCollider2D>());
        yield return new WaitForSeconds(waitTime - 0.2f);
        Destroy(gameObject);
    }
}
