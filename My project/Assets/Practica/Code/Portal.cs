using UnityEngine;

public class Portal : MonoBehaviour
{
    public Camera m_Camera;
    public Transform m_OtherPortalTransform;
    public Portal m_MirrorPortal;

    void Start()
    {
        
    }

    void Update()
    {
        Camera l_Camera = Camera.main;
        Vector3 l_CameraWorldPosition = l_Camera.transform.position;
        Vector3 l_DirectionWorldPosition = l_Camera.transform.forward;
        Vector3 l_CameraLocalPosition = m_MirrorPortal.transform.InverseTransformPoint(l_CameraWorldPosition);
        Vector3 l_DirectionLocalPosition = m_MirrorPortal.transform.InverseTransformPoint(l_CameraLocalPosition);
        m_Camera.transform.position = m_OtherPortalTransform.TransformPoint(l_CameraLocalPosition);
        m_Camera.transform.forward = m_OtherPortalTransform.TransformDirection(l_DirectionLocalPosition);
    }
}
