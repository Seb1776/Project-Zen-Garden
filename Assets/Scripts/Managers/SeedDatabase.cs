using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedDatabase : MonoBehaviour
{
    [SerializeField] private List<UnlockedSeeds> unlockedSeeds = new List<UnlockedSeeds>();

    public void UnlockPlant(PlantAsset plant, SeedPacket ui)
    {
        UnlockedSeeds newPlant = new UnlockedSeeds(plant, 1, ui);
        unlockedSeeds.Add(newPlant);
    }

    public void BuyPlant(PlantAsset plant)
    {
        if (PlayerOwnsPlant(plant))
        {
            UnlockedSeeds plantTo = GetPlantInList(plant);
            plantTo.amount++;
            plantTo.uiPacket.UpdatePlantAmount();
        }
    }

    public void UsePlant(PlantAsset plant)
    {
        if (PlayerOwnsPlant(plant))
        {   
            UnlockedSeeds plantTo = GetPlantInList(plant);
            plantTo.amount--;
            plantTo.uiPacket.UpdatePlantAmount();
        }
    }

    public bool CanPlant(PlantAsset plant)
    {
        UnlockedSeeds unlocked = null;

        if (PlayerOwnsPlant(plant))
            unlocked = GetPlantInList(plant);

        return PlayerOwnsPlant(plant) && unlocked.amount > 0;
    }

    public bool PlayerOwnsPlant(PlantAsset plant)
    {
        for (int i = 0; i < unlockedSeeds.Count; i++)
            if (unlockedSeeds[i].plant == plant)
                return true;

        return false;
    }

    public UnlockedSeeds GetPlantInList(PlantAsset plant)
    {
        for (int i = 0; i < unlockedSeeds.Count; i++)
            if (unlockedSeeds[i].plant == plant)
                return unlockedSeeds[i];
        
        return null;
    }
}

[System.Serializable]
public class UnlockedSeeds
{
    public PlantAsset plant;
    public SeedPacket uiPacket;
    public int amount;

    public UnlockedSeeds(PlantAsset plant, int amount, SeedPacket uiPacket)
    {
        this.plant = plant;
        this.amount = amount;
        this.uiPacket = uiPacket;
    }
}