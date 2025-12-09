using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum HoleType
{
    PlayerOne,
    PlayerTwo,
    Storehouse
}

public class Hole : MonoBehaviour
{
    [Header("Hole Settings")]
    [SerializeField] private HoleType holeType;
    [SerializeField] private int holeIndex;

    [Header("References")]
    [SerializeField] private Transform seedContainer;
    public TMP_Text seedText;
    
    [Tooltip("Masukkan Prefab/Objek Highlight (Kotak Kuning) di sini")]
    public GameObject turnIndicatorHighlight; 

    private List<GameObject> seeds = new List<GameObject>();
    private UIController uiController; 

    private void Awake()
    {
        if (seedContainer == null)
        {
            GameObject container = new GameObject("SeedContainer");
            container.transform.SetParent(this.transform);
            container.transform.localPosition = Vector3.zero;
            seedContainer = container.transform;
        }
        
        // Pastikan highlight mati saat game baru dimulai
        if(turnIndicatorHighlight != null) turnIndicatorHighlight.SetActive(false);
    }

    // --- LOGIKA MOUSE HOVER (HIGHLIGHT) ---
    private void OnMouseEnter()
    {
        // Cek apakah kita boleh memilih lubang ini?
        if (GameManager.Instance != null && GameManager.Instance.IsValidMove(this))
        {
            // Jika valid, nyalakan highlight
            if (turnIndicatorHighlight != null) turnIndicatorHighlight.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        // Saat mouse pergi, matikan highlight
        if (turnIndicatorHighlight != null) turnIndicatorHighlight.SetActive(false);
    }
    // --------------------------------------

    public void Initialize(int startingSeedCount, UIController controller)
    {
        this.uiController = controller;
        ClearSeeds();

        if (holeType != HoleType.Storehouse)
        {
            for (int i = 0; i < startingSeedCount; i++)
            {
                GameObject seed = SeedPool.Instance.GetSeed();
                seed.transform.SetParent(seedContainer);
                seed.transform.localPosition = GetRandomSeedPosition();
                seed.transform.localScale = Vector3.one * SeedPool.Instance.GetSeedScale(); // Fix scale
                seeds.Add(seed);
            }
        }
        UpdateSeedText();
    }

    private void OnMouseDown()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        if (GameManager.Instance != null)
        {
            // Matikan highlight setelah diklik agar tidak nyangkut
            if (turnIndicatorHighlight != null) turnIndicatorHighlight.SetActive(false);
            
            GameManager.Instance.SelectHole(this);
        }
    }

    public Transform GetSeedContainer()
    {
        return seedContainer;
    }

    public HoleType GetHoleType()
    {
        return holeType;
    }

    public int GetHoleIndex()
    {
        return holeIndex;
    }

    public IEnumerator AnimateAddSeedCoroutine(GameObject seed, Transform fromTransform, float liftHeight, float moveSpeed)
    {
        seed.transform.SetParent(seedContainer);
        seed.transform.localScale = Vector3.one * SeedPool.Instance.GetSeedScale(); // Fix scale

        Vector3 startPos = fromTransform.position;
        Vector3 liftPos = startPos + Vector3.up * liftHeight;
        Vector3 finalLocalPos = GetRandomSeedPosition();
        Vector3 finalWorldPos = seedContainer.TransformPoint(finalLocalPos);

        float journey = 0f;
        float duration = Vector3.Distance(startPos, liftPos) / (moveSpeed * 2); 
        if (duration <= 0) duration = 0.01f;
        while (journey < duration)
        {
            journey += Time.deltaTime;
            seed.transform.position = Vector3.Lerp(startPos, liftPos, journey / duration);
            yield return null;
        }
        seed.transform.position = liftPos;

        while (Vector3.Distance(seed.transform.position, finalWorldPos) > 0.01f)
        {
            seed.transform.position = Vector3.MoveTowards(seed.transform.position, finalWorldPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        seed.transform.localPosition = finalLocalPos;
        seeds.Add(seed);
        UpdateSeedText();
    }

    public List<GameObject> TakeAllSeedObjects()
    {
        List<GameObject> taken = new List<GameObject>(seeds);
        seeds.Clear();
        UpdateSeedText();
        return taken;
    }

    public int GetSeedCount() => seeds.Count;

    public int ClearSeeds()
    {
        int count = seeds.Count;
        foreach (var seed in seeds)
        {
            if (seed != null) SeedPool.Instance.ReturnSeed(seed);
        }
        seeds.Clear();
        UpdateSeedText();
        return count;
    }
    
    public void AddSeeds(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject seed = SeedPool.Instance.GetSeed();
            seed.transform.SetParent(seedContainer);
            seed.transform.localPosition = GetRandomSeedPosition();
            seed.transform.localScale = Vector3.one * SeedPool.Instance.GetSeedScale(); // Fix scale
            seeds.Add(seed);
        }
        UpdateSeedText();
    }

    public Vector3 GetRandomSeedPosition()
    {
        if (holeType == HoleType.Storehouse)
        {
            return new Vector3(
                Random.Range(-0.2f, 0.2f), 
                0f, 
                Random.Range(-0.4f, 0.4f)); 
        }
        else
        {
             return new Vector3(
                Random.Range(-0.1f, 0.1f),
                0f,
                Random.Range(-0.1f, 0.1f));
        }
    }

    private void UpdateSeedText()
    {
        if (seedText != null)
        {
            seedText.text = seeds.Count.ToString();
        }
    }
}