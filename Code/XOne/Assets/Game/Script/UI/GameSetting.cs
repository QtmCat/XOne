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
        this.Finalize();
	}
	
    private void Finalize()
    {
        this.musicText.text = SoundManager.Instance.isMusicOn ? "关闭" : "打开";
        this.soundText.text = SoundManager.Instance.isSoundOn ? "关闭" : "打开";
    }

	public void OnBack()
	{
		AUIManager.CloseDialog("GameSetting");
	}

    public void OnMusic()
	{
        SoundManager.Instance.ChangeMusic();
        this.Finalize();
	}

    public void OnSound()
	{
        SoundManager.Instance.ChangeSound();
        this.Finalize();
	}
}
