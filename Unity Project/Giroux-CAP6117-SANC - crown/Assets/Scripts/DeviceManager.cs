using UnityEngine;
using UnityEngine.XR;

public class DeviceManager : MonoBehaviour
{
    [SerializeField] private GameObject xrSimulator;

    void Awake()
    {
        // Disable the simulator if an XR device is active (i.e., running on headset)
        if (XRSettings.isDeviceActive)
        {
            xrSimulator.SetActive(false);
            Debug.Log("Headset detected — disabling XR Simulator.");
        }
        else
        {
            xrSimulator.SetActive(true);
            Debug.Log("No headset detected — enabling XR Simulator.");
        }
    }
}
