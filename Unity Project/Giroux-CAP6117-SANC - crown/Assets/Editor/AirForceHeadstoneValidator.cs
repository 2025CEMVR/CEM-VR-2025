using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SQLiteDatabase;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AirForceHeadstoneValidator : EditorWindow
{
    [MenuItem("Tools/Air Force Headstone Validator")]
    public static void ShowWindow()
    {
        GetWindow<AirForceHeadstoneValidator>("Air Force Headstone Validator");
    }

    private SQLiteDB db;
    private List<string> databaseResults = new List<string>();
    private List<string> foundGameObjects = new List<string>();
    private List<string> missingGameObjects = new List<string>();
    private bool hasRunValidation = false;
    private string debugInfo = "";

    private void OnGUI()
    {
        GUILayout.Label("Air Force Headstone Validation Tool", EditorStyles.boldLabel);

        // Check scene status
        Scene cemeteryScene = SceneManager.GetSceneByName("CemeteryAssetsFull");
        bool sceneLoaded = cemeteryScene.isLoaded;

        GUILayout.Label($"Scene Status: CemeteryAssetsFull loaded = {sceneLoaded}", EditorStyles.boldLabel);

        if (!sceneLoaded)
        {
            GUILayout.Label("CemeteryAssetsFull scene is not loaded!", EditorStyles.boldLabel);
            if (GUILayout.Button("Load CemeteryAssetsFull Scene"))
            {
                LoadCemeteryScene();
            }
            GUILayout.Space(10);
        }

        if (GUILayout.Button("Initialize Database"))
        {
            InitializeDatabase();
        }

        if (db != null)
        {
            GUILayout.Label($"Database Status: Connected to {db.DBName}", EditorStyles.boldLabel);

            if (sceneLoaded && GUILayout.Button("Run Air Force Headstone Validation"))
            {
                RunValidation();
            }
            else if (!sceneLoaded)
            {
                GUILayout.Label("Cannot run validation - scene not loaded", EditorStyles.boldLabel);
            }

            if (hasRunValidation)
            {
                DisplayResults();
            }
        }
        else
        {
            GUILayout.Label("Database Status: Not Connected", EditorStyles.boldLabel);
        }

        if (!string.IsNullOrEmpty(debugInfo))
        {
            GUILayout.Space(10);
            GUILayout.Label("Debug Information:", EditorStyles.boldLabel);
            GUILayout.TextArea(debugInfo, GUILayout.Height(100));
        }
    }

    private void LoadCemeteryScene()
    {
        try
        {
            string scenePath = "Assets/Scenes/CemeteryAssetsFull.unity";

            var openedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            EditorSceneManager.SetActiveScene(openedScene);

            debugInfo = $"✅ CemeteryAssetsFull scene loaded additively in Editor.\nScene path: {openedScene.path}";
            Debug.Log(debugInfo);
        }
        catch (System.Exception e)
        {
            debugInfo = $" Failed to load CemeteryAssetsFull scene: {e.Message}\nPlease ensure the scene path is correct and scene is added to Build Settings.";
            Debug.LogError(debugInfo);
        }
    }

    private void InitializeDatabase()
    {
        try
        {
            db = SQLiteDB.Instance;
            db.DBLocation = Application.persistentDataPath;
            db.DBName = "cemVR.db";
            db.ConnectToDefaultDatabase(db.DBName, true);
            Debug.Log("Database initialized successfully!");
            debugInfo = "Database initialized successfully!";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize database: {e.Message}");
            debugInfo = $"Database initialization failed: {e.Message}";
        }
    }

    private void RunValidation()
    {
        if (db == null)
        {
            Debug.LogError("Database not initialized!");
            debugInfo = "Database not initialized!";
            return;
        }

        databaseResults.Clear();
        foundGameObjects.Clear();
        missingGameObjects.Clear();
        debugInfo = "";

        Debug.Log("=== AIR FORCE HEADSTONE VALIDATION ===");

        Scene cemeteryScene = SceneManager.GetSceneByName("CemeteryAssetsFull");
        string sceneInfo = $"CemeteryAssetsFull scene loaded: {cemeteryScene.isLoaded}\n";
        sceneInfo += $"CemeteryAssetsFull scene name: {cemeteryScene.name}\n";
        sceneInfo += $"Active scene: {SceneManager.GetActiveScene().name}\n";
        debugInfo += sceneInfo;

        Debug.Log(sceneInfo);

        if (!cemeteryScene.isLoaded)
        {
            string errorMsg = "CemeteryAssetsFull scene is not loaded! This is likely why no GameObjects are found.";
            Debug.LogError(errorMsg);
            debugInfo += errorMsg + "\n";
            return;
        }

        GameObject testObj = GameObject.Find("A-18-B-F");
        string testInfo = $"Test object A-18-B-F found: {testObj != null}\n";
        debugInfo += testInfo;
        Debug.Log(testInfo);

        if (testObj == null)
        {
            string errorMsg = "Could not find test headstone A-18-B-F. Scene may not be properly loaded.";
            Debug.LogError(errorMsg);
            debugInfo += errorMsg + "\n";
            return;
        }

        string countQuery = "SELECT COUNT(*) as count FROM cemVRburials WHERE brnAF = '1'";
        Debug.Log($"Executing count query: {countQuery}");
        debugInfo += $"Executing count query: {countQuery}\n";

        DBReader countReader = db.Select(countQuery);
        if (countReader != null && countReader.Read())
        {
            int airForceCount = countReader.GetIntValue("count");
            string countInfo = $"Total Air Force records in database: {airForceCount}\n";
            Debug.Log(countInfo);
            debugInfo += countInfo;

            if (airForceCount == 0)
            {
                string warningMsg = "No Air Force records found in database! Testing with all headstones instead.";
                Debug.LogWarning(warningMsg);
                debugInfo += warningMsg + "\n";
                TestWithAllHeadstones();
                return;
            }
        }

        string query = "SELECT burialID, firstName, lastName, section, site FROM cemVRburials WHERE brnAF = '1' AND markerFace = 'F'";
        Debug.Log($"Executing query: {query}");
        debugInfo += $"Executing query: {query}\n";

        DBReader reader = db.Select(query);

        if (reader == null)
        {
            string errorMsg = "Query returned null reader!";
            Debug.LogError(errorMsg);
            debugInfo += errorMsg + "\n";
            return;
        }

        int totalInDatabase = 0;

        while (reader != null && reader.Read())
        {
            string burialID = reader.GetStringValue("burialID");
            string firstName = reader.GetStringValue("firstName");
            string lastName = reader.GetStringValue("lastName");
            string section = reader.GetStringValue("section");
            string site = reader.GetStringValue("site");

            databaseResults.Add(burialID);
            totalInDatabase++;

            Debug.Log($"Database Entry: {burialID} - {firstName} {lastName} ({section}-{site})");
        }

        Debug.Log($"Total Air Force headstones in database: {totalInDatabase}");
        debugInfo += $"Total Air Force headstones in database: {totalInDatabase}\n";

        int foundCount = 0;
        int missingCount = 0;

        foreach (string burialID in databaseResults)
        {
            GameObject headstoneGO = GameObject.Find(burialID);

            if (headstoneGO != null)
            {
                // Check if LogoButton0 exists (like the in-game query tool does)
                Transform logoButton0 = headstoneGO.transform.Find("LogoButton0");
                bool hasLogoButton = logoButton0 != null;
                
                if (hasLogoButton)
                {
                    foundGameObjects.Add(burialID);
                    foundCount++;
                    Debug.Log($"✅ Found GameObject: {burialID} (Has LogoButton0: {hasLogoButton})");
                }
                else
                {
                    missingGameObjects.Add(burialID);
                    missingCount++;
                    Debug.Log($"⚠️ GameObject exists but missing LogoButton0: {burialID}");
                }
            }
            else
            {
                missingGameObjects.Add(burialID);
                missingCount++;
                Debug.Log($" Missing GameObject: {burialID}");
            }
        }

        Debug.Log($"=== VALIDATION RESULTS ===");
        Debug.Log($"Total in Database: {totalInDatabase}");
        Debug.Log($"Found GameObjects with LogoButton0: {foundCount}");
        Debug.Log($"Missing GameObjects or LogoButton0: {missingCount}");
        Debug.Log($"Match Rate: {(foundCount * 100.0f / totalInDatabase):F1}%");

        debugInfo += $"=== VALIDATION RESULTS ===\n";
        debugInfo += $"Total in Database: {totalInDatabase}\n";
        debugInfo += $"Found GameObjects with LogoButton0: {foundCount}\n";
        debugInfo += $"Missing GameObjects or LogoButton0: {missingCount}\n";
        debugInfo += $"Match Rate: {(foundCount * 100.0f / totalInDatabase):F1}%\n";

        hasRunValidation = true;
    }

    private void TestWithAllHeadstones()
    {
        Debug.Log("=== TESTING WITH ALL HEADSTONES ===");
        debugInfo += "=== TESTING WITH ALL HEADSTONES ===\n";

        string query = "SELECT burialID FROM cemVRburials WHERE markerFace = 'F' LIMIT 10";
        Debug.Log($"Executing query: {query}");
        debugInfo += $"Executing query: {query}\n";

        DBReader reader = db.Select(query);

        if (reader == null)
        {
            string errorMsg = "Query returned null reader!";
            Debug.LogError(errorMsg);
            debugInfo += errorMsg + "\n";
            return;
        }

        int totalInDatabase = 0;

        while (reader != null && reader.Read())
        {
            string burialID = reader.GetStringValue("burialID");
            databaseResults.Add(burialID);
            totalInDatabase++;
            Debug.Log($"Database Entry: {burialID}");
        }

        Debug.Log($"Total headstones in database (first 10): {totalInDatabase}");
        debugInfo += $"Total headstones in database (first 10): {totalInDatabase}\n";

        int foundCount = 0;
        int missingCount = 0;

        foreach (string burialID in databaseResults)
        {
            GameObject headstoneGO = GameObject.Find(burialID);

            if (headstoneGO != null)
            {
                // Check if LogoButton0 exists (like the in-game query tool does)
                Transform logoButton0 = headstoneGO.transform.Find("LogoButton0");
                bool hasLogoButton = logoButton0 != null;
                
                if (hasLogoButton)
                {
                    foundCount++;
                    Debug.Log($"✅ Found GameObject: {burialID} (Has LogoButton0: {hasLogoButton})");
                }
                else
                {
                    missingCount++;
                    Debug.Log($"⚠️ GameObject exists but missing LogoButton0: {burialID}");
                }
            }
            else
            {
                missingCount++;
                Debug.Log($" Missing GameObject: {burialID}");
            }
        }

        Debug.Log($"=== ALL HEADSTONES TEST RESULTS ===");
        Debug.Log($"Total in Database: {totalInDatabase}");
        Debug.Log($"Found GameObjects with LogoButton0: {foundCount}");
        Debug.Log($"Missing GameObjects or LogoButton0: {missingCount}");
        Debug.Log($"Match Rate: {(foundCount * 100.0f / totalInDatabase):F1}%");

        debugInfo += $"=== ALL HEADSTONES TEST RESULTS ===\n";
        debugInfo += $"Total in Database: {totalInDatabase}\n";
        debugInfo += $"Found GameObjects with LogoButton0: {foundCount}\n";
        debugInfo += $"Missing GameObjects or LogoButton0: {missingCount}\n";
        debugInfo += $"Match Rate: {(foundCount * 100.0f / totalInDatabase):F1}%\n";

        hasRunValidation = true;
    }

    private void DisplayResults()
    {
        GUILayout.Space(10);
        GUILayout.Label("Validation Results:", EditorStyles.boldLabel);

        GUILayout.Label($"Total headstones in database: {databaseResults.Count}");
        GUILayout.Label($"Found GameObjects in scene: {foundGameObjects.Count}");
        GUILayout.Label($"Missing GameObjects: {missingGameObjects.Count}");

        float matchRate = databaseResults.Count > 0
            ? (foundGameObjects.Count * 100.0f / databaseResults.Count)
            : 0;
        GUILayout.Label($"Match Rate: {matchRate:F1}%");

        if (missingGameObjects.Count > 0)
        {
            GUILayout.Label("Missing GameObjects:", EditorStyles.boldLabel);
            foreach (string missingID in missingGameObjects)
            {
                GUILayout.Label($" {missingID}");
            }
        }
    }
}
