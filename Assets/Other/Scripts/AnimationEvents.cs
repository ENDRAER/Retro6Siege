using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public void AudioEvent()
    {
        RookHuntGameController RHGC = ScriptKing.MainBridge.BF_RHGC;
        ScriptKing.MainBridge.CreateSoundGetGO(RHGC.TVAudioSource, RHGC.YingFlash, ScriptKing._defaultPos.TV, transform);
    }
}
