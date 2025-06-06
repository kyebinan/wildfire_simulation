using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using Unity.VisualScripting;

public class TakeMyAreasMessage : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
         if (context.droneController == null) {
            Debug.LogWarning("[TakeMyAreasMessage] droneController not found in context.");
            return State.Failure;
        }

        // context.droneController.rfComponent.SendBroadcastMessage("Need help ! My battery are low.");

        return State.Success;
    }
}
