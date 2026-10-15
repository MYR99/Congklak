using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIController : MonoBehaviour
{
    public static AIController Instance { get; private set; }

    private GameManager gameManager;
    private Hole[] aiHoles;
    private Hole[] playerHoles;
    private Hole aiStorehouse;
    private Hole playerStorehouse;

    private int totalSowableHoles;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Initialize(GameManager gm, Hole[] p2Holes, Hole[] p1Holes, Hole p2Storehouse, Hole p1Storehouse)
    {
        gameManager = gm;
        aiHoles = p2Holes;
        playerHoles = p1Holes;
        aiStorehouse = p2Storehouse;
        playerStorehouse = p1Storehouse;

        totalSowableHoles = playerHoles.Length + aiHoles.Length + 1;
    }

    public void MakeMove(AILevel difficulty)
    {
        StartCoroutine(MakeMoveCoroutine(difficulty));
    }

    private IEnumerator MakeMoveCoroutine(AILevel difficulty)
    {
        yield return new WaitForSeconds(Random.Range(1f, 1.5f));

        float randomChance = Random.Range(0f, 1f);
        Hole selectedHole = null;

        switch (difficulty)
        {
            case AILevel.Easy:
                selectedHole = randomChance < 0.9f ? FindBestMove_Random() : FindBestMove_MediumLogic();
                break;

            case AILevel.Medium:
                selectedHole = randomChance < 0.5f ? FindBestMove_Random() : FindBestMove_MediumLogic();
                break;

            case AILevel.Hard:
                selectedHole = randomChance < 0.4f ? FindBestMove_Random() : FindBestMove_HardLogic();
                break;
        }

        if (selectedHole == null)
            selectedHole = FindBestMove_Random();

        if (selectedHole != null)
            gameManager.SelectHole(selectedHole);
        else
            gameManager.EndTurnAfterAI();
    }

    private List<Hole> GetValidMoves()
    {
        return aiHoles.Where(h => h.GetSeedCount() > 0).ToList();
    }

    private Hole FindBestMove_Random()
    {
        var list = GetValidMoves();
        if (list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }

    private Hole FindExtraTurnMove()
    {
        foreach (var h in GetValidMoves())
            if (IsExtraTurn(h)) return h;
        return null;
    }

    private Hole FindBestMove_MediumLogic()
    {   
        return FindExtraTurnMove() ?? FindBestMove_Random();
    }

    private Hole FindBestMove_HardLogic()
    {
        Hole extra = FindExtraTurnMove();
        if (extra != null) return extra;

        Hole best = null;
        int bestGain = 0;

        foreach (var hole in GetValidMoves())
        {
            Hole last = SimulateSow(hole);
            if (last == null || last.GetHoleType() != HoleType.PlayerTwo || last.GetSeedCount() != 0)
                continue;

            Hole opposite = GetOppositeHole(last.GetHoleIndex());
            if (opposite == null) continue;

            int gain = opposite.GetSeedCount() + 1;
            if (gain > bestGain) { bestGain = gain; best = hole; }
        }
        return best ?? FindBestMove_Random();
    }
 
    private bool IsExtraTurn(Hole hole)
    {
        int seed = hole.GetSeedCount();
        int index = hole.GetHoleIndex();
        int dist = aiHoles.Length - index;

        return (seed % totalSowableHoles == dist);
    }

    private Hole GetOppositeHole(int index)
    {
        int oppIndex = playerHoles.Length - 1 - index;
        if (oppIndex < 0 || oppIndex >= playerHoles.Length) return null;
        return playerHoles[oppIndex];
    }

    private Hole SimulateSow(Hole startHole)
    {
        int seedCount = startHole.GetSeedCount();
        Hole current = startHole;

        for (int i = 0; i < seedCount; i++)
        {
            current = gameManager.GetNextHolePublic(current);

            if (current == playerStorehouse)
                current = gameManager.GetNextHolePublic(current);
        }

        return current;
    }
}