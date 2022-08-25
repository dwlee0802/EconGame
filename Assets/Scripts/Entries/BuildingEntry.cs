using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingEntry
{
    string ID;
    string province;
    int goodtype;

    float budget;
    float wage;

    int ingredientStockpile;
    float premium;

    int level;

    //per one
    float averageIngredientCost;

    float productionStockpile;

    int lastSales;
    
    public void setIngredientStockpile(int amount)
    {
        ingredientStockpile += amount;

        if(ingredientStockpile > level * 2)
        {
            ingredientStockpile = level * 2;
        }
        if(ingredientStockpile < 0)
        {
            ingredientStockpile = 0;
        }
    }

    public string getID()
    {
        return ID;
    }

    public string getProvince()
    {
        return province;
    }

    public int getGoodType()
    {
        return goodtype;
    }

    public int getLevel()
    {
        return level;
    }

    public float getWage()
    {
        return wage;
    }

    public float getAverageIngredientCost()
    {
        return averageIngredientCost;
    }

    public float getPremium()
    {
        return premium;
    }

    public float getBudget()
    {
        return budget;
    }

    public float getIngredientStockpile()
    {
        return ingredientStockpile;
    }

    public float getProductionStockpile()
    {
        return productionStockpile;
    }

    public int getLastSales()
    {
        return lastSales;
    }

    public BuildingEntry(string iD, string province, int type, float budget = 0, float wage = 0, int ingredientStockpile = 0, float premium = 0, int level = 1, float productionStockpile = 0, float averageIngredientCost = 0, int lastSales = 0)
    {
        ID = iD;
        this.province = province;
        this.goodtype = type;
        this.budget = budget;
        this.wage = wage;
        this.ingredientStockpile = ingredientStockpile;
        this.premium = premium;
        this.level = level;
        this.productionStockpile = productionStockpile;
        this.averageIngredientCost = averageIngredientCost;
        this.lastSales = lastSales;
    }
}
