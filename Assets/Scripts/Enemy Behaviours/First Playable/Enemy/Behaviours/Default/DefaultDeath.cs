using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultDeath : AIBehaviour
{
    // Currently just destorying the game object.
    public override void OnStateEnter()
    {
        enemyHandler.Kill();
    }

    public override void OnStateExit() {}

    public override void OnStateFixedUpdate() {}

    public override void OnStateUpdate() {}
}
