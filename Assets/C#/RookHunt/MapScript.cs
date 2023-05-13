using System.Collections.Generic;
using UnityEngine;

public class MapScript : MonoBehaviour
{
    [Header("Atck")]
    [SerializeField] public GameObject Inside;
    [SerializeField] public WayCreator SnipersWay;
    [SerializeField] public List<WayCreator> Ways;
    [SerializeField] public float[] Perspective;
    [Header("Def")]
    [SerializeField] public GameObject Outside;
    [SerializeField] public List<WayCreator> WaysDef;
    [SerializeField] public float[] PerspectiveDef;
}
