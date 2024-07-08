using UnityEngine;

/// <summary>
/// Script that serves as a base template for more specific interaction scripts.
/// </summary>
public abstract class Interactable : MonoBehaviour, IInteractable
{
    [Tooltip("Toggle on to print console messages from this component.")]
    [SerializeField] private bool debug;
    [Tooltip("Toggles whether the interaction can be interacted with.")]
    [SerializeField] private bool active = true;
    [Tooltip("If true, the interaction will be disabled after it's successfully interacted with.")]
    [SerializeField] protected bool disableOnSuccessfulInteraction = false;
    [Tooltip("The audio clip that plays on interaction.")]
    [SerializeField] protected AudioClip interactionClip;

    protected AudioSource aSrc;

    /// <summary>
    /// Returns a reference to the audio clip that will play when the interaction is executed.
    /// </summary>
    protected AudioClip InteractionClip { get { return interactionClip; } }
    /// <summary>
    /// Toggles whether the interaction can be interacted with.
    /// </summary>
    public bool Active { get { return active; } set { active = value; } }

    /// <summary>
    /// Awake is called before Start.
    /// </summary>
    protected virtual void Awake()
    {
        if(TryGetComponent(out aSrc) == false)
        {
            Log($"{gameObject.name} requires an Audio Source component!", 1);
        }
    }

    /// <summary>
    /// Plays the provided clip through the provided source (via one shot).
    /// </summary>
    /// <param name="clip">The audio clip to be played.</param>
    /// <param name="source">The audio source to play the provided clip.</param>
    /// <param name="volScale">The volume to play the provided clip at.</param>
    /// <returns>Returns true if the provided audio source and audio clip are not equal to null.</returns>
    protected bool PlaySound(AudioClip clip, AudioSource source, float volScale = 1f)
    {
        if (source != null && clip != null)
        {
            source.PlayOneShot(clip, volScale);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Executed when this interaction is interacted with by the player.
    /// </summary>
    /// <param name="interactionInfo">The interaction data from the interaction system.</param>
    /// <returns>Returns true if the interaction was successfully completed.</returns>
    public virtual bool OnInteract(out Interactable engagedAction)
    {
        engagedAction = null;
        if (active == true)
        {
            PlaySound(interactionClip, aSrc);
            if (disableOnSuccessfulInteraction == true)
            {
                Active = false;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Method executed when this interaction is disengaged with.
    /// </summary>
    /// <returns>Returns false.</returns>
    public virtual bool OnDisengageInteraction()
    {
        return false;
    }

    /// <summary>
    /// Logs a formatted debugging messaged to the console, of the warning level specified.
    /// </summary>
    /// <param name="message">The message to be printed in the console.
    /// Will always have [INTERACTION] and the name of the associated game objected concatenated as a prefix.</param>
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
                    Debug.Log($"[INTERACTION] - {gameObject.name}: {message}");
                    break;
                case 1:
                    Debug.LogWarning($"[INTERACTION] - {gameObject.name}: {message}");
                    break;
                case 2:
                    Debug.LogError($"[INTERACTION] - {gameObject.name}: {message}");
                    break;
            }
        }
    }
}