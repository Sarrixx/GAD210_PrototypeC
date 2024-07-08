using System.Collections;
using UnityEngine;

/// <summary> 
/// Script responsible for moving the player camera in response to detected mouse movement.
/// </summary>
public class MouseLook : MonoBehaviour
{
    [Tooltip("Toggle on to print console messages from this component.")]
    [SerializeField] private bool debug;
    [Tooltip("Controls how much the camera will move in response to detected mouse input.")]
    [SerializeField] private float sensitivity = 2;
    [Tooltip("Controls how much the camera will continue to move after mouse input ceases.")]
    [SerializeField] private float drag = 5;
    [Tooltip("Defines the minimum vertical view angle (how far the player can look down).")]
    [SerializeField][Range(-10, -90)] private float verticalClampMin = -70f;
    [Tooltip("Defines the maximum vertical view angle (how far the player can look up).")]
    [SerializeField][Range(10, 90)] private float verticalClampMax = 70f;
    [Header("Camera Sway")]
    [Tooltip("Defines how sensitive the camera sway is. Set to 0 to turn off.")]
    [SerializeField][Min(0)] private float swaySensitivity = 1f;
    [Tooltip("Defines how quickly the camera transitions from swaying back to the normal state.")]
    [SerializeField][Range(0.1f, 10)] private float cameraSwaySpeed = 1f;
    [Tooltip("Defines the minimum angle achieved by the camera sway.")]
    [SerializeField][Range(-25f, -1)] private float swayClampMin = -10f;
    [Tooltip("Defines the maximum angle achieved by the camera sway.")]
    [SerializeField][Range(1f, 25)] private float swayClampMax = 10f;

    private Transform character;
    private Coroutine timedShakeRoutine;
    private Vector2 mouseDir;
    private Vector2 smoothing;
    private Vector2 result;
    private Vector3 originalLocalPosition;
    private bool cameraShaking = false;
    private float currentShakeMagnitude = 0f;
    private float currentSwayAmount = 0f;

    /// <summary>
    /// Enables/disables the ability to move the camera by moving the mouse.
    /// </summary>
    public bool LookEnabled { get; set; } = true;
    /// <summary>
    /// Enables/disables the swap applied to camera movement.
    /// </summary>
    public bool SwayEnabled { get; set; } = true;

    /// <summary>
    /// Singleton reference to instance of mouse look.
    /// (Only one mouse look script should be active in the scene.)
    /// </summary>
    public static MouseLook Instance { get; private set; }

