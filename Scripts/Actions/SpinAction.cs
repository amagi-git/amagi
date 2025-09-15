using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAction : BaseAction
{
    public delegate void SpinCompleteDelegate();//委托(不管是否private

    private float totalSpinAmount;

    //private SpinCompleteDelegate onSpinComplete;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive)
        {
            return;
        }
        
            float spinAddAmount = 360f* Time.deltaTime;
            transform.eulerAngles += new Vector3(0, spinAddAmount, 0);
        totalSpinAmount += spinAddAmount;
        if (totalSpinAmount >= 360f)
        {
            ActionComplete();
        }
    }
    //public void Spin(Action onActionComplete)
    //{
    //    //this.onSpinComplete = onSpinComplete;
    //    this.onActionComplete = onActionComplete;
    //    isActive = true;
    //    totalSpinAmount = 0f;
    //}

    public override void TakeAction(GridPosition gridPosition,Action onActionComplete)
    {
        
        totalSpinAmount = 0f;
        ActionStart(onActionComplete);
    }
    public override string GetActionName()
    {
        return "Spin";
    }
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        //List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition gridPosition = unit.GetGridPosition();

        return new List<GridPosition>
        {
            gridPosition
        };
    }
    public override int GetActionPointsCost()
    {
        return 2;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0,
        };
    }
}
