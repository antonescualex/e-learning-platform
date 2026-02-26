using System.IO;
using UnityEngine;

namespace Storage
{
    public class JsonStorage : IStorage
    {
        private readonly string _basePath;

        public JsonStorage(string baseFolderName = "SavedData")
        {
            _basePath = Path.Combine(Application.persistentDataPath, baseFolderName);
            Directory.CreateDirectory(_basePath);
        }

        private string FullPath(string relativePath)
        {
            return Path.Combine(_basePath, relativePath);
        }

        public bool Exists(string relativePath)
        {
            return File.Exists(FullPath(relativePath));
        }

        public void Save<T>(string relativePath, T data)
        {
            var path = FullPath(relativePath);
            var directory = Path.GetDirectoryName(path);
            var jsonData = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, jsonData);
        }

        public bool TryLoad<T>(string relativePath, out T data)
        {
            var path = FullPath(relativePath);
            if (!File.Exists(path))
            {
                data = default;
                return false;
            }

            var jsonData = File.ReadAllText(path);
            data = JsonUtility.FromJson<T>(jsonData);
            return data != null;
        }
    }
}