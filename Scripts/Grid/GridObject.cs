using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    private GridPosition gridPosition;
    private GridSystem<GridObject> gridSystem;
    private List<Unit> unitList;
    public GridObject(GridSystem<GridObject> gridsystem, GridPosition gridposition)
    {
        this.gridPosition = gridposition;
        this.gridSystem = gridsystem;
        unitList = new List<Unit>();
    }
    public override string ToString()
    {
        string unitString = "";
        foreach (Unit unit in unitList) 
        {
        unitString += unit+"\n";
        }
        return gridPosition.ToString()+"\n"+unitString;
    }
    public void AddUnit(Unit unit)  
    {
        //unitList.Add(unit);
        if (!unitList.Contains(unit))
        {
            unitList.Add(unit);
        }
    }
    public void RemoveUnit(Unit unit)
    {
        unitList.Remove(unit);
    }
    public List<Unit> GetUnitList()
    {
        return unitList;
    }
    public bool HasAnyUnit()
    {
        return unitList.Count > 0;
    }

    public Unit GetUnit()
    {
        if (HasAnyUnit())
        {
            return unitList[0];
        }
        else
        {
            return null;
        }
    }
}
