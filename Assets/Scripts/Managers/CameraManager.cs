using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CinemachineVirtualCamera VirtualCamera = null;
    public bool IsEdgeScrollingEnabled = false;
    public float EdgeBorder = 10f;

    public float MoveSpeed = 50f;
    public float RotateSpeed = 100f;

    private Vector3 DragOrigin;
    private Vector3 CameraOrigin;
    private bool IsDragging = false;

    public CameraMovementType MovementType { get; set; } = CameraMovementType.Crescent;
    public float ZoomSpeed = 40f;
    public float ZoomMin = 5f;
    public float ZoomMax = 100f;

    private Vector3 initialOffset;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private float zoomProgress = 0f;

    void Start()
    {
        VirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (VirtualCamera != null)
        {
            var transposer = VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            initialOffset = transposer.m_FollowOffset;
            initialRotation = VirtualCamera.transform.rotation;

            // Target rotation rotating around the X-axis
            targetRotation = Quaternion.Euler(90f, initialRotation.eulerAngles.y, initialRotation.eulerAngles.z);
        }
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
    }

    // Movement
    private void HandleMovement()
    {
        TryHandleWASD();
        if (IsEdgeScrollingEnabled) TryHandleEdgeScrolling();

        if (Input.GetMouseButtonDown(1))
        {
            DragOrigin = Input.mousePosition;
            CameraOrigin = transform.position;
            IsDragging = true;
        }

        if (Input.GetMouseButtonUp(1)) IsDragging = false;
        if (IsDragging) TryHandleDragAndPan();
    }

    private void TryHandleWASD()
    {
        var dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) dir += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) dir += Vector3.back;
        if (Input.GetKey(KeyCode.A)) dir += Vector3.left;
        if (Input.GetKey(KeyCode.D)) dir += Vector3.right;

        var moveDir = transform.forward * dir.z + transform.right * dir.x;
        transform.position += MoveSpeed * Time.deltaTime * moveDir;
    }

    private void TryHandleEdgeScrolling()
    {
        if (!IsEdgeScrollingEnabled) return;

        if (Input.mousePosition.x < EdgeBorder) transform.position += MoveSpeed * Time.deltaTime * Vector3.left;
        if (Input.mousePosition.x > Screen.width - EdgeBorder) transform.position += MoveSpeed * Time.deltaTime * Vector3.right;
        if (Input.mousePosition.y < EdgeBorder) transform.position += MoveSpeed * Time.deltaTime * Vector3.back;
        if (Input.mousePosition.y > Screen.height - EdgeBorder) transform.position += MoveSpeed * Time.deltaTime * Vector3.forward;
    }

    private void TryHandleDragAndPan()
    {
        var pos = Camera.main.ScreenToViewportPoint(DragOrigin - Input.mousePosition);
        var dir = new Vector3(pos.x * 100, 0, pos.y * 100);
        var moveDir = transform.forward * dir.z + transform.right * dir.x;

        transform.position = CameraOrigin + moveDir;
    }

    // Rotation
    private void HandleRotation()
    {
        var rotateDir = Vector3.zero;
        if (Input.GetKey(KeyCode.Q)) rotateDir += Vector3.down;
        if (Input.GetKey(KeyCode.E)) rotateDir += Vector3.up;

        transform.Rotate(RotateSpeed * Time.deltaTime * rotateDir);
    }

    // Zooming
    private void HandleZoom()
    {
        if (MovementType == CameraMovementType.Straight) TryStraightZoom();
        else if (MovementType == CameraMovementType.Crescent) TryZoomTowardsTop();
        else HandleZoomY();
    }

    private void TryStraightZoom()
    {
        VirtualCamera = VirtualCamera != null ? VirtualCamera : FindObjectOfType<CinemachineVirtualCamera>();
        if (Input.mouseScrollDelta.y == 0) return;

        var transposer = VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        var offset = transposer.m_FollowOffset;
        offset += -1 * Input.mouseScrollDelta.y * offset.normalized;

        if (offset.magnitude > ZoomMax || offset.magnitude < ZoomMin) return;

        transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, offset, ZoomSpeed * Time.deltaTime);
    }

    private void HandleZoomY()
    {
        VirtualCamera = VirtualCamera != null ? VirtualCamera : FindObjectOfType<CinemachineVirtualCamera>();
        if (Input.mouseScrollDelta.y == 0) return;

        var transposer = VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        var offset = transposer.m_FollowOffset;
        offset.y += -1 * Input.mouseScrollDelta.y * ZoomSpeed;
        offset.y = Mathf.Clamp(offset.y, ZoomMin, ZoomMax);

        transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, offset, ZoomSpeed * Time.deltaTime);
    }

    private void TryZoomTowardsTop()
    {
        if (VirtualCamera == null) return;

        var transposer = VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (Input.mouseScrollDelta.y != 0)
        {
            // Calculate zoom progress based on mouse scroll
            zoomProgress += -1 * Input.mouseScrollDelta.y * ZoomSpeed * Time.deltaTime;
            zoomProgress = Mathf.Clamp01(zoomProgress); // Keep progress between 0 and 1

            // Interpolate between initial offset and a point directly above the target
            Vector3 targetOffset = new Vector3(0, initialOffset.magnitude, 0);
            transposer.m_FollowOffset = Vector3.Lerp(initialOffset, targetOffset, zoomProgress);

            // Interpolate the rotation around the X-axis
            VirtualCamera.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, zoomProgress);
        }
    }
}

public enum CameraMovementType : short
{
    Straight = 0,
    SmoothedStraight = 1,
    Crescent = 2
}