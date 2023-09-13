using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;


public class ActionMove : ActionBase{
    public static event Action<Unit, float> OnStartMoving;
    public static event EventHandler OnStopMoving;

    private const string s_isMoving = "IsMoving";
    private const string s_jump = "Jump";

    private float movingAnimationSpeed = 7f;

    private int currentPositionIndex;   
    private Vector3[] pathAsVectors;

    public ActionMove(Unit unit) : base(unit){}



    public override void Ready(){
        Pathfinder.Instance.Activate(this.unit.transform.position, this.unit.GetRemainingMovement());
        this.isWaitingForAnimationToFinish = false;
    }

    public override void UnReady(){
        Pathfinder.Instance.Deactivate();
        this.isWaitingForAnimationToFinish = false;
    }

    

    public override int GetUIpriority(){
        return 0;
    }

    public override Sprite GetActionSprite(){
        return GameAssets.i.moveAction;
    }


    public override async Task Take(){
        currentPositionIndex = 0;
        this.pathAsVectors = Pathfinder.Instance.GetCurrentPath();
        Pathfinder.Instance.Deactivate();
        if(currentPositionIndex > pathAsVectors.Length){
            this.isWaitingForAnimationToFinish = false;
            return;
        }

        Task recalculateLOS = this.unit.RecalculateLOS(
            pathAsVectors[pathAsVectors.Length-1], this.unit.GetHeight());
            CharacterController.Instance.AddTask(recalculateLOS);

        OnStartMoving?.Invoke(this.unit, this.movingAnimationSpeed);
        this.unit.animator.SetBool(s_isMoving, true);
        for(int i=0; i<pathAsVectors.Length; i++)
            await Util.MoveTo(unit.transform, pathAsVectors[i], movingAnimationSpeed, unit.MantainFootsOnTerrain);
        

        this.unit.animator.SetBool(s_isMoving, false);
        OnStopMoving?.Invoke(this, EventArgs.Empty);
        this.unit.CheckForCover();
        this.pathAsVectors = null;
        Pathfinder.Instance.Activate(this.unit.transform.position, this.unit.GetRemainingMovement());
    }

    public override ActionType.Name GetActionType(){
        return ActionType.Name.move;
    }
}
