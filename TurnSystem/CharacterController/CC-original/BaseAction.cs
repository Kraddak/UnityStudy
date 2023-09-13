using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour{

    public static EventHandler OnAnyActionStarted;
    public static EventHandler OnAnyActionEnded;

    protected bool isActive;
    protected Unit unit;
    protected Action onActionComplete;

    protected virtual void Awake() {
        this.unit = GetComponent<Unit>();
        this.isActive = false;
    }

    public virtual bool IsValidGridPosition(GridPosition gridPosition){
        List<GridPosition> validGridPositionList = GetValidActionGridPositionList();
        return validGridPositionList.Contains(gridPosition);
    }

    public abstract List<GridPosition> GetValidActionGridPositionList();

    public abstract string GetActionName();



    protected void ActionComplete(){
        isActive = false;
        onActionComplete();
        OnAnyActionEnded?.Invoke(this, EventArgs.Empty);
    }


    public virtual void TakeAction(GridPosition gridPosition, Action onActionComplete){
        isActive = true;
        this.onActionComplete = onActionComplete;
        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }

    public virtual void TakeAction(Action onActionComplete){
        isActive = true;
        this.onActionComplete = onActionComplete;
        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }


    public virtual int GetActionPointsCost(){
        return 1;
    }


    public Unit GetUnit(){
        return unit;
    }


    public EnemyAIAction GetBestEnemyAIAction(){
        List<EnemyAIAction> enemyAIActionList = new List<EnemyAIAction>();

        List<GridPosition> validActionGridPositionList = GetValidActionGridPositionList();

        foreach (GridPosition gridPosition in validActionGridPositionList){
            EnemyAIAction enemyAIAction = GetEnemyAIAction(gridPosition);
            enemyAIActionList.Add(enemyAIAction);
        }

        if (enemyAIActionList.Count > 0){
            enemyAIActionList.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);
            return enemyAIActionList[0];
        } else{
            // No possible Enemy AI Actions
            return null;
        }

    }



    public abstract EnemyAIAction GetEnemyAIAction(GridPosition gridPosition);





}
