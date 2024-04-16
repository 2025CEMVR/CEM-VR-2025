using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// This class is used to control the teleportation ray navigation via the thumb stick.
/// </summary>
public class RayToggler : MonoBehaviour
{
    [SerializeField] private InputActionReference activateReference = null;
    [SerializeField] private GameObject reticleGO;
    private XRRayInteractor rayInteractor = null;
    private bool isEnabled = false;

    /// <summary>
    /// Obtains the reference to the ray interactor.
    /// </summary>
    private void Awake()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
    }

    /// <summary>
    /// Sets up the references to starting and canceling the ray interactor.
    /// </summary>
    private void OnEnable()
    {
        activateReference.action.started += ToggleRay;
        activateReference.action.canceled += ToggleRay;
    }

    /// <summary>
    /// Deactivates the references to start/cancel of the ray interactor.
    /// </summary>
    private void OnDisable()
    {
        activateReference.action.started -= ToggleRay;
        activateReference.action.canceled -= ToggleRay;
    }

    /// <summary>
    /// Enables the ray based on the stick press.
    /// </summary>
    /// <param name="context">The callback context.</param>
    private void ToggleRay(InputAction.CallbackContext context)
    {
        isEnabled = context.control.IsPressed();
    }

    /// <summary>
    /// Calls the ApplyStatus routine to toggle the ray appropriately.
    /// </summary>
    private void LateUpdate()
    {
        ApplyStatus();
    }

    /// <summary>
    /// Toggles the ray appropriately.
    /// </summary>
    /// <remarks>
    /// If the ray is turned off, the custom reticule is hidden as Unity fails to do so properly.
    /// </remarks>
    private void ApplyStatus()
    {
        if (rayInteractor.enabled != isEnabled)
        {
            rayInteractor.enabled = isEnabled;
        }
        else
        {
            // turn off the reticle since Unity failed to do so
            reticleGO.SetActive(false);
        }
    }

}
