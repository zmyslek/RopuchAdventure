using System;
using UnityEngine;
using System.Linq;

namespace TechJuego.LifeSystem
{
    //Events to get update of current life count and remaining time for a profile
    public class LifeEvents
    {
        public delegate void OnUpdateLife(string profileId, int lifecount, string remainingTime);
        public static OnUpdateLife OnGetLifeDetail;
    }

    [System.Serializable]
    public class ProfileSetting
    {
        public string ProfileId = "";
        public int MaxLifeCount = 3;
        public int TimeToAddLifeInSeconds = 60;
    }

    public class LifeHandler : MonoBehaviour
    {
        public static LifeHandler Instance;
        private readonly string lifeHolder = "LIFE";
        private LifeData lifeData = new LifeData();

        [Header("Profile settings (configure one per character)")]
        [SerializeField] private ProfileSetting[] Profiles = new ProfileSetting[0];

        private static ProfileSetting[] GetDefaultProfiles()
        {
            return new[]
            {
                new ProfileSetting { ProfileId = "Ropuch", MaxLifeCount = 5, TimeToAddLifeInSeconds = 60 },
                new ProfileSetting { ProfileId = "Dziunia", MaxLifeCount = 10, TimeToAddLifeInSeconds = 45 },
                new ProfileSetting { ProfileId = "Gryf", MaxLifeCount = 3, TimeToAddLifeInSeconds = 30 },
            };
        }

        private static string NormalizeProfileId(string profileId)
        {
            if (string.Equals(profileId, "Gryfica", StringComparison.OrdinalIgnoreCase))
            {
                return "Gryf";
            }

            return profileId;
        }

        private void EnsureDefaultProfiles()
        {
            if (Profiles == null || Profiles.Length == 0)
            {
                Profiles = GetDefaultProfiles();
            }

            for (int i = 0; i < Profiles.Length; i++)
            {
                if (Profiles[i] == null)
                {
                    continue;
                }

                Profiles[i].ProfileId = NormalizeProfileId(Profiles[i].ProfileId);
            }
        }

        private void BroadcastProfileState(ProfileEntry entry, ProfileSetting setting)
        {
            if (entry == null || setting == null)
            {
                return;
            }

            string remainingTime = "Full";
            if (entry.CurrentLifeCount < setting.MaxLifeCount && entry.AddedNextTime.Count > 0)
            {
                TimeSpan span = DateTime.Parse(entry.AddedNextTime[0]) - DateTime.Now;
                remainingTime = GetRemainingTime(span);
            }

            LifeEvents.OnGetLifeDetail?.Invoke(entry.ProfileId, entry.CurrentLifeCount, remainingTime);
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            EnsureDefaultProfiles();

            if (PlayerPrefs.HasKey(lifeHolder))
            {
                try
                {
                    lifeData = JsonUtility.FromJson<LifeData>(PlayerPrefs.GetString(lifeHolder));
                }
                catch
                {
                    lifeData = new LifeData();
                }
            }

            // Ensure each configured profile exists in saved data
            foreach (var p in Profiles)
            {
                if (string.IsNullOrEmpty(p.ProfileId))
                    continue;

                p.ProfileId = NormalizeProfileId(p.ProfileId);

                var entry = lifeData.profiles.FirstOrDefault(x => x.ProfileId == p.ProfileId);
                if (entry == null)
                {
                    entry = new ProfileEntry() { ProfileId = p.ProfileId, CurrentLifeCount = p.MaxLifeCount };
                    lifeData.profiles.Add(entry);
                }
                else
                {
                    // clamp saved value to configured max
                    entry.CurrentLifeCount = Math.Min(entry.CurrentLifeCount, p.MaxLifeCount);
                }
            }

            foreach (var entry in lifeData.profiles)
            {
                entry.ProfileId = NormalizeProfileId(entry.ProfileId);
            }

            PlayerPrefs.SetString(lifeHolder, JsonUtility.ToJson(lifeData));
            CheckLife();
        }

        // reduce life for a specific profile
        public void LooseLife(string profileId)
        {
            profileId = NormalizeProfileId(profileId);
            var entry = GetProfileEntry(profileId);
            var setting = GetProfileSetting(profileId);
            if (entry == null || setting == null)
                return;

            if (entry.CurrentLifeCount > 0)
            {
                entry.CurrentLifeCount -= 1;
                PlayerPrefs.SetString(lifeHolder, JsonUtility.ToJson(lifeData));
                SetTimeToAddNextLife(profileId);
                BroadcastProfileState(entry, setting);
            }
        }

        // add life for profile
        public void AddLife(string profileId)
        {
            profileId = NormalizeProfileId(profileId);
            var entry = GetProfileEntry(profileId);
            var setting = GetProfileSetting(profileId);
            if (entry == null || setting == null)
                return;

            if (entry.CurrentLifeCount < setting.MaxLifeCount)
            {
                entry.CurrentLifeCount += 1;
                if (entry.CurrentLifeCount >= setting.MaxLifeCount)
                {
                    entry.CurrentLifeCount = setting.MaxLifeCount;
                    entry.AddedNextTime.Clear();
                }
                PlayerPrefs.SetString(lifeHolder, JsonUtility.ToJson(lifeData));
                BroadcastProfileState(entry, setting);
            }
        }

