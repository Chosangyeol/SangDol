using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] protected string InteractName = "G - 상호작용";
    [SerializeField] protected bool CanInteract = false;
    [SerializeField] protected Transform interactUIPoint;
    private CapsuleCollider col;

    public string interactName => InteractName;
    public bool canInteract => CanInteract;

    public GameObject interactUI;
    public RawImage interactImage;
    public TMP_Text interactText;
    public Vector3 offset;

    private void Start()
    {
        Init(InteractName);
        interactUI.SetActive(false);
        if (col == null)
            col = GetComponent<CapsuleCollider>();
    }

    private void LateUpdate()
    {
        if (!canInteract) return;

        Vector3 pos = Camera.main.WorldToScreenPoint(interactUIPoint.position + offset);
        interactImage.gameObject.transform.position = pos;
        interactText.gameObject.transform.position = pos;
    }


    public void Init(string text)
    {
        if (interactText == null) return;
        
        interactText.text = text;
    }

    public bool Interact(Transform target)
    {
        if (!canInteract) return false;

        Debug.Log("상호작용");
        return true;
    }
    
    public void EnableInteract(Transform target)
    {
        if (interactUI == null) return;

        CanInteract = true;
        interactUI.SetActive(true);
    }
    
    public void DisableInteract(Transform target)
    {
        if (interactUI == null) return;

        CanInteract = false;
        interactUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        EnableInteract(other.transform);

    }

    private void OnTriggerExit(Collider other)
    {
        DisableInteract(other.transform);
    }
}

