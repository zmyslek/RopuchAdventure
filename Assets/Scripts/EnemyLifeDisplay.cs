using UnityEngine;
using UnityEngine.UI;
using TechJuego.LifeSystem;

// Attach to an enemy prefab. It will show either the centralized profile life (via LifeEvents)
// or be updated manually by the enemy script when LifeHandler is not present.
public class EnemyLifeDisplay : MonoBehaviour
{
    public string ProfileId = "";
    public Text lifeText;

    private void OnEnable()
    {
        LifeEvents.OnGetLifeDetail += OnLifeEvent;
        // initialize text if possible
        if (lifeText != null)
            lifeText.text = "";
    }

    private void OnDisable()
    {
        LifeEvents.OnGetLifeDetail -= OnLifeEvent;
    }

    void OnLifeEvent(string profileId, int lifecount, string remainingTime)
    {
        if (string.IsNullOrEmpty(ProfileId))
            return;

        if (!string.Equals(profileId, ProfileId, System.StringComparison.OrdinalIgnoreCase))
            return;

        if (lifeText != null)
            lifeText.text = lifecount.ToString();
    }

    // Called by enemy scripts when LifeHandler is not available
    public void UpdateLocal(int value)
    {
        if (lifeText != null)
            lifeText.text = value.ToString();
    }
}
