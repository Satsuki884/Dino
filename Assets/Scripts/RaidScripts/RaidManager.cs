using System;
using System.Collections.Generic;
using UnityEngine;

public class RaidManager : MonoBehaviour
{
    public static RaidManager Instance;

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

        if (!dino.CanGoToRaid())
        {
            Debug.Log("This dino cannot go to raid.");
            return false;
        }

        if (IsAlreadyInRaid(dino))
            return false;

        RaidEntry entry = new RaidEntry();
        entry.dino = dino;
        entry.returnPosition = returnPosition;
        entry.raidDuration = dino.GetRaidDuration();
        entry.timeLeft = entry.raidDuration;
        entry.rewardCoins = dino.GetRaidReward();

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

    private bool IsAlreadyInRaid(Dino dino)
    {
        foreach (RaidEntry entry in activeRaids)
        {
            if (entry.dino == dino)
                return true;
        }

        return false;
    }

    public bool TryGetRaidEntryForDino(Dino dino, out RaidEntry foundEntry)
    {
        foreach (RaidEntry entry in activeRaids)
        {
            if (entry != null && entry.dino == dino)
            {
                foundEntry = entry;
                return true;
            }
        }

        foundEntry = null;
        return false;
    }

    public void RestoreRaidDino(
    Dino dino,
    Vector3 returnPosition,
    float savedTimeLeft,
    float raidDuration,
    float rewardCoins,
    float offlineSeconds
)
    {
        if (dino == null)
            return;

        float newTimeLeft = savedTimeLeft - offlineSeconds;

        if (newTimeLeft <= 0f)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddCoins(rewardCoins);

            dino.EndRaidMode(returnPosition);

            OnRaidsChanged?.Invoke();
            return;
        }

        RaidEntry entry = new RaidEntry();
        entry.dino = dino;
        entry.returnPosition = returnPosition;
        entry.raidDuration = raidDuration;
        entry.timeLeft = newTimeLeft;
        entry.rewardCoins = rewardCoins;

        activeRaids.Add(entry);

        dino.StartRaidMode();

        OnRaidsChanged?.Invoke();
    }
}

[System.Serializable]
public class RaidEntry
{
    public Dino dino;
    public Vector3 returnPosition;

    public float raidDuration;
    public float timeLeft;
    public float rewardCoins;
}