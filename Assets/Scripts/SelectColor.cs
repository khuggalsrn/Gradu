using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.AI;
//24-02-19 원 제작자 : 강민구(ggalsrn@khu.ac.kr)
public class SelectColor : MonoBehaviour
{   
    /// <summary> SelectColor(=DrawManager)는 Singleton 패턴이다. </summary>
    private static SelectColor instance;
    public List<Memory> Lines;
    private static int linesIndex;
    public int LinesIndex{
        set {
            linesIndex = value;
            if(value > -1){
                UndoBtn.gameObject.SetActive(true);
            }
            else{
                UndoBtn.gameObject.SetActive(false);
            }
            if(value+1 < Lines.Count){
                RedoBtn.gameObject.SetActive(true);
            }
            else{
                RedoBtn.gameObject.SetActive(false);
            }
        }
        get {
            return linesIndex;
        }
    }
    /// <summary>
    /// SelectColor의 singleton instace를 불러오기 위한 변수
    /// </summary>
    public static SelectColor Instance
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
    /// <summary> 팔레트 여는 버튼 </summary>
    [SerializeField]    Button PalleteBtn;
    /// <summary> 선 굵기 정하는 버튼 </summary>
    [SerializeField]    Button ChooseThicknessBtn;
    [SerializeField]    Button ScaleChangeModeBtn;
    /// <summary> 컨트롤 모드 버튼 </summary>
    [SerializeField]    List<GameObject> CtrlModeUIs;
    
    /// <summary> 펜 타입/ 지우개 사용 버튼들 </summary>
    [SerializeField]    List<Button> ToolBtns;
    
    /// <summary> 낙서 모드에서 사용하는 버튼들 </summary>
    [SerializeField]    List<GameObject> DrawBtns;
    /// <summary> 굵기 조절하는 버튼들 </summary>
    [SerializeField]    List<GameObject> ThicknessBtns;
    
    /// <summary> 굵기 조절하는 버튼 선택시 핑크 </summary>
    [SerializeField]    Sprite Pink;
    
    /// <summary> 굵기 조절하는 버튼 선택시 회색 </summary>
    [SerializeField]    Sprite Gray;
    /// <summary> 현재 터치하여 컨트롤할 것을 선택당한 Img </summary>
    Image img => UIHandler.Instance.HandedUI.GetComponent<Image>();
    /// <summary> 무지개 색을 표현할 gradient component </summary>
    [SerializeField]    Gradient gradient;
    /// <summary> 되돌리기 버튼 </summary>
    [SerializeField]    Button UndoBtn;
    /// <summary> 앞으로감기 버튼 </summary>
    [SerializeField]    Button RedoBtn;
    /// <summary> Draw에 넘겨줄 Color 변수. Color.black으로 초기화 </summary>
    public Color clr = Color.black;
    /// <summary> private field, Draw에 넘겨줄 width 변수. 0.1 로 초기화 </summary>
    private float _width;
    /// <summary> width의 public instance </summary>
    public float width{
        get{
            return _width;
        }
        set{
            PenUp_EraserDown();
            _width = value;
        }
    }
    /// <summary> slider's handle object </summary>
    [SerializeField]    GameObject handle;
    /// <summary> 그리는 펜의 타입 => 노말 </summary>
    [SerializeField] Material NormalPen;
    /// <summary> 그리는 펜의 타입 => 브러시 </summary>
    [SerializeField] Material BrushPen;
    /// <summary> 그린 획의 수 </summary>
    public int linenum = 0;
    /// <summary> 지우는 모드인지 아닌지 판별 </summary>
    public bool modeerase = false;
    /// <summary> UI위에 마우스를 올리면 true 나가면 false </summary>
    public bool IsOnUI = false;
    /// <summary> 현재 Draw 모드인지 PictureControl 모드인지 설정하는 enum class </summary>
    public enum Mode{
        Draw, PictureControl, Capturing
    }
    /// <summary> Draw모드인지 PictureControl모드인지 저장. </summary>
    public Mode mode;
    /// <summary> mode를 변경하는 버튼 </summary>
        
