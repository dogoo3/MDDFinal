using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.CognitiveServices.Speech;
using UnityEngine;

public class TTSAzure : MonoBehaviour
{
    [SerializeField] private string subscriptionKey; // Azure Speech API 구독 키
    [SerializeField] private string serviceRegion; // Azure Speech API 서비스 리전
    private SpeechConfig _config; // Azure Speech SDK Config
    private GameDirector _gameDirector; // GameDirector 클래스
    private SkeletonHandler _skeletonHandler; // 스켈레톤 클래스

    private void Awake()
    {
        this._gameDirector = FindObjectOfType<GameDirector>();
        this._skeletonHandler = FindObjectOfType<SkeletonHandler>();
        
        // Azure TTS
        if (!string.IsNullOrEmpty(this.subscriptionKey) && !string.IsNullOrEmpty(this.serviceRegion))
        {
            this._config = SpeechConfig.FromSubscription(this.subscriptionKey, this.serviceRegion);
            this._config.SpeechSynthesisVoiceName = "en-US-AriaNeural"; // 영어로 설정
        }
    }

    /**
     * TTS 실행.
     */
    public async void RunTts(string outputText)
    {
        Debug.Log("TTS 시작");
        
        // 필수값 입력 체크
        if (string.IsNullOrEmpty(this.subscriptionKey) || string.IsNullOrEmpty(this.serviceRegion))
        {
            Debug.LogError("TTS 실패 : Azure Speech API 구독 키, 서비스 리전 값 없음");
            this._gameDirector.SetPlaying(false);
            return;
        }

        using var synthesizer = new SpeechSynthesizer(this._config, null);
        var result = await synthesizer.SpeakTextAsync(outputText);

        // TTS가 성공한 경우
        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        {
            // TTS 결과를 wav 파일로 저장
            var outputWavFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".wav";
            var outputWavFilePath = Path.Combine(Application.persistentDataPath, outputWavFileName);
            using (var audioDataStream = AudioDataStream.FromResult(result))
            {
                await audioDataStream.SaveToWaveFileAsync(outputWavFilePath);
            }
            
            Debug.Log("TTS 끝");
        
            // 아바타 실행
            this._skeletonHandler.RunSkeleton(outputText, outputWavFilePath);
        }
        else
        {
            Debug.LogError("TTS 실패 : " + result.Reason);
            this._gameDirector.SetPlaying(false);
        }
    }
}
