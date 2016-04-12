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

	public void OnStart ()
	{
		AUIManager.OpenDialog ("Prefab/UI/GameRun");
		AUIManager.CloseDialog ("MainMenu");
	}

	public void OnAbout ()
	{
		AUIManager.OpenDialog ("Prefab/UI/GameAbout");
		AUIManager.CloseDialog ("MainMenu");
	}

	public void OnSetting ()
	{
		AUIManager.OpenDialog ("Prefab/UI/GameSetting");
	}
}
