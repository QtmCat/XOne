using UnityEngine;
using System.Collections;
using QtmCat;

public class GameAbout : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void BackButtonClick()
    {
        AUIManager.OpenDialog("Prefab/UI/MainMenu");
        AUIManager.CloseDialog("Prefab/UI/GameAbout");
    }
}
