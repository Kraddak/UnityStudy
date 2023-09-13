using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Threading.Tasks;

public class CharacterController : MonoBehaviour{
    public static CharacterController Instance {get; private set;}

    /***********************************************************************************
    **************** EVENTS ************************************************************
    ************************************************************************************/
    public event Action<Unit> OnSelectedUnitChanged;
    public event Action<Unit> OnActionTerminated;
    public event EventHandler OnSelectedActionChanged;

    /***********************************************************************************
    **************** SERIALIZABLE FIELDS ***********************************************
    ************************************************************************************/
    [SerializeField] private Transform actionButtonPrefab;
    [SerializeField] private Transform targetButtonPrefab;
    [SerializeField] private Transform actionButtonContainer;
    [SerializeField] private Transform targetButtonContainer;
    [SerializeField] private Transform passiveButtonContainer;
    [SerializeField] private LayerMask unitLayerMask;

    /***********************************************************************************
    **************** FIELDS ************************************************************
    ************************************************************************************/
    [SerializeField] private Unit selectedUnit;
    private ActionBase selectedAction;
    private Transform objectHit;
    
    private Coroutine coroutine;
    private List<Task> tasks;

    /***********************************************************************************
    **************** LIFECYCLE *********************************************************
    ************************************************************************************/
    private void Awake(){
        if(Instance != null){
            Debug.Log("There is more than one instance of CharacterController " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        this.coroutine = null;
        tasks = new List<Task>();
    }

    private void Start() {
        SelectUnit(this.selectedUnit);
        
        CreateActionButton();
        foreach (Transform button in targetButtonContainer){
            Destroy(button.gameObject);
        }
    }


    private void Update() {
        //CheckObjectHoveredByCursor();

        if(InputManager.Instance.IsLeftMouseDown()){
            OnLeftClick();
        }

        if(InputManager.Instance.IsRightMouseDown()){
            OnRightClick();
        }

    }


    /***********************************************************************************
    **************** SELECTING ACTION AND UNIT *****************************************
    ************************************************************************************/
    private bool CheckObjectHoveredByCursor(){

        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), float.MaxValue);

        if(hits.Length <= 0){
            MouseWorld.Instance.ChangeCursorX();
            return false;
        }

        if(selectedUnit == null)
            return false;

        bool wasCursorChanged = false;
        for(int i=0; i<hits.Length; i++){
            if(wasCursorChanged = SelectObject<Targetable>(hits[i].transform, MouseWorld.Instance.ChangeCursorEye))
                break;
            
        }

        if(!wasCursorChanged)
            MouseWorld.Instance.ChangeCursorStandard();
        


        return true;
    }

    private bool SelectObject<T>(Transform obj, Action changeCursor){
        if(this.selectedUnit.transform == obj)
            return false;

        if(obj.TryGetComponent<T>(out T t)){
            this.objectHit = obj;
            changeCursor();
            return true;
        }
        return false;
    }
    private async void SelectUnit(Unit unit){
        if(unit == null) return;
        if(unit == selectedUnit) return;
        if(unit is Enemy) return;
        // Try to select unit
        this.selectedUnit = unit;
        await SetSelectedAction(unit.GetAction<ActionMove>());
        CreateActionButton();
        this.OnSelectedUnitChanged?.Invoke(this.selectedUnit);
    }
    private async Task SelectUnit(){
        // Try to select unit
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask)){
            if(raycastHit.transform.TryGetComponent<Unit>(out Unit unit)){
                SelectUnit(unit);
                return;
            }
        }
        // Deselecting current action
        if(!(this.selectedAction is ActionMove) && this.selectedUnit != null)
            await SetSelectedAction(this.selectedUnit.GetAction<ActionMove>());
    }

    private async Task SetSelectedAction(ActionBase action){
        if(action == null) return;

        if(selectedAction != null)
            selectedAction.UnReady();

        
        foreach (Transform button in targetButtonContainer){
            Destroy(button.gameObject);
        }
        
        selectedAction = action;
        this.OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        selectedAction.Ready();
        await Task.Yield();
    }

    /*************** MOUSE ************************************************************/
    private void OnLeftClick(){
        if(EventSystem.current.IsPointerOverGameObject())
            return;

        TryNextTask(() => SelectUnit());
    }

    private void OnRightClick(){
        if(this.selectedAction == null || this.selectedUnit == null)
            return;
        
        TryNextTask(() => this.selectedAction.Take(), 600);
    }


    /***********************************************************************************
    **************** INTERFACE *********************************************************
    ************************************************************************************/

    private void CreateActionButton(){
        foreach (Transform button in actionButtonContainer)
            Destroy(button.gameObject);
        foreach (Transform button in passiveButtonContainer)
            Destroy(button.gameObject);

        if(this.selectedUnit == null)
            return;

        List<ActionBase> actions = this.selectedUnit?.GetAvailableActions();

        actions.Sort((a1, a2) => {
            return a1.GetUIpriority().CompareTo(a2.GetUIpriority());
        });

        foreach (ActionBase action in actions){
            Transform button = Instantiate(actionButtonPrefab, actionButtonContainer);
            button.GetComponent<ActionButton>().SetBaseAction(
                () => SetSelectedAction(action), action);
        }

        GeneralPassive passive = this.selectedUnit.GetPassive<GeneralPassive>();
        if(passive != null){
            //Debug.Log("passive.GetPassiveButton() null?? " + passive.GetPassiveButton() == null);
            Transform button = Instantiate(passive.GetPassiveButton(), passiveButtonContainer);
            button.GetComponent<PassiveButton>().SetPassive(passive);

        }
        else 
            Debug.Log($"{this.selectedUnit} doesn't have a passive!");

    }


    public void CreateTargetButtons(List<Targetable.TargetInfo> targets){
        foreach (Transform button in targetButtonContainer){
            Destroy(button.gameObject);
        }

        for(int i=0; i<targets.Count; i++){
            Transform button = Instantiate(targetButtonPrefab, targetButtonContainer);
            button.GetComponent<TargetButton>().SetTargets(this.selectedAction as AttackRanged, targets[i]);
        }
    }

    /***********************************************************************************
    **************** GETTERS ***********************************************************
    ************************************************************************************/
    public Unit GetSelectedUnit(){
        return this.selectedUnit;
    }





    /***********************************************************************************
    **************** COMMAND FUNCTIONS *************************************************
    ************************************************************************************/
    public async void TryNextTask(Func<Task> task, int delay = 0){
        if(tasks.Count > 0) return;

        this.tasks.Add(task());

        await Task.WhenAll(this.tasks);
        await Task.Delay(delay);
        this.tasks.Clear();
    }



}
