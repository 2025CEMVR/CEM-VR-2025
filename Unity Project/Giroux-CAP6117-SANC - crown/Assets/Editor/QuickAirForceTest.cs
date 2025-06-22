using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SQLiteDatabase;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;

public class QuickAirForceTest
{
    [MenuItem("Tools/Quick Air Force Headstone Test")]
    public static void QuickTest()
    {
        Debug.Log("=== QUICK AIR FORCE HEADSTONE TEST ===");
        
        // Check if CemeteryAssetsFull scene is loaded
        Scene cemeteryScene = SceneManager.GetSceneByName("CemeteryAssetsFull");
        Debug.Log($"CemeteryAssetsFull scene loaded: {cemeteryScene.isLoaded}");
        Debug.Log($"CemeteryAssetsFull scene name: {cemeteryScene.name}");
        Debug.Log($"Active scene: {SceneManager.GetActiveScene().name}");
        
        if (!cemeteryScene.isLoaded)
        {
            Debug.LogWarning("CemeteryAssetsFull scene is not loaded. Attempting to load it...");
            
            // Try to load the scene additively using EditorSceneManager
            try
            {
                string scenePath = "Assets/Scenes/CemeteryAssetsFull.unity";
                var openedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                EditorSceneManager.SetActiveScene(openedScene);
                Debug.Log($"CemeteryAssetsFull scene loaded additively in Editor. Scene path: {openedScene.path}");
                Debug.Log("Scene loading completed. Running the test now...");
                
                // Run the test after scene is loaded
                RunAirForceTest();
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load CemeteryAssetsFull scene: {e.Message}");
                Debug.LogError("Please ensure the scene path is correct and scene is added to Build Settings.");
                return;
            }
        }
        
        // If scene is loaded, run the test
        RunAirForceTest();
    }
    
