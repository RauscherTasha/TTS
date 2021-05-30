using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONReader : MonoBehaviour
{
    public TextAsset jsonFile;
    public Recipes recipes;

    public void ReadFromJsonFile()
    {
        Recipes recipesInJson = JsonUtility.FromJson<Recipes>(jsonFile.text);

        foreach (Recipe recipe in recipesInJson.recipes)
        {
            Debug.Log("Found recipe: " + recipe.intro + " " + recipe.recipe);
        }

        recipes = recipesInJson;
    }

}

