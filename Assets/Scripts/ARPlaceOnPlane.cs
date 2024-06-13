using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using System.IO;
//24-02-19 원 제작자 : 강민구(ggalsrn@khu.ac.kr)
public class ARPlaceOnPlane : MonoBehaviour
{
    /// <summary> Ray를 쏴서 물체를 탐색할 수 있게 해주는 Class </summary>
    public ARRaycastManager ArRaycaster;
    public ObjManager objManager;
    /// <summary> spawn할 object prefab </summary>
    [SerializeField] GameObject PlaceObject;
    /// <summary> spawn된 object </summary>
    public GameObject spawnObject;
    /// <summary> 가상의 공간에 Plane을 놔줄 수 있께 해주는 Class </summary>
    [SerializeField] ARPlaneManager ArPlane;
    /// <summary> CanInteract가 false일 경우 터치로 인한 상호작용=>InteractByTouch() 이 불가능하다.  </summary>
    bool CanInteract = true;
    float time = 0f;
    [SerializeField] Text timetext;
    Texture2D screenTex;
    [SerializeField] Text touchposUI;
    [SerializeField] Image ActImg;
    [SerializeField] Text ActImgText;
    [SerializeField] Material ObjMat;
    [SerializeField] Text BrightnessText;
    bool IsStop;
    void Start()
    {
        IsStop = false;
        spawnObject = PlaceObject;
    }
    /// <summary>
    /// 1프레임마다 호출한다.
    /// </summary>
    void FixedUpdate()
    {
        time += 0.02f;
        timetext.text = time.ToString();
        if (time >3f && !IsStop)
        {
            time = 0f;
            // Vector2 RayPos = new Vector2(3 * Screen.width / 4, 3 * Screen.height / 4);
            // List<ARRaycastHit> hits = new List<ARRaycastHit>();
            // if (ArRaycaster.Raycast(RayPos, hits, TrackableType.Planes))
            // {
            //     Pose hitPose = hits[0].pose;
            //     if (!spawnObject)
            //     {
            //         spawnObject = PlaceObject;
            //     }
            //     else
            //     {
            //         spawnObject.transform.SetPositionAndRotation(hitPose.position, Quaternion.Euler(-90,-180,0));
            //         // spawnObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
            //     }
            // }
            StartCoroutine(ReadScreen());
        }

    }
    void Update()
    {
        // UpdateCenterObject();
        InteractByTouch();

    }
    /// <summary>
    /// 두 손가락 터치 시 그곳에 PlaceObject prefab을 생성하여 spawnObject에 지정한다. 이후 spawnObject의 위치만 바꾼다.
    /// </summary>
    void InteractByTouch()
    {
        if (true)
        {
            if (Input.touchCount > 0)
            {
                IsStop = !IsStop;
                // Touch touch = Input.GetTouch(0);
                // touchposUI.text = touch.position.ToString() + "w " + Screen.width.ToString() + "/h  " + Screen.height.ToString();
                // List<ARRaycastHit> hits = new List<ARRaycastHit>();
                // if (ArRaycaster.Raycast(touch.position, hits, TrackableType.Planes))
                // {
                //     Pose hitPose = hits[0].pose;
                //     if (!spawnObject)
                //     {
                //         spawnObject = PlaceObject;
                //     }
                //     else
                //     {
                //         spawnObject.transform.SetPositionAndRotation(hitPose.position, Quaternion.Euler(-90,-180,0));
                //     }

                // }

            }
        }
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
    private IEnumerator ReadScreen()
    {
        yield return new WaitForEndOfFrame();
        screenTex = new Texture2D(Screen.width / 2, Screen.height / 2, TextureFormat.RGB24, false);
        Rect area = new Rect(Screen.width / 2, Screen.height / 2, Screen.width, Screen.height);

        // 현재 스크린으로부터 지정 영역의 픽셀들을 텍스쳐에 저장
        screenTex.ReadPixels(area, 0, 0);

        var tw = screenTex.width;

        var th = screenTex.height;

        var source = screenTex.GetPixels();

        float avgR = 0;

        float avgG = 0;

        float avgB = 0;
        foreach (Color clr in source){
            avgR += clr.r;
            avgG += clr.g;
            avgB += clr.b;
        }
        avgR /= source.Length;
        avgG /= source.Length;
        avgB /= source.Length;

        Color avgClr = new Color(avgR,avgG,avgB);
        avgClr *= 0.5f;
        float A;
        if(float.TryParse(BrightnessText.text, out A)){
            // avgClr = new Color(avgR,avgG,avgB, A/2);
            if(A<0.4f){ // 밝기가 0.4보다 낮으면 어두운 곳이라고 판단.
                avgClr.r *= 1 + 1.3f* (0.4f-A); 
                avgClr.g *= 1 + 1.3f* (0.4f-A); 
                avgClr.b *= 1 + 1.3f* (0.4f-A); 
            }
            // Color값은 높아지면 밝아지는 것이므로 높게 조정하여 원래 색을 추정함.
            // 후에 다시 낮게바꾸어 오브젝트에 적용할 것임.
            avgClr.a = A > 0.4f? 1 : 0.6f+A;
            //밝기값을 토대로 어두운곳에서는 살짝만 투명해지게 조정.

            // ObjMat.color = avgClr;
            ActImg.color = avgClr;
            ActImgText.text = avgClr.ToString();
            objManager.SendMessage("CanSpawn",avgClr);
        }
        else{
            ActImg.color = avgClr;
            ActImgText.text = avgClr.ToString();
            objManager.SendMessage("CanSpawn",avgClr);
        }
    }
}