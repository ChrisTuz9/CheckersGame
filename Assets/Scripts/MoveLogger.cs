using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveLogger : MonoBehaviour
{
    public GameObject moveLogContent;
    public GameObject moveLogEntryPrefab;

    public void SetMoveLogger(GameObject moveLogContent, GameObject moveLogEntryPrefab)
    {
        this.moveLogContent = moveLogContent;
        this.moveLogEntryPrefab = moveLogEntryPrefab;
    }

    public void AddMoveToLog(string moveDescription)
    {
        GameObject newEntry = Instantiate(moveLogEntryPrefab, moveLogContent.transform);

        TextMeshProUGUI entryText = newEntry.GetComponent<TextMeshProUGUI>();
        entryText.text = moveDescription;

        Canvas.ForceUpdateCanvases();
        ScrollRect scrollRect = moveLogContent.GetComponentInParent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClearMoveLog()
    {
        foreach (Transform child in moveLogContent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}