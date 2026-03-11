using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WorkAppUIController : MonoBehaviour
{
    private enum WorkStep
    {
        Quiz,
        Arrange
    }

    [Header("UI References (optional)")]
    public UIDocument uiDocument;
    
    [Header("Audio")]
    public AudioSource workAudioSource;
    public AudioClip quizOptionClickClip;
    public AudioClip answerCorrectClip;
    public AudioClip answerWrongClip;

    private VisualElement root;
    private VisualElement quizPanel;
    private VisualElement arrangePanel;
    private VisualElement optionsContainer;
    private VisualElement arrangeList;

    private Label progressLabel;
    private Label stepTitleLabel;
    private Label questionLabel;
    private Label quizStatusLabel;
    private Label arrangeStatusLabel;

    private Button optionA;
    private Button optionB;
    private Button optionC;
    private Button nextButton;
    private Button completeWorkButton;
    private ComputerUIManager computerUIManager;
    private bool workFlowCompleted = false;

    private WorkStep currentStep = WorkStep.Quiz;

    private int currentQuestionIndex = 0;
    private int selectedOptionIndex = -1;

    private readonly List<Question> questions = new();
    
    // ===== Arrange (drag & drop vào ô trống) =====
    private VisualElement dropSlotsContainer;
    private VisualElement answerPoolContainer;
    
    private readonly List<DropSlot> dropSlots = new();
    
    private struct DropSlot
    {
        public string ExpectedId;          // đáp án đúng cho slot này (theo thứ tự: intro, body, end)
        public string Title;               // label slot nộp bài (Mở đầu / Nội dung / Kết)
        public VisualElement SlotRoot;     // element root của slot (wrapper: title + box)
        public VisualElement Box;          // vùng thả
        public string AssignedId;          // id đang được thả vào (null nếu trống)
        public VisualElement AssignedCard; // card đang nằm trong slot
    }

    // Drag state
    private bool isDragging = false;
    private int draggingPointerId = -1;
    private float dragOffsetX = 0f;
    private float dragOffsetY = 0f;
    private VisualElement draggedCard = null;
    private VisualElement dragOriginalParent = null;
    private int dragOriginalIndex = -1;
    private DropSlot? dragOriginalSlot = null;
    private int hoveredSlotIndex = -1;
    private Vector2 lastPointerPos = Vector2.zero;
    private int dragOriginalPoolSlotIndex = -1;
    private readonly List<VisualElement> poolSlots = new();

    private struct Question
    {
        public string Prompt;
        public string[] Options;
        public int CorrectIndex;
    }

    private void Awake()
    {
        BuildQuestions();
        if (workAudioSource == null)
        {
            workAudioSource = GetComponent<AudioSource>();
        }
        EnsureSfxChannelVolume(workAudioSource);
    }

    // Được gọi từ ComputerUIManager khi mở WorkAppPanel
    public void OnWorkAppOpened()
    {
        if (uiDocument == null)
        {
            uiDocument = FindFirstObjectByType<UIDocument>();
        }

        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            Debug.LogError("WorkAppUIController: Không tìm thấy UIDocument/rootVisualElement.");
            return;
        }

        if (root == null)
        {
            InitializeUI();
        }

        if (root == null)
        {
            // WorkAppViewRoot chưa được instantiate/visible
            return;
        }

        if (computerUIManager == null)
        {
            computerUIManager = FindFirstObjectByType<ComputerUIManager>();
        }

        ResetFlow();
        ShowQuiz();
    }

    private void BuildQuestions()
    {
        questions.Clear();

        // Câu 1: Tổng giờ làm
        questions.Add(new Question
        {
            Prompt = "Câu 1/3: Tổng số giờ làm tuần này (6h + 7h + 5h) bằng bao nhiêu?",
            Options = new[] { "18h", "16h", "20h" },
            CorrectIndex = 0
        });

        // Câu 2: % hoàn thành
        questions.Add(new Question
        {
            Prompt = "Câu 2/3: Hoàn thành 12/15 ticket tương đương bao nhiêu %? (làm tròn)",
            Options = new[] { "80%", "75%", "70%" },
            CorrectIndex = 0
        });

        // Câu 3: Chênh lệch KPI
        questions.Add(new Question
        {
            Prompt = "Câu 3/3: Chênh lệch KPI (95 - 87) bằng bao nhiêu?",
            Options = new[] { "8", "6", "10" },
            CorrectIndex = 0
        });
    }

    private void InitializeUI()
    {
        root = uiDocument.rootVisualElement.Q<VisualElement>("WorkAppViewRoot");
        if (root == null)
        {
            Debug.LogWarning("WorkAppUIController: WorkAppViewRoot chưa sẵn sàng (WorkAppPanel có thể chưa visible).");
            return;
        }

        quizPanel = root.Q<VisualElement>("QuizPanel");
        arrangePanel = root.Q<VisualElement>("ArrangePanel");
        optionsContainer = root.Q<VisualElement>("WorkOptions");
        arrangeList = root.Q<VisualElement>("WorkArrangeList");

        progressLabel = root.Q<Label>("WorkProgressLabel");
        stepTitleLabel = root.Q<Label>("WorkStepTitle");
        questionLabel = root.Q<Label>("WorkQuestionLabel");
        quizStatusLabel = root.Q<Label>("WorkQuizStatusLabel");
        arrangeStatusLabel = root.Q<Label>("WorkArrangeStatusLabel");

        optionA = root.Q<Button>("WorkOptionA");
        optionB = root.Q<Button>("WorkOptionB");
        optionC = root.Q<Button>("WorkOptionC");
        nextButton = root.Q<Button>("WorkNextButton");
        completeWorkButton = root.Q<Button>("CompleteWorkButton");

        if (optionA != null) optionA.clicked += () => SelectOption(0);
        if (optionB != null) optionB.clicked += () => SelectOption(1);
        if (optionC != null) optionC.clicked += () => SelectOption(2);

        if (nextButton != null) nextButton.clicked += OnNextClicked;

        // Nút complete cũ không còn dùng trong flow mới: luôn ẩn/khóa
        if (completeWorkButton != null)
        {
            completeWorkButton.SetEnabled(false);
            completeWorkButton.style.display = DisplayStyle.None;
        }

        // Drag handlers (runtime)
        root.RegisterCallback<PointerMoveEvent>(OnRootPointerMove);
        root.RegisterCallback<PointerUpEvent>(OnRootPointerUp);
        root.RegisterCallback<PointerCancelEvent>(OnRootPointerCancel);
    }

    private void ResetFlow()
    {
        currentStep = WorkStep.Quiz;
        currentQuestionIndex = 0;
        selectedOptionIndex = -1;
        workFlowCompleted = false;

        if (quizStatusLabel != null) quizStatusLabel.text = "";
        if (arrangeStatusLabel != null) arrangeStatusLabel.text = "";

        if (completeWorkButton != null)
        {
            completeWorkButton.SetEnabled(false);
            completeWorkButton.style.display = DisplayStyle.None;
        }
        if (nextButton != null)
        {
            nextButton.text = "Tiếp tục";
            nextButton.SetEnabled(false);
        }

        ClearOptionSelected();

        // Reset arrange state
        isDragging = false;
        draggingPointerId = -1;
        draggedCard = null;
        dragOriginalParent = null;
        dragOriginalIndex = -1;
        dragOriginalSlot = null;
        hoveredSlotIndex = -1;
        dropSlots.Clear();
    }

    private void ShowQuiz()
    {
        currentStep = WorkStep.Quiz;

        if (quizPanel != null) quizPanel.style.display = DisplayStyle.Flex;
        if (arrangePanel != null) arrangePanel.style.display = DisplayStyle.None;

        ShowQuestion(currentQuestionIndex);
    }

    private void ShowArrange()
    {
        currentStep = WorkStep.Arrange;

        if (quizPanel != null) quizPanel.style.display = DisplayStyle.None;
        if (arrangePanel != null) arrangePanel.style.display = DisplayStyle.Flex;

        if (progressLabel != null) progressLabel.text = "2/2";

        if (nextButton != null)
        {
            nextButton.text = "Kiểm tra";
            // Chỉ enable khi đã thả đủ 3 ô
            nextButton.SetEnabled(false);
        }

        BuildArrangeData();
        BuildArrangeUI();
    }

    private void ShowQuestion(int index)
    {
        if (index < 0 || index >= questions.Count) return;

        selectedOptionIndex = -1;
        ClearOptionSelected();

        if (nextButton != null) nextButton.SetEnabled(false);
        if (quizStatusLabel != null) quizStatusLabel.text = "";

        var q = questions[index];

        // Progress ở header theo "2 state" (Quiz/Arrange), còn số câu nằm trong prompt "Câu x/3"
        if (progressLabel != null) progressLabel.text = "1/2";
        if (questionLabel != null) questionLabel.text = q.Prompt;

        if (optionA != null) optionA.text = $"A) {q.Options[0]}";
        if (optionB != null) optionB.text = $"B) {q.Options[1]}";
        if (optionC != null) optionC.text = $"C) {q.Options[2]}";
    }

    private void SelectOption(int optionIndex)
    {
        selectedOptionIndex = optionIndex;
        ClearOptionSelected();
        PlayWorkClip(quizOptionClickClip);

        GetOptionButton(optionIndex)?.AddToClassList("work-option-selected");

        if (nextButton != null) nextButton.SetEnabled(true);
    }

    private void OnNextClicked()
    {
        if (currentStep == WorkStep.Quiz)
        {
            ValidateQuizAnswer();
            return;
        }

        if (currentStep == WorkStep.Arrange)
        {
            ValidateArrange();
            return;
        }
    }

    private void ValidateQuizAnswer()
    {
        if (selectedOptionIndex < 0)
        {
            if (quizStatusLabel != null) quizStatusLabel.text = "Chọn 1 đáp án trước đã.";
            return;
        }

        var q = questions[currentQuestionIndex];
        if (selectedOptionIndex != q.CorrectIndex)
        {
            if (quizStatusLabel != null) quizStatusLabel.text = "Sai rồi. Thử lại!";
            PlayWorkClip(answerWrongClip);
            // Giữ nguyên câu hỏi, cho chọn lại
            selectedOptionIndex = -1;
            ClearOptionSelected();
            if (nextButton != null) nextButton.SetEnabled(false);
            return;
        }
        PlayWorkClip(answerCorrectClip);

        currentQuestionIndex++;
        if (currentQuestionIndex >= questions.Count)
        {
            if (progressLabel != null) progressLabel.text = "3/3";
            ShowArrange();
            return;
        }

        ShowQuestion(currentQuestionIndex);
    }

    private void BuildArrangeData()
    {
        // 3 slot nộp bài trên (có title) + 3 ô trống dưới
        dropSlots.Clear();
        dropSlots.Add(new DropSlot { ExpectedId = "intro", Title = "Mở đầu" });
        dropSlots.Add(new DropSlot { ExpectedId = "body", Title = "Nội dung" });
        dropSlots.Add(new DropSlot { ExpectedId = "end", Title = "Kết" });

        if (arrangeStatusLabel != null) arrangeStatusLabel.text = "";
    }

    private void BuildArrangeUI()
    {
        if (arrangeList == null) return;

        // Query containers từ UXML (đã được add sẵn trong WorkArrangeList)
        dropSlotsContainer = arrangeList.Q<VisualElement>("DropSlotsContainer");
        answerPoolContainer = arrangeList.Q<VisualElement>("AnswerPoolContainer");

        if (dropSlotsContainer == null || answerPoolContainer == null)
        {
            Debug.LogWarning("WorkAppUIController: Không tìm thấy DropSlotsContainer/AnswerPoolContainer trong WorkArrangeList.");
            return;
        }

        dropSlotsContainer.Clear();
        answerPoolContainer.Clear();
        poolSlots.Clear();

        // Build 3 slot nộp bài trên (có title: Mở đầu / Nội dung / Kết)
        for (int i = 0; i < dropSlots.Count; i++)
        {
            var slot = dropSlots[i];

            var slotRoot = new VisualElement();
            slotRoot.name = $"DropSlot_{slot.ExpectedId}";
            slotRoot.AddToClassList("work-drop-slot");

            var title = new Label(slot.Title);
            title.AddToClassList("work-drop-slot-title");

            var box = new VisualElement();
            box.name = "Box";
            box.AddToClassList("work-drop-slot-box");

            slotRoot.Add(title);
            slotRoot.Add(box);

            slot.SlotRoot = slotRoot;
            slot.Box = box;
            slot.AssignedId = null;
            slot.AssignedCard = null;

            dropSlots[i] = slot;
            dropSlotsContainer.Add(slotRoot);
        }

        // Build 3 ô dưới — không title, chỉ box (vùng chứa nội dung)
        for (int i = 0; i < 3; i++)
        {
            var box = new VisualElement();
            box.name = $"PoolSlot_{i}";
            box.AddToClassList("work-drop-slot");
            box.AddToClassList("work-drop-slot-box");

            poolSlots.Add(box);
            answerPoolContainer.Add(box);
        }

        // Build answer cards (dưới) - shuffle
        var answers = new List<(string id, string text)>
        {
            ("intro", "Mở đầu: Kính gửi sếp, em gửi báo cáo tóm tắt tiến độ công việc hôm nay."),
            ("body", "Nội dung: Em đã hoàn thành phần tổng hợp số liệu và rà soát các hạng mục còn tồn đọng."),
            ("end",  "Kết: Nếu không có phát sinh, em sẽ gửi bản đầy đủ trước 17:00. Trân trọng.")
        };

        var rng = new System.Random();
        for (int i = answers.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (answers[i], answers[j]) = (answers[j], answers[i]);
        }

        foreach (var ans in answers)
        {
            var card = new VisualElement();
            card.name = $"AnswerCard_{ans.id}";
            card.userData = ans.id;
            card.AddToClassList("work-answer-card");

            var text = new Label(ans.text);
            text.AddToClassList("work-answer-text");
            card.Add(text);

            // Kéo card (từ pool hoặc từ slot)
            card.RegisterCallback<PointerDownEvent>(evt => BeginDragCard(card, evt));

            AddCardToPool(card);
        }

        UpdateArrangeNextButton();
    }

    private void AddCardToPool(VisualElement card, int preferredSlotIndex = -1)
    {
        if (answerPoolContainer == null) return;
        if (card == null) return;

        card.RemoveFromHierarchy();

        // Ưu tiên đúng ô được chọn (ô dưới con trỏ khi thả, hoặc ô cũ khi return origin)
        if (preferredSlotIndex >= 0 && preferredSlotIndex < poolSlots.Count)
        {
            var preferred = poolSlots[preferredSlotIndex];
            if (preferred != null)
            {
                if (preferred.childCount == 0)
                {
                    preferred.Add(card);
                    return;
                }
                // Ô đã có card: đặt card mới vào đây, đẩy card cũ sang ô trống đầu tiên (không ép thứ tự trái-phải)
                var displaced = preferred[0];
                displaced.RemoveFromHierarchy();
                preferred.Add(card);
                AddCardToPool(displaced, -1);
                return;
            }
        }

        // Không có ô ưu tiên hoặc -1: tìm ô trống đầu tiên
        for (int i = 0; i < poolSlots.Count; i++)
        {
            var slot = poolSlots[i];
            if (slot != null && slot.childCount == 0)
            {
                slot.Add(card);
                return;
            }
        }

        // Fallback (không mong xảy ra): add trực tiếp
        answerPoolContainer.Add(card);
    }

    private int FindPoolSlotIndex(VisualElement maybePoolSlot)
    {
        if (maybePoolSlot == null) return -1;
        for (int i = 0; i < poolSlots.Count; i++)
        {
            if (poolSlots[i] == maybePoolSlot) return i;
        }
        return -1;
    }

    /// <summary>Trả về (true, index) nếu đang ở ô nộp bài trên, (false, index) nếu ở ô pool dưới; index -1 nếu không nằm trong ô nào. Cùng logic cho cả 6 ô (3 trên + 3 dưới cùng định dạng).</summary>
    private (bool isDropSlot, int index) GetSlotAtPosition(Vector2 worldPos)
    {
        for (int i = 0; i < dropSlots.Count; i++)
        {
            if (dropSlots[i].Box != null && dropSlots[i].Box.worldBound.Contains(worldPos))
                return (true, i);
        }
        for (int i = 0; i < poolSlots.Count; i++)
        {
            if (poolSlots[i] != null && poolSlots[i].worldBound.Contains(worldPos))
                return (false, i);
        }
        return (false, -1);
    }

    private void BeginDragCard(VisualElement card, PointerDownEvent evt)
    {
        if (currentStep != WorkStep.Arrange) return;
        if (isDragging) return;
        if (root == null) return;
        if (dropSlotsContainer == null || answerPoolContainer == null) return;

        // Không cho kéo sau khi đã xác nhận đúng toàn bộ
        if (workFlowCompleted) return;

        isDragging = true;
        draggingPointerId = evt.pointerId;
        draggedCard = card;
        lastPointerPos = evt.position;

        dragOriginalParent = card.parent;
        dragOriginalIndex = dragOriginalParent != null ? dragOriginalParent.IndexOf(card) : -1;
        dragOriginalSlot = FindSlotByBox(dragOriginalParent);
        dragOriginalPoolSlotIndex = FindPoolSlotIndex(dragOriginalParent);

        // Nếu kéo từ slot, clear assignment trước (để có thể drop chỗ khác)
        if (dragOriginalSlot.HasValue)
        {
            ClearSlotAssignment(dragOriginalSlot.Value.ExpectedId);
        }

        float w = card.worldBound.width;
        float h = card.worldBound.height;

        dragOffsetX = evt.position.x - card.worldBound.xMin;
        dragOffsetY = evt.position.y - card.worldBound.yMin;

        card.RemoveFromHierarchy();
        card.AddToClassList("work-answer-ghost");
        card.style.position = Position.Absolute;
        card.style.width = w;
        card.style.height = h;
        card.style.left = (evt.position.x - root.worldBound.xMin) - dragOffsetX;
        card.style.top = (evt.position.y - root.worldBound.yMin) - dragOffsetY;
        root.Add(card);

        hoveredSlotIndex = -1;
        root.CapturePointer(draggingPointerId);
        evt.StopPropagation();
    }

    private void OnRootPointerMove(PointerMoveEvent evt)
    {
        if (!isDragging) return;
        if (evt.pointerId != draggingPointerId) return;
        if (draggedCard == null || root == null) return;

        lastPointerPos = evt.position;
        draggedCard.style.left = (evt.position.x - root.worldBound.xMin) - dragOffsetX;
        draggedCard.style.top = (evt.position.y - root.worldBound.yMin) - dragOffsetY;

        int newHover = FindHoveredSlotIndex(evt.position);
        if (newHover != hoveredSlotIndex)
        {
            SetHoveredSlot(hoveredSlotIndex, false);
            hoveredSlotIndex = newHover;
            SetHoveredSlot(hoveredSlotIndex, true);
        }

        evt.StopPropagation();
    }

    private void OnRootPointerUp(PointerUpEvent evt)
    {
        if (!isDragging) return;
        if (evt.pointerId != draggingPointerId) return;
        lastPointerPos = evt.position;
        EndDrag(evt.position);
        evt.StopPropagation();
    }

    private void OnRootPointerCancel(PointerCancelEvent evt)
    {
        if (!isDragging) return;
        if (evt.pointerId != draggingPointerId) return;
        EndDrag(lastPointerPos);
        evt.StopPropagation();
    }

    private void EndDrag(Vector2 dropPos)
    {
        if (root != null && draggingPointerId != -1 && root.HasPointerCapture(draggingPointerId))
        {
            root.ReleasePointer(draggingPointerId);
        }

        SetHoveredSlot(hoveredSlotIndex, false);

        if (draggedCard != null)
        {
            var (isDropSlot, slotIndex) = GetSlotAtPosition(dropPos);

            draggedCard.RemoveFromClassList("work-answer-ghost");
            draggedCard.style.position = StyleKeyword.Null;
            draggedCard.style.left = StyleKeyword.Null;
            draggedCard.style.top = StyleKeyword.Null;
            draggedCard.style.width = StyleKeyword.Null;
            draggedCard.style.height = StyleKeyword.Null;

            if (isDropSlot && slotIndex >= 0)
            {
                AssignCardToSlot(slotIndex, draggedCard);
            }
            else if (!isDropSlot && slotIndex >= 0)
            {
                // Thả vào đúng ô pool dưới con trỏ (cùng định dạng ô trên)
                AddCardToPool(draggedCard, slotIndex);
            }
            else
            {
                ReturnDraggedCardToOrigin(draggedCard);
            }
        }

        isDragging = false;
        draggingPointerId = -1;
        draggedCard = null;
        dragOriginalParent = null;
        dragOriginalIndex = -1;
        dragOriginalSlot = null;
        dragOriginalPoolSlotIndex = -1;
        hoveredSlotIndex = -1;

        UpdateArrangeNextButton();
    }

    private void ValidateArrange()
    {
        // Chỉ kiểm tra khi đã thả đủ 3 ô
        for (int i = 0; i < dropSlots.Count; i++)
        {
            if (string.IsNullOrEmpty(dropSlots[i].AssignedId))
            {
                if (arrangeStatusLabel != null) arrangeStatusLabel.text = "Bạn cần kéo đủ 3 đáp án vào 3 ô trống.";
                return;
            }
        }

        bool ok = true;
        for (int i = 0; ok && i < dropSlots.Count; i++)
        {
            ok = string.Equals(dropSlots[i].AssignedId, dropSlots[i].ExpectedId, StringComparison.Ordinal);
        }

        if (!ok)
        {
            if (arrangeStatusLabel != null) arrangeStatusLabel.text = "Chưa đúng. Hãy kéo lại đúng ô tương ứng.";
            PlayWorkClip(answerWrongClip);
            return;
        }

        workFlowCompleted = true;
        PlayWorkClip(answerCorrectClip);
        if (arrangeStatusLabel != null) arrangeStatusLabel.text = "Đã hoàn thành công việc";
        if (computerUIManager == null)
        {
            computerUIManager = FindFirstObjectByType<ComputerUIManager>();
        }
        computerUIManager?.CompleteWorkTask();

        // Không cần bấm “Kiểm tra” nữa
        if (nextButton != null) nextButton.SetEnabled(false);
    }

    private int FindHoveredSlotIndex(Vector2 pointerPos)
    {
        for (int i = 0; i < dropSlots.Count; i++)
        {
            var box = dropSlots[i].Box;
            if (box == null) continue;
            if (box.worldBound.Contains(pointerPos))
            {
                return i;
            }
        }
        return -1;
    }

    private void SetHoveredSlot(int index, bool hovered)
    {
        if (index < 0 || index >= dropSlots.Count) return;
        var slotRoot = dropSlots[index].SlotRoot;
        if (slotRoot == null) return;
        if (hovered) slotRoot.AddToClassList("work-drop-slot-hover");
        else slotRoot.RemoveFromClassList("work-drop-slot-hover");
    }

    private DropSlot? FindSlotByBox(VisualElement maybeBox)
    {
        if (maybeBox == null) return null;
        for (int i = 0; i < dropSlots.Count; i++)
        {
            if (dropSlots[i].Box == maybeBox) return dropSlots[i];
        }
        return null;
    }

    private void ClearSlotAssignment(string expectedId)
    {
        for (int i = 0; i < dropSlots.Count; i++)
        {
            if (!string.Equals(dropSlots[i].ExpectedId, expectedId, StringComparison.Ordinal)) continue;

            var slot = dropSlots[i];
            slot.AssignedId = null;
            slot.AssignedCard = null;
            dropSlots[i] = slot;
            return;
        }
    }

    private void AssignCardToSlot(int slotIndex, VisualElement card)
    {
        if (slotIndex < 0 || slotIndex >= dropSlots.Count) return;

        var slot = dropSlots[slotIndex];
        if (slot.Box == null) return;

        // Nếu slot đã có card thì trả card cũ về pool
        if (slot.AssignedCard != null)
        {
            AddCardToPool(slot.AssignedCard);
        }

        card.RemoveFromHierarchy();
        slot.Box.Add(card);

        slot.AssignedCard = card;
        slot.AssignedId = card.userData as string;
        dropSlots[slotIndex] = slot;
    }

    private void ReturnDraggedCardToOrigin(VisualElement card)
    {
        // Nếu origin là slot box, đặt lại vào slot đó
        if (dragOriginalSlot.HasValue)
        {
            for (int i = 0; i < dropSlots.Count; i++)
            {
                if (string.Equals(dropSlots[i].ExpectedId, dragOriginalSlot.Value.ExpectedId, StringComparison.Ordinal))
                {
                    AssignCardToSlot(i, card);
                    return;
                }
            }
        }

        // Mặc định trả về pool
        AddCardToPool(card, dragOriginalPoolSlotIndex);
    }

    private void UpdateArrangeNextButton()
    {
        if (nextButton == null) return;
        if (currentStep != WorkStep.Arrange) return;

        bool allFilled = true;
        for (int i = 0; allFilled && i < dropSlots.Count; i++)
        {
            allFilled = !string.IsNullOrEmpty(dropSlots[i].AssignedId);
        }

        nextButton.SetEnabled(allFilled);
    }

    private void ClearOptionSelected()
    {
        optionA?.RemoveFromClassList("work-option-selected");
        optionB?.RemoveFromClassList("work-option-selected");
        optionC?.RemoveFromClassList("work-option-selected");
    }

    private Button GetOptionButton(int optionIndex)
    {
        return optionIndex switch
        {
            0 => optionA,
            1 => optionB,
            2 => optionC,
            _ => null
        };
    }
    
    private void PlayWorkClip(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        EnsureSfxChannelVolume(workAudioSource);

        if (workAudioSource != null)
        {
            workAudioSource.PlayOneShot(clip);
            return;
        }

        if (Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
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
}