    // / <summary> 크기를 변경할 img의 component </summary>
    // public RawImage img;
    /// <summary> (원 터치) 1프레임 전에 터치한 한 점 </summary>
    Vector3 previousPosition;
    /// <summary> (투 터치)1프레임 전에 터치한 두 점사이의 거리 </summary>
    float pretouchdis = 0f;
    /// <summary> pentype, brushtype, erase가 up인지 down인지 저장, up은 1 down은 0    /// </summary>
    List<int> UpDown;
    /// <summary> pentype, brushtype 중에 마지막으로 쓴 놈 0은 펜 1은 브러시  /// </summary>
    int LastPenType;
    /// <summary> pentype과 brushtype을 다루기 위한 Material, 현재 사용중인 Material이다.  /// </summary>    
    public Material CurMat;
    void Awake() {
        if(instance == null){
            instance = this;
        }else{
            Destroy(this.gameObject);
        }
        Lines = new List<Memory>();
        LinesIndex = -1;
    }
    void Start() {
        UpDown = new List<int>{1,0,0};
        LastPenType = 0;
        mode = Mode.Draw;
        ScaleChangeModeBtn.onClick.AddListener(()=>ModeChange("Change"));
        PalleteBtn.onClick.AddListener(()=>SlidePallete());
        ChooseThicknessBtn.onClick.AddListener(()=>SlideThicknessBtn());
        UndoBtn.onClick.AddListener(Undo);
        RedoBtn.onClick.AddListener(Redo);
        for(int i =0 ; i <ToolBtns.Count ; i++){
            int temp = i;
            ToolBtns[temp].onClick.AddListener(()=>Slide_UP_Down(temp));
        }
        previousPosition = Vector3.zero;
        width = 0.1f;
    }
    void FixedUpdate() {
        CtrlPicutre();        
    }
    /// <summary>
    /// 활성화 될 때 clr변수를 자동으로 검은색으로 할당한다.
    /// </summary>
    void OnEnable() {
        clr = Color.black;    
        // clr = gradient.Evaluate(t);
    }
    /// <summary>
    /// PictureControl 모드일 때 터치 시 불러온 그림을 움직이거나 크기를 바꾸거나 각도를 조절할 수 있다.
    /// </summary>
    void CtrlPicutre(){
        if(mode == Mode.PictureControl) {
            switch (Input.touchCount){
                case 0:
                    previousPosition = Vector3.zero;
                    pretouchdis = 0;
                    break;
                case 1:
                    if(!(IsOnUI && 
                    UIHandler.Instance.HandedUI.tag == "Img")) break;
                    pretouchdis = 0;
                    Touch touch = Input.GetTouch(0);
                    Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    touchPosition.z = 0f;
                    // img.rectTransform.SetPositionAndRotation(touchPosition,Quaternion.identity);
                    if (previousPosition == Vector3.zero){
                        previousPosition = touchPosition;
                    }
                    if(Vector3.Distance(touchPosition, previousPosition) > 0.002f){
                        img.rectTransform.position += touchPosition - previousPosition;
                        previousPosition = touchPosition;
                    }
                    break;
                case 2:
                    if(!(//IsOnUI && 
                    UIHandler.Instance.HandedUI.tag == "Img")) break;
                    previousPosition = Vector3.zero;
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);
                    Vector3 currenPosition1 = Camera.main.ScreenToWorldPoint(touch1.position);
                    Vector3 currenPosition2 = Camera.main.ScreenToWorldPoint(touch2.position);
                    currenPosition1.z = 0f;
                    currenPosition2.z = 0f;
                    float touchdis = Vector2.Distance(currenPosition1,currenPosition2);
                    if (pretouchdis == 0){    
                        pretouchdis = touchdis;
                    } else{
                        float dislack = touchdis - pretouchdis;
                        // if(dislack > 0.2f){
                        //     dislack = dislack>5 ? 5 : dislack;
                        //     dislack *= 0.1f;
                        //     img.rectTransform.localScale += new Vector3(dislack,dislack,dislack);
                        // } else if(dislack < -0.2f){
                        //     dislack = dislack<-5 ? -5 : dislack;
                        //     dislack *= 0.1f;
                        //     img.rectTransform.localScale += new Vector3(dislack,dislack,dislack);
                        // }
                        if(dislack > 0.01f){
                            dislack = Mathf.Round(dislack*100) * 0.01f;
                            dislack = dislack > 0.4f? 0.4f : dislack;
                            img.rectTransform.localScale *= Mathf.Sqrt
                            (0.99f+dislack);
                        } else if(dislack < -0.01f){
                            dislack = Mathf.Round(dislack*100) * 0.01f;
                            dislack = dislack < -0.4f ? -0.4f : dislack;
                            img.rectTransform.localScale *= Mathf.Sqrt
                            (1.01f+dislack);
                        }
                        // if(img.rectTransform.localScale.x < 0){
                        //     img.rectTransform.localScale = new Vector3(0.05f,0.05f,0.05f);
                        // }
                        // if(img.rectTransform.localScale.x > 3){
                        //     img.rectTransform.localScale = new Vector3(3,3,3);
                        // }
                        if(img.rectTransform.localScale.x < 0.05f){
                            img.rectTransform.localScale = new Vector3(0.05f,0.05f* img.rectTransform.localScale.y /
                                                                                                 img.rectTransform.localScale.x ,0);
                        }
                        if(img.rectTransform.localScale.x > 10){
                            img.rectTransform.localScale = new Vector3(10,10* img.rectTransform.localScale.y /
                                                                                                 img.rectTransform.localScale.x ,1);
                        }
                        pretouchdis = touchdis;
                    }
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// 버튼에 의해 호출되는 함수다. clr변수를 검은색으로 할당한다.
    /// </summary>
    public void SeBlack(){
        PenUp_EraserDown();
        clr = Color.black;
        handle.GetComponent<Image>().color = clr;
    }
    /// <summary>
    /// 버튼에 의해 호출되는 함수다. clr변수를 하얀색으로 할당한다.
    /// </summary>
    public void SeWhite(){
        PenUp_EraserDown();
        modeerase = true;
        clr = Color.white;
        handle.GetComponent<Image>().color = clr;
    }
    /// <summary>
    /// 버튼에 의해 호출되는 함수다. clr변수를 
    /// Gradient의 값과 slider변수를 이용하여 얻은 색깔의 값을 얻는다.
    /// </summary>
    public void SeClr(Slider slider){
        PenUp_EraserDown();
        clr = gradient.Evaluate(slider.value);
        handle.GetComponent<Image>().color = clr;
    }
    /// <summary>
    /// 버튼에 의해 호출되는 함수다. 지우개 모드를 활성화 한다.
    /// </summary>
    public void ModeErase(){
        modeerase = true;
    }
    /// <summary>
    /// 버튼에 의해 호출되는 함수다. 지우개 모드를 비활성화 한다.
    /// </summary>
    public void notModeErase(){
        modeerase = false;
    }
    /// <summary>
    /// 굵기 조절하는 버튼을 핑크로 바꿔주는 함수이다. 나머지 버튼들은 회색으로 만든다.
    /// </summary>
    public void SetPink(int num){
        for(int i = 0; i < ThicknessBtns.Count; i++){
            if(i==num) ThicknessBtns[i].GetComponent<Image>().sprite = Pink;
            else ThicknessBtns[i].GetComponent<Image>().sprite = Gray;
        }
    }
    /// <summary>
    /// 그릴 Line의 PenType을 바꿔주는 함수이다.
    /// </summary>
    public void SetPenType(int i){
        switch(i){
            case 1:
                CurMat = NormalPen;
                LastPenType = 0;
                break;
            case 2:
                CurMat = BrushPen;
                LastPenType = 1;
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 버튼에 의해 호출되는 함수다. Draw모드 <-> PictureControl 모드
    /// </summary>
    public void ModeChange(string t_m = "P_C"){
        if(t_m.Equals("Change")){
            if(mode == Mode.Draw){
                Mode_Ctrl();
            }
            else
            {
                Mode_Draw();
            }
        }
        else if (t_m.Equals("Draw")){
            Mode_Draw();
        }
        else{
            Mode_Ctrl();
        }
    }
    /// <summary>
    /// 갤러리에서 사진을 가져올때 호출되는 함수. 지금 불러온 사진을 현재 조작하는 사진으로 한다.
    /// </summary>
    public void DigFromGalleryAfter(GameObject newImg){
        if(newImg != null){
            UIHandler.Instance.GetInHand_UI(newImg);
        }
        Mode_Ctrl();
    }
    /// <summary>
    /// DrawMode로 바꿀 때 UI들을 제어하기 위한 함수.
    /// </summary>
    void Mode_Draw(){
        mode = Mode.Draw;
        foreach (GameObject B in DrawBtns){
            B.gameObject.SetActive(true);
        }
        foreach (GameObject CM in CtrlModeUIs){
            CM.SetActive(false);
        }
        UIHandler.Instance.TwoBtnSet(false);
        previousPosition = Vector3.zero;
        pretouchdis = 0;
    }
    /// <summary>
    /// PictureControlMode로 바꿀 때 UI들을 제어하기 위한 함수.
    /// </summary>
    void Mode_Ctrl(){
        mode = Mode.PictureControl;
        foreach (GameObject B in DrawBtns){
            B.gameObject.SetActive(false);
        }
        foreach (GameObject CM in CtrlModeUIs){
            CM.SetActive(true);
        }
    }
    /// <summary>
    /// 펜 또는 브러시 버튼을 올리고 지우개 버튼 내리는 함수
    /// </summary>
    public void PenUp_EraserDown()
    {
        UpDown[LastPenType] = 1;
        UpDown[2] = 0;
        for(int j = LastPenType ; j < 3; j = j == LastPenType ? 2 : 3){
            Image ima = ToolBtns[j].GetComponent<Image>();
            Vector3[] temp = {
                new Vector3(ima.rectTransform.position.x , ima.rectTransform.position.y) ,
                new Vector3(ima.rectTransform.position.x , j*-75)
            };
            ima.rectTransform.DOPath( temp,
            0.2f, PathType.CatmullRom );
        }
    }
    /// <summary>
    /// 색을 바꾸기 위한 팔레트 버튼 UI가 Slide 효과를 내게 해주는 함수
    /// </summary>
    void SlidePallete(){
        int i = 0;
        PenUp_EraserDown();
        foreach (Transform child in PalleteBtn.transform){
            Image ima = child.GetComponent<Image>();
            Vector3[] temp = 
            {
            new Vector3(ima.rectTransform.position.x , PalleteBtn.GetComponent<Image>().rectTransform.position.y) ,
            new Vector3(ima.rectTransform.position.x , ima.rectTransform.position.y)
            };
            ima.rectTransform.SetPositionAndRotation(PalleteBtn.GetComponent<Image>().rectTransform.position,Quaternion.identity);
            ima.rectTransform.DOPath( temp,
                    0.2f, PathType.CatmullRom );
            i++;
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }
    /// <summary>
    /// 펜의 굵기를 바꾸기 위한 굵기 버튼 UI가 Slide 효과를 내게 해주는 함수
    /// </summary>
    void SlideThicknessBtn(){
        int i = 0;
        PenUp_EraserDown();
        foreach (Transform child in ChooseThicknessBtn.transform){
            Image ima = child.GetComponent<Image>();
            Vector3[] temp = 
            {
            new Vector3(ima.rectTransform.position.x , ChooseThicknessBtn.GetComponent<Image>().rectTransform.position.y) ,
            new Vector3(ima.rectTransform.position.x , ima.rectTransform.position.y)
            };
            ima.rectTransform.SetPositionAndRotation(ChooseThicknessBtn.GetComponent<Image>().rectTransform.position,Quaternion.identity);
            ima.rectTransform.DOPath( temp,
                    0.2f, PathType.CatmullRom );
            i++;
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }
    /// <summary>
    /// 펜 타입, 브러시 타입, 지우개 타입 버튼들이 선택/해제 될 때 Slide효과를 내게 해주는 함수
    /// </summary>
    void Slide_UP_Down(int n){
        for(int i =0 ; i <ToolBtns.Count; i++){
            Image ima = ToolBtns[i].GetComponent<Image>();
            if (i == n){
                if (UpDown[i] == 1){
                    // UpDown[i] = 0;
                    
                    // // ima.rectTransform.SetPositionAndRotation(
                    // //         new Vector3(ima.rectTransform.position.x ,
                    // //         -150)
                    // //     , Quaternion.identity
                    // // );
                    // Vector3[] temp = {
                    // new Vector3(ima.rectTransform.position.x , ima.rectTransform.position.y) ,
                    // new Vector3(ima.rectTransform.position.x , -150)};
                    
                    // ima.rectTransform.DOPath( temp,
                    // 0.2f, PathType.CatmullRom );
                }
                else{
                    UpDown[i] = 1;
                    // ima.rectTransform.SetPositionAndRotation(
                    //             new Vector3(ima.rectTransform.position.x ,
                    //             0)
                    //         , Quaternion.identity
                    //     );
                    // ima.rectTransform.DOMoveY(0 , 0.1f);
                    Vector3[] temp = {
                    new Vector3(ima.rectTransform.position.x , ima.rectTransform.position.y) ,
                    new Vector3(ima.rectTransform.position.x , 0)};
                    
                    ima.rectTransform.DOPath( temp,
                    0.2f, PathType.CatmullRom );
                }
            }
            else {
                UpDown[i] = 0;
                // ima.rectTransform.SetPositionAndRotation(
                //                 new Vector3(ima.rectTransform.position.x ,
                //                 -150)
                //             , Quaternion.identity
                //         );
                // ima.rectTransform.DOMoveY(-150 , 0.1f);
                Vector3[] temp = {
                    new Vector3(ima.rectTransform.position.x , ima.rectTransform.position.y) ,
                    new Vector3(ima.rectTransform.position.x , -150)};
                    
                    ima.rectTransform.DOPath( temp,
                    0.2f, PathType.CatmullRom );
            }
        }
    }
    /// <summary>
    /// 되돌리기 기능, Undo Button이 호출
    /// </summary>
    void Undo(){
        if(Lines[LinesIndex].state == "Draw"){
            Lines[LinesIndex].obj?.SetActive(false);
            Debug.Log(Lines[LinesIndex].obj?.name);
            LinesIndex -= 1;
        }
        else {
            Lines[LinesIndex].obj?.SetActive(true);
            LinesIndex -= 1;
        }
    }
    /// <summary>
    /// 앞으로 감기 기능, Redo Button이 호출
    /// </summary>
    void Redo(){
        if(Lines[LinesIndex+1].state == "Draw"){
            Lines[LinesIndex+1].obj?.SetActive(true);
            Debug.Log(Lines[LinesIndex+1].obj?.name);
            LinesIndex += 1;
        }
        else {
            Lines[LinesIndex+1].obj?.SetActive(false);
            LinesIndex += 1;
        }
    }
    /// <summary>
    /// 그림 그릴때 앞으로 감기 기능 모두 할당해제, Draw.cs에서 호출함
    /// </summary>
    public void RedoReset(){
        for ( int i = Lines.Count-1; i > LinesIndex; i-- ){
            // Destroy(Lines[i].obj);
            Lines.Remove(Lines[i]);
        }
    }
}
public class Memory{
    public GameObject obj;
    public string state;
    public Memory(GameObject _obj, string _state){
        obj  = _obj;
        state = _state;
    }
}
