using UnityEngine;
using System.Collections;
using SQLiteDatabase;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

///This is the real one
/// <summary>
/// This class is used to control the database processes for queries within the basic and advanced tabs.
/// </summary>
public class DatabaseControl : MonoBehaviour
{

    /// <summary>
    /// Holds a reference to the running instance of the database.
    /// </summary>
	public SQLiteDB db = SQLiteDB.Instance;

    /// <summary>
    /// Holds a reference to the QueryUIProcess script.
    /// </summary>
    public QueryUIProcess queryUIProcess;

    /// <summary>
    /// Holds a list of valid branches from the database.
    /// </summary>
	public Hashtable dbBranches = new Hashtable();
    /// <summary>
    /// Holds a list of valid awards from the database.
    /// </summary>
	public Hashtable dbAwards = new Hashtable();
    /// <summary>
    /// Holds a list of valid conflicts (wars) from the database.
    /// </summary>
	public Hashtable dbConflicts = new Hashtable();
    /// <summary>
    /// Holds a list of valid ranks from the database for a specific branch.
    /// </summary>
    public Hashtable dbRanks = new Hashtable();
    /// <summary>
    /// Holds a list of valid suffixes from the database.
    /// </summary>
    public List<string> dbSuffix;
    /// <summary>
    /// Holds a list of valid sections from the database.
    /// </summary>
    public List<string> dbSection;
    /// <summary>
    /// Holds a list of valid emblems of belief from the database.
    /// </summary>
    public List<string> dbEmblems;
    /// <summary>
    /// Holds a list of valid ranks from the database.
    /// </summary>
    public List<string> dbAllRanks;
    /// <summary>
    /// Holds a list of valid units from the database.
    /// </summary>
    public List<string> dbUnit;
    /// <summary>
    /// Holds a list of valid states from the database.
    /// </summary>
    public List<string> dbState;



    /// <summary>
    /// Holds a reference to the display field for advanced results count.
    /// </summary>
    public GameObject resultsCountA;
    /// <summary>
    /// Holds a reference to the display field for the basic results count.
    /// </summary>
    public GameObject resultsCountB;

    /// <summary>
    /// Holds a reference to the current headstone game object.
    /// </summary>
    public GameObject hsGO;
    /// <summary>
    /// Holds a reference to the button to toggle the Advanced Query navigation pointers.
    /// </summary>
    /// <remarks>
    /// This reference is necessary to control the enabling/disabling of the button.
    /// </remarks>
    public Button ptrToggleA;
    /// <summary>
    /// Holds a reference to the button to toggle the Basic Query navigation pointers.
    /// </summary>
    /// <remarks>
    /// This reference is necessary to control the enabling/disabling of the button.
    /// </remarks>
    public Button ptrToggleB;
    /// <summary>
    /// Holds a reference to the button to the Advanced Query Show All.
    /// </summary>
    /// <remarks>
    /// This reference is necessary to control the enabling/disabling of the button.
    /// </remarks>
    public Button allHSA;
    /// <summary>
    /// Holds a reference to the button to the Basic Query Show All.
    /// </summary>
    /// <remarks>
    /// This reference is necessary to control the enabling/disabling of the button.
    /// </remarks>
    public Button allHSB;
    /// <summary>
    /// Holds a reference to the button to the Advanced Query Reset.
    /// </summary>
    /// <remarks>
    /// This reference is necessary to control the enabling/disabling of the button.
    /// </remarks>
    public Button resetQueryA;
    /// <summary>
    /// Holds a reference to the button to the Basic Query Reset.
    /// </summary>
    /// <remarks>
    /// This reference is necessary to control the enabling/disabling of the button.
    /// </remarks>
    public Button resetQueryB;
    /// <summary>
    /// Holds a reference to the button to the Advanced Query Submit.
    /// </summary>
    /// <remarks>
    /// This reference is necessary to control the enabling/disabling of the button.
    /// </remarks>
    public Button submitQueryA;
    /// <summary>
    /// Holds a reference to the button to the Basic Query Submit.
    /// </summary>
    /// <remarks>
    /// This reference is necessary to control the enabling/disabling of the button.
    /// </remarks>
    public Button submitQueryB;

    /// <summary>
    ///  Holds a reference to the button for editing the advanced query.
    /// </summary>
    /// <remarks>This reference is necessary to control the editing of the fields.</remarks>
    public Button editQueryA;
    /// <summary>
    ///  Holds a reference to the button for editing the basic query.
    /// </summary>
    /// <remarks>This reference is necessary to control the editing of the fields.</remarks>
    public Button editQueryB;

