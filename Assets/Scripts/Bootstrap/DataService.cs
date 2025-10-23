using System;
using System.IO;
using UnityEngine;

public static class DataService
{
    public static readonly string ProfilesFolder = "Profiles";
    public static readonly string SettingsFolder = "Settings";
    
    //private static readonly string filePath = Application.persistentDataPath + "/profile.json";

    public static void Save<T>(string saveFolder, string fileName, T dataToSave)
    {
        try
        {
            string saveDirectory = Path.Combine(Application.persistentDataPath, saveFolder);
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            string savePath = Path.Combine(saveDirectory, fileName);
            string jsonData = JsonUtility.ToJson(dataToSave, true);
            File.WriteAllText(savePath, jsonData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save profile: {e.Message}");
        }
        
    }

    public static T Load<T>(string loadFolder, string fileName)
    {
        try
        {
            string loadPath = Path.Combine(Application.persistentDataPath, loadFolder, fileName);
            if (!File.Exists(loadPath))
            {
                return default;
            }

            string jsonData = File.ReadAllText(loadPath);
            T loadedData = JsonUtility.FromJson<T>(jsonData);
            Debug.Log($"Loaded profile from {loadPath}");
            return loadedData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load profile: {e.Message}");
            return default;
        }
    }
}
