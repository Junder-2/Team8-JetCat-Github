using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using Level;
using System.Linq;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class ThirdPersonController : MonoBehaviour, IDamageable
{
    [SerializeField] private AudioClip _deathSound;
    [SerializeField] private AudioSource _audioSource;
    
    private Rigidbody _rb;
    private CapsuleCollider _collider;
    private Vector2 _playerInput;
    private Vector3 _velocity, _desiredVelocity;
    [SerializeField]private float _maxAcceleration = 50f, _maxAirAcceleration = 18;
    private float _movementSpeed = 7f;
    private bool _desiredJump;
    private const float MinFallTime = 1f;
    private const float MinYVelocity = -10f;
    public float[] AvailableSpeeds; // add speeds to the player controller
    private int _currentSpeedIndex = 0;
    private Animator _animator;
    private float _coyoteTimer = 0.1f;
    private float _coyoteTimerCounter;
    private float _jumpBufferTime = 0.2f;
    private float _jumpBufferCounter = 0.2f;
    private float _xAirControl = 0.3f;
    private float _zAirControl = 0.5f;
    [SerializeField] private WinnerSO winner;
    

    private float _jumpHeight = 3;

    //private float _fallTime = 0;
    private float _maxGroundAngle = 25f, _maxStairAngle = 50f;
    private float _minGroundDotProduct, _minStairDotProduct;
    private Vector3 _contactNormal, _steepContactNormal;
    private int _groundContactNormalCount, _steepContactNormalCount;
    [SerializeField] private int _maxAirJumps = 0;
    private int _jumpPhase;
    private int _stepsSinceLastGrounded, _stepsSinceLastJump;
    private float _maxSnapSpeed = 100;
    private float _probeDistance = 1f;
    [SerializeField] private LayerMask _probeMask = -1, _stairMask = -1;

    private float _movementSpeedZ;

    //[SerializeField] private Transform _playerInputSpace = default;
    private bool OnGround => _groundContactNormalCount > 0;
    private bool OnSteep => _steepContactNormalCount > 0;

    private void HandleMovement() {
        UpdateState();
        AdjustVelocity();
        if (_jumpBufferCounter > 0) {
            _desiredJump = false;
            Jump();
        }

        _rb.velocity = _velocity;
        ClearState();
    }

    private float GetMinDot(int layer) {
        return (_stairMask & (1 << layer)) == 0 ? _minGroundDotProduct : _minStairDotProduct;
    }

    private void ClearState() {
        _groundContactNormalCount = _steepContactNormalCount = 0;
        _contactNormal = _steepContactNormal = Vector3.zero;
    }

    private void FlipCharacter() {
    }

    public void CharacterJump(bool input) {
        _jumpBufferCounter = _jumpBufferTime;
        var velocity = _rb.velocity;
        velocity = new Vector3(0, velocity.y, velocity.z);
        _rb.velocity = velocity;
        _playerInput.x = 0;
        _desiredJump = input;
    }

    private void UpdateState() {
        _stepsSinceLastGrounded += 1;
        _stepsSinceLastGrounded = Mathf.Clamp(_stepsSinceLastGrounded, 0, int.MaxValue);
        _stepsSinceLastJump += 1;
        _stepsSinceLastJump = Mathf.Clamp(_stepsSinceLastJump, 0, Int32.MaxValue);
        _velocity = _rb.velocity;
        if (OnGround || SnapToGround()) {
            _stepsSinceLastGrounded = 0;
            _jumpPhase = 0;
            if (_groundContactNormalCount > 1) {
                _contactNormal.Normalize();
            }
        }
        else {
            _contactNormal = Vector3.up;
        }
    }

    public void ChangeMovementSpeed(bool increase) {
        if (AvailableSpeeds == null) return;

        int lastSpeedIndex = _currentSpeedIndex;

        if (increase) _currentSpeedIndex++;
        else _currentSpeedIndex--;

        _currentSpeedIndex = Mathf.Clamp(_currentSpeedIndex, 0, AvailableSpeeds.Length - 1); // -1? 

        _movementSpeedZ = AvailableSpeeds[_currentSpeedIndex];
        
        if(increase && _currentSpeedIndex != lastSpeedIndex)
            ParticleManager.Instance.PlayParticleEffect("Dash Smoke", _rb.position, Quaternion.identity);
        
    }

    public void ResetMovementSpeed() {
        if (AvailableSpeeds == null) return;

        _movementSpeedZ = AvailableSpeeds[0];
    }

    bool SnapToGround() {
        if (_stepsSinceLastGrounded > 1 || _stepsSinceLastJump <= 2) return false;

        float speed = _velocity.magnitude;

        if (speed > _maxSnapSpeed) return false;

        if (!Physics.Raycast(_rb.position, Vector3.down, out RaycastHit hit, _probeDistance)) return false;

        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer)) return false;

        _groundContactNormalCount = 1;
        _contactNormal = hit.normal;

        float dot = Vector3.Dot(_velocity, hit.normal);
        if (dot > 0f) {
            _velocity = (_velocity - hit.normal * dot).normalized * speed;
        }

        return true;
    }

    private Vector3 ProjetOnContactPlane(Vector3 vector) {
        return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
    }

    private void AdjustVelocity() {
        Vector3 xAxis = ProjetOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjetOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(_velocity, xAxis); // this is from the input manager, rest one is from currentSpeed
        float currentZ = Vector3.Dot(_velocity, zAxis);

        float acceleration = OnGround ? _maxAcceleration : _maxAirAcceleration;
        float maxChangeSpeed = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxChangeSpeed);
        float newZ = Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxChangeSpeed);
        _velocity += OnGround
            ? xAxis * (newX - currentX) + zAxis * (newZ - currentZ)
            : xAxis * ((newX - currentX) * _xAirControl) + zAxis * ((newZ - currentZ) * _zAirControl);
    }

    private void OnValidate() {
        _minGroundDotProduct = Mathf.Cos(_maxGroundAngle * Mathf.Deg2Rad);
        _minStairDotProduct = Mathf.Cos(_maxStairAngle * Mathf.Deg2Rad);
    }

    private void Jump() {
        if (OnGround && _coyoteTimerCounter > 0f || _jumpPhase < _maxAirJumps) {
            _stepsSinceLastJump = 0;
            _jumpPhase++;
            _animator.SetTrigger("Jump");
            ParticleManager.Instance.PlayParticleEffect("Jump and Land", _rb.position, Quaternion.identity);
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * _jumpHeight);
            float alignedSpeed = Vector3.Dot(_velocity, _contactNormal);
            if (alignedSpeed > 0f) {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0);
            }
            _coyoteTimerCounter = 0f;
            _velocity += _contactNormal * jumpSpeed;
        }
    }

    private void OnCollisionStay(Collision collision) => EvaluateCollision(collision);
    private void OnCollisionExit(Collision collision) => EvaluateCollision(collision);

    private void EvaluateCollision(Collision collision) {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minDot) {
                _groundContactNormalCount += 1;
                _contactNormal += normal;
            }
            else if (normal.y > -0.01f) {
                _steepContactNormalCount += 1;
                _steepContactNormal += normal;
            }
        }
    }

    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
        _animator = GetComponent<Animator>();
        OnValidate();
    }

    private void Start() {
        CurrentHealth = MaxHealth;
    }

    public void SetMoveVector(Vector2 moveVector) {
        _playerInput.x = moveVector.x;
        //_playerInput.y = moveVector.y;
        _playerInput = Vector2.ClampMagnitude(_playerInput, 1f);
    }


    private void FixedUpdate() {
        HandleMovement();
    }

    private void StopTransform() {
        _rb.velocity = Vector3.zero;
        _playerInput = Vector2.zero;
    }

    private void Update() {
        if (!_desiredJump) _jumpBufferCounter -= Time.deltaTime;

        if (OnGround) _coyoteTimerCounter = _coyoteTimer;
        else _coyoteTimerCounter -= Time.deltaTime;
        
        _desiredVelocity = new Vector3(_playerInput.x, 0, _movementSpeedZ) * _movementSpeed; // WAT?
    }

    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; } = 10;

    private bool _death = false;
    private void Die()
    {
        _death = true;
        winner.winner = 2;
        StartCoroutine(GameOverDelay());
    }

    private IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(2f);
        GameManager.Instance.GameOver();
    }
    
    public void TakeDamage(int amount) {
        if(_death)
            return;
        
        _audioSource.PlayOneShot(_deathSound);
        CurrentHealth -= amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        if (CurrentHealth == 0) Die();
        else StartCoroutine(PlayerTakingDamage());
    }
    
    void RespawnPlayerAtClosestPoint() {
        var closestPoint = FindObjectsOfType<CheckPoint>();
        if (closestPoint != null)
        {
            Vector3 suitableCheckPoint = Vector3.zero;

            var sortedCheckPoints = closestPoint.OrderBy(checkPoint => Vector3.SqrMagnitude(checkPoint.transform.position - transform.position)).ToList();
            foreach (var checkPoint in sortedCheckPoints) {
                if (checkPoint.transform.position.z < transform.position.z + 5f) {
                    suitableCheckPoint = checkPoint.transform.position;
                    break;
                }
            }

            transform.position = suitableCheckPoint;
            GameManager.Instance.AddPlayerRespawn();
            
        }
    }

    private IEnumerator PlayerTakingDamage()
    {
        _death = true;
        
        UIManager uiManager = UIManager.Instance;
        
        StopTransform();
        Time.timeScale = 0;
        uiManager.FadeOut(1f);
        while (uiManager.Fading) {
            yield return null;   
        }
        RespawnPlayerAtClosestPoint();
        ResetMovementSpeed();
        uiManager.FadeIn(1f);
        Time.timeScale = 1;
        _death = false;
    }
}