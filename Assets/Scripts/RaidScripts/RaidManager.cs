using System;
using System.Collections.Generic;
using UnityEngine;

public class RaidManager : MonoBehaviour
{
    public static RaidManager Instance;

    [Header("Raid Settings")]
    public float raidDuration = 300f; // 5 minutes
    public int rewardPerDinoLevel = 100;

    private readonly List<RaidEntry> activeRaids = new List<RaidEntry>();

    public IReadOnlyList<RaidEntry> ActiveRaids => activeRaids;

    public event Action OnRaidsChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateRaidTimers();
    }

    public bool AddDinoToRaid(Dino dino, Vector3 returnPosition)
    {
        if (dino == null)
            return false;

        if (dino.IsInRaid())
            return false;

        if (IsAlreadyInRaid(dino))
            return false;

        RaidEntry entry = new RaidEntry();
        entry.dino = dino;
        entry.returnPosition = returnPosition;
        entry.timeLeft = raidDuration;
        entry.rewardCoins = CalculateReward(dino);

        activeRaids.Add(entry);

        dino.StartRaidMode();

        OnRaidsChanged?.Invoke();

        return true;
    }

    private void UpdateRaidTimers()
    {
        if (activeRaids.Count == 0)
            return;

        for (int i = activeRaids.Count - 1; i >= 0; i--)
        {
            RaidEntry entry = activeRaids[i];

            if (entry == null || entry.dino == null)
            {
                activeRaids.RemoveAt(i);
                OnRaidsChanged?.Invoke();
                continue;
            }

            entry.timeLeft -= Time.deltaTime;

            if (entry.timeLeft <= 0f)
            {
                CompleteRaid(entry);
            }
        }
    }

    private void CompleteRaid(RaidEntry entry)
    {
        if (entry == null)
            return;

        if (!activeRaids.Contains(entry))
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.AddCoins(entry.rewardCoins);

        ReturnDino(entry);
    }

    public void CancelRaid(RaidEntry entry)
    {
        if (entry == null)
            return;

        if (!activeRaids.Contains(entry))
            return;

        ReturnDino(entry);
    }

    private void ReturnDino(RaidEntry entry)
    {
        activeRaids.Remove(entry);

        if (entry.dino != null)
            entry.dino.EndRaidMode(entry.returnPosition);

        OnRaidsChanged?.Invoke();
    }

    private int CalculateReward(Dino dino)
    {
        if (dino == null)
            return 0;

        return Mathf.Max(1, dino.Level) * rewardPerDinoLevel;
    }

    private bool IsAlreadyInRaid(Dino dino)
    {
        foreach (RaidEntry entry in activeRaids)
        {
            if (entry.dino == dino)
                return true;
        }

        return false;
    }
}

[System.Serializable]
public class RaidEntry
{
    public Dino dino;
    public Vector3 returnPosition;
    public float timeLeft;
    public int rewardCoins;
}