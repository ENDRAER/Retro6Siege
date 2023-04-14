using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TEST : MonoBehaviour
{
    private void Start()
    {
        print("TEST SCRIPT ACTIVATED");
        WayCreator[] WC = FindObjectsOfType<WayCreator>();
        foreach (WayCreator Ass in WC)
        {
            for (int i = 0; i < Ass.PathPoints.Length; i++)
            {
                Ass.PathPoints[i] = new Vector2 (Ass.PathPoints[i].x, Ass.PathPoints[i].y + 1.5f);
            }
        }
    }
}
