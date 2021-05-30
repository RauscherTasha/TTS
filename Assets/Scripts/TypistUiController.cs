using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Linq;

public enum GameState
{
    STARTUP,
    TUTORIAL,
    RUNNING,
    GAME_OVER,
    WIN
}

//https://developer.mozilla.org/en-US/docs/Web/API/Web_Speech_API/Using_the_Web_Speech_API#demo_2
//https://purplesloth.studio/text-to-speech-in-unity-webgl-builds/

[RequireComponent(typeof(UIDocument))]


public class TypistUiController : MonoBehaviour
{
    private Queue<string> talkQueue = new Queue<string>();
    public int typistBotNumber = 9000;
    private GameState gameState;
    private Recipe[] recipes;
    private Recipe currentRecipe;
    private int level = 0;
    public int successfulLevelsToWin = 3;
    private int mistakes = 0;
    private float elapsed4s = 0f;
    private float elapsed10s = 0f;
    public float timePerRecipe = 90f;
    private float remainingTime;
    private bool clocksTicking = false;

    public int maxAllowedMistakes = 30;
    private int successfulSessions = 0;

    // this is supplied by TTS.jslib in the plugins folder
    [DllImport("__Internal")]
    private static extern void Speak(string str);
    [DllImport("__Internal")]
    private static extern void InitVoices();

    [DllImport("__Internal")]
    private static extern void StopTTS();

    private Button submitBtn;

    private TextField inputTxt;
    private TextField submittedTxt;
    private TextField serverTxt;
    private ScrollView scrollView;

    private JSONReader jsonReader;

    // Start is called before the first frame update
    void Start()
    {
        jsonReader = GetComponent<JSONReader>();

        reset();
    }

    private string getVersionString()
    {
        return String.Format("v{0:0.0}", Math.Round((double)successfulSessions / 10,1));
    }