        // refill all lives for a profile
        public void RefillLife(string profileId)
        {
            profileId = NormalizeProfileId(profileId);
            var entry = GetProfileEntry(profileId);
            var setting = GetProfileSetting(profileId);
            if (entry == null || setting == null)
                return;

            entry.CurrentLifeCount = setting.MaxLifeCount;
            entry.AddedNextTime = new System.Collections.Generic.List<string>();
            PlayerPrefs.SetString(lifeHolder, JsonUtility.ToJson(lifeData));
            BroadcastProfileState(entry, setting);
        }

        // update remaining time per-profile
        private void Update()
        {
            foreach (var entry in lifeData.profiles)
            {
                var setting = GetProfileSetting(entry.ProfileId);
                if (setting == null)
                    continue;

                if (entry.CurrentLifeCount < setting.MaxLifeCount)
                {
                    if (entry.AddedNextTime.Count > 0)
                    {
                        TimeSpan span = DateTime.Parse(entry.AddedNextTime[0]) - DateTime.Now;
                        LifeEvents.OnGetLifeDetail?.Invoke(entry.ProfileId, entry.CurrentLifeCount, GetRemainingTime(span));
                        if (span.TotalSeconds < 0)
                        {
                            entry.AddedNextTime.RemoveAt(0);
                            AddLife(entry.ProfileId);
                        }
                    }
                    else
                    {
                        LifeEvents.OnGetLifeDetail?.Invoke(entry.ProfileId, entry.CurrentLifeCount, "Full");
                    }
                }
                else
                {
                    LifeEvents.OnGetLifeDetail?.Invoke(entry.ProfileId, entry.CurrentLifeCount, "Full");
                }
            }
        }

        // get time in string format to show
        private string GetRemainingTime(TimeSpan timeSpan)
        {
            string time = "";
            if (timeSpan.TotalSeconds <= 0)
            {
                time = "Full";
            }
            else
            {
                time = String.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
            }
            return time;
        }

        // check before can play
        public bool CanPlay(string profileId)
        {
            var entry = GetProfileEntry(profileId);
            return entry != null && entry.CurrentLifeCount > 0;
        }

        // helper: set time to add next life for a profile
        void SetTimeToAddNextLife(string profileId)
        {
            profileId = NormalizeProfileId(profileId);
            var entry = GetProfileEntry(profileId);
            var setting = GetProfileSetting(profileId);
            if (entry == null || setting == null)
                return;

            var seconds = setting.TimeToAddLifeInSeconds;
            if (entry.AddedNextTime.Count > 0)
            {
                string times = entry.AddedNextTime[entry.AddedNextTime.Count - 1];
                DateTime nextTime = DateTime.Parse(times).AddSeconds(seconds);
                entry.AddedNextTime.Add(nextTime.ToString());
            }
            else
            {
                DateTime nextTime = DateTime.Now.AddSeconds(seconds);
                entry.AddedNextTime.Add(nextTime.ToString());
            }
            PlayerPrefs.SetString(lifeHolder, JsonUtility.ToJson(lifeData));
        }

        // function to check on start
        void CheckLife()
        {
            foreach (var entry in lifeData.profiles)
            {
                var setting = GetProfileSetting(entry.ProfileId);
                if (setting == null)
                {
                    continue;
                }

                BroadcastProfileState(entry, setting);
            }
        }

        ProfileEntry GetProfileEntry(string id)
        {
            id = NormalizeProfileId(id);
            if (string.IsNullOrEmpty(id))
                return null;
            return lifeData.profiles.FirstOrDefault(x => x.ProfileId == id);
        }

        ProfileSetting GetProfileSetting(string id)
        {
            id = NormalizeProfileId(id);
            if (Profiles == null)
                return null;
            return Profiles.FirstOrDefault(x => x.ProfileId == id);
        }

        // public getter
        public int GetCurrentLifeCount(string id)
        {
            var e = GetProfileEntry(id);
            return e == null ? 0 : e.CurrentLifeCount;
        }

        public bool TryGetLifeState(string id, out int lifeCount, out string remainingTime)
        {
            id = NormalizeProfileId(id);

            lifeCount = 0;
            remainingTime = "Full";

            var entry = GetProfileEntry(id);
            var setting = GetProfileSetting(id);
            if (entry == null || setting == null)
            {
                return false;
            }

            lifeCount = entry.CurrentLifeCount;
            if (entry.CurrentLifeCount < setting.MaxLifeCount && entry.AddedNextTime.Count > 0)
            {
                TimeSpan span = DateTime.Parse(entry.AddedNextTime[0]) - DateTime.Now;
                remainingTime = GetRemainingTime(span);
            }

            return true;
        }
    }
}