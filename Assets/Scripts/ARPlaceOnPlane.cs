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
    float time = 0f;
    [SerializeField] Text timetext;
    Texture2D screenTex;
    [SerializeField] Text touchposUI;
    [SerializeField] Image ActImg;
    [SerializeField] Text ActImgText;
    [SerializeField] Image ActImg2;
    [SerializeField] Text ActImgText2;
    [SerializeField] Material ObjMat;
    [SerializeField] Text BrightnessText;
    bool IsStop;
    int[] ws = {0,0,Screen.width/2,Screen.width/2};
    int[] we = {Screen.width/2,Screen.width/2,Screen.width,Screen.width};
    int[] hs = {0,Screen.height/2,0,Screen.height/2};
    int[] he = {Screen.height/2,Screen.height,Screen.height/2,Screen.height};
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
        time += 0.021f;
        timetext.text = time.ToString();
        if (time > 3f && !IsStop)
        {
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
            time += -3f;
        }

    }
    void Update()
    {
        // UpdateCenterObject();
        // InteractByTouch();

    }
        
    private IEnumerator ReadScreen()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < 4; i++)
        {
            int a = UnityEngine.Random.Range(0,4);
            screenTex = new Texture2D(Screen.width / 2, Screen.height / 2, TextureFormat.RGB24, false);
            Rect area = new Rect(ws[a], hs[a], we[a], he[a]);

            // 현재 스크린으로부터 지정 영역의 픽셀들을 텍스쳐에 저장
            screenTex.ReadPixels(area, 0, 0);

            var tw = screenTex.width;

            var th = screenTex.height;

            var source = screenTex.GetPixels();

            float avgR = 0;

            float avgG = 0;

            float avgB = 0;
            foreach (Color clr in source)
            {
                avgR += clr.r;
                avgG += clr.g;
                avgB += clr.b;
            }
            avgR /= source.Length;
            avgG /= source.Length;
            avgB /= source.Length;

            Color avgClr = new Color(avgR, avgG, avgB);
            // avgClr *= 0.5f;
            ActImg2.color = avgClr;
            ActImgText2.text = avgClr.ToString();
            float A;
            if (float.TryParse(BrightnessText.text, out A))
            {
                // avgClr = new Color(avgR,avgG,avgB, A/2);
                if (A < 0.4f)
                { // 밝기가 0.4보다 낮으면 어두운 곳이라고 판단.
                    avgClr.r *= 1 + 1.7f * (0.4f - A);
                    avgClr.g *= 1 + 1.7f * (0.4f - A);
                    avgClr.b *= 1 + 1.7f * (0.4f - A);
                }
                // Color값은 높아지면 밝아지는 것이므로 높게 조정하여 원래 색을 추정함.
                // 후에 다시 낮게바꾸어 오브젝트에 적용할 것임.
                avgClr.a = A > 0.4f ? 1 : 0.6f + A;
                //밝기값을 토대로 어두운곳에서는 살짝만 투명해지게 조정.

                // ObjMat.color = avgClr;
                ActImg.color = avgClr;
                ActImgText.text = avgClr.ToString();
                objManager.SendMessage("CanSpawn", avgClr);
            }
            else
            {
                ActImg.color = avgClr;
                ActImgText.text = avgClr.ToString();
                objManager.SendMessage("CanSpawn", avgClr);
            }
        }
    }
}
