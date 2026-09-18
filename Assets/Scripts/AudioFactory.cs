using UnityEngine;

public static class AudioFactory
{
    private static AudioSource source;
    private static AudioSource Source(){if(source==null){var go=new GameObject("ProceduralAudio");Object.DontDestroyOnLoad(go);source=go.AddComponent<AudioSource>();}return source;}
    public static void PlayStart(){Play(520,.18f,.08f);}
    public static void PlayGameOver(){Play(150,.35f,.12f);}
    public static void PlayJump(int character){Play( character==0?620:character==1?760:430,.12f,.07f);}
    public static void PlayLand(int character){Play(character==0?220:character==1?300:170,.08f,.05f);}
    private static void Play(float hz,float seconds,float volume){var clip=AudioClip.Create("GeneratedTone",Mathf.CeilToInt(44100*seconds),1,44100,false);var data=new float[clip.samples];for(int i=0;i<data.Length;i++){float t=i/44100f;data[i]=Mathf.Sin(t*hz*Mathf.PI*2)*volume*(1-i/(float)data.Length);}clip.SetData(data,0);Source().PlayOneShot(clip);}
}
