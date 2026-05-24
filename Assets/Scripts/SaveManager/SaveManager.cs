using System;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private const string SaveKey = "merge_dino_save";
    private const string SaveFileName = "merge_dino_save.json";

    private static string SavePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }
    }

    public static void Save(GameSaveData data)
    {
        if (data == null)
            return;

        data.lastSaveUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        string json = JsonUtility.ToJson(data, true);

#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
#else
        File.WriteAllText(SavePath, json);
#endif

        Debug.Log("Game saved.");
    }

    public static GameSaveData Load()
    {
        string json = "";

#if UNITY_WEBGL && !UNITY_EDITOR
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("Save not found in PlayerPrefs.");
            return null;
        }

        json = PlayerPrefs.GetString(SaveKey);
#else
        if (!File.Exists(SavePath))
        {
            Debug.Log("Save file not found.");
            return null;
        }

        json = File.ReadAllText(SavePath);
#endif

        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonUtility.FromJson<GameSaveData>(json);
    }

    public static bool HasSave()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return PlayerPrefs.HasKey(SaveKey);
#else
        return File.Exists(SavePath);
#endif
    }

    public static void DeleteSave()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
#else
        if (File.Exists(SavePath))
            File.Delete(SavePath);
#endif

        Debug.Log("Save deleted.");
    }

    public static long GetCurrentUnixTime()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}