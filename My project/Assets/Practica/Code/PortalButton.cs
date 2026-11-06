using UnityEngine;
using UnityEngine.Events;

public class PortalButton : MonoBehaviour
{
    public string m_InteractionTag = "Player";
    public UnityEvent m_Event;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(m_InteractionTag))
        {
            m_Event.Invoke();
        }
    }
}
