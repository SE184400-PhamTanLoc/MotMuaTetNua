using UnityEngine;
using UnityEngine.UIElements;

public enum ComputerUIState
{
    Desktop,        // Màn hình desktop
    Notification,   // Thông báo "Bạn có tin nhắn mới"
    MessageApp,     // App tin nhắn (sẽ implement sau)
    WorkApp         // App công việc (cần hoàn thành)
}

public class ComputerUIManager : MonoBehaviour
{
    [Header("UI Toolkit References")]
    public UIDocument uiDocument; // Gán UIDocument component vào đây
    
    [Header("Settings")]
    public KeyCode openMessageAppKey = KeyCode.Space; // Phím để mở MessageApp từ notification
    public KeyCode closeComputerKey = KeyCode.Escape; // Phím để đóng máy tính (khi task hoàn thành)
    
    [Header("Debug/Test")]
    public KeyCode completeTaskKey = KeyCode.T; // Phím để fake completion (test)
    public bool useFakeCompletion = true; // Bật/tắt fake completion
    public bool autoCloseAfterFakeCompletion = false; // Tự động đóng máy tính sau khi fake completion (tắt mặc định)
    
    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip firstMessageNotificationClip;
    [Range(0f, 1f)] public float firstMessageNotificationVolume = 0.7f;
    
    // UI Toolkit Elements (sẽ query từ UXML)
    private VisualElement rootElement;
    private VisualElement desktopPanel;
    private VisualElement notificationPanel;
    private VisualElement messageAppPanel;
    private VisualElement workAppPanel;
    private Button closeComputerButton;
    private Button closeMessageButton;
    private Button completeWorkButton;
    private Button closeWorkButton;
    
    // Desktop app icons
    private VisualElement messageAppIcon;
    private VisualElement workAppIcon;
    
    // Taskbar elements
    private VisualElement taskbar;
    private VisualElement taskbarIconsContainer;
    private Label clockLabel;
    
    // Taskbar app icons (không còn dùng logic ẩn/hiện)
    // private VisualElement taskbarMessageAppIcon;
    // private VisualElement taskbarWorkAppIcon;
    
    private ComputerUIState currentUIState = ComputerUIState.Desktop;
    private bool isTaskCompleted = false;
    private bool hasEnteredWorkApp = false; // Track xem đã vào WorkApp chưa
    private bool firstNotificationSoundPlayed = false;
    private NarrativeTextController narrativeController;
    private ChatUIController chatUIController;
    private WorkAppUIController workAppUIController;
    
    // MessageApp chat logic đã được ChatUIController quản lý - không cần biến ở đây nữa
    
    void Start()
    {
        // Cache reference để tránh FindObjectOfType mỗi frame
        narrativeController = FindFirstObjectByType<NarrativeTextController>();
        chatUIController = FindFirstObjectByType<ChatUIController>();
        workAppUIController = FindFirstObjectByType<WorkAppUIController>();
        
        // Tự động tìm UIDocument nếu reference bị null (fallback)
        if (uiDocument == null)
        {
            uiDocument = FindFirstObjectByType<UIDocument>();
            // Không cần log vì đã có fallback trong OpenComputer()
        }
        
        // Đảm bảo UIDocument component disabled từ đầu (giống dialogText)
        // KHÔNG disable GameObject, chỉ disable component
        if (uiDocument != null)
        {
            uiDocument.enabled = false;
        }
        
        // KHÔNG initialize UI Toolkit ở đây vì UIDocument đang disabled
        // Sẽ initialize khi OpenComputer() được gọi lần đầu
        EnsureUIAudioSource();
        EnsureSfxChannelVolume(uiAudioSource);
    }
    
