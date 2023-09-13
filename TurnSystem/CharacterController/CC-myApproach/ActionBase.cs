using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;



public abstract class ActionBase{
    public static EventHandler OnAnyActionEnded;

    protected Unit unit;
    protected bool isWaitingForAnimationToFinish;

    public ActionBase(Unit unit){
        this.unit = unit;
    }

    public abstract int GetUIpriority();

    public abstract void Ready();

    public abstract void UnReady();

    public abstract Task Take();

    public abstract Sprite GetActionSprite();

    public abstract ActionType.Name GetActionType();



}
