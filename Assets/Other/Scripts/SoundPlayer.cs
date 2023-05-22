using System.Collections;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioSource AU;

    void Start()
    {
        StartCoroutine(TimerToDeath());
    }

    public IEnumerator TimerToDeath()
    {
        yield return new WaitForSeconds(AU.time + 0.2f);
        Destroy(gameObject);
    }
}
