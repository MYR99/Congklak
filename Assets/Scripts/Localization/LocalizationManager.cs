using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    public TextAsset localizationJSON;

    // MAIN localization dictionary
    private Dictionary<string, Dictionary<string, string>> table =
        new Dictionary<string, Dictionary<string, string>>();

    // Editor tool needs this
    public static List<string> GetAllKeys()
    {
        if (Instance == null || Instance.table == null)
            return new List<string>();

        return Instance.table.Keys.ToList();
    }

    public enum Language
    {
        Indonesian,
        English
    }

    private Language currentLanguage;

    public delegate void OnLangChanged();
    public static event OnLangChanged LanguageChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadJSON();
            LoadSavedLanguage();
        }
        else Destroy(gameObject);
    }

    void LoadSavedLanguage()
    {
        if (PlayerPrefs.HasKey("LANG"))
            currentLanguage = (Language)PlayerPrefs.GetInt("LANG");
        else
            currentLanguage = Language.Indonesian;
    }

    void LoadJSON()
    {
        if (localizationJSON == null)
        {
            Debug.LogError("Localization JSON not assigned!");
            return;
        }

        var root = MiniJson.Deserialize(localizationJSON.text) as Dictionary<string, object>;
        if (root == null)
        {
            Debug.LogError("[Localization] Failed to parse JSON!");
            return;
        }

        if (!root.ContainsKey("entries"))
        {
            Debug.LogError("[Localization] JSON missing 'entries'");
            return;
        }

        var entriesObj = root["entries"] as Dictionary<string, object>;
        table = new Dictionary<string, Dictionary<string, string>>();

        foreach (var pair in entriesObj)
        {
            var rawValue = pair.Value as Dictionary<string, object>;
            if (rawValue == null) continue;

            table[pair.Key] = new Dictionary<string, string>()
            {
                { "Indonesian", rawValue["Indonesian"].ToString() },
                { "English", rawValue["English"].ToString() }
            };
        }

        Debug.Log("[Localization] Loaded " + table.Count + " entries.");
    }

    public static string Get(string key)
    {
        if (Instance == null || Instance.table == null)
            return key;

        if (!Instance.table.ContainsKey(key))
            return key;

        var entry = Instance.table[key];

        return Instance.currentLanguage switch
        {
            Language.Indonesian => entry["Indonesian"],
            Language.English => entry["English"],
            _ => key
        };
    }

    public static void SetLanguage(Language lang)
    {
        if (Instance == null) return;

        Instance.currentLanguage = lang;
        PlayerPrefs.SetInt("LANG", (int)lang);

        LanguageChanged?.Invoke();
    }

    public static Language GetLanguage() =>
        Instance == null ? Language.Indonesian : Instance.currentLanguage;
}