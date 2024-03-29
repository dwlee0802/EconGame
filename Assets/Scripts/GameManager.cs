using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

/* Current Status
 * come up with better algorithm for pricing, wage determination, and desired wage
 */

public class GameManager : MonoBehaviour
{
    UIManager uiManager;
    bool poorFirst = true;

    // Start is called before the first frame update
    void Start()
    {
        uiManager = GetComponent<UIManager>();
    }


    //called when the player presses the pass day button. calculates the activity in the game world.
    public void ProcessDay()
    {
        uiManager.NextTurn();
        Debug.Log("Process Day");
        OperateLaborMarket("PV1");
        OperateMarket("PV1");
        DayReflection("PV1");
        PopulationGrowth("PV1");
        DailyDeduction("PV1");
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
        float wantsprice = Mathf.Round(totalcost / producedAmount) + building.getPremium();

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
        //base amount of labor a person generates without any modifiers
        int goodType = building.getGoodType();
        int baselabor = 4;
        float ingredientStock = building.getIngredientStockpile();
        float laborStock = building.getLaborStockpile();
        List<PersonEntry> employees = PeopleQueries.GetAllEmployees(building.getID());
        float[] skillModifiers = GoodsManager.skillModifiers[goodType];
        int unitlabor = GoodsManager.goodsLaborPerUnit[goodType];

        if (GoodsManager.ProductIngredientDict[building.getGoodType()] < 0)
        {
            ingredientStock = 1;
        }

        float weightedAvgDenom = 0;

        foreach (PersonEntry employee in employees)
        {
            //output labor is modified by their skills
            //labor output = 4 * avg(1 + str * modifier + int * modifier + psn * modifier)
            //example 1: if a good has str as its modifier, a worker with 1.5 strength will produce 1.5 more than 1
            //example 2: if a good takes str and int as its modifiers and takes str twice as important, labor output would be 4 * ((str * 2 + int * 1) / 3)

            weightedAvgDenom = 0;
            //str
            weightedAvgDenom += skillModifiers[0];
            //int
            weightedAvgDenom += skillModifiers[1];
            //psn
            weightedAvgDenom += skillModifiers[2];

            laborStock += baselabor * ((employee.getStrength() * skillModifiers[0] + employee.getIntelligence() * skillModifiers[1] + employee.getPersonability() * skillModifiers[2]) / weightedAvgDenom);
        }

        int count = 0;

        int loop = 0;
        //check if there's enough ingredients in stock
        //ingredients are consumed one on one
        while(ingredientStock >= 1 && laborStock >= unitlabor)
        {
            loop++;
            if (loop > 100)
            {
                Debug.LogError("here");
                break;
            }

            ingredientStock -= 1;
            laborStock -= unitlabor;

            count += 1;

            //if the building type does not have an ingredient
            if(GoodsManager.ProductIngredientDict[building.getGoodType()] < 0)
            {
                ingredientStock = 1;
            }
            //subtract from ingredient stockpile as used.
            else
            {
                BuildingsQueries.ChangeIngredientStock(building.getID(), -1);
            }
        }

        BuildingsQueries.SetLaborStockpile(building.getID(), 0);

        Debug.Log(string.Format("Building {0} produced {1} of {2}", building.getID(), count, GoodsManager.TypeToNameDict[building.getGoodType()]));

        return count;
    }

    //Inputs data of a single person. Calculates its preferences and outputs a MarketOrder object with the data.
    //This function is called by processDay for all people that are still participating in the market until there are no participants.
    //Participating means that they have money and that their daily Health and Happiness gained is below the daily deduction amount.
    private MarketOrder GenerateBuyOrderPerson(PersonEntry person)
    {
        //which good to purchase first is the one that provides the most value based on its current health and happiness.
        int wantsgoodtype = GoodsManager.CalculateBestGood(person);

        //Price is determined by following equation: (Asking Price) = (Good��s Heath value) * (PricePerHealth) + (Good��s Happiness value) * (PricePerHappiness)
        float wantsprice = person.getPricePerHealth() * GoodsManager.CalculateHealthGain(wantsgoodtype, person.getHealth()) + person.getPricePerHappiness() * GoodsManager.CalculateHappinessGain(wantsgoodtype, person.getHappiness());

        return new MarketOrder(wantsgoodtype, wantsprice, person.getID());
    }

    
    //for purchasing ingredients
    //price is the average value for the past few days
    //The buyer can also go over the average value based on how well its business is doing
    //Buyers always try to get enough ingredients for a full production.
    private MarketOrder GenerateBuyOrderBuilding(BuildingEntry building)
    {
        //only make amount possible to buy with current funds.
        float askingPrice = building.getAverageIngredientCost();

        return new MarketOrder(GoodsManager.ProductIngredientDict[building.getGoodType()], askingPrice, building.getID());
    }

