using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    private Material _fadeMat;
    private static readonly int FadeProperty = Shader.PropertyToID("_Fade");
    private static readonly int AlphaProperty = Shader.PropertyToID("_Alpha");

    private void Awake()
    {
        _fadeMat = GetComponent<Image>().material;
    }

    public void SetFade(float fade) => _fadeMat.SetFloat(FadeProperty, fade);

    public void SetAlpha(float alpha) => _fadeMat.SetFloat(AlphaProperty, alpha);

    public float GetFade() => _fadeMat.GetFloat(FadeProperty);
}
