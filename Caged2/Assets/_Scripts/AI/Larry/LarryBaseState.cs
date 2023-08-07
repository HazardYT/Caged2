using UnityEngine;

public abstract class LarryBaseState
{
    public abstract void EnterState(LarryStateManager manager);

    public abstract void UpdateState(LarryStateManager manager, Collider[] areaCheckResults);
}