    private void OperateMarket(string provinceID)
    {
        //make an array of lists that will hold market orders by each good type.
        List<MarketOrder>[] sellOrders = new List<MarketOrder>[GoodsManager.GoodCount];
        for (int i = 0; i < GoodsManager.GoodCount; i++)
        {
            sellOrders[i] = new List<MarketOrder>();
        }

        //make an array of buyers
        List<BuildingEntry> buyerBuildings = BuildingsQueries.GetAllBuildings(provinceID);
        List<PersonEntry> buyerPersons = PeopleQueries.GetAllPeople(provinceID);

        //reset last sales
        BuildingsQueries.ResetLastSales(provinceID);
        //reset last gained
        PeopleQueries.ResetGained(provinceID);

        //fill sell order from buildings
        foreach (BuildingEntry building in BuildingsQueries.GetAllBuildings(provinceID))
        {
            MarketOrder[] sellorders = GenerateSellOrder(building);
            for (int i = 0; i < sellorders.Length; i++)
            {
                MarketOrder order = sellorders[i];

                order.originBuilding = building;
                sellOrders[order.Goodtype].Add(order);

                if(i % GoodsManager.ProductIngredientDict[order.Goodtype] == 0)
                {
                    order.originBuilding.setIngredientStockpile(-1);
                }
            }
        }

        bool sellOrderEmpty = false;

        List<MarketOrder>[] buyOrders;

        int loop = 0;

        //with the sell order list, make transactions until:
        //1. sell order is empty
        //2. no buyers left
        while (!sellOrderEmpty && (buyerBuildings.Count > 0 || buyerPersons.Count > 0))
        {
            loop++;
            if (loop > 100)
            {
                Debug.LogError("here");
                break;
            }

            //Debug.LogError("buyercount: " + buyerPersons.Count + " + " + buyerBuildings.Count);

            if(buyerBuildings.Count <= 0)
            {
                break;
            }
            if(buyerPersons.Count <= 0)
            {
                break;
            }

            buyOrders = new List<MarketOrder>[GoodsManager.GoodCount];
            for (int i = 0; i < GoodsManager.GoodCount; i++)
            {
                buyOrders[i] = new List<MarketOrder>();
            }

            //generate buy orders
            //fill buy order from people
            foreach (PersonEntry person in buyerPersons)
            {
                MarketOrder currentMarketOrder = GenerateBuyOrderPerson(person);
                currentMarketOrder.originPerson = person;
                buyOrders[currentMarketOrder.Goodtype].Add(currentMarketOrder);
            }

            //fill buy order from buildings
            foreach (BuildingEntry building in buyerBuildings)
            {
                MarketOrder order = GenerateBuyOrderBuilding(building);
                order.originBuilding = building;

                //skip if it doesnt require any ingredients
                if(order.Goodtype < 0)
                {
                    continue;
                }

                buyOrders[order.Goodtype].Add(order);
            }



            //operate market for each good type
            for(int i = 0; i < GoodsManager.GoodCount; i++)
            {
                //based on settings, try to make pairs
                if(poorFirst)
                {
                    //if there's more sell orders than buy orders, assign from the back
                    //sort ascending
                    buyOrders[i].Sort((x, y) => x.AskingPrice.CompareTo(y.AskingPrice));
                    sellOrders[i].Sort((x, y) => x.AskingPrice.CompareTo(y.AskingPrice));

                    Debug.Log(string.Format("Process market for {0} with {1} buy orders and {2} sell orders", GoodsManager.TypeToNameDict[i], buyOrders[i].Count, sellOrders[i].Count));

                    int buyindex = 0;
                    int sellindex = 0;
                    int count = 0;

                    while(true)
                    {
                        //Debug.Log(string.Format("iteration good:{0}, count: {1}", GoodsManager.TypeToNameDict[i], count));
                        count++;
                        //exit conditions
                        if(!(buyindex < buyOrders[i].Count && sellindex < sellOrders[i].Count))
                        {
                            break;
                        }

                        if(buyOrders[i][buyindex].AskingPrice >= sellOrders[i][sellindex].AskingPrice)
                        {
                            //make transactions and move both indexes
                            if(Transaction(buyOrders[i][buyindex], sellOrders[i][sellindex]) > 0)
                            {
                                Debug.Log(string.Format("Transaction made\nSeller: {0} Buyer: {1}\nGood type: {2} Price: {3}", sellOrders[i][sellindex].OriginID, buyOrders[i][buyindex].OriginID, i, sellOrders[i][sellindex].AskingPrice));

                                if(buyOrders[i][buyindex].originBuilding == null)
                                {
                                    buyOrders[i][buyindex].originPerson.lastGainedHappiness += GoodsManager.CalculateHappinessGain(i, buyOrders[i][buyindex].originPerson.getHappiness());
                                    buyOrders[i][buyindex].originPerson.lastGainedHealth += GoodsManager.CalculateHealthGain(i, buyOrders[i][buyindex].originPerson.getHealth());
                                }
                                else
                                {
                                    buyOrders[i][buyindex].originBuilding.setIngredientStockpile(1);
                                }

                                sellOrders[i].RemoveAt(0);

                                buyindex++;
                            }
                            //transaction failed due to insufficient funds
                            else
                            {
                                if (buyOrders[i][buyindex].originBuilding == null)
                                {
                                    buyOrders[i][buyindex].originPerson.finishedGoodtypes[i] = true;
                                }

                                //just move buyindex since the buyer is not wealthy enough
                                buyindex++;
                            }
                        }
                        else
                        {
                            if(buyOrders[i][buyindex].originBuilding == null)
                            {
                                buyOrders[i][buyindex].originPerson.finishedGoodtypes[i] = true;
                            }

                            buyindex++;
                        }
                    }

                    //leftover people who couldnt buy anything
                    if(buyindex < buyOrders[i].Count)
                    {
                        for(int j = 0; buyindex + j < buyOrders[i].Count; j++)
                        {
                            if (buyOrders[i][buyindex + j].originBuilding == null)
                            {
                                buyOrders[i][buyindex + j].originPerson.finishedGoodtypes[i] = true;
                            }
                        }
                    }
                }
                else
                {
                    //modified process to prioritize rich people

                }
            }

            //check for satisfied buyers leaving market
            //exit if it filled its ingredient stock or the market for ingredients it wants is empty
            List<int> removeIndex = new List<int>();

            //int exitreasoncode = -1;

            for (int i = 0; i < buyerBuildings.Count; i++)
            {
                int ingredienttype = GoodsManager.ProductIngredientDict[buyerBuildings[i].getGoodType()];


                //full ingredient stock
                if (BuildingsQueries.GetBuilding(buyerBuildings[i].getID()).getIngredientStockpile() >= buyerBuildings[i].getLevel() * 2)
                {
                    //exitreasoncode = 0;
                    removeIndex.Add(i);
                }
                //doesnt want any ingredient. redundant but just in case.
                else if (ingredienttype < 0)
                {
                    //exitreasoncode = 1;
                    removeIndex.Add(i);
                }
                //no ingredient left to buy
                else if(sellOrders[ingredienttype].Count <= 0)
                {
                    //exitreasoncode = 2;
                    removeIndex.Add(i);
                }
                //no money to buy stuff
                else if(buyerBuildings[i].getBudget() <= 0)
                {
                    //exitreasoncode = 3;
                    removeIndex.Add(i);
                }
                //everything is too expensive
                else if(sellOrders[ingredienttype][0].AskingPrice > buyerBuildings[i].getBudget())
                {
                    //exitreasoncode = 4;
                    removeIndex.Add(i);
                }
            }

            int offset = 0;

            foreach(int i in removeIndex)
            {
                //Debug.Log(buyerBuildings[i - offset].getID() + " was removed for " + exitreasoncode + "\n" + buyerBuildings[i-offset].getIngredientStockpile());
                buyerBuildings.RemoveAt(i -offset);
                offset++;
            }

            offset = 0;
            removeIndex = new List<int>();

            //exit if it got 10+ health and happiness this day
            //also exit if they failed to get goods for all types
            //this information is stored as an array of ints in their entries
            for (int i = 0; i < buyerPersons.Count; i++)
            {
                if(buyerPersons[i].lastGainedHealth >= 10 && buyerPersons[i].lastGainedHappiness >= 10)
                {
                    removeIndex.Add(i);
                }
                else
                {
                    PersonEntry person = buyerPersons[i];
                    bool somethingtobuy = false;

                    //if all goods are too expensive
                    for(int j = 0; j < GoodsManager.GoodCount; j++)
                    {
                        //if the person is not finished with a certain good, there exists something to buy
                        if(person.finishedGoodtypes[j] == false)
                        {
                            somethingtobuy = true;
                            break;
                        }
                    }
                    if(somethingtobuy == false)
                    {
                        //Debug.Log("exit 2");
                        removeIndex.Add(i);
                    }
                }
            }

            foreach (int i in removeIndex)
            {
                //Debug.Log(buyerPersons[i - offset].getID() + " was removed");
                buyerPersons.RemoveAt(i - offset);
                offset++;
            }


            //check if sell order for all types is empty
            sellOrderEmpty = true;
            for(int i = 0; i < sellOrders.Length; i++)
            {
                if(sellOrders[i].Count > 0)
                {
                    sellOrderEmpty = false;
                    break;
                }   
            }
        }

        //check for unsold items and refund them 
        for(int i = 0; i < GoodsManager.GoodCount; i++)
        {
            if(sellOrders[i].Count != 0)
            {
                foreach(MarketOrder unsold in sellOrders[i])
                {
                    BuildingsQueries.ChangeIngredientStock(unsold.originBuilding.getID(), 1);
                }    
            }
        }
    }

