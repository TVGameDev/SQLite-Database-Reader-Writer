///This is part of an ongoing effort to create a SIMPLE and EASY version of a SQLite database reader.
///Certain edge case bugs may still be present, make sure to proofread your database and check that
///the values of your table match up with the script that's reading it.



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.Text;
using Mono.Data.SqliteClient;
using System.IO;
using System;


public class DBAccessor
{

    public UnityEngine.UI.Text consoleText;
    public bool debugEnabled;
    private string saveFilepath;
    private string saveDataFilename;
    private string saveDataDefaultFilename;
    private IDbConnection dbConnection;
    private IDbCommand dbCommand;
    private IDataReader dbReader;

    public DBAccessor(string SaveFileName, string SaveDefaultsName, bool debug)
    {
        saveDataFilename = SaveFileName;
        saveDataDefaultFilename = SaveDefaultsName;
        saveFilepath = string.Format("{0}/{1}", Application.persistentDataPath, saveDataFilename);
        debugEnabled = debug;
    }

    /// <summary>
    /// The path to the active save file.
    /// </summary>
    public string SaveFilepath
    {
        get
        {
            return saveFilepath;
        }
    }

    /// <summary>
    /// Opens a connection to the Database. DO NOT FORGET TO CLOSE IT WHEN FINISHING AN OPERATION.
    /// </summary>
    public void OpenConnection()
    {
        ToConsole("OpenDB()");
        string connection;
        connection = "URI=file:" + saveFilepath;

        ToConsole("OpenDB() : Attempting to open connection...");
        try
        {
            dbConnection = new SqliteConnection(connection);
        }
        catch (Exception e)
        {
            ToConsole("OpenDB(): Tried Connection with " + connection + " but recieved error: " + e.Message);
        }

        dbConnection.Open();
        ToConsole("OpenDB(): Database State: " + dbConnection.State.ToString());

    }


    public bool SaveFileExists()
    {
        if (File.Exists(saveFilepath))
        {
            return true;
        }
        else {
            return false;
        }
    }


