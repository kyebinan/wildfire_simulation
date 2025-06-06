using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class IsDroneDecharged : ActionNode
{
    public float level = 30f;
    protected override void OnStart()
    {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.droneController == null) {
            Debug.LogWarning("[IsBatteryDechargedAt20] droneController not found in context.");
            return State.Failure;
        }

        if (context.droneController.GetBatteryLevel() <= level)
            return State.Success;

        return State.Failure;
    }
}
