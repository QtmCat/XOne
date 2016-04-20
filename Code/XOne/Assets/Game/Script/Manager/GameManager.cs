using UnityEngine;
using System.Collections;
using QtmCatFramework;

public class GameManager : MonoBehaviour 
{
	// Use this for initialization
	void Start()
	{
		AUIManager.OpenDialog("Prefab/UI/MainMenu");
	}
}
