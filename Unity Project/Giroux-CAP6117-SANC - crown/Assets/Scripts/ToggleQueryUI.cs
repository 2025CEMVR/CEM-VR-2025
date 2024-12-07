using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// This class is used to control the visibility and placement of the query UI
/// </summary>
public class ToggleQueryUI : MonoBehaviour
{
    /// <summary>
    /// Holds a reference to the input action for the toggle UI button.
    /// </summary>
    public InputActionReference toggleReference = null;
    /// <summary>
    /// Holds a reference to the Advanced Search help panel.
    /// </summary>
    public GameObject advHelp;
    /// <summary>
    /// Holds a reference to the Basic Search help panel.
    /// </summary>
    public GameObject basicHelp;
    /// <summary>
    /// Holds a reference to the Left Controller help panel.
    /// </summary>
    public GameObject leftControllerHelp;
    /// <summary>
    /// Holds a reference to the Right Controller help panel.
    /// </summary>
    public GameObject rightControllerHelp;
    /// <summary>
    /// Holds a reference to the Basic search tab.
    /// </summary>
    public Button basicTab;
    /// <summary>
    /// Holds a reference to the advanced search tab.
    /// </summary>
    public Button advancedTab;
    /// <summary>
    /// Holds a reference to the controls tab.
    /// </summary>
    public Button controlsTab;
    /// <summary>
    /// Holds a reference to the settings tab.
    /// </summary>
    public Button settingsTab;

    private bool oneTime = true;
    private Camera theCamera;

    /// <summary>
    /// Enables the input action for the UI toggle.
    /// </summary>
    private void Awake()
    {
        toggleReference.action.started += ToggleUI;
    }

    /// <summary>
    /// Get a reference to the main camera.
    /// </summary>
    private void Start()
    {
        theCamera = Camera.main;
    }

    /// <summary>
    /// Disables the input action for the UI toggle.
    /// </summary>
    private void OnDestroy()
    {
        toggleReference.action.started -= ToggleUI;
    }

    /// <summary>
    /// Toggles the UI visibility
    /// </summary>
    /// <param name="context">The input action callback context.</param>
    private void ToggleUI(InputAction.CallbackContext context)
    {
        bool isActive = !gameObject.activeSelf;

        // see if it is going on vs off
        if (isActive)
        {
            // move the menu in front of the camera
            SetMenuLocation();
        }

        gameObject.SetActive(isActive);
    }

    /// <summary>
    /// Places the menu in front of the user
    /// </summary>
    private void SetMenuLocation()
    {
        // move the menu in front of the camera
        gameObject.transform.position = theCamera.transform.position + theCamera.transform.forward * 0.75f;
        // rotate the menu to face the camera
        gameObject.transform.rotation = Quaternion.LookRotation(gameObject.transform.position - theCamera.transform.position);
        // set the tilt to 35 degrees
        Vector3 eulerRotation = gameObject.transform.rotation.eulerAngles;
        gameObject.transform.rotation = Quaternion.Euler(35, eulerRotation.y, eulerRotation.z);
        // set the proper height
        Vector3 setPos = new Vector3(gameObject.transform.localPosition.x, .8f, gameObject.transform.localPosition.z);
        gameObject.transform.localPosition = setPos;
    }

    /// <summary>
    /// Hides the UI panel.
    /// </summary>
    public void CloseUI()
    {
        // close the UI panel
        GameObject tUI = GameObject.Find("Query UI");
        tUI.SetActive(false);
    }

    /// <summary>
    /// Sets the new height of the UI at the initial frame based on the SetSignHeight calculation.
    /// </summary>
    void Update()
    {
        if (oneTime && SetSignHeight.camHeight > 0)
        {
            //// set the new height into the transform
            SetMenuLocation();
            oneTime = false;
        }
    }
    /// <summary>
    /// Enables the tabs on the UI while help screens are shown.
    /// </summary>
    void enableTabs()
    {
        basicTab.enabled = true;
        advancedTab.enabled = true;
        controlsTab.enabled = true;
        settingsTab.enabled = true;
    }

    /// <summary>
    /// Disables the tabs on the UI while help screens are shown.
    /// </summary>
    void disableTabs()
    {
        basicTab.enabled = false;
        advancedTab.enabled = false;
        controlsTab.enabled = false;
        settingsTab.enabled = false;
    }
    /// <summary>
    /// Shows the advanced search help information.
    /// </summary>
    public void ShowAdvInfo()
    {
        // open the help screen
        advHelp.SetActive(true);
        //disable the tabs until the screen is closed
        disableTabs();
    }

    /// <summary>
    /// Closes the advanced search help information.
    /// </summary>
    public void CloseAdvInfo()
    {
        advHelp.SetActive(false);
        //enable the tabs when the help screen is closed
        enableTabs();
    }

    /// <summary>
    /// Shows the basic search help information.
    /// </summary>
    public void ShowBasicInfo()
    {
        // open the help screen
        basicHelp.SetActive(true);
        //disable the tabs until the screen is closed
        disableTabs();
    }

    /// <summary>
    /// Closes the basic search help information.
    /// </summary>
    public void CloseBasicInfo()
    {
        basicHelp.SetActive(false);
        //enable the tabs when the help screen is closed
        enableTabs();
    }

    /// <summary>
    /// Shows the left controller help information.
    /// </summary>
    public void ShowLeftControllerInfo()
    {
        // open the help screen
        leftControllerHelp.SetActive(true);
        //disable the tabs until the screen is closed
        disableTabs();
    }

    /// <summary>
    /// Closes the left controller help information.
    /// </summary>
    public void CloseLeftControllerInfo()
    {
        leftControllerHelp.SetActive(false);
        //enable the tabs when the help screen is closed
        enableTabs();
    }

    /// <summary>
    /// Shows the right controller help information.
    /// </summary>
    public void ShowRightControllerInfo()
    {
        // open the help screen
        rightControllerHelp.SetActive(true);
        //disable the tabs until the screen is closed
        disableTabs();
    }

    /// <summary>
    /// Closes the right controller help information.
    /// </summary>
    public void CloseRightControllerInfo()
    {
        rightControllerHelp.SetActive(false);
        //enable the tabs when the help screen is closed
        enableTabs();
    }
}
