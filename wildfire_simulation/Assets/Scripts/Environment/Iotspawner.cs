using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IoTspawner : MonoBehaviour {

    [Header("IoT Prefab and Parent")]
    public GameObject iotPrefab;           // Assign IoT prefab in the inspector
    private Transform fireStation;          // Assign or find the FireStation manually or at Start

    private Transform parentIotGroup;      // The parent GameObject for all IoTs

    private float spacing = 50f;
    private float maxX = 975f;
    private float maxZ = 975f;
    private float fireStationAvoidanceRadius = 50f;
    private float maxAllowedAltitude = 15f;

    void Start() {
        parentIotGroup = GameObject.Find("IotSensors")?.transform;
        if (parentIotGroup == null) {
            parentIotGroup = new GameObject("IotSensors").transform;
        }

        if (fireStation == null) {
            fireStation = GameObject.Find("FireStation")?.transform;
            if (fireStation == null) {
                Debug.LogError("FireStation not assigned or found in scene.");
                return;
            }
        }

        for (float x = spacing; x <= maxX; x += spacing) {
            for (float z = spacing; z <= maxZ; z += spacing) {
                Vector3 pos = new Vector3(x, 100f, z);
                if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 200f)) {
                    float altitude = hit.point.y;
                    Vector3 spawnPos = new Vector3(x, altitude, z);

                    if (altitude <= maxAllowedAltitude && !IsNearFireStation(spawnPos)) {
                        GameObject iot = Instantiate(iotPrefab, spawnPos, Quaternion.identity, parentIotGroup);
                        iot.name = $"IoT_{(int)x}_{(int)z}";
                    }
                }
            }
        }
    }

    bool IsNearFireStation(Vector3 pos) {
        return Vector3.Distance(pos, fireStation.position) < fireStationAvoidanceRadius;
    }
}