using UnityEngine;
using UnityEngine.UIElements;

public enum ChatState
{
    None,
    MomWaitingReply,
    BossWaitingReply,
    Done
}

public class ChatUIController : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    [Header("Style Sheets")]
    public StyleSheet chatStyles; // Kéo ChatStyles.uss vào đây trong Inspector
    
    // UI Elements
    private VisualElement rootElement;
    private VisualElement messageList;
    private VisualElement messageBubbleTemplate;
    private Button sendButton;
    private TextField messageInputField;
    private VisualElement chatListItemMom;
    private VisualElement chatListItemBoss;
    
    // State
    private ChatState currentState = ChatState.None;
    private string currentSelectedChat = "Mom"; // "Mom" hoặc "Boss"
    private bool bossMessageShown = false; // Track xem đã hiển thị tin nhắn sếp chưa
    // momReplied không cần thiết vì có thể check từ momMessagesData.Count > 1
    private bool bossChatClicked = false; // Track xem đã click vào chat sếp chưa (để bỏ highlight)
    private bool chatStarted = false; // Track xem đã bắt đầu chat chưa (để không reset khi mở lại)
    
    // Lưu messages của mỗi chat (lưu data, không lưu VisualElement)
    private System.Collections.Generic.List<System.Tuple<string, bool>> momMessagesData = new System.Collections.Generic.List<System.Tuple<string, bool>>();
    private System.Collections.Generic.List<System.Tuple<string, bool>> bossMessagesData = new System.Collections.Generic.List<System.Tuple<string, bool>>();
    
    void Start()
    {
        // KHÔNG initialize ở đây vì MessageAppPanel có thể chưa visible
        // Sẽ initialize khi OnMessageAppOpened() được gọi
    }
    
    // Public method để gọi từ ComputerUIManager khi mở MessageApp
    public void OnMessageAppOpened()
    {
        if (rootElement == null)
        {
            InitializeUI();
        }
        
        if (rootElement != null)
        {
            // Chỉ start chat nếu chưa bắt đầu
            if (!chatStarted)
            {
                StartChat();
            }
            else
            {
                // Đã bắt đầu rồi, chỉ restore UI state
                RestoreChatState();
            }
        }
    }
    
    // Restore lại state khi mở lại app (không reset)
    void RestoreChatState()
    {
        // Update chat selection visual
        UpdateChatSelection();
        
        // Restore messages của chat hiện tại
        if (messageList != null)
        {
            messageList.Clear();
            // Keep template
            if (messageBubbleTemplate != null)
            {
                messageList.Add(messageBubbleTemplate);
            }
        }
        
        // Restore messages dựa trên chat đang chọn
        if (currentSelectedChat == "Mom")
        {
            foreach (var msgData in momMessagesData)
            {
                VisualElement msg = CreateMessageBubble(msgData.Item1, msgData.Item2);
                ShowMessage(msg);
            }
        }
        else if (currentSelectedChat == "Boss")
        {
            foreach (var msgData in bossMessagesData)
            {
                VisualElement msg = CreateMessageBubble(msgData.Item1, msgData.Item2);
                ShowMessage(msg);
            }
        }
        
        // Restore highlight state
        if (chatListItemBoss != null)
        {
            if (bossChatClicked || currentState != ChatState.BossWaitingReply)
            {
                // Đã click rồi hoặc chưa đến state BossWaitingReply → không highlight
                chatListItemBoss.RemoveFromClassList("highlight");
            }
            else if (currentState == ChatState.BossWaitingReply && !bossChatClicked)
            {
                // Chưa click và đã có tin nhắn sếp → highlight
                chatListItemBoss.AddToClassList("highlight");
            }
        }
        
        // Restore Send button state
        UpdateSendButtonState();
    }
    
    void InitializeUI()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }
        
        if (uiDocument == null)
        {
            Debug.LogError("ChatUIController: UIDocument not found!");
            return;
        }
        
        // Thêm ChatStyles.uss vào UIDocument nếu đã gán
        if (chatStyles != null && uiDocument.rootVisualElement != null)
        {
            uiDocument.rootVisualElement.styleSheets.Add(chatStyles);
            Debug.Log("ChatUIController: Đã thêm ChatStyles.uss");
        }
        else if (chatStyles == null)
        {
            Debug.LogWarning("ChatUIController: ChatStyles chưa được gán! Hãy kéo ChatStyles.uss vào field 'Chat Styles' trong Inspector.");
        }
        
        // MessageAppViewRoot nằm trong template, cần query từ root
        if (uiDocument.rootVisualElement == null)
        {
            Debug.LogError("ChatUIController: UIDocument rootVisualElement is null!");
            return;
        }
        
        VisualElement computerUIRoot = uiDocument.rootVisualElement;
        rootElement = computerUIRoot.Q<VisualElement>("MessageAppViewRoot");
        
        if (rootElement == null)
        {
            Debug.LogError("ChatUIController: MessageAppViewRoot not found! Make sure MessageAppPanel is visible.");
            return;
        }
        
        // Query elements từ MessageAppViewRoot (kiểm tra null trước khi query)
        if (rootElement != null)
        {
            messageList = rootElement.Q<VisualElement>("MessageList");
            messageBubbleTemplate = rootElement.Q<VisualElement>("MessageBubbleTemplate");
            sendButton = rootElement.Q<Button>("SendButton");
            messageInputField = rootElement.Q<TextField>("MessageInputField");
            chatListItemMom = rootElement.Q<VisualElement>("ChatListItem_Mom");
            chatListItemBoss = rootElement.Q<VisualElement>("ChatListItem_Boss");
        }
        
        // Setup Send button
        if (sendButton != null)
        {
            sendButton.clicked += OnSendButtonClicked;
        }
        
        // Setup chat list item click handlers và hover effects
        if (chatListItemMom != null)
        {
            chatListItemMom.RegisterCallback<ClickEvent>(evt => OnChatItemClicked("Mom"));
            chatListItemMom.RegisterCallback<MouseEnterEvent>(evt => OnChatItemHoverEnter(chatListItemMom));
            chatListItemMom.RegisterCallback<MouseLeaveEvent>(evt => OnChatItemHoverExit(chatListItemMom));
        }
        if (chatListItemBoss != null)
        {
            chatListItemBoss.RegisterCallback<ClickEvent>(evt => OnChatItemClicked("Boss"));
            chatListItemBoss.RegisterCallback<MouseEnterEvent>(evt => OnChatItemHoverEnter(chatListItemBoss));
            chatListItemBoss.RegisterCallback<MouseLeaveEvent>(evt => OnChatItemHoverExit(chatListItemBoss));
        }
        
        // Hide template
        if (messageBubbleTemplate != null)
        {
            messageBubbleTemplate.style.display = DisplayStyle.None;
        }
    }
    
    void StartChat()
    {
        // Đánh dấu đã bắt đầu chat
        chatStarted = true;
        
        // Reset state
        currentState = ChatState.None;
        currentSelectedChat = "Mom";
        bossMessageShown = false;
        // momReplied không cần reset vì đã xóa
        bossChatClicked = false;
        
        // Clear saved messages
        momMessagesData.Clear();
        bossMessagesData.Clear();
        
        // Clear message list
        if (messageList != null)
        {
            messageList.Clear();
            // Keep template
            if (messageBubbleTemplate != null)
            {
                messageList.Add(messageBubbleTemplate);
            }
        }
        
        // Update chat selection visual
        UpdateChatSelection();
        
        // Start với chat Mẹ
        momMessagesData.Add(new System.Tuple<string, bool>("Mẹ: Con ăn cơm chưa?", true));
        VisualElement firstMessage = CreateMessageBubble("Mẹ: Con ăn cơm chưa?", true);
        ShowMessage(firstMessage);
        currentState = ChatState.MomWaitingReply;
        
        // Enable Send button (đang chờ reply mẹ)
        UpdateSendButtonState();
    }
    
    // Cập nhật state của Send button dựa trên chat hiện tại và state
    void UpdateSendButtonState()
    {
        if (sendButton == null) return;
        
        bool shouldEnable = false;
        
        if (currentSelectedChat == "Mom")
        {
            // Ở chat mẹ: chỉ enable khi đang chờ reply (MomWaitingReply)
            shouldEnable = (currentState == ChatState.MomWaitingReply);
        }
        else if (currentSelectedChat == "Boss")
        {
            // Ở chat sếp: chỉ enable khi đang chờ reply (BossWaitingReply)
            shouldEnable = (currentState == ChatState.BossWaitingReply);
        }
        
        sendButton.SetEnabled(shouldEnable);
    }
    
    void OnChatItemClicked(string chatName)
    {
        if (currentSelectedChat == chatName) return; // Đã chọn rồi
        
        // Switch chat
        if (chatName == "Mom")
        {
            SwitchToMomChat();
        }
        else if (chatName == "Boss")
        {
            SwitchToBossChat();
        }
    }
    
    // Hover effects cho chat list items (CSS sẽ tự động xử lý, nhưng có thể thêm logic custom nếu cần)
    void OnChatItemHoverEnter(VisualElement chatItem)
    {
        if (chatItem == null) return;
        // CSS hover sẽ tự động xử lý, không cần set backgroundColor trong C#
        // Có thể thêm logic custom ở đây nếu cần (ví dụ: scale, shadow, etc.)
    }
    
    void OnChatItemHoverExit(VisualElement chatItem)
    {
        if (chatItem == null) return;
        // CSS hover sẽ tự động xử lý
    }
    
    void UpdateChatSelection()
    {
        // Remove selected từ tất cả
        if (chatListItemMom != null)
        {
            chatListItemMom.RemoveFromClassList("chat-list-item-selected");
        }
        if (chatListItemBoss != null)
        {
            chatListItemBoss.RemoveFromClassList("chat-list-item-selected");
        }
        
        // Add selected cho chat hiện tại
        if (currentSelectedChat == "Mom" && chatListItemMom != null)
        {
            chatListItemMom.AddToClassList("chat-list-item-selected");
        }
        else if (currentSelectedChat == "Boss" && chatListItemBoss != null)
        {
            chatListItemBoss.AddToClassList("chat-list-item-selected");
        }
    }
    
    void OnSendButtonClicked()
    {
        NextState();
    }
    
    void NextState()
    {
        switch (currentState)
        {
            case ChatState.MomWaitingReply:
                // Chỉ xử lý nếu đang ở chat mẹ
                if (currentSelectedChat == "Mom")
                {
                    // Player reply
                    momMessagesData.Add(new System.Tuple<string, bool>("Con ăn rồi ạ", false));
                    VisualElement replyMessage = CreateMessageBubble("Con ăn rồi ạ", false);
                    ShowMessage(replyMessage);
                    // momReplied không cần set vì đã xóa
                    
                    // Highlight Boss chat item (chỉ khi chưa click vào)
                    if (chatListItemBoss != null && !bossChatClicked)
                    {
                        chatListItemBoss.AddToClassList("highlight");
                    }
                    
                    // KHÔNG tự động switch sang Boss chat
                    // Chỉ đánh dấu là sếp đã nhắn (người dùng phải click để xem)
                    currentState = ChatState.BossWaitingReply;
                    // Tin nhắn của sếp sẽ hiển thị khi người dùng click vào chat sếp
                    
                    // Disable Send button (đã reply mẹ xong)
                    UpdateSendButtonState();
                }
                break;
                
            case ChatState.BossWaitingReply:
                // Chỉ xử lý nếu đang ở chat sếp
                if (currentSelectedChat == "Boss")
                {
                    // Player reply
                    bossMessagesData.Add(new System.Tuple<string, bool>("Em gửi trong 5 phút nữa", false));
                    VisualElement replyMessage = CreateMessageBubble("Em gửi trong 5 phút nữa", false);
                    ShowMessage(replyMessage);
                    currentState = ChatState.Done;
                    
                    // Disable Send button (đã reply sếp xong)
                    UpdateSendButtonState();
                }
                // Nếu đang ở chat mẹ, không làm gì (phải click vào sếp trước)
                break;
                
            case ChatState.Done:
                // Chat finished, do nothing
                break;
        }
    }
    
    void SwitchToBossChat()
    {
        // Update selected chat
        currentSelectedChat = "Boss";
        UpdateChatSelection();
        
        // Clear message list
        if (messageList != null)
        {
            messageList.Clear();
            // Keep template
            if (messageBubbleTemplate != null)
            {
                messageList.Add(messageBubbleTemplate);
            }
        }
        
        // Hiển thị tin nhắn của sếp nếu đã đến lúc và chưa hiển thị
        if (currentState == ChatState.BossWaitingReply && !bossMessageShown)
        {
            bossMessagesData.Add(new System.Tuple<string, bool>("Sếp: Báo cáo xong chưa?", true));
            VisualElement bossFirstMessage = CreateMessageBubble("Sếp: Báo cáo xong chưa?", true);
            ShowMessage(bossFirstMessage);
            bossMessageShown = true;
            
            // CHỈ BỎ HIGHLIGHT KHI ĐÃ CÓ TIN NHẮN SẾP (sau khi reply mẹ)
            // Đánh dấu đã click vào chat sếp SAU KHI có tin nhắn
            if (!bossChatClicked)
            {
                bossChatClicked = true;
                // Bỏ highlight khi click vào lần đầu SAU KHI có tin nhắn
                if (chatListItemBoss != null)
                {
                    chatListItemBoss.RemoveFromClassList("highlight");
                }
            }
        }
        else if (currentState == ChatState.BossWaitingReply)
        {
            // Đã có tin nhắn rồi, restore lại
            // CHỈ BỎ HIGHLIGHT KHI ĐÃ CÓ TIN NHẮN SẾP
            if (!bossChatClicked)
            {
                bossChatClicked = true;
                // Bỏ highlight khi click vào lần đầu SAU KHI có tin nhắn
                if (chatListItemBoss != null)
                {
                    chatListItemBoss.RemoveFromClassList("highlight");
                }
            }
            
            // Restore lại messages đã lưu (tạo lại từ data)
            foreach (var msgData in bossMessagesData)
            {
                VisualElement msg = CreateMessageBubble(msgData.Item1, msgData.Item2);
                ShowMessage(msg);
            }
        }
        else
        {
            // Chưa có tin nhắn sếp (currentState vẫn là MomWaitingReply)
            // KHÔNG bỏ highlight, vì chưa có gì để đọc
            // Restore lại messages đã lưu (nếu có)
            foreach (var msgData in bossMessagesData)
            {
                VisualElement msg = CreateMessageBubble(msgData.Item1, msgData.Item2);
                ShowMessage(msg);
            }
        }
        
        // Update Send button state khi switch chat
        UpdateSendButtonState();
    }
    
    void SwitchToMomChat()
    {
        // Update selected chat
        currentSelectedChat = "Mom";
        UpdateChatSelection();
        
        // Clear message list
        if (messageList != null)
        {
            messageList.Clear();
            // Keep template
            if (messageBubbleTemplate != null)
            {
                messageList.Add(messageBubbleTemplate);
            }
        }
        
        // Restore lại messages đã lưu (tạo lại từ data)
        foreach (var msgData in momMessagesData)
        {
            VisualElement msg = CreateMessageBubble(msgData.Item1, msgData.Item2);
            ShowMessage(msg);
        }
        
        // Update Send button state khi switch chat
        UpdateSendButtonState();
    }
    
    // Tạo message bubble mới (không add vào list)
    VisualElement CreateMessageBubble(string text, bool isLeft)
    {
        if (messageBubbleTemplate == null) return null;
        
        // Tạo message bubble mới dựa trên template
        VisualElement bubble = new VisualElement();
        
        // Copy styles từ template
        bubble.style.maxWidth = StyleKeyword.Auto;
        bubble.style.width = StyleKeyword.Auto;
        bubble.style.paddingTop = 8f;
        bubble.style.paddingRight = 8f;
        bubble.style.paddingBottom = 8f;
        bubble.style.paddingLeft = 8f;
        bubble.style.marginBottom = 6f;
        bubble.style.borderTopLeftRadius = 12f;
        bubble.style.borderTopRightRadius = 12f;
        bubble.style.borderBottomRightRadius = 12f;
        bubble.style.borderBottomLeftRadius = 12f;
        
        // Set class (left or right)
        if (isLeft)
        {
            bubble.AddToClassList("message-left");
        }
        else
        {
            bubble.AddToClassList("message-right");
        }
        
        // Tạo label cho text
        Label messageText = new Label();
        messageText.name = "MessageText";
        messageText.text = text;
        messageText.style.whiteSpace = WhiteSpace.Normal;
        
        bubble.Add(messageText);
        
        return bubble;
    }
    
    // Hiển thị message bubble vào message list
    void ShowMessage(VisualElement bubble)
    {
        if (messageList == null || bubble == null) return;
        
        // Add to message list (before template)
        messageList.Insert(messageList.childCount - 1, bubble);
    }
    
    void OnDestroy()
    {
        if (sendButton != null)
        {
            sendButton.clicked -= OnSendButtonClicked;
        }
    }
}
