using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using UnityEngine;
using System.Runtime.InteropServices;


//https://developer.mozilla.org/en-US/docs/Web/API/Web_Speech_API/Using_the_Web_Speech_API#demo_2
//https://purplesloth.studio/text-to-speech-in-unity-webgl-builds/

 [RequireComponent(typeof(UIDocument))]
public class ButtonTTS : MonoBehaviour
{
	    // this is supplied by TTS.jslib in the plugins folder
    [DllImport("__Internal")]
    private static extern void Speak(string str);
	
	private Button mainMenuButton;
	
	 private TextField mainTextField;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	 public void Say(string line)
    {
        // the jslib only works while in the browser
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Speak(line);
        }
    }
	
	 private void OnMainMenuButton()
        {
           Debug.Log("Speaking: " + mainTextField.value);
		   Speak(mainTextField.value);
        }
		
		 void OnEnable()
        {
            // Retrieving interesting elements to:
            // 1- set values
            // 2- assign behaviors
            // 3- animate!
            var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

            mainMenuButton = rootVisualElement.Q<Button>("main-menu-button");

            // Attaching callback to the button.
            mainMenuButton.RegisterCallback<ClickEvent>(ev => OnMainMenuButton());
			
			mainTextField = rootVisualElement.Q<TextField>("main-text-field");
        }
}



