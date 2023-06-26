using System;
using System.IO;
using UnityEngine;
using Python.Runtime;

public class TTSRunner : MonoBehaviour
{
    private GameDirector _gameDirector; // GameDirector 클래스
    private SkeletonHandler _skeletonHandler; // 스켈레톤 클래스

    private void Awake()
    {
        this._gameDirector = FindObjectOfType<GameDirector>();
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
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                dynamic tts = Py.Import("tts");
                var savePath = Path.Combine(Application.persistentDataPath, "tts.wav");
                tts.main(outputText, savePath);
            }
            PythonEngine.Shutdown();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            this._gameDirector.SetPlaying(false);
            return;
        }

        Debug.Log("TTS 끝");
            
        // 아바타 실행
        this._skeletonHandler.RunSkeleton(outputText);
    }
}
