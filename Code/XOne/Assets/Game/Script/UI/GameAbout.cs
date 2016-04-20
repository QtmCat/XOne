using UnityEngine;
using System.Collections;
using QtmCatFramework;

public class GameAbout : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{

	}

	public void BackButtonClick()
	{
		AUIManager.OpenDialog("Prefab/UI/MainMenu");
		AUIManager.CloseDialog("GameAbout");
	}
}
