using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BanhTetMinigame : MonoBehaviour
{
    public static BanhTetMinigame Instance;

    public GameObject panel;
    public Image displayImage;
    public TextMeshProUGUI statusText;
    public Button actionButton;
    public TextMeshProUGUI buttonText;
    public GameObject successPanel;
    public TextMeshProUGUI successText;

    [Header("Sprites")]
    public Sprite imgLaChuoi;
    public Sprite imgGaoNep;
    public Sprite imgNhanThit;
    public Sprite imgDauXanh;
    public Sprite imgFinished;

    public GameObject ingredientButtonsParent;
    public TextMeshProUGUI motherFeedbackText;

    private int _currentSequenceIndex = 0;
    private int[] _targetSequence = { 1, 2, 3, 2, 1 }; // 1: Nếp, 2: Đậu, 3: Thịt
    private bool _isGameActive = false;

    private string[] _funnyScolding = {
        "Mày gói bánh hay mày quăng lựu đạn vậy con?",
        "Học hành cho cố vô rồi gói cái bánh cũng hổng xong!",
        "Gạt nguyên liệu qua một bên, làm tầm bậy là sao? Muốn ăn đòn hả?",
        "Đậu xanh của tui đâu? Đừng có mà lộn xộn nghe chưa!",
        "Nhìn cái bánh của mày gói chắc đem cho hổng ai dám ăn luôn quá!",
        "Trời ơi là trời! Làm cho đúng thứ tự, sao mày làm ngược ngạo vậy?",
        "Mẹ dạy sao? Quên hết rồi hả? Có muốn ăn chổi không?",
        "Bỏ nếp, bỏ đậu, rồi mới tới thịt! Mày bỏ cái gì vô đó vậy?"
    };

    private string[] _praise = {
        "Giỏi lắm con trai! Tiếp đi.",
        "Đúng rồi đó, tay chân lanh lẹ lên.",
        "Khéo tay y hệt như mẹ hồi trẻ vậy.",
        "Sắp xong rồi, vô nốt cái cuối đi con!"
    };

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (panel != null && panel.activeSelf)
        {
            // Cưỡng ép hiện trỏ chuột trong suốt thời gian hiện bảng minigame
            if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        if (Keyboard.current != null && Keyboard.current.f12Key.wasPressedThisFrame)
        {
            StartGame();
        }
    }

    public static void Create(Canvas canvas, GameObject interactionUI)
    {
        // 1. Tạo UI Panel chính
        GameObject minigamePanel = TaoPanel("BanhTetMinigamePanel", canvas.transform);
        RectTransform panelRect = minigamePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(950, 800); // Tăng size cho thoải mái
        minigamePanel.GetComponent<Image>().color = new Color(0.15f, 0.05f, 0, 0.95f);
        minigamePanel.SetActive(false);

        // 2. Tiêu đề
        TaoText("Title", minigamePanel.transform, "GÓI BÁNH TÉT CÙNG MẸ", 40, TextAlignmentOptions.Center)
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 300);

        // 3. Mother Feedback - Đẩy lên cao hơn
        TextMeshProUGUI motherText = TaoText("MotherFeedback", minigamePanel.transform, "Mẹ đang nhìn mày đó...", 22, TextAlignmentOptions.Center);
        motherText.color = Color.yellow;
        motherText.fontStyle = FontStyles.Italic;
        motherText.enableWordWrapping = true;
        RectTransform motherRect = motherText.GetComponent<RectTransform>();
        motherRect.sizeDelta = new Vector2(850, 100);
        motherRect.anchoredPosition = new Vector2(0, 260);

        // 4. Manual Instruction - Đẩy lên cao hơn chút để không bị lá che
        TextMeshProUGUI statusText = TaoText("Instruction", minigamePanel.transform, "Cần: Nếp -> Đậu -> Thịt -> Đậu -> Nếp", 20, TextAlignmentOptions.Center);
        statusText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 210);

        // 5. Display Image - Làm "bụ" ra (Ngang 650, Cao 350) và hạ thấp xuống
        GameObject displayObj = new GameObject("DisplayImage");
        displayObj.transform.SetParent(minigamePanel.transform, false);
        Image displayImg = displayObj.AddComponent<Image>();
        displayImg.preserveAspect = false; // Tắt preserve để có thể làm "bụ" theo ý muốn
        RectTransform displayRect = displayImg.GetComponent<RectTransform>();
        displayRect.sizeDelta = new Vector2(650, 350); 
        displayRect.anchoredPosition = new Vector2(0, 0);

        // 6. Nút Thực hiện (Dùng để trải lá chuối đầu tiên và buộc lạt cuối cùng)
        GameObject btnObj = TaoNut("ActionBtn", minigamePanel.transform, "TRẢI LÁ CHUỐI", 24);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(250, 60);
        btnRect.anchoredPosition = new Vector2(0, -220); // Đẩy nút xuống dưới
        Button actionBtn = btnObj.GetComponent<Button>();
        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

        // 7. Nhóm nút nguyên liệu - Hạ thấp xuống để chừa chỗ cho cái ảnh "bụ"
        GameObject buttonGroup = new GameObject("IngredientButtons");
        buttonGroup.transform.SetParent(minigamePanel.transform, false);
        buttonGroup.AddComponent<RectTransform>().anchoredPosition = new Vector2(0, -280); 
        
        // Load các sprite để dùng cho nút luôn
        Sprite sNep = LoadSprite("goi_banh/nep");
        Sprite sDau = LoadSprite("goi_banh/dau_xanh");
        Sprite sThit = LoadSprite("goi_banh/thit");

        GameObject btnNep = TaoNutAnh("BtnNep", buttonGroup.transform, sNep, "NẾP");
        btnNep.GetComponent<RectTransform>().anchoredPosition = new Vector2(-220, 0);
        btnNep.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 150);

        GameObject btnDau = TaoNutAnh("BtnDau", buttonGroup.transform, sDau, "ĐẬU");
        btnDau.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        btnDau.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 150);

        GameObject btnThit = TaoNutAnh("BtnThit", buttonGroup.transform, sThit, "THỊT");
        btnThit.GetComponent<RectTransform>().anchoredPosition = new Vector2(220, 0);
        btnThit.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 150);
        
        buttonGroup.SetActive(false);

        // 8. Success Panel (Chuyển thành thông báo ở dưới cùng, không che ảnh)
        GameObject successPanel = TaoPanel("SuccessPanel", minigamePanel.transform);
        successPanel.GetComponent<Image>().color = new Color(0, 0.4f, 0, 0.9f);
        successPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 150);
        successPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -260); // Đặt hẳn xuống dưới
        successPanel.SetActive(false);
        
        TextMeshProUGUI successText = TaoText("SuccessMsg", successPanel.transform, "Mẹ cười tươi: \"Khéo tay đó con trai!\"", 28, TextAlignmentOptions.Center);
        successText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30);
        
        GameObject closeBtn = TaoNut("CloseBtn", successPanel.transform, "XONG", 20);
        closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -35);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 50);

        // 9. Script setup
        BanhTetMinigame game = minigamePanel.AddComponent<BanhTetMinigame>();
        Instance = game;
        
        game.panel = minigamePanel;
        game.displayImage = displayImg;
        game.statusText = statusText;
        game.actionButton = actionBtn;
        game.buttonText = btnText;
        game.successPanel = successPanel;
        game.successText = successText;
        game.motherFeedbackText = motherText;
        game.ingredientButtonsParent = buttonGroup;

        // Load Sprites
        game.imgLaChuoi = LoadSprite("goi_banh/la_chuoi");
        game.imgGaoNep = LoadSprite("goi_banh/nep");
        game.imgNhanThit = LoadSprite("goi_banh/thit");
        game.imgDauXanh = LoadSprite("goi_banh/dau_xanh");
        
        // Ưu tiên load tên 'banh_tet' trước vì thấy trong project bạn có file này
        game.imgFinished = LoadSprite("goi_banh/banh_tet", true); 
        if (game.imgFinished == null) game.imgFinished = LoadSprite("banh_tet", true); // Fallback ở root Resources
        if (game.imgFinished == null) game.imgFinished = LoadSprite("goi_banh/finished_banh_tet_wrapped", true);
        if (game.imgFinished == null) game.imgFinished = LoadSprite("goi_banh/banh_tet_wrapped", true);
        
        // Nếu tất cả đều fail thì mới gán fallback cuối cùng
        if (game.imgFinished == null) game.imgFinished = game.imgDauXanh;

        // Listeners
        actionBtn.onClick.AddListener(game.OnClickAction);
        btnNep.GetComponent<Button>().onClick.AddListener(() => game.OnIngredientClick(1));
        btnDau.GetComponent<Button>().onClick.AddListener(() => game.OnIngredientClick(2));
        btnThit.GetComponent<Button>().onClick.AddListener(() => game.OnIngredientClick(3));
        closeBtn.GetComponent<Button>().onClick.AddListener(game.CloseGame);

        // 8. TÍCH HỢP TRỰC TIẾP VÀO NHÀ (Chỉ làm trên NhaTetLoai4 cho gọn)
        string[] potentialHouses = { "NhaTetLoai4", "NhaTetLoai2", "NhaNhiemVu" };
        GameObject targetHouse = GameObject.Find("NhaTetLoai4");
        
        // Dọn dẹp tất cả các nhà để tránh bị "nhấn E" từ xa ở nhà cũ
        foreach (string name in potentialHouses)
        {
            GameObject h = GameObject.Find(name);
            if (h != null)
            {
                GoiBanhTrigger oldT = h.GetComponent<GoiBanhTrigger>();
                if (oldT != null) Destroy(oldT);
                
                // Xóa các BoxCollider là Trigger (để không xóa nhầm collider vật lý của nhà)
                BoxCollider[] bcs = h.GetComponents<BoxCollider>();
                foreach (var b in bcs) if (b.isTrigger) Destroy(b);
            }
        }

        if (targetHouse != null)
        {
            Debug.Log("[BanhTetMinigame] ✅ Tích hợp tương tác vào: " + targetHouse.name);
            
            // Thêm script xử lý tương tác
            GoiBanhTrigger trigger = targetHouse.AddComponent<GoiBanhTrigger>();
            trigger.Setup(interactionUI);

            // Thêm Collider vùng nhận diện SIÊU NHỎ
            // Sử dụng lossyScale để đảm bảo kích thước thực tế chỉ tầm 2-3 mét trong game
            BoxCollider bc = targetHouse.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            
            Vector3 worldSize = new Vector3(2.5f, 2.5f, 2.5f); // Kích thước mong muốn trong world
            Vector3 parentScale = targetHouse.transform.lossyScale;
            
            bc.size = new Vector3(
                worldSize.x / Mathf.Max(parentScale.x, 0.001f),
                worldSize.y / Mathf.Max(parentScale.y, 0.001f),
                worldSize.z / Mathf.Max(parentScale.z, 0.001f)
            );
            bc.center = new Vector3(0, 1f / Mathf.Max(parentScale.y, 0.001f), 0); 
            
            Debug.Log($"[BanhTetMinigame] ✅ Đã gán trigger SIÊU NHỎ (đã bù scale) vào {targetHouse.name}.");
        }
        else
        {
            Debug.LogError("[BanhTetMinigame] ❌ Không tìm thấy NhaTetLoai4!");
        }
    }

    private static Sprite LoadSprite(string path, bool silent = false)
    {
        Sprite s = Resources.Load<Sprite>(path);
        if (s != null) return s;

        Texture2D tex = Resources.Load<Texture2D>(path);
        if (tex != null)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        if (!silent)
        {
            Debug.LogError($"[BanhTetMinigame] ❌ Failed to load asset at: Resources/{path}");
        }
        return null;
    }

    public void StartGame()
    {
        _isGameActive = true;
        _currentSequenceIndex = 0;
        panel.SetActive(true);
        successPanel.SetActive(false);
        
        // Hiện tất cả nguyên liệu luôn như yêu cầu
        ingredientButtonsParent.SetActive(true);
        actionButton.gameObject.SetActive(false); // Ẩn nút "Trải lá chuối/Buộc lạt" lúc đầu
        
        motherFeedbackText.text = "Mẹ: \"Nguyên liệu có sẵn đó, bỏ Nếp-Đậu-Thịt cho đúng lớp nghe con!\"";
        UpdateUI();
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameObject goiy = GameObject.Find("GoiYTuongTacPanel");
        if (goiy != null) goiy.SetActive(false);
    }

    public void OnClickAction()
    {
        // Nút này bây giờ chỉ xử lý bước cuối "Buộc lạt"
        if (_currentSequenceIndex >= _targetSequence.Length)
        {
            FinishGame();
        }
    }

    public void OnIngredientClick(int ingredientId)
    {
        if (!_isGameActive) return;

        if (ingredientId == _targetSequence[_currentSequenceIndex])
        {
            // Đúng
            _currentSequenceIndex++;
            motherFeedbackText.text = "Mẹ: \"" + _praise[Random.Range(0, _praise.Length)] + "\"";
            UpdateUI();

            if (_currentSequenceIndex >= _targetSequence.Length)
            {
                // Xong các lớp nhân, hiện nút Buộc lạt
                ingredientButtonsParent.SetActive(false);
                actionButton.gameObject.SetActive(true);
                buttonText.text = "BUỘC LẠT";
                motherFeedbackText.text = "Mẹ: \"Chà, nhân bánh đẹp đó. Giờ buộc dây lạt lại là xong!\"";
            }
        }
        else
        {
            // Sai - Chửi và Bắt làm lại
            motherFeedbackText.text = "Mẹ: \"" + _funnyScolding[Random.Range(0, _funnyScolding.Length)] + " VỨT HẾT LÀM LẠI!\"";
            _currentSequenceIndex = 0; // Reset tiến trình
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        displayImage.color = Color.white;
        
        // Luôn hiện lá chuối làm nền nếu đang gói
        if (_currentSequenceIndex == 0)
        {
            displayImage.sprite = imgLaChuoi;
        }
        else
        {
            // Hiển thị lớp hiện tại đang làm để người chơi thấy sự thay đổi
            int currentIng = _targetSequence[_currentSequenceIndex - 1];
            switch (currentIng)
            {
                case 1: displayImage.sprite = imgGaoNep; break;
                case 2: displayImage.sprite = imgDauXanh; break;
                case 3: displayImage.sprite = imgNhanThit; break;
            }
        }
    }

    private void FinishGame()
    {
        _isGameActive = false;
        if (displayImage != null) 
        {
            displayImage.sprite = imgFinished;
            displayImage.color = Color.white;
        }
        if (successPanel != null) successPanel.SetActive(true);
        if (actionButton != null) actionButton.gameObject.SetActive(false);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HienThongBao("Bạn đã hoàn thành gói bánh Tét cùng Mẹ!");
        }
    }

    public void CloseGame()
    {
        panel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // --- HELPER METHODS ---
    private static GameObject TaoPanel(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<CanvasRenderer>();
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.8f);
        return obj;
    }

    private static TextMeshProUGUI TaoText(string name, Transform parent, string text, float size, TextAlignmentOptions align)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        return tmp;
    }

    private static GameObject TaoNutAnh(string name, Transform parent, Sprite sprite, string label)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        
        // Nền nút
        Image bg = obj.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.1f, 0.05f, 1f); 
        
        // Ảnh nguyên liệu - Căn giữa, không cần nhãn chữ bên dưới
        GameObject imgObj = new GameObject("Icon");
        imgObj.transform.SetParent(obj.transform, false);
        Image icon = imgObj.AddComponent<Image>();
        icon.sprite = sprite;
        icon.preserveAspect = true;
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = new Vector2(-20, -20); // Đều các cạnh
        iconRect.anchoredPosition = Vector2.zero;

        obj.AddComponent<Button>();
        return obj;
    }

    private static GameObject TaoNut(string name, Transform parent, string text, float fontSize = 20)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<Image>().color = new Color(0.6f, 0.15f, 0.1f, 1f);
        Button btn = obj.AddComponent<Button>();
        TaoText("Text", obj.transform, text, fontSize, TextAlignmentOptions.Center).color = Color.yellow;
        return obj;
    }
}
