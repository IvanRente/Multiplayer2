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
        if (!IsOwner)
            return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0, v);

        controller.SimpleMove(move * speed);
    }
}