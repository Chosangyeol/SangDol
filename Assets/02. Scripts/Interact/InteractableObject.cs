using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] protected string InteractName = "G - 상호작용";
    [SerializeField] protected bool CanInteract = false;
    private CapsuleCollider col;

    public string interactName => InteractName;
    public bool canInteract => CanInteract;

    public GameObject interactUI;
    public TMP_Text interactText;

    private void Start()
    {
        Init(InteractName);
        if (col == null)
            col = GetComponent<CapsuleCollider>();
    }

    public void Init(string text)
    {
        interactText.text = text;
    }

    public void Interact(Transform target)
    {
        if (!canInteract) return;

        Debug.Log("상호작용");
    }
    
    public void EnableInteract(Transform target)
    {
        
    }
    
    public void DisableInteract(Transform target)
    {
        
    }
}

