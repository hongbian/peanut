using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float turnSpeed = 10f; 
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float companionPushForce = 4f; 

    private Animator animator;
    private CharacterController controller;
    private float verticalVelocity;
    private CompanionCarry companionCarry;

void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        companionCarry = GetComponent<CompanionCarry>();
    }

    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        bool isSprinting = Input.GetKey(sprintKey) && input.sqrMagnitude > 0.01f;
        float corruptionPenalty = companionCarry != null ? companionCarry.GetCarrySpeedMultiplier() : 1f;
        float currentSpeed = speed * (isSprinting ? sprintMultiplier : 1f) * corruptionPenalty;

        Vector3 move = Vector3.ClampMagnitude(input, 1f) * currentSpeed;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f; 
            if (Input.GetButtonDown("Jump")) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 fullMove = move + Vector3.up * verticalVelocity;
        controller.Move(fullMove * Time.deltaTime);

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        animator.SetFloat("Speed", move.magnitude);

        animator.speed = isSprinting ? sprintMultiplier : 1f;
    }



    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        CompanionFollow companion = hit.collider.GetComponent<CompanionFollow>();
        if (companion == null) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
        companion.ApplyPush(pushDir.normalized * companionPushForce);
    }
}
