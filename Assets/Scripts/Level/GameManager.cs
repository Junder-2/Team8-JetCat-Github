using System.Collections.Generic;
using Car;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

namespace Level {
    public class GameManager : MonoBehaviour, ICameraListener //very temporary
    {
        [SerializeField] private AudioClip _evilCarSound;
        
        private static GameManager _instance;

        public static GameManager Instance {
            get => _instance;
        }

        private ThirdPersonController _playerController;
        private const float ZOffset = 30f;
        private const float XOffset = 30f;
        private void Awake() {
            if (_instance != null) {
                gameObject.SetActive(false);
                return;
            }

            _instance = this;

            if (!PlayerPrefs.HasKey("HighScore"))
            {
                PlayerPrefs.SetInt("HighScore", 0);
                PlayerPrefs.Save();
            }

            _highScore = PlayerPrefs.GetInt("HighScore");

            FindAllLevels();
            _arrow = FindObjectOfType<ArrowScript>();

            if (FindObjectOfType<ThirdPersonController>()) {
                _playerController = FindObjectOfType<ThirdPersonController>();
            }

            //Controller = FindObjectOfType<CameraController>();
            //Controller.AddListener(this);
        }
        
        private List<CarBehaviour> _availableCars;

        private void OnDestroy() {
            if (_instance != this) return;
            _instance = null;
        }

        private LevelManager[] _availableLevels;

        private void FindAllLevels() {
            _availableLevels = FindObjectsOfType<LevelManager>();

            foreach (var level in _availableLevels)
            {
                level.gameObject.SetActive(false);
            }
        }

        private int _currentLevel;
        private LevelManager _currentLevelManager;
        private LevelRules _currentRules;
        
        private CarBehaviour _currentCar;
        private ArrowScript _arrow;

        private CarBehaviour CurrentCar {
            get => _currentCar;
            set {
                if (value != _currentCar) {
                    _arrow.SetCurrentCar(value);
                }

                _currentCar = value;
            }
        }

        private void Start() 
        {
            SetLevel(0);

            InvokeRepeating(nameof(UpdateAvailableCars), 1, 0.2f);
            
            OnUpdateScore?.Invoke(_currentScore, _highScore);
        }

        private void UpdateAvailableCars() {
            var normalCars = FindObjectsOfType<CarBehaviour>().ToList();
            List<CarBehaviour> carsToRemove = new List<CarBehaviour>();
            bool removeCurrentCar = false;
            
            foreach (var car in normalCars) {
                if (car.GetRenderer() == null)
                    continue;
                if (!car.GetRenderer().isVisible) {
                    carsToRemove.Add(car);
                    continue;
                }

                Vector3 carPos = car.transform.position;
                Vector3 playerPos = _playerController.transform.position;

                if (Mathf.Abs(carPos.z - playerPos.z) > ZOffset || Mathf.Abs(carPos.x - playerPos.x) > XOffset) {
                    carsToRemove.Add(car);
                    continue;
                }

                if (!(car is EvilCar))
                {
                    carsToRemove.Add(car);
                    continue;
                }

                if (carPos.y >= playerPos.y + 3 || carPos.y <= playerPos.y -3) {
                    carsToRemove.Add(car);
                    continue;
                }
            }

            foreach (var car in carsToRemove)
            {
                if (_currentCar == car)
                    removeCurrentCar = true;
                normalCars.Remove(car);
            }

            _availableCars = normalCars.OrderByDescending(car => car.GetTimeAlive()).ToList();
            
            if(removeCurrentCar)
                SetSuitableCar();
            
            if(_currentCar == null)
                SetSuitableCar();
        }

        public void SetRules(LevelRules rules) {
            _currentRules = rules;
        }

        public void SetSuitableCar() 
        {
            if (_availableCars.Count == 0 || _availableCars[0] == null) return;
            
            
            CurrentCar = _availableCars[0];
            AudioSource.PlayClipAtPoint(_evilCarSound, CurrentCar.transform.position);
            InputManager.Instance.CurrentCar = _currentCar;
        }

        public void EndLevel() {
            // change to camerapan next milestone.
            //Controller.PanCamera();
            GameOver();
        }

        void NextLevel() //this is currently not in use we only have one level 
        {
            _currentLevelManager.gameObject.SetActive(false);

            _currentLevel = (_currentLevel + 1) % _availableLevels.Length;

            SetLevel(_currentLevel);
        }

        void SetLevel(int index) { //same as above we dont have multiple levels
            _currentLevelManager = _availableLevels.FirstOrDefault(level => level.GetLevelIndex() == index);
            _currentLevel = index;
            if (_currentLevelManager == null) return;
            _currentRules = _currentLevelManager.GetRules();

            _currentLevelManager.gameObject.SetActive(true);
        }

        public UnityAction<int, int> OnUpdateScore;
        private float _lastCheckPointTimeStamp = 0;
        private int _highScore;
        private int _currentScore;
        private int _catRespawns;

        public void AddPlayerRespawn() => _catRespawns++;

        public void AddCheckpointFinishScore(int baseScore = 1000)
        {
            float addedScore = baseScore - (Time.time - _lastCheckPointTimeStamp) * 2f - _catRespawns * 100f;
            _currentScore += (int)Mathf.Max(0, addedScore);

            _catRespawns = 0;
            _lastCheckPointTimeStamp = Time.time;

            if (_currentScore > _highScore)
                _highScore = _currentScore;
            
            OnUpdateScore?.Invoke(_currentScore, _highScore);
        }

        public void AddScore(int points)
        {
            _currentScore += points;
            
            if (_currentScore > _highScore)
                _highScore = _currentScore;
            
            OnUpdateScore?.Invoke(_currentScore, _highScore);
        }
        
        public void UpdateLevel() {
            NextLevel();
        }

        [SerializeField] private UnityEvent OnGameOver;

        public void GameOver() {
            // timer och gameoverscene
            OnGameOver?.Invoke();
            
            PlayerPrefs.SetInt("HighScore", _highScore);
            UIManager.Instance.LoadScene("WinnerScene");
        }
        
        public UnityEvent OnGameOver1 => OnGameOver;

        public CameraController Controller { get; set; }
    }
}