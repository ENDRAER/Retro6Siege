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
        _BridgeForLinks = MainBridge;
        RookHuntGameController RHGC = _BridgeForLinks.BF_RHGC;
        StartCoroutine(TimeToDestroyCor());
        bool resetMultiplier = true;

        if (RHGC.CurrentMode != _CurrentMode.GameOver && RHGC.CurrentMode != _CurrentMode.Menu)
        {
            RHGC.Shoots -= _BridgeForLinks.InfiniteAmmo ? 0 : 1;
            int ShootTimes = PlayerPrefs.GetInt("ShootTimes") + 1;
            PlayerPrefs.SetInt("ShootTimes", ShootTimes);
            if (ShootTimes >= 100)
            {
                _BridgeForLinks.CheckBoxes[0].SetActive(true);
                _BridgeForLinks.ModText[0].text = "infinite ammo";

                if (ShootTimes >= 1000)
                {
                    _BridgeForLinks.CheckBoxes[1].SetActive(true);
                    _BridgeForLinks.ModText[1].text = "full auto shooting";
                }
                else
                    _BridgeForLinks.ModText[1].text = "Shoot for 1000 times \n" + new string('x', ShootTimes / 100) + new string('-', 10 - ShootTimes / 100);
            }
            else
            {
                _BridgeForLinks.ModText[0].text = "Shoot for 100 times \n" + new string('x', ShootTimes / 10) + new string('-', 10 - ShootTimes / 10);
                _BridgeForLinks.ModText[1].text = "unlock prewious";
            }
        }

        CollidersInZone = Physics2D.OverlapCircleAll(transform.position, 0.005f);
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
                case "ShootToRestart":
                    RHGC.ExitInMainMenu();
                    break;
                default:
                    MainBridge.CreateSoundGetGO(RHGC.TVAudioSource, RHGC.ShootSound, _defaultPos.TV);
                    if (RHGC.InvincibleEnemies)
                        return;
                    Collider2D cover = null;
                    foreach (Collider2D _coll in CollidersInZone)
                    {
                        if (_coll.CompareTag("Cover") || _coll.CompareTag("Shield"))
                            cover = cover == null ? _coll : cover.transform.position.z < _coll.transform.position.z ? cover : _coll;

                    }
                    foreach (Collider2D _coll in CollidersInZone)
                    {
                        if (_coll.GetComponentInParent<Enemy>() != null && _coll.transform.position.z < (cover == null ? 0 : cover.transform.position.z))
                        {
                            Enemy EnemyCS = _coll.GetComponentInParent<Enemy>();
                            if (EnemyCS.EnemyType != Enemy._EnemyType.IanaClone && EnemyCS.EnemyType != _EnemyType.Alibi)
                            {
                                resetMultiplier = false;
                                RHGC.Score += (int)(100 * (1 + RHGC.KillStreak * 0.2));
                                GameObject UpScoreGO = Instantiate(UpScorePF, transform.position, Quaternion.identity);
                                UpScoreGO.transform.SetParent(RHGC.DynamicCanvas.transform);
                                UpScoreGO.GetComponent<RectTransform>().position = new Vector2(_coll.transform.position.x, _coll.transform.position.y);
                                UpScoreGO.GetComponent<TextMeshProUGUI>().text = "+" + (100 * (1 + RHGC.KillStreak * 0.2)).ToString();
                                UpScoreGO.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1 - (0.05f * RHGC.KillStreak));

                                RHGC.KillStreakPerRound++;
                                RHGC.KillStreak++;
                                RHGC.Shoots += 1.5;

                                if (EnemyCS.EnemyType == _EnemyType.Osa && !EnemyCS.ShieldDestroyed)
                                {
                                    EnemyCS.ShieldCol.enabled = false;
                                    EnemyCS.HitCollider.enabled = true;
                                    EnemyCS.ShieldDestroyed = true;
                                    EnemyCS.Speed *= 1.5f;
                                    EnemyCS.AnimID += 2;
                                    _BridgeForLinks.CreateSoundGetGO(_BridgeForLinks.BF_RHGC.TVAudioSource, RHGC.OsasShieldCrashAC, _defaultPos.TV, RHGC.transform);
                                }
                                else
                                {
                                    if (RHGC.CurrentMode == _CurrentMode.Ranked)
                                        RHGC.OpIcos[EnemyCS.id].sprite = RHGC.KilledIcon;
                                    RHGC.Enemies.Remove(EnemyCS.gameObject);
                                    Destroy(EnemyCS.gameObject.transform.parent.gameObject);
                                    _BridgeForLinks.CreateSoundGetGO(_BridgeForLinks.BF_RHGC.TVAudioSource, RHGC.HitSound, _defaultPos.TV, RHGC.transform);
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
                                _BridgeForLinks.CreateSoundGetGO(_BridgeForLinks.BF_RHGC.TVAudioSource, RHGC.AlibiKillingAC, _defaultPos.TV, RHGC.transform);
                                Destroy(EnemyCS.gameObject.transform.parent.gameObject);
                            }
                            else if (EnemyCS.EnemyType == _EnemyType.IanaClone)
                            {
                                RHGC.Enemies.Remove(EnemyCS.gameObject);
                                Destroy(EnemyCS.gameObject.transform.parent.gameObject);
                                _BridgeForLinks.CreateSoundGetGO(_BridgeForLinks.BF_RHGC.TVAudioSource, RHGC.IanaMissingAC, _defaultPos.TV, RHGC.transform);
                            }
                        }
                    }
                    break;
            }
        }
        else
            MainBridge.CreateSoundGetGO(RHGC.TVAudioSource, RHGC.ShootSound, _defaultPos.TV, RHGC.transform);

        if (resetMultiplier && RHGC.CurrentMode != _CurrentMode.Menu)
        {
            if (RHGC.KillStreak > RHGC.StatsMaxKillStreak)
                RHGC.StatsMaxKillStreak = RHGC.KillStreak;
            RHGC.KillStreak = 0;
            RHGC.KillStreakPerRound = 0;
            RHGC.StatsShootsMissed++;
        }
        RHGC.StatUpdate();
    }

    private IEnumerator TimeToDestroyCor()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
