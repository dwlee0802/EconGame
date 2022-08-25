using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;

public class PeopleQueries
{
    /*
     * accesses the People table in the DB
     * returns data as entries
     * tables are edited through this
     * modifications to the DB data should output a debug message
     * 
     * functions:
     * getAllPeople input string provinceID output List<PersonEntry>
     * getPerson input string personID output PersonEntry
     * addPerson input string provinceID (and optional parameters) output none
     * resetAllEmployers input provinceID output none
     * setEmployer input string personID string buildingID output none
     * getAllEmployees input string buildingID output List<PersonEntry>
     * changeMoney input string personID float amount output none
     * setMoney input string personID float amount output none
     * 
     */

    public static int GetEmployeeCount(string buildingID)
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
        string str = "SELECT Count(*) FROM People WHERE Employer = (@buildingID)";
        newcmd.CommandText = str;

        var parameter = newcmd.CreateParameter();
        parameter.ParameterName = "@buildingID";
        parameter.Value = buildingID;
        newcmd.Parameters.Add(parameter);

        IDataReader rdr = newcmd.ExecuteReader();

        int output = int.Parse(rdr.GetValue(0).ToString());

        dbConnection.Close();

        return output;
    }

    public static PersonEntry GetPerson(string personID)
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
        string str = "SELECT * FROM People WHERE ID = (@personID)";
        newcmd.CommandText = str;

        var parameter = newcmd.CreateParameter();
        parameter.ParameterName = "@personID";
        parameter.Value = personID;
        newcmd.Parameters.Add(parameter);

        IDataReader rdr = newcmd.ExecuteReader();

        if(rdr.Read())
        {
            PersonEntry output = new PersonEntry(
                personID,
                rdr.GetString(((int)TableColumns.PeopleColumns.Province)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Money)),
                rdr.GetInt32(((int)TableColumns.PeopleColumns.Health)),
                rdr.GetInt32(((int)TableColumns.PeopleColumns.Happiness)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Strength)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Intelligence)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.PricePerHealth)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.PricePerHappiness)),
                rdr.GetString(((int)TableColumns.PeopleColumns.Employer)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Personability))
                );

            dbConnection.Close();

            return output;
        }
        else
        {
            Debug.LogError("GetPerson ERROR");

            dbConnection.Close();

            return new PersonEntry("ERROR", "ERROR");
        }

    }

    public static List<PersonEntry> GetAllPeople(string provinceID)
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
        string str = "SELECT * FROM People WHERE Province = (@provinceID)";
        newcmd.CommandText = str;

        var parameter = newcmd.CreateParameter();
        parameter.ParameterName = "@provinceID";
        parameter.Value = provinceID;
        newcmd.Parameters.Add(parameter);

        IDataReader rdr = newcmd.ExecuteReader();

        List<PersonEntry> output = new List<PersonEntry>();

        while(rdr.Read())
        {
            output.Add(new PersonEntry(
                rdr.GetString(((int)TableColumns.PeopleColumns.ID)),
                rdr.GetString(((int)TableColumns.PeopleColumns.Province)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Money)),
                rdr.GetInt32(((int)TableColumns.PeopleColumns.Health)),
                rdr.GetInt32(((int)TableColumns.PeopleColumns.Happiness)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Strength)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Intelligence)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.PricePerHealth)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.PricePerHappiness)),
                rdr.GetString(((int)TableColumns.PeopleColumns.Employer)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Personability))
                )) ;
        }

        dbConnection.Close();

        return output;
    }

    public static List<PersonEntry> GetAllEmployees(string buildingID)
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
        string str = "SELECT * FROM People WHERE Employer = (@ID)";
        newcmd.CommandText = str;

        var parameter = newcmd.CreateParameter();
        parameter.ParameterName = "@ID";
        parameter.Value = buildingID;
        newcmd.Parameters.Add(parameter);

        IDataReader rdr = newcmd.ExecuteReader();

        List<PersonEntry> output = new List<PersonEntry>();

        while (rdr.Read())
        {
            output.Add(new PersonEntry(
                rdr.GetString(((int)TableColumns.PeopleColumns.ID)),
                rdr.GetString(((int)TableColumns.PeopleColumns.Province)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Money)),
                rdr.GetInt32(((int)TableColumns.PeopleColumns.Health)),
                rdr.GetInt32(((int)TableColumns.PeopleColumns.Happiness)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Strength)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Intelligence)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.PricePerHealth)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.PricePerHappiness)),
                rdr.GetString(((int)TableColumns.PeopleColumns.Employer)),
                rdr.GetFloat(((int)TableColumns.PeopleColumns.Personability))
                ));
        }

        dbConnection.Close();

        return output;
    }

    public static void ResetEmployers(string provinceID)
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
        string str = "UPDATE People SET employer = 'NULL' WHERE province = (@provinceID)";
        newcmd.CommandText = str;

        var parameter = newcmd.CreateParameter();
        parameter.ParameterName = "@provinceID";
        parameter.Value = provinceID;
        newcmd.Parameters.Add(parameter);

        newcmd.ExecuteNonQuery();

        //Debug.Log(string.Format("Resetted employer of all of {0} people in {1}", GetAllPeople(provinceID).Count, provinceID));

        dbConnection.Close();
    }

    public static void SetEmployer(string personID, string buildingID)
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
        string str = "UPDATE People SET Employer = (@employerID) WHERE ID = (@personID)";
        newcmd.CommandText = str;

        var parameter1 = newcmd.CreateParameter();
        parameter1.ParameterName = "@employerID";
        parameter1.Value = buildingID;
        newcmd.Parameters.Add(parameter1);

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@personID";
        parameter2.Value = personID;
        newcmd.Parameters.Add(parameter2);

        newcmd.ExecuteNonQuery();

        //Debug.Log(string.Format("{0}'s employer was set to {1}", personID, buildingID));

        dbConnection.Close();
    }

    public static void ChangeMoney(string personID, float byAmount)
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
        string str = "UPDATE People SET Money = Money + (@money) WHERE ID = (@personID)";
        newcmd.CommandText = str;

        var parameter1 = newcmd.CreateParameter();
        parameter1.ParameterName = "@money";
        parameter1.Value = byAmount;
        newcmd.Parameters.Add(parameter1);

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@personID";
        parameter2.Value = personID;
        newcmd.Parameters.Add(parameter2);

        newcmd.ExecuteNonQuery();

        //Debug.Log(string.Format("{0}'s money was changed by {1}", personID, byAmount));

        dbConnection.Close();
    }

    public static void ChangeHealth(string personID, float byAmount)
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

        PersonEntry person = GetPerson(personID);

        IDbCommand newcmd;
        newcmd = dbConnection.CreateCommand();

        var parameter1 = newcmd.CreateParameter();
        parameter1.ParameterName = "@health";

        if (person.getHealth() + byAmount > 100)
        {
            //set as 100
            string str = "UPDATE People SET Health = (@health) WHERE ID = (@personID)";
            newcmd.CommandText = str;

            parameter1.Value = 100;
        }
        else if(person.getHealth() + byAmount < 0)
        {
            //set as 0
            string str = "UPDATE People SET Health = (@health) WHERE ID = (@personID)";
            newcmd.CommandText = str;

            parameter1.Value = 0;
        }
        else
        {
            string str = "UPDATE People SET Health = Health + (@health) WHERE ID = (@personID)";
            newcmd.CommandText = str;

            parameter1.Value = byAmount;
        }

        newcmd.Parameters.Add(parameter1);

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@personID";
        parameter2.Value = personID;
        newcmd.Parameters.Add(parameter2);

        newcmd.ExecuteNonQuery();

        Debug.Log(string.Format("{0}'s health was changed by {1}", personID, byAmount));

        dbConnection.Close();
    }

    public static void ChangeHappiness(string personID, float byAmount)
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


        PersonEntry person = GetPerson(personID);

        IDbCommand newcmd;
        newcmd = dbConnection.CreateCommand();

        var parameter1 = newcmd.CreateParameter();
        parameter1.ParameterName = "@happiness";

        if (person.getHealth() + byAmount > 100)
        {
            //set as 100
            string str = "UPDATE People SET Happiness = (@happiness) WHERE ID = (@personID)";
            newcmd.CommandText = str;

            parameter1.Value = 100;
        }
        else if (person.getHealth() + byAmount < 0)
        {
            //set as 0
            string str = "UPDATE People SET Happiness = (@happiness) WHERE ID = (@personID)";
            newcmd.CommandText = str;

            parameter1.Value = 0;
        }
        else
        {
            string str = "UPDATE People SET Happiness = Health + (@happiness) WHERE ID = (@personID)";
            newcmd.CommandText = str;

            parameter1.Value = byAmount;
        }

        newcmd.Parameters.Add(parameter1);

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@personID";
        parameter2.Value = personID;
        newcmd.Parameters.Add(parameter2);

        newcmd.ExecuteNonQuery();

        Debug.Log(string.Format("{0}'s happiness was changed by {1}", personID, byAmount));

        dbConnection.Close();
    }

    public static void AddPerson(PersonEntry person)
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
        string str = "INSERT INTO People VALUES ((@param1), (@param2), (@param3), (@param4), (@param5), (@param6), (@param7), (@param8), (@param9), (@param10), (@param12))";
        newcmd.CommandText = str;

        var param1 = newcmd.CreateParameter();
        param1.ParameterName = "@param1";
        param1.Value = person.getID();
        newcmd.Parameters.Add(param1);

        var param2 = newcmd.CreateParameter();
        param2.ParameterName = "@param2";
        param2.Value = person.getProvince();
        newcmd.Parameters.Add(param2);

        var param3 = newcmd.CreateParameter();
        param3.ParameterName = "@param3";
        param3.Value = person.getMoney();
        newcmd.Parameters.Add(param3);

        var param4 = newcmd.CreateParameter();
        param4.ParameterName = "@param4";
        param4.Value = person.getHealth();
        newcmd.Parameters.Add(param4);

        var param5 = newcmd.CreateParameter();
        param5.ParameterName = "@param5";
        param5.Value = person.getHappiness();
        newcmd.Parameters.Add(param5);

        var param6 = newcmd.CreateParameter();
        param6.ParameterName = "@param6";
        param6.Value = person.getStrength();
        newcmd.Parameters.Add(param6);

        var param7 = newcmd.CreateParameter();
        param7.ParameterName = "@param7";
        param7.Value = person.getIntelligence();
        newcmd.Parameters.Add(param7);

        var param8 = newcmd.CreateParameter();
        param8.ParameterName = "@param8";
        param8.Value = person.getPricePerHealth();
        newcmd.Parameters.Add(param8);

        var param9 = newcmd.CreateParameter();
        param9.ParameterName = "@param9";
        param9.Value = person.getPricePerHappiness();
        newcmd.Parameters.Add(param9);

        var param10 = newcmd.CreateParameter();
        param10.ParameterName = "@param10";
        param10.Value = person.getEmployer();
        newcmd.Parameters.Add(param10);

        var param12 = newcmd.CreateParameter();
        param12.ParameterName = "@param12";
        param12.Value = person.getPersonability();
        newcmd.Parameters.Add(param12);

        newcmd.ExecuteNonQuery();
        Debug.Log(string.Format("new person was added.\n{0}", person));
        dbConnection.Close();
    }

    public static void RemovePerson(string personID)
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
        string str = "DELETE FROM People WHERE ID = (@personID)";
        newcmd.CommandText = str;

        var parameter2 = newcmd.CreateParameter();
        parameter2.ParameterName = "@personID";
        parameter2.Value = personID;
        newcmd.Parameters.Add(parameter2);

        newcmd.ExecuteNonQuery();

        Debug.Log(string.Format("{0} was removed.", personID));

        dbConnection.Close();
    }
}
