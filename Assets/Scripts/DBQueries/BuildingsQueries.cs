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
                rdr.GetFloat(((int)TableColumns.BuildingColumns.LaborStockpile)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.AverageIngredientCost)),
                rdr.GetInt32(((int)TableColumns.BuildingColumns.LastSales))
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
                rdr.GetFloat(((int)TableColumns.BuildingColumns.LaborStockpile)),
                rdr.GetFloat(((int)TableColumns.BuildingColumns.AverageIngredientCost)),
                rdr.GetInt32(((int)TableColumns.BuildingColumns.LastSales))
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

        //Debug.Log(string.Format("{0}'s budget was changed by {1}", buildingID, byAmount));
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

        BuildingEntry building = GetBuilding(buildingID);

        if(building.getIngredientStockpile() + byAmount > building.getLevel()*2)
        {
            SetIngredientStock(buildingID, building.getLevel() * 2);
            return;
        }

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

        //Debug.Log(string.Format("{0}'s ingredient stockpile was changed by {1} and is now {2}", buildingID, byAmount, GetBuilding(buildingID).getIngredientStockpile()));

        dbConnection.Close();
    }

    public static void SetIngredientStock(string buildingID, float byAmount)
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
        string str = "UPDATE Buildings SET IngredientStockpile = (@amount) WHERE ID = (@buildingID)";
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

        //Debug.Log(string.Format("{0}'s ingredient stockpile was set to {1}", buildingID, byAmount));
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

        //Debug.Log(string.Format("{0}'s average ingredient cost was changed by {1}", buildingID, byAmount));
    }

    public static void ChangeIngredientCost(string buildingID, float byAmount)
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

        BuildingEntry building = GetBuilding(buildingID);

        if (building.getIngredientStockpile() + byAmount > building.getLevel() * 2)
        {
            SetIngredientStock(buildingID, building.getLevel() * 2);
            return;
        }

        IDbCommand newcmd;
        newcmd = dbConnection.CreateCommand();
        string str = "UPDATE Buildings SET AverageIngredientCost = AverageIngredientCost + (@amount) WHERE ID = (@buildingID)";
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

        //Debug.Log(string.Format("{0}'s ingredient stockpile was changed by {1} and is now {2}", buildingID, byAmount, GetBuilding(buildingID).getIngredientStockpile()));

        dbConnection.Close();
    }

    public static void SetLaborStockpile(string buildingID, float amount)
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
        string str = "UPDATE Buildings SET LaborStockpile = (@amount) WHERE ID = (@buildingID)";
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

        //Debug.Log(string.Format("{0}'s production stockpile was changed to {1}", buildingID, amount));
    }

    public static void ChangeWage(string buildingID, float byAmount)
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

        BuildingEntry building = GetBuilding(buildingID);

        IDbCommand newcmd;
        newcmd = dbConnection.CreateCommand();
        string str = "UPDATE Buildings SET Wage = Wage + (@amount) WHERE ID = (@buildingID)";
        newcmd.CommandText = str;

        var parameter1 = newcmd.CreateParameter();
        parameter1.ParameterName = "@amount";
        parameter1.Value = byAmount;
        newcmd.Parameters.Add(parameter1);

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@buildingID";
        parameter2.Value = buildingID;
        newcmd.Parameters.Add(parameter2);

        //wage cannot go under zero
        if (building.getWage() + byAmount < 1)
        {
            str = "UPDATE Buildings SET Wage = (@amount) WHERE ID = (@buildingID)";
            parameter1.Value = 1;
        }

        newcmd.ExecuteNonQuery();

        //Debug.Log(string.Format("{0}'s wage was changed by {1} and is now {2}", buildingID, byAmount, GetBuilding(buildingID).getWage()));

        dbConnection.Close();
    }

    public static void ChangeLastSales(string buildingID, float byAmount)
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

        BuildingEntry building = GetBuilding(buildingID);

        IDbCommand newcmd;
        newcmd = dbConnection.CreateCommand();
        string str = "UPDATE Buildings SET LastSales = LastSales + (@amount) WHERE ID = (@buildingID)";
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

        //Debug.Log(string.Format("{0}'s last amount was changed by {1}", buildingID, byAmount));

        dbConnection.Close();
    }

    public static void ResetLastSales(string provinceID)
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
        string str = "UPDATE Buildings SET LastSales = 0 WHERE Province = (@provinceID)";
        newcmd.CommandText = str;

        var parameter = newcmd.CreateParameter();
        parameter.ParameterName = "@provinceID";
        parameter.Value = provinceID;
        newcmd.Parameters.Add(parameter);

        newcmd.ExecuteNonQuery();
        dbConnection.Close();
    }

    //returns 1 on success. otherwise -1
    public static int ChangePremium(string buildingID, float byAmount)
    {
        if (byAmount == 0)
        {
            return -1;
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

        BuildingEntry building = GetBuilding(buildingID);

        IDbCommand newcmd;
        newcmd = dbConnection.CreateCommand();
        string str = "UPDATE Buildings SET Premium = Premium + (@amount) WHERE ID = (@buildingID)";
        newcmd.CommandText = str;

        var parameter1 = newcmd.CreateParameter();
        parameter1.ParameterName = "@amount";
        parameter1.Value = byAmount;
        newcmd.Parameters.Add(parameter1);

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@buildingID";
        parameter2.Value = buildingID;
        newcmd.Parameters.Add(parameter2);

        int returncode = 1;

        //wage cannot go under zero
        if (building.getPremium() + byAmount < 0)
        {
            str = "UPDATE Buildings SET Premium = (@amount) WHERE ID = (@buildingID)";
            parameter1.Value = 0;
            newcmd.CommandText = str;
            returncode = -1;
        }

        newcmd.ExecuteNonQuery();

        //Debug.Log(string.Format("{0}'s Premium was changed by {1} and is now {2}", buildingID, byAmount, GetBuilding(buildingID).getPremium()));

        dbConnection.Close();

        return returncode;
    }

}
