using System.Collections.Generic;
using UnityEngine;

public class RfModule : MonoBehaviour
{
    [Header("Transmission Settings")]
    // [SerializeField] private float radiusEmission = 300f;
    [SerializeField] private float transmitPowerDbm = 20f;    // P_tx in dBm
    [SerializeField] private float defaultNoiseFloorDbm = -90f; // Base noise floor
    [SerializeField] private float pathLossExponent = 2.0f;

    [Header("Message Buffer")]
    [SerializeField] private int messageQueueSize = 100;

    private Queue<Message> sentMessagesQueue;
    private Queue<Message> receivedMessagesQueue;

    public IEnumerable<Message> GetSentMessages() => sentMessagesQueue;
    public IEnumerable<Message> GetReceivedMessages() => receivedMessagesQueue;

    private float effectiveNoiseFloorDbm; // May be elevated by jammers

    void Start()
    {
        sentMessagesQueue = new Queue<Message>(messageQueueSize);
        receivedMessagesQueue = new Queue<Message>(messageQueueSize);
        effectiveNoiseFloorDbm = defaultNoiseFloorDbm;
    }

    void Update()
    {
        UpdateNoiseFloorFromEnvironment();
        ListenForReception();
    }

    /// <summary>
    /// Simulate dynamic noise floor based on position (e.g. jamming zones).
    /// </summary>
    void UpdateNoiseFloorFromEnvironment()
    {
        JammerArea[] jammers = FindObjectsOfType<JammerArea>();

        float highestNoise = defaultNoiseFloorDbm;

        foreach (var jammer in jammers)
        {
            if (jammer.IsInside(transform.position))
            {
                // If inside multiple jammers, use the strongest (least negative)
                if (jammer.noiseFloorDbm > highestNoise)
                    highestNoise = jammer.noiseFloorDbm;
            }
        }

        effectiveNoiseFloorDbm = highestNoise;
    }

    private void SendBroadcastMessage(Message newMessage)
    {
        if (sentMessagesQueue.Count >= messageQueueSize)
            sentMessagesQueue.Dequeue();
        sentMessagesQueue.Enqueue(newMessage);

        foreach (var other in FindObjectsOfType<RfModule>())
        {
            if (other == this) continue;

            float distance = Vector3.Distance(transform.position, other.transform.position);
            float receivedPowerDbm = EstimateReceivedPowerDbm(transmitPowerDbm, distance);

            if (receivedPowerDbm >= other.effectiveNoiseFloorDbm)
            {
                other.ReceiveMessage(newMessage);
            }
        }
    }

    float EstimateReceivedPowerDbm(float pTxDbm, float distanceMeters)
    {
        if (distanceMeters < 1f)
            distanceMeters = 1f;

        float pathLoss = 10f * pathLossExponent * Mathf.Log10(distanceMeters);
        float receivedPower = pTxDbm - pathLoss;
        return receivedPower;
    }

    private void ListenForReception()
    {
        foreach (var message in receivedMessagesQueue)
        {
            Debug.Log($"{name} Received: {message}");
        }
    }