    private int Transaction(MarketOrder buyOrder, MarketOrder order)
    {
        //exchange goods and money.
        //access the buyer and subtract the asking price of the sell order. give him the good type specified.
        //access the seller and add money to it.
        //remove both orders from the list.
        int outputCode = 0;

        string buyerFirstTwo = (buyOrder.OriginID[0].ToString() + buyOrder.OriginID[1].ToString());

        BuildingEntry seller = BuildingsQueries.GetBuilding(order.OriginID);

        if (buyerFirstTwo == "PP")
        {
            PersonEntry buyer = PeopleQueries.GetPerson(buyOrder.OriginID);

            if (buyer.getMoney() >= order.AskingPrice)
            {
                PeopleQueries.ChangeMoney(buyer.getID(), (-1) * order.AskingPrice);
                PeopleQueries.ChangeHealth(buyer.getID(), GoodsManager.CalculateHealthGain(order.Goodtype, buyer.getHealth()));
                PeopleQueries.ChangeHappiness(buyer.getID(), GoodsManager.CalculateHappinessGain(order.Goodtype, buyer.getHappiness()));

                BuildingsQueries.ChangeMoney(seller.getID(), order.AskingPrice);
                BuildingsQueries.ChangeLastSales(seller.getID(), 1);

                uiManager.InputTransactionHistory(order.Goodtype, buyer.getID(), seller.getID(), order.AskingPrice);

                outputCode = 1;
            }
            else
                outputCode = -1;
        }
        else if (buyerFirstTwo == "BD")
        {
            BuildingEntry buyer = BuildingsQueries.GetBuilding(buyOrder.OriginID);

            if (buyer.getBudget() >= order.AskingPrice)
            {
                BuildingsQueries.ChangeMoney(buyer.getID(), (-1) * order.AskingPrice);
                BuildingsQueries.ChangeAverageIngredientCost(buyer.getID(), order.AskingPrice);
                BuildingsQueries.ChangeIngredientStock(buyer.getID(), 1);

                BuildingsQueries.ChangeMoney(seller.getID(), order.AskingPrice);
                BuildingsQueries.ChangeLastSales(seller.getID(), 1);

                uiManager.InputTransactionHistory(order.Goodtype, buyer.getID(), seller.getID(), order.AskingPrice);

                outputCode = 1;
            }
            else
                outputCode = -1;
        }
        else
        {
            Debug.LogError("Neither person nor building. who tf r u");

            outputCode = -1;
        }

        return outputCode;
    }

