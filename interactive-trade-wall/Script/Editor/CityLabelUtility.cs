using UnityEngine;
using UnityEditor;
using TMPro;

public class CityLabelUtility : EditorWindow
{
    float alphaValue = 1f;
    bool makeBold = false;

    [MenuItem("Tools/City Label Utility")]
    public static void ShowWindow()
    {
        GetWindow<CityLabelUtility>("City Label Utility");
    }

    void OnGUI()
    {
        GUILayout.Label("Mass Edit City Labels", EditorStyles.boldLabel);

        // -------- Alpha Section --------
        GUILayout.Space(10);
        GUILayout.Label("Label Alpha", EditorStyles.boldLabel);

        alphaValue = EditorGUILayout.Slider("Alpha Value", alphaValue, 0f, 1f);

        if (GUILayout.Button("Apply Alpha to All City Labels"))
        {
            ApplyAlphaToAll(alphaValue);
        }

        // -------- Bold Section --------
        GUILayout.Space(20);
        GUILayout.Label("Font Weight", EditorStyles.boldLabel);

        makeBold = EditorGUILayout.Toggle("Bold", makeBold);

        if (GUILayout.Button("Apply Bold / Normal"))
        {
            ApplyBoldToAll(makeBold);
        }
    }

    // -------------------- APPLY ALPHA --------------------
    void ApplyAlphaToAll(float a)
    {
        var allLabels = GameObject.FindObjectsOfType<CityLabelEffect>(true);

        Undo.RecordObjects(allLabels, "Apply City Label Alpha");

        foreach (var label in allLabels)
        {
            if (label.labelText != null)
            {
                var c = label.labelText.color;
                c.a = a;
                label.labelText.color = c;
                EditorUtility.SetDirty(label.labelText);
            }
        }

        Debug.Log($"[CityLabelUtility] Updated Alpha on {allLabels.Length} labels.");
    }

    // -------------------- APPLY BOLD --------------------
    void ApplyBoldToAll(bool bold)
    {
        var allLabels = GameObject.FindObjectsOfType<CityLabelEffect>(true);

        Undo.RecordObjects(allLabels, "Apply City Label Font Weight");

        foreach (var label in allLabels)
        {
            if (label.labelText != null)
            {
                if (bold)
                    label.labelText.fontStyle |= FontStyles.Bold;
                else
                    label.labelText.fontStyle &= ~FontStyles.Bold;

                EditorUtility.SetDirty(label.labelText);
            }
        }

        Debug.Log($"[CityLabelUtility] Updated FontWeight on {allLabels.Length} labels.");
    }
}