using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class creates a save file in the persistent data path C:\Users\[USER]\AppData\LocalLow\DefaultCompany\DBManager from a copy of the 'base file' in the game's StreamingAssets.
/// Database files can be read and iterated through to quickly retrieve data through formatted SQLite queries.
/// </summary>

public class GameBehavior : MonoBehaviour {
    public string baseFileName = "TestDB.db";
    public string activeFileName = "TestDBSave.db";
    public DBAccessor dbTestData;
    string printResult = "";
    public UnityEngine.UI.Text textDisplay;

	// Use this for initialization
	void Start () {
        dbTestData = new DBAccessor(activeFileName, baseFileName, false);
        CheckForSaveData(dbTestData);
        LoadTestTable();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void CheckForSaveData(DBAccessor dba)
    {
        if (dba.SaveFileExists())
        {
            Debug.Log("CheckForSaveData() : File found at: " + dba.SaveFilepath + " exists.");
        }
        else {
            Debug.Log("Save Data not found. Initializing Save Data in proper location...");
            dba.InitializeSaveData();
        }
    }

    void LoadTestTable()
    {
        textDisplay.text = "";

        //Open connection... must be done whenever trying to access the database itself.
        dbTestData.OpenConnection();

        //Returns all table names found in the database in a List<string> for iteration purposes.
        List<string> tableNames = dbTestData.GetAllTableNames();
        printResult += "Tables found in the database : " + tableNames.Count.ToString() + "\n";

        dbTestData.CloseConnection();

        foreach(string table in tableNames)
        {
            dbTestData.OpenConnection();
            string itemName = "";
            string itemType = "";
            int itemValue = 0;
             
            //Custom class of DB packages are how I store the necessary information used to locate a specific address in a table.
            DBPackage cardQuery = new DBPackage(dbTestData.SaveFilepath, "USE UNIQUE FIELD ENTRY HERE", table);

            dbTestData.CloseConnection();

            printResult += table + ":\n";

            //Get the number of items in the table
            dbTestData.OpenConnection();
            int count = dbTestData.GetTableRowCount(cardQuery.table);
            if (count <= 0)
                Debug.LogWarning(cardQuery.table + " is empty!");
            else
                Debug.Log(count);
            dbTestData.CloseConnection();

            //Now that we have the count of items in the table, we can iterate through each item by its index.
            //Indexes in tables start at 1, not 0.
            for(int i = 1; i < count + 1; i++)
            {
                dbTestData.OpenConnection();
                cardQuery.field = "item_name";
                itemName = dbTestData.LoadStringAtRow(cardQuery, i); //retrieve the data in the row at index (i) (row), from the given field (column).

                cardQuery.field = "item_type";
                itemType = dbTestData.LoadStringAtRow(cardQuery, i);

                cardQuery.field = "value";
                itemValue = dbTestData.LoadIntAtRow(cardQuery, i); //LoadIntAtRow is only proven to work on KNOWN integers at this point.
                dbTestData.CloseConnection();
                printResult += string.Format(" - {0} : {1}         ...           [{2} gold]\n", itemName, itemType, itemValue.ToString());
            }

            printResult += "\n";
        }

        textDisplay.text = printResult;
    }
}
