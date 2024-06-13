using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; //import
public class Mutant : MonoBehaviour
{
    public Camera UserLoc;
    public Animator anim;
    public AudioSource sound;

    void Start()
    {
        anim.enabled = false;
        UserLoc = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        sound = gameObject.GetComponent<AudioSource>();
    }
    void FixedUpdate()
    {
        // transform.rotation = Quaternion.Inverse(UserLoc.transform.rotation);
        transform.LookAt(UserLoc.transform.position + Vector3.down*0.1f);
        // transform.SetPositionAndRotation(transform.position,Quaternion.Euler(-UserLoc.transform.forward));
        if (Vector3.Distance(UserLoc.transform.position, transform.position) < 1.25f && !anim.enabled){
            anim.enabled = true;
            transform.DOMove(UserLoc.transform.position + Vector3.down*0.1f,0.75f);
            ObjManager.I.IsSpawnObjList[1] = false;
            sound.Play();
            Destroy(this.gameObject,1f);
        }
    }
}
