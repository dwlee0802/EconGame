using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;

public class BuildingsQueries
{
    /*
     * accesses the Buildings table in the DB
     * should output Debug message when changing DB data
     * 
     * functions:
     * getAllBuildings input string provinceID output List<BuildingEntry>
     * getBuilding input string personID output BuildingEntry
     * addBuilding input string provinceID (and optional parameters) output none
     * changeLevel input string buildingID float amount output none
     * changeBudget input string buildingID float amount output none
     */

    public static List<BuildingEntry> GetAllBuildings(string provinceID)
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
        string str = "SELECT * FROM Buildings WHERE Province = (@provinceID)";
        newcmd.CommandText = str;

        var parameter = newcmd.CreateParameter();
        parameter.ParameterName = "@provinceID";
        parameter.Value = provinceID;
        newcmd.Parameters.Add(parameter);

        IDataReader rdr = newcmd.ExecuteReader();

        List<BuildingEntry> outputList = new List<BuildingEntry>();

        while (rdr.Read())
        {
            BuildingEntry building = new BuildingEntry(
                rdr.GetString(((int)TableColumns.BuildingColumns.ID)),
                rdr.GetString(((int)TableColumns.BuildingColumns.Province)),
                rdr.GetInt32(((int)TableColumns.BuildingColumns.Type)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.Budget)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.Wage)),
                rdr.GetInt32(((int)TableColumns.BuildingColumns.IngredientStockpile)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.Premium)),
                rdr.GetInt32(((int)TableColumns.BuildingColumns.Level)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.ProductionStockpile)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.AverageIngredientCost))
                );

            outputList.Add(building);
        }

        dbConnection.Close();

        return outputList;
    }

    public static BuildingEntry GetBuilding(string buildingID)
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
        string str = "SELECT * FROM Buildings WHERE ID = (@buildingID)";
        newcmd.CommandText = str;

        var parameter = newcmd.CreateParameter();
        parameter.ParameterName = "@buildingID";
        parameter.Value = buildingID;
        newcmd.Parameters.Add(parameter);

        IDataReader rdr = newcmd.ExecuteReader();

        //generate entry
        if (rdr.Read())
        {
            BuildingEntry building = new BuildingEntry(
                rdr.GetString(((int)TableColumns.BuildingColumns.ID)),
                rdr.GetString(((int)TableColumns.BuildingColumns.Province)),
                rdr.GetInt32(((int)TableColumns.BuildingColumns.Type)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.Budget)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.Wage)),
                rdr.GetInt32(((int)TableColumns.BuildingColumns.IngredientStockpile)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.Premium)),
                rdr.GetInt32(((int)TableColumns.BuildingColumns.Level)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.ProductionStockpile)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.AverageIngredientCost))
            );

            dbConnection.Close();

            return building;
        }
        else
        {
            Debug.LogError("GetBuilding ERROR");

            BuildingEntry error = new BuildingEntry("ERROR", "ERROR", -1);

            dbConnection.Close();

            return error;
        }

    }

    public static void ChangeMoney(string buildingID, float byAmount)
    {
        if(byAmount == 0)
        {
            return;
        }
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
        string str = "UPDATE Buildings SET Budget = Budget + (@money) WHERE ID = (@buildingID)";
        newcmd.CommandText = str;

        var parameter1 = newcmd.CreateParameter();
        parameter1.ParameterName = "@money";
        parameter1.Value = byAmount;
        newcmd.Parameters.Add(parameter1);

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@buildingID";
        parameter2.Value = buildingID;
        newcmd.Parameters.Add(parameter2);

        newcmd.ExecuteNonQuery();

        dbConnection.Close();

        Debug.Log(string.Format("{0}'s budget was changed by {1}", buildingID, byAmount));
    }

    public static void ChangeIngredientStock(string buildingID, float byAmount)
    {
        if(byAmount == 0)
        {
            return;
        }

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
        string str = "UPDATE Buildings SET IngredientStockpile = IngredientStockpile + (@amount) WHERE ID = (@buildingID)";
        newcmd.CommandText = str;

        var parameter1 = newcmd.CreateParameter();
        parameter1.ParameterName = "@amount";
        parameter1.Value = byAmount;
        newcmd.Parameters.Add(parameter1);

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@buildingID";
        parameter2.Value = buildingID;
        newcmd.Parameters.Add(parameter2);

        newcmd.ExecuteNonQuery();
        dbConnection.Close();

        Debug.Log(string.Format("{0}'s ingredient stockpile was changed by {1}", buildingID, byAmount));
    }

    public static void ChangeAverageIngredientCost(string buildingID, float byAmount)
    {
        if (byAmount == 0)
        {
            return;
        }

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
        string str = "UPDATE Buildings SET AverageIngredientCost = (AverageIngredientCost * 6 + (@amount)) / 7 WHERE ID = (@buildingID)";
        newcmd.CommandText = str;

        var parameter1 = newcmd.CreateParameter();
        parameter1.ParameterName = "@amount";
        parameter1.Value = byAmount;
        newcmd.Parameters.Add(parameter1);

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@buildingID";
        parameter2.Value = buildingID;
        newcmd.Parameters.Add(parameter2);

        newcmd.ExecuteNonQuery();

        string str2 = "UPDATE Buildings SET AverageIngredientCost = ROUND(AverageIngredientCost, 2) WHERE ID = (@buildingID)";

        var parameter3 = newcmd.CreateParameter();
        parameter2.ParameterName = "@buildingID";
        parameter2.Value = buildingID;
        newcmd.Parameters.Add(parameter3);

        newcmd.CommandText = str2;
        newcmd.ExecuteNonQuery();

        dbConnection.Close();

        Debug.Log(string.Format("{0}'s average ingredient cost was changed by {1}", buildingID, byAmount));
    }

    public static void SetProductionStockpile(string buildingID, float amount)
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
        string str = "UPDATE Buildings SET ProductionStockpile = (@amount) WHERE ID = (@buildingID)";
        newcmd.CommandText = str;

        var parameter1 = newcmd.CreateParameter();
        parameter1.ParameterName = "@amount";
        parameter1.Value = amount;
        newcmd.Parameters.Add(parameter1);

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@buildingID";
        parameter2.Value = buildingID;
        newcmd.Parameters.Add(parameter2);

        newcmd.ExecuteNonQuery();

        dbConnection.Close();

        Debug.Log(string.Format("{0}'s production stockpile was changed to {1}", buildingID, amount));
    }
}
