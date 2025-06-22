using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;
using SQLiteDatabase;

/// <summary>
/// Unit tests for validating burialID format in the cemetery database.
/// Tests ensure all burialID's follow the required pattern: 1-2 letters, -, number 0-999, -, 1 letter, with optional additional -letter segments.
/// </summary>
public class BurialIDValidationTests
{
    private SQLiteDB db;
    private List<string> invalidBurialIDs;
    private List<string> validBurialIDs;

    [SetUp]
    public void SetUp()
    {
        // Initialize database connection
        db = SQLiteDB.Instance;
        db.DBLocation = Application.streamingAssetsPath;
        db.DBName = "cemVR.db";
        db.ConnectToDefaultDatabase(db.DBName, false);
        
        invalidBurialIDs = new List<string>();
        validBurialIDs = new List<string>();
    }

    [TearDown]
    public void TearDown()
    {
        if (db != null)
        {
            db.Dispose();
        }
    }

    /// <summary>
    /// Test to verify database connection and table existence.
    /// </summary>
    [Test]
    public void DatabaseConnection_ShouldBeValid()
    {
        Debug.Log("=== DATABASE CONNECTION TEST ===");
        Debug.Log($"Database Location: {db.DBLocation}");
        Debug.Log($"Database Name: {db.DBName}");
        Debug.Log($"Database Exists: {db.Exists}");
        
        // Test if database exists
        Assert.IsTrue(db.Exists, "Database should exist at the specified location");
        
        // Test if we can query the table
        string testQuery = "SELECT COUNT(*) as count FROM cemVRburials";
        Debug.Log($"Testing query: {testQuery}");
        
        DBReader reader = db.Select(testQuery);
        Assert.IsNotNull(reader, "Database query should return a valid reader");
        
        if (reader.Read())
        {
            int count = reader.GetIntValue("count");
            Debug.Log($"Total records in cemVRburials table: {count}");
            Assert.Greater(count, 0, "cemVRburials table should contain at least one record");
        }
        else
        {
            Assert.Fail("Could not read count from cemVRburials table");
        }
    }

    /// <summary>
    /// Main test: Validates that all burialID's in the database follow the correct format.
    /// Pattern: ^[A-Z]{1,2}-\d{1,3}-[A-Z](?:-[A-Z])*$
    /// Requirements:
    /// - 1-2 letters at start
    /// - Dash separator
    /// - Number 0-999 (1-3 digits)
    /// - Dash separator  
    /// - 1 letter
    /// - Optional additional -letter segments
    /// </summary>
    [Test]
    public void AllBurialIDs_ShouldFollowCorrectFormat()
    {
        // Define the regex pattern for valid burialID format
        string pattern = @"^[A-Z]{1,2}-\d{1,3}-[A-Z](?:-[A-Z])*$";
        Regex regex = new Regex(pattern);
        
        // Debug database connection
        Debug.Log($"Database Location: {db.DBLocation}");
        Debug.Log($"Database Name: {db.DBName}");
        Debug.Log($"Database Exists: {db.Exists}");
        
        // Query all burialID's from the database
        string query = "SELECT burialID FROM cemVRburials";
        Debug.Log($"Executing Query: {query}");
        
        DBReader reader = db.Select(query);
        
        // Debug reader status
        if (reader == null)
        {
            Debug.LogError("DBReader is null - query failed!");
            Assert.Fail("Database query returned null reader");
            return;
        }
        
        Debug.Log($"DBReader created successfully");
        
        int totalCount = 0;
        int validCount = 0;
        int invalidCount = 0;
        
        Debug.Log("=== BURIAL ID VALIDATION TEST ===");
        
        while (reader != null && reader.Read())
        {
            totalCount++;
            string burialID = reader.GetStringValue("burialID");
            Debug.Log($"Processing burialID #{totalCount}: {burialID}");
            
            if (regex.IsMatch(burialID))
            {
                validCount++;
                validBurialIDs.Add(burialID);
            }
            else
            {
                invalidCount++;
                invalidBurialIDs.Add(burialID);
                Debug.LogWarning($"Invalid burialID format: {burialID}");
            }
        }
        
        // Log results
        Debug.Log($"Total burialID's tested: {totalCount}");
        Debug.Log($"Valid burialID's: {validCount}");
        Debug.Log($"Invalid burialID's: {invalidCount}");
        
        if (invalidCount > 0)
        {
            Debug.LogError("Invalid burialID's found:");
            foreach (string invalidID in invalidBurialIDs)
            {
                Debug.LogError($"  - {invalidID}");
            }
        }
        
        // Assert that all burialID's are valid
        Assert.AreEqual(0, invalidCount, $"Found {invalidCount} invalid burialID's. All burialID's must follow the pattern: 1-2 letters, -, number 0-999, -, 1 letter, with optional additional -letter segments.");
    }

