using UnityEngine;

public class Turret : MonoBehaviour
{
    public LineRenderer m_Laser;
    public float m_MaxDistance = 30.0f;
    public LayerMask m_LaserLeyerMask;
    public float m_Damage = 50.0f;

    private void Update()
    {
        Vector3 l_Position = Vector3.forward * m_MaxDistance;
        Ray l_Ray = new Ray(m_Laser.transform.position, m_Laser.transform.forward);

        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxDistance, m_LaserLeyerMask.value, QueryTriggerInteraction.Ignore))
        {
            l_Position = Vector3.forward * l_RaycastHit.distance;

            if (l_RaycastHit.collider.CompareTag("Player"))
            {
                FPSController l_Player = l_RaycastHit.collider.GetComponent<FPSController>();
                if (l_Player != null)
                {
                    l_Player.TakeDamage(m_Damage * Time.deltaTime);
                }
            }
            else
            {
                Turret l_OtherTurret = l_RaycastHit.collider.GetComponentInParent<Turret>();
                if (l_OtherTurret != null && l_OtherTurret != this)
                {
                    Destroy(l_OtherTurret.gameObject);
                }
            }
        }
        m_Laser.SetPosition(1, l_Position);
    }
}