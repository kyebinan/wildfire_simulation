using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationClock : MonoBehaviour {
    public float simulationSpeed = 12f; // 1 real second = 12 simulated seconds

    public float SimulatedTime { get; private set; } // in seconds

    void Update() {
        SimulatedTime += Time.deltaTime * simulationSpeed;
    }

    public string GetFormattedTime() {
        int totalSeconds = Mathf.FloorToInt(SimulatedTime);
        int hours = (totalSeconds / 3600) % 24;
        int minutes = (totalSeconds / 60) % 60;
        int seconds = totalSeconds % 60;

        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
}

