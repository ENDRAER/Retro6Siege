using System.Collections;
using UnityEngine;

public class UpScoreText : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(TimeToDestroyCor());
    }

    void Update()
    {
        transform.Translate(0, 1 * Time.deltaTime, 0);
    }

    private IEnumerator TimeToDestroyCor()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
