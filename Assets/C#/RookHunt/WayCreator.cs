using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WayCreator : MonoBehaviour
{
    [SerializeField] public Vector2[] PathPoints;
    [Tooltip("This is a tooltip")]
    [SerializeField] public byte Special;
}