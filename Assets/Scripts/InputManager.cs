using Car;
using Level;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour {
    private InputScheme _menuHandling;
    private InputScheme _playerControls;
    private InputScheme _carControls;
    private ThirdPersonController _characterController;
    private CarBehaviour _currentCarController;
    private bool _rightButtonPressed;
    private bool _leftButtonPressed;
    private Vector2 _playerCharacterInput = Vector2.zero;

    private Vector2 PlayerCharacterInput {
        get => _playerCharacterInput;
        set {
            _playerCharacterInput = value;

            _characterController.SetMoveVector(_playerCharacterInput);
        }
    }

    private float _rButtonPressTime;
    private bool RightButtonPressed {
        set {
            if (_leftButtonPressed && value && (Time.time - _lButtonPressTime) < .25f) {
                CharacterController.CharacterJump(true);
            }

            _rButtonPressTime = Time.time;
            _rightButtonPressed = value;
        }
    }
    
    private float _lButtonPressTime;
    private bool LeftButtonPressed {
        set {
            if (_rightButtonPressed && value && (Time.time - _rButtonPressTime) < .25f) {
                CharacterController.CharacterJump(true);
            }

            _lButtonPressTime = Time.time;
            _leftButtonPressed = value;
        }
    }

    private static InputManager _instance;
    public static InputManager Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<InputManager>();
                if (_instance == null) {
                    GenerateSingleton();
                }
            }

            return _instance;
        }
    }

    private static void GenerateSingleton() {
        // remove singleton?
        GameObject inputManagerObject = new GameObject("InputManager");
        DontDestroyOnLoad(inputManagerObject);
        _instance = inputManagerObject.AddComponent<InputManager>();
    }

    private bool _carControllerCheck = false;

    public CarBehaviour CurrentCar {
        get => _currentCarController;
        set {
            var carControl = _carControls.CarControl;

            if (_carControllerCheck && value != _currentCarController) 
            {
                _currentCarController.MoveOverride(0);
                carControl.Move.performed -= val => _currentCarController.MoveOverride(val.ReadValue<float>());
                carControl.Move.canceled -= _ => _currentCarController.MoveOverride(0);
                carControl.Speed.performed -= val => _currentCarController.ShiftGear(val.ReadValue<float>());
            }

            _carControllerCheck = true;

            _currentCarController = value;

            carControl.Move.performed += val => _currentCarController.MoveOverride(val.ReadValue<float>());
            carControl.Move.canceled += _ => _currentCarController.MoveOverride(0);
            carControl.Speed.performed += val => _currentCarController.ShiftGear(val.ReadValue<float>());
        }
    }

    private GameManager _gameManager;
    private bool _gameManagerCheck;
    private GameManager LocalGameManager {
        get {
            if (!_gameManagerCheck)
                _gameManager = GameManager.Instance;

            _gameManagerCheck = true;

            return _gameManager;
        }
    }

    private UIManager _uiManager;
    private bool _uiManagerCheck;

    private UIManager LocalUIManager
    {
        get
        {
            if(!_uiManagerCheck)
                _uiManager = UIManager.Instance;

            _uiManagerCheck = true;

            return _uiManager;
        }
    }

    private bool _characterControllerCheck = false;

    private ThirdPersonController CharacterController {
        get => _characterController;

        set {
            _characterControllerCheck = true;

            _characterController = value;
        }
    }

    private void RightPlayerButtonPressed(bool pressed) {
        RightButtonPressed = pressed;
        Vector2 dir = pressed
            ? (!_leftButtonPressed ? Vector2.right : Vector2.zero)
            : (!_leftButtonPressed ? Vector2.zero : Vector2.left);
        PlayerCharacterInput = dir;
    }

    private void LeftPlayerButtonPressed(bool pressed) {
        LeftButtonPressed = pressed;
        
        Vector2 dir = pressed
            ? (!_rightButtonPressed ? Vector2.left : Vector2.zero)
            : (!_rightButtonPressed ? Vector2.zero : Vector2.right);
        PlayerCharacterInput = dir;
    }

    private void PlayerSpeedControl(float value) {
        
        CharacterController.ChangeMovementSpeed(value > 0);
    }

    private void PauseInput() {
        LocalUIManager.PauseGame();
    }

    private void ResetInput() {
        //reload scene here

        LocalUIManager.LoadScene("MainMenu");
    }

    private void Awake() {
        if (GameObject.Find("Player") != null) {
            CharacterController = GameObject.Find("Player").GetComponent<ThirdPersonController>();
        }
    }

    private void OnEnable() {
        InputSystem.FlushDisconnectedDevices();

        int gamepadCount = Gamepad.all.Count;
        

        _menuHandling ??= new InputScheme();

        _playerControls ??= new InputScheme();
        _playerControls.devices = new InputDevice[] { gamepadCount > 0 ? Gamepad.all[0] : Keyboard.current };

        _carControls ??= new InputScheme();
        _carControls.devices = new InputDevice[]
            { gamepadCount > 1 ? Gamepad.all[gamepadCount - 1] : Keyboard.current };
        //_carControls.devices = new InputDevice[] { Gamepad.all[0] }; // for testing purposes


        var navigation = _menuHandling.Navigation;

        navigation.Pause.performed += _ => PauseInput();
        navigation.Reset.performed += _ => ResetInput();

        if (_characterControllerCheck) {
            var playerMovement = _playerControls.PlayerMovement;

            playerMovement.RightButton.performed += _ => RightPlayerButtonPressed(true);
            playerMovement.RightButton.canceled += _ => RightPlayerButtonPressed(false);

            playerMovement.LeftButton.performed += _ => LeftPlayerButtonPressed(true);
            playerMovement.LeftButton.canceled += _ => LeftPlayerButtonPressed(false);

            playerMovement.Speed.performed += val => PlayerSpeedControl(val.ReadValue<float>());
        }

        _carControls.CarControl.NextCar.performed += _ => LocalGameManager.SetSuitableCar();

        _playerControls.PlayerMovement.Enable();
        _carControls.CarControl.Enable();

        _menuHandling.Navigation.Enable();
        
        SceneManager.activeSceneChanged += SceneChanged;
    }

    private void OnDisable() {
        _playerControls.PlayerMovement.Disable();
        _carControls.CarControl.Disable();
        _menuHandling.Navigation.Disable();
    }

    void SceneChanged(Scene arg0, Scene scene)
    {
        _uiManagerCheck = false;
        _gameManagerCheck = false;
        _carControllerCheck = false;
        _characterControllerCheck = false;
    }
}