using UnityEngine;
using UnityEngine.UI;

public class MessageDetailManager : MonoBehaviour {
    public static MessageDetailManager Instance;

    [Header("Prefabs and UI")]
    public GameObject blockPrefab;                // Assign your Block prefab
    public Transform detailContentParent;         // Drag the "Details/Viewport/Content" transform here

    void Awake()
    {
        Instance = this;
        // Clear previous blocks
        foreach (Transform child in detailContentParent) {
            Destroy(child.gameObject);
        }
    }

    public void Display(Message msg) {
        if (msg == null || blockPrefab == null || detailContentParent == null) return;

        // Clear previous blocks
        foreach (Transform child in detailContentParent) {
            Destroy(child.gameObject);
        }

        // Instantiate and populate a new block
        GameObject blockGO = Instantiate(blockPrefab, detailContentParent);

        Transform head = blockGO.transform.Find("Head");
        if (head != null) {
            SetTextIfExists(head, "Title", $"Type: {msg.Source}");
        }

        // SetTextIfExists(blockGO.transform, "Source", $"SRC: {msg.Source}");
        // SetTextIfExists(blockGO.transform, "Destination", $"DST: {msg.Destination}");
        // SetTextIfExists(blockGO.transform, "Protocol", "MAKI");
        // SetTextIfExists(blockGO.transform, "Timestamp", $"Time: {msg.TimeStamp:HH:mm:ss}");
        // SetTextIfExists(blockGO.transform, "Payload", $"Data: {msg.Data}");
    }

    private void SetTextIfExists(Transform parent, string childName, string value) {
        var tf = parent.Find(childName);
        if (tf != null) {
            var text = tf.GetComponent<Text>();
            if (text != null) {
                text.text = value;
            }
        }
    }
}