    private void InitializeUIToolkit()
    {
        if (uiDocument == null)
        {
            Debug.LogError("ComputerUIManager: UIDocument chưa được gán! Hãy gán trong Inspector.");
            return;
        }
        
        // Đảm bảo UIDocument component đang enabled để có thể query elements
        if (!uiDocument.enabled)
        {
            Debug.LogWarning("ComputerUIManager: UIDocument component đang disabled. Sẽ được enable khi OpenComputer().");
            return;
        }
        
        // Đảm bảo GameObject đang active
        if (!uiDocument.gameObject.activeInHierarchy)
        {
            uiDocument.gameObject.SetActive(true);
        }
        
        // Lấy root element
        rootElement = uiDocument.rootVisualElement;
        
        // Query các elements từ UXML bằng name
        desktopPanel = rootElement.Q<VisualElement>("DesktopPanel");
        
        // NotificationPanel nằm trong DesktopView template (DesktopView.uxml)
        // Cần query từ desktopPanel vì nó nằm trong template instance
        if (desktopPanel != null)
        {
            notificationPanel = desktopPanel.Q<VisualElement>("NotificationPanel");
        }
        else
        {
            // Fallback: Query từ rootElement
            notificationPanel = rootElement.Q<VisualElement>("NotificationPanel");
        }
        
        messageAppPanel = rootElement.Q<VisualElement>("MessageAppPanel");
        workAppPanel = rootElement.Q<VisualElement>("WorkAppPanel");
        closeComputerButton = rootElement.Q<Button>("CloseComputerButton");
        closeMessageButton = rootElement.Q<Button>("CloseMessageButton");
        completeWorkButton = rootElement.Q<Button>("CompleteWorkButton");
        closeWorkButton = rootElement.Q<Button>("CloseWorkButton");
        
        // Query MessageApp chat list elements (KHÔNG setup click handlers - ChatUIController sẽ quản lý)
        // ChatUIController sẽ tự query và setup click handlers
        
        // Query desktop app icons
        messageAppIcon = rootElement.Q<VisualElement>("MessageAppIcon");
        workAppIcon = rootElement.Q<VisualElement>("WorkAppIcon");
        
        // Query taskbar elements (nằm trong DesktopView template)
        if (desktopPanel != null)
        {
            taskbar = desktopPanel.Q<VisualElement>("Taskbar");
            taskbarIconsContainer = desktopPanel.Q<VisualElement>("TaskbarIconsContainer");
            clockLabel = desktopPanel.Q<Label>("ClockLabel");
            
            // Query StartButton để thêm click handler (sau này sẽ thay bằng icon tắt)
            VisualElement startButton = desktopPanel.Q<VisualElement>("StartButton");
            if (startButton != null)
            {
                startButton.RegisterCallback<ClickEvent>(evt => OnStartButtonClicked());
            }
        }
        else
        {
            // Fallback: Query từ rootElement
            taskbar = rootElement.Q<VisualElement>("Taskbar");
            taskbarIconsContainer = rootElement.Q<VisualElement>("TaskbarIconsContainer");
            clockLabel = rootElement.Q<Label>("ClockLabel");
            
            VisualElement startButton = rootElement.Q<VisualElement>("StartButton");
            if (startButton != null)
            {
                startButton.RegisterCallback<ClickEvent>(evt => OnStartButtonClicked());
            }
        }
        
        // Taskbar icons không còn cần query (không có logic ẩn/hiện)
        
        // Setup button callbacks
        if (closeComputerButton != null)
        {
            closeComputerButton.clicked += CloseComputer;
        }
        
        if (closeMessageButton != null)
        {
            closeMessageButton.clicked += CloseMessageApp;
        }
        
        if (completeWorkButton != null)
        {
            completeWorkButton.clicked += CompleteWorkTask;
        }
        
        if (closeWorkButton != null)
        {
            closeWorkButton.clicked += CloseWorkApp;
        }
        
        // Setup desktop app icon click callbacks
        if (messageAppIcon != null)
        {
            messageAppIcon.RegisterCallback<ClickEvent>(evt => OnMessageAppIconClicked());
            // Hover effects
            messageAppIcon.RegisterCallback<MouseEnterEvent>(evt => OnIconHoverEnter(messageAppIcon));
            messageAppIcon.RegisterCallback<MouseLeaveEvent>(evt => OnIconHoverExit(messageAppIcon));
        }
        
        if (workAppIcon != null)
        {
            workAppIcon.RegisterCallback<ClickEvent>(evt => OnWorkAppIconClicked());
            // Hover effects
            workAppIcon.RegisterCallback<MouseEnterEvent>(evt => OnIconHoverEnter(workAppIcon));
            workAppIcon.RegisterCallback<MouseLeaveEvent>(evt => OnIconHoverExit(workAppIcon));
        }
        
        // Setup taskbar (Start button không có chức năng)
        
        // Update clock
        UpdateClock();
        
        // Taskbar icons sẽ luôn hiển thị (không có logic ẩn/hiện)
        
        // Ẩn UI ban đầu (đảm bảo không hiện trong Scene view)
        if (rootElement != null)
        {
            rootElement.style.display = DisplayStyle.None;
        }
    }
    
