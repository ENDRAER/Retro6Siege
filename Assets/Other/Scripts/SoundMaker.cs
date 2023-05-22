using UnityEngine;

public class SoundMaker : MonoBehaviour
{
    public enum _defaultPos {Custom, TV};

    public GameObject CreateSoundGetGO(GameObject AudioSource, AudioClip audioClip, _defaultPos defaultPos, bool shouldKillUrSelf, Vector3 Position = new Vector3())
    {
        Vector3 v3 = Position;
        switch (defaultPos)
        {
            case _defaultPos.TV:
                new Vector3(0, 0, 0);
                break;
        }
        GameObject AU = Instantiate(AudioSource, v3, Quaternion.identity);
        if(shouldKillUrSelf)
            AU.GetComponent<SoundPlayer>().enebled = true;
        AU.GetComponent<AudioSource>().clip = audioClip;
        return AU;
    }
}
