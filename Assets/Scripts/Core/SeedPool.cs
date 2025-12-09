using System.Collections.Generic;
using UnityEngine;

public class SeedPool : MonoBehaviour
{
    public static SeedPool Instance { get; private set; }

    [SerializeField] private GameObject seedPrefab;

    [Header("Global Settings")]
    [SerializeField] private float seedScale = 1f; // scale global

    private Stack<GameObject> pool = new Stack<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public GameObject GetSeed()
    {
        GameObject seed = pool.Count > 0 ? pool.Pop() : Instantiate(seedPrefab);
        seed.SetActive(true);

        // scale & reset transform
        seed.transform.localScale = Vector3.one * seedScale;
        seed.transform.rotation = Quaternion.identity;
        seed.transform.localPosition = Vector3.zero;

        return seed;
    }

    public void ReturnSeed(GameObject seed)
    {
        if (seed == null) return;
        seed.SetActive(false);
        seed.transform.SetParent(transform);
        seed.transform.localPosition = Vector3.zero;
        seed.transform.localRotation = Quaternion.identity;
        pool.Push(seed);
    }

    // ✅ tambahan untuk Hole.cs
    public float GetSeedScale()
    {
        return seedScale;
    }
}