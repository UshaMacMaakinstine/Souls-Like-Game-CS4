using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    private WeaponFramework weaponFramework;

    private PlayerProperties playerProperties;

    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 5.5f;
    public float crouchSpeed = 1.5f;
    public float rotationSmoothTime = 0.1f;

    [Header("Jump and Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Crouch")]
    public float standingHeight = 2f;
    public float crouchHeight = 1.2f;
    public float crouchTransitionDuration = 0.25f;

    [Header("Roll")]
    public float rollSpeed = 6.5f;
    public float rollDuration = 0.7f;

    [Header("Attack")]
    public float lightAttackDuration = 0.6f;
    public float heavyAttackDuration = 0.9f;
    public float runningHeavyAttackDuration = 1.2f;
    public bool isLightAttack;
    public bool isHeavyAttack;

    private CharacterController controller;
    private PlayerInput inputActions;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float currentSpeed;
    private float turnSmoothVelocity;

    private bool isGrounded;
    private bool sprintHeld;
    private bool isCrouching;
    private bool isRolling;
    private bool isTransitioningCrouch;
    public bool isAttacking;
    private bool _isAttackingInternal = false;
    private bool _inputBuffered = false;

    public bool isInvincible;


    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerProperties = GetComponent<PlayerProperties>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        controller.height = standingHeight - 0.25f;
        controller.center = new Vector3(0f, standingHeight / 2f, 0f);

        inputActions = new PlayerInput();
        weaponFramework = GetComponentInChildren<WeaponFramework>();

        weaponFramework.attackMode = WeaponFramework.AttackMode.None;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {

        inputActions.Player.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.Sprint.performed += ctx => sprintHeld = true;
        inputActions.Player.Sprint.canceled += ctx => sprintHeld = false;

        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Crouch.performed += OnCrouch;
        inputActions.Player.Roll.performed += OnRoll;
        inputActions.Player.LightAttack.performed += OnLightAttack;
        inputActions.Player.HeavyAttack.performed += OnHeavyAttack;
        inputActions.Player.Heal.performed += OnHeal;
    }

    void OnDisable()
    {
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Crouch.performed -= OnCrouch;
        inputActions.Player.Roll.performed -= OnRoll;
        inputActions.Player.LightAttack.performed -= OnLightAttack;
        inputActions.Player.HeavyAttack.performed -= OnHeavyAttack;
        inputActions.Player.Heal.performed -= OnHeal;

        inputActions.Player.Disable();
    }

    void Update()
    {
        GroundCheck();

        if (!isRolling && !isTransitioningCrouch && !isAttacking)
        {
            HandleMovement();
        }

        ApplyGravity();
        UpdateAnimator();
    }

    void GroundCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;
    }

    void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        bool isMoving = inputDirection.magnitude >= 0.1f;

        bool shouldSprint = sprintHeld && !isCrouching && isMoving;

        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (shouldSprint)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = walkSpeed;

        if (isMoving)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        if (isGrounded && !isCrouching && !isRolling && !isAttacking)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("JumpTrigger");
        }
    }

    void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (isRolling || isAttacking || isTransitioningCrouch)
            return;

        if (!isCrouching)
            StartCoroutine(EnterCrouch());
        else
            StartCoroutine(ExitCrouch());
    }

    void OnRoll(InputAction.CallbackContext ctx)
    {
        if (isGrounded && !isRolling && !isAttacking && !isTransitioningCrouch)
            StartCoroutine(Roll());
    }

    void OnLightAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (_isAttackingInternal)
        {
            _inputBuffered = true; // Record the click but don't start a new coroutine
        }
        else if (!isRolling && !isCrouching)
        {
            StartCoroutine(DoLightAttack());
        }
    }

    void OnHeavyAttack(InputAction.CallbackContext ctx)
    {
        if (!isAttacking && !isRolling && !isTransitioningCrouch && !isCrouching)
            StartCoroutine(DoHeavyAttack());
    }

    void OnHeal(InputAction.CallbackContext ctx)
    {
        if (!isAttacking && !isRolling && !isTransitioningCrouch && !isCrouching)
            playerProperties.Heal(10f);
    }

    IEnumerator EnterCrouch()
    {
        isTransitioningCrouch = true;
        animator.SetTrigger("StandToCrouch");

        float elapsed = 0f;
        float startHeight = controller.height;

        while (elapsed < crouchTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crouchTransitionDuration);

            controller.height = Mathf.Lerp(startHeight, crouchHeight, t);
            controller.center = new Vector3(0f, controller.height / 2f, 0f);
            yield return null;
        }

        controller.height = crouchHeight;
        controller.center = new Vector3(0f, crouchHeight / 2f, 0f);

        isCrouching = true;
        isTransitioningCrouch = false;
    }

    IEnumerator ExitCrouch()
    {
        isTransitioningCrouch = true;
        animator.SetTrigger("CrouchToStand");

        float elapsed = 0f;
        float startHeight = controller.height;

        while (elapsed < crouchTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crouchTransitionDuration);

            controller.height = Mathf.Lerp(startHeight, standingHeight, t);
            controller.center = new Vector3(0f, controller.height / 2f, 0f);
            yield return null;
        }

        controller.height = standingHeight - 0.25f;
        controller.center = new Vector3(0f, standingHeight / 2f, 0f);

        isCrouching = false;
        isTransitioningCrouch = false;
    }

    bool CanStandUp()
    {
        Vector3 origin = transform.position + Vector3.up * crouchHeight;
        float checkDistance = standingHeight - crouchHeight + 0.1f;

        return !Physics.SphereCast(origin, controller.radius * 0.9f, Vector3.up, out _, checkDistance);
    }

    IEnumerator Roll()
    {
        isInvincible = true;
        isRolling = true;
        controller.center = new Vector3(0f, (standingHeight / 2f) + 1f, 0f);
        animator.SetTrigger("RollTrigger");

        float elapsed = 0f;
        Vector3 rollDirection = transform.forward;

        while (elapsed < (rollDuration * 0.60f))
        {
            elapsed += Time.deltaTime;
            controller.Move(rollDirection * rollSpeed * Time.deltaTime);
            yield return null;
        }

        while(elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rollDuration);
            float centerY = Mathf.Lerp((standingHeight / 2f) + 1f, (standingHeight / 2f), t);
            controller.center = new Vector3(0f, centerY, 0f);
            controller.Move(rollDirection * rollSpeed * Time.deltaTime);
            yield return null;
        }

        isRolling = false;
        isInvincible = false;
    }

    IEnumerator DoLightAttack()
    {
        _isAttackingInternal = true;
        isAttacking = true;

        // --- HIT 1 ---
        animator.SetTrigger("LightAttack1");
        yield return new WaitForSeconds(0.133f);
        weaponFramework.attackMode = WeaponFramework.AttackMode.lightAttack;
        // Wait for the length of the animation clip
        yield return new WaitForSeconds(0.8f);
        if (!_inputBuffered) { EndCombo(); yield break; }

        // --- HIT 2 ---
        _inputBuffered = false;
        animator.SetTrigger("LightAttack2");
        yield return new WaitForSeconds(0.567f);
        if (!_inputBuffered) { EndCombo(); yield break; }

        // --- HIT 3 ---
        _inputBuffered = false;
        animator.SetTrigger("LightAttack3");
        yield return new WaitForSeconds(0.833f); // Final hit duration

        EndCombo();
    }

    void EndCombo()
    {
        _isAttackingInternal = false;
        _inputBuffered = false;
        isAttacking = false;
        weaponFramework.attackMode = WeaponFramework.AttackMode.None;

        // Safety: Clear all triggers so spamming doesn't restart the loop immediately
        animator.ResetTrigger("LightAttack1");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
    }

    IEnumerator DoHeavyAttack()
    {
        isAttacking = true;
        if(sprintHeld && currentSpeed > 0.1f)
        {
            animator.SetTrigger("RunningHeavyAttack");
            float elapsed = 0f;
            Vector3 attackDirection = transform.forward;

            while (elapsed < 0.75f)
            {
                elapsed += Time.deltaTime;
                controller.Move(attackDirection * sprintSpeed * Time.deltaTime);
                yield return null;
            }

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            weaponFramework.attackMode = WeaponFramework.AttackMode.runningHeavy;

            while (elapsed < (runningHeavyAttackDuration - 1.6f))
            {
                elapsed += Time.deltaTime;
                controller.Move(attackDirection * walkSpeed * Time.deltaTime);
                yield return null;
            }

            weaponFramework.attackMode = WeaponFramework.AttackMode.None;

            yield return new WaitForSeconds(runningHeavyAttackDuration - elapsed);
            isHeavyAttack = true;
        }
        else
        {
            isHeavyAttack = true;
            animator.SetTrigger("HeavyAttack");
            yield return new WaitForSeconds(0.26f);
            weaponFramework.attackMode = WeaponFramework.AttackMode.heavyAttack;
            yield return new WaitForSeconds((heavyAttackDuration - 1.6f));
            weaponFramework.attackMode = WeaponFramework.AttackMode.None;
            yield return new WaitForSeconds((heavyAttackDuration - (heavyAttackDuration - 0.7f)));
        }
        isAttacking = false;
        isHeavyAttack = false;
    }

    public IEnumerator TriggerInvincibiltyFrames(float time)
    {
        isInvincible = true;

        yield return new WaitForSeconds(time);

        isInvincible = false;

    }

    void UpdateAnimator()
    {
        float speed = Mathf.Clamp01(moveInput.magnitude);
        bool shouldSprint = sprintHeld && !isCrouching && speed > 0.1f;

        animator.SetFloat("Speed", speed);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsSprinting", shouldSprint);
        animator.SetBool("IsAttacking", isAttacking);
    }
}