    /// <summary>
    /// Holds a reference to the item dropdown on the basic tab.
    /// </summary>
    public TMP_Dropdown basicItemDD;
    /// <summary>
    /// Holds a reference to the operator dropdown on the basic tab.
    /// </summary>
    public TMP_Dropdown basicOperDD;
    /// <summary>
    /// Holds a reference to the value dropdown on the basic tab.
    /// </summary>
    public TMP_Dropdown basicValueDD;


    /// <summary>
    /// Specifies whether the navigation pointers are visible.
    /// </summary>
    public bool pointerOn;
    /// <summary>
    /// Holds a list of all the burial IDs matching the query criteria.
    /// </summary>
    public List<string> allIDs = new List<string>();

    /// <summary>
    /// Specifies whether a query is active.
    /// </summary>
    public bool queryActive;

    /// <summary>
    /// Holds the reference to the basic tab game object.
    /// </summary>
    public GameObject tabButton1;
    /// <summary>
    /// Holds the reference to the advanced tab game object.
    /// </summary>
    public GameObject tabButton2;
    /// <summary>
    /// Holds the reference to the controls tab game object.
    /// </summary>
    public GameObject tabButton3;
    /// <summary>
    /// Holds the reference to the settings tab game object.
    /// </summary>
    public GameObject tabButton4;
    
    public GameObject tabButton5;
    
    /// <summary>
    /// Holds the reference to the content of the basic tab.
    /// </summary>
    public GameObject tabContent1;
    /// <summary>
    /// Holds the reference to the content of the advanced tab.
    /// </summary>
    public GameObject tabContent2;
    /// <summary>
    /// Holds the reference to the content of the controls tab.
    /// </summary>
    public GameObject tabContent3;
    /// <summary>
    /// Holds the reference to the content of the settings tab.
    /// </summary>
    public GameObject tabContent4;

    /// <summary>
    /// Environment tab
    /// </summary>
    public GameObject tabContent5;

    /// <summary>
    /// Hold the reference to which of the tabs is currently active.
    /// </summary>
    public int activeTab;

    /// <summary>
    /// Holds the reference to the state of the close biography flag.
    /// </summary>
    public bool closeBioFlag;

    /// <summary>
    /// Hold the reference to a stack containing the bio boards that are visible.
    /// </summary>
    public List<string> bioCloseStack;
    /// <summary>
    /// Holds the reference to the advanced query row game objects.
    /// </summary>
    public GameObject[] advRows;
    /// <summary>
    /// Holds the reference to the field list dropdowns.
    /// </summary>
    public TMP_Dropdown[] fieldList;
    /// <summary>
    /// Holds the reference to the operator dropdowns.
    /// </summary>
    public TMP_Dropdown[] operList;
    /// <summary>
    /// Holds the reference to the value dropdowns.
    /// </summary>
    public TMP_Dropdown[] valueList;
    /// <summary>
    /// Holds the reference to the input fields.
    /// </summary>
    public TMP_InputField[] inputList;
    /// <summary>
    /// Holds the reference to the AND/OR dropdowns.
    /// </summary>
    public TMP_Dropdown[] andOrs;
    /// <summary>
    /// Holds the reference to the left parentheses toggles.
    /// </summary>
    public Toggle[] leftParen;
    /// <summary>
    /// Holds the reference to the right parentheses toggles.
    /// </summary>
    public Toggle[] rightParen;
    /// <summary>
    /// Holds the reference to the delete buttons.
    /// </summary>
    public Button[] delButtons;

    /// <summary>
    /// Holds each gameobject to be acted upon as a query is run
    /// </summary>
    private Queue<GameObject> headstoneQueue;

    /// <summary>
    /// Flag denoting whether or not the coroutine queue is running.
    /// </summary>
    private bool isProcessing;

    /// <summary>
    /// Enables the error listener for the database.
    /// </summary>
    void OnEnable()
    {
        SQLiteEventListener.onError += OnError;
    }

    /// <summary>
    /// Disables the error listener for the database.
    /// </summary>
    void OnDisable()
    {
        SQLiteEventListener.onError -= OnError;
    }

    /// <summary>
    /// Displays the error message in the console.
    /// </summary>
    /// <param name="err">The database error.</param>
    void OnError(string err)
    {
        Debug.Log(err);
    }

