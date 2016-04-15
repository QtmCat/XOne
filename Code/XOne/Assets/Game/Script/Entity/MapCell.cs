using UnityEngine;
using System.Collections;
using QtmCatFramework;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MapCell : StateMachine 
{
    public bool isShow;

    public bool isNest;

    public IceLevel iceLevel;

    public StoneLevel stoneLevel;



    private Button button;

	void Start()
    {
        
    }

    private void Init ()
    {

    }
}

public enum IceLevel
{
    None = 0,
    One,
    Double,
    Triple
}

public enum StoneLevel
{
    None = 0,
    One,
    Double,
    Triple
}
