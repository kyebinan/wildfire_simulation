using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class SendMessage : ActionNode
{
    public DroneMessageType messageType;
    private SimulationClock simClock;

    protected override void OnStart()
    {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (context.droneController == null || context.droneController.RFComponent == null) {
            Debug.LogWarning("[SendMessage] Missing droneController or RF module.");
            return State.Failure;
        }

        simClock = FindObjectOfType<SimulationClock>();
        if (simClock == null) {
            Debug.LogError("[SendMessage] No SimulationClock found in the scene!");
        }

        string sourceId = context.droneController.name;
        string destination = "All"; // default to broadcast
        string data = "";
        string timeStamp = $"{simClock.GetFormattedTime()}";
        Vector3 location = context.droneController.transform.position;

        // Message msg = null;

        switch (messageType) {
            case DroneMessageType.FireAlert:
                // msg = new Message("Fire detected", sourceId, destination, DroneMessageType.FireAlert, "🔥 detected near sector A3");
                // location: GPS or world coordinates
                // confidence: confidence score (0–1)
                Vector3 fireLocation = new Vector3();
                float confidence = 1f;
                data = $"Fire Location: X:{fireLocation.x}, Y:{fireLocation.z} | Confidence: {confidence}";
                context.droneController.RFComponent.FireDectectionAlertMessage(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.StatusUpdate:
                // position: current location
                // batteryLevel: remaining %
                // temperature, altitude, etc.
                float battery = context.droneController.GetBatteryLevel();
                float temperature = 35f;
                float altitude = context.droneController.Altitude;
                data = $"Battery: {Mathf.RoundToInt(battery)}% | Temperature: {temperature}° | Altitude: {altitude}";
                context.droneController.RFComponent.StatusUpdateMessage(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.TaskDelegation:
                // msg = new Message("Task delegation", sourceId, "Drone_02", DroneMessageType.TaskDelegation, "Take over scanning zone B5");
                // targetDroneId: receiving drone ID
                // task: "ScanSector", "RelayComms"...
                // areaId: area to scan
                // urgency: level or deadline
                destination = "Drone_00";
                data = $"AreaIds: [...] | Task: [...] | Urgency: [...]";
                context.droneController.RFComponent.TaskDelegationMessage(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.PathUpdate:
                // msg = new Message("Path coordination", sourceId, destination, DroneMessageType.PathUpdate, "ETA to B2: 60s");
                // plannedPath: list of waypoints
                // ETA: estimated time of arrival
                // priorityLevel
                data = $"Planned Path: [...] | ETA: [...] | Priority Level: [...]";
                context.droneController.RFComponent.PathUpdateMessage(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.SensorReading:
                // msg = new Message("Sensor data", sourceId, destination, DroneMessageType.SensorReading, "Thermal: 75°C");
                // sensorType: "thermal", "smoke", "gas"
                // readingValue: raw or processed data
                // location
                data = $"Sensor Type: thermal | Reading Value: [...] | location: [...]";
                context.droneController.RFComponent.SensorReadingMessage(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.Warning:
                // msg = new Message("Warning", sourceId, destination, DroneMessageType.Warning, "Obstacle detected ahead");
                // source: drone ID
                // issueType: "ObstacleDetected", "BatteryLow"
                // location
                data = $"Issue Type: BatteryLow";
                context.droneController.RFComponent.WarningMessage(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.AnomalyReport:
                // msg = new Message("Anomaly", sourceId, destination, DroneMessageType.AnomalyReport, "GPS drift suspected");
                // issueType: "ObstacleDetected", "BatteryLow"
                // location
                // severity: info, warning, critical
                data = $"Issue Type: [...] | Severity: [...]";
                context.droneController.RFComponent.AnomalyReportMessage(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.ConsensusVote:
                // msg = new Message("Vote cast", sourceId, destination, DroneMessageType.ConsensusVote, "Proposal: Change Leader – Vote: YES");
                context.droneController.RFComponent.ConsensusVoteMessage(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.Relay:
                // msg = new Message("Forwarding data", sourceId, "GroundStation", DroneMessageType.Relay, "Payload relayed");
                context.droneController.RFComponent.RelayMessage(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.CertificateRequest:
                // msg = new Message("Cert Request", sourceId, "AuthDrone", DroneMessageType.CertificateRequest, "Requesting PKI certificate");
                context.droneController.RFComponent.CertificateRequestMessageMAKI(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.CertificateReply:
                // msg = new Message("Cert Reply", sourceId, destination, DroneMessageType.CertificateReply, "Certificate issued");
                context.droneController.RFComponent.CertificateReplyMessageMAKI(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.KeyDistribution:
                // msg = new Message("Key Distribution", sourceId, destination, DroneMessageType.KeyDistribution, "Distribute session key K1");
                context.droneController.RFComponent.KeyDistributionMessageMAKI(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.KeyRevocation:
                // msg = new Message("Key Revocation", sourceId, destination, DroneMessageType.KeyRevocation, "Revoke key K2");
                context.droneController.RFComponent.KeyRevocationMessageMAKI(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.TrustEvaluation:
                // msg = new Message("Trust Eval", sourceId, destination, DroneMessageType.TrustEvaluation, "Drone_07: trust score = 0.82");
                context.droneController.RFComponent.TrustEvaluationMessageMAKI(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.Challenge:
                // msg = new Message("Challenge", sourceId, "Drone_09", DroneMessageType.Challenge, "Auth nonce: 7832");
                break;

            case DroneMessageType.ChallengeResponse:
                // msg = new Message("ChallengeResponse", sourceId, "Drone_09", DroneMessageType.ChallengeResponse, "Response nonce: 7832-hash");
                break;

            case DroneMessageType.Heartbeat:
                // msg = new Message("Heartbeat", sourceId, destination, DroneMessageType.Heartbeat, "Still alive");
                context.droneController.RFComponent.HeartbeatMessageMAKI(location, sourceId, destination, data, timeStamp);
                break;

            case DroneMessageType.Log:
                // msg = new Message("Log", sourceId, destination, DroneMessageType.Log, "Battery check passed");
                break;

            case DroneMessageType.DiagnosticReport:
                // msg = new Message("Diagnostics", sourceId, destination, DroneMessageType.DiagnosticReport, "No anomaly found");
                break;

            // default:
            //     Debug.LogWarning("[SendMessage] Unknown message type.");
            //     return State.Failure;
        }

        // context.droneController.rfComponent.SendBroadcastMessage(msg);
        // Debug.Log($"[SendMessage] Sent: {msg.Type} from {msg.Source} to {msg.Destination}");

        return State.Success;
    }

}
