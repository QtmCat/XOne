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

    private State idleState;

    private State aniState;

    void Awake ()
    {
        Instance = this;

    }

    void Start ()
    {
        this.Init ();
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
        this.incubator          = this.gameObject.GetComponent<Incubator> ();

        this.InitState ();
        this.InitMapCellList ();
    }

    private void InitState ()
    {
        this.idleState          = this.CreateState ((int)StateType.Idle);
        idleState.OnEnter       = this.OnEneter_IdleState;
        this.aniState           = this.CreateState ((int)StateType.Ani);
        aniState.OnEnter        = this.OnEnter_AniState;
    }

    private void InitMapCellList ()
    {
        // 断言
        ADebug.Assert (this.transform.childCount == Constant.RanksNum, "the col Num is not Constant.RanksNum");

        this.mapCellList        = new MapCell[Constant.RanksNum][];
        for (int i = 0; i < Constant.RanksNum; i++)
        {
            Transform col       = this.transform.GetChild (i);
            // 断言
            string info         = string.Format ("the row of col{0} Num is not Constant.RanksNum", i);
            ADebug.Assert (col.childCount == Constant.RanksNum, info);

            this.mapCellList[i] = col.GetComponentsInChildren<MapCell> ();

            // 初始化MapCell
            for (int j = 0; j < Constant.RanksNum; j++)
            {
                MapCell mapCell = this.mapCellList[i][j];
                Element element = this.incubator.CreateElement ();
                mapCell.Setup (i, j, element);
            }
        }
    }

    public void SetPointerMapCell (MapCell mapCell)
    {
        //if (this.curState != this.idleState)
        //{
        //    return;
        //}
        if (this.curMapCell == null)
        {
            this.SetCurMapCell (mapCell);
            return;
        }
        int colOffset           = Math.Abs (mapCell.colIndex - this.curMapCell.colIndex);
        int rowOffset           = Math.Abs (mapCell.rowIndex - this.curMapCell.rowIndex);
        if (colOffset + rowOffset == 1)
        {
            this.nextMapCell    = mapCell;

            this.SetState ((int)StateType.Ani);
            this.ExChangeElement (this.curMapCell, this.nextMapCell);
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

    private void ExChangeElement (MapCell mapCell1, MapCell mapCell2)
    {
        Element element         = mapCell1.element;
        mapCell1.SetElement (mapCell2.element);
        mapCell2.SetElement (element);

        mapCell1.element.Reset ();
        mapCell2.element.Reset ();
    }

    private bool CrashTest ()
    {
        return false;
    }

    private void OnEneter_IdleState ()
    {

    }

    private void OnEnter_AniState ()
    {

    }

    public MapCell[][] mapCellList { private set; get; }

    public enum StateType
    {
        None = 0,
        Idle,
        Ani,
    }
}
