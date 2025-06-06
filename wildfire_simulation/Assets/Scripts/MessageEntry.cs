using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MessageEntry : MonoBehaviour, IPointerClickHandler {
    // UI elements
    private Text numeroText, timeText, sourceText, destinationText, protocolText, lengthText, infoText;
    private Image background;

    // Reference
    private Message currentMessage;
    private bool isSent;
    private static MessageEntry currentSelected;

    private readonly Color sentColor = new Color32(0x11, 0x27, 0x2D, 255);     // Default for sent
    private readonly Color receivedColor = new Color32(0x31, 0x4F, 0x78, 255); // Default for received
    private readonly Color selectedColor = new Color32(0x9E, 0xB6, 0x84, 255); // Highlight on click

    void Awake() {
        background = GetComponent<Image>();
        numeroText = transform.Find("Numero")?.GetComponent<Text>();
        timeText = transform.Find("Time")?.GetComponent<Text>();
        sourceText = transform.Find("Source")?.GetComponent<Text>();
        destinationText = transform.Find("Destination")?.GetComponent<Text>();
        protocolText = transform.Find("Protocol")?.GetComponent<Text>();
        lengthText = transform.Find("Length")?.GetComponent<Text>();
        infoText = transform.Find("Info")?.GetComponent<Text>();
    }

    public void Setup(Message msg, bool sent) {
        currentMessage = msg;
        isSent = sent;

        if (numeroText) numeroText.text = "1";
        if (timeText) timeText.text = $"{msg.TimeStamp:HH:mm:ss}";
        if (sourceText) sourceText.text = msg.Source;
        if (destinationText) destinationText.text = msg.Destination;
        if (protocolText) protocolText.text = "MAKI";
        if (lengthText) lengthText.text = $"{msg.Data?.Length ?? 0}";
        if (infoText) infoText.text = $"[{msg.Type}]";

        UpdateBackgroundColor();
    }

    private void UpdateBackgroundColor() {
        background.color = isSent ? sentColor : receivedColor;
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (currentSelected != null && currentSelected != this)
            currentSelected.Deselect();

        currentSelected = this;
        Select();

        // Tell manager to update detail panel
        MessageDetailManager.Instance.Display(currentMessage);
    }

    public void Select() {
        background.color = selectedColor;
    }

    public void Deselect() {
        UpdateBackgroundColor();
    }
}
