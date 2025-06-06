using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Responsible for spawning and organizing drones in the scene.
/// This script does not manage camera or display logic.
/// </summary>
public class DroneManager : MonoBehaviour
{
    [Header("Drone prefab to spawn")]
    public GameObject dronePrefab;

    [Header("How many drones to spawn (5‑25)")]
    [Range(5, 25)] public int dronesToSpawn = 10;
    private Transform fireStation;
    private Transform firefighterTeam;
    private readonly List<GameObject> _drones = new List<GameObject>(25);
    public IReadOnlyList<GameObject> Drones => _drones;

    void Start()
    {
        if (dronePrefab == null)
        {
            Debug.LogError("[DroneManager] Drone prefab not assigned");
            return;
        }

        GameObject environment = GameObject.Find("Environment");
        if (environment == null)
        {
            Debug.LogError("[DroneManager] Environment object not found in scene.");
            return;
        }

        fireStation = environment.transform.Find("FireStation");
        if (fireStation == null)
        {
            Debug.LogError("[DroneManager] FireStation not found as child of Environment.");
            return;
        }

        GameObject firefighterTeamGO = GameObject.Find("FirefighterTeam");
        if (firefighterTeamGO == null)
        {
            Debug.LogError("[DroneManager] FirefighterTeam object not found.");
            return;
        }
        firefighterTeam = firefighterTeamGO.transform;

        int spawned = 0;
        for (int i = 1; i <= 25 && spawned < dronesToSpawn; i++)
        {
            string relativePath = $"LandingPoints/DroneLandingBase.{i:00}";
            Transform pad = fireStation.Find(relativePath);
            if (pad == null)
            {
                Debug.LogWarning($"[DroneManager] Landing pad '{relativePath}' not found – skipping");
                continue;
            }

            GameObject drone = Instantiate(dronePrefab, pad.position, pad.rotation, firefighterTeam);
            drone.name = $"drone_{i:00}";
            _drones.Add(drone);
            spawned++;
        }

        Debug.Log($"[DroneManager] Spawned {spawned} / {dronesToSpawn} drones.");

        AssignAreaToDrones();
    }

    /// <summary>
    /// Divides the 10x10 hectare grid evenly and assigns contiguous zones to each drone.
    /// </summary>
    private void AssignAreaToDrones()
    {
        int gridSize = 20;
        int blockSize = 4; // Each drone gets a 4x4 block (total 16 patrol points)
        int dronesPerRow = gridSize / blockSize; // 5 drones per row

        int droneIndex = 0;

        foreach (GameObject drone in _drones) {
            if (droneIndex >= 25)
                break; // Support only 25 drones

            DroneController controller = drone.GetComponent<DroneController>();
            if (controller == null) continue;

            controller.Areas = new List<Vector3>();
            controller.Altitude = 100f + 2*droneIndex;

            int row = droneIndex / dronesPerRow; // Which 4-row block
            int col = droneIndex % dronesPerRow; // Which 4-column block

            int startXi = col * blockSize;
            int endXi = Mathf.Min(startXi + blockSize, gridSize);

            int startZi = row * blockSize;
            int endZi = Mathf.Min(startZi + blockSize, gridSize);

            for (int xi = startXi; xi < endXi; xi++) {
                for (int zi = startZi; zi < endZi; zi++) {
                    float x = 25f + xi * 50f;
                    float z = 25f + zi * 50f;
                    Vector3 position = new Vector3(x, 0f, z);
                    controller.Areas.Add(position);
                }
            }

            Debug.Log($"[DroneManager] Assigned {controller.Areas.Count} patrol points to {drone.name}");
            droneIndex++;
        }
    }
}