    private void StopTTSSafely()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            StopTTS();
        }
    }

    private void reset()
    {
        // reload recipes
        jsonReader.ReadFromJsonFile();
        recipes = jsonReader.recipes.recipes;

        System.Random rnd = new System.Random();
        recipes = recipes.OrderBy(x => rnd.Next()).ToArray();
        recipes = recipes.Take(successfulLevelsToWin).ToArray();


        StopTTSSafely();
        
        // select recipes to be typed
        serverTxt.value = "";
        submittedTxt.value = "type 'login' to begin\nuse 'enter' to submit";
        
        level = 0;
        mistakes = 0;
        clocksTicking = false;
        gameState = GameState.STARTUP;
        talkQueue.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            OnSubmitButton();
        }
        else if (Input.GetKeyUp(KeyCode.Backspace))
        {
            if(inputTxt.value.Length > 0)
            {
                inputTxt.value = inputTxt.value.Remove(inputTxt.value.Length - 1);
            }
        }
        else if (Input.anyKeyDown)
        {
            if (Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Return)) return;
            inputTxt.value += Input.inputString;

        }

        elapsed4s += Time.deltaTime;
        if (elapsed4s >= 4f)
        {
            elapsed4s = elapsed4s % 4f;
            if(gameState == GameState.GAME_OVER)
            {
                SayAndType("Game Over!");
            }

            if (talkQueue.Count > 0)
            {
                SayAndType(talkQueue.Dequeue());
            }
            else
            {
                if(gameState == GameState.TUTORIAL)
                {
                    gameState = GameState.RUNNING;
                    startNextRecipe();
                }
            }
        }

        if (clocksTicking)
        {
            elapsed10s += Time.deltaTime;
            if (elapsed10s >= 10f)
            {
                remainingTime -= elapsed10s;
                if (remainingTime <= 0)
                {
                    clocksTicking = false;
                    gameState = GameState.GAME_OVER;
                    gameOverTimesUp();
                }
                Say((int)Math.Round(remainingTime, 0) + " seconds left");
                elapsed10s = elapsed10s % 10f;
            }
        }
       


    }


    private void startNextRecipe()
    {

        currentRecipe = recipes[level];
        submittedTxt.value = "";
        type("\n\n#################\n");

        foreach (string s in currentRecipe.getIntroLines())
        {
            SayAndType(s + "\n");
        }

       
        type("\n\n");


        string currentLine = currentRecipe.getCurrentRecipeLine();
        SayAndType(currentLine + "\n");

        currentRecipe.state = RecipeState.Running;
        remainingTime = timePerRecipe;
        elapsed10s = 0f;
        clocksTicking = true;
    }

    private bool processInput(string input)
    {
        int distance = LevenshteinDistance.Compute(currentRecipe.getCurrentRecipeLine(), input);
        mistakes += distance;
        if(distance > 0)
        {
            type("WARNING: You made " + distance + " additional mistakes (total " + mistakes + " of max " + maxAllowedMistakes + ")");
        }
        Debug.Log("Total number of mistakes: " + distance);

        input += "\n";
        if(submittedTxt.value.Length == 0)
        {
            input += "###\n";
        }
        submittedTxt.value += input;

        if (mistakes > maxAllowedMistakes)
        {
            gameOverMistakes();
            return false;
        }
        return true;
    }

    private void continueRecipeDictation(string input)
    {
        if (!processInput(input))
        {
            return;
        }

        // server speaks next line
        string nextLine = currentRecipe.getNextRecipeLine();
        
        SayAndType(nextLine + "\n");
    }

    private void finishRecipeDictation(string input)
    {
        if (!processInput(input))
        {
            return;
        }

        // server speaks the outro
        foreach (string s in currentRecipe.getOutroLines())
        {
            SayAndType(s + "\n");
        }
        type("\n");

        level++;

        if (level == successfulLevelsToWin)
        {
            gameOverWin();
            return;
        }

        // Start next round/recipe dictation
        startNextRecipe();
    }

    private void gameOverWin()
    {
        clocksTicking = false;
        gameState = GameState.WIN;
        type("\n");
        successfulSessions++;
        SayAndType("Transscribed recipes: " + level);
        SayAndType("Mistakes made: " + mistakes);
        if (successfulSessions > 0)
        {
            SayAndType("Consecutive successful sessions: " + successfulSessions);
        }
        SayAndType("Congratulations,\nyour work was acceptable\nyou may 'logout' now");
    }

    private void gameOverMistakes()
    {
        processGameOver("Game Over!\nToo many mistakes\nType 'reset' to restore your factory settings and try again");
    }

    private void gameOverTimesUp()
    {
        processGameOver("Game Over!\nToo slow\nType 'reset' to restore your factory settings and try again");
    }

    private void processGameOver(String gameOverText)
    {
        StopTTSSafely();
        type("\n");
        clocksTicking = false;
        gameState = GameState.GAME_OVER;
        successfulSessions = 0;
        submittedTxt.value = gameOverText;
        SayAndType(gameOverText);
    }

    private void provideTutorial()
    {
        gameState = GameState.TUTORIAL;

        submittedTxt.value = "Pay attention to the\nbenevolent server\n(or 'skip')";

        talkQueue.Enqueue("Hello typist-bot number " + typistBotNumber + " " + getVersionString());
        talkQueue.Enqueue("It is me, your benevolent Server");
        if (successfulSessions > 0)
        {
            talkQueue.Enqueue("You know the drill, so let's get right to it");
        }
        else
        {
            talkQueue.Enqueue("Your purpose is to transscribe recipes");
            talkQueue.Enqueue("Make too many mistakes and you will be reset");
            talkQueue.Enqueue("If you are too slow, you will be reset");
            talkQueue.Enqueue("Type 'reset' to reset on your own");
            talkQueue.Enqueue("Hit 'enter' to submit your input");
            talkQueue.Enqueue("Dictation will start soon, beware of bugs");
            talkQueue.Enqueue("Have ''fun''");
        }
   
    }

    private void OnSubmitButton()
    {
        string text = inputTxt.value;
        Debug.Log(text);

        if (text != "")
        {
            if (text == "reset")
            {
                typistBotNumber++;
                reset();
                // shuffle recipes
            }else if (text == "login" && gameState == GameState.STARTUP)
            {
                // special login/tutorial stuff
                provideTutorial();
            }
            else if(gameState == GameState.RUNNING)
            {
                handleInputBasedOnRecipeState(text);
            }else if(gameState == GameState.WIN && text == "logout")
            {
                reset();
            }else if(gameState == GameState.TUTORIAL && text == "skip")
            {
                talkQueue.Clear();
                talkQueue.Enqueue("How rude");
            } 
            inputTxt.value = "";

        }
       
    }

    private void handleInputBasedOnRecipeState(string input)
    {
        if (currentRecipe.state == RecipeState.Running && currentRecipe.isLastRecipeLine())
        {
            currentRecipe.state = RecipeState.Done;
        }

            switch (currentRecipe.state)
        {
            case RecipeState.NotBegun:
                startNextRecipe();
                break;
            case RecipeState.Running:
                continueRecipeDictation(input);
                break;
            case RecipeState.Done:
                finishRecipeDictation(input);
                break;
            default:
                throw new Exception("Recipe state not recognized");
        }
    }

    public void SayAndType(string line)
    {
        Say(line);
        type(line);
    }

    private void type(string line)
    {
        serverTxt.value = line + "\n" + serverTxt.value;
    }

    public void Say(string line)
    {
        Debug.Log("Server speaks: " + line);

        // the jslib only works while in the browser
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Speak(line);
        }
    }

    void OnEnable()
    {
        // the jslib only works while in the browser
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            InitVoices();
        }

        // Retrieving interesting elements to:
        // 1- set values
        // 2- assign behaviors
        // 3- animate!
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        submitBtn = rootVisualElement.Q<Button>("submit-btn");

        // Attaching callback to the button.
        submitBtn.RegisterCallback<ClickEvent>(ev => OnSubmitButton());

        inputTxt = rootVisualElement.Q<TextField>("input-txt");
        submittedTxt = rootVisualElement.Q<TextField>("submitted-txt");
        serverTxt = rootVisualElement.Q<TextField>("server-txt");
    }
}
