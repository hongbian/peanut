using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float turnSpeed = 10f; // now a smoothing factor, not degrees/sec
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float companionPushForce = 4f; // player is bigger/stronger, shoves companion aside on contact

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
        // 1. Read input as a WORLD-space direction
        //    D/A = world X (left/right along the corridor)
        //    W/S = world Z (into the scene / toward the camera)
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        bool isSprinting = Input.GetKey(sprintKey) && input.sqrMagnitude > 0.01f;
        float corruptionPenalty = companionCarry != null ? companionCarry.GetCarrySpeedMultiplier() : 1f;
        float currentSpeed = speed * (isSprinting ? sprintMultiplier : 1f) * corruptionPenalty;

        // clamp so diagonal isn't faster than straight
        Vector3 move = Vector3.ClampMagnitude(input, 1f) * currentSpeed;

        // Gravity + jump (kept separate from horizontal `move` so it doesn't skew rotation/animation)
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f; // small downward force to stay grounded
            if (Input.GetButtonDown("Jump")) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        verticalVelocity += gravity * Time.deltaTime;

        // 2. Move via the CharacterController so its trigger/collision sweep actually runs
        //    (Transform.Translate bypasses CC and OnTriggerEnter/Exit never fire)
        Vector3 fullMove = move + Vector3.up * verticalVelocity;
        controller.Move(fullMove * Time.deltaTime);

        // 3. Rotate to FACE the movement direction, smoothly
        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        // 4. Animator speed = how fast we're actually moving horizontally (ignores vertical/jump)
        animator.SetFloat("Speed", move.magnitude);

        // No run clip yet — speed up the walk cycle's playback as a placeholder for sprinting
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