    private void ReceiveMessage(Message receivedMessage)
    {
        if (receivedMessagesQueue.Count >= messageQueueSize)
            receivedMessagesQueue.Dequeue();
        receivedMessagesQueue.Enqueue(receivedMessage);

        Debug.Log($"{name} Message received: {receivedMessage}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 10);
    }

    public void FireDectectionAlertMessage(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Notify swarm or base of detected fire.
        // type: "FireAlert"
        // source: drone ID
        // location: GPS or world coordinates
        // confidence: confidence score (0–1)
        // timestamp: local or simulated time
        string type = DroneMessageStrings.TypeToString[DroneMessageType.FireAlert];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void StatusUpdateMessage(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Share real-time drone status.
        // type: "StatusUpdate"
        // source: drone ID
        // position: current location
        // batteryLevel: remaining %
        // temperature, altitude, etc.
        // assignedArea, destination
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.StatusUpdate];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void TaskDelegationMessage(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Ask another drone to handle a task.
        // type: "TaskDelegation"
        // source: delegating drone ID
        // targetDroneId: receiving drone ID
        // task: "ScanSector", "RelayComms"...
        // areaId: area to scan
        // urgency: level or deadline
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.TaskDelegation];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void PathUpdateMessage(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Avoid collisions and optimize coverage.
        // type: "PathUpdate" or "PositionForecast"
        // source: drone ID
        // plannedPath: list of waypoints
        // ETA: estimated time of arrival
        // priorityLevel
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.PathUpdate];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void SensorReadingMessage(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Share real-time sensor info.
        // type: "SensorReading"
        // source: drone ID
        // sensorType: "thermal", "smoke", "gas"
        // readingValue: raw or processed data
        // location
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.SensorReading];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void AnomalyReportMessage(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Report risks or failures.
        // type: "AnomalyReport"
        // source: drone ID
        // issueType: "ObstacleDetected", "BatteryLow"
        // location
        // severity: info, warning, critical
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.AnomalyReport];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }


     public void WarningMessage(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Report risks or failures.
        // type: "Warning"
        // source: drone ID
        // issueType: "ObstacleDetected", "BatteryLow"
        // location
        // severity: info, warning, critical
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.Warning];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void ConsensusVoteMessage(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: MARL, task assignment, or leader election.
        // type: "ConsensusVote"
        // proposalId
        // senderId
        // vote: "Yes", "No", or float weight
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.ConsensusVote];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void RelayMessage(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Relay data if direct link is lost.
        // type: "Relay"
        // originalSender
        // intermediate (optional)
        // destination
        // payload: full inner message
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.Relay];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void KeyDistributionMessageMAKI(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Share public key and certificate.
        // type: "MAKI_IdentityAnnouncement"
        // senderId
        // publicKey
        // certificate
        // timestamp
        // signature
        string type = DroneMessageStrings.TypeToString[DroneMessageType.KeyDistribution];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void CertificateRequestMessageMAKI(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Start secure communication.
        // type: "MAKI_KeyExchangeRequest"
        // senderId, receiverId
        // nonce
        // ephemeralPublicKey
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.CertificateRequest];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void CertificateReplyMessageMAKI(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Respond to key exchange request.
        // type: "MAKI_KeyExchangeResponse"
        // senderId, receiverId
        // sessionKeyFragment or encryptedKey
        // nonce
        // timestamp
        // signature
        string type = DroneMessageStrings.TypeToString[DroneMessageType.CertificateReply];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void TrustEvaluationMessageMAKI(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Share trust ratings for peers.
        // type: "MAKI_TrustUpdate"
        // sourceId
        // targetId
        // trustLevel: 0–1 or -1 (blacklisted)
        // evidence (optional)
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.TrustEvaluation];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void KeyRevocationMessageMAKI(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Invalidate a compromised drone.
        // type: "MAKI_Revocation"
        // revokedId
        // reason
        // initiatorId
        // signature
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.KeyRevocation];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }

    public void HeartbeatMessageMAKI(Vector3 location, string source, string destination, string data, string timeStamp)
    {
        // Purpose: Maintain active, authenticated presence.
        // type: "MAKI_Beacon"
        // senderId
        // sessionToken
        // signature or HMAC
        // timestamp
        string type = DroneMessageStrings.TypeToString[DroneMessageType.Heartbeat];
        Message newMessage = new Message(location, source, destination, type, data, timeStamp);
        SendBroadcastMessage(newMessage);
    }
}


/// <summary>
/// Represents a network message in the simulation.
/// </summary>
public class Message
{
    public Vector3 SenderPosition { get; private set; }
    public string Source { get; private set; }        // Sender drone name or ID
    public string Destination { get; private set; }   // Target drone name or ID (can be "Broadcast")
    public string Type { get; private set; }      // e.g., "RF", "UDP", "Ping"
    public string Data { get; private set; }          // Payload or structured message
    public string TimeStamp { get; private set; }

    public Message(Vector3 senderPosition, string source = "", string destination = "", string type = "", string data = "", string timeStamp = "")
    {
        SenderPosition = senderPosition;
        Source = source;
        Destination = destination;
        Type = type;
        Data = data;
        TimeStamp = timeStamp;
    }

    public override string ToString()
    {
        return $"[{Type}] {Source} → {Destination} | {Data}";
    }
}


/// <summary>
/// Defines all possible types of messages exchanged in the drone swarm.
/// </summary>
public enum DroneMessageType {

    // ──────────────── Core Mission Messages ────────────────

    FireAlert,              // 🔥 Fire detection alert
    StatusUpdate,           // 📍 Periodic drone status broadcast
    TaskDelegation,         // 🤝 Task handoff or delegation
    PathUpdate,             // 🧭 Planned trajectory update
    SensorReading,          // 📡 Sensor data (thermal, smoke, gas, etc.)
    Warning,                // ⚠️ Warnings like low battery, obstacle, jam
    AnomalyReport,          // ⚠️ Detailed anomaly report
    ConsensusVote,          // 🧠 Consensus / voting / coordination message
    Relay,                  // 🛰️ Data relay for out-of-range communication

    // ──────────────── Security / MAKI-Specific Messages ────────────────

    CertificateRequest,     // 🛡️ Request for public key certificate
    CertificateReply,       // 🔐 Response containing public key certificate
    KeyDistribution,        // 🔑 Distribution of session or symmetric keys
    KeyRevocation,          // 🗑️ Notification to revoke a compromised key
    TrustEvaluation,        // 📊 Sharing trust scores or feedback
    Challenge,              // 🕵️ Cryptographic or trust challenge
    ChallengeResponse,      // 🔐 Response to a challenge

    // ──────────────── System / Maintenance ────────────────

    Heartbeat,              // 🫀 Keep-alive ping to ensure node presence
    Log,                    // 📜 Logging/debugging info
    DiagnosticReport,       // 🛠️ Internal health or debug report
}

public static class DroneMessageStrings {
    public static readonly Dictionary<DroneMessageType, string> TypeToString = new Dictionary<DroneMessageType, string> {
        // ───── Core Mission Messages ─────
        { DroneMessageType.FireAlert, "FireAlert" },
        { DroneMessageType.StatusUpdate, "StatusUpdate" },
        { DroneMessageType.TaskDelegation, "TaskDelegation" },
        { DroneMessageType.PathUpdate, "PathUpdate" },
        { DroneMessageType.SensorReading, "SensorReading" },
        { DroneMessageType.Warning, "Warning" },
        { DroneMessageType.AnomalyReport, "AnomalyReport" },
        { DroneMessageType.ConsensusVote, "ConsensusVote" },
        { DroneMessageType.Relay, "Relay" },

        // ───── Security / MAKI ─────
        { DroneMessageType.CertificateRequest, "CertificateRequest" },
        { DroneMessageType.CertificateReply, "CertificateReply" },
        { DroneMessageType.KeyDistribution, "KeyDistribution" },
        { DroneMessageType.KeyRevocation, "KeyRevocation" },
        { DroneMessageType.TrustEvaluation, "TrustEvaluation" },
        { DroneMessageType.Challenge, "Challenge" },
        { DroneMessageType.ChallengeResponse, "ChallengeResponse" },

        // ───── System / Maintenance ─────
        { DroneMessageType.Heartbeat, "Heartbeat" },
        { DroneMessageType.Log, "Log" },
        { DroneMessageType.DiagnosticReport, "DiagnosticReport" },
    };
}