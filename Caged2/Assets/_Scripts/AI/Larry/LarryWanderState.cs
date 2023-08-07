using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class LarryWanderState : LarryBaseState
{
    bool MovingToWalkPoint;
    public override void EnterState(LarryStateManager manager){
        manager.CurrentAIState = State.Wander;
        Debug.Log("Wander state hi");
    }
    public override void UpdateState(LarryStateManager manager, Collider[] areaCheckResults){

        HandleWalkPoint(manager);
        SearchForPlayers();
        ListenForSounds();
        SearchForHidingSpots();
    }
    void SearchForPlayers(){
        
    }
    void ListenForSounds(){

    }
    void SearchForHidingSpots(){

    }
    void HandleWalkPoint(LarryStateManager manager){
        if (!manager._walkPointSet) SearchForWalkPoint(manager);

        if (manager.agent.pathStatus == NavMeshPathStatus.PathComplete){
            if (!MovingToWalkPoint) manager.StartCoroutine(MoveToWalkPoint(manager));
            Debug.DrawLine(manager.transform.position, manager._walkPointPosition, Color.yellow);
        }
        else{ 
            manager._walkPointSet = false;
        }
        if (Vector3.Distance(manager.transform.position, manager._walkPointPosition) < 2f){
            manager._walkPointSet = false;
        }
    }
    IEnumerator MoveToWalkPoint(LarryStateManager manager){
        MovingToWalkPoint = true;
        manager.agent.speed = Random.Range(manager._wanderSpeedRange.x, manager._wanderSpeedRange.y);
        yield return new WaitForSeconds(Random.Range(manager._wanderWaitRange.x, manager._wanderWaitRange.y));
        manager.agent.SetDestination(manager._walkPointPosition);
        MovingToWalkPoint = false;
    }
    void SearchForWalkPoint(LarryStateManager manager)
    {
        bool validWalkPoint = false;
        int attempts = 0;

        do
        {
            attempts++;

            float randomAngle = Random.Range(-Mathf.PI /1.5f, Mathf.PI /1.5f);
            float x = manager._wanderDistance * Mathf.Cos(randomAngle);
            float z = manager._wanderDistance * Mathf.Sin(randomAngle);

            Vector3 rotatedDirection = new(x, 0, z);
            Vector3 walkPointDirection = new(manager.transform.position.x + rotatedDirection.x, manager.transform.position.y, manager.transform.position.z + rotatedDirection.z);
            NavMesh.SamplePosition(walkPointDirection, out NavMeshHit hit, manager._wanderDistance, NavMesh.AllAreas);

            if (manager.agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                validWalkPoint = true;
            }
            if (validWalkPoint)
            {
                manager._walkPointPosition = hit.position;
                manager._walkPointSet = true;
                Debug.Log("(SearchWalkPoint) - Found Valid Walkpoint");
                return;
            }
            else
            {
                Debug.Log("(SearchWalkPoint) - Finding Random Walkpoint");
                Vector3 randomDirection = Random.insideUnitSphere * manager._wanderDistance;
                randomDirection += manager.transform.position;
                NavMesh.SamplePosition(randomDirection, out hit, manager._wanderDistance, NavMesh.AllAreas);

                if (manager.agent.pathStatus == NavMeshPathStatus.PathComplete)
                {
                    manager._walkPointPosition = hit.position;
                    manager._walkPointSet = true;
                    return;
                }
            }
        } while (!validWalkPoint && attempts < manager._maxWalkPointAttempts);
    }
}
