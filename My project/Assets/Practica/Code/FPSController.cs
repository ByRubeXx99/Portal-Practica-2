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

    // public GameUI GameUI;
    float m_Yaw;
    float m_Pitch = 0.0f;
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
    public KeyCode m_SprintKeyCode = KeyCode.LeftShift;
    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;
    public KeyCode m_DebugLockCursorKeyCode = KeyCode.O;
    public KeyCode m_ReloadKeyCode = KeyCode.R;

    [Header("Shooting")]
    public Camera m_Camera;
    public KeyCode m_ShootKeyCode = KeyCode.Mouse0;
    public float m_ShootDistance = 200.0f;
    public LayerMask m_ShootLayerMask;

    [Header("Portals")]
    Portal m_BluePortal;

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

        Vector3 l_Direction = Vector3.zero;
        Vector3 l_Right=transform.right;
        Vector3 l_Forward=transform.forward;
        l_Right.y=0.0f;
        l_Forward.y=0.0f;
        l_Right.Normalize();
        l_Forward.Normalize();

        if(Input.GetKey(m_RightKeyCode)) l_Direction=transform.right;
        else if(Input.GetKey(m_LeftKeyCode)) l_Direction=-transform.right;

        if(Input.GetKey(m_UpKeyCode)) l_Direction+=transform.forward;
        if(Input.GetKey(m_DownKeyCode)) l_Direction-=transform.forward;

        float l_Speed = m_Speed;

        if(Input.GetKey(m_SprintKeyCode)) l_Speed = m_SprintSpeed;

        if(m_CharacterController.isGrounded)
        {
            if(m_VerticalSpeed < 0.0f) m_VerticalSpeed = -2.0f;
            if(Input.GetKeyDown(m_JumpKeyCode)) m_VerticalSpeed = m_JumpSpeed;
        }
        else m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;

        l_Direction.Normalize();
        l_Direction = l_Direction * l_Speed * Time.deltaTime;
        l_Direction.y = m_VerticalSpeed * Time.deltaTime;

        m_CharacterController.Move(l_Direction);

        if(CanShoot() && MustShoot()) Shoot(m_BluePortal);
    }

    bool CanShoot()
    {
        return true;
    }

    bool MustShoot()
    {
        return Input.GetKeyDown(m_ShootKeyCode);
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
}
