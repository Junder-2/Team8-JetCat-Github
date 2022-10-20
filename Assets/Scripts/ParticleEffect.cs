using System.Collections;
using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem particleSystem;

    public void Play(float delay = 0)
    {
        if (delay == 0)
        {
            particleSystem.Play();
            return;
        }

        StartCoroutine(PlayDelay(delay));

    }

    public void Stop()
    {
        particleSystem.Stop();
    }

    public void DestroyOnEnd()
    {
        StartCoroutine(StopAction());
    }

    IEnumerator PlayDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    IEnumerator StopAction()
    {
        var updateTimer = new WaitForSeconds(.2f);

        do
        {
            yield return updateTimer;
        } while (particleSystem.isPlaying);
        
        Destroy(gameObject);
    }
}
