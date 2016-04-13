using UnityEngine;
using System.Collections;
using System;

public class SoundManager : MonoBehaviour 
{
    public static SoundManager Instance;


    void Awake ()
    {
        Instance = this;
        this.Init();
    }

	void Start () 
    {
	    
	}
	
	void Update () 
    {
	    
	}

    void OnDestroy ()
    {
        Instance = null;
    }

    private void Init ()
    {
        this.isMusicOn = Convert.ToBoolean(PlayerPrefs.GetString(Constant.IsMusicOn, "true"));
        this.isSoundOn = Convert.ToBoolean(PlayerPrefs.GetString(Constant.IsSoundOn, "true"));
    }

    public void ChangeMusic ()
    {
        this.SetMusic(!this.isMusicOn);
    }

    public void ChangeSound ()
    {
        this.SetSound(!this.isSoundOn);
    }

    public void SetMusic (bool state)
    {
        this.isMusicOn = state;
        PlayerPrefs.SetString(Constant.IsMusicOn, Convert.ToString(state));
        PlayerPrefs.Save();
    }

    public void SetSound (bool state)
    {
        this.isSoundOn = state;
        PlayerPrefs.SetString(Constant.IsSoundOn, Convert.ToString(state));
        PlayerPrefs.Save();
    }

    public bool isMusicOn { private set; get; }

    public bool isSoundOn { private set; get; }
}
