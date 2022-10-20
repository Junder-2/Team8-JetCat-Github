using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public enum IntensityEnum {
    Off,
    Weak,
    Strong,
    Full
};

public class FlickeringLight : MonoBehaviour {
    private Light _lightToFlicker;
    private const float OffLight = 0;
    private float _weakLight;
    private float _strongLight;
    private float _fullLight;

    private IntensityEnum _intensity;
    public FlickMode[] LightModes;

    private IntensityEnum Intensity {
        get => _intensity;
        set {
            switch (value) {
                case IntensityEnum.Off:
                    if (AllowedTransition(_intensity, value)) _lightToFlicker.intensity = OffLight;
                    break;
                case IntensityEnum.Weak:
                    if (AllowedTransition(_intensity, value)) _lightToFlicker.intensity = _weakLight;
                    break;
                case IntensityEnum.Strong:
                    if (AllowedTransition(_intensity, value)) _lightToFlicker.intensity = _strongLight;
                    break;
                case IntensityEnum.Full:
                    if (AllowedTransition(_intensity, value)) _lightToFlicker.intensity = _fullLight;
                    break;
            }

            _intensity = value;
        }
    }

    private void Awake() {
        _lightToFlicker = GetComponentInChildren<Light>();
        if (!_lightToFlicker) return;

        var intensity = _lightToFlicker.intensity;
        _fullLight = intensity;
        _strongLight = intensity * 0.80f;
        _weakLight = intensity * 0.2f;
        if (LightModes.Length > 0) {
            StartCoroutine(FlickeringLights());
        }
    }

    private IEnumerator FlickeringLights() {
        while (true) {
            var newMode = LightModes[Random.Range(0, LightModes.Length)].Randomize();
            Intensity = newMode.Intensity;
            yield return new WaitForSeconds(newMode.Duration);
        }
    }

    private bool AllowedTransition(IntensityEnum currentIntensityEnum, IntensityEnum targetIntensityEnum) {
        if (currentIntensityEnum == IntensityEnum.Off && targetIntensityEnum == IntensityEnum.Full) {
            Intensity = IntensityEnum.Strong;
            return false;
        }
        return true;
    }
}

[System.Serializable]
public class FlickMode {
    public IntensityEnum Intensity;
    [Range(0.1f, 3f)] public float MinDuration;
    [Range(0.1f, 3f)] public float MaxDuration;
    [HideInInspector] public float Duration;
    public FlickMode Randomize() {
        Duration = Random.Range(MinDuration, MaxDuration);
        return this;
    }
}