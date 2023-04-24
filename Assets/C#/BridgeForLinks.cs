using UnityEngine;

public class BridgeForLinks : MonoBehaviour
{
    public static BridgeForLinks MainBridge_instance;
    public RookHuntGameController BF_RookHuntGameController;
    public GameObject Canvas2D;
    public GameObject WorldCanvas;

    private void Awake()
    {
        MainBridge_instance = this;
    }
}
