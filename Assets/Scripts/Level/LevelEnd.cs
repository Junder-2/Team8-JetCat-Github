using UnityEngine;
using UnityEngine.SceneManagement;

namespace Level
{
    [RequireComponent(typeof(BoxCollider))]
    public class LevelEnd : MonoBehaviour
    {
        [SerializeField] private WinnerSO winner;
        private void OnTriggerEnter(Collider other)
        {
            winner.winner = 1;
            var player = other.gameObject.GetComponent<ThirdPersonController>();
            if(player == null) return;
        
            GameManager.Instance.EndLevel();
        }
    }
}
