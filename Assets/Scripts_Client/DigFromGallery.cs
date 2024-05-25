using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
//24-02-19 원 제작자 : 강민구(ggalsrn@khu.ac.kr)
public class DigFromGallery : MonoBehaviour
{
    /// <summary> Texture2D를 바를 rawimg </summary>
    public List<Image> img;
    /// <summary> Texture2D를 바를 obj </summary>
    public GameObject[] tempcube;
    /// <summary> img불러오기 기능을 사용할 버튼 </summary>
    public Button LoadImageBtn;
    /// <summary> img 불러오는 것을 하나만 가능한지 여부 </summary>
    [SerializeField] bool OnlyOne;
    void Start()
    {
        LoadImageBtn.onClick.AddListener(OnClickImageLoad);
    }
    /// <summary>
    /// 갤러리에서 가져오기 버튼을 누르면 호출된다. 디바이스의 저장소에서 사진파일을 가져오는 것을 시도한다. 50메가가 최대 용량이다. 
    /// </summary>
    public void OnClickImageLoad()
    {
        NativeGallery.GetImageFromGallery((file)=>
        {
            if (!string.IsNullOrEmpty(file))
            {
                FileInfo selected = new FileInfo(file);
                
                //용량 제한 점검
                if( selected.Length > 50000000)//5천만바이트 = 50메가
                {
                    return;
                }

            //불러오기
            
                //불러오자
                StartCoroutine(LoadImage(file));
            }

        });
    }
    /// <summary>
    /// 이미지를 byte[]로 받아서 texture화 시킨다. texture화 시킨 것을 2d img에 바르거나 3d obj에 바른다. 
    /// </summary>
    IEnumerator LoadImage(string path)
    {
        yield return null;

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(0,0);
        tex.LoadImage(fileData);

        /// 여러개를 불러와 낙서에 사용할 수 있기 때문에
        /// List로 사용했다.
        if(img.Count > 0){
            if (!OnlyOne) img.Add(Instantiate(img[0],img[0].transform.parent));
            Rect rect = new Rect(0,0, tex.width, tex.height);
            img[img.Count-1].sprite = Sprite.Create(tex, rect, new Vector2(0.5f,0.5f));
            img[img.Count-1].color = new Color(255,255,255,255);
            img[img.Count-1].SetNativeSize();
            img[img.Count-1].gameObject.SetActive(true);
            ImageSizesetting(img[img.Count-1],450,450);
            // img.rectTransform.SetPositionAndRotation(new Vector3(720,1544,0),Quaternion.identity);
            // img[img.Count-1].rectTransform.SetLocalPositionAndRotation(new Vector3(0,0,0),Quaternion.identity);
            SelectColor.Instance?.DigFromGalleryAfter(img[img.Count-1].gameObject);
        }else{
            Digpng(tex);   
        }
    }
    /// <summary>
    /// 사진 이미지가 비율을 지킬 수 있도록 한다.
    /// </summary>
    void ImageSizesetting(Image img, float x , float y)
    {
        var imgX = img.rectTransform.sizeDelta.x;
        var imgY = img.rectTransform.sizeDelta.y;

        if( x / y > imgX / imgY )//이미지 세로길이가 더 길다
        {
            img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x);
            img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imgY * (x/ imgX));
            // img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y);
            // img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imgX * (y/ imgY));
        }
        else//가로길이가 더 길다
        {
            // img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, x);
            // img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imgY * (x/ imgX));
            img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, y);
            img.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imgX * (y/ imgY));
        
        }

    }
    /// <summary>
    /// parameter로 주어진 texture2d를 전역변수인 3d obj에 양면에 바른다.
    /// </summary>
    /// <param name="tex">갤러리에서 가져온 사진파일을 Texture2D로 변환한 것 </param>
    void Digpng(Texture2D tex){
        
        // tempcube = Instantiate(tempcube,tempcube.transform.parent);
        tempcube[0].SetActive(true);
        tempcube[1].SetActive(true);
        
        ///평면의 크기를 사진규격과 비례하게 만드는 코드. 잘 작동 하므로 재조정 안 필요함.
        tempcube[0].transform.localScale = new Vector3(0.02f, 0.02f, 0.02f * tex.height/tex.width);


        // Debug.Log(path);
        // byte[] bytes = System.IO.File.ReadAllBytes(path);
        // Debug.Log(bytes.Length);
        // Texture2D tex = new Texture2D(2,2);
        // tex.LoadImage(bytes);
        tempcube[0].GetComponent<Renderer>().material.mainTexture = tex;
        tempcube[1].GetComponent<Renderer>().material.mainTexture = tex;
    }
}
