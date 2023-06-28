using System;
using UnityEngine;

public class TTSRunner : MonoBehaviour
{
    private PlayDirector _playDirector; // PlayDirector 클래스
    private SkeletonHandler _skeletonHandler; // 스켈레톤 클래스

    private void Awake()
    {
        this._playDirector = FindObjectOfType<PlayDirector>();
        this._skeletonHandler = FindObjectOfType<SkeletonHandler>();
    }

    /**
     * TTS 실행.
     */
    public void RunTts(string outputText)
    {
        Debug.Log("TTS 시작");
        
        try
        {
            
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            this._playDirector.SetPlaying(false);
            return;
        }

        Debug.Log("TTS 끝");
            
        // 아바타 실행
        this._skeletonHandler.RunSkeleton(outputText);
    }
}
