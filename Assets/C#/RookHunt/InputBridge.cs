using UnityEngine;

public class InputBridge : MonoBehaviour
{
    [SerializeField] public GameObject HitColiderGO;
    [SerializeField] public Transform Zero1;
    [SerializeField] public Transform Zero2;
    [SerializeField] public float RotSpeed;
    [SerializeField] public float MinRot;
    [SerializeField] public float MaxRot;
    [SerializeField] public int maxFPS;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject.tag == "Screen")
                {
                    GameObject HitColider = Instantiate(HitColiderGO, new Vector3(Zero2.position.x + (hit.point.x * 3.365f), Zero2.position.y + (hit.point.y * 3.365f), -2), new Quaternion(0, 0, 0, 0)); 
                }
            }
        }
    }

    void LateUpdate() 
    {
        Application.targetFrameRate = maxFPS;
        transform.eulerAngles += new Vector3(-Input.GetAxis("Mouse Y") * RotSpeed, Input.GetAxis("Mouse X") * RotSpeed);
        if (transform.eulerAngles.x < MaxRot && transform.eulerAngles.x > 180)
            transform.eulerAngles = new Vector3(MaxRot, transform.eulerAngles.y);
        if (transform.eulerAngles.x > MinRot && transform.eulerAngles.x <= 180)
            transform.eulerAngles = new Vector3(MinRot, transform.eulerAngles.y);
    }
}
// 2d scene = screen * 3.365