    /// <summary>
    /// Checks to see if a live savefile exists with the given name inside the standard Application.persistentDataPath.
    /// If it does, it will be replaced with a new instance created from the default file located in the Application.streamingAssetsPath
    /// </summary>
    public void InitializeSaveData()
    {
        ToConsole("InitializeSaveData()");
        if (File.Exists(saveFilepath))
        {
            ToConsole("Attempting to overwrite file at " + saveFilepath + "with file from " + Application.streamingAssetsPath + "/" + saveDataDefaultFilename);
            File.Delete(saveFilepath);
            if (File.Exists(saveFilepath))
            {
                ToConsole("File Deletion Failed");
            }
            if (!File.Exists(saveFilepath))
            {
                ToConsole("File Deletion Succeeeded. No file exists at " + saveFilepath);
            }
        }
        else if (!File.Exists(saveFilepath))
        {
            ToConsole("File does not exist at " + saveFilepath);
        }

        ToConsole("Attempting to Stream Database...");
#if UNITY_EDITOR
        string streamAssetDBPath = "file://" + Application.streamingAssetsPath + "/" + saveDataDefaultFilename;

        ToConsole("attempting load from WWW( " + streamAssetDBPath);
        WWW loadDB = new WWW(streamAssetDBPath);
#endif
#if UNITY_ANDROID
#if !UNITY_EDITOR
		string streamAssetDBPath = Application.streamingAssetsPath + "/" + saveDataDefaultFilename;
		WWW loadDB = new WWW(streamAssetDBPath);
		ToConsole("attempting load from WWW " + streamAssetDBPath);
#endif
#endif
        try
        {
            while (!loadDB.isDone) { }  // CAREFUL with this loop. If you are having trouble set a timeout on it.
        }
        catch (Exception e)
        {
            ToConsole("Something wrong with loop : " + e.Message);
        }

        if (loadDB.bytesDownloaded <= 0)
        {
            ToConsole("No bytes retrieved from WWW loadDB from " + streamAssetDBPath);
        }
        else {
            ToConsole("Data successfully retrieved from" + streamAssetDBPath);
        }

        // then save to Application.persistentDataPath

        ToConsole("Copying from StreamingAssets Default DB to location on persistentDataPath");
        try
        {
            File.WriteAllBytes(saveFilepath, loadDB.bytes);
        }
        catch (Exception e)
        {
            ToConsole("While writing all bytes to the chosen filepath, encountered : " + e.Message);
        }

        ToConsole("File copy attempted. Checking if it exists...");

        if (File.Exists(saveFilepath))
        {
            ToConsole("File exists at " + saveFilepath);
            if (loadDB.bytesDownloaded <= 0)
            {
                ToConsole("However, the file is probably empty as WWW loadDB streamed from " + streamAssetDBPath + "returned no bytes");
            }
            ToConsole("---");
        }
        if (!File.Exists(saveFilepath))
        {
            ToConsole("File Initialization failed. No file exists at " + saveFilepath);
        }
    }
    /// <summary>
    /// Close the Connection to the Database.
    /// </summary>
    public void CloseConnection()
    {
        ToConsole("CloseConnection()");
        ToConsole("Attempting to close DB Connection...");
        try
        {
            if (dbReader != null)
            {
                dbReader.Close();
                ToConsole("dbReader Closed : " + dbReader.IsClosed.ToString());
                dbReader = null;
                if (dbReader == null)
                {
                    ToConsole("dbReader Null : TRUE");
                }
            }
        }
        catch (Exception e)
        {
            ToConsole("Close Connection() : While attempting to close/null dbReader caught : " + e.Message);
        }
        try
        {
            if (dbCommand != null)
            {
                dbCommand.Dispose();
                ToConsole("dbReader Disposed");
                dbCommand = null;
                if (dbCommand == null)
                {
                    ToConsole("dbCommand Null : TRUE");
                }
            }
        }
        catch (Exception e)
        {
            ToConsole("Close Connection() : While attempting to close/null dbCommand caught : " + e.Message);
        }
        try
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
                if (dbConnection.State == ConnectionState.Closed)
                {
                    dbConnection = null;
                    if (dbConnection == null)
                    {
                        ToConsole("dbConnection Null : TRUE");
                    }
                    ToConsole("Connection Closed Successfully.");
                }
                else {
                    ToConsole("Closing Failed, state: " + dbConnection.State.ToString());
                }
            }
        }
        catch (Exception e)
        {
            ToConsole("Close Connection() : While attempting to close/null dbConnection caught : " + e.Message);
        }
    }

    //TODO: Parse for newline and other special characters. passing in special characters doesn't appear to translate properly from SQL

    public int GetTableRowCount(string tableName)
    {
        int count = 0;
        OpenConnection();
        using (dbCommand = dbConnection.CreateCommand())
        {
            dbCommand.CommandText = "SELECT COUNT(*) FROM " + tableName;
            count = Convert.ToInt32(dbCommand.ExecuteScalar());
            CloseConnection();
            return count;
        }
    }

    public List<string> GetAllTableNames()
    {
        OpenConnection();
        List<string> tableNames = new List<string>();
        using (dbCommand = dbConnection.CreateCommand())
        {
            dbCommand.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";//WHERE type = 'table' // ORDER BY 1 
            dbReader = dbCommand.ExecuteReader(CommandBehavior.KeyInfo);
            while (dbReader.Read())
            {
                try
                {
                    tableNames.Add(dbReader.GetString(0));                
                }
                catch(Exception e)
                {
                    Debug.LogError(e.Message);
                    return tableNames;
                }
            }
            CloseConnection();
            return tableNames;
        }
    }

    /// <summary>
    /// Returns a string entered in a .db file, located at a rowID of 'index'.
    /// </summary>
    /// <param name="package"> Contains Table and Field to inspect</param>
    /// <param name="rowIndex">Entry to read</param>
    /// <returns></returns>
    public string LoadStringAtRow(DBPackage package, int rowIndex)
    {
        using (dbCommand = dbConnection.CreateCommand())
        {
            string query = string.Format("SELECT {0} FROM {1} WHERE rowID = '{2}'", package.field, package.table, rowIndex.ToString());
            dbCommand.CommandText = query;
            dbReader = dbCommand.ExecuteReader();
            while (dbReader.Read())
            {
                try
                {
                    //string returnValue = dbReader.GetString(0);
                    string returnValue = dbReader.GetValueOrDefault<string>(package.field);
                    /*if(returnValue == null)
                    {
                        Debug.Log("Null recieved");
                    }*/
                   /* if(returnValue.ToUpper() == "NULLENTRY")
                    {
                        //Debug.Log("Null properly received");
                        return null;
                    }*/
                    return returnValue;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                    ToConsole(e.Message);
                    return null;
                }
            }
            Debug.LogWarning("string not returned from query [" + query +"] Returning default of null");
            return null;
        }
    }

    /// <summary>
    /// Returns an integer entered in a .db file, located at a rowID of 'index'.
    /// ONLY execute on values you know are integers, has not been tested with non integer values.
    /// </summary>
    /// <param name="package"> Contains Table and Field to inspect</param>
    /// <param name="rowIndex">Entry to read</param>
    /// <returns></returns>
    public int LoadIntAtRow(DBPackage package, int rowIndex)
    {
        using (dbCommand = dbConnection.CreateCommand())
        {
            string query = string.Format("SELECT {0} FROM {1} WHERE rowID = '{2}'", package.field, package.table, rowIndex.ToString());
            dbCommand.CommandText = query;
            dbReader = dbCommand.ExecuteReader();
            while (dbReader.Read())
            {
                try
                {
                    int returnValue = dbReader.GetInt32(0);
                    return returnValue;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                    ToConsole(e.Message);
                    return 0;
                }
            }
            Debug.LogWarning("int not returned! Returning default 0");
            return 0;
        }
    }

    public void WriteValueToDatabaseAtRow (DBPackage package, int rowIndex, int newValue)
    {
        using (dbCommand = dbConnection.CreateCommand())
        {
            string query = string.Format("UPDATE {0} SET {1} = '{2}' WHERE rowID = '{3}'", package.table, package.field, newValue.ToString(), rowIndex.ToString());
            dbCommand.CommandText = query;
            dbReader = dbCommand.ExecuteReader();
        }
    }

    public void WriteValueToDatabaseAtRow(DBPackage package, int rowIndex, float newValue)
    {
        using (dbCommand = dbConnection.CreateCommand())
        {
            string query = string.Format("UPDATE {0} SET {1} = '{2}' WHERE rowID = '{3}'", package.table, package.field, newValue.ToString(), rowIndex.ToString());
            dbCommand.CommandText = query;
            dbReader = dbCommand.ExecuteReader();
        }
    }

    public void WriteValueToDatabaseAtRow(DBPackage package, int rowIndex, string newValue)
    {
        using (dbCommand = dbConnection.CreateCommand())
        {
            string query = string.Format("UPDATE {0} SET {1} = '{2}' WHERE rowID = '{3}'", package.table, package.field, newValue, rowIndex.ToString());
            dbCommand.CommandText = query;
            dbReader = dbCommand.ExecuteReader();
        }
    }

    public void ToConsole(string text)
    {
        if (debugEnabled)
        {
            if(consoleText != null)
                consoleText.text += "\n\n" + text;
            else
            {
                Debug.Log(text);
            }
        }
    }
}

public struct DBPackage
{
    public string databasePath;
    public string field;
    public string table;

    public DBPackage(string DatabasePath, string Field, string Table)
    {
        this.databasePath = DatabasePath;
        this.field = Field;
        this.table = Table;
    }
}

/// <summary>
/// 
/// </summary>
public static class NullSafeGetter
{
    // refer to http://stackoverflow.com/questions/2609875/null-safe-way-to-get-values-from-an-idatareader

    public static T GetValueOrDefault<T>(this IDataRecord  row, string fieldName)
    {
        int ordinal = row.GetOrdinal(fieldName);
        return row.GetValueOrDefault<T>(ordinal);
    }

    public static T GetValueOrDefault<T>(this IDataRecord row, int ordinal)
    {
        return (T)(row.IsDBNull(ordinal) ? default(T) : row.GetValue(ordinal));
    }
}

