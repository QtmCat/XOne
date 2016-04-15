using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class MapCellManager : MonoBehaviour 
{
    public static MapCellManager Instance;

    void Awake ()
    {
        Instance = this;
    }

    void Start ()
    {
        this.Init();
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
        this.InitMapCellList ();
    }

    private void InitMapCellList ()
    {
        // 断言
        Assert.IsTrue(this.transform.childCount == Constant.RanksNum, "the col Num is not Constant.RanksNum");

        this.mapCellList        = new MapCell[Constant.RanksNum][];
        for (int i = 0; i < Constant.RanksNum; i++)
        {
            Transform col       = this.transform.GetChild(i);
            // 断言
            string info         = string.Format("the row of col{0} Num is not Constant.RanksNum", i);
            Assert.IsTrue(col.childCount == Constant.RanksNum, info);

            this.mapCellList[i] = col.GetComponentsInChildren<MapCell>();
        }
    }

    public MapCell[][] mapCellList { private set; get; }
}