    /// <summary>
    /// Initialization of the database and hash tables for child tables. 
    /// </summary>
    /// <remarks>
    /// This process runs before the headstones are rendered.
    /// </remarks>
    void Awake()
    {
        isProcessing = false;
       // set database location (directory)
       db.DBLocation = Application.persistentDataPath;
        db.DBName = "cemVR.db";
        Debug.Log ("Database Directory : "+db.DBLocation);
        pointerOn = false;

        // open the database via refresh from the streamingassets
        db.ConnectToDefaultDatabase(db.DBName, true);

        // load the branch, conflict, and awards dictionaries
        DBReader reader = db.Select("select * from cemVRbranch");
        while (reader != null && reader.Read())
        {
            dbBranches.Add(reader.GetStringValue("code"), reader.GetStringValue("branch"));
        }

        reader = db.Select("select * from cemVRconflict");
        while (reader != null && reader.Read())
        {
            dbConflicts.Add(reader.GetStringValue("code"), reader.GetStringValue("longDesc"));
        }

        reader = db.Select("select * from cemVRawards");
        while (reader != null && reader.Read())
        {
            dbAwards.Add(reader.GetStringValue("shortDesc"), reader.GetStringValue("longDesc"));
        }

        reader = db.Select("select distinct suffix from cemVRburials");
        while (reader != null && reader.Read())
        {
            // build the list for advanced query
            dbSuffix.Add(reader.GetStringValue("suffix"));
        }

        reader = db.Select("select distinct section from cemVRburials");
        while (reader != null && reader.Read())
        {
            // build the list for basic query
            dbSection.Add(reader.GetStringValue("section"));
        }

        reader = db.Select("select distinct emblemBelief from cemVRburials");
        while (reader != null && reader.Read())
        {
            // build the list for basic query
            dbEmblems.Add(reader.GetStringValue("emblemBelief"));
        }

        reader = db.Select("select distinct rank from cemVRburials");
        while (reader != null && reader.Read())
        {
            // build the list for basic query
            dbAllRanks.Add(reader.GetStringValue("rank"));
        }

        reader = db.Select("select distinct unit from cemVRburials");
        while (reader != null && reader.Read())
        {
            // build the list for basic query
            dbUnit.Add(reader.GetStringValue("unit"));
        }

        reader = db.Select("select distinct state from cemVRburials");
        while (reader != null && reader.Read())
        {
            // build the list for basic query
            dbState.Add(reader.GetStringValue("state"));
        }

        // sort the lists
        dbSuffix.Sort();
        dbSection.Sort();
        dbEmblems.Sort();
        dbAllRanks.Sort();
        dbUnit.Sort();
        dbState.Sort();

        closeBioFlag = true;
    }

    void Start()
    {
        disableButtons();
        queryActive = false;
        activeTab = 3;
        headstoneQueue = new Queue<GameObject>();
    }

    /// <summary>
    /// Disables the baseline buttons on the query tabs.
    /// </summary>
    void disableButtons()
    {
        // setup the UI button controls
        ptrToggleA.enabled = false;
        ptrToggleB.enabled = false;
        allHSA.enabled = false;
        allHSB.enabled = false;
        submitQueryA.enabled = false;
        submitQueryB.enabled = false;
        editQueryA.enabled = false;
        editQueryB.enabled = false;
    }

    /// <summary>
    /// Enable reset and pointers on query submission.
    /// </summary>
    void enableReset()
    {
        // allow reset and pointer toggle to be enabled
        if (activeTab == 1)
        {
            // basic tab
            ptrToggleB.enabled = true;
        }
        else
        {
            // advanced tab
            ptrToggleA.enabled = true;
        }
    }
    /// <summary>
    /// Releases resources used by the database
    /// </summary>
    /// <remarks>
    /// Used to avoid any lock on database, which would then require a 
    /// restart of the editor or application after each run.
    /// </remarks>
	void OnApplicationQuit()
    {
        // release all resource using by database.
        db.Dispose();
    }

    /// <summary>
    /// Retrieves the rank description for the selected branch and rank code.
    /// </summary>
    /// <param name="tBranch">The branch of service.</param>
    /// <param name="tCode">The rank code.</param>
    /// <returns></returns>
    public string GetRank(string tBranch, string tCode)
    {
        // query for the rank description
        string tStr = "SELECT rankDesc FROM cemVRrank WHERE branch == '" + tBranch + "' and abbrev == '" + tCode + "'";
        string tOut = "not found";
        DBReader reader = db.Select(tStr);
        while (reader != null && reader.Read())
        {
            tOut = reader.GetStringValue("rankDesc");
        }

        if (tOut == "not found")
        {
            // try with a branch of OTH
            tStr = "SELECT rankDesc FROM cemVRrank WHERE branch == 'OTH' and abbrev == '" + tCode + "'";
            reader = db.Select(tStr);
            while (reader != null && reader.Read())
            {
                tOut = reader.GetStringValue("rankDesc");
            }
        }

        return (tOut);
    }

