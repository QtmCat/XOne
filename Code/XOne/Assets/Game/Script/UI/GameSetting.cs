using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using QtmCatFramework;

public class GameSetting : MonoBehaviour 
{
	public Text musicText;
	public Text soundText;

	void Start() 
    {
        this.Populate();
	}

    private void Populate()
    {
        this.musicText.text = SoundManager.instance.isMusicOn ? "关闭" : "打开";
        this.soundText.text = SoundManager.instance.isSoundOn ? "关闭" : "打开";
    }

	public void OnBack()
	{
		AUIManager.CloseDialog("GameSetting");
	}

    public void OnMusic()
	{
        SoundManager.instance.ChangeMusic();
        this.Populate();
	}

    public void OnSound()
	{
        SoundManager.instance.ChangeSound();
        this.Populate();
	}
}
