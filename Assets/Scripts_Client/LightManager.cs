using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class LightManager : MonoBehaviour
{
    public ARCameraManager aRCameraManager;
    public Text textUI;
    void Start()
    {
        aRCameraManager.frameReceived += FrameLightUpdated;
    }
    public void FrameLightUpdated(ARCameraFrameEventArgs args)
    {
        var brightness = args.lightEstimation.averageBrightness;
        var color = args.lightEstimation.mainLightColor;
        if (brightness.HasValue)
        {
            textUI.text = brightness.Value.ToString();
            textUI.text += color.Value;
        }

    }

   
}
