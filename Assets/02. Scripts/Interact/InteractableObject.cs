using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    public string InteractText { get; private set; }
    public void Init(string text)
    {
        InteractText = text;
    }
    public void Interact(CharacterModel character)
    {
        
    }
    public void EnableInteract()
    {
        
    }
    public void DisableInteract()
    {
        
    }
}

