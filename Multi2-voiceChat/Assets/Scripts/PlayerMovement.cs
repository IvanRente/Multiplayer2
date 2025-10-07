using FishNet.Object;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        if (!IsOwner)
            return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);
        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        transform.position += moveSpeed * Time.deltaTime * moveDirection;
    }
}