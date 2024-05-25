using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
//24-02-19 원 제작자 : 강민구(ggalsrn@khu.ac.kr)
/// <summary>
/// 낙서하는 Scene에서 불러오는 사진들과 관련된 UI를 제어하는 클래스
/// </summary>
public class UIHandler : MonoBehaviour
, IPointerClickHandler
, IPointerExitHandler
, IPointerEnterHandler
{   
    /// <summary> UIHandler는 Singleton 패턴이다. </summary>
    private static UIHandler instance;
    /// <summary>
    /// 현재 만지고 있는 UI == 사진
    /// </summary>
    public GameObject HandedUI;
    /// <summary>
    /// 불러온 사진 확인 버튼
    /// </summary>
    [SerializeField] Button AccessableBtn ;
    /// <summary>
    /// 불러온 사진 삭제 버튼
    /// </summary>
    [SerializeField] Button DeletebleBtn ;
    /// <summary>
    /// 현재 불러온 사진을 Control하고 있는 상태인지
    /// </summary>
    bool IsAccess;
    /// <summary>
    /// 테두리 Image를 가진 Object
    /// </summary>
    [SerializeField] GameObject RedLine;
    /// <summary>
    /// 테두리 코너쪽 Image를 가진 Object
    /// </summary>
    [SerializeField] GameObject RedCorner;
    /// <summary> 선택한 사진을 회전시키는 scroll </summary>
    [SerializeField] GameObject Rotationscroll ;
    /// <summary> 선택한 사진을 회전시키는 scroll의 회전값을 알려주는 숫자</summary>
    [SerializeField]    Text RotationscrollText;
    /// <summary> Rotate하기위한 1프레임 전 scroll값 </summary>
    float prevalue;
    void Start(){

        if(instance == null){
            instance = this;
        }else{
            Destroy(this.gameObject);
        }

        IsAccess = false;
        AccessableBtn.onClick.AddListener(AccessEnable);
        DeletebleBtn.onClick.AddListener(DeletePicture);
        Rotationscroll.GetComponent<ScrollRect>().horizontalScrollbar.
        onValueChanged.AddListener(RotateObj);

    }
    /// <summary>
    /// UIHanlder의 singleton instace를 불러오기 위한 변수
    /// </summary>
    public static UIHandler Instance
    {
        get
        {
            if(null == instance)
            {
                return null;
            }
            return instance;
        }
    }
    /// <summary>
    /// UI를 클릭했는지 결과를 가져와주는 함수
    /// </summary>
    /// <param name="eventData">마우스 클릭 이벤트데이터</param>
    public void OnPointerClick(PointerEventData eventData){
        if(!IsAccess && (SelectColor.Instance.mode == SelectColor.Mode.PictureControl)){
            GetInHand_UI(eventData.pointerCurrentRaycast.gameObject);
            Debug.Log(HandedUI);
            // AccessableBtn.gameObject.SetActive(true);
            // AccessableBtn.transform.SetParent(HandedUI.transform, false);
            // AccessableBtn.transform.localPosition =
            // new Vector3(-HandedUI.GetComponent<RawImage>().rectTransform.sizeDelta.x,
            // HandedUI.GetComponent<RawImage>().rectTransform.sizeDelta.y);
        }
        
        AccessableBtn.transform.SetAsLastSibling();
        DeletebleBtn.transform.SetAsLastSibling();
        Debug.Log("Click");
        // GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
    }
    /// <summary>
    /// UI에 포인터가 들어갔는지 알려주는 함수. IsOnUI를 true로 한다. 모바일 환경에서는 클릭 시에만 포인터가 움직이는 것을 이용했다.
    /// </summary>
    /// <param name="eventData">마우스 클릭 이벤트데이터</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectColor.Instance.IsOnUI = true;
        
        Debug.Log("Enter");
    }
    /// <summary>
    /// UI에 포인터가 안들어갔는지 알려주는 함수. IsOnUI를 true로 한다. 모바일 환경에서는 클릭 시에만 포인터가 움직이는 것을 이용했다.
    /// </summary>
    /// <param name="eventData">마우스 클릭 이벤트데이터</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        SelectColor.Instance.IsOnUI = false;
        Debug.Log("Exit");
    }
    /// <summary>
    /// 가져온 사진들 중에 선택한 GameObject를 HandedUI에 할당시키고 테두리를 그려준다.
    /// </summary>
    /// <param name="gameob">선택한 Object</param>
    public void GetInHand_UI(GameObject gameob){
        if(!gameob) return;
        HandedUI = gameob;
        TwoBtnSet(true);
        UI_Image_Line();
    }
    /// <summary>
    /// 현재 선택된 사진을 내려놓고 다른 사진을 제어할 수 있게 해주는 함수
    /// </summary>
    async void AccessEnable(){
        await TwoBtnSet(false);
        HandedUI = null;
        Rotationscroll.GetComponent<ScrollRect>().horizontalScrollbar.value = 0.5f;
    }
    /// <summary>
    /// 현재 선택된 사진을 지우는 함수
    /// </summary>
    void DeletePicture(){
        Destroy(HandedUI.gameObject);
        AccessEnable();
    }
    /// <summary>
    /// AccessableBtn 과 DeletebleBtn, Rotationscroll을 param에 맞게 설정하고 현재 선택중인 사진이 있다면 그 사진의 테두리를 없앤다.
    /// </summary>
    /// <param name="tf">true or false</param>
    /// <returns>delay 10ms</returns>
    public Task TwoBtnSet(bool tf){
        IsAccess = tf;
        AccessableBtn.gameObject.SetActive(tf);
        DeletebleBtn.gameObject.SetActive(tf);
        Rotationscroll.gameObject.SetActive(tf);
        Debug.Log(HandedUI);
        if(HandedUI){
            foreach(Transform child in HandedUI.transform){
                Destroy(child.gameObject);
            }
        }
        return Task.Delay(10);
    }
    /// <summary>
    /// 선택한 사진에 테두리를 부여한다.
    /// </summary>
    void UI_Image_Line(){
        float imgX = HandedUI.GetComponent<Image>().rectTransform.sizeDelta.x;
        float imgY = HandedUI.GetComponent<Image>().rectTransform.sizeDelta.y;
        Debug.Log(imgX + imgY);
        float LineX = RedLine.GetComponent<Image>().rectTransform.sizeDelta.x;
        float LineY = RedLine.GetComponent<Image>().rectTransform.sizeDelta.y;

        float countx = imgX / LineX;
        float county = imgY / LineX;//x로 나누는게 맞음.90도로 회전시켜보기 때문.

        GameObject temp = Instantiate(RedCorner, HandedUI.transform);
        temp.GetComponent<Image>().rectTransform.SetLocalPositionAndRotation(
            new Vector3(imgX/2, imgY/2), Quaternion.Euler(0,0,270));

        temp = Instantiate(RedCorner, HandedUI.transform);
        temp.GetComponent<Image>().rectTransform.SetLocalPositionAndRotation(
            new Vector3(imgX/2, -imgY/2), Quaternion.Euler(0,0,180));

        temp = Instantiate(RedCorner, HandedUI.transform);
        temp.GetComponent<Image>().rectTransform.SetLocalPositionAndRotation(
            new Vector3(-imgX/2, imgY/2), Quaternion.Euler(0,0,0));

        temp = Instantiate(RedCorner, HandedUI.transform);
        temp.GetComponent<Image>().rectTransform.SetLocalPositionAndRotation(
            new Vector3(-imgX/2, -imgY/2), Quaternion.Euler(0,0,90));


        for (int i = 2; i < county-1 ; i++){
            temp = Instantiate(RedLine, HandedUI.transform);
            temp.GetComponent<Image>().rectTransform.SetLocalPositionAndRotation(
                new Vector3(imgX/2, -imgY/2 + i * LineX), Quaternion.Euler(0,0,90));
            // temp.GetComponent<Image>().rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,imgY);
        }
        for (int i = 2; i < countx-1 ; i++){
            temp = Instantiate(RedLine, HandedUI.transform);
            temp.GetComponent<Image>().rectTransform.SetLocalPositionAndRotation(
                new Vector3(-imgX/2 + i * LineX , imgY/2),Quaternion.identity);
            // temp.GetComponent<Image>().rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,imgX);
        }
        for (int i = 2; i < county-1 ; i++){
            temp = Instantiate(RedLine, HandedUI.transform);
            temp.GetComponent<Image>().rectTransform.SetLocalPositionAndRotation(
                new Vector3(-imgX/2, -imgY/2 + i * LineX),Quaternion.Euler(0,0,90));
            // temp.GetComponent<Image>().rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,imgY);
        }
        for (int i = 2; i < countx-1 ; i++){
            temp = Instantiate(RedLine, HandedUI.transform);
            temp.GetComponent<Image>().rectTransform.SetLocalPositionAndRotation(
                new Vector3(-imgX/2 + i * LineX ,-imgY/2),Quaternion.identity);
            // temp.GetComponent<Image>().rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,imgX);
        }
   }
   /// <summary>
   /// 선택한 사진을 값에 맞게 회전시킨다.
   /// </summary>
   /// <param name="_value">현재 scroll의 value</param>
   void RotateObj(float _value){
        _value = _value < 0? 0 : _value;
        _value = _value - 0.5f;
        float lack = _value - prevalue;
        prevalue = _value;
        RotationscrollText.text = Mathf.Round(_value*360).ToString() + "˚";
        HandedUI?.transform.Rotate(0,0,-lack*360);
    }
}
