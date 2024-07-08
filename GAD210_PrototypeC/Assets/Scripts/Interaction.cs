using UnityEngine;

/// <summary> 
/// Script responsible for detecting and responding to interactions within the game world.
/// </summary>
public class Interaction : MonoBehaviour
{
    [Tooltip("Toggle on to print console messages from this component.")]
    [SerializeField] private bool debug;
    [Tooltip("The distance that the player can reach interactions.")]
    [SerializeField] private float distance = 3f;
    [Tooltip("The layers to query for interactions.")]
    [SerializeField] private LayerMask interactionLayers;

    private Interactable engagedInteraction;

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    private void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, distance, interactionLayers) == true || engagedInteraction != null)
        {
            if (debug == true)
            {
                Debug.DrawRay(transform.position, transform.forward * distance, Color.green, 0.2f);
            }

            if (Input.GetButtonDown("Interaction") == true)
            {
                if (engagedInteraction == null)
                {
                    if (hitInfo.transform.TryGetComponent(out IInteractable target) == true)
                    {
                        if (target.OnInteract(out engagedInteraction) == true)
                        {
                            Log($"Interacted with {hitInfo.transform.gameObject.name}.");
                        }
                    }
                }
                else if(engagedInteraction.OnDisengageInteraction() == true)
                {
                    engagedInteraction = null;
                }
            }
        }
    }

    /// <summary>
    /// Logs a formatted debugging messaged to the console, of the warning level specified.
    /// </summary>
    /// <param name="message">The message to be printed in the console.
    /// Will always have [PLAYER INTERACTION] and the name of the associated game objected concatenated as a prefix.</param>
    /// <param name="level">A level of 0 prints a standard message.
    /// A level of 1 prints a warning message.
    /// A level of 2 prints an error message.</param>
    public void Log(string message, int level = 0)
    {
        if (debug == true)
        {
            switch (level)
            {
                default: case 0:
                    Debug.Log($"[PLAYER INTERACTION] - {gameObject.name}: {message}");
                    break;
                case 1:
                    Debug.LogWarning($"[PLAYER INTERACTION] - {gameObject.name}: {message}");
                    break;
                case 2:
                    Debug.LogError($"[PLAYER INTERACTION] - {gameObject.name}: {message}");
                    break;
            }
        }
    }
}

/// <summary>
/// Interface that provides the framework for integrating scripts with the interaction system.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Method used to define interaction response functionality for a class.
    /// </summary>
    /// <param name="engagedAction">Parameter should have the value of the interaction being interacted with assigned to it,
    /// if the interaction includes disengage functionality.</param>
    /// <returns>Should return true if the interaction was successful.</returns>
    bool OnInteract(out Interactable engagedAction);

    /// <summary>
    /// Method used to define functionality for disengaging with an interaction.
    /// </summary>
    /// <returns>Should return true if disengaging with the current interaction was successful.</returns>
    bool OnDisengageInteraction();
}