using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
//24-02-19 원 제작자 : 강민구(ggalsrn@khu.ac.kr)
public class ARPlaceOnPlane : MonoBehaviour
{
    /// <summary> Ray를 쏴서 물체를 탐색할 수 있게 해주는 Class </summary>
    public ARRaycastManager ArRaycaster;
    /// <summary> spawn할 object prefab </summary>
    [SerializeField] GameObject PlaceObject;
    /// <summary> spawn된 object </summary>
    public GameObject spawnObject;
    /// <summary> 가상의 공간에 Plane을 놔줄 수 있께 해주는 Class </summary>
    [SerializeField] ARPlaneManager ArPlane;
    /// <summary> Checking 함수를 사용하는 버튼 </summary>
    public List<Button> TurnModeBtns;
    /// <summary> CanInteract가 false일 경우 터치로 인한 상호작용=>InteractByTouch() 이 불가능하다.  </summary>
    bool CanInteract = true;
    /// <summary> spawn된 object를 회전시키는 scroll </summary>
    [SerializeField]    GameObject Rotationscroll;
    /// <summary> spawn된 object를 회전시키는 scroll의 회전값을 알려주는 숫자</summary>
    [SerializeField]    Text RotationscrollText;
    /// <summary> spawn된 object를 위로 90도 세워버리는 Btn </summary>
    [SerializeField]    Button degree90btn;
    /// <summary> 회전시 사용하는 value값. 1프레임 전의 slide value </summary>
    float prevalue = 0;
    /// <summary> (투 터치)1프레임 전에 터치한 두 점사이의 거리 </summary>
    float pretouchdis = 0f;
    void Start()
    {
        foreach(Button btn in TurnModeBtns){
            btn.onClick.AddListener(Checking);
        }
        // Rotationscroll.GetComponent<ScrollRect>().horizontalScrollbar.
        // onValueChanged.AddListener(RotateObj);
        
        // degree90btn.onClick.AddListener(Degree90);
        
    }
    /// <summary>
    /// 1프레임마다 호출한다.
    /// </summary>
    void FixedUpdate()
    {
        // UpdateCenterObject();
        InteractByTouch();
    }
    /// <summary>
    /// 두 손가락 터치 시 그곳에 PlaceObject prefab을 생성하여 spawnObject에 지정한다. 이후 spawnObject의 위치만 바꾼다.
    /// </summary>
    void InteractByTouch()
    {
        if(CanInteract){
            switch (Input.touchCount){
                case 0:
                    pretouchdis = 0;
                    break;
                case 1:
                    Touch touch = Input.GetTouch(0);
                    if( !UICheck()){ 
                        // if(touch.phase != TouchPhase.Moved){
                        //     return;
                        // }
                        List<ARRaycastHit> hits = new List<ARRaycastHit>();
                        if(ArRaycaster.Raycast(touch.position, hits, TrackableType.Planes))
                        {
                            Pose hitPose = hits[0].pose;
                            if(!spawnObject)
                            {
                                spawnObject = PlaceObject;
                                // spawnObject = Instantiate(PlaceObject, hitPose.position, hitPose.rotation);
                                Rotationscroll.gameObject.SetActive(true);
                                degree90btn.gameObject.SetActive(true);
                            }
                            else
                            {
                                Rotationscroll.GetComponent<ScrollRect>().horizontalScrollbar.value = 0.5f;
                                // spawnObject.transform.position = hitPose.position;
                                // spawnObject.transform.rotation = hitPose.rotation;
                                spawnObject.transform.SetPositionAndRotation(hitPose.position,hitPose.rotation);
                            }

                        }
                    }
                    break;
                case 2:
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);
                    Vector3 currenPosition1 =// Camera.main.ScreenToWorldPoint(
                        touch1.position
                        // )
                        ;
                    Vector3 currenPosition2 =// Camera.main.ScreenToWorldPoint(
                        touch2.position
                        // )
                        ;
                    currenPosition1.z = 0f;
                    currenPosition2.z = 0f;
                    // float curslope = (currenPosition1.x - currenPosition2.x) == 0 ? 99999999
                    // :
                    // currenPosition1.y - currenPosition2.y
                    // /
                    // currenPosition1.x - currenPosition2.x;
                    // if(pretouchslope == 0){
                    //     pretouchslope = curslope;
                    // } else {
                    //     float Angle = 2*Mathf.PI*(Mathf.Atan(curslope) - Mathf.Atan(pretouchslope));
                    //     if(Mathf.Abs(Angle)>0f)
                    //         spawnObject.transform.Rotate(0f,Angle,0f);
                    // }
                    float touchdis = Vector2.Distance(currenPosition1,currenPosition2);
                    if (pretouchdis == 0){    
                        pretouchdis = touchdis;
                    } else{
                        float dislack = touchdis - pretouchdis;
                        if(dislack > 0.2f){
                            dislack = Mathf.Round(dislack*10) * 0.1f;
                            dislack = dislack > 0.4f? 0.4f : dislack;
                            spawnObject.transform.localScale *= Mathf.Sqrt(0.8f+dislack);
                        } else if(dislack < -0.2f){
                            dislack = Mathf.Round(dislack*10) * 0.1f;
                            dislack = dislack < -0.4f ? -0.4f : dislack;
                            spawnObject.transform.localScale *= Mathf.Sqrt(1.2f+dislack);
                        }

                        if(spawnObject.transform.localScale.x < 0.002f){
                            spawnObject.transform.localScale = new Vector3(0.002f,0.002f,0.002f * spawnObject.transform.localScale.z /
                                                                                                 spawnObject.transform.localScale.x );
                        }
                        if(spawnObject.transform.localScale.x > 1){
                            spawnObject.transform.localScale = new Vector3(1,1,1* spawnObject.transform.localScale.z /
                                                                                                 spawnObject.transform.localScale.x );
                        }
                        pretouchdis = touchdis;
                    }
                    break;
            }
        }
    }
    /// <summary> UI 터치했는지 체크</summary>
    public bool UICheck(){
        PointerEventData eventdatacurrentposition = new PointerEventData(EventSystem.current);
        eventdatacurrentposition.position = new Vector2(Input.GetTouch(0).position.x,Input.GetTouch(0).position.y);
        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventdatacurrentposition, results);
        return results.Count>0;
    }
    /// <summary>
    /// 촬영 모드에 들어가고 나오는 버튼을 누르면 호출된다. 검은 선을 띄는 plane들을 모두 활성<->비활성 시킨다.
    /// </summary>
    public void Checking(){
        CanInteract = !CanInteract;
        foreach (var plane in ArPlane.trackables)
        {
            plane.gameObject.SetActive(!plane.gameObject.activeSelf);
        }
        if(Rotationscroll.activeSelf){
            Rotationscroll.SetActive(false);
            degree90btn.gameObject.SetActive(false);
        }else{
            Rotationscroll.SetActive(spawnObject!=null);
            degree90btn.gameObject.SetActive(spawnObject!=null);
        }
    }
    /// <summary>
    /// Slide bar로 값을 조절하여 spawnObject를 회전시킨다. Slider의 value 값의 범위는 [0,1]이므로 [0,360](각도)의 값을 대응 시킨다.
    /// </summary>
    void RotateObj(float _value){
        _value = _value - 0.5f;
        float lack = _value - prevalue;
        prevalue = _value;
        RotationscrollText.text = Mathf.Round(_value*360).ToString();
        spawnObject.transform.Rotate(0,lack*360,0);
    }
    /// <summary>
    /// 가져온 낙서가 그려진 평면을 내가 보는 방향에 맞게 90도 회전시킨다.
    /// </summary>
    void Degree90(){
        spawnObject.transform.Rotate(90,0,0);
    }
    // void UpdateCenterObject()
    // {
    //     Vector3 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f,0.5f));
    //     List<ARRaycastHit> hits = new List<ARRaycastHit>();
    //     ArRaycaster.Raycast(screenCenter,hits, TrackableType.Planes);

    //     if (hits.Count > 0)
    //     {
    //         Pose placementPose = hits[0].pose;
    //         PlaceObject.SetActive(true);
    //         PlaceObject.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
    //     }
    //     // else
    //     // {
    //     //     PlaceObject.SetActive(false);
    //     // }
    // }
}
