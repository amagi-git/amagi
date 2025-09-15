using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class MoveAction : BaseAction//ผฬณะ
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    //[SerializeField] private Animator unitAnimator;
    [SerializeField] private int maxMoveDistance = 4;
    private List<Vector3> positionList;
    private int currentPositionIndex;
    
    //protected override void Awake()
    //{   
    //    base.Awake(); 
    //    //targetPosition = transform.position;
    //}
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
        Vector3 targetPosition = positionList[currentPositionIndex];
        Vector3 moveDirection = (targetPosition - transform.position).normalized;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
        float stoppingDistance = .1f; ;
        if (Vector3.Distance(targetPosition, transform.position) > stoppingDistance)
        {
            
            float moveSpeed = 4f;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            
        }
        else
        {
            currentPositionIndex++;
            if (currentPositionIndex >= positionList.Count)
            {
                OnStopMoving?.Invoke(this, EventArgs.Empty);
                ActionComplete();
            }
            
        }
        
    }

    //public void Move(GridPosition gridPosition,Action onActionComplete)
    //{   
    //    this.onActionComplete = onActionComplete;
    //    this.targetPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
    //    isActive = true;
    //}
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {   
        List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(unit.GetGridPosition(), gridPosition,out int pathLength);
        currentPositionIndex = 0;
        positionList = new List<Vector3> ();
        foreach(GridPosition pathGridPosition in pathGridPositionList)
        {
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }
        //this.targetPosition = ;
        OnStartMoving?.Invoke(this,EventArgs.Empty);
        ActionStart(onActionComplete);
    }
    //public bool IsValidActionGridPosition(GridPosition gridPosition)
    //{
    //    List<GridPosition> validGridPosition = GetValidActionGridPositionList();
    //    return validGridPosition.Contains(gridPosition);
    //}
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();
        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                for (int floor = -maxMoveDistance; floor <= maxMoveDistance; floor++)
                {
                    GridPosition offsetGridPosition = new GridPosition(x, z, floor);
                    GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                    {
                        continue;
                    }
                    if (testGridPosition == unitGridPosition)
                    {
                        //unit already at
                        continue;
                    }
                    if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                    {
                        //another unit at
                        continue;
                    }
                    if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
                    {
                        continue;
                    }
                    if (!Pathfinding.Instance.HasPath(unitGridPosition, testGridPosition))
                    {
                        continue;
                    }

                    int pathfindingDistanceMultiplier = 10;//pathfinding
                    if (Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > maxMoveDistance * pathfindingDistanceMultiplier)
                    {
                        //path length too long;
                        continue;
                    }


                    //Debug.Log(testGridPosition);
                    validGridPositionList.Add(testGridPosition);
                }
            }
        }
        return validGridPositionList;
    }
    public override string GetActionName()
    {
        return "Move";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {   
        int targetCountAtGridPosition = unit.GetAction<ShootAction>().GetValidActionGridPositionList().Count;
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = targetCountAtGridPosition*10,
        };
    }
}
