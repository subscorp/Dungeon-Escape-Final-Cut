using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if(_instance == null)
            {
                Debug.LogError("GamaManager is NULL");
            }
            return _instance;
        }
    }

    public bool HasKeyToCastle { get; set; } 
    public bool HasBootsOfFlight { get; set; }
    public bool PlayerHitSpike { get; set; }
    public int numDiamondsCollected { get; set; }
    public int numEnemiesKilled { get; set; }
    public int numJumps { get; set; }
    public bool boughtAppleInCurrentVisit { get; set; }
    public bool boughtBootsInCurrentVisit{ get; set; }
    public bool boughtKeyInCurrentVisit { get; set; }
    public bool PlayerInShop { get; set; }
    public Player player { get; private set; }
    private float _elapsedTime;
    public float ElapsedTime 
    {
        get { return _elapsedTime; }
    }
    public int ElapsedMinutes { get; set; }
    public float ElapsedSeconds { get; set; }
    public string DisplayTime { get; set; }
    public bool IsDead { get; set; }
    public string KonamiCodeString { get; set; }
    public bool PlayerInFrontOfDoorA { get; set; }
    public bool PlayerInFrontOfDoorB { get; set; }
    public bool HintProvided { get; set; }
    public string CurrentBeatTime { get; set; }
    public bool IsLoggedIn { get; set; }

    // Achivements related properties
    public bool GameComplete { get; set; } // Achievement 1 
    public bool AllEnemiesKilled { get; set; } // Achievement 2
    public bool CollectedAllDiamonds { get; set; } // Achievement 3
    public bool NoHitRun { get; set; } // Achievement 4
    public bool DidNotWatchAd { get; set; } // Achievement 5
    public string UserIdentifier { get; set; }
    public bool HandledAdNotWorking { get; set; }
    [SerializeField]
    private GameObject _winManager;
    [SerializeField]
    private GameObject _gameOverManager;

    public bool GotKonamiCode { get; set; }
    public bool DuringKonamiCode { get; set; }
    private bool shouldWait = false;

    private void Awake()
    {
        //PlayerPrefs.DeleteAll(); // for debug only, don't forget to comment out

        _instance = this;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        AudioManager.Instance.PlayMusic();
        numJumps = 0;
        NoHitRun = true;
        DidNotWatchAd = true;
        boughtAppleInCurrentVisit = false;
        boughtBootsInCurrentVisit = false;
        boughtKeyInCurrentVisit = false;
        PlayerInShop = false;
        HandledAdNotWorking = false;
        IsDead = false;
        PlayerInFrontOfDoorA = false;
        PlayerInFrontOfDoorB = false;
        HintProvided = false;
        HasBootsOfFlight = false;
        DuringKonamiCode = false;
        GameComplete = false;
        IsLoggedIn = false;
        CurrentBeatTime = "";
        DisplayTime = "";
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(OnSignInResult);
    }

    private void Start()
    {
        ResetTime();
        DisplayWinScreen(false);
        DisplayGameOverScreen(false);
    }

    public void Update()
    {
        _elapsedTime += Time.deltaTime;
        ElapsedMinutes = (int)(_elapsedTime / 60);
        ElapsedSeconds = _elapsedTime % 60;
        DisplayTime = string.Format("{0:00}:{1:00.00}", ElapsedMinutes, ElapsedSeconds);
    }

    private void OnSignInResult(SignInStatus signInStatus)
    {
        if (signInStatus == SignInStatus.Success)
        {
            Debug.Log("Authenticated. Hello, " + Social.localUser.userName + " (" + Social.localUser.id + ")");
            UserIdentifier = Social.localUser.userName;
            Debug.Log("In GameManager::Start, UserIdentifier: " + UserIdentifier);
            if (UserIdentifier == null || UserIdentifier == "")
                UserIdentifier = "temp";
            else
                IsLoggedIn = true;
        }
        else
        {
            Debug.Log("*** Failed to authenticate with " + signInStatus);
            UserIdentifier = "temp";
            IsLoggedIn = false;
        }
    }

    public void HomeButton()
    {
        AudioManager.Instance.PlayHomeButtonSFX();
        SceneManager.LoadScene("Main_Menu");
    }

    public bool RefillHealth()
    {
        if(player.Health < 4)
        {
            player.Health++;
            UIManager.Instance.UpdateLives(player.Health, false);
            return true;
        }
        return false;
    }

    public void DoAchievementUnlock(string achievementId, System.Action<bool> onUnlock)
    {
        if (PlayGamesPlatform.Instance == null)
            return;

        PlayGamesPlatform.Instance.LoadAchievements((achievements) =>
        {
            bool alreadyUnlocked = false;
            if (achievements == null)
            {
                Debug.Log("achievements is NULL");
                return;
            }

            // Check if the achievement is already unlocked
            foreach (var achievement in achievements)
            {
                if (achievement.id == achievementId && achievement.completed)
                {
                    alreadyUnlocked = true;
                    break;
                }
            }

            // If the achievement is not already unlocked, unlock it
            if (!alreadyUnlocked)
            {
                Social.ReportProgress(achievementId, 100.0f, (bool success) =>
                {
                    onUnlock(success);
                });
            }
            else
            {
                onUnlock(false);
            }
        });
    }

    public void DoAchievementIncrement(string achievementId, int steps = 1)
    {
        if (PlayGamesPlatform.Instance == null)
            return;

        PlayGamesPlatform.Instance.IncrementAchievement(achievementId, steps, (bool success) =>
        {
            if (success)
            {
                Debug.Log("Achievement incremented: " + achievementId);
            }
            else
            {
                Debug.LogWarning("Failed to increment achievement: " + achievementId);
            }
        });
    }

    public void SubmitScore(string leaderboardId, long score, Action<bool> callback)
    {
        Debug.Log("Score in SubmitScore: " + score);
        PlayGamesPlatform.Instance.ReportScore(score, leaderboardId, null);
    }

    public void ResetTime()
    {
        _elapsedTime = 0f;
        ElapsedMinutes = 0;
        ElapsedSeconds = 0;
    }

    public void DisplayWinScreen(bool val)
    {
        _winManager.SetActive(val);
    }

    internal void DisplayGameOverScreen(bool val)
    {
        _gameOverManager.SetActive(val);
    }

    public void HandleKonamiCode()
    {
        if (GotKonamiCode)
            return;

        if (KonamiCodeString == "UUDDLRLRBA")
        {
            DuringKonamiCode = false;
            KonamiCodeString = "";
            GotKonamiCode = true;
            AudioManager.Instance.PlayKonamiCodeCorrectSFX();
            player.AddGems(100, true);
            UIManager.Instance.UpdateLivesOnKonamiCode();
            player.KonamiCodeSpeed();
        }
        else
            ResetKonamiCodeChecks();
    }

    public void HandleKonamiCodeDirections(float moveHorizontal, float moveVertical)
    {
        if (shouldWait || GotKonamiCode)
            return;

        float horizontalThreshold = 0.5f;
        float verticalThreshold = 0.5f;
        bool appendedToString = false;

        if (Mathf.Abs(moveHorizontal) >= horizontalThreshold && Mathf.Abs(moveVertical) <= verticalThreshold)
        {
            appendedToString = true;

            if (moveHorizontal > 0)
                KonamiCodeString += "R";
            else if (moveHorizontal < 0)
                KonamiCodeString += "L";
        }
        else if (Mathf.Abs(moveVertical) >= horizontalThreshold && Mathf.Abs(moveHorizontal) <= verticalThreshold)
        {
            appendedToString = true;

            if (moveVertical > 0)
                KonamiCodeString += "U";
            else if (moveVertical < 0)
                KonamiCodeString += "D";
        }

        if (appendedToString)
        {
            StartCoroutine(DirectionsCooldown());
            HandleKonamiCode();
        }
    }
    private void ResetKonamiCodeChecks()
    {
        if (GotKonamiCode)
            return;

        if (KonamiCodeString == "UUDD" && KonamiCodeString.Length == 4)
            DuringKonamiCode = true;

        if ((KonamiCodeString != "U" && KonamiCodeString.Length == 1) ||
        (KonamiCodeString != "UU" && KonamiCodeString.Length == 2) ||
        (KonamiCodeString != "UUD" && KonamiCodeString.Length == 3) ||
        (KonamiCodeString != "UUDD" && KonamiCodeString.Length == 4) ||
        (KonamiCodeString != "UUDDL" && KonamiCodeString.Length == 5) ||
        (KonamiCodeString != "UUDDLR" && KonamiCodeString.Length == 6) ||
        (KonamiCodeString != "UUDDLRL" && KonamiCodeString.Length == 7) ||
        (KonamiCodeString != "UUDDLRLR" && KonamiCodeString.Length == 8) ||
        (KonamiCodeString != "UUDDLRLRB" && KonamiCodeString.Length == 9))
        {

            if(KonamiCodeString.Length >= 3)
            {
                DuringKonamiCode = false;
                AudioManager.Instance.PlayKonamiCodeWrongSFX();
            }

            KonamiCodeString = "";
        }
    }

    IEnumerator DirectionsCooldown()
    {
        shouldWait = true;
        yield return new WaitForSeconds(0.2f);
        shouldWait = false;
    }
}
