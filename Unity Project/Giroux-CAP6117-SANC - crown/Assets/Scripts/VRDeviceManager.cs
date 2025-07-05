using UnityEngine;
using UnityEngine.XR.Management;

/// <summary>
/// Manages XR Device Simulator visibility based on connected VR devices
/// </summary>
public class VRDeviceManager : MonoBehaviour
{
    [Header("XR Device Simulator")]
    [SerializeField] private GameObject xrDeviceSimulator;
    [SerializeField] private bool enableSimulatorOnDesktop = true;
    [SerializeField] private bool disableSimulatorOnVR = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private void Start()
    {
        if (xrDeviceSimulator == null)
        {
            // Try to find the XR Device Simulator automatically
            xrDeviceSimulator = GameObject.Find("XR Device Simulator");
        }
        
        ManageSimulatorVisibility();
    }

    private void ManageSimulatorVisibility()
    {
        bool hasVRDevice = HasVRDeviceConnected();
        
        if (showDebugInfo)
        {
            Debug.Log($"VR Device Manager: VR Device Connected = {hasVRDevice}");
        }

        if (xrDeviceSimulator != null)
        {
            if (hasVRDevice && disableSimulatorOnVR)
            {
                // Real VR headset detected - disable simulator
                xrDeviceSimulator.SetActive(false);
                if (showDebugInfo)
                    Debug.Log("VR Device Manager: Disabled XR Device Simulator (VR headset detected)");
            }
            else if (!hasVRDevice && enableSimulatorOnDesktop)
            {
                // No VR headset - enable simulator for desktop testing
                xrDeviceSimulator.SetActive(true);
                if (showDebugInfo)
                    Debug.Log("VR Device Manager: Enabled XR Device Simulator (desktop mode)");
            }
        }
        else
        {
            if (showDebugInfo)
                Debug.LogWarning("VR Device Manager: XR Device Simulator not found in scene");
        }
    }

    private bool HasVRDeviceConnected()
    {
        // Check if XR is initialized and active
        if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
        {
            var xrManager = XRGeneralSettings.Instance.Manager;
            if (xrManager.activeLoader != null)
            {
                // Check for specific VR devices
                var inputDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
                    UnityEngine.XR.InputDeviceCharacteristics.HeadMounted, 
                    inputDevices);
                
                if (inputDevices.Count > 0)
                {
                    if (showDebugInfo)
                        Debug.Log($"VR Device Manager: Found {inputDevices.Count} VR headset(s)");
                    return true;
                }
            }
        }

        // Additional check for Oculus devices
        if (Application.platform == RuntimePlatform.Android)
        {
            // On Android, assume Quest/Go is connected
            return true;
        }

        return false;
    }

    // Public method to manually refresh device detection
    public void RefreshDeviceDetection()
    {
        ManageSimulatorVisibility();
    }

    // Editor method to test device detection
    [ContextMenu("Test Device Detection")]
    private void TestDeviceDetection()
    {
        bool hasDevice = HasVRDeviceConnected();
        Debug.Log($"Device Detection Test: VR Device Connected = {hasDevice}");
    }
} 