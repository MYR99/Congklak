#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.IO;

public class LocalizationEditorTool : EditorWindow
{
    [MenuItem("Tools/Localization/Auto-Assign TMP Keys")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationEditorTool>("Localization Auto-Assign");
    }

    void OnGUI()
    {
        GUILayout.Label("Auto-Assign TMP keys from JSON", EditorStyles.boldLabel);

        if (GUILayout.Button("Assign Keys In Active Scene"))
        {
            AssignKeysInScene();
        }
    }

    // Load JSON manually from Resources
    static List<string> LoadJsonKeys()
    {
        TextAsset json = Resources.Load<TextAsset>("Localization/localization");

        if (json == null)
        {
            Debug.LogError("[LocalizationEditorTool] Cannot find Resources/localization.json");
            return new List<string>();
        }

        var root = MiniJson.Deserialize(json.text) as Dictionary<string, object>;
        if (root == null || !root.ContainsKey("entries"))
        {
            Debug.LogError("[LocalizationEditorTool] JSON missing 'entries'");
            return new List<string>();
        }

        var entriesObj = root["entries"] as Dictionary<string, object>;
        return entriesObj.Keys.ToList();
    }

    static void AssignKeysInScene()
    {
        List<string> jsonKeys = LoadJsonKeys();

        int changed = 0;
        var allTexts = Resources.FindObjectsOfTypeAll<TMP_Text>()
            .Where(t => t.gameObject.scene.isLoaded)
            .ToArray();

        foreach (var text in allTexts)
        {
            if (EditorUtility.IsPersistent(text.gameObject))
                continue;

            var comp = text.GetComponent<TMP_TextAutoTranslate>();
            if (comp == null)
                comp = Undo.AddComponent<TMP_TextAutoTranslate>(text.gameObject);

            string nameKey = text.gameObject.name.Trim();
            string textKey = text.text.Trim();

            string keyToAssign = null;

            if (jsonKeys.Contains(nameKey))
                keyToAssign = nameKey;
            else if (jsonKeys.Contains(textKey))
                keyToAssign = textKey;
            else
                keyToAssign = nameKey.ToUpper().Replace(" ", "_");

            comp.SetKey(keyToAssign);

            EditorUtility.SetDirty(comp);
            changed++;
        }

        Debug.Log($"[LocalizationEditorTool] Key assigned to {changed} TMP objects.");
    }
}
#endif