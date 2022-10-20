using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class CheckPoint : MonoBehaviour {
    private CapsuleCollider _collider;
    private CapsuleCollider _playerCollider;

    private void Awake() {
        _collider = GetComponent<CapsuleCollider>();
        _playerCollider = GameObject.Find("Player").GetComponent<CapsuleCollider>();
    }

    private void Start() {
        _collider.radius = _playerCollider.radius;
        _collider.height = _playerCollider.height;
    }
}
