using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Điều khiển sách 3 trang: lật 1 chiều, mỗi lần nhấn E vừa next hội thoại vừa sang trang tiếp.
/// Gọi StartBook() khi vào AlbumFocus; khi hết 3 trang sẽ chuyển sang AlbumInteractable.
/// </summary>
public class BookController : MonoBehaviour
{
    [System.Serializable]
    public class BookPageData
    {
        [Tooltip("Ảnh hiển thị cho trang (để trống = nền mặc định)")]
        public Sprite image;
        [Tooltip("Câu hội thoại hiển thị trong dialog box khi mở trang này")]
        [TextArea(1, 4)]
        public string dialogue = "...";
    }

    [Header("UI")]
    public UIDocument bookUIDocument;
    [Tooltip("Gán stylesheet BookUI.uss vào đây hoặc vào UIDocument")]
    public StyleSheet bookStylesheet;

    [Header("Nội dung 3 trang đầu")]
    public BookPageData[] pages = new BookPageData[]
    {
        new BookPageData { dialogue = "..." },
        new BookPageData { dialogue = "..." },
        new BookPageData { dialogue = "..." }
    };

    private VisualElement root;
    private VisualElement page1, page2, page3;
    private NarrativeTextController narrativeController;
    private int currentPageIndex;
    private const int PageCount = 3;

    void Awake()
    {
        if (bookUIDocument == null)
            bookUIDocument = GetComponent<UIDocument>();
        if (bookUIDocument != null)
            bookUIDocument.enabled = false;

        narrativeController = FindFirstObjectByType<NarrativeTextController>();
    }

    void Start()
    {
        EnsureUIRefs();
    }

    private void EnsureUIRefs()
    {
        if (bookUIDocument == null || root != null) return;
        var r = bookUIDocument.rootVisualElement;
        if (r == null) return;

        root = r.Q<VisualElement>("BookRoot");
        if (root != null && bookStylesheet != null)
            root.styleSheets.Add(bookStylesheet);

        var container = root != null ? root.Q<VisualElement>("PageContainer") : null;
        if (container != null)
        {
            page1 = container.Q<VisualElement>("Page1");
            page2 = container.Q<VisualElement>("Page2");
            page3 = container.Q<VisualElement>("Page3");
        }
    }

    /// <summary>
    /// Bắt đầu xem sách: hiện trang 1 + hội thoại trang 1. Mỗi lần E = next hội thoại + lật trang.
    /// </summary>
    public void StartBook()
    {
        if (bookUIDocument == null)
        {
            if (GameFlow.Instance != null)
                GameFlow.Instance.ChangeState(GameState.AlbumInteractable);
            return;
        }

        bookUIDocument.enabled = true;
        EnsureUIRefs();
        if (root == null)
        {
            if (GameFlow.Instance != null)
                GameFlow.Instance.ChangeState(GameState.AlbumInteractable);
            return;
        }

        currentPageIndex = 0;
        ShowPageVisual(0);
        ShowDialogueForPage(0);
    }

    private void ShowPageVisual(int pageIndex)
    {
        if (page1 != null) page1.style.display = pageIndex == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        if (page2 != null) page2.style.display = pageIndex == 1 ? DisplayStyle.Flex : DisplayStyle.None;
        if (page3 != null) page3.style.display = pageIndex == 2 ? DisplayStyle.Flex : DisplayStyle.None;

        var page = GetPageElement(pageIndex);
        if (page != null && pages != null && pageIndex < pages.Length && pages[pageIndex].image != null)
            page.style.backgroundImage = new StyleBackground(pages[pageIndex].image);
        else if (page != null)
            page.style.backgroundImage = StyleKeyword.Null;
    }

    private VisualElement GetPageElement(int index)
    {
        if (index == 0) return page1;
        if (index == 1) return page2;
        if (index == 2) return page3;
        return null;
    }

    private void ShowDialogueForPage(int pageIndex)
    {
        string text = pages != null && pageIndex < pages.Length ? pages[pageIndex].dialogue : "...";
        if (narrativeController != null)
            narrativeController.ShowText(text, OnPageDialogueComplete);
        else
            OnPageDialogueComplete();
    }

    private void OnPageDialogueComplete()
    {
        currentPageIndex++;
        if (currentPageIndex < PageCount)
        {
            ShowPageVisual(currentPageIndex);
            ShowDialogueForPage(currentPageIndex);
        }
        else
        {
            HideBook();
            if (GameFlow.Instance != null)
                GameFlow.Instance.ChangeState(GameState.AlbumInteractable);
        }
    }

    public void HideBook()
    {
        if (bookUIDocument != null)
            bookUIDocument.enabled = false;
        currentPageIndex = 0;
    }

    public bool IsBookVisible => bookUIDocument != null && bookUIDocument.enabled;
}
