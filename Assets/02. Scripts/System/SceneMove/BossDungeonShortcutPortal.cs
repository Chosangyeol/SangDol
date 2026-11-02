using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
public class BossDungeonShortcutPortal : InteractableObject
{
    [SerializeField] private Transform destination;
    [SerializeField, Min(1)] private int targetLevel = 10;
    [SerializeField] private bool applyPortalTrialSetup = true;

    protected override void Start()
    {
        base.Start();
        Init(interactName);
    }

    public override bool Interact(Transform target)
    {
        if (!canInteract || isLocked || target == null || destination == null) return false;

        CharacterModel player = target.GetComponentInParent<CharacterModel>();
        if (player == null || player.Stat == null || player.PlayerController == null) return false;

        if (!NavMesh.SamplePosition(destination.position, out NavMeshHit hit, 2f, NavMesh.AllAreas)) return false;
        if (!base.Interact(target)) return false;

        Vector3 previousPosition = player.transform.position;
        Vector3 arrival = hit.position;
        player.PlayerController.TeleportTo(arrival);
        if (Vector3.Distance(player.transform.position, arrival) > 1f) return false;

        if (applyPortalTrialSetup)
            PortalTrialSetup.Apply(player, targetLevel);
        player.transform.rotation = destination.rotation;

        Vector3 displacement = player.transform.position - previousPosition;
        if (player.cams != null)
        {
            foreach (var camera in player.cams)
                if (camera != null) camera.OnTargetObjectWarped(player.transform, displacement);
        }

        return true;
    }

    protected override void OnTriggerEnter(Collider other) => base.OnTriggerEnter(other);
    protected override void OnTriggerExit(Collider other) => base.OnTriggerExit(other);
}