    /// <summary>
    /// Activates all headstones in the scene.
    /// </summary>
    /// <remarks>
    /// This displays the navigation pointers and branch of service biography buttons.
    /// </remarks>
    public void AllHeadstones()
    {
        // query for all headstones
        string tStr = "SELECT burialID FROM cemVRburials WHERE markerFace == 'F'";
        RunQuery(tStr);
    }

    /// <summary>
    /// Process the query from basic or advanced.
    /// </summary>
    /// <param name="tStr"></param>
    public void RunQuery(string tStr)
    {
        // process the incoming query
        allIDs.Clear();

        DBReader reader = db.Select(tStr);
        while (reader != null && reader.Read())
        {   
            // look for the game object associated with this headstone
            // not all may exist in the scene yet
            GameObject hsgo = GameObject.Find(reader.GetStringValue("burialID"));
            if (hsgo is not null)
            {
                // add this headstone to the list of IDs
                allIDs.Add(reader.GetStringValue("burialID"));
                // add it to the coroutine queue
                headstoneQueue.Enqueue(hsgo);
            }
        }

        //Process the results of the query in a coroutine
        //Note: This will also initialize their bios and button textures
        //if not yet set.
        if (!isProcessing)
        {
            StartCoroutine(runQueryCoroutine());
        }

        pointerOn = true;
        queryActive = true;
        // disable all the buttons before setting proper tab
        disableButtons();
        return;
    }

    //Processes the query results as a coroutine
    private IEnumerator runQueryCoroutine()
    {
        int counter = 0;
        isProcessing = true;
        while (headstoneQueue.Count > 0)
        {
            GameObject next = headstoneQueue.Dequeue();
            Debug.Log("PUMPKIN: " + next.gameObject.name);
            // set the logo button and pointer to active for both levels of detail
            Transform child = next.transform.Find("LogoButton0");
            if (child == null) continue;
            next.transform.Find("LogoButton0").gameObject.SetActive(true);
            next.transform.Find("LogoButton1").gameObject.SetActive(true);
            
            // set both pointers
            Transform hsPointer = next.transform.Find("pointer0");
            hsPointer.gameObject.SetActive(true);
            hsPointer = next.transform.Find("pointer1");
            hsPointer.gameObject.SetActive(true);
            HSdata hsd = next.GetComponent<HSdata>();
            
            if (!hsd.initialized)
            {
                hsd.callAwake();
                hsd.StartCoroutine(hsd.GetBranchTexture());
                hsd.initialized = true;
            }
            counter++;
            string rCnt = "Results: " + counter.ToString() + " grave site";
            if (counter != 1)
            {
                rCnt += "s";
            }
            // put it on the screen
            if (activeTab == 1)
            {
                resultsCountB.GetComponent<TMP_Text>().text = rCnt;
            }
            else
            {
                resultsCountA.GetComponent<TMP_Text>().text = rCnt;
            }

            yield return null;
        }

        // allow reset and pointer toggle to be enabled
        enableReset();
        if (activeTab == 1)
        {
            // disable the dropdowns
            disableDD();
        }
        isProcessing = false;
    }

    private void disableDD()
    {
        basicItemDD.enabled = false;
        basicOperDD.enabled = false;
        basicValueDD.enabled = false;
    }

    private void AdjustAdv(bool setting)
    {
        for (int i = 0; i <= 4; i++)
        {
            // adjust the fields on this row
            leftParen[i].enabled = setting;
            fieldList[i].enabled = setting;
            operList[i].enabled = setting;
            valueList[i].enabled = setting;
            inputList[i].enabled = setting;

            rightParen[i].enabled = setting;
            // last row does have an AND/OR
            if (i != 4)
            {
                andOrs[i].enabled = setting;
            }

            // there is no delete button on the initial row
            if (i != 0)
            {
                delButtons[i].enabled = setting;
            }
        }
    }


