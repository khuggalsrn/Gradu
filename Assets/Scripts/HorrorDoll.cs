using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorrorDoll : MonoBehaviour
{
    public Camera UserLoc;
    public AudioSource sound;

    void Start()
    {
        UserLoc = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        sound = gameObject.GetComponent<AudioSource>();
    }
    void FixedUpdate()
    {
        // transform.rotation = Quaternion.Inverse(UserLoc.transform.rotation);

        // transform.SetPositionAndRotation(transform.position,Quaternion.Euler(-UserLoc.transform.forward));
        if (Vector3.Distance(UserLoc.transform.position, transform.position) < 2f && ObjManager.I.IsSpawnObjList[0]){
            transform.LookAt(UserLoc.transform.position);
            transform.rotation *= Quaternion.AngleAxis(-90, new Vector3(0,0,1));
            transform.rotation *= Quaternion.AngleAxis(-90, new Vector3(1,0,0));
            transform.rotation *= Quaternion.AngleAxis(-90, new Vector3(0,1,0));
        }
        if (Vector3.Distance(UserLoc.transform.position, transform.position) < 1f && ObjManager.I.IsSpawnObjList[0]){
            ObjManager.I.IsSpawnObjList[0] = false;
            sound.Play();
            Destroy(this.gameObject,0.4f);
        }
    }
}
