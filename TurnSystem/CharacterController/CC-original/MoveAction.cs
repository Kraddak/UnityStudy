using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction{


    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    [SerializeField] private int maxMoveDistance = 4;
    private List<Vector3> pathAsVectors;
    private int currentPositionIndex;    


    private void Update() {
        if(!isActive) return;

        Vector3 targetPosition = pathAsVectors[currentPositionIndex];

        float stoppingDistance = .1f;
        if(Vector3.Distance(transform.position, targetPosition) > stoppingDistance){
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            float moveSpeed = 4f;
            float rotateSpeed = 10f;

            // Rotation
            transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
            // Movement
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

        }else{
            if(++currentPositionIndex >= pathAsVectors.Count){
                OnStopMoving?.Invoke(this, EventArgs.Empty);
                ActionComplete();
            }

        }
    }


    public override void TakeAction(GridPosition destination, Action onActionComplete){
        List<GridPosition> path = Pathfinding.Instance.FindPath(unit.GetGridPosition(), destination, out int pl);

        this.currentPositionIndex = 0;
        this.pathAsVectors = new List<Vector3>();

        foreach (GridPosition node in path)
            pathAsVectors.Add(LevelGrid.Instance.GetWorldPosition(node));
        

        OnStartMoving?.Invoke(this, EventArgs.Empty);
        
        base.TakeAction(destination, onActionComplete);
    }

    
    public override List<GridPosition> GetValidActionGridPositionList(){
        return GetValidActionGridPositionList(unit.GetGridPosition());
    }

    public List<GridPosition> GetValidActionGridPositionList(GridPosition position){
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = position;

        for(int x = -maxMoveDistance; x <= maxMoveDistance; x++){
            for(int z = -maxMoveDistance; z <= maxMoveDistance; z++){
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if(!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;

                if(unitGridPosition == testGridPosition) continue;

                if(LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) continue;

                if(!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition)) continue;

                if(!Pathfinding.Instance.HasPath(unitGridPosition, testGridPosition)) continue;

                int pathfindingDistanceMultiplier = 10;
                if (Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > maxMoveDistance * pathfindingDistanceMultiplier)                {
                    // Path length is too long
                    continue;
                }

            
                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }


    public override string GetActionName(){
        return  "Move";
    }



    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPosition = unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition);

        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = targetCountAtGridPosition * 10,
        };
    }



    public int GetTargetCountAtPosition(GridPosition gridPosition){
        return GetValidActionGridPositionList(gridPosition).Count;
    }

}
