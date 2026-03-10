using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// DialogueManager - Hệ thống hội thoại hoàn chỉnh
/// Sử dụng Input System mới
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("=== UI REFERENCES ===")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI tenNguoiNoiText;
    public TextMeshProUGUI noiDungText;
    public GameObject luaChonPanel;
    public GameObject luaChonButtonPrefab;
    public Button tiepTucButton;
    public Image npcAvatarImage;

    [Header("=== CÀI ĐẶT ===")]
    public float tocDoHienChu = 0.025f;
    public bool hienUngDanhChu = true;

    private List<DialogueNode> _danhSachNode;
    private int _nodeHienTai = 0;
    private bool _dangHienChu = false;
    private bool _dangHoiThoai = false;
    private Coroutine _coroutineHienChu;
    private Action _onDialogueEnd;

    [System.Serializable]
    public class DialogueNode
    {
        public string tenNguoiNoi = "???";
        [TextArea(3, 6)]
        public string noiDung = "";
        public List<DialogueChoice> danhSachLuaChon = new List<DialogueChoice>();
        public Sprite avatar;
        public int nodeKeTiep = -1;
        public Action onNodeShow;
    }

    [System.Serializable]
    public class DialogueChoice
    {
        public string noiDungLuaChon = "";
        public int nodeKeTiep = -1;
        public Action onChon;
        public bool anNut = false;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (tiepTucButton != null)
            tiepTucButton.onClick.AddListener(XuLyTiepTuc);
    }

    private void Update()
    {
        if (!_dangHoiThoai) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (!_dangHienChu)
        {
            if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)
            {
                if (_danhSachNode != null && _nodeHienTai >= 0 && _nodeHienTai < _danhSachNode.Count)
                {
                    var node = _danhSachNode[_nodeHienTai];
                    if (node.danhSachLuaChon == null || node.danhSachLuaChon.Count == 0)
                    {
                        XuLyTiepTuc();
                    }
                }
            }
        }
        else
        {
            if (kb.spaceKey.wasPressedThisFrame ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            {
                BoQuaHieuUngChu();
            }
        }
    }

    // === PUBLIC API ===
    public void BatDauHoiThoai(List<DialogueNode> danhSachNode, Action onEnd = null)
    {
        if (_dangHoiThoai) return;

        _danhSachNode = danhSachNode;
        _nodeHienTai = 0;
        _dangHoiThoai = true;
        _onDialogueEnd = onEnd;

        dialoguePanel.SetActive(true);
        KhoaDieuKhien(true);
        HienThiNode(_nodeHienTai);
    }

    public bool DangHoiThoai => _dangHoiThoai;

    // === XỬ LÝ NỘI BỘ ===
    private void HienThiNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= _danhSachNode.Count)
        {
            KetThucHoiThoai();
            return;
        }

        _nodeHienTai = nodeIndex;
        var node = _danhSachNode[nodeIndex];

        node.onNodeShow?.Invoke();

        if (tenNguoiNoiText != null)
            tenNguoiNoiText.text = node.tenNguoiNoi;

        if (npcAvatarImage != null && node.avatar != null)
            npcAvatarImage.sprite = node.avatar;

        if (hienUngDanhChu)
        {
            if (_coroutineHienChu != null)
                StopCoroutine(_coroutineHienChu);
            _coroutineHienChu = StartCoroutine(HieuUngDanhChu(node.noiDung));
        }
        else
        {
            noiDungText.text = node.noiDung;
            HienThiLuaChonHoacTiepTuc(node);
        }
    }

    private IEnumerator HieuUngDanhChu(string noiDung)
    {
        _dangHienChu = true;
        noiDungText.text = "";

        if (luaChonPanel != null) luaChonPanel.SetActive(false);
        if (tiepTucButton != null) tiepTucButton.gameObject.SetActive(false);

        foreach (char c in noiDung)
        {
            noiDungText.text += c;
            yield return new WaitForSecondsRealtime(tocDoHienChu);
        }

        _dangHienChu = false;
        var node = _danhSachNode[_nodeHienTai];
        HienThiLuaChonHoacTiepTuc(node);
    }

    private void BoQuaHieuUngChu()
    {
        if (_coroutineHienChu != null)
        {
            StopCoroutine(_coroutineHienChu);
            _coroutineHienChu = null;
        }
        _dangHienChu = false;

        if (_danhSachNode != null && _nodeHienTai >= 0 && _nodeHienTai < _danhSachNode.Count)
        {
            var node = _danhSachNode[_nodeHienTai];
            noiDungText.text = node.noiDung;
            HienThiLuaChonHoacTiepTuc(node);
        }
    }

    private void HienThiLuaChonHoacTiepTuc(DialogueNode node)
    {
        XoaCacNutLuaChon();

        if (node.danhSachLuaChon != null && node.danhSachLuaChon.Count > 0)
        {
            if (luaChonPanel != null) luaChonPanel.SetActive(true);
            if (tiepTucButton != null) tiepTucButton.gameObject.SetActive(false);

            foreach (var luaChon in node.danhSachLuaChon)
            {
                if (luaChon.anNut) continue;
                TaoNutLuaChon(luaChon);
            }
        }
        else
        {
            if (luaChonPanel != null) luaChonPanel.SetActive(false);
            if (tiepTucButton != null) tiepTucButton.gameObject.SetActive(true);
        }
    }

    private void TaoNutLuaChon(DialogueChoice luaChon)
    {
        if (luaChonButtonPrefab == null || luaChonPanel == null) return;

        GameObject nutObj = Instantiate(luaChonButtonPrefab, luaChonPanel.transform);
        nutObj.SetActive(true);

        TextMeshProUGUI nutText = nutObj.GetComponentInChildren<TextMeshProUGUI>();
        if (nutText != null)
            nutText.text = luaChon.noiDungLuaChon;

        Button nut = nutObj.GetComponent<Button>();
        if (nut != null)
        {
            int nextNode = luaChon.nodeKeTiep;
            Action action = luaChon.onChon;

            nut.onClick.AddListener(() =>
            {
                action?.Invoke();

                if (nextNode >= 0)
                    HienThiNode(nextNode);
                else
                    KetThucHoiThoai();
            });
        }
    }

    private void XoaCacNutLuaChon()
    {
        if (luaChonPanel == null) return;
        foreach (Transform child in luaChonPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void XuLyTiepTuc()
    {
        if (_dangHienChu)
        {
            BoQuaHieuUngChu();
            return;
        }

        if (_danhSachNode == null || _nodeHienTai < 0) return;

        var node = _danhSachNode[_nodeHienTai];

        if (node.nodeKeTiep >= 0)
            HienThiNode(node.nodeKeTiep);
        else
            KetThucHoiThoai();
    }

    private void KetThucHoiThoai()
    {
        _dangHoiThoai = false;
        _danhSachNode = null;
        _nodeHienTai = -1;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        KhoaDieuKhien(false);

        _onDialogueEnd?.Invoke();
        _onDialogueEnd = null;
    }

    private void KhoaDieuKhien(bool khoa)
    {
        var fpsController = FindObjectOfType<StarterAssets.FirstPersonController>();
        if (fpsController != null)
            fpsController.enabled = !khoa;

        var playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = !khoa;

        Cursor.lockState = khoa ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = khoa;
    }
}
