using System;
using UnityEngine;
using UnityEngine.UI;
namespace TechJuego.LifeSystem
{
    // Simple UI bridge that listens to a single profile's life events
    public class LifeUi : MonoBehaviour
    {
        public string ProfileId = "Ropuch"; // set in inspector to the profile this UI represents
        public Text m_LifeCount;
        public Text m_TileRemaining;

        private void OnEnable()
        {
            LifeEvents.OnGetLifeDetail += LifeEvents_OnUpdateLife;
            SyncFromHandler();
        }

        private void Start()
        {
            SyncFromHandler();
        }

        private void LifeEvents_OnUpdateLife(string profileId, int lifecount, string remainingTime)
        {
            if (!string.Equals(profileId, ProfileId, StringComparison.OrdinalIgnoreCase))
                return;

            if (m_LifeCount != null)
                m_LifeCount.text = lifecount.ToString();
            if (m_TileRemaining != null)
                m_TileRemaining.text = remainingTime;
        }

        private void OnDisable()
        {
            LifeEvents.OnGetLifeDetail -= LifeEvents_OnUpdateLife;
        }

        private void SyncFromHandler()
        {
            if (LifeHandler.Instance == null)
            {
                return;
            }

            if (!LifeHandler.Instance.TryGetLifeState(ProfileId, out int lifeCount, out string remainingTime))
            {
                return;
            }

            if (m_LifeCount != null)
                m_LifeCount.text = lifeCount.ToString();
            if (m_TileRemaining != null)
                m_TileRemaining.text = remainingTime;
        }
    }
}