    /// <summary>
    /// Test specific valid burialID patterns to ensure the regex works correctly.
    /// </summary>
    [Test]
    public void ValidBurialIDPatterns_ShouldMatch()
    {
        string pattern = @"^[A-Z]{1,2}-\d{1,3}-[A-Z](?:-[A-Z])*$";
        Regex regex = new Regex(pattern);
        
        // Test valid patterns
        string[] validPatterns = {
            "A-1-F",           // Basic pattern: 1 letter, 1 digit, 1 letter
            "AB-123-F",        // 2 letters, 3 digits, 1 letter
            "A-999-B",         // 1 letter, 3 digits, 1 letter
            "MA-1-B-F",        // 2 letters, 1 digit, 1 letter, optional -letter
            "A-50-C-D",        // 1 letter, 2 digits, 1 letter, optional -letter
            "XY-0-Z",          // 2 letters, 1 digit (0), 1 letter
            "A-1-F-G-H"        // Multiple optional -letter segments
        };
        
        foreach (string testID in validPatterns)
        {
            Assert.IsTrue(regex.IsMatch(testID), $"Valid burialID '{testID}' should match the pattern");
        }
    }

    /// <summary>
    /// Test specific invalid burialID patterns to ensure the regex correctly rejects them.
    /// </summary>
    [Test]
    public void InvalidBurialIDPatterns_ShouldNotMatch()
    {
        string pattern = @"^[A-Z]{1,2}-\d{1,3}-[A-Z](?:-[A-Z])*$";
        Regex regex = new Regex(pattern);
        
        // Test invalid patterns
        string[] invalidPatterns = {
            "A1-F",            // Missing dash after letters
            "A-1F",            // Missing dash before last letter
            "ABC-1-F",         // Too many letters at start (3 instead of 1-2)
            "A-1000-F",        // Too many digits (4 instead of 1-3)
            "a-1-F",           // Lowercase letters
            "A-1-f",           // Lowercase letter at end
            "A--1-F",          // Double dash
            "A-1--F",          // Double dash
            "A-1-F-",          // Trailing dash
            "-A-1-F",          // Leading dash
            "A-1",             // Missing last letter
            "1-A-F",           // Number at start instead of letters
            "A-1-F-123",       // Numbers in optional segment
            "A-1-F-G-",        // Trailing dash in optional segment
            ""                 // Empty string
        };
        
        foreach (string testID in invalidPatterns)
        {
            Assert.IsFalse(regex.IsMatch(testID), $"Invalid burialID '{testID}' should not match the pattern");
        }
    }

    /// <summary>
    /// Test to ensure no burialID's are null or empty in the database.
    /// </summary>
    [Test]
    public void AllBurialIDs_ShouldNotBeNullOrEmpty()
    {
        string query = "SELECT burialID FROM cemVRburials";
        DBReader reader = db.Select(query);
        
        int nullOrEmptyCount = 0;
        List<string> nullOrEmptyIDs = new List<string>();
        
        while (reader != null && reader.Read())
        {
            string burialID = reader.GetStringValue("burialID");
            
            if (string.IsNullOrEmpty(burialID))
            {
                nullOrEmptyCount++;
                nullOrEmptyIDs.Add(burialID ?? "NULL");
            }
        }
        
        if (nullOrEmptyCount > 0)
        {
            Debug.LogError($"Found {nullOrEmptyCount} null or empty burialID's:");
            foreach (string id in nullOrEmptyIDs)
            {
                Debug.LogError($"  - {id}");
            }
        }
        
        Assert.AreEqual(0, nullOrEmptyCount, "All burialID's should have valid values");
    }
} 