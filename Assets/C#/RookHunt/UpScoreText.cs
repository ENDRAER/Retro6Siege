using System.Collections;
using UnityEngine;

public class UpScoreText : MonoBehaviour
{
    [SerializeField] private float TimeToDestroy;
    [SerializeField] private float FloatSpeed;

    private void Start()
    {
        StartCoroutine(TimeToDestroyCor());
    }

    void Update()
    {
        transform.Translate(0, FloatSpeed * Time.deltaTime, 0);
    }

    private IEnumerator TimeToDestroyCor()
    {
        yield return new WaitForSeconds(TimeToDestroy);
        Destroy(gameObject);
    }
}
