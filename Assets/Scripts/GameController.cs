using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class GameController : GameBaseController
{
    public static GameController Instance = null;
    public CharacterSet[] characterSets;
    public GridManager gridManager;
    public Cell[,] grid;
    public GameObject playerPrefab;
    public Transform parent;
    public Sprite[] defaultAnswerBox;
    public List<PlayerController> playerControllers = new List<PlayerController>();
    public bool showCells = false;
    public CanvasGroup[] audioTypeButtons, fillInBlankTypeButtons;
    public TextMeshProUGUI choiceText;

    protected override void Awake()
    {
        if (Instance == null) Instance = this;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        this.CreateGrids();
    }

    void CreateGrids()
    {
        Sprite gridTexture = LoaderConfig.Instance.gameSetup.gridTexture != null ?
                            SetUI.ConvertTextureToSprite(LoaderConfig.Instance.gameSetup.gridTexture as Texture2D) : null;
        this.grid = gridManager.CreateGrid(gridTexture);
    }

    private IEnumerator InitialQuestion()
    {
        this.createPlayer();

        var questionController = QuestionController.Instance;
        if(questionController == null) yield break;
        questionController.nextQuestion();
        this.controlDuplicateUIButtons(questionController);

        yield return new WaitForEndOfFrame();

        if (questionController.currentQuestion.answersChoics != null &&
            questionController.currentQuestion.answersChoics.Length > 0)
        {
            string[] answers = questionController.currentQuestion.answersChoics;
            this.gridManager.UpdateGridWithWord(answers, null);
        }
        else
        {
            string word = questionController.currentQuestion.correctAnswer;
            this.gridManager.UpdateGridWithWord(null, word);
        }
    }

    void createPlayer()
    {
        var cellPositions = this.gridManager.availablePositions;
        var characterPositionList = this.gridManager.CharacterStartupPositionsCellIds;

        for (int i = 0; i < this.maxPlayers; i++)
        {
            if (i < this.playerNumber)
            {
                var playerController = GameObject.Instantiate(this.playerPrefab, this.parent).GetComponent<PlayerController>();
                playerController.gameObject.name = "Player_" + i;
                playerController.UserId = i;
                this.playerControllers.Add(playerController);
                var cellVector2 = cellPositions[characterPositionList[i]];
                Vector3 actualCellPosition = this.gridManager.cells[cellVector2.x, cellVector2.y].transform.localPosition;
                this.gridManager.cells[cellVector2.x, cellVector2.y].setCellEnterColor(true);
                this.playerControllers[i].Init(this.characterSets[i], this.defaultAnswerBox, actualCellPosition);

                if (i == 0 && LoaderConfig.Instance != null && LoaderConfig.Instance.apiManager.peopleIcon != null)
                {
                    var _playerName = LoaderConfig.Instance?.apiManager.loginName;
                    var icon = SetUI.ConvertTextureToSprite(LoaderConfig.Instance.apiManager.peopleIcon as Texture2D);
                    this.playerControllers[i].UserName = _playerName;
                    this.playerControllers[i].updatePlayerIcon(true, _playerName, icon);
                }
                else
                {
                    var icon = SetUI.ConvertTextureToSprite(this.characterSets[i].defaultIcon as Texture2D);
                    this.playerControllers[i].updatePlayerIcon(true, null, icon);
                }
            }
            else
            {
                int notUsedId = i + 1;
                var notUsedPlayerIcon = GameObject.FindGameObjectWithTag("P" + notUsedId + "_Icon");
                if (notUsedPlayerIcon != null) { 
                    var notUsedIcon = notUsedPlayerIcon.GetComponent<PlayerIcon>();

                    if(notUsedIcon != null)
                    {
                        notUsedIcon.HiddenIcon();
                    }
                    //notUsedPlayerIcon.SetActive(false);
                }

                var notUsedPlayerController = GameObject.FindGameObjectWithTag("P" + notUsedId + "-controller");
                if (notUsedPlayerController != null)
                {
                    var notUsedMoveController = notUsedPlayerController.GetComponent<CharacterMoveController>();
                    notUsedMoveController.TriggerActive(false);
                }
                   // notUsedPlayerController.SetActive(false);
            }
        }
    }


    public override void enterGame()
    {
        base.enterGame();
        StartCoroutine(this.InitialQuestion());
    }

    public override void endGame()
    {
        bool showSuccess = false;
        for (int i = 0; i < this.playerControllers.Count; i++)
        {
            if(i < this.playerNumber)
            {
                var playerController = this.playerControllers[i];
                if (playerController != null)
                {
                    if (playerController.Score >= 30)
                    {
                        showSuccess = true;
                    }
                    this.endGamePage.updateFinalScore(i, playerController.Score);
                }
            }
        }
        this.endGamePage.setStatus(true, showSuccess);
        base.endGame();
    }

    void controlDuplicateUIButtons(QuestionController questionController = null)
    {
        var currentQuestion = questionController.currentQuestion;
        switch (currentQuestion.questiontype)
        {
            case QuestionType.Audio:
                SetUI.SetWholeGroupTo(this.audioTypeButtons, true);
                break;
            case QuestionType.FillInBlank:
                SetUI.SetWholeGroupTo(this.fillInBlankTypeButtons, true);
                break;
            case QuestionType.None:
            case QuestionType.Picture:
            case QuestionType.Text:
                SetUI.SetWholeGroupTo(this.audioTypeButtons, false);
                SetUI.SetWholeGroupTo(this.fillInBlankTypeButtons, false);
                break;
            default:
                SetUI.SetWholeGroupTo(this.audioTypeButtons, false);
                SetUI.SetWholeGroupTo(this.fillInBlankTypeButtons, false);
                break;
        }

        if (this.choiceText != null)
        {
            string formattedText = ""; // Initialize a string to hold the formatted answers

            for (int i = 0; i < currentQuestion.answersChoics.Length; i++)
            {
                char label = (char)('A' + i); // Convert index to corresponding letter
                formattedText += label + ": " + currentQuestion.answersChoics[i] + "\n"; // Append each formatted answer
            }
            this.choiceText.text = formattedText; // Set the combined text to the txt
        }
    }

    public void PrepareNextQuestion()
    {
        LogController.Instance?.debug("Prepare Next Question");
        for (int i = 0; i < this.playerNumber; i++)
        {
            if (this.playerControllers[i] != null)
            {
                this.playerControllers[i].characterStatus = CharacterStatus.nextQA;
            }
        }
    }

    public void UpdateNextQuestion()
    {
        this.playersResetPosition();
        LogController.Instance?.debug("Next Question");
        var questionController = QuestionController.Instance;

        if (questionController != null) {
            questionController.nextQuestion();

            this.controlDuplicateUIButtons(questionController);

            if (questionController.currentQuestion.answersChoics != null &&
                questionController.currentQuestion.answersChoics.Length > 0)
            {
                string[] answers = questionController.currentQuestion.answersChoics;
                this.gridManager.UpdateGridWithWord(answers, null);
            }
            else
            {
                string word = questionController.currentQuestion.correctAnswer;
                this.gridManager.UpdateGridWithWord(null, word);
            }
        }       
    }

    void playersResetPosition()
    {
        var cellPositions = this.gridManager.availablePositions;
        var characterPositionList = this.gridManager.CharacterStartupPositionsCellIds;

        for (int i = 0; i < this.playerNumber; i++)
        {
            if (this.playerControllers[i] != null)
            {
                var cellVector2 = cellPositions[characterPositionList[i]];
                Vector3 actualCellPosition = this.gridManager.cells[cellVector2.x, cellVector2.y].transform.localPosition;
                this.gridManager.cells[cellVector2.x, cellVector2.y].setCellEnterColor(true);
                this.playerControllers[i].resetRetryTime();
                this.playerControllers[i].collectedCell.Clear();
                this.playerControllers[i].playerReset(actualCellPosition);
            }
        }
    }
   
    
    private void Update()
    {
        if(!this.playing) return;

        if(Input.GetKeyDown(KeyCode.F1))
        {
            this.showCells = !this.showCells;
            this.gridManager.setAllCellsStatus(this.showCells);
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            this.playersResetPosition();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            this.UpdateNextQuestion();
        }

        /*if (this.playerControllers.Count == 0) return;

        for (int j = 0; j < SortOrderController.Instance.roads.Length; j++)
        {
            var road = SortOrderController.Instance.roads[j];
            road.showRoadHint(false);
            for (int i = 0; i < this.playerNumber; i++)
            {
                var characterCanvas = this.playerControllers[i].characterCanvas;
                if (characterCanvas.sortingOrder == road.orderLayer && this.showCells)
                {
                    road.showRoadHint(true);
                }
            }  
        }

       // bool anyPlayerReachToSubmitPoint = false;

        for (int i = 0; i < this.playerNumber; i++)
        {
            if (this.playerControllers[i] != null)
            {
                var player = this.playerControllers[i];
                switch (player.stayTrail)
                {
                    case StayTrail.submitPoint:
                        if (player.Retry > 0 && !this.showingPopup)
                        {
                            int currentTime = Mathf.FloorToInt(((this.gameTimer.gameDuration - this.gameTimer.currentTime) / this.gameTimer.gameDuration) * 100);
                            this.playerControllers[i].checkAnswer(currentTime, () =>
                            {
                                for (int i = 0; i < this.playerNumber; i++)
                                {
                                    if (this.playerControllers[i] != null)
                                    {
                                        this.playerControllers[i].playerReset();
                                    }
                                }
                            });
                        }
                        break;
                    case StayTrail.startPoints:
                        player.autoDeductAnswer();
                        break;
                }
            }
        }

        bool isBattleMode = this.playerNumber > 1;

        if (isBattleMode)
        {
            bool isNextQuestion = true;

            for (int i = 0; i < this.playerNumber; i++)
            {
                if (this.playerControllers[i] == null || !this.playerControllers[i].IsTriggerToNextQuestion)
                {
                    isNextQuestion = false;
                    break;
                }
            }

            if (isNextQuestion)
            {
                this.UpdateNextQuestion();
            }
        }
        else
        {
            if (this.playerControllers[0] != null && this.playerControllers[0].IsTriggerToNextQuestion)
            {
                this.UpdateNextQuestion();
            }
        }
        */

    } 
}


public enum CharacterStatus
{
    born,
    idling,
    rotating,
    moving,
    nextQA,
    recover
}