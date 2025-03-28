using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookMutation : ProjectileMutation
{
    public class GrapplingHookAttachment
    {
        private Vector3 position;
        private MovementController movementController;
        private float pullForce;
        private float creationTime;
        private float duration;

        public GrapplingHookAttachment(Vector3 position, MovementController movementController, float pullForce, float duration)
        {
            this.position = position;
            this.movementController = movementController;
            this.pullForce = pullForce;
            creationTime = Time.timeSinceLevelLoad;
            this.duration = duration;
        }

        public void Pull()
        {
            Vector3 difference = position - movementController.transform.position;
            movementController.RequestApplyForce(difference.normalized * pullForce, ForceMode.Force);
        }

        public bool IsWithinRange()
        {
            Vector3 difference = position - movementController.transform.position;
            return difference.magnitude <= 5;
        }

        public bool IsExpired()
        {
            return (Time.timeSinceLevelLoad - creationTime) > duration;
        }
    }

    [SerializeField] private float pullForce;
    [SerializeField] private float grappleDuration;
    private List<GrapplingHookAttachment> grapplingHookAttachments = new List<GrapplingHookAttachment>();

    private void Start()
    {
        projectileSpawner.OnHit += OnProjectileHit;
    }

    public override void UpdateTimers()
    {
        base.UpdateTimers();

        if (!networkUseable.IsOwner) return;
        for (int i = 0; i < grapplingHookAttachments.Count; i++)
        {
            GrapplingHookAttachment grapplingHookAttachment = grapplingHookAttachments[i];
            grapplingHookAttachment.Pull();

            if (grapplingHookAttachment.IsWithinRange() || grapplingHookAttachment.IsExpired())
            {
                grapplingHookAttachments.RemoveAt(i);
                i--;
            }
        }
    }

    private void OnProjectileHit(Projectile projectile, ProjectileSpawner projectileSpawner, RaycastHit raycastHit)
    {
        if (raycastHit.collider.TryGetComponent(out MovementController movementController))
        {
            grapplingHookAttachments.Add(new GrapplingHookAttachment(transform.position, movementController, pullForce, grappleDuration));
        }
        else
        {
            grapplingHookAttachments.Add(new GrapplingHookAttachment(raycastHit.point, player.GetComponent<MovementController>(), pullForce, grappleDuration));
        }
    }
}