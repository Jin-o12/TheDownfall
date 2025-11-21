using UnityEngine;
using TMPro;
using System.Collections.Generic;

public enum UIType
{
    MainMenu,
    Loading,
    InGame,
    GameOver,
    Settings,
    PauseMenu,
    SaveCheckNotice,
    SaveLoadingNotice,
    Record,
    Inventory
}

[System.Serializable]
public class UIConfig
{
    public UIType type;
    public GameObject uiObject;
    [Tooltip("이 UI가 활성화될 때 게임을 일시정지 시키려면 체크")]
    public bool isPause;
    [Tooltip("이 UI가 활성화될 때 마우스를 잠그려면 체크")]
    public bool mouseLock = false;
    [Tooltip("이 UI가 캔버스 역할을 한다면 체크")]
    public bool isCanvas = false;
}

public class UI
{
    #region UI 기본 정보
    public UIType type          { get; private set; }
    public GameObject uiObject  { get; private set; }
    public bool mouseLock       { get; private set; }
    public bool isCanvas        { get; private set; }
    private bool isPause;
    public bool isActive        { get; private set; }
    #endregion

    public UI(UIConfig config)
    {
        type = config.type;
        uiObject = config.uiObject;
        mouseLock = config.mouseLock;
        isCanvas = config.isCanvas;
        isPause = config.isPause;
        
        // 생성 시점에는 항상 비활성화
        isActive = false;
        uiObject.SetActive(false);
    }

    /* 해당 UI 활성화 */
    public void Open()
    {
        // UI 활성화
        isActive = true;
        uiObject.SetActive(true);

        // 퍼즈 상태
        if(isPause)
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
        }

        // 마우스 잠금 상태
        if(mouseLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        Debug.Log($"{type} UI 열림");
    }

    /* UI 비활성화 */
    public void Close()
    {
        isActive = false;
        uiObject.SetActive(false);
    }

    public bool GetIsPause() { return isPause; }
}

public class UIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private List<UIConfig> uiConfigs = new List<UIConfig>();

    #region 오브젝트
    [Header("인게임 오브젝트")]
    public GameObject Character;
    public GameObject SpawnManager;
    public GameObject mainScreenCamera;
    #endregion

    #region 게임 결과창 UI
    [Header("게임 결과창 UI")]
    [SerializeField] public TMP_Text gameResultLeft;
    [SerializeField] public TMP_Text gameResultRight;
    [SerializeField] public TMP_Text gameResultCenter;
    #endregion

    #region 외부 참조 스크립트
    [Header("기타 스크립트")]
    public KeyBinding keyBinding;
    public GameLoader gameLoader;
    #endregion

    #region Private 변수들 (Non-Inspector)

    /* 게임 UI 스택과 상태애 대한 변수 */
    private Stack<UI> uiStack = new Stack<UI>();                                // UI 흐름을 관리할 스택
    private Dictionary<UIType, UI> uiDictionary = new Dictionary<UIType, UI>(); // UI객체 저장 및 검색을 위한 딕셔너리
    private bool isPause;       // 일시정지 상태 여부
    private int pauseStackNum;  // 일시정지 상태를 시작한 지점의 스택 번호 (0은 값 없음 상태)
    public bool GetIsPause() { return isPause; }

    #endregion // Private 변수들 Variables (Non-Inspector)

    void Awake()
    {
        uiStack.Clear();
        pauseStackNum = 0;
        isPause = false;

        // List로 받은 객체들을 Dictionaty로 변환하여 저장
        foreach (UIConfig config in uiConfigs)
        {
            if (config.uiObject == null)
            {
                Debug.LogWarning($"{config.type} 오브젝트가 할당되지 않음");
                continue;
            }

            if (uiDictionary.ContainsKey(config.type))
            {
                Debug.LogWarning($"{config.type} 중복");
                continue;
            }
            config.uiObject.SetActive(false);
        
            UI newUI = new UI(config); 
            uiDictionary.Add(config.type, newUI);
        }   
    }

    void Start()
    {
        if (uiDictionary.ContainsKey(UIType.MainMenu))
            PushUI(UIType.MainMenu);
        else if (uiDictionary.Count > 0)
            PushUI(uiDictionary.Keys.GetEnumerator().Current);
    }

    void Update()
    {
        CheckUIError();
    }

    public void CheckUIError()
    {
        if(uiStack.Count == 0)
        {
            Debug.LogError("활성화 된 UI가 없습니다.");
            return;
        }
    }

    public void PushUI(UIType type)
    {
        UpdatePause();
        if (!uiDictionary.ContainsKey(type))
        {
            Debug.LogError($"\"{type}\" UI를 찾을 수 없습니다.");
            return;
        }

        if(uiStack.Count >= 1 && uiDictionary[type].isCanvas)
        {
            uiStack.Peek().Close();
            uiStack.Clear();
            uiStack.Push(uiDictionary[type]);
            uiDictionary[type].Open();
            UpdatePause();
            UpdateMouse();
            return;
        }
        
        // 첫 팝업이 아니라면, 직전 팝업을 비활성화
        if (uiStack.Count > 1)
        {
            UI previousUI = uiStack.Peek();
            previousUI.Close(); 
        }
        
        // 원하는 UI 활성화 및 스택에 추가
        UI newUI = uiDictionary[type];
        newUI.Open(); // 새 UI 열기 (TimeScale, Cursor 등 설정 포함)
        uiStack.Push(newUI);
        
        UpdateMouse();
    }

    public void PopUI()
    {   
        UpdatePause();
        if (uiStack.Count == 0) return;
        
        UI currentUI = uiStack.Pop();
        currentUI.Close();
        
        // 스택에 남은 UI가 있다면 다시 활성화
        if (uiStack.Count > 0)
        {
            UI previousUI = uiStack.Peek();
            previousUI.Open();
        }
        else
        {
            Time.timeScale = 1f;
        }
        UpdateMouse();
    }

    private void UpdatePause()
    {
        if(uiStack.Count > 0 && isPause)
        {
            // 퍼즈 상태에서 벗어나는지 체크
            if(uiStack.Count < pauseStackNum)
            {
                isPause = false;
                pauseStackNum = 0;
                return;
            }
        }
        else if(uiStack.Count > 0 && !isPause)
        {
            // 스택이 비었을 때
            if(uiStack.Count == 0)
            {
                isPause = false;
                return;
            }

            // 첫 퍼즈 상태 진입
            if(uiStack.Peek().GetIsPause())
            {
                isPause =  true;
                pauseStackNum = uiStack.Count;
                return;
            }
        }
    }

    private void UpdateMouse()
    {
        if(uiStack.Peek().mouseLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
