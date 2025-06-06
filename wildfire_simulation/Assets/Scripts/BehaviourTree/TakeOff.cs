using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class TakeOff : ActionNode
{
    private const float positionTolerance = 0.5f;      // Tolerance in meters
    private const float velocityTolerance = 0.05f;      // Considered stable if velocity < 0.05 m/s

    protected override void OnStart()
    {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.droneController == null) {
            Debug.LogWarning("[TakeOff] Missing droneController or Rigidbody in context.");
            return State.Failure;
        }

        Vector3 currentPosition = context.droneController.transform.position;
        Vector3 landingStation = context.droneController.GetLandingStation();
        Vector3 newDestination = new Vector3(landingStation.x, context.droneController.Altitude , landingStation.z);
        // Set the destination on the hexacopter controller
        context.droneController.SetDestination(newDestination);

        float distance = Vector3.Distance(currentPosition, newDestination);
        float velocity = context.physics.velocity.magnitude;

        if (distance > positionTolerance || velocity > velocityTolerance) {
            return State.Running;
        }
        return State.Success;
    }
}
