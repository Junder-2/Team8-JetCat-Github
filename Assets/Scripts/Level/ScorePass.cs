using UnityEngine;

namespace Level
{
    [RequireComponent(typeof(BoxCollider))]
    public class ScorePass : MonoBehaviour
    {
        [SerializeField] private int baseScore = 1000;
        
        private bool _passed;
    
        private void OnTriggerEnter(Collider other)
        {
            if(_passed) return;
        
            var player = other.gameObject.GetComponent<ThirdPersonController>();
            if(player == null) return;

            _passed = true;
            GameManager.Instance.AddCheckpointFinishScore(baseScore);
        }
    }
}
