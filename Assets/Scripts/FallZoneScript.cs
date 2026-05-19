using UnityEngine;
using UnityEngine.SceneManagement;

public class FallZoneScript : MonoBehaviour
{
    [SerializeField]
    int endSceneIndex = 2;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        RopuchControllerScript ropuch = other.GetComponentInParent<RopuchControllerScript>();
        if (ropuch != null)
        {
            SceneManager.LoadScene(endSceneIndex);
            return;
        }

        GryficaScript gryfica = other.GetComponentInParent<GryficaScript>();
        if (gryfica != null)
        {
            Destroy(gryfica.transform.root.gameObject);
            return;
        }

        DziuniaScript dziunia = other.GetComponentInParent<DziuniaScript>();
        if (dziunia != null)
        {
            Destroy(dziunia.transform.root.gameObject);
            return;
        }
    }
}