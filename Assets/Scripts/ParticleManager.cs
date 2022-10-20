using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    private readonly Dictionary<string, GameObject> _particleEffects = new();
    
    public static ParticleManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GameObject.Destroy(this.gameObject);
            return;
        }
        
        foreach (var obj in Resources.LoadAll<GameObject>("Effects"))
        {
            _particleEffects.Add(obj.name, obj);
        }
    }

    public void PlayParticleEffect(string particleName, Vector3 pos, Quaternion rotation, bool destroyOnEnd = true, float playDelay = 0)
    {
        if(!_particleEffects.TryGetValue(particleName, out var particleObject))
            return;
        
        if(Instantiate(particleObject, pos, rotation).TryGetComponent(out ParticleEffect effect));
        
        if(destroyOnEnd)
            effect.DestroyOnEnd();
        
        effect.Play(playDelay);
    }
}
