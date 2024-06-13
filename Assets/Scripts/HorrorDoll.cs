using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorrorDoll : MonoBehaviour
{
    public Camera UserLoc;

    void Start()
    {
        UserLoc = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    void FixedUpdate()
    {
        // transform.rotation = Quaternion.Inverse(UserLoc.transform.rotation);
        transform.LookAt(UserLoc.transform.position);
        transform.rotation *= Quaternion.AngleAxis(-90, new Vector3(0,0,1));
        transform.rotation *= Quaternion.AngleAxis(-90, new Vector3(1,0,0));
        transform.rotation *= Quaternion.AngleAxis(-90, new Vector3(0,1,0));

        // transform.SetPositionAndRotation(transform.position,Quaternion.Euler(-UserLoc.transform.forward));
        if (Vector3.Distance(UserLoc.transform.position, transform.position) < 1f){
            // Destroy(this.gameObject,0.1f);
            ObjManager.I.IsSpawnObjList[1] = false;
        }
    }
}
