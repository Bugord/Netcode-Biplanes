using UnityEngine;

namespace UI.Screens
{
    public abstract class BaseScreen : MonoBehaviour
    {
        public virtual void Close()
        {
            NavigationSystem.Instance.PopScreen(this);
        }

        public virtual void Destroy()
        {
            Destroy(gameObject);
        }

        public virtual void SetInactive()
        {
            gameObject.SetActive(false);
        }

        public virtual void SetActive()
        {
            gameObject.SetActive(true);
        }
    }
}