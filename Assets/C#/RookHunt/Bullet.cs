using System.Collections;
using UnityEngine;
using System;
using TMPro;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float TimeToDestroy;
    [SerializeField] private GameObject UpScorePF;
    [NonSerialized] private BridgeForLinks _BridgeForLinks;


    private Collider2D[] CollidersInZone;

    void Start()
    {
        _BridgeForLinks = GameObject.Find("3D Camera").gameObject.GetComponent<BridgeForLinks>();
        RookHuntGameController RHControllerCS = _BridgeForLinks.BF_RookHuntGameController;
        RHControllerCS.Shoots--;
        StartCoroutine(TimeToDestroyCor());
        bool resetMultiplier = true;
        
        CollidersInZone = Physics2D.OverlapCircleAll(transform.position, 0.005f);
        if (CollidersInZone.Length != 0)
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
                        if (_coll.gameObject.CompareTag("Cover") || _coll.gameObject.CompareTag("Shield"))
                            cover = cover == null ? _coll : cover.transform.position.z < _coll.transform.position.z? cover : _coll;

                    }
                    foreach (Collider2D _coll in CollidersInZone)
                    {
                        if (_coll.GetComponentInParent<Enemy>() != null && _coll.transform.position.z < (cover == null ? 0 : cover.transform.position.z))
                        {
                            resetMultiplier = false;
                            GameObject UpScoreGO = Instantiate(UpScorePF, transform.position, Quaternion.identity);
                            UpScoreGO.transform.SetParent(GameObject.Find("CenterForUI").transform);
                            UpScoreGO.GetComponent<RectTransform>().position = new Vector2(_coll.transform.position.x, _coll.transform.position.y);
                            UpScoreGO.GetComponent<TextMeshProUGUI>().text = "+" + (100 * (1 + RHControllerCS.KillStreak * 0.2)).ToString();

                            RHControllerCS.Score += (int)(100 * (1 + RHControllerCS.KillStreak * 0.2));
                            RHControllerCS.KillStreak++;
                            RHControllerCS.Shoots += 1.5;

                            UpScoreGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1 - (0.05f * RHControllerCS.KillStreak), 1 - (0.05f * RHControllerCS.KillStreak));
                            Destroy(_coll.gameObject.GetComponentInParent<Enemy>().gameObject);
                        }
                    }
                    break;
            }
        }
        if (resetMultiplier)
            RHControllerCS.KillStreak = 0;
        RHControllerCS.StatUpdate();
    }

    private IEnumerator TimeToDestroyCor()
    {
        yield return new WaitForSeconds(TimeToDestroy);
        Destroy(gameObject);
    }
}
