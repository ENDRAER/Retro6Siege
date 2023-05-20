using UnityEngine;

public class InputBridge : MonoBehaviour
{
    [SerializeField] private GameObject HitColiderGO;
    [SerializeField] private Transform Zero1;
    [SerializeField] private Transform Zero2;
    [SerializeField] private float RotSpeed;
    [SerializeField] private float MinRot;
    [SerializeField] private float MaxRot;
    [SerializeField] private int maxFPS;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        transform.eulerAngles += new Vector3(-Input.GetAxis("Mouse Y") * RotSpeed, Input.GetAxis("Mouse X") * RotSpeed);
        transform.eulerAngles = new Vector3(Mathf.Clamp((transform.eulerAngles.x > 200 ? -(360 - transform.eulerAngles.x) : transform.eulerAngles.x), MinRot, MaxRot), transform.eulerAngles.y);

        Application.targetFrameRate = maxFPS;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject.tag == "Screen")
                {
                    Instantiate(HitColiderGO, new Vector3((hit.point.x - Zero1.position.x) * 3.365f + Zero2.position.x, (hit.point.y - Zero1.position.y) * 3.365f + Zero2.position.y, -2), new Quaternion(0, 0, 0, 0));
                }
            }
        }
    }
}
// 2d scene = TV screen * 3.365