    /// <summary>
    /// Clears all the pointers, buttons, and query fields.
    /// </summary>
    /// <remarks>
    /// This is called from the UI reset button.
    /// </remarks>
    public void ResetPointers()
    {
        // query to reset all front side of marker
        string tStr = "SELECT burialID FROM cemVRburials WHERE markerFace == 'F'";
        allIDs.Clear();

        DBReader reader = db.Select(tStr);
        while (reader != null && reader.Read())
        {
            // get the game object associated with this headstone
            try
            {
                hsGO = GameObject.Find(reader.GetStringValue("burialID"));
                // not all may exist in the scene yet
                if (hsGO is not null)
                {
                    // set the logo button and pointer to active for both levels of detail
                    Transform hsLogo = hsGO.transform.Find("LogoButton0");
                    hsLogo.gameObject.SetActive(false);
                    hsLogo = hsGO.transform.Find("LogoButton1");
                    hsLogo.gameObject.SetActive(false);
                    Transform hsPointer = hsGO.transform.Find("pointer0");
                    hsPointer.gameObject.SetActive(false);
                    hsPointer = hsGO.transform.Find("pointer1");
                    hsPointer.gameObject.SetActive(false);

                    // use rotation object
                    Transform hsRot = hsGO.transform.Find("BillboardRotation");
                    hsRot.gameObject.SetActive(false);
                }
            } catch (System.Exception e)
            {
                Debug.Log("DatabaseControl.cs ResetPointers Error message: " + e.Message);
            }
        }
        // turn buttons off before adjusting
        disableButtons();

        // turn All on
        if (activeTab == 1)
        {
            // basic tab
            allHSB.enabled = true;
            resultsCountB.GetComponent<TMP_Text>().text = "Results:";
            // since all dropdowns have values, turn on Submit and Reset
            submitQueryB.enabled = true;
            basicItemDD.enabled = true;
            basicOperDD.enabled = true;
            basicValueDD.enabled = true;
            refreshDD();
        }
        else
        {
            // advanced tab
            allHSA.enabled = true;
            resultsCountA.GetComponent<TMP_Text>().text = "Results:";
            submitQueryA.enabled = false;
            for (int i = 0; i <= 4; i++)
            {
                leftParen[i].isOn = false;
                fieldList[i].value = 0;
                fieldList[i].RefreshShownValue();
                operList[i].options.Clear();
                operList[i].value = 0;
                operList[i].RefreshShownValue();
                valueList[i].options.Clear();
                valueList[i].value = 0;
                valueList[i].RefreshShownValue();
                inputList[i].text = "";

                // last row doesn't have an andor
                if (i != 4)
                {
                    andOrs[i].value = 0;
                    andOrs[i].RefreshShownValue();
                }
                rightParen[i].isOn = false;
            }
            // adjust the visibility of the advanced rows
            AdjustAdv(true);
            // reset the main tracking variables
            queryUIProcess.ResetRows();
        }
        pointerOn = false;
        queryActive = false;
    }


    /// <summary>
    /// Sets all the fields to enabled to edit the query.
    /// </summary>
    /// <remarks>
    /// This is called from the UI edit button.
    /// </remarks>
    public void EditAdv()
    {
        // query to reset all front side of marker
        string tStr = "SELECT burialID FROM cemVRburials WHERE markerFace == 'F'";
        allIDs.Clear();

        DBReader reader = db.Select(tStr);
        while (reader != null && reader.Read())
        {
            try
            {


                // get the game object associated with this headstone
                hsGO = GameObject.Find(reader.GetStringValue("burialID"));
                // not all may exist in the scene yet
                if (hsGO is not null)
                {
                    // set the logo button and pointer to active for both levels of detail
                    Transform hsLogo = hsGO.transform.Find("LogoButton0");
                    hsLogo.gameObject.SetActive(false);
                    hsLogo = hsGO.transform.Find("LogoButton1");
                    hsLogo.gameObject.SetActive(false);

                    Transform hsPointer = hsGO.transform.Find("pointer0");
                    hsPointer.gameObject.SetActive(false);
                    hsPointer = hsGO.transform.Find("pointer1");
                    hsPointer.gameObject.SetActive(false);
                    // use rotation object
                    Transform hsRot = hsGO.transform.Find("BillboardRotation");
                    hsRot.gameObject.SetActive(false);
                }
            } catch(System.Exception e)
            {
                Debug.Log("DatabaseControl.cs EditAdv Line 728 Error Message: " + e.Message);
            }
        }
        // turn buttons off before adjusting
        disableButtons();
        // re-enable some
        allHSA.enabled = true;
        resultsCountA.GetComponent<TMP_Text>().text = "Results:";
        submitQueryA.enabled = true;
        AdjustAdv(true);
        pointerOn = false;
        queryActive = false;
        editQueryA.enabled = false;
    }

