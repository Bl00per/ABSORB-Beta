using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultStagger : AIBehaviour
{
    [Header("Properties")]
    public float staggerTime = 2.0f;

    private bool _isStaggered = false;

    public override void OnStateEnter()
    {
        if (enemyHandler.GetFunctional())
            brain.GetNavMeshAgent().isStopped = true;
            
        if (!_isStaggered)
            StartCoroutine(StaggerSequence());
    }

    public override void OnStateExit()
    {
        if (enemyHandler.GetFunctional())
            brain.GetNavMeshAgent().isStopped = false;
    }

    public override void OnStateFixedUpdate() { }

    public override void OnStateUpdate() { }

    private IEnumerator StaggerSequence()
    {
        _isStaggered = true;
        yield return new WaitForSecondsRealtime(staggerTime);
        _isStaggered = false;
        brain.SetBehaviour("Movement");
    }
}
