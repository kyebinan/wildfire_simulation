using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IotModule : MonoBehaviour
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
    }

    void Update()
    {
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 25);
    }
}
