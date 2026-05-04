using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    private TargetLock targetLock;
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
    public float[] lightAttackDuration = { 0.8f, 0.567f, 0.833f};
    public float heavyAttackDuration = 0.9f;
    public float runningHeavyAttackDuration = 1.2f;
    public bool isLightAttack;
    public bool isHeavyAttack;
    
    [Header("Boss Interaction")]
    public float controlMultiplier = 1f; // 1 = normal, -1 = reversed
    public bool isMovementFrozen = false; // For Stuns/Cutscenes

    private CharacterController controller;
    private PlayerInput inputActions; // Your generated class
    private InputActionAsset activeAsset; // The actual data container

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float currentSpeed;
    private float turnSmoothVelocity;

    public GameObject menu;

    private bool isGrounded;
    private bool sprintHeld;
    private bool isCrouching;
    private bool isRolling;
    private bool isTransitioningCrouch;
    public bool isAttacking;
    private bool _isAttackingInternal = false;
    private bool _inputBuffered = false;

    private int WeaponType = 1;
    /*
    1 = sword
    2 = spear
    */

    public bool isInvincible;
    // Added this helper so your Hitbox script can find it!
    public bool isDodging => isInvincible;

    void Awake()
    {
        targetLock = GetComponent<TargetLock>();
        controller = GetComponent<CharacterController>();
        playerProperties = GetComponent<PlayerProperties>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        controller.height = standingHeight - 0.25f;
        controller.center = new Vector3(0f, standingHeight / 2f, 0f);

        var playerInputComponent = GetComponent<UnityEngine.InputSystem.PlayerInput>();

        if (playerInputComponent != null)
        {
            // 2. Initialize the wrapper using the empty constructor
            inputActions = new PlayerInput();

            // 3. Manually swap the internal asset for the one on the component
            // This is the "backdoor" to fix the read-only error
            activeAsset = playerInputComponent.actions;

            // Use the common property name for the generated wrapper's asset
            // If your generated script is named PlayerInput, it usually stores its asset in .asset
            // But since we can't assign it directly, we ensure the wrapper IS using the instance
            inputActions.devices = playerInputComponent.devices;
        }
        else
        {
            Debug.LogError("Missing Player Input component on " + gameObject.name);
        }

        weaponFramework = GetComponentInChildren<WeaponFramework>();

        if (weaponFramework != null)
            weaponFramework.attackMode = WeaponFramework.AttackMode.None;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        // 3. Enable the asset
        if (activeAsset != null)
        {
            activeAsset.Enable();
            var playerMap = activeAsset.FindActionMap("Player");

            // FindAction is much more reliable than the wrapper in this case
            playerMap.FindAction("Move").performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            playerMap.FindAction("Move").canceled += ctx => moveInput = Vector2.zero;

            playerMap.FindAction("Look").performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            playerMap.FindAction("Look").canceled += ctx => lookInput = Vector2.zero;

            playerMap.FindAction("Sprint").performed += ctx => sprintHeld = true;
            playerMap.FindAction("Sprint").canceled += ctx => sprintHeld = false;

            playerMap.FindAction("Jump").performed += OnJump;
            playerMap.FindAction("Crouch").performed += OnCrouch;
            playerMap.FindAction("Roll").performed += OnRoll;
            playerMap.FindAction("LightAttack").performed += OnLightAttack;
            playerMap.FindAction("HeavyAttack").performed += OnHeavyAttack;
            playerMap.FindAction("Heal").performed += OnHeal;

            playerMap.FindAction("Pause").performed += OnPause;
        }
    }

    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Jump.performed -= OnJump;
            inputActions.Player.Crouch.performed -= OnCrouch;
            inputActions.Player.Roll.performed -= OnRoll;
            inputActions.Player.LightAttack.performed -= OnLightAttack;
            inputActions.Player.HeavyAttack.performed -= OnHeavyAttack;
            inputActions.Player.Heal.performed -= OnHeal;
            inputActions.Player.Pause.performed -= OnPause;

            inputActions.Player.Disable();
        }
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

    // Helps Mr. Watson see if you are "moving" during Paws Up
    public float CurrentVelocityMagnitude => controller.velocity.magnitude;

    void GroundCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;
    }

    void HandleMovement()
    {
        if (isMovementFrozen) return;
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        // REVERSE CONTROLS GIMMICK
        if (playerProperties != null && playerProperties.controlsReversed)
        {
            inputDirection *= -1f;
        }

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
        if (isMovementFrozen) return;
        if (isGrounded && !isCrouching && !isRolling && !isAttacking)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("JumpTrigger");
        }
    }

    void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (isMovementFrozen) return;
        if (isRolling || isAttacking || isTransitioningCrouch)
            return;

        if (!isCrouching)
            StartCoroutine(EnterCrouch());
        else
            StartCoroutine(ExitCrouch());
    }

    void OnRoll(InputAction.CallbackContext ctx)
    {
        if (isMovementFrozen) return;
        if (isGrounded && !isRolling && !isAttacking && !isTransitioningCrouch)
            StartCoroutine(Roll());
    }

    void OnLightAttack(InputAction.CallbackContext ctx)
    {
        if (isMovementFrozen) return;
        if (!ctx.performed) return;

        if (_isAttackingInternal)
        {
            _inputBuffered = true;
        }
        else if (!isRolling && !isCrouching)
        {
            StartCoroutine(DoLightAttack());
        }
    }

    void OnHeavyAttack(InputAction.CallbackContext ctx)
    {
        if (isMovementFrozen) return;
        if (!isAttacking && !isRolling && !isTransitioningCrouch && !isCrouching)
            StartCoroutine(DoHeavyAttack());
    }

    void OnHeal(InputAction.CallbackContext ctx)
    {
        if (isMovementFrozen) return;
        if (!isAttacking && !isRolling && !isTransitioningCrouch && !isCrouching)
            playerProperties.Heal(10f);
    }

    void OnPause(InputAction.CallbackContext ctx)
    {
        if(menu.activeInHierarchy)
        {
            menu.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            menu.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
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

        while (elapsed < rollDuration)
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

        animator.SetTrigger("LightAttack1");
        yield return new WaitForSeconds(0.133f / animator.speed);
        if (weaponFramework != null) weaponFramework.attackMode = WeaponFramework.AttackMode.lightAttack;
        yield return new WaitForSeconds(0.8f / animator.speed);
        if (!_inputBuffered) { EndCombo(); yield break; }

        _inputBuffered = false;
        animator.SetTrigger("LightAttack2");
        yield return new WaitForSeconds(0.567f / animator.speed);
        if (!_inputBuffered) { EndCombo(); yield break; }

        _inputBuffered = false;
        animator.SetTrigger("LightAttack3");
        yield return new WaitForSeconds(0.833f / animator.speed);

        EndCombo();
    }

    void EndCombo()
    {
        _isAttackingInternal = false;
        _inputBuffered = false;
        isAttacking = false;
        if (weaponFramework != null) weaponFramework.attackMode = WeaponFramework.AttackMode.None;

        animator.ResetTrigger("LightAttack1");
        animator.ResetTrigger("LightAttack2");
        animator.ResetTrigger("LightAttack3");
    }

    IEnumerator DoHeavyAttack()
    {
        isAttacking = true;
        if (sprintHeld && currentSpeed > 0.1f)
        {
            animator.SetTrigger("RunningHeavyAttack");
            float elapsed = 0f;
            Vector3 attackDirection = transform.forward;

            while (elapsed < 0.75f * animator.speed)
            {
                elapsed += Time.deltaTime;
                controller.Move(attackDirection * sprintSpeed * Time.deltaTime);
                yield return null;
            }

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (weaponFramework != null) weaponFramework.attackMode = WeaponFramework.AttackMode.runningHeavy;

            while (elapsed < (runningHeavyAttackDuration - 1.6f) / animator.speed)
            {
                elapsed += Time.deltaTime;
                controller.Move(attackDirection * walkSpeed * Time.deltaTime);
                yield return null;
            }

            if (weaponFramework != null) weaponFramework.attackMode = WeaponFramework.AttackMode.None;

            yield return new WaitForSeconds((runningHeavyAttackDuration - elapsed) / animator.speed) ;
            isHeavyAttack = true;
        }
        else
        {
            isHeavyAttack = true;
            animator.SetTrigger("HeavyAttack");
            yield return new WaitForSeconds(0.26f*animator.speed);
            if (weaponFramework != null) weaponFramework.attackMode = WeaponFramework.AttackMode.heavyAttack;
            yield return new WaitForSeconds((heavyAttackDuration - 1.6f)/animator.speed);
            if (weaponFramework != null) weaponFramework.attackMode = WeaponFramework.AttackMode.None;
            yield return new WaitForSeconds((heavyAttackDuration - (heavyAttackDuration - 0.7f))/animator.speed);
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
        animator.SetInteger("WeaponType", WeaponType);

        animationSpeedAdjustment();
    }

    //changes animation speed for attack animations
    void animationSpeedAdjustment()
    {
        // Safety checks to prevent errors
        if(weaponFramework == null) return;
        if(animator == null) return;
        if (weaponFramework.weaponStats.attackSpeed <= 0) return;

        // Reset to normal speed if not attacking
        if (!isAttacking) 
        {
            animator.speed = 1f;
            return;
        }

        animator.speed = weaponFramework.weaponStats.attackSpeed;
        return;
    }
}