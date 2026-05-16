using System.Collections.Generic;

//Classes to store current life progress and time to add next life per profile
namespace TechJuego.LifeSystem
{
    [System.Serializable]
    public class ProfileEntry
    {
        public string ProfileId;
        public int CurrentLifeCount;
        public List<string> AddedNextTime = new List<string>();
    }

    [System.Serializable]
    public class LifeData
    {
        public List<ProfileEntry> profiles = new List<ProfileEntry>();
    }
}