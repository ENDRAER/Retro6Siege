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

    void LateUpdate() 
    {
        transform.eulerAngles += new Vector3(-Input.GetAxis("Mouse Y") * RotSpeed, Input.GetAxis("Mouse X") * RotSpeed);
        transform.eulerAngles = new Vector3(Mathf.Clamp((transform.eulerAngles.x > 200? -(360 - transform.eulerAngles.x) : transform.eulerAngles.x), MinRot, MaxRot), transform.eulerAngles.y);

        Application.targetFrameRate = maxFPS;
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
}
// 2d scene = TV screen * 3.365