using System.Collections.Generic;
using UnityEngine;

public class OutlineDebugTester : MonoBehaviour
{
    [Header("Debug")]
    public bool enableOnStart = true;
    public bool keepForcingState = true;
    public bool targetEnabledState = true;
    public KeyCode toggleKey = KeyCode.O;

    private Outline[] outlines = new Outline[0];

    private void Awake()
    {
        CacheOutlines();
    }

    private void Start()
    {
        if (enableOnStart)
        {
            SetOutlinesEnabled(targetEnabledState);
        }
    }

    private void Update()
    {
        if (keepForcingState && outlines.Length > 0)
        {
            SetOutlinesEnabled(targetEnabledState);
        }

        if (Input.GetKeyDown(toggleKey))
        {
            targetEnabledState = !targetEnabledState;
            SetOutlinesEnabled(targetEnabledState);
            Debug.Log($"[OutlineDebugTester] {gameObject.name} -> outline {(targetEnabledState ? "ON" : "OFF")}, count={outlines.Length}");
        }
    }

    [ContextMenu("Refresh Outlines")]
    public void CacheOutlines()
    {
        List<Outline> found = new List<Outline>();

        Outline self = GetComponent<Outline>();
        if (self != null) found.Add(self);

        Outline parent = GetComponentInParent<Outline>(true);
        if (parent != null && !found.Contains(parent)) found.Add(parent);

        Outline[] children = GetComponentsInChildren<Outline>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Outline o = children[i];
            if (o != null && !found.Contains(o))
            {
                found.Add(o);
            }
        }

        outlines = found.ToArray();
        Debug.Log($"[OutlineDebugTester] Found {outlines.Length} outline(s) on {gameObject.name}");
    }

    [ContextMenu("Force ON")]
    public void ForceOn()
    {
        targetEnabledState = true;
        SetOutlinesEnabled(true);
    }

    [ContextMenu("Force OFF")]
    public void ForceOff()
    {
        targetEnabledState = false;
        SetOutlinesEnabled(false);
    }

    private void SetOutlinesEnabled(bool enabled)
    {
        for (int i = 0; i < outlines.Length; i++)
        {
            if (outlines[i] != null)
            {
                outlines[i].enabled = enabled;
            }
        }
    }
}
