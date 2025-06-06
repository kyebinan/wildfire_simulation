using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabsManager : MonoBehaviour
{
    private GameObject[] tabs;
    private Button[] tabButtons;

    private Color inactiveColor = new Color32(0x2D, 0x2D, 0x30, 0xFF); // #2D2D30 #e3fec6
    private Color activeColor = new Color32(0x9E, 0xB6, 0x84, 0xFF); // rgb(158, 182, 132)
    void Awake()
    {
        Transform menu = transform.Find("Menu");
        Transform tabsContainer = transform.Find("Tabs");

        if (menu == null || tabsContainer == null)
        {
            Debug.LogError("[TabsManager] Menu or Tabs container not found in hierarchy.");
            enabled = false;
            return;
        }

        tabButtons = menu.GetComponentsInChildren<Button>();
        tabs = new GameObject[tabsContainer.childCount];
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i] = tabsContainer.GetChild(i).gameObject;
        }

        if (tabButtons.Length != tabs.Length)
        {
            Debug.LogError("[TabsManager] Number of buttons does not match number of tabs.");
            enabled = false;
            return;
        }

        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => SwitchToTab(index));
        }

        SwitchToTab(0); // Initialize first tab
    }

    public void SwitchToTab(int tabID)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = (i == tabID);
            tabs[i].SetActive(isActive);

            Image btnImage = tabButtons[i].GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = isActive ? activeColor : inactiveColor;
            }
        }
    }
}
