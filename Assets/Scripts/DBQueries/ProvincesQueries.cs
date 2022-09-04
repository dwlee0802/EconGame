using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;

public class ProvincesQueries
{
    /*
     * accesses the Province table in the DB
     * should output Debug message when changing DB data
     * 
     * functions:
     */

    public static ProvinceEntry GetProvince(string provinceID)
    {
        //establish DB connection---------------------
        IDbConnection dbConnection;
        string dbname = "GameDatabase.db";
        string currentPath = System.IO.Path.GetDirectoryName(Application.dataPath);
        currentPath = currentPath + "\\" + dbname;
        //Debug.Log("Database file path: " + currentPath);
        dbConnection = new SqliteConnection("URI=file:" + currentPath);
        dbConnection.Open();
        //--------------------------------------------


        //execute query
        IDbCommand newcmd;
        newcmd = dbConnection.CreateCommand();
        string str = "SELECT * FROM Provinces WHERE ID = (@personID)";
        newcmd.CommandText = str;

        var parameter = newcmd.CreateParameter();
        parameter.ParameterName = "@personID";
        parameter.Value = provinceID;
        newcmd.Parameters.Add(parameter);

        IDataReader rdr = newcmd.ExecuteReader();

        if (rdr.Read())
        {
            ProvinceEntry output = MakeProvinceEntry(rdr);
            dbConnection.Close();

            return output;
        }
        else
        {
            Debug.LogError("GetPerson ERROR");

            dbConnection.Close();

            return new ProvinceEntry("ERROR", "ERROR", "ERROR", 0);
        }

    }

    public static void SetGrowthPoints(string provinceID, int amount)
    {        
        //establish DB connection---------------------
        IDbConnection dbConnection;
        string dbname = "GameDatabase.db";
        string currentPath = System.IO.Path.GetDirectoryName(Application.dataPath);
        currentPath = currentPath + "\\" + dbname;
        //Debug.Log("Database file path: " + currentPath);
        dbConnection = new SqliteConnection("URI=file:" + currentPath);
        dbConnection.Open();
        //--------------------------------------------


        IDbCommand newcmd;
        newcmd = dbConnection.CreateCommand();
        string str = "UPDATE Provinces SET GrowthPoints = (@employerID) WHERE ID = (@personID)";
        newcmd.CommandText = str;

        var parameter1 = newcmd.CreateParameter();
        parameter1.ParameterName = "@employerID";
        parameter1.Value = amount;
        newcmd.Parameters.Add(parameter1);

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@personID";
        parameter2.Value = provinceID;
        newcmd.Parameters.Add(parameter2);

        newcmd.ExecuteNonQuery();

        //Debug.Log(string.Format("{0}'s employer was set to {1}", personID, buildingID));

        dbConnection.Close();
    }

    static ProvinceEntry MakeProvinceEntry(IDataReader rdr)
    {
        ProvinceEntry output = new ProvinceEntry(
                   rdr.GetString(((int)TableColumns.ProvinceColumns.ID)),
                   rdr.GetString(((int)TableColumns.ProvinceColumns.Name)),
                   rdr.GetString(((int)TableColumns.ProvinceColumns.Nation)),
                   rdr.GetInt32(((int)TableColumns.ProvinceColumns.GrowthPoints))
                   );

        return output;
    }
}
