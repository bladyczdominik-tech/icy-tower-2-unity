using UnityEngine;

public static class AudioFactory
{
    private static AudioSource source;

    private static AudioSource Source()
    {
        if (source == null)
        {
            GameObject audioObject = new GameObject("ProceduralAudio");
            Object.DontDestroyOnLoad(audioObject);
            source = audioObject.AddComponent<AudioSource>();
        }

        return source;
    }

    public static void PlayStart() => Play(520f, 0.18f, 0.08f);
    public static void PlayGameOver() => Play(150f, 0.35f, 0.12f);
    public static void PlayJump(int character) => Play(character == 0 ? 620f : character == 1 ? 760f : 430f, 0.12f, 0.07f);
    public static void PlayLand(int character) => Play(character == 0 ? 220f : character == 1 ? 300f : 170f, 0.08f, 0.05f);

    private static void Play(float frequency, float seconds, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * seconds);
        AudioClip clip = AudioClip.Create("GeneratedTone", sampleCount, 1, sampleRate, false);
        float[] data = new float[sampleCount];

        for (int i = 0; i < data.Length; i++)
        {
            float time = i / (float)sampleRate;
            float envelope = 1f - i / (float)data.Length;
            data[i] = Mathf.Sin(time * frequency * Mathf.PI * 2f) * volume * envelope;
        }

        clip.SetData(data, 0);
        Source().PlayOneShot(clip);
    }
}
