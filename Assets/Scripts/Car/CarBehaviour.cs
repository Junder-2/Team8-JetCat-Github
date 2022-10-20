using System;
using UnityEngine;

namespace Car {
    [RequireComponent(typeof(Rigidbody))]
    public abstract class CarBehaviour : MonoBehaviour, IDamageable, IDamager {
        protected Rigidbody Rb;

        private WayPoint[] _wayPoints;

        protected Vector3 MoveDirection;

        [SerializeField] protected float turnSpeed, baseSpeed, addedSpeed, acceleration;
        [SerializeField] protected bool flattenMovementVector;
        [SerializeField] private AudioClip crashSound, honkSound;
        public int Damage { get; set; } = 100;
        private byte _currentGear = 0;
        private const byte MaxGear = 3;
        public int UpwardsModifier = 10;
        public int RagdollMultiplier = 4;

        private float _currentMaxSpeed;

        private Vector3 _directionBetweenWaypoints;

        private float _spawnTimeStamp;

        public float GetTimeAlive() 
        {
            return _spawnTimeStamp-Time.time;
        }

        public Renderer GetRenderer() {
            if (gameObject.GetComponentInChildren<Renderer>()) {
                return gameObject.GetComponentInChildren<Renderer>();
            }

            return null;
        }

        protected virtual void Start() {
            SetGear(0);
            _currentWayPoint = 1;
            _directionBetweenWaypoints =
                _wayPoints[_currentWayPoint].Position - _wayPoints[_currentWayPoint - 1].Position;
            
            Rb = GetComponent<Rigidbody>();

            _spawnTimeStamp = Time.time;

            var invisibleWallsGameObject = GameObject.Find("InvisibleWalls");
            
            if(invisibleWallsGameObject == null)
                return;

            Transform invisibleWalls = invisibleWallsGameObject.transform;
            foreach (Transform child in invisibleWalls) {
                if (child.GetComponent<Collider>()) {
                    Physics.IgnoreCollision(GetComponent<Collider>(), child.GetComponent<Collider>());
                }
            }
        }

        protected virtual void Update() {
            if (_moveOverrideTimer > 0) {
                _moveOverrideTimer -= Time.deltaTime;
                if (_moveOverrideTimer < 0) {
                    _moveOverrideTimer = 0;
                }
            }

            int wayPoint = GetCurrentWayPoint();

            MoveDirection = CalculateDriveDirection(wayPoint, flattenMovementVector);

            DriveLoop();
        }

        public void SetWayPointData(WayPoint[] newData) {
            _wayPoints = newData;

            _moveSign = Mathf.Sign((_wayPoints[0].Position - _wayPoints[^1].Position).x) > 0;
        }

        public void ReplaceCurrentWayPoint(WayPoint newPoint) {
            _wayPoints[_currentWayPoint] = newPoint;
        }

        protected Vector3 CalculateDriveDirection(int waypointIndex, bool flattenVector = true) {
            Vector3 direction = (_wayPoints[waypointIndex].Position - Rb.position);

            if (flattenVector)
                direction.y = 0;

            return direction.normalized;
        }

        private Quaternion _playerMove;
        private float _moveOverrideTimer = 0;
        private int _steerSign;
        public void MoveOverride(float input) //add invertcontrol 
        {
            float dir = MathHelper.ZeroSign(input);

            _moveOverrideTimer = dir == 0 ? 1 : -1;

            _steerSign = (int)dir * (_moveSign ? 1 : -1);

            _playerMove = Quaternion.Euler(0,_steerSign*turnSpeed, 0);
        }

        public void MultiplySpeed(float multi)
        {
            baseSpeed *= multi;
            
            SetGear(_currentGear);
        }
        
        public void ShiftGear(float input) => SetGear(Mathf.Sign(input) < 0 ? _currentGear - 1 : _currentGear + 1);

        private void SetGear(int index) {
            index = Math.Clamp(index, 0, MaxGear);

            _currentGear = (byte)index;

            _currentMaxSpeed = baseSpeed + addedSpeed * _currentGear;
        }

        private int _currentWayPoint = 0;

        private bool _moveSign;

        protected int GetCurrentWayPoint() {
            var i = _currentWayPoint;

            float xDiff = (_wayPoints[i].Position - Rb.position).x;

            bool passed = _moveSign ? xDiff > 0.1f : xDiff < 0.1f;

            if (passed)
                _currentWayPoint++;

            if (_currentWayPoint < _wayPoints.Length)
            {
                if(passed)
                    _directionBetweenWaypoints =
                        _wayPoints[_currentWayPoint].Position - _wayPoints[_currentWayPoint - 1].Position;
                
                return _currentWayPoint;
            }
            DeSpawn();

            return 0;
        }

        private Vector3 _currentMoveDir;
        private float _currentMoveSpeed;
        private const float MaxTurn = 85f;

        protected virtual void DriveLoop()
        {
            if (_moveOverrideTimer == 0)
                _currentMoveDir = Vector3.MoveTowards(_currentMoveDir, MoveDirection, turnSpeed * Time.deltaTime);
            else
            {
                Vector3 nextMoveDirection =
                    Vector3.MoveTowards(_currentMoveDir, _playerMove * _currentMoveDir, Time.deltaTime);

                float nextMoveAngle =
                    Vector3.Angle(_directionBetweenWaypoints.normalized, nextMoveDirection);

                if (nextMoveAngle < MaxTurn)
                    _currentMoveDir = nextMoveDirection;
                else if(nextMoveAngle > MaxTurn+1)
                    _currentMoveDir = Vector3.MoveTowards(_currentMoveDir, MoveDirection, turnSpeed * Time.deltaTime);
            }
            

            if (_currentMoveSpeed > _currentMaxSpeed) {
                _currentMoveSpeed = Mathf.MoveTowards(_currentMoveSpeed, _currentMaxSpeed, Time.deltaTime);
            }
            else _currentMoveSpeed += _currentMaxSpeed * acceleration * (Time.deltaTime);

            Vector3 flatMoveDir = _currentMoveDir;
            flatMoveDir.y = 0;
            flatMoveDir.Normalize();
            
            if(flatMoveDir != Vector3.zero)
                transform.forward = flatMoveDir;

            Rb.velocity = _currentMoveDir * _currentMoveSpeed;
        }
        //todo avoid Objects????

        public void DeSpawn() 
        {
            
            Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.GetComponent<CarBehaviour>())
                collision.gameObject.GetComponent<CarBehaviour>().TakeDamage(100);
            
            if (!collision.gameObject.CompareTag("Player")) return;
            
            AudioSource.PlayClipAtPoint(honkSound, transform.position);
            var contact = collision.GetContact(0);
            var playerRb = collision.gameObject.GetComponent<Rigidbody>();
            playerRb.constraints = RigidbodyConstraints.None;
            playerRb.AddForceAtPosition(
                (Rb.velocity + playerRb.velocity + (Vector3.up * UpwardsModifier)).normalized * RagdollMultiplier *
                Rb.mass, contact.point, ForceMode.Impulse);
            collision.gameObject.GetComponent<ThirdPersonController>().TakeDamage(Damage);
        }
        
        public int CurrentHealth { get; set; } = 10;

        public int MaxHealth { get; set; } = 10;
        public void TakeDamage(int amount) {
            CurrentHealth -= amount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
            AudioSource.PlayClipAtPoint(crashSound, transform.position);
            
            if (CurrentHealth == 0) {
                Die();
                return;
            }
        }
        private void Die() {
            ParticleManager.Instance.PlayParticleEffect("Enemy Death Particle", Rb.position, Quaternion.identity);
            DeSpawn();
        }
    }
}