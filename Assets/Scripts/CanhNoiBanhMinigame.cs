using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class CanhNoiBanhMinigame : MonoBehaviour
{
    public static CanhNoiBanhMinigame Instance;

    [Header("UI References")]
    public GameObject panel;
    public Image intensityBar;
    public Image progressBar;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI characterNameText;

    [Header("Gameplay Settings")]
    public float fuelDecayRate = 20f; // Tăng decay để khó hơn tí
    public float cookingSpeed = 5f;   // Tăng tốc độ nấu hơn nữa
    public float fuelGainPerWood = 45f; // Tăng củi lên nhiều để cảm giác E có lực

    [Header("Difficulty Settings")]
    public float safeZoneMin = 40f;
    public float safeZoneMax = 80f;
    public GameObject failPanel;
    public TextMeshProUGUI failReasonText;
    public Image safeZoneImage;

    [Header("State")]
    private float _fireIntensity = 50f; // Bắt đầu ở giữa cho dễ
    private float _cookingProgress = 0f;
    private bool _isGameActive = false;
    private int _dialogueIndex = 0;
    private bool _isGameOver = false;
    private GameObject _instructionPrompt; // Thêm reference để tắt/bật hướng dẫn

    [System.Serializable]
    public struct DialogueStep
    {
        public string character;
        public string text;
        public float progressTrigger; // 0 to 100
    }

    private List<DialogueStep> _storyline = new List<DialogueStep>();

    private void Awake()
    {
        Instance = this;
        SetupStoryline();
    }

    private void SetupStoryline()
    {
        _storyline.Clear();
        _storyline.Add(new DialogueStep { character = "Ông Nội", text = "Tết xưa vui lắm con ơi. Cả làng quây quần bên nồi bánh, đốt lửa hồng giữa đêm đông...", progressTrigger = 5f });
        _storyline.Add(new DialogueStep { character = "Ông Nội", text = "Mấy đứa nhỏ hồi đó cứ đòi thức canh bánh, rồi lăn ra ngủ quên lúc nào không hay.", progressTrigger = 25f });
        _storyline.Add(new DialogueStep { character = "Bố", text = "Con thấy đó, gia đình mình năm nào cũng vậy, dù bận thế nào cũng phải về canh nồi bánh.", progressTrigger = 50f });
        _storyline.Add(new DialogueStep { character = "Bố", text = "Mai mốt lớn rồi chắc nó bận lắm… ít về nhà thôi.", progressTrigger = 75f });
        _storyline.Add(new DialogueStep { character = "Ông Nội", text = "Kìa, thằng này nói gì lạ vậy? Tết là phải về chứ.", progressTrigger = 90f });
    }

    private void Update()
    {
        if (!_isGameActive) return;

        // 1. Decay fire
        _fireIntensity -= fuelDecayRate * Time.deltaTime;
        _fireIntensity = Mathf.Clamp(_fireIntensity, 0, 100);

        // 2. Advance progress if in safe zone
        if (_fireIntensity <= 0)
        {
            FailGame("Lửa đã tắt... Bánh không thể chín được đâu con.");
            return;
        }
        if (_fireIntensity >= 100)
        {
            FailGame("Lửa quá lớn! Bánh bị cháy khét lẹt rồi!");
            return;
        }

        if (_fireIntensity >= safeZoneMin && _fireIntensity <= safeZoneMax)
        {
            _cookingProgress += cookingSpeed * Time.deltaTime;
            _cookingProgress = Mathf.Clamp(_cookingProgress, 0, 100);
            statusText.text = "Lửa đang ở độ chín lý tưởng!";
            statusText.color = Color.green;
        }
        else
        {
            statusText.text = "Lửa không ổn định, bánh không chín!";
            statusText.color = _fireIntensity < safeZoneMin ? Color.yellow : Color.red;
        }

        // 3. Update UI
        if (intensityBar != null) intensityBar.fillAmount = _fireIntensity / 100f;
        if (progressBar != null) progressBar.fillAmount = _cookingProgress / 100f;

        // 4. Check Dialogue
        if (_dialogueIndex < _storyline.Count && _cookingProgress >= _storyline[_dialogueIndex].progressTrigger)
        {
            ShowDialogue(_storyline[_dialogueIndex]);
            _dialogueIndex++;
        }

        // 5. Check Completion
        if (_cookingProgress >= 100)
        {
            FinishGame();
        }

        // 6. Interaction Input
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            AddFirewood();
        }
    }

    public void StartGame()
    {
        // Kiểm tra điều kiện tiên quyết
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.currentDay != GameManager.TetDay.Day30)
            {
                GameManager.Instance.HienThongBao("Chưa đến lúc canh nồi bánh đâu.");
                return;
            }
            if (!GameManager.Instance.daNhanNhiemVuNgay30TuMe)
            {
                GameManager.Instance.HienThongBao("Trước khi canh nồi bánh, hãy hỏi Mẹ xem cần làm gì đã.");
                return;
            }
            if (!GameManager.Instance.daGoiBanhTet)
            {
                GameManager.Instance.HienThongBao("Phải gói bánh xong mới có bánh mà canh chứ!");
                return;
            }
        }

        gameObject.SetActive(true);
        _isGameActive = true;
        _isGameOver = false;
        _cookingProgress = 0f;
        _fireIntensity = 60f; // Bắt đầu ở mức an toàn trung bình
        _dialogueIndex = 0;
        panel.SetActive(true);
        if (failPanel != null) failPanel.SetActive(false);
        if (_instructionPrompt != null) _instructionPrompt.SetActive(true);
        
        SetAtmosphere(true);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        Debug.Log("[CanhNoiBanh] 🏮 Mission Started!");
    }

    public void AddFirewood()
    {
        if (!_isGameActive || _isGameOver) return;
        _fireIntensity += fuelGainPerWood;
        _fireIntensity = Mathf.Clamp(_fireIntensity, 0, 100);
        Debug.Log($"[CanhNoiBanh] 🔥 Đã thêm củi! Nhiệt độ hiện tại: {_fireIntensity}");
    }

    private void FailGame(string reason)
    {
        _isGameActive = false;
        _isGameOver = true;
        SetAtmosphere(false);
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (failPanel != null)
        {
            failPanel.SetActive(true);
            failReasonText.text = reason;
        }
        statusText.text = "Nhiệm vụ thất bại!";
        statusText.color = Color.red;
        Debug.Log("[CanhNoiBanh] ❌ Failed: " + reason);
    }

    private void ShowDialogue(DialogueStep step)
    {
        characterNameText.text = step.character;
        dialogueText.text = step.text;
        Debug.Log($"[{step.character}]: {step.text}");
    }

    private void SetAtmosphere(bool isNight)
    {
        // Thử tìm Directional Light để hạ độ sáng
        Light sun = RenderSettings.sun;
        if (sun == null) sun = GameObject.Find("Directional Light")?.GetComponent<Light>();
        
        if (sun != null)
        {
            sun.intensity = isNight ? 0.05f : 1.0f; // Tối hẳn
            RenderSettings.ambientIntensity = isNight ? 0.1f : 1.0f;
            RenderSettings.ambientLight = isNight ? new Color(0.05f, 0.05f, 0.1f) : Color.white;
        }
        
        // Thử tìm các đốm lửa để bật sáng
        GameObject lua = GameObject.Find("cui_lua");
        if (lua != null)
        {
            Light fireLight = lua.GetComponentInChildren<Light>();
            if (fireLight != null) fireLight.enabled = isNight;
        }
    }

    private void FinishGame()
    {
        _isGameActive = false;
        SetAtmosphere(false);
        
        // Mở khóa chuột
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.daCanhNoiBanh = true;
            GameManager.Instance.HienThongBao("Nấu bánh xong rồi! Đã đến lúc vớt bánh! 🥘✨");
            GameManager.Instance.OnNhiemVuThayDoi?.Invoke();
        }
        
        dialogueText.text = "Bánh đã chín! Mọi người chuẩn bị vớt bánh thôi...";
        characterNameText.text = "Dẫn chuyện";
        if (_instructionPrompt != null) _instructionPrompt.SetActive(false);

        Invoke("ClosePanel", 4f);
    }

    private void ClosePanel()
    {
        panel.SetActive(false);
        gameObject.SetActive(false);
    }

    // --- Static Create Method ---
    public static void Create(Canvas canvas, GameObject interactionUI)
    {
        GameObject root = new GameObject("CanhNoiBanhUI");
        root.transform.SetParent(canvas.transform, false);
        
        // QUAN TRỌNG: Root phải tỏa ra toàn màn hình để các con tính anchor chuẩn
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        
        // Background Panel (Hộp thoại bên dưới)
        GameObject p = new GameObject("Panel");
        p.transform.SetParent(root.transform, false);
        Image pImg = p.AddComponent<Image>();
        pImg.color = new Color(0.4f, 0.1f, 0.05f, 0.9f); // Màu đỏ thẫm đặc trưng
        RectTransform pRect = p.GetComponent<RectTransform>();
        pRect.anchorMin = new Vector2(0.2f, 0.05f);
        pRect.anchorMax = new Vector2(0.8f, 0.3f);
        pRect.offsetMin = Vector2.zero;
        pRect.offsetMax = Vector2.zero;

        // Border vàng
        GameObject border = new GameObject("Border");
        border.transform.SetParent(p.transform, false);
        Image borderImg = border.AddComponent<Image>();
        borderImg.color = new Color(1f, 0.84f, 0f, 1f); // Vàng gold
        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = new Vector2(1, 0.05f); // Chỉ ở cạnh dưới
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;

        // Dialogue Area
        GameObject charName = new GameObject("CharName");
        charName.transform.SetParent(p.transform, false);
        TextMeshProUGUI charTmp = charName.AddComponent<TextMeshProUGUI>();
        charTmp.fontSize = 44; // Huge
        charTmp.color = Color.yellow;
        charTmp.fontStyle = FontStyles.Bold;
        charTmp.alignment = TextAlignmentOptions.Left;
        RectTransform nameRect = charName.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.05f, 0.7f);
        nameRect.anchorMax = new Vector2(0.5f, 0.95f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        charTmp.text = "Nhân vật";

        GameObject diag = new GameObject("DialogueText");
        diag.transform.SetParent(p.transform, false);
        TextMeshProUGUI diagTmp = diag.AddComponent<TextMeshProUGUI>();
        diagTmp.fontSize = 34; // Huge
        diagTmp.color = Color.white;
        diagTmp.enableWordWrapping = true;
        diagTmp.alignment = TextAlignmentOptions.TopLeft;
        RectTransform diagRect = diag.GetComponent<RectTransform>();
        diagRect.anchorMin = new Vector2(0.05f, 0.1f);
        diagRect.anchorMax = new Vector2(0.95f, 0.7f);
        diagRect.offsetMin = Vector2.zero;
        diagRect.offsetMax = Vector2.zero;
        diagTmp.text = "...";

        // Gauges (Thanh trượt trung tâm)
        GameObject gaugesRoot = new GameObject("Gauges");
        gaugesRoot.transform.SetParent(root.transform, false);
        RectTransform gRootRect = gaugesRoot.AddComponent<RectTransform>();
        gRootRect.anchorMin = new Vector2(0.5f, 0.7f);
        gRootRect.anchorMax = new Vector2(0.5f, 0.7f);
        gRootRect.sizeDelta = new Vector2(800, 100); // Tăng chiều rộng lên 800 để chữ nằm ngang
        gRootRect.anchoredPosition = new Vector2(0, 0);

        GameObject intensityBg = new GameObject("IntensityBg");
        intensityBg.transform.SetParent(gaugesRoot.transform, false);
        intensityBg.AddComponent<Image>().color = Color.black;
        intensityBg.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 20); // Tăng bar lên 600
        intensityBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10); // Hạ xuống tí

        // Tạo sprite trắng cho thanh trượt để fillAmount hoạt động
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // SAFE ZONE MARKER
        GameObject safeZone = new GameObject("SafeZoneMark");
        safeZone.transform.SetParent(intensityBg.transform, false);
        Image safeImg = safeZone.AddComponent<Image>();
        safeImg.color = new Color(0, 1, 0, 0.3f); // Vùng xanh mờ
        RectTransform safeRect = safeZone.GetComponent<RectTransform>();
        safeRect.anchorMin = new Vector2(0.4f, 0); // safeZoneMin = 40
        safeRect.anchorMax = new Vector2(0.8f, 1); // safeZoneMax = 80
        safeRect.sizeDelta = Vector2.zero;

        GameObject intensityBar = new GameObject("IntensityBar");
        intensityBar.transform.SetParent(intensityBg.transform, false);
        Image intensityImg = intensityBar.AddComponent<Image>();
        intensityImg.sprite = whiteSprite;
        intensityImg.color = Color.red;
        intensityImg.type = Image.Type.Filled;
        intensityImg.fillMethod = Image.FillMethod.Horizontal;
        intensityBar.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        intensityBar.GetComponent<RectTransform>().anchorMax = Vector2.one;
        intensityBar.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // Label cho Intensity
        GameObject intensityLabel = new GameObject("IntensityLabel");
        intensityLabel.transform.SetParent(intensityBg.transform, false);
        TextMeshProUGUI iLabelTmp = intensityLabel.AddComponent<TextMeshProUGUI>();
        iLabelTmp.fontSize = 24; // Huge
        iLabelTmp.alignment = TextAlignmentOptions.Center;
        RectTransform iLabelRect = intensityLabel.GetComponent<RectTransform>();
        iLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
        iLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
        iLabelRect.pivot = new Vector2(0.5f, 0.5f);
        iLabelRect.anchoredPosition = new Vector2(0, 50);

        GameObject progressBg = new GameObject("ProgressBg");
        progressBg.transform.SetParent(gaugesRoot.transform, false);
        progressBg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
        progressBg.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 8); // Tăng bar lên 600
        progressBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -25); // Hạ thấp xuống

        GameObject progressBar = new GameObject("ProgressBar");
        progressBar.transform.SetParent(progressBg.transform, false);
        Image progressImg = progressBar.AddComponent<Image>();
        progressImg.sprite = whiteSprite;
        progressImg.color = Color.green;
        progressImg.type = Image.Type.Filled;
        progressImg.fillMethod = Image.FillMethod.Horizontal;
        progressBar.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        progressBar.GetComponent<RectTransform>().anchorMax = Vector2.one;
        progressBar.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        GameObject status = new GameObject("StatusText");
        status.transform.SetParent(gaugesRoot.transform, false);
        TextMeshProUGUI statusTmp = status.AddComponent<TextMeshProUGUI>();
        statusTmp.fontSize = 32; // Huge
        statusTmp.alignment = TextAlignmentOptions.Center;
        statusTmp.enableWordWrapping = false;
        RectTransform statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 0.5f);
        statusRect.anchorMax = new Vector2(0.5f, 0.5f);
        statusRect.pivot = new Vector2(0.5f, 0.5f);
        statusRect.sizeDelta = new Vector2(800, 50);
        statusRect.anchoredPosition = new Vector2(0, 160);

        // FAIL PANEL
        GameObject failP = new GameObject("FailPanel");
        failP.transform.SetParent(root.transform, false);
        Image failImg = failP.AddComponent<Image>();
        failImg.color = new Color(0, 0, 0, 0.95f);
        RectTransform failPRect = failP.GetComponent<RectTransform>();
        failPRect.anchorMin = new Vector2(0.25f, 0.3f);
        failPRect.anchorMax = new Vector2(0.75f, 0.7f);
        failPRect.offsetMin = Vector2.zero;
        failPRect.offsetMax = Vector2.zero;

        GameObject failTitle = new GameObject("FailTitle");
        failTitle.transform.SetParent(failP.transform, false);
        TextMeshProUGUI failTitleTmp = failTitle.AddComponent<TextMeshProUGUI>();
        failTitleTmp.text = "NHIỆM VỤ THẤT BẠI!";
        failTitleTmp.color = Color.red;
        failTitleTmp.fontSize = 36;
        failTitleTmp.alignment = TextAlignmentOptions.Center;
        failTitle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 80);

        GameObject failReason = new GameObject("FailReason");
        failReason.transform.SetParent(failP.transform, false);
        TextMeshProUGUI failReasonTmp = failReason.AddComponent<TextMeshProUGUI>();
        failReasonTmp.fontSize = 24;
        failReasonTmp.alignment = TextAlignmentOptions.Center;
        failReason.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);

        GameObject retryBtnObj = new GameObject("RetryButton");
        retryBtnObj.transform.SetParent(failP.transform, false);
        Image rbImg = retryBtnObj.AddComponent<Image>();
        rbImg.color = new Color(0.6f, 0.1f, 0.1f);
        Button rbBtn = retryBtnObj.AddComponent<Button>();
        RectTransform rbRect = retryBtnObj.GetComponent<RectTransform>();
        rbRect.sizeDelta = new Vector2(250, 60);
        rbRect.anchoredPosition = new Vector2(0, -60);
        GameObject rbTxt = new GameObject("Text");
        rbTxt.transform.SetParent(retryBtnObj.transform, false);
        TextMeshProUGUI rbTmp = rbTxt.AddComponent<TextMeshProUGUI>();
        rbTmp.text = "THỬ LẠI";
        rbTmp.fontSize = 24;
        rbTmp.alignment = TextAlignmentOptions.Center;
        rbTmp.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 60);

        // Component setup
        CanhNoiBanhMinigame game = root.AddComponent<CanhNoiBanhMinigame>();
        game.panel = p;
        game.failPanel = failP;
        game.failReasonText = failReasonTmp;
        game.intensityBar = intensityImg;
        game.progressBar = progressImg;
        game.statusText = statusTmp;
        game.dialogueText = diagTmp;
        game.characterNameText = charTmp;
        rbBtn.onClick.AddListener(game.StartGame);
        
        root.SetActive(false);

        // Instruction (Nhấn E để thêm củi)
        GameObject instruction = new GameObject("InstructionPrompt");
        instruction.transform.SetParent(root.transform, false);
        TextMeshProUGUI insTmp = instruction.AddComponent<TextMeshProUGUI>();
        insTmp.text = "Nhấn E để THÊM CỦI";
        insTmp.fontSize = 40; // Huge
        insTmp.alignment = TextAlignmentOptions.Center;
        RectTransform insRect = instruction.GetComponent<RectTransform>();
        insRect.anchorMin = new Vector2(0.3f, 0.4f);
        insRect.anchorMax = new Vector2(0.7f, 0.45f);
        insRect.offsetMin = Vector2.zero;
        insRect.offsetMax = Vector2.zero;
        game._instructionPrompt = instruction;

        // --- WORLD INTEGRATION ---
        // ƯU TIÊN gắn vào nồi bánh thật trong scene nếu có
        GameObject worldTarget = GameObject.Find("noi_banh_tet");
        string targetNameForLog = "noi_banh_tet";

        // Fallback: nếu không có nồi thì gắn vào cụm củi_lửa như cũ
        if (worldTarget == null)
        {
            worldTarget = GameObject.Find("cui_lua");
            targetNameForLog = "cui_lua";
        }

        if (worldTarget != null)
        {
            Debug.Log($"[CanhNoiBanh] ✅ Tích hợp tương tác vào {targetNameForLog}");
            CanhNoiBanhTrigger t = worldTarget.GetComponent<CanhNoiBanhTrigger>();
            if (t == null) t = worldTarget.AddComponent<CanhNoiBanhTrigger>();
            t.Setup(interactionUI);

            // Dùng SphereCollider trigger riêng cho vùng tương tác, không đụng collider mesh có sẵn
            SphereCollider sc = null;
            foreach (var c in worldTarget.GetComponents<Collider>())
            {
                if (c is SphereCollider sphere)
                {
                    sc = sphere;
                    break;
                }
            }
            if (sc == null) sc = worldTarget.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 3f / Mathf.Max(worldTarget.transform.lossyScale.x, 0.001f);
        }
    }
}
