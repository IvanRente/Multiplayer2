using UnityEngine;
using FishNet.Object;

[RequireComponent(typeof(CharacterController))]
public class SimpleMovement : NetworkBehaviour
{
    public float speed = 5f;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Only allow input for this player's owner
        if (!IsOwner)
            return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v);

        // Move relative to world space
        controller.SimpleMove(move * speed);
    }
}