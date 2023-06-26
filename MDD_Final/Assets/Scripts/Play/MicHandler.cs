using System;
using UnityEngine;

public class MicHandler : MonoBehaviour
{
    // 입력
    private string _deviceName; // 마이크 디바이스 이름
    private AudioClip _mic; // 마이크 AudioClip
    private int _micChannelCount; // 마이크 채널 수
    private const int SampleRate = 44100; // 초당 샘플링 

    // 읽기
    private bool _isReading = true; // 읽기 진행중 여부
    private bool _isRunningDialogue = false; // 발화 진행중 여부
    private const int LoudnessCheckPosSize = 64; // Loudness 체크 대상 샘플링 Position 크기
    private const float DialogueLoudness = 5.0f; // 발화인 Loudness 크기 기준
    private const float SilenceLoudness = 3.0f; // 발화가 아닌 Loudness 크기 기준
    private const int SilenceCountDest = 200; // 발화 종료로 인식할 연속 Loudness 횟수 기준
    private int _slienceCount = 0; // 연속 Loudness 횟수 카운트
    private int _readCompleteSamplingPos = 0; // 읽기 완료된 샘플링 위치
    private float[] _samplingDatas; // 샘플링 데이터
    private float[] _collectedSamplingDatas; // 누적 샘플링 데이터
    
    // STT
    private GameDirector _gameDirector; // GameDirector 클래스
    private STTAzure _sttAzure; // STT 클래스
    
    private void Awake()
    {
        // 입력
        this._deviceName = Microphone.devices[0]; // 첫 번째 마이크 사용
        this._mic = Microphone.Start(this._deviceName, true, 1, SampleRate);
        this._micChannelCount = this._mic.channels;
        
        // 읽기
        this._collectedSamplingDatas = Array.Empty<float>();

        // STT
        this._gameDirector = FindObjectOfType<GameDirector>();
        this._sttAzure = FindObjectOfType<STTAzure>();
    }

    private void Update()
    {
        // 아바타 재생이 시작된 경우 리턴
        if (this._gameDirector.GetPlaying()) return;
        
        // 읽기 진행중인 경우
        if (this._isReading)
        {
            // 매 프레임마다 마이크 입력 읽기
            ReadMicInput();
        }
        // 읽기 진행중이 아닌 경우
        else
        {
            // 누적 샘플링 데이터에 값이 없는 경우 리턴
            if (this._collectedSamplingDatas.Length <= 0)
            {
                this._isReading = true;
                return;
            }
            
            // 아바타 재생 프로세스 시작
            this._gameDirector.SetPlaying(true);
            
            // STT로 전송
            this.SendToStt();
        }
    }

    /**
     * 마이크 입력 읽기.
     */
    private void ReadMicInput()
    {
        // 현재 샘플링 위치
        var currentSamplingPos = Microphone.GetPosition(this._deviceName);

        // Loudness 취득
        var loudness = this.GetLoudness(this._mic, currentSamplingPos, LoudnessCheckPosSize);
        
        // 사용자 발화 진행중이 아닌 경우 + loudness가 기준 이상인 경우
        if (!this._isRunningDialogue && loudness >= DialogueLoudness)
        {
            // 사용자 발화 시작으로 인식
            Debug.Log("사용자 발화 시작");
            this._isRunningDialogue = true;
        }
        // 사용자 발화 진행중인 경우 + SilenceCount를 기준까지 채운 경우
        else if (this._isRunningDialogue && this._slienceCount >= SilenceCountDest)
        {
            // 사용자 발화 끝으로 인식
            Debug.Log("사용자 발화 끝");
            this._isRunningDialogue = false;
            this._isReading = false;
            this._slienceCount = 0;
        }
        // 사용자 발화 진행중인 경우
        else if (this._isRunningDialogue)
        {
            // 현재 샘플링 위치가 읽기 완료된 샘플링 위치 이후인 경우
            var diff = currentSamplingPos - this._readCompleteSamplingPos;
            if (diff > 0)
            {
                // 샘플링 데이터 배열 크기 설정
                this._samplingDatas = new float[diff * this._micChannelCount];
                // 샘플링 데이터에 읽기 완료된 샘플링 위치 이후의 데이터 전부 삽입
                this._mic.GetData(this._samplingDatas, this._readCompleteSamplingPos);
            
                /* 샘플링 데이터 누적 시작 */
                var oldLeng = this._collectedSamplingDatas.Length;
                var addLeng = this._samplingDatas.Length;
                var newLeng = oldLeng + addLeng;
            
                // 기존에 누적된 샘플링 데이터가 있는 경우
                if (oldLeng > 0)
                {
                    // 기존에 누적된 샘플링 데이터를 oldSampleDatas에 임시 복사
                    var oldSampleDatas = new float[oldLeng];
                    this._collectedSamplingDatas.CopyTo(oldSampleDatas, 0);
            
                    // _collectedSamplingDatas를 새로 할당
                    this._collectedSamplingDatas = new float[newLeng];
                
                    // oldSampleDatas의 샘플링 데이터 삽입
                    for (var i = 0; i < oldLeng; i++) this._collectedSamplingDatas[i] = oldSampleDatas[i];
                }
                // 기존에 누적된 샘플링 데이터가 없는 경우
                else
                {
                    // _collectedSamplingDatas를 새로 할당
                    this._collectedSamplingDatas = new float[newLeng];
                }

                // GetData로 얻은 샘플링 데이터 삽입
                for (var j = oldLeng; j < newLeng; j++) this._collectedSamplingDatas[j] = this._samplingDatas[j - oldLeng];
                /* 샘플링 데이터 누적 끝 */
            }
        }

        // 현재 샘플링 위치를 읽기 완료된 샘플링 위치로 저장
        this._readCompleteSamplingPos = currentSamplingPos;
        
        // Silence 체크
        if (loudness <= SilenceLoudness) ++this._slienceCount;
        else this._slienceCount = 0;
    }

    /**
     * Loudness 취득.
     */
    private float GetLoudness(AudioClip audioClip, int lastPos, int targetPosSize)
    {
        // 대상 샘플링 위치
        var startPos = lastPos - targetPosSize;
        if (startPos < 0) return 0;

        // 대상 샘플링 데이터
        var loudnessCheckSamplingDatas = new float[targetPosSize];
        audioClip.GetData(loudnessCheckSamplingDatas, startPos);

        // 대상 샘플링 데이터 크기 평균치에 100배 증폭
        float sumloudness = 0;
        for (var i = 0; i < targetPosSize; i++) sumloudness += Mathf.Abs(loudnessCheckSamplingDatas[i]);
        var avrgloudness = sumloudness / targetPosSize * 100;
        
        // 0.1을 안넘는 경우 0으로 취급
        if (avrgloudness < 0.1f) avrgloudness = 0;

        return avrgloudness;
    }
    
    /**
     * STT로 전송.
     */
    private void SendToStt()
    {
        // 누적 샘플링 데이터를 AudioClip으로 생성
        var inputAudioClip = AudioClip.Create("UserSpeech", this._collectedSamplingDatas.Length,
            this._micChannelCount, SampleRate, false);
        inputAudioClip.SetData(this._collectedSamplingDatas, 0);
        
        // STT 실행
        this._sttAzure.RunStt(inputAudioClip);
        
        // 누적 샘플링 데이터 초기화
        this._collectedSamplingDatas = Array.Empty<float>();

        // 읽기 진행중으로 전환
        this._isReading = true;
    }
}
