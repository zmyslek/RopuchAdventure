using UnityEngine;
using UnityEngine.UI;
namespace TechJuego.LifeSystem
{
    public class UiHandler : MonoBehaviour
    {
        public Button m_AddLife;
        public Button m_LooseLife;
        public Button m_RefillAllLife;
        public string ProfileId = "Ropuch";
        private void OnEnable()
        {
            m_AddLife.onClick.RemoveAllListeners();
            m_AddLife.onClick.AddListener(OnClickAddLife);

            m_LooseLife.onClick.RemoveAllListeners();
            m_LooseLife.onClick.AddListener(OnClickLooseLife);

            m_RefillAllLife.onClick.RemoveAllListeners();
            m_RefillAllLife.onClick.AddListener(OnClickRefillAllLife);
        }
        void OnClickAddLife()
        {
            if (LifeHandler.Instance != null)
                LifeHandler.Instance.AddLife(ProfileId);
        }
        void OnClickLooseLife()
        {
            if (LifeHandler.Instance != null)
                LifeHandler.Instance.LooseLife(ProfileId);
        }
        void OnClickRefillAllLife()
        {
            if (LifeHandler.Instance != null)
                LifeHandler.Instance.RefillLife(ProfileId);
        }
    }
}