using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;
using System.Linq;

public class ComputeFlightPlan : ActionNode {
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.droneController == null || context.droneController.Areas.Count == 0) {
            Debug.LogWarning("[ComputeFlightPlan] Missing droneController or empty area list.");
            return State.Failure;
        }

        List<Vector3> patrolArea = context.droneController.Areas;
        Vector3 coord = patrolArea[context.droneController.IndexArea];
        Vector3 target = new Vector3(coord.x, context.droneController.Altitude, coord.z);
        Vector3 current = context.droneController.transform.position;

        float distanceToTarget = Vector3.Distance(current, target);

        if (distanceToTarget > 500f) {
            context.droneController.SetSubGoals(current, target, 250f);
            context.droneController.SetDestination(context.droneController.subGoals.Dequeue());
        } else {
            context.droneController.SetDestination(target);
        }

        context.droneController.IndexArea = (context.droneController.IndexArea + 1) % context.droneController.Areas.Count;

        return State.Success;
    }

}
