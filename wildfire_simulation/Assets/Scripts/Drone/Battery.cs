using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the drone's battery life and displays it using segmented UI indicators.
/// External systems can drain, reset, or query battery level via public API.
/// </summary>
public class Battery : MonoBehaviour
{
    private float batteryLife;        ///< Current battery level (0–max)
    private float maxBatteryLife = 100f; ///< Maximum battery capacity

    /// <summary>
    /// Initializes battery to full charge.
    /// </summary>
    private void Start()
    {
        batteryLife = maxBatteryLife;
    }

    /// <summary>
    /// Regular update loop – updates text and point indicators.
    /// External modules should use API for drain simulation.
    /// </summary>
    private void Update()
    {
    }

    /// <summary>
    /// Determines whether a battery point should be hidden.
    /// </summary>
    /// <param name="life">Battery level</param>
    /// <param name="point">Index of the indicator</param>
    /// <returns>True if point should be hidden</returns>
    bool DisplayHealthPoint(float life, int point)
    {
        return (point * 10 >= life);
    }

    // ──────── Public API ────────

    /// <summary>
    /// Reduces battery life by a specified amount.
    /// Use this to simulate external power drain.
    /// </summary>
    /// <param name="amount">Amount to drain (0–100)</param>
    public void DrainBattery(float amount)
    {
        batteryLife -= amount;
        batteryLife = Mathf.Clamp(batteryLife, 0f, maxBatteryLife);
    }

    /// <summary>
    /// Sets battery life directly (e.g., after recharge or debugging).
    /// </summary>
    /// <param name="value">Battery value to set (0–100)</param>
    public void SetBatteryLevel(float value)
    {
        batteryLife = Mathf.Clamp(value, 0f, maxBatteryLife);
    }

    /// <summary>
    /// Returns current battery life.
    /// </summary>
    /// <returns>Battery percentage (0–100)</returns>
    public float GetBatteryLevel()
    {
        return batteryLife;
    }

    /// <summary>
    /// Refills the battery to maximum capacity.
    /// Useful for landing recharge logic.
    /// </summary>
    public void RechargeFull()
    {
        batteryLife = maxBatteryLife;
    }

    /// <summary>
    /// Updates visibility of battery segment indicators.
    /// </summary>
    public void BatteryPointFiller(Image[] batteryPoints, Text batteryText)
    {
        for (int i = 0; i < batteryPoints.Length; i++)
        {
            if (batteryPoints[i] != null)
                batteryPoints[i].enabled = !DisplayHealthPoint(batteryLife, i);

            if (batteryText != null)
                batteryText.text = $"{Mathf.RoundToInt(batteryLife)}%";
        }
    }
}
