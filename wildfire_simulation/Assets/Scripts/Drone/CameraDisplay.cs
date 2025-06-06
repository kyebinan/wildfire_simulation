using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CameraDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    private Text batteryText;          ///< Text displaying battery percentage
    private Image[] batteryPoints;     ///< Array of small indicators for remaining charge
    private Text DroneNameText;
    private Text DronePositionText;
    private Text DroneAltitudeText;
    private Text TimeText;
    private Text DroneNextPositionText;
    private Text DroneAreasText;
    private Battery batteryComponent;
    private SimulationClock simClock;
    private Transform map;
    private Transform miniMap;
    private Transform myDrone;

    private HashSet<Vector3> previouslyDrawnAreas = new HashSet<Vector3>();
    DroneController controller;

    void Start()
    {
        controller = GetComponent<DroneController>();
        if (controller == null)
            Debug.LogWarning("[CameraDisplay] DroneController not found.");

        batteryComponent = GetComponent<Battery>();
        if (batteryComponent == null)
            Debug.LogWarning("[CameraDisplay] Battery component not found.");

        simClock = FindObjectOfType<SimulationClock>();
        if (simClock == null)
            Debug.LogError("[CameraDisplay] No SimulationClock found in the scene!");

        // Find Canvas
        Transform canvas = transform.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("[CameraDisplay] No Canvas child found.");
            return;
        }

        // Link UI elements under the Canvas
        Transform batteryTextTf = canvas.Find("BatteryText");
        if (batteryTextTf == null) Debug.LogError("[CameraDisplay] 'BatteryText' not found under Canvas.");
        batteryText = batteryTextTf?.GetComponent<Text>();

        Transform droneNameTf = canvas.Find("DroneName");
        if (droneNameTf == null) Debug.LogError("[CameraDisplay] 'DroneNameText' not found under Canvas.");
        DroneNameText = droneNameTf?.GetComponent<Text>();
        DroneNameText.text = $"Name : {this.name}";

        Transform positionTf = canvas.Find("DronePosition");
        if (positionTf == null) Debug.LogError("[CameraDisplay] 'DronePositionText' not found under Canvas.");
        DronePositionText = positionTf?.GetComponent<Text>();

        Transform altitudeTf = canvas.Find("DroneAltitude");
        if (altitudeTf == null) Debug.LogError("[CameraDisplay] 'DroneAltitudeText' not found under Canvas.");
        DroneAltitudeText = altitudeTf?.GetComponent<Text>();

        Transform timeTf = canvas.Find("Time");
        if (timeTf == null) Debug.LogError("[CameraDisplay] 'TimeText' not found under Canvas.");
        TimeText = timeTf?.GetComponent<Text>();

        Transform nextPosTf = canvas.Find("DroneNextPosition");
        if (nextPosTf == null) Debug.LogError("[CameraDisplay] 'DroneNextPositionText' not found under Canvas.");
        DroneNextPositionText = nextPosTf?.GetComponent<Text>();

        Transform areasTf = canvas.Find("AreaText");
        if (areasTf == null) Debug.LogError("[CameraDisplay] 'DroneAreasText' not found under Canvas.");
        DroneAreasText = areasTf?.GetComponent<Text>();

        // Get battery point indicators
        Transform pointsParent = canvas.Find("BatteryBar");
        if (pointsParent == null)
        {
            Debug.LogError("[CameraDisplay] 'BatteryBar' not found under Canvas.");
        }
        else
        {
            List<Image> pointImages = new List<Image>();
            foreach (Transform child in pointsParent)
            {
                if (child.name.StartsWith("Point"))
                {
                    Image img = child.GetComponent<Image>();
                    if (img != null)
                    {
                        pointImages.Add(img);
                    }
                }
            }
            batteryPoints = pointImages.ToArray();
        }

        // Highlight areas on the map
        miniMap = canvas.Find("Minimap");
        if (miniMap == null)
        {
            Debug.LogError("[CameraDisplay] 'Minimap' not found under Canvas.");
        }

        map = miniMap.Find("Map");
        if (map == null)
        {
            Debug.LogError("[CameraDisplay] 'Map' not found under Canvas.");
        }

        myDrone = miniMap.Find("MyDrone");
        if (myDrone == null)
        {
            Debug.LogError("[CameraDisplay] 'MyDrone' not found under Canvas.");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        batteryComponent.BatteryPointFiller(batteryPoints, batteryText);

        if (DronePositionText != null)
            DronePositionText.text = $"Position X:{Mathf.RoundToInt(this.transform.position.x)} Y:{Mathf.RoundToInt(this.transform.position.z)}";

        if (DroneNextPositionText != null)
            DroneNextPositionText.text = $"Next Pos  X:{Mathf.RoundToInt(this.transform.position.x)} Y:{Mathf.RoundToInt(this.transform.position.z)}";

        if (DroneAltitudeText != null)
            DroneAltitudeText.text = $"Altitude: {Mathf.RoundToInt(this.transform.position.y)} m";


        if (DroneAreasText != null)
        {
            string text = "";
            foreach (Vector3 area in controller.Areas)
            {
                text += $"({area.x},{area.z}) ";
            }
            DroneAreasText.text = $"Areas: {text}";
        }

        RectTransform droneIcon = myDrone.GetComponent<RectTransform>();
        if (droneIcon != null)
        {
            Vector3 newPos = droneIcon.localPosition;
            newPos.x = 100f - this.transform.position.z / 5f;
            newPos.y = -100 + this.transform.position.x / 5f;
            newPos.z = 0f; // UI space — keep Z at 0
            droneIcon.localPosition = newPos;
        }
        else
        {
            Debug.LogError("[CameraDisplay] 'MyDrone' does not have a RectTransform component.");
}

        if (TimeText != null)
            TimeText.text = $"Time: {simClock.GetFormattedTime()}";
            
        HashSet<Vector3> currentAreas = new HashSet<Vector3>(controller.Areas);

        // Skip drawing if areas haven't changed
        if (currentAreas.SetEquals(previouslyDrawnAreas)) {
            return; // No change, skip
        }

        previouslyDrawnAreas = currentAreas;

        foreach (Vector3 area in currentAreas) {
            int xIndex = Mathf.FloorToInt(area.x / 50f);
            int zIndex = Mathf.FloorToInt(area.z / 50f);

            Transform row = map.Find($"X{xIndex}");
            if (row == null) {
                Debug.LogWarning($"[CameraDisplay] X{xIndex} not found in Minimap.");
                continue;
            }

            Transform cell = row.Find($"Z{zIndex}");
            if (cell == null) {
                Debug.LogWarning($"[CameraDisplay] Z{zIndex} not found in X{xIndex}.");
                continue;
            }

            Image img = cell.GetComponent<Image>();
            if (img != null)
            {
                // img.color = new Color(1f, 1f, 0f, 0.4f); // Transparent yellow
                img.color = new Color32(0x9E, 0xB6, 0x84, 127); // #9EB684 new Color(0f, 1f, 0f, 0.4f); // RGBA: Green with 40% opacity
            }
        }
    }
}
