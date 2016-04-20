using UnityEngine;
using System.Collections;
using QtmCatFramework;

public class MainMenu : MonoBehaviour 
{
	void Start() 
	{
		
	}

	public void OnStart()
	{
		AUIManager.OpenDialog ("Prefab/UI/GameRun");
		AUIManager.CloseDialog ("MainMenu");
	}

	public void OnAbout()
	{
		AUIManager.OpenDialog ("Prefab/UI/GameAbout");
		AUIManager.CloseDialog ("MainMenu");
	}

	public void OnSetting()
	{
		AUIManager.OpenDialog ("Prefab/UI/GameSetting");
	}
}
