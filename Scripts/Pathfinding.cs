using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance { get; private set; }
    public const int MOVE_STRAIGHT_COST = 10;//1
    public const int MOVE_DIAGONAL_COST = 14;//1.4

    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private LayerMask obstaclesLayerMask;
    [SerializeField] private LayerMask floorLayerMask;
    private int width;
    private int height;
    private float cellSize;
    private int floorAmount;
    private List<GridSystem<PathNode>> gridSystemList;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("more then one instance");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //levelgrid
        
    }
    public void Setup(int width,int height,float cellSize,int floorAmount)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.floorAmount = floorAmount;
        gridSystemList = new List<GridSystem<PathNode>>();
        for (int floor = 0; floor < floorAmount; floor++)
        {
            GridSystem<PathNode> gridSystem = new GridSystem<PathNode>(width, height, cellSize, floor, LevelGrid.FLOOR_HEIGHT, (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));
            //gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
            gridSystemList.Add(gridSystem);
        }
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                for (int floor = 0; floor < floorAmount; floor++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, floor);
                    Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                    float raycastOffsetDistance = 1f;
                    GetNode(x, z, floor).SetIsWalkable(false);

                    if (Physics.Raycast(worldPosition + Vector3.up * raycastOffsetDistance, Vector3.down, raycastOffsetDistance * 2, floorLayerMask))//从上到下射plane
                    {
                        GetNode(x, z, floor).SetIsWalkable(true);
                        Debug.Log($"Obstacle found at grid position ({x}, {z},{floor})");

                    }
                    if (Physics.Raycast(worldPosition + Vector3.down * raycastOffsetDistance, Vector3.up, raycastOffsetDistance * 2, obstaclesLayerMask))
                    {
                        GetNode(x, z,floor).SetIsWalkable(false);
                        Debug.Log($"Obstacle found at grid position ({x}, {z},{floor})");

                    }
                }
            }
        }
    }

    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition,out int pathLength)
    {
        List<PathNode> openList = new List<PathNode>();//正在搜索
        List<PathNode> closedList = new List<PathNode>();//已经搜索过

        PathNode startNode = GetGridSystem(startGridPosition.floor).GetGridObject(startGridPosition);
        PathNode endNode = GetGridSystem(endGridPosition.floor).GetGridObject(endGridPosition);
        openList.Add(startNode);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                for (int floor = 0; floor < floorAmount; floor++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, floor);
                    PathNode pathNode = GetGridSystem(floor).GetGridObject(gridPosition);

                    pathNode.SetGCost(int.MaxValue);
                    pathNode.SetHCost(0);//到达目标的启发式估计
                    pathNode.CalculateFCost();
                    pathNode.ResetCameFromPathNode();
                }
            }
        }
        startNode.SetGCost(0);
        startNode.SetHCost(CalculateDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList);
            if (currentNode == endNode)
            {
                pathLength = endNode.GetGCost();
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }
                if (!neighbourNode.IsWalkable())
                {
                    closedList.Add(neighbourNode);
                    continue;
                }
                int tentativeGCost = currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighbourNode.GetGridPosition());

                if (tentativeGCost < neighbourNode.GetGCost())
                {
                    neighbourNode.SetCameFromPathNode(currentNode);
                    neighbourNode.SetGCost(tentativeGCost);
                    neighbourNode.SetHCost(CalculateDistance(neighbourNode.GetGridPosition(), endGridPosition));
                    neighbourNode.CalculateFCost();
                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }

        //no path found
        pathLength = 0;
        return null;
    }
    public int CalculateDistance(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        GridPosition gridPositionDistance = gridPositionA - gridPositionB;
        //int distance = Mathf.Abs(gridPositionDistance.x)+Mathf.Abs(gridPositionDistance.z);
        int xDistance = Mathf.Abs(gridPositionDistance.x);
        int zDistance = Mathf.Abs(gridPositionDistance.z);
        int remaining = Mathf.Abs(xDistance - zDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostPathNode = pathNodeList[0];
        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].GetFCost() < lowestFCostPathNode.GetFCost())
            {
                lowestFCostPathNode = pathNodeList[i];
            }
        }
        return lowestFCostPathNode;
    }

    private GridSystem<PathNode> GetGridSystem(int floor)
    {
        return gridSystemList[floor];
    }
    private PathNode GetNode(int x, int z,int floor)
    {
        return GetGridSystem(floor).GetGridObject(new GridPosition(x, z, floor));
    }
    private List<PathNode> GetNeighbourList(PathNode currenyNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        GridPosition gridPosition = currenyNode.GetGridPosition();
        if (gridPosition.x - 1 >= 0)
        {
            //left
            neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 0,gridPosition.floor));

            if (gridPosition.z - 1 >= 0)
            {
                //left down
                neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1, gridPosition.floor));//对角线
            }
            if (gridPosition.z + 1 < height)
            { //left up
                neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1, gridPosition.floor));
            }

        }
        if (gridPosition.x + 1 < width)
        {//right
            neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 0, gridPosition.floor));

            if (gridPosition.z - 1 >= 0)
            {
                neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1, gridPosition.floor));
            }
            if (gridPosition.z + 1 < height)
            {
                neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1, gridPosition.floor));
            }
        }
            //down
            if (gridPosition.z - 1 >= 0)
            {
                neighbourList.Add(GetNode(gridPosition.x + 0, gridPosition.z - 1, gridPosition.floor));
            }
            //up
            if (gridPosition.z + 1 <height)
            {
                
                neighbourList.Add(GetNode(gridPosition.x + 0, gridPosition.z + 1, gridPosition.floor));
            }
            
            List<PathNode> totalNeighbourList = new List<PathNode>();
            totalNeighbourList.AddRange(neighbourList);

            foreach(PathNode pathNode in neighbourList)
        {
            GridPosition neighbourGridPosition = pathNode.GetGridPosition();
            if (neighbourGridPosition.floor - 1 >= 0)
            {
                totalNeighbourList.Add(GetNode(neighbourGridPosition.x,neighbourGridPosition.z,neighbourGridPosition.floor-1));
            }
            if (neighbourGridPosition.floor + 1 <floorAmount)
            {
                totalNeighbourList.Add(GetNode(neighbourGridPosition.x, neighbourGridPosition.z, neighbourGridPosition.floor + 1));
            }
        }
        return totalNeighbourList;
    }
    private List<GridPosition> CalculatePath(PathNode endNode)
    {
        List<PathNode> pathNodeList = new List<PathNode>();
        pathNodeList.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.GetCameFromPathNode() != null)
        {
            pathNodeList.Add(currentNode.GetCameFromPathNode());
            currentNode = currentNode.GetCameFromPathNode();
        }

        pathNodeList.Reverse();
        List<GridPosition> gridPositionList = new List<GridPosition>();
        foreach(PathNode pathNode in pathNodeList)
        {
            gridPositionList.Add(pathNode.GetGridPosition());
        }
        return gridPositionList;
    }

    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return GetGridSystem(gridPosition.floor).GetGridObject(gridPosition).IsWalkable();
    }
    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        return FindPath(startGridPosition, endGridPosition,out int pathLength)!=null;
    }

    public int GetPathLength(GridPosition startGridPosition,GridPosition endGridPosition)
    {
        FindPath(startGridPosition,endGridPosition,out int pathLength);
        return pathLength;
    }
}
