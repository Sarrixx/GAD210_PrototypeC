using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CraftingKeywordParser : MonoBehaviour
{
    private static List<KeywordProperty> keywordList = new List<KeywordProperty>() 
    { 
        new KeywordProperty(new string[] { "strength", "strong", "power", "force", "forceful", "power", "powerful", "overwhelming" }, "strength"),
        new KeywordProperty(new string[] { "sword", "blade", "damage", "cutting", "slicing", "sharp", "damaging" }, "damage"),
        new KeywordProperty(new string[] { "fire", "firey", "flame", "flaming", "burning", "smouldering", "smoking", "ash", "embers", "ember", "burns", "burning" }, "fire damage"),
        new KeywordProperty(new string[] { "ice", "icey", "frozen", "cold", "freezing", "frost", "frosty", "shivering" }, "cold damage"),
        new KeywordProperty(new string[] { "arcane", "magic", "enchanted", "mana" }, "arcane damage"),
        new KeywordProperty(new string[] { "healing", "restoring", "blessed", "regenerative", "regeneration", "restoration", "health" }, "healing"),
        new KeywordProperty(new string[] { "defense", "shield", "block" }, "defense"),
        new KeywordProperty(new string[] { "poison", "toxic", "venom" }, "poison damage"),
        new KeywordProperty(new string[] { "electric", "shock", "thunder" }, "electric damage"),
        new KeywordProperty(new string[] { "stealth", "sneak", "invisible", "stealthy", "sneaky" }, "stealth"),
        new KeywordProperty(new string[] { "explosive", "blast", "boom" }, "explosive damage"),
        new KeywordProperty(new string[] { "range", "gun", "bow", "crossbow", "pistol", "rifle", "distance", "long" }, "range"),
        new KeywordProperty(new string[] { "accuracy", "precise", "sharp" }, "accuracy"),
        new KeywordProperty(new string[] { "agility", "nimble", "quick" }, "agility"),
        new KeywordProperty(new string[] { "vampire", "lifesteal", "drain" }, "life steal"),
        new KeywordProperty(new string[] { "holy", "divine", "sacred" }, "holy damage"),
        new KeywordProperty(new string[] { "dark", "shadow", "evil" }, "dark damage"),
        new KeywordProperty(new string[] { "water", "aqua", "wet" }, "water damage"),
        new KeywordProperty(new string[] { "wind", "air", "gust" }, "wind damage"),
        new KeywordProperty(new string[] { "earth", "rock", "stone" }, "earth damage"),
    };

    private string[] names = new string[] 
    {
        "Swift Strong Sword of Fire and Ice",
        "Heavy Durable Blade of Poisonous Strength",
        "Magic Healing Sword of Divine Power",
        "Swift Flame Blade of Arcane Speed",
        "Strong Frost Sword of Healing Regeneration",
        "Massive Venomous Sword of Electric Shock",
        "Speedy Agility Blade with Explosive Damage",
        "Sturdy Luminous Sword of Holy Fire",
        "Frosty Wind Blade of Dark Venom",
        "Arcane Swift Sword with Heavy Defense",
        "Flaming Sword of Poisonous Agility and Speed",
        "Strong Healing Sword with Holy Light",
        "Swift Durable Blade of Toxic Strength",
        "Cold Fire Sword with Magic Power",
        "Massive Shield of Ice and Thunder",
        "Healing Divine Sword of Speedy Strength",
        "Frost Venomous Sword of Agility",
        "Light Swift Blade of Earth and Stone",
        "Heavy Poisonous Sword of Dark Magic",
        "Strong Flaming Sword of Explosive Speed",
        "Flaming Swift Strong Sword of Poisonous Agility and Explosive Arcane Strength",
        "Durable Heavy Blade of Holy Fire, Dark Venom, and Frost",
        "Swift Healing Sword of Magic Power, Electric Shock, and Speed",
        "Sturdy Agility Blade of Swift Ice, Toxic Venom, and Speed",
        "Massive Arcane Sword of Earth and Stone, Healing Regeneration"
    };
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (string name in names)
        {
            ItemProperty[] properties = ParseString(name);
            Item i = new Item(name, properties);
            i.Print();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    ItemProperty[] ParseString(string str)
    {
        List<ItemProperty> properties = new List<ItemProperty>();
        foreach (KeywordProperty keywordProperty in keywordList)
        {
            foreach(string keyword in keywordProperty.keywords)
            {
                if(str.ToLower().Contains(keyword.ToLower()) == true)
                {
                    int propertyIndex = -1;
                    for(int i = 0; i < properties.Count; i++)
                    {
                        if (properties[i].name == keywordProperty.property)
                        {
                            propertyIndex = i;
                            break;
                        }
                    }
                    if (propertyIndex >= 0)
                    {
                        properties[propertyIndex] = new ItemProperty(properties[propertyIndex].name, properties[propertyIndex].Value + 5);
                    }
                    else
                    {
                        properties.Add(new ItemProperty(keywordProperty.property, 5));
                    }
                    continue;
                }
            }
        }
        if(properties.Count > 5)
        {
            while(properties.Count > 5)
            {
                properties.RemoveAt(Random.Range(0, properties.Count));
            }
        }
        return properties.ToArray();
    }
}

public struct Item
{
    public readonly string id;
    public readonly ItemProperty[] properties;

    public Item(string id, ItemProperty[] properties)
    {
        this.id = id;
        this.properties = properties;
    }

    public void Print()
    {
        string output = $"{id}: ";
        foreach(ItemProperty itemProperty in properties)
        {
            output += $"{itemProperty.name.ToUpper()} {itemProperty.Value}\n";
        }
        Debug.Log(output);
    }
}

/// <summary>
/// e.g. swift, flame, etc.
/// </summary>
public struct KeywordProperty
{
    public readonly string[] keywords;
    public readonly string property;

    public KeywordProperty(string[] keywords, string property)
    {
        this.keywords = keywords;
        this.property = property;
    }
}

/// <summary>
/// e.g. speed, strength, agility, etc.
/// </summary>
public struct ItemProperty
{
    public readonly string name;
    public float Value { get; set; }

    public ItemProperty(string propertyID, float propertyValue)
    {
        name = propertyID;   
        Value = propertyValue;
    }
}