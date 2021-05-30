
public enum RecipeState
{
    NotBegun,
    Running,
    Done
}

[System.Serializable]
public class Recipe
{
   

    public string intro;
    public string recipe;
    public string outro;

    private int recipeIndex = 0;
    private string[] recipeLines = null;

    public RecipeState state = RecipeState.NotBegun;


    public string[] getIntroLines()
    {
        return intro.Split(System.Environment.NewLine.ToCharArray());
    }

    public string[] getOutroLines()
    {
        return outro.Split(System.Environment.NewLine.ToCharArray());
    }

    public string[] getRecipeLines()
    {
        
        if (recipeLines == null)
        {
            recipeLines = recipe.Split(System.Environment.NewLine.ToCharArray());
        }

        return recipeLines;
    }

    public string getCurrentRecipeLine()
    {
        string[] lines = getRecipeLines();
        if (recipeIndex >= lines.Length) recipeIndex = lines.Length - 1;
        return lines[recipeIndex];

    }

    public string getNextRecipeLine()
    {
        recipeIndex++;
        return getCurrentRecipeLine();
    }

    public bool isLastRecipeLine()
    {
        return recipeIndex >= recipeLines.Length - 1;
    }

}
