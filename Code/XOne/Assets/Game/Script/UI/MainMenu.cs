using UnityEngine;
using System.Collections;
using QtmCatFramework;

public class MainMenu : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{
		
	}

	// Update is called once per frame
	void Update () 
	{

	}

	public void StartButtonClick()
	{
		AUIManager.OpenDialog ("Prefab/UI/GameRun");
		AUIManager.CloseDialog ("MainMenu");
	}

	public void AboutButtonClick()
	{
		AUIManager.OpenDialog("Prefab/UI/GameAbout");
		AUIManager.CloseDialog("MainMenu");
	}

	public void SettingButtonClick()
	{
		AUIManager.OpenDialog ("Prefab/UI/GameSetting");
	}
}
