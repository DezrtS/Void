using UnityEngine;

public class InverseKinematicsObject : MonoBehaviour
{
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;
    [SerializeField] private Transform lookAtTarget;

    public Transform LeftHandTarget => leftHandTarget;
    public Transform RightHandTarget => rightHandTarget;
    public Transform LookAtTarget => lookAtTarget;
}