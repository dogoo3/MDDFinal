using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Python.Runtime;

public class STTRunner : MonoBehaviour
{
    private GameDirector _gameDirector; // GameDirector 클래스
    private GptRunner _gptRunner; // GPT 클래스
    private TTSRunner _ttsRunner; // TTS 클래스
    private string _inputText; // STT 결과

    private void Awake()
    {
        this._gameDirector = FindObjectOfType<GameDirector>();
        this._gptRunner = FindObjectOfType<GptRunner>();
        this._ttsRunner = FindObjectOfType<TTSRunner>();
    }

    /**
     * STT 실행.
     */
    public void RunStt(AudioClip inputAudioClip)
    {
        // 오디오 클립을 wav 파일로 저장
        var inputWavFileName = "stt.wav";
        SavWav.Save(inputWavFileName, inputAudioClip);
        var inputWavFilePath = Path.Combine(Application.persistentDataPath, inputWavFileName);

        try
        {
            PythonEngine.Initialize();
            using (Py.GIL())
            {
                dynamic stt = Py.Import("stt");
                dynamic inputText = stt.main(inputWavFilePath);
                this._inputText = (string) inputText;
            }
            PythonEngine.Shutdown();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            this._gameDirector.SetPlaying(false);
            return;
        }
        
        Debug.Log("STT 끝");
        
        Debug.Log("STT Result : " + this._inputText);
            
        // GPT 실행
        var outputText = this._gptRunner.RunGpt(this._inputText);
            
        // TTS 실행
        if (outputText != null) this._ttsRunner.RunTts(outputText);
        else this._gameDirector.SetPlaying(false);
    }
}
