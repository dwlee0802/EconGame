using System.Collections;
using System.Collections.Generic;

public class GoodsManager
{
    public static Dictionary<int, string> TypeToNameDict = new Dictionary<int, string>()
    {
        { 0, "Bread" },
        { 1, "Meat"},
        { 2, "Wheat"},
    };
    public static Dictionary<string, int> NameToTypeDict = new Dictionary<string, int>()
    {
        { "Bread", 0 },
        { "Meat", 1},
        { "Wheat", 2},
    };

    public static Dictionary<int, int> ProductIngredientDict = new Dictionary<int, int>()
    {
        { 0, 2},
        { 1, 2},
        { 2, -1},
    };
    
    static int goodCount = TypeToNameDict.Count;

    //how much a worker's skill affects production
    public static float[][] skillModifiers = new float[3][]
    {
        //strength, intelligence, personability

        //bread
        new float[] { 1f, 0.5f, 0.5f},
        //meat
        new float[] { 2f, 1f, 0.5f},
        //wheat
        new float[] { 1f, 0.5f, 0.5f}
    };

    //how much one ingredient is converted to the product
    public static int[] goodsProductionRatio = new int[3]
    {
        //bread
        2,
        //meat
        2,
        //wheat
        4
    };

    //how much one ingredient is converted to the product
    public static int[] goodsLaborPerUnit = new int[3]
    {
        //bread
        2,
        //meat
        2,
        //wheat
        1
    };

    //How much utility a certain good type provides when one more of that good is added. Good count x 11. Current value is rounded to the nearest tenth.
    static int[,] marginalHealth = new int[3, 15] 
    {
        //bread
        {9,9,8,7,6,5,4,3,2,1,0,0,0,0,0},
        //meat
        {8,9,10,9,8,7,6,5,4,3,0,0,0,0,0},
        //wheat
        {6,5,5,4,3,2,2,1,1,0,0,0,0,0,0}
    };
    static int[,] marginalHappiness = new int[3, 15]
    {
        //bread
        {7,7,7,5,4,3,2,1,0,0,0,0,0,0,0},
        //meat
        {9,8,7,6,5,4,3,3,3,3,3,2,1,0,0},
        //wheat
        {4,4,3,2,2,1,1,0,0,0,0,0,0,0,0}
    };

    public static int GoodCount { get => goodCount;}

    //Determines the good that provides the most total util based on current stats.
    public static int CalculateBestGood(PersonEntry person)
    {
        int highestvalue = 0;
        int highesttype = 0;

        for(int i = 0; i < goodCount; i++)
        {
            //if the person is done with the current good index, skip
            if(person.finishedGoodtypes[i])
            {
                continue;
            }

            int currentvalue = marginalHealth[i, ((int)(person.getHealth() / 10))] + marginalHappiness[i, ((int)(person.getHappiness() / 10))];
            if (highestvalue < currentvalue)
            {
                highestvalue = currentvalue;
                highesttype = i;
            }
        }

        return highesttype;
    }

    public static int CalculateHealthGain(int type, int currentHealth)
    {
        return marginalHealth[type, (int)(currentHealth / 10)];
    }

    public static int CalculateHappinessGain(int type, int currentHappiness)
    {
        return marginalHappiness[type, (int)(currentHappiness / 10)];
    }
}
