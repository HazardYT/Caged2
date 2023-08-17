using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class LarryWanderState : LarryBaseState
{
    bool MovingToWalkPoint;
    public override void EnterState(LarryStateManager manager){
        manager.CurrentAIState = State.Wander;
        manager.StartCoroutine(FindWalkPointFromPOI(manager));
        Debug.Log("Wander state hi");
    }
    public override void UpdateState(LarryStateManager manager, Collider[] areaCheckResults){
        
        SearchForPlayers(manager, areaCheckResults);
        HandleWalkPoint(manager);
        ListenForSounds(manager);
        SearchForHidingSpots(manager);
    }
    void SearchForPlayers(LarryStateManager manager, Collider[] results){
        foreach(Collider collider in results){
            if (collider == null) continue;
            if (Physics.Raycast(manager.transform.position, collider.transform.position - manager.transform.position, out RaycastHit hit, manager._checkRadius, manager.layerMask)){
                if (hit.transform.CompareTag("Player")){
                    if (Vector3.Distance(manager.transform.position, collider.transform.position) <= manager._chaseRadius){
                        manager._Target = collider.transform;
                        manager.SwitchState(manager.ChaseState);
                    }
                }
            }
        }
    }
    private void ListenForSounds(LarryStateManager manager)
    {
        AudioSource[] audioSources = Object.FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source.isPlaying)
            {
                if (source.gameObject.CompareTag("Listenable"))
                {
                    float distance = Vector3.Distance(source.transform.position, manager.transform.position);
                    if (distance <= manager._hearingDistance)
                    {
                        float hearingVolume = Mathf.Lerp(0.2f, 0.8f, distance / manager._hearingDistance);
                        if (source.volume > hearingVolume)
                        {
                            if (source.volume > manager._hearingChaseVolume){
                                manager._Target = source.transform;
                                manager.SwitchState(manager.ChaseState);
                            }
                            else{
                                if (NavMesh.SamplePosition(source.transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas)){
                                    manager._walkPointPosition = hit.position;
                                    manager._walkPointSet = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    void SearchForHidingSpots(LarryStateManager manager){

    }

    void HandleWalkPoint(LarryStateManager manager){
        if (!manager._walkPointSet) SearchForWalkPoint(manager);

        if (manager.agent.pathStatus == NavMeshPathStatus.PathComplete){
            if (!MovingToWalkPoint) manager.StartCoroutine(MoveToWalkPoint(manager));
            Debug.DrawLine(manager.transform.position, manager._walkPointPosition, Color.blue);
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
        int attempts = 0;
        do
        {
            attempts++;

            float stepLength = Random.Range(manager._wanderDistance / 2, manager._wanderDistance);

            Vector2 randomDirection = Random.insideUnitCircle.normalized * stepLength;

            Vector3 nextPosition = manager.transform.position + new Vector3(randomDirection.x, 0f, randomDirection.y);

            NavMesh.SamplePosition(nextPosition, out NavMeshHit hit, manager._wanderDistance, NavMesh.AllAreas);
            if (manager.agent.pathStatus == NavMeshPathStatus.PathComplete){
            manager._walkPointPosition = hit.position;
            manager._walkPointSet = true;
            }

        } while (attempts < manager._maxWalkPointAttempts);

        // If max attempts reached, set agent's destination to a random point within _walkpointDistance
        Vector2 randomDirectionFallback = Random.insideUnitCircle.normalized * manager._wanderDistance;
        Vector3 fallbackPosition = manager.transform.position + new Vector3(randomDirectionFallback.x, 0f, randomDirectionFallback.y);
        NavMesh.SamplePosition(fallbackPosition, out NavMeshHit fallbackHit, manager._wanderDistance, NavMesh.AllAreas);

        manager._walkPointPosition = fallbackHit.position;
        manager._walkPointSet = true;

        Debug.LogWarning("Failed to find a suitable walk point after " + attempts + " attempts. Using fallback point.");
    }
    IEnumerator FindWalkPointFromPOI(LarryStateManager manager){
        while (manager.CurrentAIState == State.Wander)
        {
            yield return new WaitForSeconds(manager._wanderToPOIDelay);

            int i = Random.Range(0, manager._POIList.Length);
            manager._currentPOI = manager._POIList[i];

            Vector3 randomPoint2D = Random.insideUnitCircle.normalized * 3;

            Vector3 randomPoint3D = manager._currentPOI.position + new Vector3(randomPoint2D.x, 0f, randomPoint2D.y);

            NavMesh.SamplePosition(randomPoint3D, out NavMeshHit navMeshHit, manager._wanderDistance, NavMesh.AllAreas);
            manager._walkPointPosition = navMeshHit.position;
            manager._walkPointSet = true;

        }
    }


}
