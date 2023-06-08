using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AvatarManager : MonoBehaviour
{
    [SerializeField] private GameObject avatar; // 아바타
    [SerializeField] private AudioSource audioSource; // 오디오 소스
    private AudioClip _audioClip; // 재생할 오디오 클립
    private string _wavFilePath; // WAV 파일 경로
    private bool _isPlaying; // 제스처 재생중 여부
    private List<Transform> _avatarTransforms = new List<Transform>(); // 아바타 구조 저장
    private readonly int[] _jointIndex = {2, 3, 4, 5};
    private double[,] _gestureData; // 제스처 데이터
    private int _rowCnt; // 제스처 데이터의 열 갯수
    private int _colCnt;// 제스처 데이터의 행 갯수
    private GesticulatorRunner _gesticulatorRunner; // Gesticulator Runner 클래스
    
    private void Awake()
    {
        this._gesticulatorRunner = FindObjectOfType<GesticulatorRunner>();
        
        // 아바타 구조 읽기
        this.ReadAvatarTransform(this.avatar.transform.GetChild(0));
    }

    private void Start()
    {
        for (int i = 0; i < this._avatarTransforms.Count; i++)
        {
            Debug.Log(i);
            Debug.Log(this._avatarTransforms[i]);
        }
    }

    private void FixedUpdate()
    {
        if (this._isPlaying)
        {
            this._avatarTransforms[0].localPosition = new Vector3(0, 0, 0);
            this._avatarTransforms[0].localRotation = Quaternion.identity; 
        
            for (var i = 0; i < this._rowCnt; i++)
            {
                for (var j = 1; j < this._colCnt/3; j++)
                {
                    var eulerAngle = new Vector3
                    {
                        z = (float) this._gestureData[i, j*3 + 0],
                        x = (float) this._gestureData[i, j*3 + 1],
                        y = (float) this._gestureData[i, j*3 + 2]
                    };
                    Quaternion rotation = EulerAngleToQuaternion(eulerAngle.x, eulerAngle.y, eulerAngle.z);
                    
                    this._avatarTransforms[j].localRotation = rotation;
                }
            }
        }
    }

    /**
     * 아바타 구조 읽기.
     */
    private void ReadAvatarTransform(Transform rootTransform)
    {
        this._avatarTransforms.Add(rootTransform);

        foreach (Transform childTransform in rootTransform)
        {
            this.ReadAvatarTransformInChild(childTransform);
        }
    }
        
    /**
     * 아바타 자식 구조 읽기
     */
    private void ReadAvatarTransformInChild(Transform childTransform)
    {
        this._avatarTransforms.Add(childTransform);

        foreach (Transform grandChildTransform in childTransform)
        {
            this.ReadAvatarTransformInChild(grandChildTransform);
        }
    }
        
    /**
     * 아바타 실행.
     */
    public void RunAvatar(string text, AudioClip audioClip, string wavFilePath)
    {
        // Gesticulator 실행
        this._gestureData = this._gesticulatorRunner.RunGesticulator(text, wavFilePath);
        this._rowCnt = this._gestureData.GetLength(0);
        this._colCnt = this._gestureData.GetLength(1);
        // Debug.Log(this._rowCnt);
        // Debug.Log(this._colCnt);
        
        Debug.Log("(5/8) 아바타 실행 시작");
        
        // AudioSource 세팅
        this._audioClip = audioClip;
        this._wavFilePath = wavFilePath;
        this.audioSource.spatialBlend = 0; // 스테레오 출력으로 믹싱 설정
        this.audioSource.clip = this._audioClip;
        
        // 오디오 재생, 제스처 재생
        StartCoroutine(this.PlayAudio());
        StartCoroutine(this.PlayGesture());
    }

    /**
     * 오디오 재생.
     */
    private IEnumerator PlayAudio()
    {
        // 오디오 재생
        this.audioSource.Play();
        
        // 재생이 끝날 때까지 대기
        yield return new WaitForSeconds(this.audioSource.clip.length);
        
        // wav 파일 삭제
        File.Delete(this._wavFilePath);
    }
        
    /**
     * 제스처 재생.
     */
    private IEnumerator PlayGesture()
    {
        _isPlaying = true;
        
        // 재생이 끝날 때까지 대기
        yield return new WaitForSeconds(this.audioSource.clip.length);

        _isPlaying = false;
        
        Debug.Log("(8/8) 아바타 실행 종료");
    }
        
    /**
     * Euler Angle을 Quaternion로 변환.
     */
    private Quaternion EulerAngleToQuaternion(float eulerX, float eulerY, float eulerZ)
    {
        Quaternion rotation = Quaternion.Euler(eulerX, eulerY, eulerZ);
        return rotation;
    }
}
