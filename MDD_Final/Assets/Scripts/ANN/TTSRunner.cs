using System;
using System.IO;
using UnityEngine;
using Python.Runtime;

public class TTSRunner : MonoBehaviour
{
    private PlayDirector _playDirector; // PlayDirector 클래스
    private PythonnetSetter _pythonnetSetter; // 파이썬넷 세팅 클래스
    private SkeletonHandler _skeletonHandler; // 스켈레톤 클래스

    private void Awake()
    {
        this._playDirector = FindObjectOfType<PlayDirector>();
        this._pythonnetSetter = FindObjectOfType<PythonnetSetter>();
        this._skeletonHandler = FindObjectOfType<SkeletonHandler>();
    }

    /**
     * TTS 실행.
     */
    public void RunTts(string outputText)
    {
        Debug.Log("TTS 시작");

        // 파이썬넷 환경 세팅
        this._pythonnetSetter.SetPyEnvForStts();

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
            this._playDirector.SetPlaying(false);
            return;
        }

        Debug.Log("TTS 끝");
            
        // 아바타 실행
        this._skeletonHandler.RunSkeleton(outputText);
    }
}
