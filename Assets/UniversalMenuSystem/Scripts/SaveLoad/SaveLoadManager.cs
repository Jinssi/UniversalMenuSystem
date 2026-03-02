// ============================================================================
// Universal Menu System - Save/Load Manager
// Compatible with Unity 6000.0+
// ============================================================================
// Manages save slots with file-based persistence. Game-agnostic.
// Inject your own ISaveProvider for custom serialization backends.
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniversalMenuSystem.SaveLoad
{
    public interface ISaveProvider
    {
        void WriteSave(string slotId, SaveSlotData data);
        SaveSlotData ReadSave(string slotId);
        void DeleteSave(string slotId);
        bool SaveExists(string slotId);
        List<SaveSlotData> GetAllSaves();
    }

    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private int maxSlots = 10;
        [SerializeField] private string saveSubFolder = "Saves";
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private string autoSaveSlotId = "autosave";

        public event Action<SaveSlotData> OnSaveCompleted;
        public event Action<SaveSlotData> OnLoadCompleted;
        public event Action<string> OnSaveDeleted;
        public event Action<string> OnSaveError;

        public int MaxSlots => maxSlots;
        public bool EnableAutoSave => enableAutoSave;
        public string AutoSaveSlotId => autoSaveSlotId;

        public ISaveProvider Provider
        {
            get => _provider;
            set => _provider = value;
        }

        private ISaveProvider _provider;
        private string _saveDirectory;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _saveDirectory = Path.Combine(Application.persistentDataPath, saveSubFolder);
            if (_provider == null)
                _provider = new DefaultFileSaveProvider(_saveDirectory);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Save(string slotId, string payload, string locationName = "", float playTimeSeconds = 0f)
        {
            try
            {
                SaveSlotData data = new SaveSlotData
                {
                    slotId = slotId,
                    displayName = GetDisplayName(slotId),
                    timestamp = DateTime.Now.ToString("o"),
                    playTimeSeconds = playTimeSeconds,
                    locationName = locationName,
                    payload = payload,
                    hasData = true
                };
                _provider.WriteSave(slotId, data);
                OnSaveCompleted?.Invoke(data);
                Debug.Log($"[SaveLoad] Saved to slot: {slotId}");
            }
            catch (Exception e)
            {
                string msg = $"Failed to save slot '{slotId}': {e.Message}";
                Debug.LogError($"[SaveLoad] {msg}");
                OnSaveError?.Invoke(msg);
            }
        }

        public bool Load(string slotId, out string payload)
        {
            payload = null;
            try
            {
                if (!_provider.SaveExists(slotId))
                {
                    Debug.LogWarning($"[SaveLoad] Slot '{slotId}' does not exist.");
                    return false;
                }
                SaveSlotData data = _provider.ReadSave(slotId);
                if (data == null || !data.hasData)
                {
                    Debug.LogWarning($"[SaveLoad] Slot '{slotId}' is empty.");
                    return false;
                }
                payload = data.payload;
                OnLoadCompleted?.Invoke(data);
                Debug.Log($"[SaveLoad] Loaded from slot: {slotId}");
                return true;
            }
            catch (Exception e)
            {
                string msg = $"Failed to load slot '{slotId}': {e.Message}";
                Debug.LogError($"[SaveLoad] {msg}");
                OnSaveError?.Invoke(msg);
                return false;
            }
        }

        public SaveSlotData GetSlotInfo(string slotId)
        {
            if (!_provider.SaveExists(slotId))
                return SaveSlotData.CreateEmpty(slotId, GetDisplayName(slotId));
            return _provider.ReadSave(slotId);
        }

        public List<SaveSlotData> GetAllSlots()
        {
            List<SaveSlotData> slots = new List<SaveSlotData>();
            if (enableAutoSave)
                slots.Add(GetSlotInfo(autoSaveSlotId));
            for (int i = 0; i < maxSlots; i++)
                slots.Add(GetSlotInfo($"slot_{i}"));
            return slots;
        }

        public void DeleteSlot(string slotId)
        {
            try
            {
                _provider.DeleteSave(slotId);
                OnSaveDeleted?.Invoke(slotId);
                Debug.Log($"[SaveLoad] Deleted slot: {slotId}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoad] Failed to delete slot '{slotId}': {e.Message}");
            }
        }

        public bool HasAnySaveData()
        {
            if (enableAutoSave && _provider.SaveExists(autoSaveSlotId)) return true;
            for (int i = 0; i < maxSlots; i++)
                if (_provider.SaveExists($"slot_{i}")) return true;
            return false;
        }

        public SaveSlotData GetMostRecentSave()
        {
            List<SaveSlotData> all = GetAllSlots();
            SaveSlotData newest = null;
            DateTime newestTime = DateTime.MinValue;
            foreach (var slot in all)
            {
                if (!slot.hasData) continue;
                DateTime dt = slot.GetDateTime();
                if (dt > newestTime)
                {
                    newestTime = dt;
                    newest = slot;
                }
            }
            return newest;
        }

        private string GetDisplayName(string slotId)
        {
            if (slotId == autoSaveSlotId) return "AUTOSAVE";
            return slotId.Replace("slot_", "Slot ").ToUpperInvariant();
        }
    }

    public class DefaultFileSaveProvider : ISaveProvider
    {
        private readonly string _directory;

        public DefaultFileSaveProvider(string directory)
        {
            _directory = directory;
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
        }

        public void WriteSave(string slotId, SaveSlotData data)
        {
            string path = GetPath(slotId);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }

        public SaveSlotData ReadSave(string slotId)
        {
            string path = GetPath(slotId);
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveSlotData>(json);
        }

        public void DeleteSave(string slotId)
        {
            string path = GetPath(slotId);
            if (File.Exists(path))
                File.Delete(path);
        }

        public bool SaveExists(string slotId) => File.Exists(GetPath(slotId));

        public List<SaveSlotData> GetAllSaves()
        {
            List<SaveSlotData> saves = new List<SaveSlotData>();
            if (!Directory.Exists(_directory)) return saves;
            foreach (string file in Directory.GetFiles(_directory, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    SaveSlotData data = JsonUtility.FromJson<SaveSlotData>(json);
                    if (data != null) saves.Add(data);
                }
                catch { /* Skip corrupted files */ }
            }
            return saves;
        }

        private string GetPath(string slotId) => Path.Combine(_directory, $"{slotId}.json");
    }
}
