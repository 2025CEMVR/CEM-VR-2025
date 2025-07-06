using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This class sets up the grip and trigger for the controller.
/// </summary>
[RequireComponent(typeof(ActionBasedController))]
public class HandController : MonoBehaviour
{
    /// <summary>
    /// Holds a reference to the controller.
    /// </summary>
    ActionBasedController controller;

    /// <summary>
    /// Holds a reference to the hand.
    /// </summary>
    public Hand hand;

    /// <summary>
    /// Retrieves the controller component.
    /// </summary>
    void Start()
    {
        controller = GetComponent<ActionBasedController>();
    }

    /// <summary>
    /// Sets the grip and trigger values in the hand object.
    /// </summary>
    void Update()
    {
        if (controller != null && hand != null)
        {
            if (controller.selectAction.action != null)
            {
                hand.SetGrip(controller.selectAction.action.ReadValue<float>());
            }

            if (controller.activateAction.action != null)
            {
                hand.SetTrigger(controller.activateAction.action.ReadValue<float>());
            }
        }
    }
}
