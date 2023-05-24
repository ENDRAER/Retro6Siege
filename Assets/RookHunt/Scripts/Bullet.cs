using static RookHuntGameController;
using static ScriptKing;
using static Enemy;
using System.Collections;
using UnityEngine;
using System;
using TMPro;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject UpScorePF;
    [NonSerialized] private ScriptKing _BridgeForLinks;
    [NonSerialized] private Collider2D[] CollidersInZone;

    void Awake()
    {
        StartCoroutine(TimeToDestroyCor());
        _BridgeForLinks = ScriptKing.MainBridge;
        RookHuntGameController HRGC = _BridgeForLinks.BF_RookHuntGameController;
        HRGC.Shoots  -= HRGC.CurrentMode != _CurrentMode.GameOver ? (HRGC.CurrentMode != _CurrentMode.Menu? 1 : 0) : 0;
        bool resetMultiplier = true;
        
        CollidersInZone = Physics2D.OverlapCircleAll(transform.position, 0.005f);
        if (CollidersInZone.Length != 0)
        {
            switch (CollidersInZone[0].gameObject.name)
            {
                case "RANKED_PlayList":
                    HRGC.RankedGameStart();
                    break;
                case "INFINITE_PlayList":
                    HRGC.InfiniteModeStart();
                    break;
                case "ShootToRestart":
                    HRGC.ExitInMainMenu();
                    break;
                default:
                    if (HRGC.InvincibleEnemies)
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
                            Enemy EnemyCS = _coll.GetComponentInParent<Enemy>();
                            if (EnemyCS.EnemyType != Enemy._EnemyType.IanaClone)
                            {
                                resetMultiplier = false;

                                HRGC.Score += (int)(100 * (1 + HRGC.KillStreak * 0.2));
                                HRGC.KillStreak++;
                                HRGC.Shoots += 1.5;

                                GameObject UpScoreGO = Instantiate(UpScorePF, transform.position, Quaternion.identity);
                                UpScoreGO.transform.SetParent(ScriptKing.MainBridge.WorldCanvas.transform);
                                UpScoreGO.GetComponent<RectTransform>().position = new Vector2(_coll.transform.position.x, _coll.transform.position.y);
                                UpScoreGO.GetComponent<TextMeshProUGUI>().text = "+" + (100 * (1 + HRGC.KillStreak * 0.2)).ToString();
                                UpScoreGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1 - (0.05f * HRGC.KillStreak), 1 - (0.05f * HRGC.KillStreak));

                                if (EnemyCS.EnemyType == _EnemyType.Osa && !EnemyCS.ShieldDestroyed)
                                {
                                    EnemyCS.ShieldCol.enabled = false;
                                    EnemyCS.HitCollider.enabled = true;
                                    EnemyCS.ShieldDestroyed = true;
                                    EnemyCS.Speed *= 1.5f;
                                    EnemyCS.AnimID += 2;
                                }
                                else
                                {
                                    HRGC.OpIcos[EnemyCS.id].sprite = HRGC.KilledIcon;
                                    HRGC.Enemies.Remove(EnemyCS.gameObject);
                                    _BridgeForLinks.CreateSoundGetGO(_BridgeForLinks.BF_RookHuntGameController.TVAudioSource, _BridgeForLinks.ShootSound, _defaultPos.TV, true);
                                    Destroy(EnemyCS.gameObject.transform.parent.gameObject);
                                }
                            }
                            else
                            {
                                HRGC.Enemies.Remove(EnemyCS.gameObject);
                                Destroy(EnemyCS.gameObject.transform.parent.gameObject);
                            }
                        }
                    }
                    break;
            }
        }
        if (resetMultiplier && HRGC.CurrentMode != _CurrentMode.Menu)
        {
            if (HRGC.KillStreak > HRGC.StatsMaxKillStreak)
                HRGC.StatsMaxKillStreak = HRGC.KillStreak;
            HRGC.KillStreak = 0;
            HRGC.StatsShootsMissed++;
        }
        HRGC.StatUpdate();
    }

    private IEnumerator TimeToDestroyCor()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
