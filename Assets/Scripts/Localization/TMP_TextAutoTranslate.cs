using TMPro;
using UnityEngine;

public class TMP_TextAutoTranslate : MonoBehaviour
{
    public string key;
    private TMP_Text textElement;

    private void Awake()
    {
        textElement = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        LocalizationManager.LanguageChanged += Refresh;
        Refresh(); 
    }

    private void OnDisable()
    {
        LocalizationManager.LanguageChanged -= Refresh;
    }

    public void Refresh()
    {
        if (textElement == null || string.IsNullOrEmpty(key))
            return;

        textElement.text = LocalizationManager.Get(key);
    }

    public void SetKey(string newKey)
    {
        key = newKey;
        Refresh(); 
    }
}