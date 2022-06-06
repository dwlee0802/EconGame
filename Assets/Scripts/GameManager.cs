using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

/* Current Status
 * -Need to come up with how to change parameters for next day
 * -Make people output more buy orders. Probably 4 will be enough.
 */

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PeopleQueries.RemovePerson("PP8");
        /*
        string dbname = "GameDatabase.db";
        string currentPath = System.IO.Path.GetDirectoryName(Application.dataPath);
        currentPath = currentPath + "\\" + dbname;

        Debug.Log("Database file path: " + currentPath);

        dbConnection = new SqliteConnection("URI=file:" + currentPath);
        dbConnection.Open();

        if (dbConnection.State == ConnectionState.Open)
        {
            Debug.Log("Connection SUCCESS");
        }
        else
        {
            Debug.Log("Connection FAILED");
        }

        dbConnection.Close();
        */

        //dbReader.NextResult();

        /*
        dbReader.Close();
        dbReader = null;
        dbCommand.Dispose();
        dbCommand = null;
        dbConnection.Close();
        dbConnection = null;
        */


        /*
        //testing
        Debug.Log("Testing GetProvinceIDs");
        foreach (string item in GetProvinceIDs())
        {
            Debug.Log(item);
        }


        Debug.Log("Testing GetPersonIDs");
        foreach (string item in GetProvinceIDs())
        {
            Debug.Log("Person ID in province " + item);

            foreach (string personID in GetPersonIDs(item))
            {
                Debug.Log(personID);
            }
        }


        Debug.Log("Test people count");

        Debug.Log("There are total of " + int.Parse(ReadDatabase("SELECT COUNT(ID) FROM People").GetValue(0).ToString()) + " People living in the game world");

        Debug.Log("Testing GetBuildingIDs");
        foreach (string item in GetProvinceIDs())
        {
            Debug.Log("Building ID in province " + item);

            foreach (string buildingID in GetBuildingIDs(item))
            {
                Debug.Log(buildingID);
            }
        }


        Debug.Log("Testing Best Good"); 
        foreach (string provinceID in GetProvinceIDs())
        {
            Debug.Log("The people living in province " + provinceID);

            foreach (string personID in GetPersonIDs(provinceID))
            {
                IDataReader dbReader = ReadDatabase("SELECT * FROM People WHERE ID = \'" + personID + "\'");

                if (dbReader.Read())
                {
                    Debug.Log(personID + "'s health is " + dbReader.GetInt32(3).ToString() + " and happiness is " + dbReader.GetInt32(4).ToString() + ". It wants " + GoodsManager.TypeToNameDict[GoodsManager.CalculateBestGood(dbReader.GetInt32(3), dbReader.GetInt32(4))]);
                }
            }
        }
        */
        /*
        Debug.Log("Testing Buy Order");
        foreach (string provinceID in GetProvinceIDs())
        {
            Debug.Log("The people living in province " + provinceID);

            foreach (string personID in GetPersonIDs(provinceID))
            {
                    Debug.Log(personID + " wants " + GenerateBuyOrderPerson(PeopleQueries.GetPerson(personID)));
            }
        }
        /*
       
        Debug.Log("Testing Sell Order");
        foreach (string provinceID in GetProvinceIDs())
        {
            Debug.Log("The buildings in province " + provinceID);

            foreach (var building in BuildingsQueries.GetAllBuildings(provinceID))
            {
                Debug.Log("Generating for buildingID " + building.getID() + ". It has " + PeopleQueries.GetEmployeeCount(building.getID()) + " employees.");

                foreach(var item in GenerateSellOrder(building))
                {
                    Debug.Log(building.getID() + " wants to sell " + item);
                }
            }
        }
        
        

        
        IDataReader dbReader = ReadDatabase("SELECT * FROM People WHERE ID = \'PP1\'");

        while (dbReader.Read())
        {
            Debug.Log(dbReader.GetInt32(3).ToString() + ", " + dbReader.GetInt32(4).ToString());
        }

        UpdateDatabase("UPDATE People SET Health = 5 WHERE ID = \'PP1\'"); 
        
        dbReader = ReadDatabase("SELECT * FROM People WHERE ID = \'PP1\'");

        while (dbReader.Read())
        {
            Debug.Log(dbReader.GetInt32(3).ToString() + ", " + dbReader.GetInt32(4).ToString());
        }
        

        /*
        Debug.Log("Testing prepared statements");
        IDbCommand newcmd;
        newcmd = dbConnection.CreateCommand();
        string str = "SELECT * FROM People WHERE province = (@provinceID)";
        newcmd.CommandText = str;

        var parameter = newcmd.CreateParameter();
        parameter.ParameterName = "@provinceID";
        parameter.Value = "PV1";
        newcmd.Parameters.Add(parameter);

        IDataReader rdr = newcmd.ExecuteReader();

        while(rdr.Read())
        {
            Debug.Log("value: " + rdr.GetString(0));
        }
        */

    }


    //called when the player presses the pass day button. calculates the activity in the game world.
    public void ProcessDay()
    {
        Debug.Log("Process Day");
        OperateLaborMarket("PV1");
        OperateMarket("PV1");
    }


    //Market operation related methods.

    //Inputs data of a single building. Calculates how much and which good it produces. Outputs a MarketOrder object with the data.
    private MarketOrder[] GenerateSellOrder(BuildingEntry building)
    {
        //calculate how much to sell and at what price
        //price is determined by (total cost / expected sales) + premium.
        //total cost includes total wage and ingredient costs.

        float totalwage = PayWages(building);

        //produce goods. Amount is increased by modifiers (not implemented yet)
        //subtract ingredients used to make goods
        int producedAmount = Production(building);

        //total cost is total wage plus ingredient costs
        float totalcost = totalwage + building.getAverageIngredientCost() * producedAmount;

        //add premium
        float wantsprice = ((int)((totalcost / producedAmount) * 1000)) / 1000 + building.getPremium();

        MarketOrder[] outputOrder = new MarketOrder[producedAmount];

        for(int i = 0; i< producedAmount; i++)
        {
            //type, price, origin
            outputOrder[i] = new MarketOrder(building.getGoodType(), wantsprice, building.getID());
        }

        return outputOrder;
    }

    private int Production(BuildingEntry building)
    {
        float ingredientStock = building.getIngredientStockpile();
        float productionStock = building.getProductionStockpile();
        List<PersonEntry> employees = PeopleQueries.GetAllEmployees(building.getID());

        if (GoodsManager.ProductIngredientDict[building.getGoodType()] < 0)
        {
            ingredientStock = 1;
        }

        foreach (PersonEntry employee in employees)
        {
            productionStock += employee.getStrength() * GoodsManager.skillModifiers[building.getGoodType()][0]
                + employee.getStrength() * GoodsManager.skillModifiers[building.getGoodType()][0]
                + employee.getIntelligence() * GoodsManager.skillModifiers[building.getGoodType()][1]
                + employee.getPersonability() * GoodsManager.skillModifiers[building.getGoodType()][2];
        }

        int count = 0;

        //check if there's enough ingredients in stock
        while(ingredientStock >= 1 && productionStock >= 1)
        {
            productionStock -= 1;
            ingredientStock -= 1;
            count += 1;

            if(GoodsManager.ProductIngredientDict[building.getGoodType()] < 0)
            {
                ingredientStock = 1;
            }
        }

        if(productionStock > building.getLevel() * 2)
        {
            productionStock = building.getLevel() * 2;
        }

        if (GoodsManager.ProductIngredientDict[building.getGoodType()] >= 0)
        {
            BuildingsQueries.ChangeIngredientStock(building.getID(), -count);
        }


        BuildingsQueries.SetProductionStockpile(building.getID(), productionStock);

        return count;
    }

    //Inputs data of a single person. Calculates its preferences and outputs a MarketOrder object with the data.
    //This function is called by processDay for all people that are still participating in the market until there are no participants.
    //Participating means that they have money and that their daily Health and Happiness gained is below the daily deduction amount.
    private MarketOrder GenerateBuyOrderPerson(PersonEntry person)
    {
        string personID = person.getID();
        MarketOrder outputOrder = null;

        //which good to purchase first is the one that provides the most value based on its current health and happiness.
        int wantsgoodtype = GoodsManager.CalculateBestGood(person.getHealth(), person.getHappiness());

        //Price is determined by following equation: (Asking Price) = (Good¡¯s Heath value) * (PricePerHealth) + (Good¡¯s Happiness value) * (PricePerHappiness)
        float wantsprice = person.getPricePerHealth() * GoodsManager.CalculateHealthGain(wantsgoodtype, person.getHealth()) + person.getPricePerHappiness() * GoodsManager.CalculateHappinessGain(wantsgoodtype, person.getHappiness());

        outputOrder = new MarketOrder(wantsgoodtype, wantsprice, personID);

        return outputOrder;
    }

    
    //for purchasing ingredients
    //price is the average value for the past few days
    //The buyer can also go over the average value based on how well its business is doing
    //Buyers always try to get enough ingredients for a full production.
    private List<MarketOrder> GenerateBuyOrderBuilding(BuildingEntry building)
    {
        List<MarketOrder> outputOrders = new List<MarketOrder>();

        //only make amount possible to buy with current funds.
        float currentFunds = building.getBudget();
        float askingPrice = building.getAverageIngredientCost();
        float potentialStock = building.getIngredientStockpile();

        while(currentFunds >= askingPrice && potentialStock <= building.getLevel() * 2)
        {
            currentFunds -= askingPrice;
            potentialStock++;

            int ingredientType = 0;

            if(GoodsManager.ProductIngredientDict[building.getGoodType()] < 0)
            {
                break;
            }
            else
            {
                ingredientType = GoodsManager.ProductIngredientDict[building.getGoodType()];
            }

            outputOrders.Add(new MarketOrder(ingredientType, askingPrice, building.getID()));
        }

        return outputOrders;
    }

    

    //Operates the market for a single province.
    private void OperateMarket(string provinceID)
    {
        //make an array of lists that will hold market orders by each good type.
        List<MarketOrder>[] buyOrders = new List<MarketOrder>[GoodsManager.GoodCount];
        for(int i = 0; i < GoodsManager.GoodCount; i++)
        {
            buyOrders[i] = new List<MarketOrder>();
        }
        List<MarketOrder>[] sellOrders = new List<MarketOrder>[GoodsManager.GoodCount];
        for (int i = 0; i < GoodsManager.GoodCount; i++)
        {
            sellOrders[i] = new List<MarketOrder>();
        }

        //make a list of people who are participating in the market
        //participating means that their needs are not met and they have money

        //fill buy order from people
        foreach (PersonEntry person in PeopleQueries.GetAllPeople(provinceID))
        {

            MarketOrder currentMarketOrder = GenerateBuyOrderPerson(person);

            buyOrders[currentMarketOrder.Goodtype].Add(currentMarketOrder);
        }

        //fill buy order and sell order from buildings
        foreach(BuildingEntry building in BuildingsQueries.GetAllBuildings(provinceID))
        {
            foreach(MarketOrder order in GenerateBuyOrderBuilding(building))
            {
                buyOrders[order.Goodtype].Add(order);
            }

            foreach(MarketOrder order in GenerateSellOrder(building))
            {
                sellOrders[order.Goodtype].Add(order);
            }
        }

        //Sort each market order lists. Buy orders from highest to lowest, sell orders from lowest to highest.
        for(int i = 0; i < GoodsManager.GoodCount; i++)
        {
            buyOrders[i].Sort((y, x) => x.AskingPrice.CompareTo(y.AskingPrice));
        }
        for (int i = 0; i < GoodsManager.GoodCount; i++)
        {
            sellOrders[i].Sort((x, y) => x.AskingPrice.CompareTo(y.AskingPrice));
        }

        //Check for transaction opportunities. If two orders have the same good type and the sell order's asking price is lower than the buy order's, exchange.
        for(int i = 0; i < GoodsManager.GoodCount; i++)
        {
            Debug.Log(string.Format("Operate market for good type {0}", GoodsManager.TypeToNameDict[i]));
            Debug.Log(string.Format("{0} buy orders and {1} sell orders", buyOrders[i].Count, sellOrders[i].Count));


            int transactioncount = 0;

            int smallercount = buyOrders[i].Count;
                
            if(smallercount > sellOrders[i].Count)
            {
                smallercount = sellOrders[i].Count;
            }

            for(int j = 0; j < smallercount; j++)
            {
                Debug.Log(string.Format("Checking transaction between buy order for {0} and sell order for {1}", buyOrders[i][j].AskingPrice, sellOrders[i][j].AskingPrice));

                if(sellOrders[i][j].AskingPrice <= buyOrders[i][j].AskingPrice)
                {
                    //exchange goods and money.
                    //access the buyer and subtract the asking price of the sell order. give him the good type specified.
                    //access the seller and add money to it.
                    //remove both orders from the list.
                    MarketOrder order = sellOrders[i][j];

                    string buyerFirstTwo = (buyOrders[i][j].OriginID[0].ToString() + buyOrders[i][j].OriginID[1].ToString());

                    BuildingEntry seller = BuildingsQueries.GetBuilding(sellOrders[i][j].OriginID);

                    transactioncount++;

                    if (buyerFirstTwo == "PP")
                    {
                        PersonEntry buyer = PeopleQueries.GetPerson(buyOrders[i][j].OriginID); 
                        
                        if (buyer.getMoney() >= order.AskingPrice)
                        {
                            PeopleQueries.ChangeMoney(buyer.getID(), (-1) * order.AskingPrice);
                            PeopleQueries.ChangeHealth(buyer.getID(), GoodsManager.CalculateHealthGain(order.Goodtype, buyer.getHealth()));
                            PeopleQueries.ChangeHappiness(buyer.getID(), GoodsManager.CalculateHappinessGain(order.Goodtype, buyer.getHappiness()));
                        
                            BuildingsQueries.ChangeMoney(seller.getID(), order.AskingPrice);

                        }
                    }
                    else if(buyerFirstTwo == "BD")
                    {
                        BuildingEntry buyer = BuildingsQueries.GetBuilding(buyOrders[i][j].OriginID);

                        if (buyer.getBudget() >= order.AskingPrice)
                        {
                            BuildingsQueries.ChangeMoney(buyer.getID(), (-1) * order.AskingPrice);
                            BuildingsQueries.ChangeAverageIngredientCost(buyer.getID(), order.AskingPrice);
                            BuildingsQueries.ChangeIngredientStock(buyer.getID(), 1);
                            
                            BuildingsQueries.ChangeMoney(seller.getID(), order.AskingPrice);

                        }
                    }
                    else
                    {
                        Debug.LogError("Neither person nor building. who tf r u");
                    }
                }
                else
                {
                    Debug.Log(string.Format("End checking for transactions for {0}", GoodsManager.TypeToNameDict[i]));
                    break;
                }
            }

            Debug.LogWarning(string.Format("{0} transactions made for {1}. ", transactioncount, GoodsManager.TypeToNameDict[i]));
        }
        

    }

    private void OperateLaborMarket(string provinceID)
    {
        List<BuildingEntry> buildings = BuildingsQueries.GetAllBuildings(provinceID);
        List<PersonEntry> people = PeopleQueries.GetAllPeople(provinceID);

        //every building wants to employ level * 2 amount of employees
        //the wage starts at 0 and slowly goes up until the target employment is reached

        //1. reset all people's employers to null.
        PeopleQueries.ResetEmployers(provinceID);

        //2. order buildings by their wage, descending
        buildings.Sort((x, y) => -1 * x.getWage().CompareTo(y.getWage()));

        //3. for each building, order the people by strength/intellect descending and hire as much as needed.

        //buildings with higher wages get to choose first
        foreach (var building in buildings)
        {
            //order them by their proficiency skill total
            float[] skillModifier = GoodsManager.skillModifiers[building.getGoodType()];

            if (people.Count == 0)
            {
                break;
            }
            

            people.Sort((x, y) => -1 * (x.getStrength() * skillModifier[0] + x.getIntelligence() * skillModifier[1] + x.getPersonability() * skillModifier[2]).CompareTo(y.getStrength() * skillModifier[0] + y.getIntelligence() * skillModifier[1] + y.getPersonability() * skillModifier[2]) );

            int lastremovedindex = -1;
            int limit = building.getLevel() * 2;

            //hire people and then remove them from the list if available. If labor pool is depleted, exit.
            if (people.Count < limit)
            {
                //less available people than needed -> hire everyone
                limit = people.Count;
            }

            for (int i = 0; i < limit; i++)
            {
                PeopleQueries.SetEmployer(people[i].getID(), building.getID());
                lastremovedindex = i;
            }

            for(int i = lastremovedindex; i >= 0; i-- )
            {
                people.RemoveAt(i);
            }
        }
    }

    private float PayWages(BuildingEntry building)
    {
        //pay wages to employees
        //what happens if the company doesnt have money? they get laid off and the building shuts down next round.

        List<PersonEntry> people = PeopleQueries.GetAllEmployees(building.getID());
        float leftamount = building.getBudget();
        float wage = building.getWage();
        float total = 0;

        foreach(PersonEntry person in people)
        {
            if(leftamount >= wage)
            {
                PeopleQueries.ChangeMoney(person.getID(), wage);
                BuildingsQueries.ChangeMoney(building.getID(), -1 * wage);
                leftamount -= wage;
                total += wage;
            }
            else if(leftamount < wage)
            {
                PeopleQueries.ChangeMoney(person.getID(), leftamount);
                BuildingsQueries.ChangeMoney(building.getID(), -1 * leftamount);
                total += leftamount;
            }
        }

        return total;
    }

    private void DailyDeduction(string provinceID)
    {
        //deduct 10 happiness and health everyday
        //cant go under zero
        //cant go over 100
    }

    private void DayReflection(string provinceID)
    {
        /*
         * Modifies the parameters and data based on the day's results to be used tmrw
         * parameters affected:
         * 
         * premium of buildings
         * people's price per happiness and health
         * wage
         * strength intelligence
         */

        foreach(BuildingEntry building in BuildingsQueries.GetAllBuildings(provinceID))
        {
            //premium
            //-decrease when the building is losing money
            //-cannot go under zero. If it is zero and is still losing money, consider reducing the level.
            //if everything is sold, increase.


            //wage
            //-if the maximum employee count is not reached, increase steadily.
            //-cannot go under zero
            //-when the building is making money, decrease very slowly
            //-when the building is losing money, increase.
        }

        foreach(PersonEntry person in PeopleQueries.GetAllPeople(provinceID))
        {
            //price per happiness and health
            //-increase or decrease based on how much is gained that day.
            //--if lower than 10, increase
            //--if same or higher than 10, decrease
            //--how much to increase or decrease is influenced by the difference btw 10 and gained amount

            //strength and intelligence and personability
            //the level affects work efficiency.
            //as the pop works it gains points and when sufficient amount is gained, it levels up
            //more points are required as the level gets higher.
            //non-productive buildings can 'hire' pops for an education.
        }
    }
}

public class MarketOrder
{
    int goodtype;
    float askingPrice;
    string originID;

    public MarketOrder(int goodtype, float askingPrice, string originID)
    {
        this.goodtype = goodtype;
        this.askingPrice = askingPrice;
        this.originID = originID;
    }

    public int Goodtype { get => goodtype;}
    public string OriginID { get => originID;}
    public float AskingPrice { get => askingPrice;}

    public override string ToString()
    {
        return "Market Order for " + GoodsManager.TypeToNameDict[goodtype] + "\nAsking Price: " + askingPrice + "\nOrigin: " + originID;
    }
}
