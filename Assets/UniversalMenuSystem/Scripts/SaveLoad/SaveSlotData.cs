// ============================================================================
// Universal Menu System - Save Slot Data
// Compatible with Unity 6000.0+
// ============================================================================
// Metadata for a single save slot. The actual game data is opaque (string).
// ============================================================================

using System;

namespace UniversalMenuSystem.SaveLoad
{
    [Serializable]
    public class SaveSlotData
    {
        public string slotId;
        public string displayName;
        public string timestamp;
        public float playTimeSeconds;
        public string locationName;
        public string thumbnailBase64;
        public string payload;
        public bool hasData;

        public DateTime GetDateTime()
        {
            if (DateTime.TryParse(timestamp, out DateTime dt))
                return dt;
            return DateTime.MinValue;
        }

        public string GetFormattedPlayTime()
        {
            TimeSpan ts = TimeSpan.FromSeconds(playTimeSeconds);
            return ts.Hours > 0
                ? $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        public string GetFormattedTimestamp()
        {
            DateTime dt = GetDateTime();
            if (dt == DateTime.MinValue) return "---";
            return dt.ToString("yyyy-MM-dd  HH:mm");
        }

        public static SaveSlotData CreateEmpty(string slotId, string displayName)
        {
            return new SaveSlotData
            {
                slotId = slotId,
                displayName = displayName,
                timestamp = "",
                playTimeSeconds = 0f,
                locationName = "",
                thumbnailBase64 = null,
                payload = "",
                hasData = false
            };
        }
    }
}
