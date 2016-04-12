using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using QtmCatFramework;

public class GameSetting : MonoBehaviour {

	public static bool IsMusicOn;
	public static bool IsSoundOn;

	public Text musicText;
	public Text soundText;

	// Use this for initialization
	void Start () {
		IsMusicOn = Convert.ToBoolean(PlayerPrefs.GetString ("IsMusicOn", "true"));
		IsSoundOn = Convert.ToBoolean(PlayerPrefs.GetString ("IsSoundOn", "true"));

		musicText.text = IsMusicOn ? "关闭" : "打开";
		soundText.text = IsSoundOn ? "关闭" : "打开";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void BackButtonClick()
	{
		AUIManager.CloseDialog("GameSetting");
	}

	public void MusicButtonClick()
	{
		if (IsMusicOn) 
		{
			IsMusicOn = false;
		} 
		else 
		{
			IsMusicOn = true;
		}
		musicText.text = IsMusicOn ? "关闭" : "打开";
	}

	public void SoundButtonClick()
	{
		if (IsSoundOn) 
		{
			IsSoundOn = false;
		} 
		else 
		{
			IsSoundOn = true;
		}
		soundText.text = IsSoundOn ? "关闭" : "打开";
	}
}
