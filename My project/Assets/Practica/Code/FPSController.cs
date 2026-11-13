using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    bool m_AngleLocked = false;
    public Transform m_PitchController;
    bool m_AttachingObject = false;
    bool m_ObjectAttached = false;
    float m_GrabTime;
    Rigidbody m_GrabObject;

    // public GameUI GameUI;
    float m_Yaw;
    float m_Pitch = 0.0f;
    private Vector3 m_Direction;
    public float m_YawSpeed;
    public float m_PitchSpeed;
    public bool m_InvertedYaw;
    public bool m_InvertedPitch;
    public float m_MinPitch;
    public float m_MaxPitch;
    public float m_Speed;
    CharacterController m_CharacterController;
    float m_VerticalSpeed = 0.0f;
    public float m_JumpSpeed = 0.0f;
    public float m_SprintSpeed = 0.0f;

    [Header("Input")]
    public KeyCode m_RightKeyCode = KeyCode.D;
    public KeyCode m_LeftKeyCode = KeyCode.A;
    public KeyCode m_UpKeyCode = KeyCode.W;
    public KeyCode m_DownKeyCode = KeyCode.S;
    public KeyCode m_JumpKeyCode = KeyCode.Space;
    public KeyCode m_GrabObjectKeyCode = KeyCode.E;
    public KeyCode m_SprintKeyCode = KeyCode.LeftShift;
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;
    public KeyCode m_DebugLockCursorKeyCode = KeyCode.O;
    public KeyCode m_ReloadKeyCode = KeyCode.R;

    [Header("Shooting")]
    public Camera m_Camera;
    public KeyCode m_ShootKeyCode = KeyCode.Mouse0;
    public KeyCode m_ShootOrangePortalKeyCode = KeyCode.Mouse1;
    public float m_ShootDistance = 200.0f;
    public LayerMask m_ShootLayerMask;

    [Header("Portals")]
    public Portal m_BluePortal;
    public Portal OrangePortal;
    public bool FirstPortal = false;
    public float m_MaxValidAngle = 40.0f;
    public float m_TeleportPortalOffset = 1.0f;

    [Header("Grab")]
    public Transform m_GrabTransform;
    public Vector3 m_GrabOffset;
    public float m_TimeToGrab = 0.0f;
    public float m_ShootAttachedObjectForce = 20.0f;
    public List<GameObject> m_GrabableObjects = new List<GameObject>();

    private void Awake()
    {
        m_CharacterController=GetComponent<CharacterController>();
    }

    private void Start()
    {
        m_Yaw = transform.rotation.eulerAngles.y;
        m_Pitch = transform.rotation.eulerAngles.x;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if(Input.GetKeyDown(m_DebugLockAngleKeyCode))
            m_AngleLocked = !m_AngleLocked;
        if(Input.GetKeyDown(m_DebugLockCursorKeyCode))
            Cursor.lockState = Cursor.lockState != CursorLockMode.Locked ? CursorLockMode.Locked : CursorLockMode.None;

        float l_MouseX = Input.GetAxis("Mouse X");
        float l_MouseY = Input.GetAxis("Mouse Y");

        if (m_InvertedYaw) l_MouseX = -l_MouseX;
        if (m_InvertedPitch) l_MouseY = -l_MouseY;

        if(!m_AngleLocked)
        {
            m_Yaw += l_MouseX * m_YawSpeed * Time.deltaTime;
            m_Pitch -= l_MouseY * m_PitchSpeed * Time.deltaTime;
        }

        m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);

        transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
        m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0.0f, 0.0f);

        m_Direction = Vector3.zero;
        Vector3 l_Right=transform.right;
        Vector3 l_Forward=transform.forward;
        l_Right.y=0.0f;
        l_Forward.y=0.0f;
        l_Right.Normalize();
        l_Forward.Normalize();

        if(Input.GetKey(m_RightKeyCode)) m_Direction=transform.right;
        else if(Input.GetKey(m_LeftKeyCode)) m_Direction=-transform.right;

        if(Input.GetKey(m_UpKeyCode)) m_Direction+=transform.forward;
        if(Input.GetKey(m_DownKeyCode)) m_Direction-=transform.forward;

        float l_Speed = m_Speed;

        if(Input.GetKey(m_SprintKeyCode)) l_Speed = m_SprintSpeed;

        if(m_CharacterController.isGrounded)
        {
            if(m_VerticalSpeed < 0.0f) m_VerticalSpeed = -2.0f;
            if(Input.GetKeyDown(m_JumpKeyCode)) m_VerticalSpeed = m_JumpSpeed;
        }
        else m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;

        m_Direction.Normalize();
        Vector3 l_Direction = m_Direction * l_Speed * Time.deltaTime;
        m_Direction.y = m_VerticalSpeed * Time.deltaTime;

        m_CharacterController.Move(l_Direction);

        if (CanGrabObject())
        {
            GrabObject();
        }
        if(CanShoot() && MustShoot())
        {
            if(FirstPortal == false) Shoot(m_BluePortal);
            else Shoot(OrangePortal);
        }
        UpdateAttachingObject();
        UpdateAttachedObject();
    }
    bool CanGrabObject()
    {
        if (Input.GetKeyDown(m_GrabObjectKeyCode) && !m_AttachingObject && !m_ObjectAttached)
            return true;
        return false;
    }
    void GrabObject()
    {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootDistance, m_ShootLayerMask.value))
        {
            foreach (GameObject l_Object in m_GrabableObjects)
            {
                if(l_RaycastHit.collider.CompareTag(l_Object.tag))
                {
                    m_GrabOffset = new Vector3(0, l_Object.transform.localScale.y*2.0f, 0);
                    GrabObject(l_RaycastHit.collider);
                    break;
                }
            }
        }
    }
    void GrabObject(Collider _Collider)
    { 
        m_GrabObject = _Collider.GetComponent<Rigidbody>();
        m_GrabObject.isKinematic = true;
        m_AttachingObject = true;
        m_GrabObject.transform.SetParent(m_GrabTransform);
        m_GrabTime = Time.time;
    }
    void UpdateAttachingObject()
    {
        if (m_AttachingObject)
        {
            float l_DiffTime = Time.time - m_GrabTime;
            float l_Pct = Mathf.Min(1.0f, l_DiffTime / m_TimeToGrab);
            m_GrabObject.transform.position = Vector3.Lerp(m_GrabObject.transform.position + m_GrabOffset, m_GrabTransform.position, l_Pct);
            m_GrabObject.transform.rotation = Quaternion.Lerp(m_GrabObject.transform.rotation, m_GrabTransform.rotation, l_Pct);
            if (l_Pct == 1.0f)
            {
                m_AttachingObject = false;
                m_ObjectAttached = true;
            }
        }
    }

    void UpdateAttachedObject()
    {
        if (m_ObjectAttached)
        {
            if (Input.GetKeyUp(m_GrabObjectKeyCode))
                ThrowAttachedObject(0.0f);
            else if (Input.GetKeyDown(m_ShootKeyCode))
                ThrowAttachedObject(m_ShootAttachedObjectForce);
        }
    }

    void ThrowAttachedObject(float Force)
    {
        m_GrabObject.isKinematic = false;
        m_GrabObject.linearVelocity = m_PitchController.transform.forward * Force;
        m_GrabObject.transform.SetParent(null);
        m_ObjectAttached = false;
    }

    bool CanShoot()
    {
        return !(m_AttachingObject || m_ObjectAttached);
    }

    bool MustShoot()
    {
        
        if(FirstPortal == false)
        {
            FirstPortal = !FirstPortal;
            return Input.GetKeyDown(m_ShootKeyCode);
        }
        else
        {
            FirstPortal = !FirstPortal;
            return Input.GetKeyDown(m_ShootOrangePortalKeyCode);
        }
    }

    void Shoot(Portal _Portal)
    {
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

        if(Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootDistance, m_ShootLayerMask.value))
        {
            if(_Portal.IsValidPosition(l_RaycastHit.point, l_RaycastHit.normal))
                _Portal.gameObject.SetActive(true);
            else
                _Portal.gameObject.SetActive(false);
        }
        else
            _Portal.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("DeadZone"))
        {
           Kill(); 
        }
        else if(other.CompareTag("Portal"))
        {
            Portal l_Portal = other.GetComponent<Portal>();
            if(CanTeleport(l_Portal))
            Teleport(l_Portal);
        }
        else if (other.CompareTag("CompanionSpawner"))
        {
            other.GetComponent<CompanionSpawner>().Execute();
        }
    }
    private bool CanTeleport(Portal Portal)
    {
        float l_MaxDotAngle = Mathf.Cos(m_MaxValidAngle *  Mathf.Deg2Rad);
        float l_DotAngle = Vector3.Dot(-m_Direction, Portal.transform.forward);
        return l_DotAngle >= l_MaxDotAngle;
    }
    void Teleport(Portal _Portal)
    {
        Vector3 l_WorldPosition = transform.position + m_Direction * m_TeleportPortalOffset;
        Vector3 l_WorldForward = transform.forward;
        Vector3 l_LocalPosition = _Portal.m_OtherPortalTransform.InverseTransformPoint(l_WorldPosition);
        Vector3 l_LocalForward = _Portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldForward);

        m_CharacterController.enabled = false;
        transform.forward = _Portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);
        transform.position = _Portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);
        m_Yaw = transform.rotation.eulerAngles.y;
        m_Direction = Vector3.zero;
        m_CharacterController.enabled = true;
    }
    void Kill()
    {
        
    }
}
