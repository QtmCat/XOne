using UnityEngine;
using System.Collections;
using QtmCatFramework;

public class GameRun : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
    {
	    
	}
	

	public void OnBack ()
	{
		AUIManager.OpenDialog ("Prefab/UI/MainMenu");
		AUIManager.CloseDialog ("GameRun");
	}
}
