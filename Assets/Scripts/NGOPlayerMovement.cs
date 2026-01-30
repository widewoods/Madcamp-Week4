using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NGOPlayerMovement : NetworkBehaviour
{
  [SerializeField] float moveSpeed = 5f;
  [SerializeField] float turnSpeed = 720f;

  [SerializeField] float gravity = -20f;
  [SerializeField] float jumpHeight = 1.5f;

  CharacterController controller;
  InputAction moveAction;
  InputAction jumpAction;

  private float verticalVelocity;

  void Awake()
  {
    controller = GetComponent<CharacterController>();
    if (controller == null)
      controller = gameObject.AddComponent<CharacterController>();
    moveAction = InputSystem.actions.FindAction("Move");
    jumpAction = InputSystem.actions.FindAction("Jump");
  }

  void Update()
  {
    if (!IsOwner) return;

    Vector2 moveValue = moveAction.ReadValue<Vector2>();
    Vector3 input = new Vector3(moveValue.x, 0f, moveValue.y);
    if (input.sqrMagnitude > 1f) input.Normalize();


    Vector3 moveDir = transform.right * input.x + transform.forward * input.z;
    moveDir.y = 0f;
    if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

    if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;

    if (controller.isGrounded && jumpAction.WasPressedThisFrame())
    {
      verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    verticalVelocity += gravity * Time.deltaTime;

    Vector3 velocity = moveDir * moveSpeed;
    velocity.y = verticalVelocity;

    controller.Move(velocity * Time.deltaTime);
  }

}
