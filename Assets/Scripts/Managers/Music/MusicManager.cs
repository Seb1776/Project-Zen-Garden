using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [SerializeField] private WorldMusic[] assets;
    [SerializeField] private GameWorlds currentWorld;
    [SerializeField] private AudioClip ageTransitionEffect;

    private Coroutine playLoopCoroutine;
    private Coroutine swapCoroutine;
    private AudioSource source;
    private MusicAsset currentAsset;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void Start()
    {
        SetWorldsPrices();
        SelectMusicAsset();
    }

    void SetWorldsPrices()
    {
        /*foreach (WorldMusic wm in assets)
        {   
            if (wm.asset != null)
            {
                wm.worldButton.interactable = false;
                wm.worldPrice.text = "$ " + wm.asset.unlockPrice.ToString("0,0");
            }
        }*/
    }

    public void SelectMusicAsset()
    {
        foreach (WorldMusic wm in assets)
        {
            if (wm.world == currentWorld)
            {
                currentAsset = wm.asset;
                PlayMusic(wm.asset);
                break;
            }
        }
    }

    public void UnlockWorld(MusicAsset world)
    {
        foreach (WorldMusic wm in assets)
        {
            if (wm.asset == world)
            {
                wm.unlockButton.SetActive(false);
                wm.worldButton.interactable = true;
            }
        }
    }

    public MusicAsset GetCurrentMusic()
    {
        return currentAsset;
    }

    public void ChangeMusicAge(MusicAsset worldMusic)
    {
        if (swapCoroutine != null)
        {
            StopCoroutine(swapCoroutine);
            swapCoroutine = StartCoroutine(SwapMusicAge(worldMusic));
        }

        else
            swapCoroutine = StartCoroutine(SwapMusicAge(worldMusic));
    }

    public IEnumerator SwapMusicAge(MusicAsset worldMusic)
    {
        source.Stop();
        source.clip = null;
        source.PlayOneShot(ageTransitionEffect);

        yield return new WaitForSeconds(ageTransitionEffect.length - 1f);

        PlayMusic(worldMusic);
    }

    void PlayMusic(MusicAsset ma)
    {
        int randomTrackIdx = Random.Range(0, ma.worldTracks.Length);
        currentAsset = ma;

        if (ma.worldTracks[randomTrackIdx].startTrack != null)
        {
            source.loop = false;
            source.clip = ma.worldTracks[randomTrackIdx].startTrack;
            source.Play();

            if (playLoopCoroutine != null)
            {
                StopCoroutine(playLoopCoroutine);
                playLoopCoroutine = StartCoroutine(StartTrack(ma.worldTracks[randomTrackIdx].musicTrack));
            }

            else
                playLoopCoroutine = StartCoroutine(StartTrack(ma.worldTracks[randomTrackIdx].musicTrack));
        }

        else
        {
            source.loop = true;
            source.clip = ma.worldTracks[randomTrackIdx].musicTrack;
            source.Play();
        }
    }

    IEnumerator StartTrack(AudioClip loopTrack)
    {
        while (source.isPlaying)
            yield return null;

        source.loop = true;
        source.clip = loopTrack;
        source.Play();
    }
}

[System.Serializable]
public class WorldMusic
{
    public GameWorlds world;
    public MusicAsset asset;
    public Button worldButton;
    public Text worldPrice;
    public GameObject unlockButton;
    public bool hasTravelledToThisWorld;
}
