using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class GoBackToStation : ActionNode
{
    private const float positionTolerance = 0.25f;      // Tolerance in meters
    private const float velocityTolerance = 0.05f;  
    private bool hasReachedAbove = false; // Phase tracking
    protected override void OnStart()
    {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.droneController == null || context.physics == null) {
            Debug.LogWarning("[GoBackToStation] Missing droneController or Rigidbody in context.");
            return State.Failure;
        }

        Vector3 currentPosition = context.droneController.transform.position;
        Vector3 landingStation = context.droneController.GetLandingStation();

        // Phase 1: Go above the landing pad
        if (!hasReachedAbove) {
            Vector3 aboveLanding = new Vector3(landingStation.x, context.droneController.Altitude, landingStation.z);
            context.droneController.SetDestination(aboveLanding);

            float horizontalDistance = Vector2.Distance(
                new Vector2(currentPosition.x, currentPosition.z),
                new Vector2(aboveLanding.x, aboveLanding.z)
            );
            float verticalDifference = Mathf.Abs(currentPosition.y - aboveLanding.y);
            float velocity = context.physics.velocity.magnitude;

            if (horizontalDistance <= positionTolerance && verticalDifference <= positionTolerance && velocity <= velocityTolerance) {
                hasReachedAbove = true;
                Debug.Log("[GoBackToStation] Reached position above landing station. Preparing to descend...");
            } else {
                return State.Running;
            }
        }

        // Phase 2: Descend vertically
        context.droneController.SetDestination(landingStation);

        float fullDistance = Vector3.Distance(currentPosition, landingStation);
        float finalVelocity = context.physics.velocity.magnitude;

        if (fullDistance > positionTolerance || finalVelocity > velocityTolerance) {
            return State.Running;
        }

        // Successful vertical landing
        context.droneController.SetMotorState(false);
        Debug.Log("[GoBackToStation] Drone landed successfully.");
        return State.Success;
    }
}
