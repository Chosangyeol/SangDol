using UnityEngine;
using TMPro;
using UnityEngine.UI;

public interface IInteractable
{
    string interactName { get; }
    bool canInteract { get; }

    void Init(string text);

    bool Interact(Transform target);

    void EnableInteract(Transform target);

    void DisableInteract(Transform target);

}
