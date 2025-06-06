using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class IsDroneOnBase : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.droneController == null) {
            Debug.LogWarning("[IsDroneOnBase] droneController not found in context.");
            return State.Failure;
        }

        Vector3 currentPosition = context.droneController.transform.position;
        Vector3 landingStation = context.droneController.GetLandingStation();
        float distance = Vector3.Distance(currentPosition, landingStation);
    
        if (distance < 0.1)
            return State.Success;
        
        return State.Failure;
    }
}
