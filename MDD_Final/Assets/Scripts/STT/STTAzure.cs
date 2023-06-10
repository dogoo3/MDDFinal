using System;
using System.IO;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using UnityEngine;

public class STTAzure : MonoBehaviour
{
    public static STTAzure instance; // 인스턴스화
    [SerializeField] private string subscriptionKey; // Azure Speech API 구독 키
    [SerializeField] private string serviceRegion; // Azure Speech API 서비스 리전
    private SpeechConfig _config; // Azure Speech SDK Config
    private SkeletonHandler _skeletonHandler; // 스켈레톤 핸들러 클래스

    private void Awake()
    {
        // 인스턴스화
        instance = this;

        this._skeletonHandler = FindObjectOfType<SkeletonHandler>();
        
        // Azure STT
        if (!string.IsNullOrEmpty(this.subscriptionKey) && !string.IsNullOrEmpty(this.serviceRegion))
        {
            this._config = SpeechConfig.FromSubscription(this.subscriptionKey, this.serviceRegion);
            this._config.SpeechRecognitionLanguage = "en-US"; // 영어로 설정
        }
    }

    public async void SendAudioSample(AudioClip audioClip)
    {
        // 필수값 입력 체크
        if (string.IsNullOrEmpty(this.subscriptionKey) || string.IsNullOrEmpty(this.serviceRegion))
        {
            Debug.LogError("STT 실패 : Azure Speech API 구독 키, 서비스 리전 값 없음");
            return;
        }
        
        Debug.Log("(3/8) STT 시작");
        
        // 오디오 클립을 wav 파일로 저장
        var wavFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".wav";
        SavWav.Save(wavFileName, audioClip);
        var wavFilePath = Path.Combine(Application.persistentDataPath, wavFileName);

        // Azure STT 실행
        using var recognizer = new SpeechRecognizer(this._config, AudioConfig.FromWavFileInput(wavFilePath));
        var result = await recognizer.RecognizeOnceAsync();

        // STT가 성공한 경우
        if (result.Reason == ResultReason.RecognizedSpeech) 
        {
            Debug.Log("(4/8) STT 종료");
            
            // 아바타 실행
            _skeletonHandler.RunSkeleton(result.Text, wavFilePath, audioClip);
        }
        // STT가 성공하지 못한 경우
        else
        {
            Debug.LogError("STT 실패 : " + result.Reason);
        }
    }
}