    /// <summary>
    /// Sets all the fields to enabled to edit the query.
    /// </summary>
    /// <remarks>
    /// This is called from the UI edit button.
    /// </remarks>
    public void EditBasic()
    {
        // query to reset all front side of marker
        string tStr = "SELECT burialID FROM cemVRburials WHERE markerFace == 'F'";
        allIDs.Clear();

        DBReader reader = db.Select(tStr);
        while (reader != null && reader.Read())
        {
            try
            {
                // get the game object associated with this headstone
                hsGO = GameObject.Find(reader.GetStringValue("burialID"));
                // not all may exist in the scene yet
                if (hsGO is not null)
                {
                    // set the logo button and pointer to active for both levels of detail
                    Transform hsLogo = hsGO.transform.Find("LogoButton0");
                    hsLogo.gameObject.SetActive(false);
                    hsLogo = hsGO.transform.Find("LogoButton1");
                    hsLogo.gameObject.SetActive(false);
                    Transform hsPointer = hsGO.transform.Find("pointer0");
                    hsPointer.gameObject.SetActive(false);
                    hsPointer = hsGO.transform.Find("pointer1");
                    hsPointer.gameObject.SetActive(false);
                    // use rotation object
                    Transform hsRot = hsGO.transform.Find("BillboardRotation");
                    hsRot.gameObject.SetActive(false);
                }
            } catch(System.Exception e) 
            {
                Debug.Log("DatabaseControl.cs EditBasic Error Message: " + e.Message);
            }
        }
        // turn buttons off before adjusting
        disableButtons();
        // re-enable some
        allHSB.enabled = true;
        resultsCountB.GetComponent<TMP_Text>().text = "Results:";
        basicItemDD.enabled = true;
        basicOperDD.enabled = true;
        basicValueDD.enabled = true;
        submitQueryB.enabled = true;
        pointerOn = false;
        queryActive = false;
        editQueryB.enabled = false;
    }


    /// <summary>
    /// Closes the biography boards.
    /// </summary>
    /// <remarks>
    /// This is controlled by the closeBoards option.
    /// </remarks>
    public void CloseBios()
    {
        if (closeBioFlag)
        {
            foreach (var hs in bioCloseStack)
            {
                // get the game object associated with this headstone
                hsGO = GameObject.Find(hs);
                if (hsGO is not null)
                {
                    // deactivate the biography
                    Transform hsRot = hsGO.transform.Find("BillboardRotation");
                    hsRot.gameObject.SetActive(false);
                }
            }
            // clear out the stack
            bioCloseStack.Clear();
        }
    }


    private void refreshDD()
    {
        basicItemDD.value = 0;
        basicItemDD.RefreshShownValue();
        basicOperDD.value = 0;
        basicOperDD.RefreshShownValue();
        basicValueDD.value = 0;
        basicValueDD.RefreshShownValue();
    }

    /// <summary>
    /// Toggles the navigation pointer visibility.
    /// </summary>
    /// <remarks>
    /// This is called from the UI Pointers button.
    /// </remarks>
    public void TogglePointers()
    {
        // run through active list and turn them on/off 
        foreach (string tID in allIDs)
        {
            // get the game object associated with this headstone
            hsGO = GameObject.Find(tID);
            // process both
            Transform hsPointer0 = hsGO.transform.Find("pointer0");
            Transform hsPointer1 = hsGO.transform.Find("pointer1");
            if (pointerOn)
            {
                hsPointer0.gameObject.SetActive(false);
                hsPointer1.gameObject.SetActive(false);
            }
            else
            {
                hsPointer0.gameObject.SetActive(true);
                hsPointer1.gameObject.SetActive(true);
            }
        }
        // toggle the state of the pointers
        pointerOn = !pointerOn;
    }