    void Update()
    {
        // Chỉ xử lý khi đang ở ComputerActive
        if (GameFlow.Instance == null)
        {
            return;
        }
        
        if (!GameFlow.Instance.IsState(GameState.ComputerActive))
        {
            return;
        }
        
        // QUY TẮC: Dialog box luôn ưu tiên hơn UI máy tính
        // Nếu dialog box đang hiện, ComputerUIManager không xử lý input
        if (narrativeController != null && narrativeController.IsDialogActive)
        {
            return;
        }
        
        // Xử lý input dựa trên state hiện tại
        HandleInput();
    }
    
    private void InitializeUI()
    {
        // Ẩn tất cả panels ban đầu (sử dụng DisplayStyle.None cho UI Toolkit)
        if (desktopPanel != null) desktopPanel.style.display = DisplayStyle.None;
        if (notificationPanel != null) notificationPanel.style.display = DisplayStyle.None;
        if (messageAppPanel != null) messageAppPanel.style.display = DisplayStyle.None;
        if (workAppPanel != null) workAppPanel.style.display = DisplayStyle.None;
        
        // Disable close button ban đầu
        if (closeComputerButton != null)
        {
            closeComputerButton.SetEnabled(false);
            closeComputerButton.style.display = DisplayStyle.None;
        }
        
        // Ẩn close work button ban đầu (chỉ hiện sau khi hoàn thành task)
        if (closeWorkButton != null)
        {
            closeWorkButton.style.display = DisplayStyle.None;
        }
    }
    
    private void HandleInput()
    {
        // Đóng máy tính bằng ESC (chỉ khi task đã hoàn thành)
        if (Input.GetKeyDown(closeComputerKey) && isTaskCompleted)
        {
            Debug.Log("ESC pressed - Closing computer");
            CloseComputer();
            return;
        }
        
        // Fake completion bằng phím T (để test) - hoạt động trong mọi state
        if (Input.GetKeyDown(completeTaskKey))
        {
            Debug.Log($"T key pressed! useFakeCompletion: {useFakeCompletion}, isTaskCompleted: {isTaskCompleted}");
            
            if (useFakeCompletion)
            {
                CompleteWorkTask();
                Debug.Log("Fake completion triggered by T key");
                
                // Tự động đóng máy tính sau khi fake completion (nếu bật)
                if (autoCloseAfterFakeCompletion)
                {
                    Debug.Log("Auto-closing computer after fake completion");
                    CloseComputer();
                }
            }
            else
            {
                Debug.LogWarning("Fake completion is disabled! Enable 'useFakeCompletion' in Inspector.");
            }
            return;
        }
        
        switch (currentUIState)
        {
            case ComputerUIState.Notification:
                // Nhấn Space để chuyển sang MessageApp
                if (Input.GetKeyDown(openMessageAppKey))
                {
                    OpenMessageApp();
                }
                break;
                
            case ComputerUIState.MessageApp:
                // Chỉ cho phép thoát MessageApp khi đã hoàn thành luồng chat
                UpdateMessageAppCloseButtonState();
                break;
                
            case ComputerUIState.WorkApp:
                // Logic cho WorkApp sẽ implement sau
                break;
        }
    }

    private bool CanCloseMessageApp()
    {
        if (chatUIController == null)
        {
            // Fallback an toàn nếu thiếu reference
            return true;
        }

        return chatUIController.IsChatFlowDone();
    }

    private void UpdateMessageAppCloseButtonState()
    {
        if (closeMessageButton == null) return;

        bool canClose = CanCloseMessageApp();
        closeMessageButton.SetEnabled(canClose);
        closeMessageButton.style.opacity = canClose ? 1f : 0.5f;
    }
    
    public void OpenComputer()
    {
        // Fallback: Tự tìm UIDocument nếu reference bị null
        if (uiDocument == null)
        {
            uiDocument = FindFirstObjectByType<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("ComputerUIManager: UIDocument chưa được gán và không tìm thấy trong scene! Hãy đảm bảo ComputerDocument GameObject có trong scene.");
                return;
            }
            // Code đã tự động tìm và gán, không cần warning nữa
        }
        
        // Đảm bảo GameObject đang active
        if (!uiDocument.gameObject.activeInHierarchy)
        {
            uiDocument.gameObject.SetActive(true);
        }
        
