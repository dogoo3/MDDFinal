using System;
using System.Collections;
using System.Linq;
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
    private float[] _loudnessDatas; // Loudness 저장 배열
    private const float SilenceCheckInterval = 0.1f; // Silence 체크 간격(초)
    private const float SilenceCheckDestTime = 3f; // Silence 체크 목표 시간(초)
    private float _silenceCheckedTime = 0f; // Silence 체크된 시간(초) 
    private const float DialogueStartLoudness = 4.0f; // 사용자 발화 시작으로 볼 Loudness 크기
    private const float DialogueEndLoudness = 0.18f; // 사용자 발화 끝으로 볼 Loudness 크기
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
        // Debug.Log("deviceName : " + this._deviceName);
        this._mic = Microphone.Start(this._deviceName, true, 1, SampleRate);
        this._micChannelCount = this._mic.channels;
        
        // 읽기
        this._collectedSamplingDatas = Array.Empty<float>();
        this._loudnessDatas = Array.Empty<float>();

        // STT
        this._gameDirector = FindObjectOfType<GameDirector>();
        this._sttAzure = FindObjectOfType<STTAzure>();
    }

    private void Start()
    {
        this.StartSilenceCheckRoutine();
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
        // Debug.Log("currentSamplingPos : " + currentSamplingPos);
        // Debug.Log("currentSamplingPos : " + Math.Round(currentSamplingPos / (double) SampleRate, 1) + "s");

        // Loudness 취득
        var loudness = this.GetLoudness(this._mic, currentSamplingPos, LoudnessCheckPosSize);
        // Debug.Log(loudness);
        
        // 사용자 발화 진행중이 아닌 경우 + loudness가 기준을 넘는 경우
        if (!this._isRunningDialogue && loudness > DialogueStartLoudness)
        {
            // 사용자 발화 시작으로 인식
            Debug.Log("사용자 발화 시작");
            this._isRunningDialogue = true;
        }
        // 사용자 발화 진행중인 경우
        else if (this._isRunningDialogue)
        {
            this.AddToLoudnessDatas(loudness);
            
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
                    for (var i = 0; i < oldLeng; i++)
                    {
                        this._collectedSamplingDatas[i] = oldSampleDatas[i];
                    }
                }
                // 기존에 누적된 샘플링 데이터가 없는 경우
                else
                {
                    // _collectedSamplingDatas를 새로 할당
                    this._collectedSamplingDatas = new float[newLeng];
                }

                // GetData로 얻은 샘플링 데이터 삽입
                for (var j = oldLeng; j < newLeng; j++)
                {
                    this._collectedSamplingDatas[j] = this._samplingDatas[j - oldLeng];
                }
                /* 샘플링 데이터 누적 끝 */
            }
        }

        // 현재 샘플링 위치를 읽기 완료된 샘플링 위치로 저장
        this._readCompleteSamplingPos = currentSamplingPos;
    }

    /**
     * Loudness 취득.
     */
    private float GetLoudness(AudioClip audioClip, int lastPos, int targetPosSize)
    {
        // 체크 대상 샘플링 위치
        var startPos = lastPos - targetPosSize;
        if (startPos < 0)
        {
            return 0;
        }

        // 체크 대상 샘플링 데이터
        var loudnessCheckSamplingDatas = new float[targetPosSize];
        audioClip.GetData(loudnessCheckSamplingDatas, startPos);

        // 체크 대상 샘플링 데이터 크기 평균치에 100배 증폭
        float sumloudness = 0;
        for (var i = 0; i < targetPosSize; i++)
        {
            sumloudness += Mathf.Abs(loudnessCheckSamplingDatas[i]);
        }
        var avrgloudness = sumloudness / targetPosSize * 100;
        
        // 0.1을 안넘는 경우 0으로 취급
        if (avrgloudness < 0.1f)
        {
            avrgloudness = 0;
        }

        return avrgloudness;
    }
    
    /**
     * Loudness 데이터 저장.
     */
    private void AddToLoudnessDatas(float loudness)
    {
        var newDecibels = new float[this._loudnessDatas.Length + 1];
        
        for (var i = 0; i < this._loudnessDatas.Length; i++)
        {
            newDecibels[i] = this._loudnessDatas[i];
        }
        newDecibels[this._loudnessDatas.Length] = loudness;

        this._loudnessDatas = newDecibels;
    }
    
    /**
     * Silence 체크 코루틴 시작.
     */
    public void StartSilenceCheckRoutine()
    {
        // StartCoroutine(SilenceCheckRoutine());
    }
    
     /**
     * Silence 체크 코루틴.
     */
    private IEnumerator SilenceCheckRoutine()
    {
        while (true)
        {
            // 읽기 진행중이 아닌 경우, 사용자 발화 진행중이 아닌 경우 무시
            if (!(this._isReading && this._isRunningDialogue)) continue;
            
            // 체크 누적 시간을 채웠을 때
            if (this._silenceCheckedTime >= SilenceCheckDestTime)
            {
                if (this._loudnessDatas.Length > 0) {
                    var avrg = this._loudnessDatas.Average();
                    Debug.Log(avrg);
                    
                    // 누적 시간동안 Loudness 평균이 기준 아래인 경우
                    if (avrg < DialogueEndLoudness)
                    {
                        // 사용자 발화 끝으로 인식
                        this._isRunningDialogue = false;
                        Debug.Log("사용자 발화 끝");
                    
                        // Silence Check 초기화
                        this._loudnessDatas = Array.Empty<float>();
                        this._silenceCheckedTime = 0f;
                        
                        // STT로 전송되도록 읽기 진행중 종료 후 코루틴 종료
                        this._isReading = false;
                        yield break;
                    }
                }
                    
                // Silence Check 초기화
                this._loudnessDatas = Array.Empty<float>();
                this._silenceCheckedTime = 0f;
            }

            // 체크 누적 시간 증가
            this._silenceCheckedTime += SilenceCheckInterval;
            
            // 체크 시간만큼 실제로 대기
            yield return new WaitForSeconds(SilenceCheckInterval);
        }
    }

    /**
     * STT로 전송.
     */
    private void SendToStt()
    {
        // 누적 샘플링 데이터를 AudioClip으로 생성
        var audioClip = AudioClip.Create("Mic_Recording", this._collectedSamplingDatas.Length,
            this._micChannelCount, SampleRate, false);
        audioClip.SetData(this._collectedSamplingDatas, 0);
        
        // STT 실행
        this._sttAzure.RunStt(audioClip);
        
        // 누적 샘플링 데이터 초기화
        this._collectedSamplingDatas = Array.Empty<float>();

        // 읽기 진행중으로 전환
        this._isReading = true;
    }
}
