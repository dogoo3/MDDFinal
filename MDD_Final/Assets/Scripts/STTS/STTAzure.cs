using System.IO;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using UnityEngine;

public class STTAzure : MonoBehaviour
{
    [SerializeField] private string subscriptionKey; // Azure Speech API 구독 키
    [SerializeField] private string serviceRegion; // Azure Speech API 서비스 리전
    private SpeechConfig _config; // Azure Speech SDK Config
    private GameDirector _gameDirector; // GameDirector 클래스
    private GptRunner _gptRunner; // GPT 클래스
    private TTSRunner _ttsRunner; // TTS 클래스

    private void Awake()
    {
        this._gameDirector = FindObjectOfType<GameDirector>();
        this._gptRunner = FindObjectOfType<GptRunner>();
        this._ttsRunner = FindObjectOfType<TTSRunner>();
        
        // Azure STT
        if (!string.IsNullOrEmpty(this.subscriptionKey) && !string.IsNullOrEmpty(this.serviceRegion))
        {
            this._config = SpeechConfig.FromSubscription(this.subscriptionKey, this.serviceRegion);
            this._config.SpeechRecognitionLanguage = "en-US"; // 영어로 설정
        }
    }

    /**
     * STT 실행.
     */
    public async void RunStt(AudioClip inputAudioClip)
    {
        Debug.Log("STT 시작");
        
        // 필수값 입력 체크
        if (string.IsNullOrEmpty(this.subscriptionKey) || string.IsNullOrEmpty(this.serviceRegion))
        {
            Debug.LogError("STT 실패 : Azure Speech API 구독 키, 서비스 리전 값 없음");
            this._gameDirector.SetPlaying(false);
            return;
        }
        
        // 오디오 클립을 wav 파일로 저장
        var inputWavFileName = "stt.wav";
        SavWav.Save(inputWavFileName, inputAudioClip);
        var inputWavFilePath = Path.Combine(Application.persistentDataPath, inputWavFileName);

        // Azure STT 실행
        using var recognizer = new SpeechRecognizer(this._config, AudioConfig.FromWavFileInput(inputWavFilePath));
        var result = await recognizer.RecognizeOnceAsync();

        // STT가 성공한 경우
        if (result.Reason == ResultReason.RecognizedSpeech) 
        {
            Debug.Log("STT 끝");
            
            // GPT 실행
            var outputText = this._gptRunner.RunGpt(result.Text);
            
            // TTS 실행
            if (outputText != null) this._ttsRunner.RunTts(outputText);
            else this._gameDirector.SetPlaying(false);
        }
        // STT가 성공하지 못한 경우
        else
        {
            Debug.LogError("STT 실패 : " + result.Reason);
            this._gameDirector.SetPlaying(false);
        }
    }
}
