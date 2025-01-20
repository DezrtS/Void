using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatData", menuName = "Scriptable Objects/PlayerStatData")]
public class PlayerStatData : ScriptableObject
{
    [Header("Health")]
    [SerializeField] private Stat maxHealth = new Stat(100);

    [Header("Movement", order = 0)]
    [Header("Acceleration")]
    [SerializeField] private Stat timeToAccelerate = new Stat(0.25f);
    [SerializeField] private Stat timeToDeaccelerate = new Stat(0.25f);

    [Header("Speed")]
    [SerializeField] private Stat walkSpeed = new Stat(4);
    [SerializeField] private Stat sprintSpeed = new Stat(7);
    [SerializeField] private Stat crouchSpeed = new Stat(2);

    [Header("Jump")]
    [SerializeField] private Stat jumpPower = new Stat(7);

    [Header("Crouch")]
    [SerializeField] private Stat crouchHeight = new Stat(1);

    public Stat MaxHealth => maxHealth;
    public Stat TimeToAccelerate => timeToAccelerate;
    public Stat TimeToDeacclerate => timeToDeaccelerate;

    public Stat WalkSpeed => walkSpeed;
    public Stat SprintSpeed => sprintSpeed;
    public Stat CrouchSpeed => crouchSpeed;

    public Stat JumpPower => jumpPower;

    public Stat CrouchHeight => crouchHeight;
}