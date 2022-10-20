using UnityEngine;

public class PlayerAligner : MonoBehaviour {
    private Rigidbody _parentRb;
    private Vector3 _currentRotation;
    private const float SmoothTime = 20f;
    private Vector3 _currentVelocity = Vector3.zero;
    private Vector3 _localAngle;
    
    private void Awake() {
        _parentRb = GetComponentInParent<Rigidbody>();
        _localAngle = transform.localEulerAngles;
    }
    private void Update() {
        if(_parentRb.velocity == Vector3.zero)
            return;
        Vector3 desiredRotation = Quaternion.LookRotation(_parentRb.velocity).eulerAngles;
        Vector3 targetRotation = new Vector3(0, desiredRotation.y, 0);
        if (targetRotation.y > 180f) targetRotation.y -= 360f;
        else if (targetRotation.y < -180) targetRotation.y += 360f;

        var smoothDesiredRotation = Vector3.SmoothDamp(_localAngle, targetRotation, ref _currentVelocity, SmoothTime*Time.deltaTime);
        _localAngle = smoothDesiredRotation;
        transform.localEulerAngles = smoothDesiredRotation;
    }
}