    private void OperateLaborMarket(string provinceID)
    {
        int employment = 0;

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
                employment++;
            }

            for(int i = lastremovedindex; i >= 0; i-- )
            {
                people.RemoveAt(i);
            }
        }

        Debug.Log("Employed " + employment.ToString());
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

        //Debug.Log(string.Format("total wage paid by {0} is {1}", building.getID(), total));

        return total;
    }

    private void DailyDeduction(string provinceID)
    {
        //deduct 10 happiness and health everyday
        //cant go under zero
        //cant go over 100

        foreach(PersonEntry person in PeopleQueries.GetAllPeople(provinceID))
        {
            PeopleQueries.ChangeHealth(person.getID(), -10);
            PeopleQueries.ChangeHappiness(person.getID(), -10);
        }
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
         * strength intelligence personability
         */

        foreach(BuildingEntry building in BuildingsQueries.GetAllBuildings(provinceID))
        {
            //goal: maximize profit

            //premium
            //-decrease when the building is losing money. losing money means goods are not selling enough
            //-cannot go under zero. If it is zero and is still losing money, consider reducing the level.
            //if everything is sold, increase.


            //wage
            //-if the maximum employee count is not reached, increase steadily.
            //-cannot go under 1
            //-when the building is making money, decrease very slowly
            //-when the building is losing money, increase.


            //did not hire enough - increase wage to attract workers. also increase premium
            if(building.getLevel() * 2 > PeopleQueries.GetEmployeeCount(building.getID()))
            {
                BuildingsQueries.ChangeWage(building.getID(), 1);
                BuildingsQueries.ChangePremium(building.getID(), 1);
            }
            else
            {
                //full hire but didnt sell all - decrease premium first to reduce price. If premium is at zero, reduce wage.
                if(BuildingsQueries.GetBuilding(building.getID()).getLastSales() < building.getLevel() * 2)
                {
                    //decrease premium. if it is already zero, the fuction fails and returns -1
                    if (BuildingsQueries.ChangePremium(building.getID(), -1) < 0)
                    {
                        BuildingsQueries.ChangeWage(building.getID(), 1);
                    }
                }
                //sold all - increase premium
                else
                {
                    BuildingsQueries.ChangePremium(building.getID(), 1);
                }
            }

            //not enough ingredients?
            if(building.getIngredientStockpile() < building.getLevel() * 2)
            {
                if(building.getAverageIngredientCost() > 0)
                {
                    BuildingsQueries.ChangeIngredientCost(building.getID(), 1);
                }
            }
        }

        foreach (PersonEntry person in PeopleQueries.GetAllPeople(provinceID))
        {
            //price per happiness and health
            //-increase or decrease based on how much is gained that day.
            //--if lower than 10, increase
            //--if same or higher than 10, decrease
            //--how much to increase or decrease is influenced by the difference btw 10 and gained amount
            if(person.lastGainedHealth < 5)
            {
                PeopleQueries.ChangePricePerHealth(person.getID(), 2);
            }
            else if(person.lastGainedHealth < 10)
            {
                PeopleQueries.ChangePricePerHealth(person.getID(), 1);
            }
            else
            {
                PeopleQueries.ChangePricePerHealth(person.getID(), -1);
            }

            if(person.lastGainedHappiness < 5)
            {
                PeopleQueries.ChangePricePerHappiness(person.getID(), 2);
            }
            else if(person.lastGainedHappiness < 10)
            {
                PeopleQueries.ChangePricePerHappiness(person.getID(), 1);
            }
            else
            {
                PeopleQueries.ChangePricePerHappiness(person.getID(), -1);
            }


            //strength and intelligence and personability
            //the level affects work efficiency.
            //as the pop works it gains points and when sufficient amount is gained, it levels up
            //more points are required as the level gets higher.
            //non-productive buildings can 'hire' pops for an education.
            //natural increase only allows up to 1.5
            //it takes 15 days to reach full productivity from 0: increase by total of 0.1
            //how the 0.1 is distributed to the three skills is by the skill multiplyer by type in the goodsmanager
            //if the value is already at 1.5 then the experience is wasted
            //if the person is unemployed, skill is decreased

            float naturalexp = 1.5f;

            if(person.getEmployer() == "NULL")
            {
                //reduce skills
                PeopleQueries.ChangeStrength(person.getID(), -0.033f);
                PeopleQueries.ChangeIntelligence(person.getID(), -0.033f/3);
                PeopleQueries.ChangePersonability(person.getID(), -0.033f/3);
            }
            else
            {
                float[] modifiers = GoodsManager.skillModifiers[BuildingsQueries.GetBuilding(person.getEmployer()).getGoodType()];
                //raise skills based on the appropriate skills
                //divide up 0.1 according to how much it is important in its current job
                //dont go over natural experience cap
                float sum = modifiers[0] + modifiers[1] + modifiers[2];
                float skillnum = 0;
                for (int i = 0; i < 3; i++)
                {
                    if(i == 0)
                    {
                        skillnum = person.getStrength();
                    }
                    else if(i == 1)
                    {
                        skillnum = person.getIntelligence();
                    }
                    else
                    {
                        skillnum = person.getPersonability();
                    }

                    if(skillnum + 0.1f * modifiers[0] / sum > naturalexp)
                    {
                        PeopleQueries.SetSkill(person.getID(), i, naturalexp);
                    }
                    else
                    {
                        if(i == 0)
                        {
                            PeopleQueries.ChangeStrength(person.getID(), 0.1f * modifiers[0] / sum);
                        }
                        else if(i == 1)
                        {
                            PeopleQueries.ChangeIntelligence(person.getID(), 0.1f * modifiers[1] / sum);
                        }
                        else
                        {
                            PeopleQueries.ChangePersonability(person.getID(), 0.1f * modifiers[2] / sum);
                        }
                    }
                }
            }
        }
    }

    private void PopulationGrowth(string provinceID)
    {
        //controls population growth
        //each person generates growth points based on their life status
        //calculates next required growth point for population increase
        //formula for next pop increase req is (pop count) * 8
        //add a new person to the province if growth point is met
        //subtract from growth point and carry left over to next day
        //if leftover is enough for another growth, repeat

        int gainedGrowth = ProvincesQueries.GetProvince(provinceID).getGrowthPoints();
        int popCount = PeopleQueries.GetPopulationCount(provinceID);

        foreach(PersonEntry person in PeopleQueries.GetAllPeople(provinceID))
        {
            gainedGrowth += person.getHealth() / 10;
            gainedGrowth += person.getHappiness() / 10;
        }

        while(true)
        {
            if(gainedGrowth >= popCount * 8)
            {
                //pops dont get removed, so use popcount to make new ID
                popCount++;
                gainedGrowth -= popCount * 8;
                PeopleQueries.AddPerson(new PersonEntry("PP" + popCount.ToString(), provinceID));
            }
            else
            {
                ProvincesQueries.SetGrowthPoints(provinceID, gainedGrowth);
                break;
            }
        }
    }

    public void MakeNewProductionBuilding(string province, int type)
    {
        int buildingcount = BuildingsQueries.GetBuildingCount(province) + 1;

        //need way to generate unique IDs for buildings
        //brainstorming: keep a list of deleted buildings' IDs and use them up til its empty
        //try catch the sql error and keep adding 1 to the ID

        BuildingEntry newbuilding = new BuildingEntry("BD" + buildingcount.ToString(), province, type, 1000, 3, 0, 0, 1, 0, 10, 0);

        BuildingsQueries.AddProductionBuilding(newbuilding);
    }
}

public class MarketOrder
{
    int goodtype;
    float askingPrice;
    string originID;

    public BuildingEntry originBuilding = null;
    public PersonEntry originPerson = null;

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
