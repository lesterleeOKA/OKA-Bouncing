using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : UserData
{
    public CharacterMoveController moveButton;
    //public BloodController bloodController;
    public CharacterStatus characterStatus = CharacterStatus.idling;
    public Scoring scoring;
    public string answer = string.Empty;
    public bool IsCorrect = false;
    public bool IsTriggerToNextQuestion = false;
    public bool IsCheckedAnswer = false;
    public CanvasGroup answerBoxCg;
    public Image answerBoxFrame;
    public float speed;
    [HideInInspector]
    public Transform characterTransform;
    [HideInInspector]
    public Canvas characterCanvas = null;
    public Vector3 startPosition = Vector3.zero;
    public int characterOrder = 11;
    private CharacterAnimation characterAnimation = null;
    private TextMeshProUGUI answerBox = null;
    public List<Cell> collectedCell = new List<Cell>();
    public float countGetAnswerAtStartPoints = 2f;
    private float countAtStartPoints = 0f;

    public RectTransform rectTransform = null;
    public float rotationSpeed = 200f; // Speed of rotation
    public float moveSpeed = 5f; // Speed of movement
    private Rigidbody2D rb = null;
   // public bool isRotating = true;
    public Vector3 playerCurrentPosition = Vector3.zero;
    private float randomDirection;
    private Vector2 moveDirection;
    public float reduceBaseFactor = 0.93f;
    private float reducedFactor = 0f;
    public CanvasGroup bornParticle;
    public GameObject playerAppearEffect;
    public GameObject[] answerParticles;
    public float resetCount = 5.0f;

    public void Init(CharacterSet characterSet = null, Sprite[] defaultAnswerBoxes = null, Vector3 startPos = default)
    {
        if(LoaderConfig.Instance.gameSetup.playersMovingSpeed > 0f)
        {
            this.moveSpeed = LoaderConfig.Instance.gameSetup.playersMovingSpeed;
        }

        if(LoaderConfig.Instance.gameSetup.playersRotationSpeed > 0f)
        {
            this.rotationSpeed = LoaderConfig.Instance.gameSetup.playersRotationSpeed;
        }

        for (int i=0; i < this.answerParticles.Length; i++)
        {
            if(this.answerParticles[i] != null)
            {
                if(i == this.UserId)
                {
                    this.answerParticles[i].SetActive(true);
                }
                else
                {
                    this.answerParticles[i].SetActive(false);
                }
            }
        }
        this.GetComponent<CircleCollider2D>().enabled = true;
        SetUI.Set(this.bornParticle, true, 1f);
        this.characterStatus = CharacterStatus.born;
        this.transform.DOScale(1f, 1f).OnComplete(()=>
        {
            this.characterStatus = CharacterStatus.idling;
            this.playerAppearEffect.SetActive(false);
            SetUI.Set(this.bornParticle, false, 1f);
        });
        this.rb = GetComponent<Rigidbody2D>();
        this.SetRandomRotationDirection();

        this.countAtStartPoints = this.countGetAnswerAtStartPoints;
        this.updateRetryTimes(false);
        this.startPosition = startPos;
        this.characterTransform = this.transform;
        this.characterTransform.localPosition = this.startPosition;
        this.characterCanvas = this.GetComponent<Canvas>();
        this.characterCanvas.sortingOrder = this.characterOrder;
        this.characterAnimation = this.GetComponent<CharacterAnimation>();
        this.characterAnimation.characterSet = characterSet;

        if(this.answerBoxCg != null ) {
            this.answerBoxCg.transform.localScale = Vector3.zero;
            if (this.UserId < 2)
            {
                this.answerBoxCg.transform.localPosition = new Vector2(60f, -60f);
            }
            else
            {
                this.answerBoxCg.transform.localPosition = new Vector2(-60f, 60f);
            }
            SetUI.SetScale(this.answerBoxCg, false);
            this.answerBox = this.answerBoxCg.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (this.moveButton == null)
        {
            this.moveButton = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "-controller").GetComponent<CharacterMoveController>();
            this.moveButton.OnPointerDownEvent += this.StopRotation;
            this.moveButton.OnPointerUpEvent += this.StopMove;
        }

        /*if (this.bloodController == null)
        {
            this.bloodController = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_Blood").GetComponent<BloodController>();
        }
        */
        if (this.PlayerIcons[0] == null)
        {
            this.PlayerIcons[0] = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_Icon").GetComponent<PlayerIcon>();
        }

        if (this.scoring.scoreTxt == null)
        {
            this.scoring.scoreTxt = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_Score").GetComponent<TextMeshProUGUI>();
        }

        if (this.scoring.answeredEffectTxt == null)
        {
            this.scoring.answeredEffectTxt = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_AnswerScore").GetComponent<TextMeshProUGUI>();
        }

        if (this.scoring.resultScoreTxt == null)
        {
            this.scoring.resultScoreTxt = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_ResultScore").GetComponent<TextMeshProUGUI>();
        }

        this.scoring.init();
        this.characterStatus = CharacterStatus.rotating;
        this.reducedFactor = this.reduceBaseFactor;
    }

    void updateRetryTimes(bool deduct = false)
    {
        if (deduct)
        {
            if (this.Retry > 0)
            {
                this.Retry--;
            }

            /*if (this.bloodController != null)
            {
                this.bloodController.setBloods(false);
            }*/
        }
        else
        {
            this.NumberOfRetry = LoaderConfig.Instance.gameSetup.retry_times;
            this.Retry = this.NumberOfRetry;
        }
    }

    public void updatePlayerIcon(bool _status = false, string _playerName = "", Sprite _icon = null)
    {
        for (int i = 0; i < this.PlayerIcons.Length; i++)
        {
            if (this.PlayerIcons[i] != null)
            {
                this.PlayerColor = this.characterAnimation.characterSet.playerColor;
                this.PlayerIcons[i].playerColor = this.characterAnimation.characterSet.playerColor;
                this.PlayerIcons[i].SetStatus(_status, _playerName, _icon);
            }
        }

    }


    string CapitalizeFirstLetter(string str)
    {
        if (string.IsNullOrEmpty(str)) return str; // Return if the string is empty or null
        return char.ToUpper(str[0]) + str.Substring(1).ToLower();
    }

    public void checkAnswer(int currentTime, Action onCompleted = null)
    {
        var currentQuestion = QuestionController.Instance?.currentQuestion;
        if(currentQuestion.answersChoics == null || currentQuestion.answersChoics.Length == 0) return;

        var getChoice = this.answerBox.text;
        var lowerQIDAns = currentQuestion.correctAnswer.ToLower();
        switch (getChoice)
        {
            case "A":
                this.answer = currentQuestion.answersChoics[0].ToLower();
                break;
            case "B":
                this.answer = currentQuestion.answersChoics[1].ToLower();
                break;
            case "C":
                this.answer = currentQuestion.answersChoics[2].ToLower();
                break;
            case "D":
                this.answer = currentQuestion.answersChoics[3].ToLower();
                break;
        }
        if(this.answer != lowerQIDAns) { 
            this.backToStartPosition();
            return;
        }

        if (!this.IsCheckedAnswer)
        {
            this.IsCheckedAnswer = true;
            var loader = LoaderConfig.Instance;
            int eachQAScore = currentQuestion.qa.score.full == 0 ? 10 : currentQuestion.qa.score.full;
            int currentScore = this.Score;

            int resultScore = this.scoring.score(this.answer, currentScore, lowerQIDAns, eachQAScore);
            this.Score = resultScore;
            this.IsCorrect = this.scoring.correct;
            StartCoroutine(this.showAnswerResult(this.scoring.correct,()=>
            {
                if (this.UserId == 0 && loader != null && loader.apiManager.IsLogined) // For first player
                {
                    float currentQAPercent = 0f;
                    int correctId = 0;
                    float score = 0f;
                    float answeredPercentage;
                    int progress = (int)((float)currentQuestion.answeredQuestion / QuestionManager.Instance.totalItems * 100);

                    if (this.answer == lowerQIDAns)
                    {
                        if (this.CorrectedAnswerNumber < QuestionManager.Instance.totalItems)
                            this.CorrectedAnswerNumber += 1;

                        correctId = 2;
                        score = eachQAScore; // load from question settings score of each question

                        LogController.Instance?.debug("Each QA Score!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + eachQAScore + "______answer" + this.answer);
                        currentQAPercent = 100f;
                    }
                    else
                    {
                        if (this.CorrectedAnswerNumber > 0)
                        {
                            this.CorrectedAnswerNumber -= 1;
                        }
                    }

                    if (this.CorrectedAnswerNumber < QuestionManager.Instance.totalItems)
                    {
                        answeredPercentage = this.AnsweredPercentage(QuestionManager.Instance.totalItems);
                    }
                    else
                    {
                        answeredPercentage = 100f;
                    }

                    loader.SubmitAnswer(
                               currentTime,
                               this.Score,
                               answeredPercentage,
                               progress,
                               correctId,
                               currentTime,
                               currentQuestion.qa.qid,
                               currentQuestion.correctAnswerId,
                               this.CapitalizeFirstLetter(this.answer),
                               currentQuestion.correctAnswer,
                               score,
                               currentQAPercent,
                               onCompleted
                               );
                }
                else
                {
                   onCompleted?.Invoke();
                }
            }, ()=>
            {
                onCompleted?.Invoke();
            }));
        }
    }

    public void resetRetryTime()
    {
        this.scoring.resetText();
        this.updateRetryTimes(false);
       // this.bloodController.setBloods(true);
        this.IsTriggerToNextQuestion = false;
    }

    public IEnumerator showAnswerResult(bool correct, Action onCorrectCompleted = null, Action onFailureCompleted = null)
    {
        float delay = 2f;
        if (correct)
        {
            GameController.Instance?.PrepareNextQuestion();
            LogController.Instance?.debug("Add marks" + this.Score);
            GameController.Instance?.setGetScorePopup(true);
            AudioController.Instance?.PlayAudio(1);
            onCorrectCompleted?.Invoke();
            yield return new WaitForSeconds(delay);
            GameController.Instance?.setGetScorePopup(false);
            GameController.Instance?.UpdateNextQuestion();
        }
        else
        {
            GameController.Instance?.setWrongPopup(true);
            AudioController.Instance?.PlayAudio(2);
            this.updateRetryTimes(true);
            yield return new WaitForSeconds(delay);
            GameController.Instance?.setWrongPopup(false);
            if (this.Retry <= 0)
            {
                this.IsTriggerToNextQuestion = true;
            }
            onFailureCompleted?.Invoke();
        }
        this.scoring.correct = false;
    }

    public void characterReset(Vector3 newStartPostion)
    {
        this.randomDirection = UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f;
        this.startPosition = newStartPostion;
        this.characterCanvas.sortingOrder = this.characterOrder;
        this.characterTransform.localPosition = this.startPosition;
    }

    void FixedUpdate()
    {
        if(this.rectTransform != null)
        {
            switch (this.characterStatus)
            {
                case CharacterStatus.born:
                case CharacterStatus.idling:
                    this.StopCharacter();
                    return;
                case CharacterStatus.rotating:
                    if(this.transform.localScale == Vector3.zero)
                    {
                        this.characterStatus = CharacterStatus.recover;
                    }
                    else
                    {
                        this.moveButton.TriggerActive(true);
                        Vector3 direction = Vector3.forward * rotationSpeed * Time.deltaTime * this.randomDirection;
                        this.rectTransform.Rotate(direction);
                    }
                    break;
                case CharacterStatus.moving:
                    this.MoveForward();
                    break;
                case CharacterStatus.nextQA:
                    this.HoldCharacter();
                    break;
                case CharacterStatus.recover:
                    if(this.resetCount > 0f)
                    {
                        this.resetCount -= Time.deltaTime;
                    }
                    else
                    {
                        //var gridManager = GameController.Instance.gridManager;
                        //this.playerReset(gridManager.newCharacterPosition);
                        this.playerReset(this.startPosition);
                    }
                    break;
            }

            if (Input.GetKeyDown(KeyCode.Space) && this.UserId == 0)
            {
                this.characterStatus = CharacterStatus.moving;
                this.moveDirection = this.rectTransform.up;
            }

            bool isMoving = this.rb.velocity.sqrMagnitude > 0.05f;
            if (isMoving)
            {
                this.rb.velocity *= this.reducedFactor;
            }
            else
            {
                this.reducedFactor = this.reduceBaseFactor;
                this.rb.velocity = Vector3.zero;
            }
        }
    }

    void SetRandomRotationDirection()
    {
        if(this.rectTransform == null) this.rectTransform = this.GetComponent<RectTransform>();
        this.rb = GetComponent<Rigidbody2D>();
        this.rb.gravityScale = 0;
        this.randomDirection =  UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f;
    }

    public void StopRotation(BaseEventData data)
    {
        if(this.characterStatus == CharacterStatus.rotating && 
           this.transform.localScale == Vector3.one && 
           this.GetComponent<Collider2D>().enabled)
        {
            this.moveButton.PointerEffect(true);
            AudioController.Instance?.PlayAudio(0);
            this.playerCurrentPosition = this.transform.localPosition;
            this.moveDirection = this.rectTransform.up;
            this.characterStatus = CharacterStatus.moving;
        }
    }

    public void StopMove(BaseEventData data)
    {
        if(this.characterStatus != CharacterStatus.nextQA)
        {
            this.moveButton.PointerEffect(false);
            this.characterStatus = CharacterStatus.idling;
        }
    }

    void MoveForward()
    {
        this.rb.velocity = this.moveDirection * this.moveSpeed;
        this.rb.angularVelocity = 0f;
        //this.FaceDirection(this.moveDirection);
    }

    public void backToStartPosition()
    {
        this.HoldCharacter();
        this.characterStatus = CharacterStatus.born;
        SetUI.Set(this.bornParticle, true, 1f);
        this.transform.DOScale(0f, 0f);
        if (this.playerAppearEffect != null) this.playerAppearEffect.SetActive(true);
        this.GetComponent<CircleCollider2D>().enabled = true;

        this.transform.DOScale(1f, 1f).OnComplete(() =>
        {
            if (this.characterStatus != CharacterStatus.nextQA)
            {
                this.characterStatus = CharacterStatus.idling;
                this.playerAppearEffect.SetActive(false);
                SetUI.Set(this.bornParticle, false, 1f);
            }
        });
        this.characterReset(this.startPosition);
    }

    public void playerReset(Vector3 newStartPostion)
    {
        this.HoldCharacter();
        this.characterStatus = CharacterStatus.born;
        SetUI.Set(this.bornParticle, true, 1f);
        this.transform.DOScale(0f, 0f);
        if(this.playerAppearEffect != null) this.playerAppearEffect.SetActive(true);
        this.GetComponent<CircleCollider2D>().enabled = true;

        this.transform.DOScale(1f, 1f).OnComplete(() =>
        {
            if(this.characterStatus != CharacterStatus.nextQA)
            {
                this.characterStatus = CharacterStatus.idling;
                this.playerAppearEffect.SetActive(false);
                SetUI.Set(this.bornParticle, false, 1f);
            }
        });
        this.deductAnswer();
        this.setAnswer("");
        this.characterReset(newStartPostion);
        this.IsCheckedAnswer = false;
        this.IsCorrect = false;
        this.resetCount = 2.0f;
        this.collectedCell.Clear();
    }

    public void setAnswer(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            this.answer = "";
            SetUI.SetScale(this.answerBoxCg, false);
        }
        else
        {
            var gridManager = GameController.Instance.gridManager;
            if (gridManager.isMCType) { 
                this.answer = content;
            }
            else
            {
                this.answer += content;
            }
            float endValue = this.UserId < 2 ? 1f : -1f;
            SetUI.SetScale(this.answerBoxCg, true, endValue, 0.5f, Ease.OutElastic);
        }

        if(this.answerBox != null)
            this.answerBox.text = this.answer;
    }

    public void autoDeductAnswer()
    {
        if(this.collectedCell.Count > 0) {
            if (this.countAtStartPoints > 0f)
            {
                this.countAtStartPoints -= Time.deltaTime;
            }
            else
            {
                this.deductAnswer();
                this.countAtStartPoints = this.countGetAnswerAtStartPoints;
            }
        }
        else
        {
            this.countAtStartPoints = this.countGetAnswerAtStartPoints;
        }
    }

    public void deductAnswer()
    {
       var gridManager = GameController.Instance.gridManager;
        if (this.answer.Length > 0)
        {
            string deductedChar;
            if (gridManager.isMCType)
            {
                deductedChar = this.answer;
                this.setAnswer("");
            }
            else
            {
                deductedChar = this.answer[this.answer.Length - 1].ToString();
                this.answer = this.answer.Substring(0, this.answer.Length - 1);
                if (this.answerBox != null)
                    this.answerBox.text = this.answer;

                if (this.answer.Length == 0)
                {
                    SetUI.SetScale(this.answerBoxCg, false);
                }
            }

            if (this.collectedCell.Count > 0)
            {
                var latestCell= this.collectedCell[this.collectedCell.Count - 1];
                //latestCell.SetTextStatus(true);
                gridManager.updateNewWordPosition(latestCell);
                this.collectedCell.RemoveAt(this.collectedCell.Count - 1);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the other collider has a specific tag, e.g., "Player"
        if (other.CompareTag("Word"))
        {
            var cell = other.GetComponent<Cell>();
            if (cell != null)
            {
                cell.setCellEnterColor(true, GameController.Instance.showCells);
                if (cell.isSelected && this.Retry > 0)
                {
                    //LogController.Instance.debug("Player has entered the trigger!" + other.name);
                    AudioController.Instance?.PlayAudio(9);

                    var gridManager = GameController.Instance.gridManager;
                    if (gridManager.isMCType){
                        if (this.collectedCell.Count > 0)
                        {
                            var latestCell = this.collectedCell[this.collectedCell.Count - 1];
                            //latestCell.SetTextStatus(true);
                            gridManager.updateNewWordPosition(latestCell);
                            this.collectedCell.RemoveAt(this.collectedCell.Count - 1);
                        }
                    }
                    this.setAnswer(cell.content.text);
                    this.collectedCell.Add(cell);
                    cell.SetTextStatus(false);
                    this.characterStatus = CharacterStatus.idling;
                    var gameTimer = GameController.Instance.gameTimer;
                    int currentTime = Mathf.FloorToInt(((gameTimer.gameDuration - gameTimer.currentTime) / gameTimer.gameDuration) * 100);
                    this.checkAnswer(currentTime);
                }
            }
        }
        else if (other.CompareTag("Wall"))
        {
            this.ReBornCharacter();
        }
    }

    void StopCharacter()
    {
        this.rb.velocity = Vector2.zero;
        this.rb.angularVelocity = 0f;
        if(this.characterStatus != CharacterStatus.born) this.characterStatus = CharacterStatus.rotating;
    }

    void HoldCharacter()
    {
        this.rb.velocity = Vector2.zero;
        this.rb.angularVelocity = 0f;
    }

    void ReBornCharacter()
    {
        if (this.GetComponent<CircleCollider2D>().enabled)
        {
            SetUI.SetScale(this.answerBoxCg, false);
            AudioController.Instance?.PlayAudio(11, false, 0.5f);
            this.deductAnswer();
            this.characterStatus = CharacterStatus.recover;
            this.transform.DOScale(0f, 1f);
            this.moveButton.TriggerActive(false);
            this.GetComponent<CircleCollider2D>().enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Word"))
        {
            var cell = other.GetComponent<Cell>();
            if (cell != null)
            {
                cell.setCellEnterColor(false);
                if (cell.isSelected)
                {
                    LogController.Instance.debug("Player has exited the trigger!" + other.name);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (this.characterStatus == CharacterStatus.moving)
        {
            if (this.gameObject.name != collision.gameObject.name)
            {
                Rigidbody2D rb = collision.rigidbody;
                Vector2 relativeVelocity = collision.relativeVelocity;

                // Calculate the distance between the two objects
                float distance = Vector2.Distance(this.playerCurrentPosition, collision.transform.localPosition);

                var distanceFactor = distance / 10000f;
                this.reducedFactor = this.reduceBaseFactor + distanceFactor;
                // Apply the reduced factor
                collision.gameObject.GetComponent<PlayerController>().reducedFactor = this.reducedFactor;
                rb.angularVelocity = 0f;

                // Debug log the collision information
                LogController.Instance.debug($"Collision with: {collision.gameObject.name},distanceFactor: {distanceFactor}, Reduced Factor: {reducedFactor}, Distance: {distance}");
            }
            AudioController.Instance?.PlayAudio(10); //blob
            this.characterStatus = CharacterStatus.idling;
        }
    }



    /*private void OnCollisionExit2D(Collision2D collision)
    {
        collision.collider.enabled = true;
    }*/
}
