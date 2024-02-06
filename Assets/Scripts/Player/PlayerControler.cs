using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;

public class PlayerControler : MonoBehaviour
{
    private PlayerInputSchema _playerInput;
    public PlayerInputSchema.MainActions input;

    private CharacterController _characterController;
    [SerializeField] private Animator _animator;
    private AudioSource _audioSource;
    [SerializeField] private LayerMask _aimColliderLayerMask;
    [SerializeField] private Transform _aimBall;

    [Header("Controller")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _gravity = -9.8f;
    [SerializeField] private float _jumpHeight = 1.2f;

    private Vector3 _playerVelocity;

    private bool _isGrounded;

    private Vector3 _lastPosition;
    private Vector2 _moveInput;
    private float _moveMinimum = 0.01f;

    public bool freeze = false;
    public bool activeGrapple;
    private Vector3 _velocityToSet;
    private bool _enableMovementOnNextTouch;

    private bool isMoving
    {
        get
        {
            float distance = Vector3.Distance(transform.position, _lastPosition);
            return (distance > _moveMinimum);
        }
    }

    [Header("Camera")]
    [SerializeField] private Camera _camera;
    [SerializeField] float _sensitivity = 15;

    private float _xRotation = 0f;
    private float _yRotation = 0f;

    // ATTACK
    [Header("Attacking")]
    [SerializeField] private float _attackDistance = 3f;
    [SerializeField] private float _attackDelay = 0.4f;
    [SerializeField] private float _attackSpeed = 1f;
    [SerializeField] private int _maxAttackDamage = 6;
    [SerializeField] private int _minAttackDamage = 3;
    [SerializeField] private LayerMask _attackLayer;

    [SerializeField] private GameObject _hitEffect;
    [SerializeField] private AudioClip _swordSwing;
    [SerializeField] private AudioClip _hitSound;

    private bool _attacking = false;
    private bool _readyToAttack = true;
    private int _attackCount = 0;

    // ANIMATIONS
    private const string IDLE = "Idle";
    private const string WALK = "Walk";
    private const string ATTACK1 = "Attack 1";
    private const string ATTACK2 = "Attack 2";

    private string _currentAnimationState;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        _playerInput = new PlayerInputSchema();
        input = _playerInput.Main;
        AssignInputs();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        _moveInput = Vector2.zero;
    }

    private void OnApplicationFocus(bool focus)
    {
        Cursor.lockState = focus ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = focus;
    }

    private void Update()
    {
        _isGrounded = _characterController.isGrounded;

        #region AIM
        Vector3 mouseWorldPosition = Vector3.zero;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, _aimColliderLayerMask))
        {
            _aimBall.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }
        #endregion

        SetAnimations();

        if (activeGrapple) return;

        if (freeze)
        {
            _moveInput = Vector3.zero;
        }

        _moveInput = input.Movement.ReadValue<Vector2>();

        if (input.Attack.IsPressed())
        {
            SwordAttack();
        }

    }

    private void FixedUpdate()
    {
        if (freeze || activeGrapple) return;

        _lastPosition = transform.position;
        MoveInput(_moveInput);
    }

    private void LateUpdate()
    {
        LookInput(input.Look.ReadValue<Vector2>());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_enableMovementOnNextTouch)
        {
            _enableMovementOnNextTouch = false;
            ResetRestrictions();
            GetComponent<Grappling>().StopGrapple();
        }
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void AssignInputs()
    {
        input.Jump.performed += ctx => Jump();
        input.Attack.started += ctx => SwordAttack();
    }

    private void SetAnimations()
    {
        if (!_attacking)
        {
            if (!isMoving)
            {
                ChangeAnimationState(IDLE);
            }
            else
            {
                ChangeAnimationState(WALK);
            }
        }
    }

    private void LookInput(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        _xRotation -= (mouseY * Time.deltaTime) * _sensitivity;
        _xRotation = Mathf.Clamp(_xRotation, -80, 80);

        _camera.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);

        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * _sensitivity);
    }

    private void MoveInput(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;

        float speed = this.input.Sprint.IsPressed() ? _sprintSpeed : _moveSpeed;

        _characterController.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);
        _playerVelocity.y += _gravity * Time.deltaTime;
        if (_isGrounded && _playerVelocity.y < 0)
            _playerVelocity.y = -2f;
        _characterController.Move(_playerVelocity * Time.deltaTime);
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
    }

    private void SetVelocity()
    {
        _enableMovementOnNextTouch = true;
        _characterController.SimpleMove(_velocityToSet);
    }

    private void Jump()
    {
        if (_isGrounded)
        {
            _playerVelocity.y = Mathf.Sqrt(_jumpHeight * -3f * _gravity);
        }
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;
        _velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);

        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = _gravity;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }



    private void SwordAttack()
    {
        if (!_readyToAttack || _attacking)
            return;

        _readyToAttack = false;
        _attacking = true;

        Invoke(nameof(ResetAttack), _attackSpeed);
        Invoke(nameof(AttackRaycast), _attackDelay);

        _audioSource.pitch = Random.Range(0.9f, 1.1f);
        _audioSource.PlayOneShot(_swordSwing);

        if (_attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            _attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            _attackCount = 0;
        }
    }

    private void ResetAttack()
    {
        _attacking = false;
        _readyToAttack = true;
    }

    private void AttackRaycast()
    {
        if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit hit, _attackDistance, _attackLayer))
        {
            HitTarget(hit.point);
            if (hit.transform.TryGetComponent(out Enemy enemy))
            {
                int randomDamage = Random.Range(_minAttackDamage, _maxAttackDamage + 1);
                enemy.Damage(randomDamage);
            }
        }
    }

    private void HitTarget(Vector3 position)
    {
        _audioSource.pitch = 1;
        _audioSource.PlayOneShot(_hitSound);

        GameObject hitEfcGO = Instantiate(_hitEffect, position, Quaternion.identity);
        Destroy(hitEfcGO, 5);
    }

    private void ChangeAnimationState(string newState)
    {
        if (_currentAnimationState == newState)
        {
            return;
        }

        _currentAnimationState = newState;
        _animator.CrossFadeInFixedTime(_currentAnimationState, 0.2f);
    }
}
