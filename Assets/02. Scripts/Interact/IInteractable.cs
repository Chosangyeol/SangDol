using UnityEngine;

public interface IInteractable
{
    public string InteractText { get; }

    public void Init(string text);

    public void Interact(CharacterModel character);

    public void EnableInteract();

    public void DisableInteract();

}
