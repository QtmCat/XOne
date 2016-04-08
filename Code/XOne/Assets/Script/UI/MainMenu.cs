using UnityEngine;
using System.Collections;
using QtmCat;

public class MainMenu : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        Debug.LogError("1234569");
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void StartButtonClick()
    {
        Debug.LogError("1234569");
        ADebug.Log("123");
    }

    public void AboutButtonClick()
    {
        AUIManager.CloseDialog("Prefab/UI/MainMenu");
        AUIManager.OpenDialog("Prefab/UI/GameAbout");
    }
}
