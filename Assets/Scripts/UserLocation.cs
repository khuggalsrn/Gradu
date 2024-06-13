using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
//24-02-19 원 제작자 : 강민구(ggalsrn@khu.ac.kr)
public class UserLocation : MonoBehaviour
{

    [Header("Setting")]
    /// <summary>
    /// Start문에서 GPS를 실행할 것인지 여부
    /// </summary>
    [SerializeField] bool startGPSOnStart; 
    
    /// <summary>
    /// 현재 위치로부터의 최대 오차를 지정하는 변수 (정확도) 10M = defalut
    /// </summary>
    [SerializeField] float desiredAccuracyInMeters; 
    
    /// <summary>
    /// 특정 거리 이상 이동하면 갱신되도록 지정하는 변수 (갱신 빈도) 10M = defalut
    /// </summary>
    [SerializeField] float updateDistanceInMeters; 
    [SerializeField] public double latitude;
    [SerializeField] public double longitude;
    [SerializeField] public double altitude;

    [Header("Cache")]
    private LocationService locationService; // 핵심 클래스
    /// <summary>
    /// UserLocation Singleton instance
    /// </summary>
    private static UserLocation instance;
    /// <summary>
    /// UserLocation Singleton instance
    /// </summary>
    public static UserLocation Instance
    {
        get{
            return instance;
        }
        set {
            instance = value;
        }
    }
    private void Awake()
    {
        locationService = Input.location; //계속 참조할 것이므로 캐싱
        if(instance == null){
            instance = this;
        }else{
            Destroy(this.gameObject);
        }
        if (startGPSOnStart) //Start문에서 GPS를 실행하고자 하면
            StartGPS(); //실행
    }
    
    /// <summary>
    /// GPS를 실행하는 함수
    /// </summary>
    /// <param name="permissionName"> null </param>
    public void StartGPS(string permissionName = null)
    {
        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation)) //이미 위치 권한을 획득했으면
        {
            locationService.Start(desiredAccuracyInMeters, updateDistanceInMeters); //서비스 시작
        }
        else //아직 위치 권한을 획득하지 못했으면
        {
            PermissionCallbacks callbacks = new(); //콜백 함수 생성 후
            callbacks.PermissionGranted += StartGPS; //현재 함수를 재귀로 들어오도록
            Permission.RequestUserPermission(Permission.FineLocation, callbacks); //권한 요청 후, 다시 GPS를 시작하도록 함수 실행
        }
    }
    /// <summary>
    /// GPS를 정지하는 함수
    /// </summary>
    public void StopGPS() 
    {
        locationService.Stop(); //서비스 정지
    }
    /// <summary>
    /// GetLocation을 실행하는 함수, GPS 위치를 최신화
    /// </summary>
    /// <returns></returns>
    public bool StartGetLocation(){
        LocationServiceStatus temp = LocationServiceStatus.Initializing;
        return GetLocation(out temp,out latitude,out longitude,out altitude);
    }
    /// <summary>
    /// 현재 위치를 최신화
    /// </summary>
    /// <param name="status"> GPS 상태 </param>
    /// <param name="latitude"> 위도 </param>
    /// <param name="longitude"> 경도 </param>
    /// <param name="altitude"> 고도 </param>
    /// <returns></returns>
    private bool GetLocation(out LocationServiceStatus status, out double latitude, out double longitude, out double altitude) //위치 정보를 얻는 함수
    {
        latitude = 0f; //위도
        longitude = 0f; //경도
        altitude = 0f; //고도
        status = locationService.status; //서비스 상태

        if (!locationService.isEnabledByUser) //만약, 사용자가 스마트폰의 GPS 기능을 껐다면
            return false;

        switch (status)
        {
            case LocationServiceStatus.Stopped: //GPS를 시작하지 않음
            case LocationServiceStatus.Failed: //GPS 정보를 가져올 수 없음
            case LocationServiceStatus.Initializing: //GPS 기능 시작 후 초기화 중
                return false; //false를 반환해서 정상적으로 정보를 주지 못했음을 알림 (그 원인은 status에 담김)

            default: //GPS 기능이 정상적임 (Running)
                LocationInfo locationInfo = locationService.lastData; //마지막 GPS 정보를 담고
                latitude = locationInfo.latitude; //위도 지정
                longitude = locationInfo.longitude; //경도 지정
                altitude = locationInfo.altitude; //고도 지정
                return true; //true를 반환해서 정상적으로 정보를 줬음을 알림
        }
    }
}

