using System.Collections;
using System.Collections.Generic;
using System.IO;
using SQLiteDatabase;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
public class HeadstoneSetupTool : EditorWindow
{

    //Deprecated
    public static void AssignTexturesOld()
    {
        SQLiteDB db = SQLiteDB.Instance;
        db.DBLocation = Application.persistentDataPath;
        db.DBName = "cemVR.db";
        db.ConnectToDefaultDatabase(db.DBName, true);
        HSdata[] hsdarray = GameObject.FindObjectsOfType<HSdata>();
        int size = 0;
        foreach (HSdata hsd in hsdarray)
        {
            GameObject headstone = hsd.gameObject;
            LoadDatabaseInfoFor(headstone, db);
            size++;
        }
        Debug.Log("Number of headstones in scene: " + size);
    }


    //Deprecated
    public static void BranchButtons()
    {
        SQLiteDB db = SQLiteDB.Instance;
        db.DBLocation = Application.persistentDataPath;
        db.DBName = "cemVR.db";
        db.ConnectToDefaultDatabase(db.DBName, true);
        HSdata[] hsdarray = GameObject.FindObjectsOfType<HSdata>();
        foreach (HSdata hsd in hsdarray)
        {
            LoadDatabaseInfoFor(hsd.gameObject, db);
        }
    }

    private static void LoadDatabaseInfoFor(GameObject headstone, SQLiteDB db)
    {
        //TMP_Text biography;
        string bIDstr = headstone.name.Substring(0,headstone.name.Length - 2);
        string[] hsPieces = bIDstr.Split('-');
        if (hsPieces.Length > 2)
        {
            // need to put the A/B lettering back on
            hsPieces[1] += "-" + hsPieces[2];
        }
        var sqlStr = "SELECT * FROM cemVRburials WHERE section == '" + hsPieces[0] + "' and site == '" + hsPieces[1] + "' ORDER BY markerFace";
        DBReader reader = db.Select(sqlStr);
        Transform rotPiece = headstone.transform.Find("BillboardRotation");
        Transform child = rotPiece.transform.Find("bioBoardBack");
        Transform rb = child.Find("board");
        Transform gchild = child.Find("Canvas");
        Transform ggchild = gchild.Find("biography");
        string hsBranch = "";
        string prefix = "Assets/Prefabs/Headstone Parts/LogoButton0";
        // get the text for the biography
        //biography = ggchild.GetComponent<TMP_Text>();
        //biography.text = "";
        //bool bioDone = false;
        //List<string> buildItems = new List<string>();
        //string lifeYears = "", branches = "", conflicts = "", awards = "", sex = "", mainBranch = "";
        //int bYear, dYear, numItems, cnt, addPeriod;
        while (reader != null && reader.Read())
        {
            // first check to see if a biography material exists for this headstone
            // it is stored in the headstonePhoto field
            if (reader.GetStringValue("headstonePhoto") != "")
            {
                //LoadBioMaterialTexture(rb.GetComponent<Renderer>(), headstone.name.Trim());
            }
            //The dynamic bio building will remain a part of HSdata as it's very dependent on DBControl seemingly
            // only set the first branch once per headstone for button material
            Transform logob = headstone.transform.Find("LogoButton0");
            if (hsBranch == "" && logob == null)
            {
                // if this gets expanded to other cemeteries, need to cover all subsets
                if (reader.GetStringValue("brnAA") == "1" || reader.GetStringValue("brnAR") == "1" || reader.GetStringValue("brnAC") == "1" || reader.GetStringValue("brnAT") == "1" || reader.GetStringValue("brnNC") == "1" || reader.GetStringValue("brnWS") == "1")
                {
                    // set to Army
                    hsBranch = prefix + "Army.prefab";
                }
                else if (reader.GetStringValue("brnMC") == "1")
                {
                    // set to Marine Corps
                    hsBranch = prefix + "Marines.prefab";
                }
                else if (reader.GetStringValue("brnNA") == "1")
                {
                    // set to Navy
                    hsBranch = prefix + "Navy.prefab";
                }
                else if (reader.GetStringValue("brnAF") == "1")
                {
                    // set to Air Force
                    hsBranch = prefix + "AirForce.prefab";
                }
                else if (reader.GetStringValue("brnCG") == "1")
                {
                    // set to Coast Guard
                    hsBranch = prefix + "CoastGuard.prefab";
                }
                else
                {
                    // if nothing is set, set to blank
                    hsBranch = prefix + ".prefab";
                }
                if (hsBranch != "")
                {
                    GameObject prefabB = AssetDatabase.LoadAssetAtPath<GameObject>(hsBranch);
                    Debug.Log(headstone.name);
                    Debug.Log(hsBranch);
                    GameObject branchb = PrefabUtility.InstantiatePrefab(prefabB) as GameObject;
                    branchb.transform.SetParent(headstone.transform);
                    branchb.transform.localPosition = new Vector3(0.0f,0.233700007f,0.0f);
                    branchb.transform.localScale = new Vector3(0.699999988f,0.699999988f,0.699999988f);
                    branchb.name = "LogoButton0";
                    branchb.SetActive(false);
                }
            } // end hsBranch
        }

        //Fallback
        if (hsBranch == "")
        {
            hsBranch = prefix + ".prefab";
            GameObject prefabB = AssetDatabase.LoadAssetAtPath<GameObject>(hsBranch);
            Debug.Log(headstone.name);
            Debug.Log(hsBranch);
            GameObject branchb = PrefabUtility.InstantiatePrefab(prefabB) as GameObject;
            branchb.transform.SetParent(headstone.transform);
            branchb.transform.localPosition = new Vector3(0.0f,0.233700007f,0.0f);
            branchb.transform.localScale = new Vector3(0.699999988f,0.699999988f,0.699999988f);
            branchb.name = "LogoButton0";
            branchb.SetActive(false);
        }
    }

    private static void LoadBioMaterialTexture(Renderer renderBio, string hname)
    {
        string _strBioMatFileName = Application.dataPath + "/StreamingAssets/hsBioMaterials/" + hname + "-bio.png";
        Debug.Log(_strBioMatFileName);
        byte[] tFile = File.ReadAllBytes(_strBioMatFileName);
        Texture2D texBio = new Texture2D(2,2);
        texBio.LoadImage(tFile);
        renderBio.material.SetTexture("_MainTex", texBio);
    }

    private static void LoadBranchTexture(GameObject headstone, string branch)
    {
        string _strBranchFileName = Application.dataPath + "/StreamingAssets/buttonMaterials/" + branch.Trim();
        Debug.Log(_strBranchFileName);
        byte[] tFile = File.ReadAllBytes(_strBranchFileName);
        Texture2D texBranch = new Texture2D(2,2);
        texBranch.LoadImage(tFile);
        Transform child = headstone.transform.Find("LogoButton0");
        Transform gchild = child.Find("Push");
        // get the renderer for the push button object
        Renderer _renderButton = gchild.GetComponent<Renderer>();
        _renderButton.material.SetTexture("_MainTex", texBranch);
    }
}

#endif
