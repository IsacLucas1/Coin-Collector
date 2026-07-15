using System;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class PlayerMovement : NetworkBehaviour
{
    public float viteza = 5f, vitezaRotatie = 10f, mouseSensitivity = 2f;
    private float xRotation = 0f;
    private Rigidbody rb;
    private Animator animator;
    private Camera cam;
    private Vector3 lastPos;
    private NetworkVariable<int> _netScore = new NetworkVariable<int>(0);
    private NetworkVariable<float> _netAnimSpeed = new NetworkVariable<float>(
        0f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner
    );

    public int Score
    {
        get { return _netScore.Value; }
        set
        {
            if (IsServer)
            {
                _netScore.Value = value;
            }
        }
    }
    public override void OnNetworkSpawn()
    {
        _netScore.OnValueChanged += OnScoreChanged;
        UpdateScoreUI(OwnerClientId, _netScore.Value);
        animator = GetComponentInChildren<Animator>();
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            if (IsOwner)
            {
                cam.enabled = true;
            }
            else
            {
                cam.enabled = false;
            }
        }
        if (skinnedMeshRenderer != null)
        {
            if (OwnerClientId == 0)
            {
                skinnedMeshRenderer.material.color = Color.blue;
            }
            else
            {
                skinnedMeshRenderer.material.color = Color.red;
            }
        }

        if (IsOwner)
        {
            MoveToRandomPosition();
        }
    }

    public override void OnNetworkDespawn()
    {
        _netScore.OnValueChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(int vechi, int nou)
    {
        UpdateScoreUI(OwnerClientId, nou);

        if (nou >= 100)
        {
            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowWinScreen(OwnerClientId);
            }
        }
    }

    private void UpdateScoreUI(ulong clientId, int scoreValue)
    {
        if (GameUI.Instance != null)
        {
            GameUI.Instance.UpdateScoreUI(clientId, scoreValue);
        }
    }

    void MoveToRandomPosition()
    {
        float x = Random.Range(-3f, 3f);
        float z = Random.Range(-3f, 3f);
        transform.position = new Vector3(x, 0.1f, z);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
    }
    void Update()
    {
        if(GameStartManager.Instance!=null && !GameStartManager.Instance.isGameActive)
        {
            return;
        }
        
        if (IsOwner)
        {
            // Dacă suntem proprietarul, calculăm viteza și o trimitem în rețea
            float moveX_anim = Input.GetAxis("Horizontal");
            float moveZ_anim = Input.GetAxis("Vertical");
            
            // Calculăm magnitudinea (cât de tare apăsăm tastele)
            // Clamp la 1 pentru ca pe diagonală să nu depășească 1, apoi înmulțim cu viteza maximă (5)
            float inputMagnitude = Mathf.Clamp01(new Vector2(moveX_anim, moveZ_anim).magnitude);
            float currentSpeed = inputMagnitude * viteza;

            // Scriem valoarea în variabila de rețea.
            // Ceilalți clienți vor citi această valoare automat.
            _netAnimSpeed.Value = currentSpeed;
        }

        // AICI aplicăm animația PENTRU TOATĂ LUMEA (Owner + Ceilalți)
        // Citim valoarea din _netAnimSpeed.Value care este sincronizată
        if (animator != null)
        {
            // Folosim un Lerp mic pentru ca animația să nu "sară" brusc la ceilalți jucători din cauza lag-ului
            float smoothSpeed = Mathf.Lerp(animator.GetFloat("Speed"), _netAnimSpeed.Value, Time.deltaTime * 10f);
            animator.SetFloat("Speed", smoothSpeed);
        }
        
        if (!IsOwner) return;
        
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
        
        Vector3 directie = cam.transform.right * moveX + cam.transform.forward * moveZ;
    
        directie.y = 0;
        if (directie.magnitude > 0.1f) 
        {
            directie = directie.normalized;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Clamp01(new Vector2(moveX, moveZ).magnitude) * viteza);
        }
        
        transform.Translate(directie * viteza * Time.deltaTime, Space.World);
    }
}

