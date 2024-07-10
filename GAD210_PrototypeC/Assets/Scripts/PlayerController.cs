using UnityEngine;

/// <summary>
/// Script responsible for responding to player movement inputs, allowing them to navigate the game world.
/// </summary>
[RequireComponent(typeof(CharacterController), typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("Toggle on to print console messages from this component.")]
    [SerializeField] private bool debug;
    [Header("Movement Properties")]
    [Tooltip("Defines the default speed at which the player will move.")]
    [Range(1f, 10f)][SerializeField] private float defaultSpeed;
    [Header("Physics Properties")]
    [Tooltip("Defines how strong gravity is.")]
    [Range(1f, 100f)][SerializeField] private float gravity;
    [Tooltip("Defines how strong gravity is.")]
    [Range(0f, 10f)][SerializeField] private float headDetectionRange = 0.5f;
    [Range(0f, 10f)][SerializeField] private float groundDetectionRange = 0.5f;
    [Header("Jumping Properties")]
    [Tooltip("Disables the ability for the player to jump when toggled to true.")]
    [SerializeField] private bool disableJump = false;
    [Tooltip("Defines the force at which the player will jump.")]
    [Range(0f, 30f)][SerializeField] private float jumpForce;
    [Tooltip("The audio clips for jumping. (Will be randomly selected from.)")]
    [SerializeField] private AudioClip[] jumpClips;

    private float defaultGravity = 0f;
    private float defaultJumpForce = 0f;
    private float velocity = 0f;
    private float currentMovementSpeed = 0f;
    private Vector3 motionFrameStep;
    private CharacterController controller;
    //private Footsteps footsteps;

    public float Gravity { get { return gravity; } }
    public float JumpForce { get { return jumpForce; } }

    [Header("Sprinting Properties")]
    [SerializeField] private bool sprintingEnabled = false;
    [Tooltip("Movement speed multiplier while sprinting.")]
    [Range(1f, 3f)][SerializeField] private float sprintMultiplier = 2f;
    [Tooltip("Defines the rate at which stamina is drained.")]
    [Range(0.001f, 2f)][SerializeField] private float staminaDrainRate = 0.5f;
    [Tooltip("Defines the rate at which stamina charges.")]
    [Range(0.001f, 2f)][SerializeField] private float staminaChargeRate = 0.7f;

    private float currentStamina = 1f;

    [Header("Crouching Properties")]
    [Tooltip("The amount of time to transition between crouch states.")]
    [Range(0.01f, 2f)][SerializeField] private float crouchTime = 0.2f;
    [Tooltip("Movement speed multiplier while crouching.")]
    [Range(0.1f, 1f)][SerializeField] private float crouchMultiplier = 0.5f;

    private float crouchTimer = -1f;
    private float controllerHeight = 0;
    private float startHeight = 0;
    private float targetHeight = 0;
    private bool crouching = false;

    /// <summary>
    /// Returns true if the player controller is currently sprinting.
    /// </summary>
    public bool Sprinting { get; private set; } = false;

    /// <summary>
    /// Enables/disables the ability to move the player character by device input.
    /// </summary>
    public bool MovementEnabled { get; set; } = true;
    /// <summary>
    /// A static reference for immediate access to the active instance of the player controller.
    /// This implements the singleton pattern to ensure that only one active instance of the player controller can be active in the scene.
    /// </summary>
    public static PlayerController Instance { get; private set; }


    /// <summary>
    /// Awake is called before Start.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (TryGetComponent(out controller) == false)
            {
                Log($"{gameObject.name} requires a Character Controller component!", 1);
            }
            else
            {
                controllerHeight = controller.height;
            }
            defaultGravity = gravity;
            defaultJumpForce = jumpForce;
        }
        else if(Instance != this)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    private void Start()
    {
        currentMovementSpeed = defaultSpeed;
    }

    /// <summary>
    /// FixedUpdate may be called more than once per frame.
    /// </summary>
    private void FixedUpdate()
    {
        if (controller != null)
        {
            if (controller.isGrounded == true)
            {
                velocity = -gravity * Time.deltaTime;
            }
            else
            {
                velocity -= gravity * Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) == true)
        {
            Application.OpenURL("https://forms.gle/XoRZPWFpwxzbp8Ka9");
            Application.Quit();
        }

        if (Physics.SphereCast(transform.position, controller.radius, Vector3.up, out RaycastHit _, headDetectionRange) == true)
        {
            if (velocity > 0)
            {
                velocity = 1 / -velocity;
            }
        }

        if (MovementEnabled == true)
        {
            if (sprintingEnabled == true)
            {
                if (Sprinting == false && Input.GetButtonDown("Sprint") == true)
                {
                    ToggleSprintSpeed(true);
                }
                else if (Sprinting == true && Input.GetButtonUp("Sprint") == true)
                {
                    ToggleSprintSpeed(false);
                }
                else if (controller.isGrounded == true && Sprinting == true && Input.GetButton("Sprint") == false)
                {
                    ToggleSprintSpeed(false);
                }
                //else if (controller.isGrounded == true && Sprinting == false && Input.GetButton("Sprint") == true)
                //{
                //    ToggleSprintSpeed(true);
                //}
            }
            if (crouching == false && Input.GetButton("Crouch") == true)
            {
                ToggleCrouch(true);
            }
            else if (crouching == true && Input.GetButtonUp("Crouch") == true)
            {
                ToggleCrouch(false);
            }
            if (controller.isGrounded == true && crouching == true && Input.GetButton("Crouch") == false)
            {
                ToggleCrouch(false);
            }
            Jump();
            ApplyMovementTick();
        }
        else
        {
            ApplyMovementTick(true);
        }

        CrouchTick();
        StaminaTick();
    }

    private void OnDrawGizmos()
    {
        if (debug == true)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * headDetectionRange, controller != null ? controller.radius : 0.25f);
        }
    }

    /// <summary>
    /// Calculates the movement step for the current frame, and applies movement to the character controller.
    /// </summary>
    /// <param name="ignoreInputs">When true, the calculated motion step for the frame will ignore the detected movement inputs.
    /// When false,  the calculated motion step will account for detected movement inputs.</param>
    private void ApplyMovementTick(bool ignoreInputs = false)
    {
        if (controller != null)
        {
            motionFrameStep = Vector3.zero;
            if (ignoreInputs == false)
            {
                float verticalInput = Input.GetAxisRaw("Vertical");
                float horizontalInput = Input.GetAxisRaw("Horizontal");
                motionFrameStep += transform.forward * verticalInput;
                motionFrameStep += transform.right * horizontalInput;
                motionFrameStep = currentMovementSpeed * motionFrameStep.normalized;
            }
            motionFrameStep.y += velocity;
            controller.Move(motionFrameStep * Time.deltaTime);
        }
    }

    /// <summary>
    /// Calculates the amount of stamina to subtract or recharge for the frame, based on whether sprinting is active or not.
    /// </summary>
    private void StaminaTick()
    {
        if (Sprinting == false)
        {
            if (currentStamina < 1)
            {
                currentStamina += staminaChargeRate * Time.deltaTime;
                if (currentStamina >= 1f)
                {
                    currentStamina = 1f;
                }
            }
        }
        else
        {
            if (currentStamina > 0)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                if (currentStamina <= 0)
                {
                    currentStamina = 0;
                    ToggleSprintSpeed(false);
                }
            }
        }
    }

    private void CrouchTick()
    {
        if (crouchTimer >= 0)
        {
            crouchTimer += Time.deltaTime;
            controller.height = Mathf.Lerp(startHeight, targetHeight, crouchTimer / crouchTime);
            if (crouchTimer >= crouchTime)
            {
                crouchTimer = -1f;
            }
        }
    }

    /// <summary>
    /// Causes the character to jump when jumping input has been detected.
    /// </summary>
    private void Jump()
    {
        if (disableJump == false)
        {
            if (controller != null && controller.isGrounded == true && Input.GetButtonDown("Jump") == true)
            {
                velocity = jumpForce;
            }
        }
    }

    private bool ToggleCrouch(bool toggle)
    {
        if (toggle == true)
        {
            if (crouching == false && controller.isGrounded == true)
            {
                ToggleSprintSpeed(false);
                crouching = true;
                targetHeight = controllerHeight / 2;
                startHeight = controller.height;
                crouchTimer = 0;

                currentMovementSpeed = defaultSpeed * crouchMultiplier;
                return true;
            }
        }
        else
        {
            if (crouching == true && controller.isGrounded == true)
            {
                crouching = false;
                targetHeight = controllerHeight;
                startHeight = controller.height;
                crouchTimer = 0;
                currentMovementSpeed = defaultSpeed;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Toggles the current movement speed between default speed and sprinting.
    /// </summary>
    /// <param name="toggle">If true, movement speed will be set to sprint speed.
    /// If false, movement speed will be set to default speed.</param>
    private void ToggleSprintSpeed(bool toggle)
    {
        if (toggle == true)
        {
            if (Sprinting == false && controller.isGrounded == true)
            {
                ToggleCrouch(false);
                Sprinting = true;
                currentMovementSpeed = defaultSpeed * sprintMultiplier;
            }
        }
        else
        {
            if (Sprinting == true && controller.isGrounded == true)
            {
                Sprinting = false;
                currentMovementSpeed = defaultSpeed;
            }
        }
    }

    /// <summary>
    /// Forces the character to move to a position.
    /// </summary>
    /// <param name="position">The vector coordinates of the location to teleport the player to, in world space.</param>
    public void TeleportToPosition(Vector3 position)
    {
        if (controller != null)
        {
            controller.enabled = false;
            ///###
            ///this part might not necessary depending on the context of the game systems
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            ///###
            transform.position = position;
            controller.enabled = true;
        }
    }

    /// <summary>
    /// Toggles the player's ability to move the player controller.
    /// </summary>
    /// <param name="toggle">A value of true will enable movement if not already enabled.
    /// A value of false will disable movement if not already disabled.</param>
    /// <returns>Returns true if movement was successfully toggled to the provided state.
    /// Returns false if the current movement state is already the toggled to the provided state.</returns>
    public bool ToggleMovement(bool toggle)
    {
        if (toggle == true && MovementEnabled == true)
        {
            MovementEnabled = false;
            return true;
        }
        else if (toggle == false && MovementEnabled == false)
        {
            MovementEnabled = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Logs a formatted debugging messaged to the console, of the warning level specified.
    /// </summary>
    /// <param name="message">The message to be printed in the console.
    /// Will always have [PLAYER CONTROLLER] and the name of the associated game objected concatenated as a prefix.</param>
    /// <param name="level">A level of 0 prints a standard message.
    /// A level of 1 prints a warning message.
    /// A level of 2 prints an error message.</param>
    public void Log(string message, int level = 0)
    {
        if (debug == true)
        {
            switch (level)
            {
                default:
                case 0:
                    Debug.Log($"[PLAYER CONTROLLER] - {gameObject.name}: {message}");
                    break;
                case 1:
                    Debug.LogWarning($"[PLAYER CONTROLLER] - {gameObject.name}: {message}");
                    break;
                case 2:
                    Debug.LogError($"[PLAYER CONTROLLER] - {gameObject.name}: {message}");
                    break;
            }
        }
    }

    public void ModifyGravity(float newGravity = -1, float newJumpForce = -1)
    {
        if(newGravity < 0 || newJumpForce < 0)
        {
            gravity = defaultGravity;
            jumpForce = defaultJumpForce;
        }
        else
        {
            gravity = newGravity;
            jumpForce = newJumpForce;
        }
    }
}