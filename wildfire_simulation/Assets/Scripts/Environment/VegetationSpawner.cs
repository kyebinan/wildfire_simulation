using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegetationSpawner : MonoBehaviour {
    public List<GameObject> bushesPrefabs;
    public List<GameObject> smallTreesPrefabs;
    public List<GameObject> bigTreesPrefabs;
    
    private Transform fireStation;
    private float spawnRadius = 1000f;
    private Transform floreParent;
    public int bigTreesCount = 10000;
    public int smallTreesCount = 5000;
    public int bushesCount = 10000;

    private List<Vector3> iotPositions = new List<Vector3>(); // Store IoT positions

    void Start() {
        fireStation = transform.Find("FireStation");
        floreParent = new GameObject("Flore").transform;
        floreParent.SetParent(transform);

        // Find all IoT sensors in the scene
        GameObject iotParent = GameObject.Find("IotSensors");
        if (iotParent != null) {
            foreach (Transform child in iotParent.transform) {
                iotPositions.Add(child.position);
            }
        } else {
            Debug.LogWarning("IotSensors parent not found.");
        }

        if (fireStation != null) {
            Debug.Log("FireStation found in the environment!");

            for (int i = 0; i < bigTreesCount; i++)
                SpawnVegetation(bigTreesPrefabs);

            for (int i = 0; i < smallTreesCount; i++)
                SpawnVegetation(smallTreesPrefabs);

            for (int i = 0; i < bushesCount; i++)
                SpawnVegetation(bushesPrefabs);
        } else {
            Debug.LogWarning("FireStation not found in the environment!");
        }
    }

    void SpawnVegetation(List<GameObject> prefabList) {
        if (prefabList.Count > 0) {
            int randomIndex = Random.Range(0, prefabList.Count);
            GameObject selectedPrefab = prefabList[randomIndex];

            Vector3 spawnPosition = FindValidSpawnPosition();
            if (spawnPosition != Vector3.zero) {
                GameObject spawnedVegetation = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
                spawnedVegetation.transform.SetParent(floreParent);
            }
        } else {
            Debug.LogWarning("Prefab list is empty.");
        }
    }

    Vector3 FindValidSpawnPosition() {
        for (int i = 0; i < 100; i++) {
            Vector3 randomPos = new Vector3(Random.Range(0f, spawnRadius), 0f, Random.Range(0f, spawnRadius));
            randomPos = IsPositionOnTerrain(randomPos);

            if (randomPos == Vector3.zero)
                continue;

            if (!IsPositionNearFireStation(randomPos) && !IsNearAnyIoT(randomPos)) {
                return randomPos;
            }
        }

        Debug.LogWarning("No valid position found for spawning.");
        return Vector3.zero;
    }

    Vector3 IsPositionOnTerrain(Vector3 position) {
        Vector3 desiredPosition = new Vector3(position.x, 0f, position.z);
        RaycastHit hit;
        Ray ray = new Ray(desiredPosition + Vector3.up * 100f, Vector3.down);
        if (Physics.Raycast(ray, out hit)) {
            desiredPosition.y = hit.point.y;
            return desiredPosition;
        }
        return Vector3.zero;
    }

    bool IsPositionNearFireStation(Vector3 position) {
        return Vector3.Distance(position, fireStation.position) < 50f;
    }

    bool IsNearAnyIoT(Vector3 position) {
        foreach (Vector3 iotPos in iotPositions) {
            if (Vector3.Distance(position, iotPos) < 1f) {
                return true;
            }
        }
        return false;
    }
}
