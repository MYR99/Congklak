#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System;

public class LocalizationCSVExporter : EditorWindow
{
    private string defaultPath = "Assets/localization_export.csv";

    [MenuItem("Tools/Localization/Export to CSV")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationCSVExporter>("Export Localization CSV");
    }

    private void OnGUI()
    {
        GUILayout.Label("Export localization.json → CSV", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        defaultPath = EditorGUILayout.TextField("CSV Output Path", defaultPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Export Now", GUILayout.Height(30)))
        {
            ExportCSV(defaultPath);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "CSV akan berisi:\nKEY, Indonesian, English\n\nSiap untuk dipakai di Google Sheets atau Excel.",
            MessageType.Info);
    }

    private void ExportCSV(string path)
    {
        // Load JSON
        TextAsset ta = Resources.Load<TextAsset>("Localization/localization");
        if (ta == null)
        {
            EditorUtility.DisplayDialog("Error", "Tidak menemukan localization.json di Resources/Localization/", "OK");
            return;
        }

        JsonLocalization json = null;

        try
        {
            json = JsonUtility.FromJson<JsonLocalization>(ta.text);
        }
        catch (Exception e)
        {
            Debug.LogError("Gagal parse JSON: " + e.Message);
            return;
        }

        if (json == null || json.entries == null)
        {
            EditorUtility.DisplayDialog("Error", "Format localization.json tidak valid.", "OK");
            return;
        }

        // Languages
        List<string> langs = json.languages != null ? json.languages : new List<string>();
        if (langs.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "Field 'languages' kosong dalam JSON.", "OK");
            return;
        }

        // CSV builder
        StringBuilder sb = new StringBuilder();

        // Header
        sb.Append("KEY");
        foreach (var lang in langs)
        {
            sb.Append($",{lang}");
        }
        sb.AppendLine();

        // Rows
        foreach (var entry in json.entries)
        {
            string key = entry.Key;
            sb.Append(key);

            foreach (string lang in langs)
            {
                entry.Value.TryGetValue(lang, out string translated);
                translated = translated?.Replace("\"", "\"\""); // escape quotes
                sb.Append($",\"{translated}\"");
            }

            sb.AppendLine();
        }

        // Save file
        try
        {
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Export selesai!\nLokasi:\n{path}", "OK");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Gagal menyimpan CSV.\n{e.Message}", "OK");
        }
    }

    [Serializable]
    private class JsonLocalization
    {
        public List<string> languages;
        public Dictionary<string, Dictionary<string, string>> entries;
    }
}
#endif