using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using QtmCatFramework;
using System;

[RequireComponent (typeof (Incubator))]
public class MapCellManager : StateMachine
{
    public static MapCellManager Instance;

    private Incubator incubator;

    private MapCell curMapCell;

    private MapCell nextMapCell;

    private List<MapCell> dropList;

    void Awake ()
    {
        Instance = this;

    }

    void Start ()
    {
        this.Init ();
    }

	public override void Update ()
    {
        base.Update ();
    }

    void OnDestroy ()
    {
        Instance = null;
    }

    private void Init ()
    {
        this.incubator          = this.gameObject.GetComponent<Incubator> ();
        this.dropList           = new List<MapCell> ();

        this.InitState ();
        this.InitMapCellList ();
    }

    private void InitState ()
    {
        State idleState         = this.CreateState ((int)StateType.Idle);
        idleState.OnEnter       = this.OnEneterForIdleState;
        State aniState          = this.CreateState ((int)StateType.ExChange);
        aniState.OnEnter        = this.OnEnterForAniState;

        this.SetState ((int)StateType.Idle);
    }

    private void InitMapCellList ()
    {
        // 断言
        ADebug.Assert (this.transform.childCount == AConstant.RanksNum, "the col Num is not Constant.RanksNum");

        this.mapCellList        = new MapCell[AConstant.RanksNum][];
        for (int i = 0; i < AConstant.RanksNum; i++)
        {
            Transform col       = this.transform.GetChild (i);
            // 断言
            ADebug.Assert (col.childCount == AConstant.RanksNum, "the row of col{0} Num is not Constant.RanksNum", i);

            this.mapCellList[i] = col.GetComponentsInChildren<MapCell> ();

            // 初始化MapCell
            for (int j = 0; j < AConstant.RanksNum; j++)
            {
                MapCell mapCell = this.mapCellList[i][j];
                Element element = this.incubator.CreateElement ();
                mapCell.Setup (i, j, element);
            }
        }
    }

    public void SetPointerMapCell (MapCell mapCell)
    {
        if (this.GetCurStateId() != (int)StateType.Idle)
        {
            return;
        }
        if (this.curMapCell == null)
        {
            this.SetCurMapCell (mapCell);
            return;
        }
        int colOffset           = Math.Abs (mapCell.colIndex - this.curMapCell.colIndex);
        int rowOffset           = Math.Abs (mapCell.rowIndex - this.curMapCell.rowIndex);
        if (colOffset + rowOffset == 1)
        {
            this.SetState ((int)StateType.ExChange);
            this.nextMapCell    = mapCell;
            this.ExChangeElement (this.curMapCell, this.nextMapCell, true);
            this.SetCurMapCell (null);
        }
        else
        {
            this.SetCurMapCell (mapCell);
        }
    }

    private void SetCurMapCell (MapCell mapCell)
    {
        if (this.curMapCell != null)
        {
            this.curMapCell.SetSelected (false);
        }
        if (mapCell != null)
        {
            mapCell.SetSelected (true);
        }
        this.curMapCell         = mapCell;
    }

    private void ExChangeElement (MapCell mapCell1, MapCell mapCell2, bool isCrashTest)
    {
        Action callback         = () => 
        {
            if (isCrashTest)
            {
                if (!this.CrashTestExchange (mapCell1, mapCell2))
                {
                    this.ExChangeElement (mapCell1, mapCell2, false);
                }
            }
            else
            {
                this.SetState ((int)StateType.Idle);
            }
        };

        Element element         = mapCell1.element;
        mapCell1.SetElement (mapCell2.element);
        mapCell2.SetElement (element);

        mapCell1.ResetElementPos (callback);
        mapCell2.ResetElementPos ();
    }

    // 检测交换的两个元素的消除情况
    private bool CrashTestExchange (MapCell mapCell1, MapCell mapCell2)
    {
        return this.CrashTestSingle (mapCell1) || this.CrashTestSingle (mapCell2);
    }

