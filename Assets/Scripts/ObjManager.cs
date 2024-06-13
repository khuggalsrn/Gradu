using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ObjManager : MonoBehaviour
{
    /// <summary> Ray를 쏴서 물체를 탐색할 수 있게 해주는 Class </summary>
    public ARRaycastManager ArRaycaster;
    public Camera ArCam;
    public List<GameObject> ObjList;
    public List<Material> ObjMatList;
    public List<Vector3> ObjDegreeList;
    public List<Vector3> ObjLocList;
    public List<bool> IsSpawnObjList;
    private static ObjManager instance;
    public static ObjManager I
    {
        get
        {
            return instance;
        }
    }
    private void CheckInstance()
    {
        // 싱글톤 인스턴스가 존재하지 않았을 경우, 본인으로 초기화
        if (instance == null)
            instance = this;

        else
            Destroy(gameObject);
    }
    private void Awake()
    {
        CheckInstance();
    }
    void Start()
    {
        for (int i = 0; i < IsSpawnObjList.Count; i++)
        {
            IsSpawnObjList[i] = false;
        }
    }
    public void CanSpawn(Color clr)
    {
        Debug.Log("CanSpawn?");
        if (80/255 <= clr.r && // 0.3137... 0.31 < r < 0.43
         clr.r <= 110/255 && // 0.43137...
           35/255 <= clr.g && // 0.13725... 0.13 < g < 0.23
           clr.g <= 60/255 && // 0.23529...
            clr.b <= 30/255) // 0.05882 ... b < 0.1 갈색.
        {
            Debug.Log("Spawn!");
            Spawn(0, clr);
        }
        else {
            
            Spawn(1, clr);
        }
    }

    void Spawn(int objnum, Color clr)
    {
#if UNITY_EDITOR
        GameObject obj = Instantiate(ObjList[objnum]);
        Material mat = ObjMatList[objnum];
        Vector3 AddDegree = ObjDegreeList[objnum];
        Vector3 AddLoc = Vector3.zero;
        switch (objnum)
        {
            case 0:
                AddLoc = new Vector3(0, 1, 0); //ObjLocList[objnum]+ 
                break;
            case 1:
                AddLoc = new Vector3(0, 0, 0); //ObjLocList[objnum]+ 
                break;
            default:
                break;
        }
        mat.color = clr;
        obj.transform.localScale /= 2;
        obj.transform.SetPositionAndRotation(AddLoc, Quaternion.Euler(AddDegree));
#else
        // if(IsSpawnObjList[objnum]) return;
        Vector2 RayPos = new Vector2(3 * Screen.width / 4, 3 * Screen.height / 4);
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (ArRaycaster.Raycast(RayPos, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;
            IsSpawnObjList[objnum] = true;
            GameObject obj = Instantiate(ObjList[objnum]);
            Material mat = ObjMatList[objnum];
            Vector3 AddDegree = ObjDegreeList[objnum];
            Vector3 AddLoc = Vector3.zero;
            
            switch(objnum){
                case 0:
                    AddLoc = new Vector3(0,ArCam.transform.position.y,0); //ObjLocList[objnum]+ 
                    break;
                case 1:
                    AddLoc = new Vector3(0,ArCam.transform.position.y - hitPose.position.y -ObjLocList[1].y,0); //ObjLocList[objnum]+ 
                    break;
                default:
                    break;
            }
            obj.transform.localScale /= 2;
            mat.color = clr;
            if(clr.a < 1f) // 밝은곳은 a값이 1임 더 낮다는곳은 어두운 곳이었다는 것
                mat.color *= 1 / Mathf.pow(1 + 1.3f *(0.4f -(clr.a - 0.6f)), 2); // 어두운 곳에서
            // 빛의 밝기가 A였고 0.6f를 더한 값이 왔으므로 0.6빼면
            // 빛의 밝기값이 오는 것.
            // 그것에 가중치를 곱하고 매태리얼 컬러가 밝기에따라 어둡게 변하도록함.
            obj.transform.SetPositionAndRotation(hitPose.position + AddLoc, Quaternion.Euler(AddDegree));
            // spawnObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
        }
#endif        
    }
}
