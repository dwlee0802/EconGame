using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    /*
     * To do list:
     * Add growth from previous day
     */
    [SerializeField]
    GameObject provinceMenuPanel;

    [SerializeField]
    Text provinceName;

    string currentProvinceID = "PV1";

    //overview view holds important information from other views in one place. Index 0
    [SerializeField]
    GameObject overviewView;
    //demographics view shows the information about people in the province. Index 1
    [SerializeField]
    GameObject demographicsView;
    //industry view shows information about buildings in the province. Index 2
    [SerializeField]
    GameObject industryView;
    //market view shows good transactions made within the province. Index 3
    [SerializeField]
    GameObject marketView;
    [SerializeField]
    Dropdown marketGoodtypeDropdown;
    [SerializeField]
    GameObject transactionEntryPrefab;

    List<TransactionEntry>[] transactionHistory = new List<TransactionEntry>[GoodsManager.GoodCount];

    private void Awake()
    {
        InitializeTransactionHistory();
    }

    private void InitializeTransactionHistory()
    {
        for (int i = 0; i < GoodsManager.GoodCount; i++)
        {
            transactionHistory[i] = new List<TransactionEntry>();
        }
    }

    public void NextTurn()
    {
        InitializeTransactionHistory();
    }

    public void ChangeProvinceView(int viewIndex)
    {
        overviewView.SetActive(false);
        demographicsView.SetActive(false);
        industryView.SetActive(false);
        marketView.SetActive(false);

        switch(viewIndex)
        {
            case 0:
                overviewView.SetActive(true);
                LoadOverviewView();
                break;
            case 1:
                demographicsView.SetActive(true);
                LoadDemographicsView();
                break;
            case 2:
                industryView.SetActive(true);
                LoadIndustryView();
                break;
            case 3:
                marketView.SetActive(true);
                LoadMarketView();
                break;
        }
    }

    void LoadOverviewView()
    {

    }

    void LoadDemographicsView()
    {
        Transform averageWealth = demographicsView.transform.GetChild(0).GetChild(0);
        Transform averageHealth = demographicsView.transform.GetChild(0).GetChild(2);
        Transform averageHappiness = demographicsView.transform.GetChild(0).GetChild(3);
        Transform populationCount = demographicsView.transform.GetChild(0).GetChild(4);
        Transform averageStrength = demographicsView.transform.GetChild(0).GetChild(6);
        Transform averageIntelligence = demographicsView.transform.GetChild(0).GetChild(7);
        Transform averagePersonability = demographicsView.transform.GetChild(0).GetChild(8);
        float totalWealth = 0;
        float totalHealth = 0;
        float totalHappiness = 0;
        float totalStrength = 0;
        float totalIntelligence = 0;
        float totalPersonability = 0;
        int count = 0;

        foreach(PersonEntry person in PeopleQueries.GetAllPeople(currentProvinceID))
        {
            count++;
            totalWealth += person.getMoney();
            totalHealth += person.getHealth();
            totalHappiness += person.getHappiness();
            totalStrength += person.getStrength();
            totalIntelligence += person.getIntelligence();
            totalPersonability += person.getPersonability();
        }

        averageWealth.GetComponent<Text>().text = "Avg Wealth: " + (totalWealth / count).ToString("F2");
        averageHealth.GetComponent<Text>().text = "Avg Health: " + (totalHealth / count).ToString("F2");
        averageHappiness.GetComponent<Text>().text = "Avg Happiness: " + (totalHappiness / count).ToString("F2");
        populationCount.GetComponent<Text>().text = "Population:: " + count;
        averageStrength.GetComponent<Text>().text = "Avg STR: " + (totalStrength / count).ToString("F2");
        averageIntelligence.GetComponent<Text>().text = "Avg INT: " + (totalIntelligence / count).ToString("F2");
        averagePersonability.GetComponent<Text>().text = "Avg PSN: " + (totalPersonability / count).ToString("F2");
    }

    void LoadIndustryView()
    {

    }

    public void LoadMarketView()
    {
        int showforgoodtype = marketGoodtypeDropdown.value;
        Transform transactionsContent = marketView.transform.GetChild(4).GetChild(0).GetChild(0).GetChild(0);

        //clear content
        int childcount = transactionsContent.childCount;
        for(int i = 0; i < childcount; i++)
        {
            Destroy(transactionsContent.GetChild(childcount - 1 - i).gameObject);
        }

        //Debug.Log(transactionHistory[showforgoodtype].Count);

        foreach(TransactionEntry entry in transactionHistory[showforgoodtype])
        {
            GameObject transactionEntry = Instantiate(transactionEntryPrefab);

            //buyerID
            transactionEntry.transform.GetChild(0).GetComponent<Text>().text = entry.buyerID;
            //sellerID
            transactionEntry.transform.GetChild(1).GetComponent<Text>().text = entry.sellerID;
            //price
            transactionEntry.transform.GetChild(2).GetComponent<Text>().text = entry.price.ToString();

            transactionEntry.transform.SetParent(transactionsContent);
        }
    }

    public void InputTransactionHistory(int goodtype, string buyerID, string sellerID, float price)
    {
        //Debug.Log("UI Manager logged transaction with type " + GoodsManager.TypeToNameDict[goodtype] + " of price " + price);
        transactionHistory[goodtype].Add(new TransactionEntry(goodtype, buyerID, sellerID, price));
    }

    //0 for buyerID, 1 for SellerID, 2 for price
    public void SortTransactionHistory(int command)
    {
        if(command == 0)
        {

        }
        else if (command == 1)
        {

        }
        else if(command == 2)
        {

        }
    }
}

public class TransactionEntry
{
    public int day;
    public int goodtype;
    public string buyerID;
    public string sellerID;
    public float price;

    public TransactionEntry(int _goodtype, string _buyerID, string _sellerID, float _price, int _day = 0)
    {
        day = _day;
        goodtype = _goodtype;
        buyerID = _buyerID;
        sellerID = _sellerID;
        price = _price;
    }
}
