using System.Collections.Generic;
using UnityEngine;

public class MapScript : MonoBehaviour
{
    [SerializeField] public GameObject Inside;
    [SerializeField] public GameObject Outside;
    [SerializeField] public List<WayCreator> Ways;
    [SerializeField] public WayCreator SnipersWay;
}