    /// <summary>
    /// Awake is called before Start.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            character = transform.root;
            cameraShaking = false;
            originalLocalPosition = transform.localPosition;
            AlignForwardWithCharacter();
        }
        else if (Instance != this)
        {
            Log($"More than one MouseLook script detected in the scene!\n\nMouseLook implements the singleton pattern, there should only be one insance of the script in the scene at any time.", 1);
        }
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    private void Update()
    {
        if (LookEnabled == true)
        {
            mouseDir = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            mouseDir *= sensitivity;
            smoothing = Vector2.Lerp(smoothing, mouseDir, 1 / drag);
            result += smoothing;
            result.y = Mathf.Clamp(result.y, verticalClampMin, verticalClampMax);
            SetRotation(result.x, -result.y);

            ApplyCameraSway();
        }

        if (cameraShaking == true)
        {
            float x = Random.Range(-0.1f, 0.1f) * currentShakeMagnitude;
            float y = Random.Range(-0.1f, 0.1f) * currentShakeMagnitude;

            transform.localPosition = new Vector3(originalLocalPosition.x + x, originalLocalPosition.y + y, originalLocalPosition.z);
        }
    }

    /// <summary>
    /// Sets the rotation of the camera and parent object based on the passed parameters.
    /// </summary>
    /// <param name="characterYRot">The amount of rotation to be applied to the Y axis of the parent object.</param>
    /// <param name="cameraXRot">The amount of rotation to be applied to the X axis of this object.</param>
    /// <param name="lookEnabled">The state setting for mouse control of the camera after applying the rotation.</param>
    private void SetRotation(float characterYRot, float cameraXRot, bool lookEnabled = true)
    {
        LookEnabled = false;
        character.rotation = Quaternion.AngleAxis(characterYRot, character.up);
        transform.localRotation = Quaternion.AngleAxis(cameraXRot, Vector3.right);
        LookEnabled = lookEnabled;
    }

    /// <summary>
    /// Calculates and applies the interpolation for the camera sway.
    /// </summary>
    private void ApplyCameraSway()
    {
        if (SwayEnabled == true)
        {
            float targetSwayAmount = -mouseDir.x * swaySensitivity;
            currentSwayAmount = Mathf.Lerp(currentSwayAmount, targetSwayAmount, Time.deltaTime * cameraSwaySpeed);
            currentSwayAmount = Mathf.Clamp(currentSwayAmount, swayClampMin, swayClampMax);
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, currentSwayAmount);
        }
    }

    /// <summary>
    /// Coroutine responsible for timing and toggling the camera shake when active.
    /// </summary>
    /// <param name="time">The amount of time to shake the camera for.</param>
    /// <param name="magnitude">The intensity of the camera shake.</param>
    /// <returns>Yield returns a new WaitForSeconds with a value of the provided time parameter.</returns>
    private IEnumerator ShakeForTime(float time, float magnitude)
    {
        ToggleCameraShake(true, magnitude);
        yield return new WaitForSeconds(time);
        ToggleCameraShake(false);
        timedShakeRoutine = null;
    }

    /// <summary>
    /// Calculates a rotation based on the provided direction and camera offset, and force applies it to the camera.
    /// </summary>
    /// <param name="direction">The desired direction vector for the character to face.</param>
    /// <param name="cameraOffset">The desired camera Y offset.</param>
    /// <param name="lookEnabled">The state setting for mouse control of the camera after applying the rotation.</param>
    public void OverrideRotation(Vector3 direction, float cameraOffset, bool lookEnabled)
    {
        Vector3 flattenedDirection = new Vector3(direction.x, direction.y, direction.z).normalized;
        float characterRotation = Mathf.Atan2(flattenedDirection.x, flattenedDirection.z) * Mathf.Rad2Deg;
        result = new Vector2(characterRotation, result.y + cameraOffset);
        SetRotation(characterRotation, -result.y, lookEnabled);
    }

    /// <summary>
    /// Synchronises the current mouse look rotation with the forward direction of this object's parent.
    /// </summary>
    public void AlignForwardWithCharacter()
    {
        OverrideRotation(character.forward, 0, true);
    }

    /// <summary>
    /// Toggles the camera shake on and sets the magnitude of the shaking effect.
    /// </summary>
    /// <param name="toggle">If true, the camera will shake. If false, the camera will stop shaking.</param>
    /// <param name="magnitude">Value that controls the intensity of the shaking effect.</param>
    public void ToggleCameraShake(bool toggle, float magnitude = 1f)
    {
        cameraShaking = toggle;
        currentShakeMagnitude = magnitude;
    }

    /// <summary>
    /// Toggles the camera shake on for the passed number of seconds, and sets the magnitude of the shaking effect.
    /// </summary>
    /// <param name="time">The number of seconds the camera will shake for.</param>
    /// <param name="magnitude">Value that controls the intensity of the shaking effect.</param>
    public void TimedCameraShake(float time, float magnitude)
    {
        if (timedShakeRoutine != null)
        {
            StopCoroutine(timedShakeRoutine);
        }
        timedShakeRoutine = StartCoroutine(ShakeForTime(time, magnitude));
    }

    /// <summary>
    /// Logs a formatted debugging messaged to the console, of the warning level specified.
    /// </summary>
    /// <param name="message">The message to be printed in the console.
    /// Will always have [MOUSE LOOK] and the name of the associated game objected concatenated as a prefix.</param>
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
                    Debug.Log($"[MOUSE LOOK] - {gameObject.name}: {message}");
                    break;
                case 1:
                    Debug.LogWarning($"[MOUSE LOOK] - {gameObject.name}: {message}");
                    break;
                case 2:
                    Debug.LogError($"[MOUSE LOOK] - {gameObject.name}: {message}");
                    break;
            }
        }
    }
}