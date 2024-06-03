using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Rito.Tests;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using System;
using System.IO;

public class LightManager : MonoBehaviour
{
    public ARCameraManager aRCameraManager;
    public Text textUI;
    [SerializeField] Test_ScreenShot t_ss;
    public Image destination;
    private Texture2D _imageTexture; // destination 소스 텍스쳐
    void Start()
    {
        aRCameraManager.frameReceived += FrameLightUpdated;

        // 현재 스크린으로부터 지정 영역의 픽셀들을 텍스쳐에 저장
    }
    public void FrameLightUpdated(ARCameraFrameEventArgs args)
    {
        var brightness = args.lightEstimation.averageBrightness;
        var color = args.lightEstimation.mainLightColor;
        if (brightness.HasValue)
        {
            textUI.text = brightness.Value.ToString();
        }
        if (color.HasValue)
        {   
            textUI.text = color.Value.ToString();
        }
    }
    private void Update() {
        if(Input.GetMouseButtonDown(0)){
            // StartCoroutine(TakeScreenShotRoutine());
            StartCoroutine(showscreen());
        }
        if(Input.GetKeyDown(KeyCode.Keypad0)){
            // StartCoroutine(TakeScreenShotRoutine());
            t_ss.ReadScreenShotAndShow();
        }
    }
    private IEnumerator showscreen()
    {
        yield return new WaitForEndOfFrame();
        Texture2D screenTex = new Texture2D(Screen.width/2, Screen.height/2, TextureFormat.RGB24, false);
        Rect area = new Rect(Screen.width/2, Screen.height/2, Screen.width, Screen.height);

        // 현재 스크린으로부터 지정 영역의 픽셀들을 텍스쳐에 저장
        screenTex.ReadPixels(area, 0, 0);
        //기존의 텍스쳐 소스 제거
        if (_imageTexture != null)
            Destroy(_imageTexture);
        if (destination.sprite != null)
        {
            // Destroy(destination.sprite);
            destination.sprite = null;
        }
        try
        {
            byte[] texBuffer = screenTex.EncodeToPNG();

            _imageTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
            _imageTexture.LoadImage(texBuffer);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }

        // 이미지 스프라이트에 적용
        Rect rect = new Rect(0, 0, _imageTexture.width, _imageTexture.height);
        Sprite sprite = Sprite.Create(_imageTexture, rect, Vector2.one * 0.5f);
        destination.sprite = sprite;
        destination.SetNativeSize();
        // 마무리 작업
        Destroy(screenTex);
    }
    // private IEnumerator TakeScreenShotRoutine()
    // {
    //     yield return new WaitForEndOfFrame();
    //     CaptureScreenAndSave();
    // }
    // private void CaptureScreenAndSave() {
        
        
        
        
    // }

}