    /// <summary>
    /// Process the submitted basic query with the drop down criteria.
    /// </summary>
    public void SubmitBasic()
    {
        string fieldName, operatorName, valueContent;

        // get the dropdown values
        fieldName = basicItemDD.options[basicItemDD.value].text;
        operatorName = basicOperDD.options[basicOperDD.value].text;
        valueContent = basicValueDD.options[basicValueDD.value].text;

        string queryStr = "select * from cemVRburials where ";
        // need to adjust for field names
        switch (fieldName.ToLower())
        {
            case "branch":
                // check to see if they are looking for blank branch
                if (valueContent == "")
                {
                    // for no branch, need to select the civilians
                    queryStr += "veteran";
                    valueContent = "0";
                }
                else
                {
                    // find the proper branch code to build the field name
                    foreach (DictionaryEntry item in dbBranches)
                    {
                        // only process the selected branch
                        if (valueContent == item.Value.ToString())
                        {
                            // the value content will be 1 regardless of whether 
                            // looking for equals or not equals
                            valueContent = "1";
                            fieldName = "brn" + item.Key.ToString();
                            queryStr += fieldName;
                            break;
                        }
                    }
                }
                break;
            case "award":
                // find the proper award code to build the field name
                foreach (DictionaryEntry item in dbAwards)
                {
                    // only process the selected award
                    if (valueContent == item.Value.ToString())
                    {
                        // the value content will be 1 regardless of whether 
                        // looking for equals or not equals
                        valueContent = "1";
                        fieldName = "award" + item.Key.ToString();
                        queryStr += fieldName;
                        break;
                    }
                }
                break;
            case "conflict":
                // find the proper war code to build the field name
                foreach (DictionaryEntry item in dbConflicts)
                {
                    // only process the selected war
                    if (valueContent == item.Value.ToString())
                    {
                        // the value content will be 1 regardless of whether 
                        // looking for equals or not equals
                        valueContent = "1";
                        fieldName = "war" + item.Key.ToString();
                        queryStr += fieldName;
                        break;
                    }
                }
                break;
            case "last name":
                queryStr += "lastName";
                break;
            case "section":
                queryStr += "section";
                break;
            case "emblem of belief":
                queryStr += "emblemBelief";
                break;
            case "rank":
                queryStr += "rank";
                break;
            case "unit":
                queryStr += "unit";
                break;
            case "state":
                queryStr += "state";
                break;
        }

        if (operatorName == "Equals")
        {
            queryStr += " == ";
        }
        else
        {
            queryStr += " <> ";
        }

        queryStr += "'" + valueContent + "'";
        // process the query
        RunQuery(queryStr);

        // turn the submit button off as the query has been processed
        queryActive = true;
        submitQueryB.enabled = false;
        // turn the reset button on
        resetQueryB.enabled = true;
        editQueryB.enabled = true;
        // disable the choice lists until Reset or Edit is pressed
        disableDD();
    }

    /// <summary>
    /// Process the submitted advanced query.
    /// </summary>
    public void SubmitAdvanced()
    {
        string fieldName, operName, valueContent, t4;
        string queryStr = "select * from cemVRburials where ";

        // need to loop through the rows and build it
        for (int i = 0; i <= 4; i++)
        {
            fieldName = fieldList[i].options[fieldList[i].value].text.ToLower();
            if (fieldName.Length > 4)
            {
                t4 = fieldName.Substring(fieldName.Length - 4);
            }
            else
            {
                t4 = fieldName;
            }
            // only process full rows
            if (fieldName is null || fieldName == "")
            {
                break;
            }
            operName = operList[i].options[operList[i].value].text;
            // check for name and year fields as they are text
            if (t4 == "name" || t4 == "year")
            {
                // it was a name or date field
                valueContent = inputList[i].textComponent.text;
                // strip off invisible last char
                valueContent = valueContent.Substring(0, valueContent.Length - 1);
            }
            else
            {
                // it was a dropdown
                valueContent = valueList[i].options[valueList[i].value].text;
            }

            // insert paren
            if (leftParen[i].isOn)
            {
                queryStr += "(";
            }
            switch (fieldName)
            {
                case "branch":
                    // check to see if they are looking for blank branch
                    if (valueContent == "")
                    {
                        // for no branch, need to select the civilians
                        queryStr += "veteran";
                        valueContent = "0";
                    }
                    else
                    {
                        // find the proper branch code to build the field name
                        foreach (DictionaryEntry item in dbBranches)
                        {
                            // only process the selected branch
                            if (valueContent == item.Value.ToString())
                            {
                                // the value content will be 1 regardless of whether 
                                // looking for equals or not equals
                                valueContent = "1";
                                fieldName = "brn" + item.Key.ToString();
                                queryStr += fieldName;
                                break;
                            }
                        }
                    }
                    break;
                case "award":
                    // find the proper award code to build the field name
                    foreach (DictionaryEntry item in dbAwards)
                    {
                        // only process the selected award
                        if (valueContent == item.Value.ToString())
                        {
                            // the value content will be 1 regardless of whether 
                            // looking for equals or not equals
                            valueContent = "1";
                            fieldName = "award" + item.Key.ToString();
                            queryStr += fieldName;
                            break;
                        }
                    }
                    break;
                case "conflict":
                    // find the proper war code to build the field name
                    foreach (DictionaryEntry item in dbConflicts)
                    {
                        // only process the selected war
                        if (valueContent == item.Value.ToString())
                        {
                            // the value content will be 1 regardless of whether 
                            // looking for equals or not equals
                            valueContent = "1";
                            fieldName = "war" + item.Key.ToString();
                            queryStr += fieldName;
                            break;
                        }
                    }
                    break;

                case "last name":
                    queryStr += "lastName";
                    break;
                case "first name":
                    queryStr += "firstName";
                    break;
                case "middle name":
                    queryStr += "middleName";
                    break;
                case "suffix":
                    queryStr += "suffix";
                    break;
                case "section":
                    queryStr += "section";
                    break;
                case "emblem of belief":
                    queryStr += "emblemBelief";
                    break;
                case "rank":
                    queryStr += "rank";
                    break;
                case "unit":
                    queryStr += "unit";
                    break;
                case "state":
                    queryStr += "state";
                    break;
                case "birth year":
                    queryStr += "birthYear";
                    break;
                case "death year":
                    queryStr += "deathYear";
                    break;
            }

            switch (operName)
            {
                case "Equals":
                    queryStr += " == ";
                    break;
                case "Does not equal":
                    queryStr += " <> ";
                    break;
                case "Greater than":
                    queryStr += " > ";
                    break;
                case "Less than":
                    queryStr += " < ";
                    break;
                case "Greater or equal":
                    queryStr += " >= ";
                    break;
                case "Less or equal":
                    queryStr += " >= ";
                    break;
                case "Starts with":
                    queryStr += " LIKE '" + valueContent + "%'";
                    break;
                case "Contains":
                    queryStr += " LIKE '%" + valueContent + "%'";
                    break;
            }

            // Starts with and contains were built above
            if (operName != "Starts with" && operName != "Contains")
            {
                queryStr += "'" + valueContent + "'";
            }
            // insert paren
            if (rightParen[i].isOn)
            {
                queryStr += ")";
            }

            // last row doesn't have and and/or
            if (i != 4)
            {
                if (andOrs[i].value == 1)
                {
                    queryStr += " AND ";
                }
                else if (andOrs[i].value == 2)
                {
                    queryStr += " OR ";
                }
            }
        }

        // process the query
        RunQuery(queryStr);

        // turn the submit button off as the query has been processed
        queryActive = true;
        submitQueryA.enabled = false;
        // turn the reset button on
        resetQueryA.enabled = true;
        editQueryA.enabled = true;
        // disable the fields until Reset or edit is pressed
        AdjustAdv(false);
    }

