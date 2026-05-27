using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Çanta / Avladıklarım gibi yan panelleri küçük başlık butonu ile açıp kapatır.
/// </summary>
public class CollapsiblePlayerPanel : MonoBehaviour
{
    [SerializeField] RectTransform panelRoot;
    [SerializeField] Button toggleButton;
    [SerializeField] Text toggleLabel;
    [SerializeField] GameObject contentRoot;

    Vector2 expandedAnchorMin;
    Vector2 expandedAnchorMax;
    bool alignLeft = true;
    bool isExpanded;
    string panelTitle = "Panel";

    public bool IsExpanded => isExpanded;

    public event Action<bool> ExpandedChanged;

    string persistenceKey;

    public void Configure(
        RectTransform root,
        Button toggle,
        Text toggleText,
        GameObject content,
        Vector2 anchorMin,
        Vector2 anchorMax,
        string title,
        bool leftAligned,
        bool startExpanded = false,
        string panelPersistenceKey = null)
    {
        panelRoot = root;
        toggleButton = toggle;
        toggleLabel = toggleText;
        contentRoot = content;
        expandedAnchorMin = anchorMin;
        expandedAnchorMax = anchorMax;
        panelTitle = title;
        alignLeft = leftAligned;
        persistenceKey = panelPersistenceKey;

        toggleButton.onClick.RemoveListener(OnToggleClicked);
        toggleButton.onClick.AddListener(OnToggleClicked);

        if (!string.IsNullOrEmpty(persistenceKey)
            && CollapsiblePanelPersistence.TryGetExpanded(persistenceKey, out var saved))
        {
            SetExpanded(saved);
            return;
        }

        SetExpanded(startExpanded);
    }

    void OnToggleClicked() => SetExpanded(!isExpanded);

    public void SetExpanded(bool expanded)
    {
        isExpanded = expanded;

        if (contentRoot != null)
            contentRoot.SetActive(expanded);

        ApplyLayout();

        if (toggleLabel != null)
            toggleLabel.text = panelTitle;

        if (!string.IsNullOrEmpty(persistenceKey))
            CollapsiblePanelPersistence.SetExpanded(persistenceKey, isExpanded);

        ExpandedChanged?.Invoke(isExpanded);
    }

    void ApplyLayout()
    {
        if (panelRoot == null)
            return;

        if (isExpanded)
        {
            panelRoot.anchorMin = expandedAnchorMin;
            panelRoot.anchorMax = expandedAnchorMax;
            panelRoot.pivot = new Vector2(0.5f, 0.5f);
            panelRoot.offsetMin = Vector2.zero;
            panelRoot.offsetMax = Vector2.zero;
            return;
        }

        if (alignLeft)
        {
            panelRoot.anchorMin = new Vector2(0.02f, 0.82f);
            panelRoot.anchorMax = new Vector2(0.02f, 0.82f);
            panelRoot.pivot = new Vector2(0f, 1f);
        }
        else
        {
            panelRoot.anchorMin = new Vector2(0.98f, 0.82f);
            panelRoot.anchorMax = new Vector2(0.98f, 0.82f);
            panelRoot.pivot = new Vector2(1f, 1f);
        }

        panelRoot.anchoredPosition = Vector2.zero;
        panelRoot.sizeDelta = new Vector2(132f, 40f);
    }
}
