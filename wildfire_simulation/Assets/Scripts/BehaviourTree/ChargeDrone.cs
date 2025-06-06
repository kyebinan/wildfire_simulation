using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class ChargeDrone : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.droneController == null) {
            Debug.LogWarning("[ChargeDrone] droneController not found in context.");
            return State.Failure;
        }

        context.droneController.RechargeBatteryFull();
        context.droneController.SetMotorState(true);
        return State.Success;
    }
}
