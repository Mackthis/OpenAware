using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientListScreen : MonoBehaviour
{
    [SerializeField] private IngredientListManager ingredientListManager;

    // the objects drawn on screen to display the list
    List<GameObject> ingredientObjects = new();
    GameObject addIngredientObj;

    // (assigned within unity)
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject listItemObject;
    [SerializeField] private GameObject listTitle;
    [SerializeField] private GameObject addIngredientButton;
    [SerializeField] private GameObject saveButton;
    [SerializeField] private Transform scrollViewContent;

    private IngredientList currentIngredientList;
    private void OnEnable()
    {
        currentIngredientList = ingredientListManager.IngredientLists[ingredientListManager.CurrentListIndex];

        backButton.SetActive(true);
        Button backB = backButton.GetComponent<Button>();
        backB.onClick.AddListener(delegate { OnBackButtonClick(); });

        Button saveB = saveButton.GetComponent<Button>();
        saveB.onClick.AddListener(delegate { OnSaveButtonClick(); });


        DisplayList();
    }

    private void OnDisable()
    {
        RemoveIngredientObjects();
    }

    private void OnSaveButtonClick()
    {
        ingredientListManager.CloseList();
    }

    private void DisplayList()
    {
        RemoveIngredientObjects();

        // display list name
        listTitle.GetComponent<TMP_InputField>().text = currentIngredientList.ListName;

        Dictionary<Ingredient, float> ingredients = currentIngredientList.Ingredients;

        // display each ingredient
        for (int i = 0; i < currentIngredientList.Ingredients.Count; i++)
        {
            // create a new list item to display this ingredient
            GameObject listItem = Instantiate(listItemObject, scrollViewContent);
            listItem.SetActive(true);

            // change the text to match the ingredient info
            Button ingredientButton = listItem.transform.GetChild(0).GetComponent<Button>();
            Button delButton = listItem.transform.GetChild(1).GetComponent<Button>();

            Ingredient ingredient = ingredients.ElementAt(i).Key;
            ingredientButton.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = ingredient.Name;
            ingredientButton.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = ingredients.ElementAt(i).Value.ToString();
            ingredientButton.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = ingredient.Type.ToString();
            ingredientObjects.Add(listItem);

            int itemIndex = i;
            ingredientButton.onClick.AddListener(() => { OnIngredientButtonClick(itemIndex); });

            // create a deleteButton for this ingredient
            delButton.onClick.AddListener(() => { OnDeleteButtonClick(ingredient); });
        }
        //Add the "Add ingredient" button to the bottom of the list.
        addIngredientObj = Instantiate(addIngredientButton, scrollViewContent);
        addIngredientObj.SetActive(true);
        Button addIngredientBtn = addIngredientObj.GetComponent<Button>();
        addIngredientBtn.onClick.AddListener(() => { OnAddIngredientButtonClick(); });
    }

    /// <summary>
    /// removes all objects from the scrollview
    /// </summary>
    private void RemoveIngredientObjects()
    {
        foreach (GameObject o in ingredientObjects)
        {
            Destroy(o);
        }
        Destroy(addIngredientObj);
        ingredientObjects = new List<GameObject>();
    }

    private void OnIngredientButtonClick(int index)
    {
        ingredientListManager.OpenIngredientScreen(index);
    }

    public void OnDeleteButtonClick(Ingredient ingredient)
    {
        ingredientListManager.DeleteIngredient(ingredient);
        DisplayList();
    }

    private void OnBackButtonClick()
    {
        ingredientListManager.CloseList();
    }

    private void OnAddIngredientButtonClick()
    {
        ingredientListManager.OpenSearchScreen();
        //ingredientListManager.AddIngredient(new Ingredient("banana", QuantityType.PCS, 3));
        //DisplayList();
    }
}
