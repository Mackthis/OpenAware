using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;
using UnityEngine.InputSystem;
//using System.Text.Json;
//using System.Text.Json.Serialization;

public class IngredientListManager : MonoBehaviour
{
    public List<IngredientList> IngredientLists { get; private set; }

    //public IngredientList { get; private set; } currentIngredientList;
    public int CurrentListIndex { get; private set; } = -1;
    public Ingredient CurrentIngredient { get; private set; }

    // objects assigned within unity
    [SerializeField] private GameObject listsOverviewScreen;
    [SerializeField] private GameObject ingredientListScreen;
    [SerializeField] private GameObject addIngredientScreen;
    [SerializeField] private GameObject ingredientScreen;
    
    IIngredientDatabase ingredientDatabase;
    MaterialCalculator materialCalculator; // for now only used to check whether the conversion works; probably to be removed later

    string filePath;

    private void Awake()
    {
        ingredientDatabase = new MockupIngredientDatabase();
        materialCalculator = new MaterialCalculator();
        filePath = Application.persistentDataPath + "/ingredientLists";
        IngredientLists = ReadFile();
        listsOverviewScreen.SetActive(true);
    }

    List<IngredientList> ReadFile()
    {
        if (!File.Exists(filePath))
        {
            return new List<IngredientList>();
        }
            
        string json = File.ReadAllText(filePath);

        JSONIngredientInfo info = JsonUtility.FromJson<JSONIngredientInfo>(json);

        List<IngredientList> lists = new();
        
        // reconstruct all lists
        for (int i = 0; i < info.listNames.Length; i++)
        {
            Dictionary<Ingredient, float> ingredients = new();

            string[] ingredientIDs;
            string[] ingredientQuantities;

            try
            {
                ingredientIDs = info.ingredientIDs[i].Split(",");
                ingredientQuantities = info.ingredientQuantities[i].Split(",");
            }
            catch (System.NullReferenceException)
            {
                Debug.LogWarning("IngredientLists file is not in correct format. Lists will be deleted");
                File.Delete(filePath); // use this for emptying the saved ingredientLists
                return lists;
            }

            // add the ingredients to the lists
            for (int j = 0; j < ingredientIDs.Length - 1; j++)
            {
                int ingredientID = int.Parse(ingredientIDs[j]);
                float ingredientQuantity = float.Parse(ingredientQuantities[j]);

                ingredients.Add(ingredientDatabase.GetIngredient(ingredientID), ingredientQuantity);
            }
            lists.Add(new IngredientList(info.listNames[i], ingredients));
        }

        return lists;
    }

    public void SaveFile()
    {
        JSONIngredientInfo info = new JSONIngredientInfo();
        
        info.listNames = new string[IngredientLists.Count];
        info.ingredientIDs = new string[IngredientLists.Count];
        info.ingredientQuantities = new string[IngredientLists.Count];

        // convert the lists to strings
        for (int i = 0; i < IngredientLists.Count; i++)
        {
            info.listNames[i] = IngredientLists[i].ListName;

            info.ingredientIDs[i] = "";
            info.ingredientQuantities[i] = "";

            for (int j = 0; j < IngredientLists[i].NumberOfIngredients(); j++)
            {
                Ingredient ingredient = IngredientLists[i].Ingredients.ElementAt(j).Key;
                info.ingredientIDs[i] += ingredient.ID + ",";
                info.ingredientQuantities[i] += IngredientLists[i].Ingredients[ingredient] + ",";
            }
        }

        string json = JsonUtility.ToJson(info);

        File.WriteAllText(filePath, json);
    }

    public void OpenList(int i)
    {
        addIngredientScreen.SetActive(false);
        ingredientScreen.SetActive(false);
        listsOverviewScreen.SetActive(false);
        CurrentListIndex = i;
        ingredientListScreen.SetActive(true);

        /* // code for checking the ingredientList to materialList conversion 
        foreach(KeyValuePair<ProductMaterial,float> k in materialCalculator.IngredientsToMaterials(ingredientLists[i]).Materials)
            Debug.Log("Material: " + k.Key.ID.ToString() + "; Quantity: " + k.Value.ToString());
        */
    }

    public void CloseList()
    {
        ingredientListScreen.SetActive(false);
        listsOverviewScreen.SetActive(true);
        SaveFile();
    }

    public void OpenSearchScreen()
    {
        ingredientListScreen.SetActive(false);
        addIngredientScreen.SetActive(true);
    }

    public void OpenIngredientScreen(int itemIndex) 
    {
        ingredientListScreen.SetActive(false);
        addIngredientScreen.SetActive(false);
        //Debug.Log(" setting current ingredient to " + itemIndex);
        CurrentIngredient = IngredientLists[CurrentListIndex].Ingredients.ElementAt(itemIndex).Key;
        ingredientScreen.SetActive(true);
    }

    public void AddIngredient(Ingredient ingredient, float quantity)
    {
        IngredientLists[CurrentListIndex].AddIngredient(ingredient, quantity);
        SaveFile();
    }

    public void DeleteIngredient(Ingredient ingredient)
    {
        IngredientLists[CurrentListIndex].RemoveIngredient(ingredient);
        SaveFile();
    }

    public void CreateList()
    {
        // TODO: let user pick list name --> add seperate screen

        // adds four ingredients to the list for testing (to be removed later!)
        Dictionary<Ingredient, float> testList = new()
        {
            { ingredientDatabase.GetIngredient(0), 2 },
            { ingredientDatabase.GetIngredient(1), 200 },
            { ingredientDatabase.GetIngredient(4), 500 },
            { ingredientDatabase.GetIngredient(5), 300 }
        };

        IngredientLists.Add(new IngredientList("MyList", testList));
        SaveFile();
    }

    public void DeleteList(int i)
    {
        IngredientLists.Remove(IngredientLists[i]);
        SaveFile();
    }
}

[Serializable]
public class JSONIngredientInfo
{
    public string[] listNames;
    public string[] ingredientIDs;
    public string[] ingredientQuantities;
}
