using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using System;
using TMPro;
using static RookHuntGameController;
using static ScriptKing;
using static Enemy;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject UpScorePF;
    [NonSerialized] private ScriptKing _SK;
    [NonSerialized] private Collider2D[] CollidersInZone;

    void Awake()
    {
        _SK = MainBridge;
        RookHuntGameController RHGC = _SK.BF_RHGC;
        StartCoroutine(TimeToDestroyCor());

        bool resetMultiplier = true;
        if (_SK.LargeBullet)
            transform.localScale = new(6, 6, 6);
        if (RHGC.CurrentMode != _CurrentMode.GameOver && RHGC.CurrentMode != _CurrentMode.Menu)
        {
            RHGC.Shots -= _SK.InfiniteAmmo ? 0 : 1;
            RHGC.ShootTimesPerMatch++;
        }

        CollidersInZone = Physics2D.OverlapCircleAll(transform.position, _SK.LargeBullet? 0.03f : 0.025f);
        if (CollidersInZone.Length != 0)
        {
            switch (CollidersInZone[0].gameObject.name)
            {
                case "RANKED_PlayList":
                    RHGC.RankedGameStart();
                    break;
                case "INFINITE_PlayList":
                    RHGC.InfiniteModeStart();
                    break;
                case "ShotToRestart":
                    RHGC.ExitInMainMenu();
                    break;
                default:
                    MainBridge.CreateSoundGetGO(RHGC.TVAudioSource, RHGC.ShotSound, _defaultPos.TV);
                    if (RHGC.InvincibleEnemies)
                        return;
                    Collider2D cover = null;
                    foreach (Collider2D _coll in CollidersInZone)
                    {
                        if (_coll.CompareTag("Cover"))
                            cover = cover == null ? _coll : cover.transform.position.z < _coll.transform.position.z ? cover : _coll;
                        else if (_coll.CompareTag("Shield") && !_SK.NoMoreShieldHitBox)
                        {
                            cover = cover == null ? _coll : cover.transform.position.z < _coll.transform.position.z ? cover : _coll;
                            _SK.BuffersCounter(5, "ShieldHits", 25, 1, "Shoot any shield 25 times", "Instant shield break");
                            _SK.CreateSoundGetGO(_SK.BF_RHGC.TVAudioSource, RHGC.OsasShieldCrashAC, _defaultPos.TV, RHGC.transform);
                        }
                    }

                    int MultipleHit = 0;
                    foreach (Collider2D _coll in CollidersInZone)
                    {
                        if (_coll.GetComponentInParent<Enemy>() != null && (_coll.transform.position.z < (cover == null ? 0 : cover.transform.position.z) || _SK.LargeBullet))
                        {
                            Enemy EnemyCS = _coll.GetComponentInParent<Enemy>();
                            if (EnemyCS.EnemyType != _EnemyType.IanaClone && EnemyCS.EnemyType != _EnemyType.Alibi)
                            {
                                MultipleHit++;
                                resetMultiplier = false;
                                RHGC.Score += (int)(100 * math.clamp(1 + RHGC.KillStreak * 0.2,0,20));
                                GameObject UpScoreGO = Instantiate(UpScorePF, transform.position, Quaternion.identity);
                                UpScoreGO.transform.SetParent(RHGC.DynamicCanvas.transform);
                                UpScoreGO.GetComponent<RectTransform>().position = new Vector2(_coll.transform.position.x, _coll.transform.position.y);
                                UpScoreGO.GetComponent<TextMeshProUGUI>().text = "+" + (100 * math.clamp(1 + RHGC.KillStreak * 0.2, 0, 20)).ToString();
                                UpScoreGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1 - (0.05f * RHGC.KillStreak));

                                RHGC.KillStreakPerRound++;
                                RHGC.KillStreak++;
                                RHGC.Shots += 1.5;
                                if (RHGC.KillStreak == 20)
                                    _SK.BuffersCounter(6, "KillSteakEarned", 1, 1, "Get a 20 kill streak", "Don't lose killstreak on miss");
                                if (EnemyCS.EnemyType == _EnemyType.Ash)
                                    _SK.BuffersCounter(4, "AshKills", 20, 1, "Kill Ash 20 times \n", "Ashpocalipse\n(infinite game mode only)");

                                if (EnemyCS.EnemyType == _EnemyType.Osa && !EnemyCS.ShieldDestroyed && !_SK.NoMoreShieldHitBox)
                                {
                                    EnemyCS.ShieldCol.enabled = false;
                                    EnemyCS.HitCollider.enabled = true;
                                    EnemyCS.ShieldDestroyed = true;
                                    EnemyCS.Speed *= 1.5f;
                                    EnemyCS.AnimID += 2;
                                    _SK.BuffersCounter(5, "ShieldHits", 25, 1, "Shoot any shield 25 times", "Instant shield break");
                                    _SK.CreateSoundGetGO(_SK.BF_RHGC.TVAudioSource, RHGC.OsasShieldCrashAC, _defaultPos.TV, RHGC.transform);
                                }
                                else
                                {
                                    if (RHGC.CurrentMode == _CurrentMode.Ranked)
                                        RHGC.OpIcos[EnemyCS.id].sprite = RHGC.KilledIcon;
                                    RHGC.Enemies.Remove(EnemyCS.gameObject);
                                    Destroy(EnemyCS.gameObject.transform.parent.gameObject);
                                    _SK.CreateSoundGetGO(_SK.BF_RHGC.TVAudioSource, RHGC.HitSound, _defaultPos.TV, RHGC.transform);
                                }
                            }
                            else if (EnemyCS.EnemyType == _EnemyType.Alibi)
                            {
                                RHGC.Score -= 69;

                                GameObject UpScoreGO = Instantiate(UpScorePF, transform.position, Quaternion.identity);
                                UpScoreGO.transform.SetParent(RHGC.DynamicCanvas.transform);
                                UpScoreGO.GetComponent<RectTransform>().position = new Vector2(_coll.transform.position.x, _coll.transform.position.y);
                                UpScoreGO.GetComponent<TextMeshProUGUI>().text = "-69";
                                UpScoreGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 0, 0);

                                RHGC.Enemies.Remove(EnemyCS.gameObject);
                                _SK.CreateSoundGetGO(_SK.BF_RHGC.TVAudioSource, RHGC.AlibiKillingAC, _defaultPos.TV, RHGC.transform);
                                Destroy(EnemyCS.gameObject.transform.parent.gameObject);
                            }
                            else if (EnemyCS.EnemyType == _EnemyType.IanaClone)
                            {
                                RHGC.Enemies.Remove(EnemyCS.gameObject);
                                Destroy(EnemyCS.gameObject.transform.parent.gameObject);
                                _SK.CreateSoundGetGO(_SK.BF_RHGC.TVAudioSource, RHGC.IanaMissingAC, _defaultPos.TV, RHGC.transform);
                            }
                        }
                    }
                    if(MultipleHit != 0)
                        _SK.BuffersCounter(3, "MultipleKills", 5, MultipleHit, "hit two attackers with one shot 5 times", "Big bullets");
                    break;
            }
        }
        else
            MainBridge.CreateSoundGetGO(RHGC.TVAudioSource, RHGC.ShotSound, _defaultPos.TV, RHGC.transform);

        if (resetMultiplier && RHGC.CurrentMode != _CurrentMode.Menu)
        {
            if (!_SK.NoMoreLosingKillStreak)
            {
                if (RHGC.KillStreak > RHGC.StatsMaxKillStreak)
                    RHGC.StatsMaxKillStreak = RHGC.KillStreak;
                RHGC.KillStreak = 0;
                RHGC.KillStreakPerRound = 0;
            }
            RHGC.StatsShotsMissed++;
        }
        RHGC.StatUpdate();
    }

    private IEnumerator TimeToDestroyCor()
    {
        yield return new WaitForSeconds(0.25f);
        Destroy(gameObject);
    }
}
