using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class MoveToDestination : ActionNode {

    private const float positionTolerance = 1f;      // Tolerance in meters
    private const float velocityTolerance = 0.5f;      // Considered stable if velocity < 0.5 m/s

    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.droneController == null){
            Debug.LogWarning("[MoveToDestination] Missing droneController or Rigidbody in context.");
            return State.Failure;
        }
        Vector3 currentPosition = context.droneController.transform.position;
        Vector3 targetPosition = context.droneController.GetDestination();

        float distance = Vector3.Distance(currentPosition, targetPosition);
        float velocity = context.physics.velocity.magnitude;

        if (distance > positionTolerance || velocity > velocityTolerance){
            return State.Running;
        }

        return State.Success;
    }
}
