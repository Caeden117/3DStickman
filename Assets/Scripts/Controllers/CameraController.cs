using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform lookAtPoint;
    
    [Space, SerializeField] private float tumbleSpeed = 360f;
    [SerializeField] private float trackSpeed = 15f;
    [SerializeField] private float dollySpeed = 15f;

    private void Update()
    {
        HandleTumble();
        HandleTrack();
        HandleDolly();
        HandleCameraLookAt();
    }

    private void HandleCameraLookAt()
    {
        var direction = lookAtPoint.position - transform.position;
        transform.forward = direction.normalized;
    }

    private void HandleTumble()
    {
        if (!Input.GetKey(KeyCode.LeftAlt) || !Input.GetMouseButton(0)) return;

        var mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        var rightRotation = Quaternion.AngleAxis(mouseDelta.x * (tumbleSpeed * Time.deltaTime), Vector3.up);
        var upRotation = Quaternion.AngleAxis(-mouseDelta.y * (tumbleSpeed * Time.deltaTime), transform.right);

        var direction = lookAtPoint.position - transform.position;
        var distance = direction.magnitude;

        direction = rightRotation * upRotation * direction;
        transform.position = lookAtPoint.position - direction.normalized * distance;

        transform.rotation = rightRotation * upRotation * transform.rotation;
        lookAtPoint.rotation = rightRotation * upRotation * lookAtPoint.rotation;

    }

    private void HandleTrack()
    {
        if (!Input.GetKey(KeyCode.LeftAlt) || !Input.GetMouseButton(1)) return;

        var mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        var right = transform.right;
        var up = transform.up;

        var translation = (-right * mouseDelta.x + -up * mouseDelta.y) * (trackSpeed * Time.deltaTime);

        transform.position += translation;
        lookAtPoint.position += translation;
    }

    private void HandleDolly()
    {
        if (!Input.GetKey(KeyCode.LeftAlt)) return;

        var scrollDeltaRaw = Input.GetAxis("Mouse ScrollWheel");

        // Early return if no scroll input
        if (Mathf.Approximately(scrollDeltaRaw, 0f)) return;

        var scrollDelta = Mathf.Sign(scrollDeltaRaw);
        var direction = lookAtPoint.position - transform.position;

        // Prevent dollying in too close
        if (direction.magnitude < 0.5f && scrollDelta > 0f) return;

        var translation = direction.normalized * scrollDelta * (dollySpeed * Time.deltaTime);

        transform.position += translation;
    }
}
