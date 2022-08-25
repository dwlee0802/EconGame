using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonEntry
{
    string ID;
    string province;

    float money;
    int health;
    int happiness;

    float strenghth;
    float intelligence;
    float personability;

    float pricePerHealth;
    float pricePerHappiness;

    string employer;

    public int lastGainedHealth = 0;
    public int lastGainedHappiness = 0;

    public bool[] finishedGoodtypes = new bool[GoodsManager.GoodCount];

    public string getID()
    {
        return ID;
    }

    public string getProvince()
    {
        return province;
    }
    public int getHealth()
    {
        return health;
    }

    public int getHappiness()
    {
        return happiness;
    }

    public float getPricePerHealth()
    {
        return pricePerHealth;
    }

    public float getPricePerHappiness()
    {
        return pricePerHappiness;
    }

    public float getStrength()
    {
        return strenghth;
    }

    public float getIntelligence()
    {
        return intelligence;
    }
    public float getPersonability()
    {
        return personability;
    }

    public float getMoney()
    {
        return money;
    }

    public string getEmployer()
    {
        return employer;
    }

    public PersonEntry(string iD, string province, float money = 0f, int health = 0, int happiness = 0, float strenghth = 0f, float intelligence = 0f, float pricePerHealth = 1, float pricePerHappiness = 1, string employer = "NONE", float personability = 0f)
    {
        ID = iD;
        this.province = province;
        this.money = money;
        this.health = health;
        this.happiness = happiness;
        this.strenghth = strenghth;
        this.intelligence = intelligence;
        this.pricePerHealth = pricePerHealth;
        this.pricePerHappiness = pricePerHappiness;
        this.employer = employer;
        this.personability = personability;
    }
    public override string ToString()
    {
        return string.Format(
            ("ID: {0} Province: {1} Money: {2}\n" +
            "Health: {3} Happiness: {4}\n" +
            "Str: {5} Int: {6} PSB: {7}\n"), ID, province, money, health, happiness, strenghth, intelligence, personability);
    }
}
