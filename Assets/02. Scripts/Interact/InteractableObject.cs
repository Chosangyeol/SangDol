using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class InteractableObject : PoolableMono, IInteractable
{
    [Header("Base Settings")]
    [SerializeField] protected string InteractName = "G - 상호작용";
    [SerializeField] protected bool CanInteract = false;
    [SerializeField] protected bool IsLocked = true;
    [SerializeField] protected InteractType _interactType;

    [Header("UI Settings")]
    [SerializeField] protected Transform interactUIPoint;
    public GameObject interactUI;
    public TMP_Text interactText;
    public Vector3 offset;

    [Header("Hold Settings")]
    public bool IsHoldInteraction = false;
    public bool IsAutoProgress = false;
    public float RequiredHoldTime = 2.0f;

    private Camera _mainCam;
    private CapsuleCollider col;

    public GameObject interactObject;

    public string interactName => InteractName;
    public bool canInteract => CanInteract;
    public bool isLocked => IsLocked;
    public InteractType interactType => _interactType;
    public bool isHoldInteraction => IsHoldInteraction;
    public bool isAutoProgress => IsAutoProgress;
    public float holdTime => RequiredHoldTime;



    protected virtual void Awake()
    {
        _mainCam = Camera.main;
        if (col == null)
            col = GetComponent<CapsuleCollider>();
    }

    protected virtual void Start()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        Camera overlayCam = null;

        foreach (var cam in Camera.allCameras)
        {
            var data = cam.GetUniversalAdditionalCameraData();
            if (data.renderType == CameraRenderType.Overlay) // URP 전용 체크
            {
                overlayCam = cam;
                break;
            }
        }

        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            canvas.worldCamera = _mainCam;
        }

        if (overlayCam != null)
        {
            canvas.worldCamera = overlayCam;
            _mainCam = overlayCam; // LateUpdate에서 Billboard 처리를 위해 저장
        }

        Init(InteractName);
        if (interactUI != null) interactUI.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!CanInteract || interactUI == null || !interactUI.activeSelf) return;

        // 4. UI가 카메라를 정면으로 바라보게 함
        interactUI.transform.forward = _mainCam.transform.forward;
    }


    public virtual void Init(string text)
    {
        if (interactText != null) interactText.text = text;
    }

    public virtual void SetLock(bool locked)
    {
        IsLocked = locked;
        if (CanInteract) UpdateUIState();
    }

    public virtual bool Interact(Transform target)
    {
        if (!CanInteract || IsLocked)
        {
            Debug.Log($"{InteractName}은 현재 사용할 수 없습니다.");
            return false;
        }

        // 성공 시 머리 위 가이드 UI를 꺼줌 (로아 스타일)
        if (interactUI != null) interactUI.SetActive(false);
        return true;
    }

    protected virtual void UpdateUIState()
    {
        if (interactUI == null) return;

        // 범위 안에 있고 + 잠겨있지 않을 때만 "G" 가이드 표시
        interactUI.SetActive(CanInteract && !IsLocked);
    }

    public void EnableInteract(Transform target)
    {
        CanInteract = true;
        UpdateUIState();
    }
    
    public void DisableInteract(Transform target)
    {
        CanInteract = false;
        UpdateUIState();
    }

    public virtual void OnInteractCancel()
    {
        UpdateUIState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) EnableInteract(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) DisableInteract(other.transform);
    }
}

