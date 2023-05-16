using static RookHuntGameController;
using System.Collections;
using UnityEngine;
using System;
using TMPro;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject UpScorePF;
    [NonSerialized] private BridgeForLinks _BridgeForLinks;
    [NonSerialized] private Collider2D[] CollidersInZone;

    void Awake()
    {
        StartCoroutine(TimeToDestroyCor());
        _BridgeForLinks = BridgeForLinks.MainBridge_instance;
        RookHuntGameController RHControllerCS = _BridgeForLinks.BF_RookHuntGameController;
        RHControllerCS.Shoots  -= RHControllerCS.CurrentMode != _CurrentMode.GameOver ? (RHControllerCS.CurrentMode != _CurrentMode.Menu? 1 : 0) : 0;
        bool resetMultiplier = true;
        
        CollidersInZone = Physics2D.OverlapCircleAll(transform.position, 0.005f);
        if (CollidersInZone.Length != 0)
        {
            switch (CollidersInZone[0].gameObject.name)
            {
                case "RANKED_PlayList":
                    RHControllerCS.RankedGameStart();
                    break;
                case "INFINITE_PlayList":
                    RHControllerCS.InfiniteModeStart();
                    break;
                case "ShootToRestart":
                    RHControllerCS.ExitInMainMenu();
                    break;
                default:
                    if (RHControllerCS.InvincibleEnemies)
                        return;
                    Collider2D cover = null;
                    foreach (Collider2D _coll in CollidersInZone)
                    {
                        if (_coll.CompareTag("Cover") || _coll.CompareTag("Shield"))
                            cover = cover == null ? _coll : cover.transform.position.z < _coll.transform.position.z? cover : _coll;

                    }
                    foreach (Collider2D _coll in CollidersInZone)
                    {
                        if (_coll.GetComponentInParent<Enemy>() != null && _coll.transform.position.z < (cover == null ? 0 : cover.transform.position.z))
                        {
                            resetMultiplier = false;
                            GameObject UpScoreGO = Instantiate(UpScorePF, transform.position, Quaternion.identity);
                            UpScoreGO.transform.SetParent(BridgeForLinks.MainBridge_instance.WorldCanvas.transform);
                            UpScoreGO.GetComponent<RectTransform>().position = new Vector2(_coll.transform.position.x, _coll.transform.position.y);
                            UpScoreGO.GetComponent<TextMeshProUGUI>().text = "+" + (100 * (1 + RHControllerCS.KillStreak * 0.2)).ToString();

                            RHControllerCS.Score += (int)(100 * (1 + RHControllerCS.KillStreak * 0.2));
                            RHControllerCS.KillStreak++;
                            RHControllerCS.Shoots += 1.5;

                            UpScoreGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1 - (0.05f * RHControllerCS.KillStreak), 1 - (0.05f * RHControllerCS.KillStreak));
                            _coll.GetComponentInParent<Enemy>().YouShouldKillUrSelfNOW(true);
                        }
                    }
                    break;
            }
        }
        if (resetMultiplier && RHControllerCS.CurrentMode != _CurrentMode.Menu)
        {
            if (RHControllerCS.KillStreak > RHControllerCS.StatsMaxKillStreak)
                RHControllerCS.StatsMaxKillStreak = RHControllerCS.KillStreak;
            RHControllerCS.KillStreak = 0;
            RHControllerCS.StatsShootsMissed++;
        }
        RHControllerCS.StatUpdate();
    }

    private IEnumerator TimeToDestroyCor()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
