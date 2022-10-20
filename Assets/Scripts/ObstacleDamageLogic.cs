using Car;
using System;
using UnityEngine;


[RequireComponent(typeof(Collider))]
public class ObstacleDamageLogic : MonoBehaviour, IDamager
{
   private void OnCollisionEnter(Collision collision) {
      if (collision.gameObject.TryGetComponent(out CarBehaviour carDamageLogic)) {
         carDamageLogic.TakeDamage(Damage);
      }

      if (collision.gameObject.TryGetComponent(out ThirdPersonController playerController)) {
         playerController.TakeDamage(1);
      }
   }

   private void OnTriggerEnter(Collider other) {
      
      
      if (other.gameObject.TryGetComponent(out CarBehaviour carDamageLogic)) {
         carDamageLogic.TakeDamage(Damage);
      }

      if (other.gameObject.TryGetComponent(out ThirdPersonController playerController)) {
         if (gameObject.name == "DeathZone") {
            playerController.TakeDamage(100);
            return;
         }
         playerController.TakeDamage(1);
      }
   }

   public int Damage { get; set; } = 100;
}
