using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private FirstPersonController _firstPersonController;
    [SerializeField] private StarterAssetsInputs _input;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _attackDistance = 3f;
    [SerializeField] private float _attackDelay = 0.4f;
    [SerializeField] private float _attackSpeed = 1f;
    [SerializeField] private int _attackDamage = 1;
    [SerializeField] private LayerMask _attackLayer;

    [SerializeField] private GameObject _hitEffect;
    [SerializeField] private AudioClip _swordSwing;
    [SerializeField] private AudioClip _hitSound;

    private bool attacking = false;
    private bool readyToAttack = true;
    private int attackCount = 0;
    private PlayerInputSchema _playerInput;

    private void Awake()
    {
        
    }

    private void Update()
    {
        
    }

    public void Attack()
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        //Invoke(nameof(ResetAttack), _attackSpeed);
        //Invoke(nameof(AttackRaycast), _attackDelay);

        _audioSource.pitch = Random.Range(0.9f, 1.1f);
        _audioSource.PlayOneShot(_swordSwing);
    }

    private void ResetAttack()
    {
        readyToAttack = true;
        attacking = false;
    }
}
