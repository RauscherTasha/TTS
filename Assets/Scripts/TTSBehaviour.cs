using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class TTSBehaviour : MonoBehaviour
{
    // this is supplied by TTS.jslib in the plugins folder
    [DllImport("__Internal")]
    private static extern void Speak(string str);


    // ...

    public void Say(string line)
    {
        // the jslib only works while in the browser
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Speak(line);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
