using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LarryChaseState : LarryBaseState
{
    public override void EnterState(LarryStateManager manager){
        manager.CurrentAIState = State.Chase;
    }
    public override void UpdateState(LarryStateManager manager, Collider[] areaCheckResults){
        
    }
}
