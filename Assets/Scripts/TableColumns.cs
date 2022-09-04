using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableColumns
{
    //adding columns checklist
    //-add on sql
    //-add here
    //-update entries initializers and getters
    //-update queries for both single get and all get

    public enum PeopleColumns
    {
        ID,
        Province,
        Money,
        Health,
        Happiness,
        Strength,
        Intelligence,
        PricePerHealth,
        PricePerHappiness,
        Employer,
        Personability,
        GainedHealth,
        GainedHappiness
    }

    public enum BuildingColumns
    {
        ID,
        Province,
        Type,
        Owner,
        Budget,
        Wage,
        IngredientStockpile,
        Premium,
        Level,
        AverageIngredientCost,
        ProductionStockpile,
        LastSales
    }

    public enum ProvinceColumns
    {
        ID,
        Name,
        Nation,
        GrowthPoints
    }
}
