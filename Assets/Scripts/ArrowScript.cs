using System;
using System.Collections;
using Car;
using UnityEngine;

public class ArrowScript : MonoBehaviour {
   private Transform _cameraTransform;
   private const float RotationOffset = -90f;
   private Transform _currentCar;
   private bool _animationPlaying;
   private float _mincarYOffset = 3.0f;
   private float _maxcarYOffset = 5.5f;
   private void Awake() {
      _cameraTransform = Camera.main.transform;
   }
   
   public void SetCurrentCar(CarBehaviour car) {
      _currentCar = car.transform;
   }

   private void Update() {
      if (_currentCar == null) {
         return;
      }

      var rotation = transform.rotation;
      rotation = Quaternion.Euler(new Vector3(rotation.x, _cameraTransform.rotation.y + RotationOffset, rotation.z));
      transform.rotation = rotation;
      transform.position = new Vector3(_currentCar.position.x, transform.position.y, _currentCar.position.z);
      if (_currentCar != null && !_animationPlaying) {
         StartCoroutine(AnimateUpArrow());
      }
      else if (_animationPlaying && _currentCar == null) {
         StopAllCoroutines();
         _animationPlaying = false;
      }

   }

   private IEnumerator AnimateUpArrow() {
      _animationPlaying = true;
      WaitForEndOfFrame nextFrame = new WaitForEndOfFrame();
      if (_currentCar == null) {
         _animationPlaying = false;
         yield break;
      }
      var targetYPos = (_currentCar.position.y + _maxcarYOffset);
      while (MathF.Abs(transform.position.y - targetYPos) > 0.2f) { // add break during while loop?
         var nextYValue = Mathf.Lerp(transform.position.y, targetYPos, 0.03f);
         transform.position = new Vector3(transform.position.x, nextYValue, transform.position.z);
         yield return nextFrame;
      }
      transform.position = new Vector3(transform.position.x, targetYPos, transform.position.z);
      yield return AnimateArrowDown();
   }

   private IEnumerator AnimateArrowDown() {
      WaitForEndOfFrame nextFrame = new WaitForEndOfFrame();
      if (_currentCar == null) {
         _animationPlaying = false;
         yield break;
      }
      var targetYPos = (_currentCar.position.y + _mincarYOffset);
      while (MathF.Abs(transform.position.y - targetYPos) > 0.2f) { // add break during while loop?
         var nextYValue = Mathf.Lerp(transform.position.y, targetYPos, 0.03f);
         transform.position = new Vector3(transform.position.x, nextYValue, transform.position.z);
         yield return nextFrame;
      }
      transform.position = new Vector3(transform.position.x, targetYPos, transform.position.z);
      yield return AnimateUpArrow();
   }

}
