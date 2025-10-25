using FishNet.Object;
using UnityEngine;

public class FirstPersonFPS : NetworkBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Camera cameraPrefab;

    [Header("Look")]
    [SerializeField] private float sensitivity = 1.2f;
    [SerializeField] private float pitchMin = -85f;
    [SerializeField] private float pitchMax = 85f;

    private float _pitch;
    private Camera _cam;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner)
        {
            var remoteCam = GetComponentInChildren<Camera>(true);
            if (remoteCam) remoteCam.enabled = false;
            enabled = false;
            return;
        }

        _cam = Instantiate(cameraPrefab, head);
        _cam.transform.localPosition = Vector3.zero;
        _cam.transform.localRotation = Quaternion.identity;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsOwner) return;

        float mouseX = Input.GetAxis("Mouse X") * 100f * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * 100f * sensitivity * Time.deltaTime;

        transform.Rotate(0f, mouseX, 0f, Space.Self);

        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);
        head.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
}