    /// <summary>
    /// Hides all of the tab contents and changes the tab colors to non-selected values.
    /// </summary>
    public void HideAllTabs()
    {
        tabContent1.SetActive(false);
        tabContent2.SetActive(false);
        tabContent3.SetActive(false);
        tabContent4.SetActive(false);
        tabContent5.SetActive(false);
        tabButton1.GetComponent<Button>().image.color = new Color32(212, 212, 212, 255);
        tabButton2.GetComponent<Button>().image.color = new Color32(212, 212, 212, 255);
        tabButton3.GetComponent<Button>().image.color = new Color32(212, 212, 212, 255);
        tabButton4.GetComponent<Button>().image.color = new Color32(212, 212, 212, 255);
        tabButton5.GetComponent<Button>().image.color = new Color32(212, 212, 212, 255);
        activeTab = 0;
    }

    /// <summary>
    /// Displays the basic query tab.
    /// </summary>
    public void ShowTab1()
    {
        HideAllTabs();
        tabContent1.SetActive(true);
        tabButton1.GetComponent<Button>().image.color = new Color32(255, 255, 255, 255);
        activeTab = 1;
        // enable the all button for basic tab
        allHSB.enabled = true;
        submitQueryB.enabled = true;
        // if a query was already active on tab switch, enable reset
        if (queryActive)
        {
            enableReset();
        }
        else
        {
            // refresh the dropdowns
            refreshDD();
        }
    }

    /// <summary>
    /// Displays the advanced query tab.
    /// </summary>
    public void ShowTab2()
    {
        HideAllTabs();
        tabContent2.SetActive(true);
        tabButton2.GetComponent<Button>().image.color = new Color32(255, 255, 255, 255);
        activeTab = 2;
        // enable the all button for Advanced tab
        allHSA.enabled = true;
        // if a query was already active on tab switch, enable reset
        if (queryActive)
        {
            enableReset();
        }
    }

    /// <summary>
    /// Displays the controls tab.
    /// </summary>
    public void ShowTab3()
    {
        HideAllTabs();
        tabContent3.SetActive(true);
        tabButton3.GetComponent<Button>().image.color = new Color32(255, 255, 255, 255);
        activeTab = 3;
    }

    /// <summary>
    /// Displays the settings tab.
    /// </summary>
    public void ShowTab4()
    {
        HideAllTabs();
        tabContent4.SetActive(true);
        tabButton4.GetComponent<Button>().image.color = new Color32(255, 255, 255, 255);
        activeTab = 4;
    }

     /// <summary>
    /// Displays the environment tab.
    /// </summary>
    public void ShowTab5()
    {
        HideAllTabs();
        tabContent5.SetActive(true);
        tabButton5.GetComponent<Button>().image.color = new Color32(255, 255, 255, 255);
        activeTab = 5;
    }
}