    private static void RunAirForceTest()
    {
        try
        {
            // Test with a known headstone first
            GameObject testObj = GameObject.Find("A-18-B-F");
            Debug.Log($"Test object A-18-B-F found: {testObj != null}");
            
            if (testObj == null)
            {
                Debug.LogError("Could not find test headstone A-18-B-F. Scene may not be properly loaded.");
                return;
            }
            
            // Initialize database
            SQLiteDB db = SQLiteDB.Instance;
            db.DBLocation = Application.persistentDataPath;
            db.DBName = "cemVR.db";
            db.ConnectToDefaultDatabase(db.DBName, true);
            
            Debug.Log("Database connected successfully");
            
            // First, check if there are any Air Force records at all
            string countQuery = "SELECT COUNT(*) as count FROM cemVRburials WHERE brnAF = '1'";
            Debug.Log($"Executing count query: {countQuery}");
            
            DBReader countReader = db.Select(countQuery);
            if (countReader != null && countReader.Read())
            {
                int airForceCount = countReader.GetIntValue("count");
                Debug.Log($"Total Air Force records in database: {airForceCount}");
                
                if (airForceCount == 0)
                {
                    Debug.LogWarning("No Air Force records found in database! Testing with all headstones instead.");
                    TestWithAllHeadstones(db);
                    return;
                }
            }
            
            // Query for Air Force headstones
            string query = "SELECT burialID, firstName, lastName, section, site FROM cemVRburials WHERE brnAF = '1' AND markerFace = 'F'";
            Debug.Log($"Executing query: {query}");
            
            DBReader reader = db.Select(query);
            
            if (reader == null)
            {
                Debug.LogError("Query returned null reader!");
                return;
            }
            
            List<string> databaseResults = new List<string>();
            List<string> foundGameObjects = new List<string>();
            List<string> missingGameObjects = new List<string>();
            List<GameObjectLocation> foundLocations = new List<GameObjectLocation>();
            Dictionary<string, string> burialIDToName = new Dictionary<string, string>();
            
            int totalInDatabase = 0;
            
            while (reader != null && reader.Read())
            {
                string burialID = reader.GetStringValue("burialID");
                string firstName = reader.GetStringValue("firstName");
                string lastName = reader.GetStringValue("lastName");
                string section = reader.GetStringValue("section");
                string site = reader.GetStringValue("site");
                
                databaseResults.Add(burialID);
                burialIDToName[burialID] = $"{firstName} {lastName}";
                totalInDatabase++;
                
                Debug.Log($"Database Entry: {burialID} - {firstName} {lastName} ({section}-{site})");
            }
            
            Debug.Log($"Total Air Force headstones in database: {totalInDatabase}");
            
            // Check GameObjects in scene with detailed location tracking
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
                        
                        // Get person name
                        string personName = burialIDToName.ContainsKey(burialID) ? burialIDToName[burialID] : "Unknown";
                        
                        // Get detailed location information
                        GameObjectLocation location = new GameObjectLocation
                        {
                            burialID = burialID,
                            sceneName = headstoneGO.scene.name,
                            position = headstoneGO.transform.position,
                            parentName = headstoneGO.transform.parent != null ? headstoneGO.transform.parent.name : "None",
                            hierarchyPath = GetGameObjectPath(headstoneGO),
                            isActive = headstoneGO.activeInHierarchy,
                            layer = LayerMask.LayerToName(headstoneGO.layer)
                        };
                        foundLocations.Add(location);
                        
                        Debug.Log($"✅ Found GameObject: {burialID} - {personName} (Has LogoButton0: {hasLogoButton})");
                        Debug.Log($"   Scene: {location.sceneName}");
                        Debug.Log($"   Position: {location.position}");
                        Debug.Log($"   Parent: {location.parentName}");
                        Debug.Log($"   Path: {location.hierarchyPath}");
                        Debug.Log($"   Active: {location.isActive}");
                        Debug.Log($"   Layer: {location.layer}");
                    }
                    else
                    {
                        missingGameObjects.Add(burialID);
                        missingCount++;
                        string personName = burialIDToName.ContainsKey(burialID) ? burialIDToName[burialID] : "Unknown";
                        Debug.Log($"⚠️ GameObject exists but missing LogoButton0: {burialID} - {personName}");
                    }
                }
                else
                {
                    missingGameObjects.Add(burialID);
                    missingCount++;
                    string personName = burialIDToName.ContainsKey(burialID) ? burialIDToName[burialID] : "Unknown";
                    Debug.Log($" Missing GameObject: {burialID} - {personName}");
                }
            }
            
            // Display summary
            Debug.Log($"=== TEST RESULTS ===");
            Debug.Log($"Total in Database: {totalInDatabase}");
            Debug.Log($"Found GameObjects with LogoButton0: {foundCount}");
            Debug.Log($"Missing GameObjects or LogoButton0: {missingCount}");
            Debug.Log($"Match Rate: {(foundCount * 100.0f / totalInDatabase):F1}%");
            
            if (missingGameObjects.Count > 0)
            {
                Debug.Log("Missing GameObjects or LogoButton0:");
                foreach (string missingID in missingGameObjects)
                {
                    string personName = burialIDToName.ContainsKey(missingID) ? burialIDToName[missingID] : "Unknown";
                    Debug.Log($"   {missingID} - {personName}");
                }
            }
            
            // Export results to text file
            ExportResultsToFile(databaseResults, foundLocations, missingGameObjects, burialIDToName, "Editor_AirForce_Test");
            
            // Clean up
            db.Dispose();
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Test failed with error: {e.Message}");
        }
    }
    
    private static void TestWithAllHeadstones(SQLiteDB db)
    {
        Debug.Log("=== TESTING WITH ALL HEADSTONES ===");
        
        string query = "SELECT burialID, firstName, lastName FROM cemVRburials WHERE markerFace = 'F' LIMIT 10";
        Debug.Log($"Executing query: {query}");
        
        DBReader reader = db.Select(query);
        
        if (reader == null)
        {
            Debug.LogError("Query returned null reader!");
            return;
        }
        
        List<string> databaseResults = new List<string>();
        List<GameObjectLocation> foundLocations = new List<GameObjectLocation>();
        List<string> missingGameObjects = new List<string>();
        Dictionary<string, string> burialIDToName = new Dictionary<string, string>();
        int totalInDatabase = 0;
        
        while (reader != null && reader.Read())
        {
            string burialID = reader.GetStringValue("burialID");
            string firstName = reader.GetStringValue("firstName");
            string lastName = reader.GetStringValue("lastName");
            
            databaseResults.Add(burialID);
            burialIDToName[burialID] = $"{firstName} {lastName}";
            totalInDatabase++;
            Debug.Log($"Database Entry: {burialID} - {firstName} {lastName}");
        }
        
        Debug.Log($"Total headstones in database (first 10): {totalInDatabase}");
        
        // Check GameObjects in scene
        int foundCount = 0;
        int missingCount = 0;
        
        foreach (string burialID in databaseResults)
        {
            GameObject headstoneGO = GameObject.Find(burialID);
            
            if (headstoneGO != null)
            {
                foundCount++;
                
                string personName = burialIDToName.ContainsKey(burialID) ? burialIDToName[burialID] : "Unknown";
                
                GameObjectLocation location = new GameObjectLocation
                {
                    burialID = burialID,
                    sceneName = headstoneGO.scene.name,
                    position = headstoneGO.transform.position,
                    parentName = headstoneGO.transform.parent != null ? headstoneGO.transform.parent.name : "None",
                    hierarchyPath = GetGameObjectPath(headstoneGO),
                    isActive = headstoneGO.activeInHierarchy,
                    layer = LayerMask.LayerToName(headstoneGO.layer)
                };
                foundLocations.Add(location);
                
                Debug.Log($"✅ Found GameObject: {burialID} - {personName}");
                Debug.Log($"   Scene: {location.sceneName}");
                Debug.Log($"   Position: {location.position}");
                Debug.Log($"   Parent: {location.parentName}");
                Debug.Log($"   Path: {location.hierarchyPath}");
            }
            else
            {
                missingGameObjects.Add(burialID);
                missingCount++;
                string personName = burialIDToName.ContainsKey(burialID) ? burialIDToName[burialID] : "Unknown";
                Debug.Log($" Missing GameObject: {burialID} - {personName}");
            }
        }
        
        Debug.Log($"=== ALL HEADSTONES TEST RESULTS ===");
        Debug.Log($"Total in Database: {totalInDatabase}");
        Debug.Log($"Found GameObjects: {foundCount}");
        Debug.Log($"Missing GameObjects: {missingCount}");
        Debug.Log($"Match Rate: {(foundCount * 100.0f / totalInDatabase):F1}%");
        
        // Export results to text file
        ExportResultsToFile(databaseResults, foundLocations, missingGameObjects, burialIDToName, "Editor_AllHeadstones_Test");
    }
    
    private static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    private static void ExportResultsToFile(
    List<string> databaseResults,
    List<GameObjectLocation> foundLocations,
    List<string> missingGameObjects,
    Dictionary<string, string> burialIDToName,
    string fileName)
    {
        try
        {
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileNameWithTimestamp = $"{fileName}_{timestamp}.txt";
            string filePath = Path.Combine(Application.dataPath, "..", "Debug_Exports", fileNameWithTimestamp);
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"=== AIR FORCE HEADSTONE TEST RESULTS ===");
                writer.WriteLine($"Timestamp: {System.DateTime.Now}");
                writer.WriteLine($"Total Database Records: {databaseResults.Count}");
                writer.WriteLine($"Found GameObjects with LogoButton0: {foundLocations.Count}");
                writer.WriteLine($"Missing GameObjects or LogoButton0: {missingGameObjects.Count}");
                writer.WriteLine($"Match Rate: {(foundLocations.Count * 100.0f / databaseResults.Count):F1}%");
                writer.WriteLine();
                
                writer.WriteLine("=== FOUND GAMEOBJECTS (with LogoButton0) ===");
                foreach (var location in foundLocations)
                {
                    string personName = burialIDToName.ContainsKey(location.burialID) ? burialIDToName[location.burialID] : "Unknown";
                    writer.WriteLine($"BurialID: {location.burialID} - {personName}");
                    writer.WriteLine($"  Scene: {location.sceneName}");
                    writer.WriteLine($"  Position: {location.position}");
                    writer.WriteLine($"  Parent: {location.parentName}");
                    writer.WriteLine($"  Hierarchy Path: {location.hierarchyPath}");
                    writer.WriteLine($"  Active: {location.isActive}");
                    writer.WriteLine($"  Layer: {location.layer}");
                    writer.WriteLine();
                }
                
                writer.WriteLine("=== MISSING GAMEOBJECTS OR LOGOBUTTON0 ===");
                foreach (string missingID in missingGameObjects)
                {
                    string personName = burialIDToName.ContainsKey(missingID) ? burialIDToName[missingID] : "Unknown";
                    writer.WriteLine($" {missingID} - {personName}");
                }
                
                writer.WriteLine();
                writer.WriteLine("=== DATABASE RECORDS ===");
                foreach (string burialID in databaseResults)
                {
                    string personName = burialIDToName.ContainsKey(burialID) ? burialIDToName[burialID] : "Unknown";
                    writer.WriteLine($"Database: {burialID} - {personName}");
                }
            }
            
            Debug.Log($"Results exported to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export results: {e.Message}");
        }
    }

}
