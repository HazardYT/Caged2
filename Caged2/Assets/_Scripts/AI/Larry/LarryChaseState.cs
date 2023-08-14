using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LarryChaseState : LarryBaseState
{
    public override void EnterState(LarryStateManager manager){
        manager.CurrentAIState = State.Chase;
        manager.agent.speed = manager._agentChaseSpeed;
    }
    public override void UpdateState(LarryStateManager manager, Collider[] areaCheckResults){
        ChasePlayer(manager, areaCheckResults);
    }
    void ChasePlayer(LarryStateManager manager, Collider[] areaCheckResults) {
        bool foundPlayer = false; // Track if player was found within chaseRadius



        foreach (Collider obj in areaCheckResults) {
            if (obj == null) continue;
            if (obj.CompareTag("Player")) {
                if (!foundPlayer) {
                    // Player not previously found, check if player is within chaseRadius now
                    if (Vector3.Distance(manager.transform.position, obj.transform.position) <= manager._chaseRadius) {
                        if (Physics.Raycast(manager.transform.position, obj.transform.position - manager.transform.position, out RaycastHit hit, manager.layerMask)) {
                            foundPlayer = true;
                        }
                    }
                }
                if (Vector3.Distance(manager.transform.position, obj.transform.position) <= manager._checkRadius) {
                    if (Physics.Raycast(manager.transform.position, obj.transform.position - manager.transform.position, out RaycastHit hit, manager.layerMask)) {
                        if (hit.transform.CompareTag("Player")){
                            if (Vector3.Distance(manager.transform.position, hit.transform.position) <= manager._attackRadius) {
                                Debug.Log("Attack");
                                return;
                            }
                            manager._Target = hit.transform;
                            manager.agent.SetDestination(manager._Target.position);
                            return;
                        }
                    }
                }
            }
        }

        if (foundPlayer) {
            // Player was found within chaseRadius or checkRadius, continue chasing
            manager.agent.SetDestination(manager._Target.position);
        } else {
            // Player not found, switch to wander state
            manager.SwitchState(manager.WanderState);
        }
    }
}
