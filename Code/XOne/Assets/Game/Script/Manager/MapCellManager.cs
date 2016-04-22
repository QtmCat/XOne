using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using QtmCatFramework;
using System;

[RequireComponent(typeof(Incubator))]
public class MapCellManager : StateMachine
{
    public static MapCellManager instance;

    private Incubator incubator;

    private MapCell curMapCell;

    private MapCell nextMapCell;

    private List<MapCell> nestList;

    private List<MapCell> dropList;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        this.Init();
    }

    public override void Update()
    {
        base.Update();
    }

    void OnDestroy()
    {
        instance = null;
    }

    private void Init()
    {
        this.incubator  = this.gameObject.GetComponent<Incubator>();
        this.dropList   = new List<MapCell>();

        this.InitState();
        this.InitMapCellList();
    }

    private void InitState()
    {
        State idleState     = this.CreateState(StateType.idle);
        idleState.OnEnter   = this.OnEneterForIdleState;

        State aniState      = this.CreateState(StateType.exChange);
        aniState.OnEnter    = this.OnEnterForAniState;

        this.CreateState(StateType.drop);

        this.SetState(StateType.idle);
    }

    private void InitMapCellList()
    {
        ADebug.Assert(this.transform.childCount == AConstant.ranks_num, "the col Num is not Constant.RanksNum");

        this.mapCellList            = new MapCell[AConstant.ranks_num][];
        this.nestList               = new List<MapCell>();

        for (int i = 0; i < AConstant.ranks_num; i++)
        {
            Transform col           = this.transform.GetChild(i);

            ADebug.Assert(col.childCount == AConstant.ranks_num, "the row of col{0} Num is not Constant.RanksNum", i);

            this.mapCellList[i]     = col.GetComponentsInChildren<MapCell>();
            
            // 初始化MapCell
            for (int j = 0; j < AConstant.ranks_num; j++)
            {
                MapCell mapCell     = this.mapCellList[i][j];
                // nest
                if (mapCell.isNest)
                {
                    this.nestList.Add(mapCell);
                }

                Element element     = this.incubator.CreateElement();
                mapCell.Setup(i, j, element);
            }
        }
    }

    public void SetPointerMapCell(MapCell mapCell)
    {
		if ((StateType) this.GetCurStateId() != StateType.idle)
        {
            return;
        }

        if (this.curMapCell == null)
        {
            this.SetCurMapCell(mapCell);
            return;
        }

        int colOffset           = Math.Abs(mapCell.colIndex - this.curMapCell.colIndex);
        int rowOffset           = Math.Abs(mapCell.rowIndex - this.curMapCell.rowIndex);
        if (colOffset + rowOffset == 1)
        {
            this.nextMapCell    = mapCell;

            this.SetState(StateType.exChange);
            this.ExChangeElement(this.curMapCell, this.nextMapCell, true);

            this.SetCurMapCell(null);
        }
        else
        {
            this.SetCurMapCell(mapCell);
        }
    }

    private void SetCurMapCell(MapCell mapCell)
    {
        if (this.curMapCell != null)
        {
            this.curMapCell.SetSelected(false);
        }

        if (mapCell != null)
        {
            mapCell.SetSelected(true);
        }

        this.curMapCell = mapCell;
    }

    private void ExChangeElement(MapCell mapCell1, MapCell mapCell2, bool isCrashTest)
    {
        Action Callback = () =>
        {
            if (isCrashTest)
            {
                if (!this.CrashTestExchange(mapCell1, mapCell2))
                {
                    this.ExChangeElement(mapCell1, mapCell2, false);
                }
            }
            else
            {
                this.SetState(StateType.idle);
            }
        };

        Element element = mapCell1.element;
        mapCell1.SetElement(mapCell2.element);
        mapCell2.SetElement(element);

        mapCell1.ResetElementPos();
        mapCell2.ResetElementPos(Callback);
    }

    // 检测交换的两个元素的消除情况
    private bool CrashTestExchange(MapCell mapCell1, MapCell mapCell2)
    {
        return this.CrashTestSingle(mapCell1) || this.CrashTestSingle(mapCell2);
    }

    // 检测单个元素的消除情况
    private bool CrashTestSingle(MapCell mapCell)
    {
        List<MapCell> list      = new List<MapCell>();
        list.Add(mapCell);

        // 同一行
        List<MapCell> rowList   = new List<MapCell>();
        for (int x = mapCell.colIndex - 1; x >= 0; x--)
        {
            MapCell temp        = this.mapCellList[x][mapCell.rowIndex];
            if (!mapCell.IsSameColor(temp))
            {
                break;
            }
            rowList.Add(temp);
        }

        for (int x = mapCell.colIndex + 1; x < AConstant.ranks_num; x++)
        {
            MapCell temp        = this.mapCellList[x][mapCell.rowIndex];
            if (!mapCell.IsSameColor(temp))
            {
                break;
            }
            rowList.Add(temp);
        }

        if (rowList.Count >= AConstant.crash_min - 1)
        {
            foreach (MapCell temp in rowList)
            {
                list.Add(temp);
            }
        }

        // 同一列
        List<MapCell> colList   = new List<MapCell>();
        for (int y = mapCell.rowIndex - 1; y >= 0; y--)
        {
            MapCell temp        = this.mapCellList[mapCell.colIndex][y];
            if (!mapCell.IsSameColor(temp))
            {
                break;
            }
            colList.Add(temp);
        }

        for (int y = mapCell.rowIndex + 1; y < AConstant.ranks_num; y++)
        {
            MapCell temp        = this.mapCellList[mapCell.colIndex][y];
            if (!mapCell.IsSameColor(temp))
            {
                break;
            }
            colList.Add(temp);
        }

        if (colList.Count >= AConstant.crash_min - 1)
        {
            foreach (MapCell temp in colList)
            {
                list.Add(temp);
            }
        }

        // 是否达到最小消除数量
        if (list.Count >= AConstant.crash_min)
        {
            this.DoCrash(list);
            return true;
        }
        return false;
    }

    private void DoCrash(List<MapCell> list)
    {
        while (list.Count > 0)
        {
            MapCell mapCell = list[0];

            if (mapCell.element.type == ElementType.normal)
            {
                mapCell.CrashElement();
            }
            else if (mapCell.element.type == ElementType.horizontal)
            {

            }
            else if (mapCell.element.type == ElementType.vertical)
            {

            }
            else if (mapCell.element.type == ElementType.boom)
            {

            }
            else if (mapCell.element.type == ElementType.super)
            {

            }

            list.RemoveAt(0);
        }

        this.HatchTest();
        this.DropTest();
    }

    private void DropTest()
    {
        for (int x = 0; x < AConstant.ranks_num; x++)
        {
            for (int y = 0; y < AConstant.ranks_num; y++)
            {
                MapCell mapCell = this.mapCellList[x][y];
                if (mapCell.element != null)
                {
                    this.DropTestSingle(mapCell);
                }
            }
        }
    }

    private void DropTestSingle(MapCell mapCell)
    {
        MapCell dropMapCell = this.GetDropDownMapCell(mapCell);
        if (dropMapCell != null)
        {
            this.DropElement(mapCell, dropMapCell);
            this.HatchTest();
        }
        else
        {

        }
    }

    private MapCell GetDropDownMapCell(MapCell mapCell)
    {
        MapCell dropMapCell = null;
        for (int y = mapCell.rowIndex - 1; y >= 0; y--)
        {
            MapCell temp    = this.mapCellList[mapCell.colIndex][y];
            if (!temp.IsCoundDrop())
            {
                break;
            }
            dropMapCell     = temp;
        }

        return dropMapCell;
    }

    private void GetDropLeftDownMapCell()
    {

    }

    private void GetDropRightDownMapCell()
    {

    }

    private void DropElement(MapCell startMapCell, MapCell destMapCell)
    {
        this.AddDropList(destMapCell);
        Action Callback = () =>
        {
            this.RemoveDropList(destMapCell);
            this.CrashTestSingle(destMapCell);
        };

        Element element = startMapCell.element;
        startMapCell.SetElement(destMapCell.element);
        destMapCell.SetElement(element);

        destMapCell.Drop(Callback);
    }

    private void AddDropList(MapCell mapCell)
    {
        this.dropList.Add(mapCell);

		if ((StateType) this.GetCurStateId() != StateType.drop)
        {
            this.SetState(StateType.drop);
        }
    }

    private void RemoveDropList(MapCell mapCell)
    {
        this.dropList.Remove(mapCell);
        if (this.dropList.Count == 0)
        {
            this.SetState(StateType.idle);
        }
    }

    private void HatchTest()
    {
        foreach (MapCell mapCell in this.nestList)
        {
            if (mapCell.IsCoundHatch())
            {
                Element element = this.incubator.CreateElement();
                mapCell.SetHatchElement(element);
                mapCell.Drop(null);
                this.DropTestSingle(mapCell);
            }
        }
    }

    private void OnEneterForIdleState()
    {

    }

    private void OnEnterForAniState()
    {

    }

    public MapCell[][] mapCellList { private set; get; }

    public enum StateType
    {
        none = 0,
        idle,
        exChange,
        drop,
    }
}