        // Enable UIDocument component và initialize nếu chưa
        if (!uiDocument.enabled)
        {
            uiDocument.enabled = true;
            InitializeUIToolkit();
            InitializeUI();
        }
        // Nếu đã enabled nhưng chưa initialize (lần đầu)
        else if (rootElement == null)
        {
            InitializeUIToolkit();
            InitializeUI();
        }

        if (rootElement != null)
        {
            rootElement.style.display = DisplayStyle.Flex;
        }

        currentUIState = ComputerUIState.Desktop;
        isTaskCompleted = false;
        hasEnteredWorkApp = false; // Reset flag khi mở máy tính
        firstNotificationSoundPlayed = false;

        ShowDesktop();
        ShowNotification(); // Hiển thị notification trước
        
        Debug.Log("Mở UI máy tính");
    }
    
    public void CloseComputer()
    {
        if (!isTaskCompleted)
        {
            Debug.Log("Chưa thể đóng máy tính: Task chưa hoàn thành!");
            return;
        }

        if (rootElement != null)
        {
            rootElement.style.display = DisplayStyle.None;
        }

        if (uiDocument != null)
        {
            uiDocument.enabled = false;
        }

        // Reset state
        currentUIState = ComputerUIState.Desktop;
        InitializeUI();
        
        Debug.Log("Đóng UI máy tính");

        if (GameFlow.Instance != null)
        {
            GameFlow.Instance.ChangeState(GameState.ComputerFinished);
        }
    }
    
    // Hiển thị Desktop
    private void ShowDesktop()
    {
        if (desktopPanel != null)
        {
            desktopPanel.style.display = DisplayStyle.Flex;
        }
        
        // Ẩn các app panels
        if (messageAppPanel != null)
            messageAppPanel.style.display = DisplayStyle.None;
        if (workAppPanel != null)
            workAppPanel.style.display = DisplayStyle.None;
        
        // Chỉ set state Desktop nếu không đang ở Notification
        if (currentUIState != ComputerUIState.Notification)
        {
            currentUIState = ComputerUIState.Desktop;
        }
        
        // Update Start button visual (sáng lên nếu có thể đóng máy tính)
        UpdateStartButtonVisual();
    }
    
    // Update visual của nút Start (hình tròn rỗng: viền xám mặc định, viền đỏ khi có thể tắt máy)
    private void UpdateStartButtonVisual()
    {
        VisualElement startButton = rootElement?.Q<VisualElement>("StartButton");
        if (startButton != null)
        {
            if (isTaskCompleted && currentUIState == ComputerUIState.Desktop)
            {
                startButton.AddToClassList("start-button-can-close");
            }
            else
            {
                startButton.RemoveFromClassList("start-button-can-close");
            }
        }
    }
    
    // Hiển thị Notification với animation slide từ dưới lên
    private void ShowNotification()
    {
        if (notificationPanel != null)
        {
            currentUIState = ComputerUIState.Notification;
            TryPlayFirstNotificationSound();
            
            // Vô hiệu hóa desktop icons khi ở Notification (không thể click)
            if (messageAppIcon != null)
            {
                messageAppIcon.SetEnabled(false);
            }
            if (workAppIcon != null)
            {
                workAppIcon.SetEnabled(false);
            }
            
            // Bắt đầu animation slide từ dưới lên (display sẽ được set trong coroutine)
            StartCoroutine(SlideNotificationIn());
            StartCoroutine(PlayFirstNotificationSoundDelayed(0.08f));
            
            Debug.Log("Notification hiển thị - Chỉ có thể ấn Space");
        }
    }
    
    private void TryPlayFirstNotificationSound()
    {
        if (firstNotificationSoundPlayed)
        {
            return;
        }

        PlayUIClip(firstMessageNotificationClip, firstMessageNotificationVolume);
        firstNotificationSoundPlayed = true;
    }
    
    private System.Collections.IEnumerator PlayFirstNotificationSoundDelayed(float delaySeconds)
    {
        if (firstNotificationSoundPlayed) yield break;
        yield return new WaitForSeconds(delaySeconds);
        TryPlayFirstNotificationSound();
    }
    
    private void PlayUIClip(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning($"[ComputerUI] firstMessageNotificationClip chưa được gán | object={gameObject.name} | scene={gameObject.scene.name}");
            return;
        }

        EnsureUIAudioSource();
        EnsureSfxChannelVolume(uiAudioSource);

        if (uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
            return;
        }

        if (Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }

    private void EnsureUIAudioSource()
    {
        if (uiAudioSource != null)
        {
            return;
        }

        uiAudioSource = GetComponent<AudioSource>();
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
            uiAudioSource.spatialBlend = 0f;
            uiAudioSource.volume = 1f;
            uiAudioSource.mute = false;
        }
    }

    private void EnsureSfxChannelVolume(AudioSource source)
    {
        if (source == null) return;

        AudioChannelVolume channelVolume = source.GetComponent<AudioChannelVolume>();
        if (channelVolume == null)
        {
            channelVolume = source.gameObject.AddComponent<AudioChannelVolume>();
        }

        channelVolume.channel = AudioChannelType.Sfx;
        channelVolume.useAudioSourceVolumeAsBaseOnAwake = false;
        channelVolume.baseVolume = source.volume;
        channelVolume.ApplyCurrentVolume();
    }
    
    // Animation slide Notification từ dưới lên
    private System.Collections.IEnumerator SlideNotificationIn()
    {
        if (notificationPanel == null) yield break;
        
        float animationDuration = 0.5f; // Thời gian animation (0.5 giây)
        
        // Lấy vị trí cuối cùng từ style hiện tại (hoặc từ UXML)
        float endY = 881f; // Vị trí kết thúc (từ UXML: top: 881px)
        float endX = 1362f; // Vị trí cuối cùng (từ UXML: left: 1362px)
        
        // Vị trí bắt đầu (dưới màn hình - thêm 200px để nó ở ngoài màn hình)
        float startY = endY + 200f;
        
        // Set vị trí ban đầu (dưới màn hình)
        notificationPanel.style.top = startY;
        notificationPanel.style.left = endX;
        
        // Đảm bảo panel hiển thị trước khi animate (và fade in)
        notificationPanel.style.display = DisplayStyle.Flex;
        notificationPanel.style.opacity = 0f; // Bắt đầu với opacity = 0
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Ease out animation (nhanh đầu, chậm cuối)
            t = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease out
            
            // Animate position (slide từ dưới lên)
            float currentY = Mathf.Lerp(startY, endY, t);
            notificationPanel.style.top = currentY;
            
            // Animate opacity (fade in) - USS transition sẽ tự động xử lý, nhưng ta cũng có thể control bằng code
            notificationPanel.style.opacity = t;
            
            yield return null;
        }
        
        // Đảm bảo vị trí và opacity cuối cùng chính xác
        notificationPanel.style.top = endY;
        notificationPanel.style.opacity = 1f;
    }
    
    // Ẩn Notification
    private void HideNotification()
    {
        if (notificationPanel != null)
        {
            notificationPanel.style.display = DisplayStyle.None;
        }
        
        // Enable lại desktop icons khi ẩn notification
        if (messageAppIcon != null && !hasEnteredWorkApp)
        {
            messageAppIcon.SetEnabled(true);
        }
        if (workAppIcon != null)
        {
            workAppIcon.SetEnabled(true);
        }
    }
    
    // Mở MessageApp (từ Notification hoặc Desktop icon)
    public void OpenMessageApp()
    {
        Debug.Log("[OpenMessageApp] Bắt đầu mở MessageApp");
        HideNotification();
        
        if (messageAppPanel != null)
        {
            desktopPanel.style.display = DisplayStyle.None;
            messageAppPanel.style.display = DisplayStyle.Flex;
            currentUIState = ComputerUIState.MessageApp;
            
            // Reset về chat mặc định (Mẹ) - OLD LOGIC (sẽ bị thay thế bởi ChatUIController)
            // ResetMessageAppToDefault();
            
            // Notify ChatUIController
            if (chatUIController != null)
            {
                chatUIController.OnMessageAppOpened();
            }

            UpdateMessageAppCloseButtonState();
            
            // Debug: Log kích thước sau khi hiển thị (delay 1 frame để layout được tính toán)
            Debug.Log("[OpenMessageApp] Bắt đầu coroutine DebugUISizes");
            Debug.Log($"[OpenMessageApp] GameObject active: {gameObject.activeInHierarchy}, enabled: {enabled}");
            StartCoroutine(DebugUISizes());
            
            // Debug ngay lập tức (không đợi frame)
            DebugUISizesImmediate();
        }
        else
        {
            Debug.LogError("[OpenMessageApp] messageAppPanel is NULL!");
        }
        
        Debug.Log("Mở MessageApp");
    }
    
    // Desktop app icon click handlers
    private void OnMessageAppIconClicked()
    {
        Debug.Log("[Desktop] MessageAppIcon clicked!");
        ShowMessageApp();
    }
    
    private void OnWorkAppIconClicked()
    {
        Debug.Log("[Desktop] WorkAppIcon clicked!");
        ShowWorkApp();
    }
    
    // Hover effects cho app icons
    private void OnIconHoverEnter(VisualElement icon)
    {
        if (icon == null) return;
        
        // Thêm background color nhẹ khi hover
        icon.style.backgroundColor = new Color(1f, 1f, 1f, 0.15f);
    }
    
    private void OnIconHoverExit(VisualElement icon)
    {
        if (icon == null) return;
        
        // Reset background color
        icon.style.backgroundColor = Color.clear;
    }
    
    // Show MessageApp (từ Desktop icon) - chỉ cho phép khi chưa vào WorkApp
    private void ShowMessageApp()
    {
        // Chỉ cho phép mở MessageApp khi chưa vào WorkApp
        if (hasEnteredWorkApp)
        {
            Debug.Log("Đã vào WorkApp, không thể mở lại MessageApp!");
            return;
        }
        
        if (messageAppPanel != null)
        {
            desktopPanel.style.display = DisplayStyle.None;
            messageAppPanel.style.display = DisplayStyle.Flex;
            currentUIState = ComputerUIState.MessageApp;
            
            // Reset về chat mặc định (Mẹ) - OLD LOGIC (sẽ bị thay thế bởi ChatUIController)
            // ResetMessageAppToDefault();
            
            // Notify ChatUIController
            if (chatUIController != null)
            {
                chatUIController.OnMessageAppOpened();
            }

            UpdateMessageAppCloseButtonState();
            
            // Debug: Log kích thước sau khi hiển thị (delay 1 frame để layout được tính toán)
            Debug.Log("[ShowMessageApp] Bắt đầu coroutine DebugUISizes");
            StartCoroutine(DebugUISizes());
            
            // Debug ngay lập tức (không đợi frame)
            DebugUISizesImmediate();
        }
        
        Debug.Log("Mở MessageApp từ Desktop");
    }
    
    // Show WorkApp (từ Desktop icon) - đánh dấu đã vào WorkApp
    private void ShowWorkApp()
    {
        if (workAppPanel != null)
        {
            desktopPanel.style.display = DisplayStyle.None;
            workAppPanel.style.display = DisplayStyle.Flex;
            currentUIState = ComputerUIState.WorkApp;

            // Notify WorkApp controller để init UI khi panel đã visible
            if (workAppUIController != null)
            {
                workAppUIController.OnWorkAppOpened();
            }
        }
        
        // Đánh dấu đã vào WorkApp (không thể quay lại MessageApp)
        hasEnteredWorkApp = true;
        
        // Vô hiệu hóa MessageApp icon trên desktop
        if (messageAppIcon != null)
        {
            messageAppIcon.SetEnabled(false);
            messageAppIcon.style.opacity = 0.5f; // Làm mờ icon
        }
        
        Debug.Log("Mở WorkApp từ Desktop - Đã khóa MessageApp");
    }
    
    // Đóng MessageApp (chỉ quay về Desktop, không tự động mở WorkApp)
    public void CloseMessageApp()
    {
        if (!CanCloseMessageApp())
        {
            Debug.Log("Chưa thể đóng MessageApp: Hãy hoàn thành luồng chat trước.");
            return;
        }

        if (messageAppPanel != null)
        {
            messageAppPanel.style.display = DisplayStyle.None;
        }
        
        // Chỉ quay về Desktop, không tự động mở WorkApp
        // Người chơi có thể vào lại MessageApp từ icon hoặc vào WorkApp
        ShowDesktop();
        
        Debug.Log("Đóng MessageApp, quay về Desktop");
    }
    
    // XÓA: Logic chat đã được ChatUIController quản lý
    // Không cần OnChatItemClicked, UpdateChatListSelection, UpdateChatContent, ResetMessageAppToDefault nữa
    
    // Được gọi khi WorkApp task hoàn thành
    public void CompleteWorkTask()
    {
        if (isTaskCompleted) return;
        
        isTaskCompleted = true;
        
        // Hiện close button để đóng WorkApp và quay về Desktop
        if (closeWorkButton != null)
        {
            closeWorkButton.style.display = DisplayStyle.Flex;
            Debug.Log("CloseWorkButton enabled - Có thể đóng WorkApp");
        }
        
        // Update Start button visual (sẽ sáng lên khi quay về Desktop)
        UpdateStartButtonVisual();
        
        Debug.Log("WorkApp task hoàn thành! Có thể đóng WorkApp và quay về Desktop, sau đó ấn Start để đóng máy tính.");
    }
    
    // Đóng WorkApp và quay về Desktop (sau khi hoàn thành task)
    public void CloseWorkApp()
    {
        // Chỉ cho phép đóng khi task đã hoàn thành
        if (!isTaskCompleted)
        {
            Debug.Log("Chưa thể đóng WorkApp: Task chưa hoàn thành!");
            return;
        }
        
        if (workAppPanel != null)
        {
            workAppPanel.style.display = DisplayStyle.None;
        }
        
        // Quay về Desktop
        ShowDesktop();
        
        // Enable Start button để đóng máy tính (sau này sẽ thay bằng icon tắt)
        // Start button sẽ tự động enable khi task hoàn thành
        
        Debug.Log("Đóng WorkApp, quay về Desktop - Có thể ấn Start để đóng máy tính");
    }
    
    // Reset khi cần (nếu có restart game)
    public void ResetTask()
    {
        isTaskCompleted = false;
        currentUIState = ComputerUIState.Desktop;
        InitializeUI();
    }
    
    // ========== TASKBAR FUNCTIONS ==========
    
    // Set đồng hồ (chỉ set một lần, không cập nhật real-time)
    private void UpdateClock()
    {
        if (clockLabel == null) return;
        
        // Set thời gian mặc định là 19:30 (không cập nhật real-time)
        clockLabel.text = "19:30";
    }
    
    // Taskbar icons logic đã bị bỏ - icons sẽ luôn hiển thị trong UXML
    
    
    // Getter để check state hiện tại (có thể dùng cho các component khác)
    public ComputerUIState GetCurrentState()
    {
        return currentUIState;
    }
    
    // Getter để check task completion
    public bool IsTaskCompleted()
    {
        return isTaskCompleted;
    }
    
    // Getter để check xem có đang ở Notification state không (cho CursorStateController)
    public bool IsNotificationState()
    {
        return currentUIState == ComputerUIState.Notification;
    }
    
    // Start button click handler (sau này sẽ thay bằng icon tắt)
    private void OnStartButtonClicked()
    {
        // Chỉ cho phép đóng máy tính khi task đã hoàn thành và đang ở Desktop
        if (isTaskCompleted && currentUIState == ComputerUIState.Desktop)
        {
            CloseComputer();
            Debug.Log("Start button clicked - Đóng máy tính");
        }
        else if (!isTaskCompleted)
        {
            Debug.Log("Chưa thể đóng máy tính: Task chưa hoàn thành!");
        }
        else
        {
            Debug.Log("Chỉ có thể đóng máy tính khi đang ở Desktop!");
        }
    }
    
    // Debug method: Log kích thước của tất cả UI elements quan trọng
    private System.Collections.IEnumerator DebugUISizes()
    {
        Debug.Log("[DebugUISizes] Coroutine bắt đầu");
        
        // Đợi 1 frame để layout được tính toán xong
        yield return null;
        
        Debug.Log("[DebugUISizes] Sau yield null, bắt đầu log");
        Debug.Log("=== DEBUG UI SIZES ===");
        Debug.Log($"Screen Size: {Screen.width} x {Screen.height}");
        Debug.Log($"Screen Resolution: {Screen.currentResolution}");
        
        if (uiDocument != null)
        {
            Debug.Log($"UIDocument Panel Settings: {uiDocument.panelSettings?.name}");
            if (uiDocument.panelSettings != null)
            {
                Debug.Log($"  - Scale Mode: {uiDocument.panelSettings.scaleMode}");
                Debug.Log($"  - Reference Resolution: {uiDocument.panelSettings.referenceResolution}");
                Debug.Log($"  - Screen Match Mode: {uiDocument.panelSettings.screenMatchMode}");
            }
        }
        
        if (rootElement != null)
        {
            Debug.Log($"ComputerUIRoot:");
            Debug.Log($"  - Resolved: {rootElement.resolvedStyle.width} x {rootElement.resolvedStyle.height}");
            Debug.Log($"  - Layout: {rootElement.layout.width} x {rootElement.layout.height}");
            Debug.Log($"  - Style Width: {rootElement.style.width}");
            Debug.Log($"  - Style Height: {rootElement.style.height}");
        }
        
        if (messageAppPanel != null)
        {
            Debug.Log($"MessageAppPanel:");
            Debug.Log($"  - Resolved: {messageAppPanel.resolvedStyle.width} x {messageAppPanel.resolvedStyle.height}");
            Debug.Log($"  - Layout: {messageAppPanel.layout.width} x {messageAppPanel.layout.height}");
            Debug.Log($"  - Style Width: {messageAppPanel.style.width}");
            Debug.Log($"  - Style Height: {messageAppPanel.style.height}");
            Debug.Log($"  - Position: {messageAppPanel.resolvedStyle.left}, {messageAppPanel.resolvedStyle.top}");
            Debug.Log($"  - Display: {messageAppPanel.style.display}");
        }
        
        // Query MessageAppViewRoot từ messageAppPanel
        if (messageAppPanel != null)
        {
            // Kiểm tra parent của MessageAppPanel
            VisualElement parent = messageAppPanel.parent;
            if (parent != null)
            {
                Debug.Log($"MessageAppPanel Parent ({parent.name}):");
                Debug.Log($"  - Resolved: {parent.resolvedStyle.width} x {parent.resolvedStyle.height}");
                Debug.Log($"  - Layout: {parent.layout.width} x {parent.layout.height}");
            }
            
            VisualElement messageAppViewRoot = messageAppPanel.Q<VisualElement>("MessageAppViewRoot");
            if (messageAppViewRoot != null)
            {
                Debug.Log($"MessageAppViewRoot:");
                Debug.Log($"  - Resolved: {messageAppViewRoot.resolvedStyle.width} x {messageAppViewRoot.resolvedStyle.height}");
                Debug.Log($"  - Layout: {messageAppViewRoot.layout.width} x {messageAppViewRoot.layout.height}");
                Debug.Log($"  - Style Width: {messageAppViewRoot.style.width}");
                Debug.Log($"  - Style Height: {messageAppViewRoot.style.height}");
                Debug.Log($"  - Flex Grow: {messageAppViewRoot.style.flexGrow}");
                
                // Kiểm tra MessageAppBody (có flex-grow: 1)
                VisualElement messageAppBody = messageAppViewRoot.Q<VisualElement>("MessageAppBody");
                if (messageAppBody != null)
                {
                    Debug.Log($"MessageAppBody:");
                    Debug.Log($"  - Resolved: {messageAppBody.resolvedStyle.width} x {messageAppBody.resolvedStyle.height}");
                    Debug.Log($"  - Layout: {messageAppBody.layout.width} x {messageAppBody.layout.height}");
                    Debug.Log($"  - Flex Grow: {messageAppBody.style.flexGrow}");
                    Debug.Log($"  - Flex Shrink: {messageAppBody.style.flexShrink}");
                }
            }
            else
            {
                Debug.LogWarning("MessageAppViewRoot không tìm thấy!");
            }
        }
        
        Debug.Log("=== END DEBUG ===");
    }
    
    // Debug method đơn giản (không dùng coroutine) để test
    private void DebugUISizesImmediate()
    {
        Debug.Log("=== DEBUG UI SIZES (IMMEDIATE) ===");
        Debug.Log($"Screen Size: {Screen.width} x {Screen.height}");
        
        if (rootElement != null)
        {
            Debug.Log($"ComputerUIRoot Layout: {rootElement.layout.width} x {rootElement.layout.height}");
        }
        else
        {
            Debug.LogWarning("rootElement is NULL!");
        }
        
        if (messageAppPanel != null)
        {
            Debug.Log($"MessageAppPanel Layout: {messageAppPanel.layout.width} x {messageAppPanel.layout.height}");
            Debug.Log($"MessageAppPanel Display: {messageAppPanel.style.display}");
        }
        else
        {
            Debug.LogWarning("messageAppPanel is NULL!");
        }
        
        Debug.Log("=== END DEBUG (IMMEDIATE) ===");
    }
}
