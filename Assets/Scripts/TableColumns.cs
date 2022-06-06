using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableColumns
{
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
        Personability
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
        ProductionStockpile
    }

    public enum ProvinceColumns
    {
        ID,
        Name,
        Nation
    }
}
