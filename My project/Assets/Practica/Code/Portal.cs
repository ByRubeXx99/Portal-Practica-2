using System;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public FPSController m_Player;
    public Camera m_Camera;
    public Transform m_OtherPortalTransform;
    public Portal m_MirrorPortal;
    public float m_CameraOffset = 0.0f;
    public List<Transform> m_ValidPositions;
    public float m_RaycastValidationOffsetDistance = 0.1f;
    public float m_MaxAngleValidation = 5.0f;

    void Update()
    {
        Camera l_Camera = m_Player.m_Camera;
        Vector3 l_CameraWorldPosition = l_Camera.transform.position;
        Vector3 l_DirectionWorldPosition = l_Camera.transform.forward;
        Vector3 l_CameraLocalPosition = m_MirrorPortal.transform.InverseTransformPoint(l_CameraWorldPosition);
        Vector3 l_DirectionLocalPosition = m_MirrorPortal.transform.InverseTransformDirection(l_DirectionWorldPosition);
        m_Camera.transform.position = m_OtherPortalTransform.TransformPoint(l_CameraLocalPosition);
        m_Camera.transform.forward = m_OtherPortalTransform.TransformDirection(l_DirectionLocalPosition);

        m_Camera.nearClipPlane = 0.1f;
        m_Camera.ResetProjectionMatrix();

        Vector3 l_PlaneNormal = transform.forward;
        Vector3 l_PlanePos = transform.position + l_PlaneNormal * m_CameraOffset;
        Vector3 l_CamSpacePos = m_Camera.worldToCameraMatrix.MultiplyPoint(l_PlanePos);
        Vector3 l_CamSpaceNormal = m_Camera.worldToCameraMatrix.MultiplyVector(l_PlaneNormal).normalized;
        float l_CamSpaceDist = -Vector3.Dot(l_CamSpacePos, l_CamSpaceNormal);

        if (l_CamSpaceDist < -0.05f)
        {
            Vector4 l_ClipPlane = new Vector4(l_CamSpaceNormal.x, l_CamSpaceNormal.y, l_CamSpaceNormal.z, l_CamSpaceDist);
            m_Camera.projectionMatrix = m_Camera.CalculateObliqueMatrix(l_ClipPlane);
        }
    }

    public bool IsValidPosition(Vector3 Position, Vector3 Normal)
    {
        Camera l_Camera = m_Player.m_Camera;
        transform.position = Position;
        transform.rotation = Quaternion.LookRotation(Normal);
        float l_MaxDotAngle = Mathf.Cos(m_MaxAngleValidation*Mathf.Deg2Rad);
        
        for(int i=0; i<m_ValidPositions.Count; i++)
        {
            Vector3 l_Direction = m_ValidPositions[i].position - l_Camera.transform.position;
            float l_Distance = l_Direction.magnitude + m_RaycastValidationOffsetDistance;
            l_Direction.Normalize();
            Ray l_Ray = new Ray(l_Camera.transform.position, l_Direction);
            if(Physics.Raycast(l_Ray, out RaycastHit l_HitInfo, l_Distance, m_Player.m_ShootLayerMask.value, QueryTriggerInteraction.Ignore))
            {
                if(l_HitInfo.collider.CompareTag("PortalEnabled"))
                {
                    float l_DistanceToHitPoint = Vector3.Distance(l_HitInfo.point, m_ValidPositions[i].position);
                    if(l_DistanceToHitPoint < m_RaycastValidationOffsetDistance)
                    {
                        float l_DotAngle = Vector3.Dot(Normal, l_HitInfo.normal);
                        if(l_DotAngle<l_MaxDotAngle)
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }
        return true;
    }
}
