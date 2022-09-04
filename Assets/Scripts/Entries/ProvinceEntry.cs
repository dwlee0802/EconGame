using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvinceEntry
{
    string ID;
    string name;
    string nation;
    int growthPoints;

    public int getGrowthPoints()
    {
        return growthPoints;
    }

    public ProvinceEntry(string iD, string name, string nation, int growthPoints = 0)
    {
        ID = iD;
        this.name = name;
        this.nation = nation;
        this.growthPoints = growthPoints;
    }
}
