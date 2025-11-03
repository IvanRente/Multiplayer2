using FishNet.Object;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;

    public float jumpHeight = 1.2f;
    public float gravity = -19.6f;
    public Transform groundCheck;
    public float groundRadius = 0.25f;
    public LayerMask groundMask;

    private CharacterController _cc;
    private Vector3 _vel;
    private bool _isGrounded;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        GroundProbe();

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 move = (transform.right * input.x + transform.forward * input.z);
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        _cc.Move(move * speed * Time.deltaTime);

        if (_isGrounded && _vel.y < 0f) _vel.y = -2f;
        if (_isGrounded && Input.GetButtonDown("Jump"))
            _vel.y = Mathf.Sqrt(2f * -gravity * jumpHeight);

        _vel.y += gravity * Time.deltaTime;
        _cc.Move(_vel * Time.deltaTime);
    }

    private void GroundProbe()
    {
        Vector3 origin = (groundCheck != null) ? groundCheck.position : transform.position + Vector3.down * (_cc.height * 0.5f);
        _isGrounded = Physics.CheckSphere(origin, groundRadius, groundMask, QueryTriggerInteraction.Ignore);
    }
}
