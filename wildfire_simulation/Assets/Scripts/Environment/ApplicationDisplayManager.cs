using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the camera feeds, overlays, and UI display logic for the drone fleet.
/// This script is responsible for coordinating which drone is currently being viewed,
/// and for updating camera viewports and UI elements accordingly.
/// </summary>
public class ApplicationDisplayManager : MonoBehaviour
{
    [Header("Drone Manager Reference")]
    public DroneManager droneManager; ///< Reference to the DroneManager script

    [Header("UI Elements")]
    public Text droneNameText; ///< UI Text element for displaying the active drone's name

    [Header("Fixed External Cameras")]
    public Camera mainCamera;   ///< Top-right camera (scene overview)
    public Camera secondCamera; ///< Bottom-right camera (auxiliary feed)

    [Header("Message UI")]
    public GameObject messageEntryPrefab; // Drag your prefab here
    public Transform messageListContent;  // Drag "Content" GameObject here

    private int currentDroneIndex = 0; ///< Index of the currently active drone

    public Transform detailContentParent;         // Drag the "Details/Viewport/Content" transform here

    // ──────────────────────────────────────────────────────────────────────


    void Start()
    {
        if (droneManager == null)
        {
            Debug.LogError("[ApplicationDisplayManager] DroneManager reference not assigned.");
            enabled = false;
            return;
        }

        SwitchToDrone(0); // Display the first drone by default
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            SwitchDrone(1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            SwitchDrone(-1);
    }

    /// <summary>
    /// Switches to the next or previous drone based on direction.
    /// </summary>
    /// <param name="direction">+1 for next, -1 for previous</param>
    void SwitchDrone(int direction)
    {
        int count = droneManager.Drones.Count;
        if (count == 0) return;

        currentDroneIndex = (currentDroneIndex + direction + count) % count;
        SwitchToDrone(currentDroneIndex);

        // Clear previous blocks
        foreach (Transform child in detailContentParent) {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Activates the display of a specific drone, updating cameras and UI.
    /// </summary>
    /// <param name="index">Index of the drone to display</param>
    void SwitchToDrone(int index)
    {
        IReadOnlyList<GameObject> drones = droneManager.Drones;

        for (int i = 0; i < drones.Count; i++)
        {
            Transform followerCam = drones[i].transform.Find("FollowerCamera");
            Transform bottomCam = drones[i].transform.Find("BottomCamera");

            if (followerCam != null)
            {
                Camera cam = followerCam.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.enabled = (i == index);
                    if (i == index)
                        cam.rect = new Rect(0f, 0f, 0.5f, 0.5f); // bottom-left
                }

                AudioListener listener = followerCam.GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = (i == index);
            }

            if (bottomCam != null)
            {
                Camera cam = bottomCam.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.enabled = (i == index);
                    if (i == index)
                        cam.rect = new Rect(0f, 0.5f, 0.5f, 0.5f); // top-left
                }
            }

            // Optionally show/hide per-drone UI (e.g., logs or HUD)
            Canvas droneCanvas = drones[i].GetComponentInChildren<Canvas>(true);
            if (droneCanvas != null)
                droneCanvas.enabled = (i == index);
        }

        if (droneNameText != null)
            droneNameText.text = drones[index].name;

        // Ensure external cameras stay enabled
        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f); // top-right
            mainCamera.depth = 0;
            mainCamera.enabled = true;
        }

        if (secondCamera != null)
        {
            secondCamera.rect = new Rect(0.5f, 0f, 0.5f, 0.5f); // bottom-right
            secondCamera.depth = 0;
            secondCamera.enabled = true;
        }

        Debug.Log($"[ApplicationDisplayManager] Switched view to drone: {drones[index].name}");

        // Clear existing messages
        foreach (Transform child in messageListContent)
        {
            Destroy(child.gameObject);
        }

        // Get the RF module from the drone
        RfModule rf = drones[index].GetComponent<RfModule>();
        if (rf != null)
        {
            var sentMessages = rf.GetSentMessages();
            if (sentMessages != null)
            {
                foreach (var msg in sentMessages)
                {
                    if (msg == null) continue;
                    GameObject entry = Instantiate(messageEntryPrefab, messageListContent);
                    entry.GetComponent<MessageEntry>().Setup(msg, true);
                }
            }

            var receivedMessages = rf.GetReceivedMessages();
            if (receivedMessages != null)
            {
                foreach (var msg in receivedMessages)
                {
                    if (msg == null) continue;
                    GameObject entry = Instantiate(messageEntryPrefab, messageListContent);
                    entry.GetComponent<MessageEntry>().Setup(msg, false);
                }
            }
        }
    }
}