    // 检测单个元素的消除情况
    private bool CrashTestSingle (MapCell mapCell)
    {
        List<MapCell> list      = new List<MapCell> ();
        list.Add (mapCell);
        // 同一行
        List<MapCell> rowList   = new List<MapCell> ();
        for (int x = mapCell.colIndex - 1; x >= 0; x--)
        {
            MapCell temp        = this.mapCellList[x][mapCell.rowIndex];
            if (!mapCell.isSameColor (temp))
            {
                break;
            }
            rowList.Add (temp);
        }
        for (int x = mapCell.colIndex + 1; x < AConstant.RanksNum; x++)
        {
            MapCell temp        = this.mapCellList[x][mapCell.rowIndex];
            if (!mapCell.isSameColor (temp))
            {
                break;
            }
            rowList.Add (temp);
        }
        if (rowList.Count >= AConstant.CrashMin - 1)
        {
            foreach (MapCell temp in rowList)
            {
                list.Add (temp);
            }
        }
        // 同一列
        List<MapCell> colList   = new List<MapCell> ();
        for (int y = mapCell.rowIndex - 1; y >= 0; y--)
        {
            MapCell temp        = this.mapCellList[mapCell.colIndex][y];
            if (!mapCell.isSameColor (temp))
            {
                break;
            }
            colList.Add (temp);
        }
        for (int y = mapCell.rowIndex + 1; y < AConstant.RanksNum; y++)
        {
            MapCell temp        = this.mapCellList[mapCell.colIndex][y];
            if (!mapCell.isSameColor (temp))
            {
                break;
            }
            colList.Add (temp);
        }
        if (colList.Count >= AConstant.CrashMin - 1)
        {
            foreach (MapCell temp in colList)
            {
                list.Add (temp);
            }
        }
        //
        if (list.Count >= AConstant.CrashMin)
        {
            this.DoCrash(list);
            return true;
        }
        return false;
    }

    private void DoCrash (List<MapCell> list)
    {
        while (list.Count > 0)
        {
            MapCell mapCell = list[0];
            if (mapCell.element.type == ElementType.Normal)
            {
                mapCell.CrashElement ();
            }
            else if (mapCell.element.type == ElementType.Horizontal)
            {

            }
            else if (mapCell.element.type == ElementType.Vertical)
            {

            }
            else if (mapCell.element.type == ElementType.Boom)
            {

            }
            else if (mapCell.element.type == ElementType.Super)
            {

            }
            //
            list.RemoveAt (0);
        }
        this.DropTest ();
    }

    private void DropTest ()
    {
        for (int x = 0; x < AConstant.RanksNum; x++)
        {
            for (int y = 0; y < AConstant.RanksNum; y++)
            {
                MapCell mapCell = this.mapCellList[x][y];
                if (mapCell.element != null)
                {
                    this.DropTestSingle (mapCell);
                }
            }
        }
    }

    private void DropTestSingle (MapCell mapCell)
    {
        MapCell dropMapCell     = this.GetDropDownMapCell (mapCell);
        if (dropMapCell != null)
        {
            this.DropElement (mapCell, dropMapCell);
        }
        else
        {

        }
    }

    private MapCell GetDropDownMapCell (MapCell mapCell)
    {
        MapCell dropMapCell     = null;
        for (int y = mapCell.rowIndex - 1; y >= 0; y--)
        {
            MapCell temp        = this.mapCellList[mapCell.colIndex][y];
            if (temp.element != null)
            {
                break;
            }
            dropMapCell         = temp;
        }
        return dropMapCell;
    }

    private void GetDropLeftDownMapCell ()
    {

    }

    private void GetDropRightDownMapCell ()
    {

    }

    private void DropElement (MapCell startMapCell, MapCell destMapCell)
    {
        Action callback         = () =>
        {

        };

        Element element         = startMapCell.element;
        startMapCell.SetElement (destMapCell.element);
        destMapCell.SetElement (element);

        destMapCell.Drop (callback);
    }

    private void OnEneterForIdleState ()
    {

    }

    private void OnEnterForAniState ()
    {

    }

    public MapCell[][] mapCellList { private set; get; }

    public enum StateType
    {
        None = 0,
        Idle,
        ExChange,
        Drop,
    }
}
