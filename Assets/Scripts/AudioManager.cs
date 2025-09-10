using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip flipClip;
    public AudioClip matchClip;        // base match sfx
    public AudioClip bigMatchClip;     // optional stronger sfx for higher combos
    public AudioClip missClip;
    public AudioClip gameOverClip;

    [Header("Mixer")]
    public AudioSource sfx;            // assign in Inspector

    void Play(AudioClip clip, float vol = 1f, float pitch = 1f)
    {
        if (!clip || !sfx) return;
        sfx.pitch = pitch;
        sfx.PlayOneShot(clip, vol);
        sfx.pitch = 1f;
    }

    public void Flip() => Play(flipClip, 0.9f);

    // Called with combo (preferred)
    public void Match(int combo)
    {
        if (combo >= 3 && bigMatchClip)
            Play(bigMatchClip, 1f, 1f + Mathf.Min(0.15f, 0.03f * (combo - 3)));
        else
            Play(matchClip, 1f, 1f + Mathf.Min(0.1f, 0.02f * Mathf.Max(0, combo - 1)));
    }

    // Overload so GameControllerUI can just call Match()
    public void Match() => Play(matchClip, 1f);

    public void Miss() => Play(missClip, 1f);
    public void GameOver() => Play(gameOverClip, 1f);
}
