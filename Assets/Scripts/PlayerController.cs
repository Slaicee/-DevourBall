using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    public float moveForce = 800f;
    public float maxSpeed = 20f;       // ����ٶ�

    [Header("旋转")]
    public float rollSpeed = 12f;     // ��������ٶ�

    [Header("视角")]
    public CinemachineFreeLook freelookCam;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = 0.4f;
        rb.angularDrag = 0.02f;
        rb.freezeRotation = false;

        if (GameStateManager.Instance == null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void FixedUpdate()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState != GameState.Playing)
            return;

        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(vertical) < 0.1f && Mathf.Abs(horizontal) < 0.1f)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.1f);
            return;
        }

        Transform cam = Camera.main.transform;

        Vector3 cameraForward = cam.forward;
        Vector3 cameraRight = cam.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDir = cameraForward * vertical + cameraRight * horizontal;

        rb.AddForce(moveDir * moveForce * Time.fixedDeltaTime);

        transform.Rotate(-moveDir.z * rollSpeed * Time.fixedDeltaTime,
                         0,
                         moveDir.x * rollSpeed * Time.fixedDeltaTime,
                         Space.World);

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
    }

    void Update()
    {
        if (freelookCam == null) return;
        if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState != GameState.Playing)
            return;

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