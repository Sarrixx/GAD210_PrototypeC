using UnityEngine;

public class GravityVolume : MonoBehaviour
{
    [SerializeField] private bool invert = false;
    [SerializeField] [Range(0.01f, 2f)] private float gravityModifier = 0.25f;
    [SerializeField] [Range(0.1f, 5f)] private float jumpForceModifier = 1.25f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") == true && other.TryGetComponent(out PlayerController controller) == true)
        {
            if (invert == false)
            {
                controller.ModifyGravity(controller.Gravity * gravityModifier, controller.JumpForce * jumpForceModifier);
            }
            else
            {
                controller.ModifyGravity();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") == true && other.TryGetComponent(out PlayerController controller) == true)
        {
            if (invert == false)
            {
                controller.ModifyGravity();
            }
            else
            {
                controller.ModifyGravity(controller.Gravity * gravityModifier, controller.JumpForce * jumpForceModifier);
            }
        }
    }
}
