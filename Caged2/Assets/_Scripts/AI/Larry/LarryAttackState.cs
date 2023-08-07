using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LarryAttackState : LarryBaseState
{
    public override void EnterState(LarryStateManager manager){
        manager.CurrentAIState = State.Attack;
    }
    public override void UpdateState(LarryStateManager manager, Collider[] areaCheckResults){

    }
}
