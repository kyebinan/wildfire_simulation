using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class IsDroneCharged : ActionNode
{
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.droneController == null) {
            Debug.LogWarning("[IsDroneCharged] droneController not found in context.");
            return State.Failure;
        }

        if (context.droneController.GetBatteryLevel() == 100f)
        {
            // Activate motors
            context.droneController.SetMotorState(true);
            Debug.Log($"[IsDroneCharged] {context.droneController.name} is charged.");
            return State.Success;
        }

        return State.Failure;
    }
}
