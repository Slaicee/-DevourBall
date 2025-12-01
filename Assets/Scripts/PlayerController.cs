using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveForce = 800f;
    public float maxSpeed = 20f;       // 最大速度

    [Header("滚动参数")]
    public float rollSpeed = 12f;     // 球体滚动速度

    [Header("相机关联")]
    public CinemachineFreeLook freelookCam;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = 0.4f;
        rb.angularDrag = 0.02f;
        rb.freezeRotation = false;

        Cursor.lockState = CursorLockMode.Locked;   //锁定在屏幕中心
        Cursor.visible = false;                     //隐藏鼠标
    }

    void FixedUpdate()
    {
        // 1.读取原始输入
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(vertical) < 0.1f && Mathf.Abs(horizontal) < 0.1f)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.1f);
            return;
        }

        // 2.获取相机当前的水平朝向
        Transform cam = Camera.main.transform;

        Vector3 cameraForward = cam.forward;
        Vector3 cameraRight = cam.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // 3.计算最终移动方向
        Vector3 moveDir = cameraForward * vertical + cameraRight * horizontal;

        // 4.力驱动逻辑
        rb.AddForce(moveDir * moveForce * Time.fixedDeltaTime);

        // 5.球体滚动逻辑
        transform.Rotate(-moveDir.z * rollSpeed * Time.fixedDeltaTime,
                         0,
                         moveDir.x * rollSpeed * Time.fixedDeltaTime,
                         Space.World);

        // 6.限速逻辑
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
    }

    // 鼠标滚轮调相机距离
    void Update()
    {
        if (freelookCam == null) return;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            for (int i = 0; i < freelookCam.m_Orbits.Length; i++)
            {
                float newRadius = freelookCam.m_Orbits[i].m_Radius - scroll * 5f;
                freelookCam.m_Orbits[i].m_Radius = Mathf.Clamp(newRadius, 3f, 10f);
            }
        }
    }
}