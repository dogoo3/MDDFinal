using System.IO;
using UnityEngine;

public class SkeletonHandler : MonoBehaviour
{
    [SerializeField] private GameObject skeleton; // 스켈레톤
    private Transform[] _jointTransforms; // 스켈레톤 트랜스폼 저장
    private int _settedJointTramsformCount = 0; // 스켈레톤 트랜스폼 저장 카운트
    private const int GesticulatorJointCount = 15; // Gesticulator가 생성하는 Joint 갯수
    private GesticulatorRunner _gesticulatorRunner; // Gesticulator Runner 클래스
    private AvatarHandler _avatarHandler; // 스켈레톤 핸들러 클래스
    private string _wavFilePath; // wav 파일 경로
    
    // 제스처 재생
    private Quaternion[,] _gestureData; // 제스처 데이터
    private int _gestureDataFrameCount = 0; // 제스처 데이터의 Row 갯수
    private int _gestureDataJointCount = 0; // 제스처 데이터의 Col 갯수
    private int _currentFrameIdx = 0; // 현재 프레임
    
    private void Awake()
    {
        this._gesticulatorRunner = FindObjectOfType<GesticulatorRunner>();
        this._avatarHandler = FindObjectOfType<AvatarHandler>();
        
        // Gesticulator가 생성하는 Joint 갯수에 맞춰 JointTransforms 세팅
        this._jointTransforms = new Transform[GesticulatorJointCount];
        this.SetJointTransforms(this.skeleton.transform.GetChild(0));
    }

    /**
     * JointTransforms 세팅.
     */
    private void SetJointTransforms(Transform parentTransform, bool isRoot = true)
    {
        // GesticulatorJointCount를 채우면 종료
        if (this._settedJointTramsformCount == GesticulatorJointCount) return;
        
        // Root(=Hip)는 제외
        if (!isRoot)
        {
            // jointTransforms에 트랜스폼 추가
            this._jointTransforms[_settedJointTramsformCount] = parentTransform;

            // settedJointTramsformCount + 1
            ++this._settedJointTramsformCount;
        }
        
        // 자식 트랜스폼에 대한 재귀적 처리
        foreach (Transform childTransform in parentTransform)
        {
            this.SetJointTransforms(childTransform, false);
        }
    }
        
    /**
     * 스켈레톤 실행.
     */
    public void RunSkeleton(string text, string wavFilePath, AudioClip audioClip)
    {
        this._wavFilePath = wavFilePath;
        
        // Gesticulator 실행
        this._gestureData = this._gesticulatorRunner.RunGesticulator(text, this._wavFilePath);
        
        // 제스처 데이터의 Row 갯수 = 프레임 갯수, Col 갯수 = Joint 갯수 
        this._gestureDataFrameCount = this._gestureData.GetLength(0);
        this._gestureDataJointCount = this._gestureData.GetLength(1);
        
        // 아바타에서 오디오 재생
        // 아바타의 제스처는 스켈레톤 제스처에서 전이 설정되어 있음
        StartCoroutine(_avatarHandler.PlayAudio(audioClip));
        
        // 스켈레톤 제스처 재생 - gestureData의 한 프레임씩 20fps 속도로 실행
        var delay = 0;
        var repeatRate = 1.0f / 20;
        InvokeRepeating(nameof(PlaySkeleton), delay, repeatRate);
    }

    /**
     * 스켈레톤 제스처 재생 - gestureData의 한 프레임씩 20fps 속도로 실행.
     */
    private void PlaySkeleton()
    {
        // Debug.Log("Current Frame : " + this._currentFrameIdx);
        
        // 각 Joint 각도 변경
        for (var j = 0; j < this._gestureDataJointCount; j++)
        {
            Quaternion rotation = this._gestureData[this._currentFrameIdx, j];
            
            this._jointTransforms[j].localRotation = rotation;
            // Debug.Log(this._jointTransforms[j].name + " : " + this._gestureData[i, j]);
        }
        
        // 현재 프레임 + 1
        ++this._currentFrameIdx;

        // 현재 프레임이 마지막 프레임이면
        if (this._currentFrameIdx == this._gestureDataFrameCount)
        {
            // InvokeRepeating 종료
            CancelInvoke(nameof(PlaySkeleton));
            this._currentFrameIdx = 0;
            
            // wav 파일 삭제
            File.Delete(this._wavFilePath);
        }
    }
}
