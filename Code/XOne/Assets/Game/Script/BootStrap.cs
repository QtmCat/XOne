using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public class BootStrap : MonoBehaviour
{
    public string[]      SceneNames;

    private List<string> messageQueue    = new List<string>();

    private GUIContent   combinedMessage = new GUIContent();

    public GUIStyle      style;

    public Texture2D     loadingScreen;

    public GameObject    loadingPrefab;

    public GameObject    firstTimeloadingPrefab;

    public static        BootStrap instance;

    public bool          toStartGame;

    public bool          isFirstTime
    {
        get
        {
            return PlayerPrefs.GetInt("BootStrapFirstTime", 0) == 0;
        }
    }

    private void AppendMessage(string message)
    {
        bool flag = message.StartsWith("\r");

        if (flag)
        {
            message = message.Substring(1);
        }

        if (flag && (this.messageQueue.Count > 0))
        {
            this.messageQueue[this.messageQueue.Count - 1] = message;
        }
        else
        {
            this.messageQueue.Add(message);
        }

        while (true)
        {
            this.combinedMessage.text = (string.Join("\n", this.messageQueue.ToArray()));

            if ((this.messageQueue.Count <= 1) || (this.style.CalcHeight(this.combinedMessage, (float) Screen.width) <= (Screen.height / 5)))
            {
                return;
            }

            this.messageQueue.RemoveAt(0);
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (this.loadingScreen != null)
        {
            UnityEngine.Resources.UnloadAsset(this.loadingScreen);
        }

        this.loadingScreen = null;
    }

    private void OnGUI()
    {
        if (this.loadingScreen != null)
        {
            GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), this.loadingScreen, 0);
        }

        if ((this.messageQueue.Count > 0))
        {
            GUI.Label(new Rect(0f, (float)(Screen.height - (Screen.height / 5)), (float)Screen.width, (float)(Screen.height / 5)), this.combinedMessage, this.style);
        }

        GUI.Label(new Rect(20f, 20f, 300f, 50f), "Version: ", this.style);
    }

    public void Start()
    {
        //MonoBehaviour[] components;

        //GameObject go;
        //IBootstrapPhaseFinished iPhaseFinished;
        //int waitCounter;

        GL.Clear(false, true, Color.black, 1f);
        //if ((FirstTimeloadingPrefab != null) && (PlayerPrefs.GetInt("BootStrapFirstTime") == 0))
        //{
        //    PlayerPrefs.SetInt("BootStrapFirstTime", 1);
        //    PlayerPrefs.Save();
        //    waitCounter = 0;
        //    go = UnityEngine.Object.Instantiate(FirstTimeloadingPrefab) as GameObject;
        //    components = go.GetComponents<MonoBehaviour>();
        //    foreach (var component in components)
        //    {
        //        iPhaseFinished = component as IBootstrapPhaseFinished;
        //        if (iPhaseFinished != null)
        //        {
        //            waitCounter++;
        //            iPhaseFinished.Finished = (Action)Delegate.Combine(iPhaseFinished.Finished, new Action(() => { waitCounter--; }));
        //        }
        //    }

        //    while (waitCounter > 0)
        //    {
        //        yield return null;
        //    }
        //}
        this.toStartGame = false;
        //PlayerPrefs.SetInt("BootStrapFirstTime", 0);
        //if ((FirstTimeloadingPrefab != null) && (PlayerPrefs.GetInt("BootStrapFirstTime", 0) == 0))
        //{
        //    GameObject obj = UnityEngine.Object.Instantiate(FirstTimeloadingPrefab) as GameObject;

        //    while (!startGame)
        //    {
        //        yield return null;
        //    }
        //}


        if (loadingPrefab != null)
        {
            UnityEngine.Object.Instantiate(loadingPrefab);
        }

        //

//        Application.LoadLevelAdditive(1);

        UnityEngine.Object.Destroy(gameObject);
    }

    //private IEnumerator StartloadingScreens()
    //{       
    //    MonoBehaviour[] components;
    //    GameObject go;
    //    IBootstrapPhaseFinished iPhaseFinished;
    //    int waitCounter;

    //    GL.Clear(false, true, Color.black, 1f);
    //    if ((FirstTimeloadingPrefab != null) && (PlayerPrefs.GetInt("BootStrapFirstTime") == 0))
    //    {
    //        PlayerPrefs.SetInt("BootStrapFirstTime", 1);
    //        PlayerPrefs.Save();
    //        waitCounter = 0;
    //        go = UnityEngine.Object.Instantiate(FirstTimeloadingPrefab) as GameObject;
    //        components = go.GetComponents<MonoBehaviour>();
    //        foreach (var component in components)
    //        {
    //            iPhaseFinished = component as IBootstrapPhaseFinished;
    //            if (iPhaseFinished != null)
    //            {
    //                waitCounter++;
    //                iPhaseFinished.Finished = (Action)Delegate.Combine(iPhaseFinished.Finished, new Action(()=>{waitCounter--;}));
    //            }
    //        }

    //        while (waitCounter > 0)
    //        {
    //            yield return null;
    //        } 
    //    }       

    //}

}