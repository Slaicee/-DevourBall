using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public SaveData CurrentData { get; private set; }

    private int sessionObjectsEaten;
    private float sessionStartTime;
    private bool pendingAutoLoad;

    private string SaveFilePath =>
        Path.Combine(Application.persistentDataPath, "devour_save.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFromDisk();
    }

    private void Start()
    {
        sessionStartTime = Time.time;

        if (CurrentData.HasGameProgress())
            pendingAutoLoad = true;
    }

    private void OnEnable()
    {
        EventBus.OnEat += HandleEat;
        EventBus.OnGameStateChanged += HandleGameStateChanged;
        EventBus.OnScoreChanged += HandleScoreChanged;
    }

    private void OnDisable()
    {
        EventBus.OnEat -= HandleEat;
        EventBus.OnGameStateChanged -= HandleGameStateChanged;
        EventBus.OnScoreChanged -= HandleScoreChanged;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            EventBus.OnEat -= HandleEat;
            EventBus.OnGameStateChanged -= HandleGameStateChanged;
            EventBus.OnScoreChanged -= HandleScoreChanged;
            Instance = null;
        }
    }

    private int autoLoadFrameDelay = 1;

    private void Update()
    {
        if (pendingAutoLoad)
        {
            if (--autoLoadFrameDelay <= 0)
            {
                pendingAutoLoad = false;
                LoadGame();
            }
            return;
        }

        if (GameStateManager.Instance != null && GameStateManager.Instance.IsPlaying())
        {
            CurrentData.totalPlayTime += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame();
        }
    }

    // ---- stats ----

    public void SaveToDisk()
    {
        string json = JsonUtility.ToJson(CurrentData, true);
        File.WriteAllText(SaveFilePath, json);
    }

    private void LoadFromDisk()
    {
        if (File.Exists(SaveFilePath))
        {
            string json = File.ReadAllText(SaveFilePath);
            CurrentData = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            CurrentData = new SaveData();
        }
    }

    public bool HasSave()
    {
        if (!File.Exists(SaveFilePath)) return false;
        string json = File.ReadAllText(SaveFilePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data != null && data.HasGameProgress();
    }

    public void DeleteSave()
    {
        if (File.Exists(SaveFilePath))
            File.Delete(SaveFilePath);
    }

    private void HandleEat(GameObject obj, Eatable.EatableType type, Vector3 pos, Bounds bounds)
    {
        sessionObjectsEaten++;
        CurrentData.totalObjectsEaten++;
    }

    private void HandleGameStateChanged(GameState prev, GameState curr)
    {
        if (curr == GameState.GameOver)
        {
            DeleteSave();
        }
    }

    private void HandleScoreChanged(int score)
    {
        if (score > CurrentData.highScore)
        {
            CurrentData.highScore = score;
        }
    }

    // ---- game save / load ----

    public void SaveGame()
    {
        if (GameStateManager.Instance == null || !GameStateManager.Instance.IsPlaying())
        {
            Debug.LogWarning("Can only save while playing.");
            return;
        }

        var growth = FindObjectOfType<GrowthSystem>();
        var countdown = FindObjectOfType<CountdownUI>();
        var citySink = FindObjectOfType<CitySink>();

        if (growth != null)
        {
            Vector3 pos = growth.transform.position;
            CurrentData.playerPosX = pos.x;
            CurrentData.playerPosY = pos.y;
            CurrentData.playerPosZ = pos.z;
            CurrentData.playerSize = growth.Size;
            CurrentData.playerRadius = growth.Radius;
        }

        if (countdown != null)
            CurrentData.remainingTime = countdown.GetRemainingTime();

        if (citySink != null)
            CurrentData.waterLevelY = citySink.WaterLevelY;

        CurrentData.remainingEatableIds = EatableManager.All
            .Select(e => GetObjectId(e.gameObject))
            .ToList();

        SaveToDisk();
        Debug.Log($"Game saved. {CurrentData.remainingEatableIds.Count} objects, " +
            $"size: {CurrentData.playerSize:F2}, time: {CurrentData.remainingTime:F0}s");
    }

    public void LoadGame()
    {
        if (!HasSave())
        {
            Debug.Log("No save file found.");
            return;
        }

        LoadFromDisk();

        // Kill any running cutscene and enable player scripts
        var cutscene = FindObjectOfType<OpeningCutscene>();
        if (cutscene != null)
        {
            cutscene.ForceSkip();
        }

        // Destroy eatables that were already eaten in the save
        var allEatables = FindObjectsOfType<Eatable>();
        foreach (var e in allEatables)
        {
            string id = GetObjectId(e.gameObject);
            if (!CurrentData.remainingEatableIds.Contains(id))
            {
                EatableManager.All.Remove(e);
                Destroy(e.gameObject);
            }
        }

        // Restore player
        var growth = FindObjectOfType<GrowthSystem>();
        if (growth != null)
        {
            Vector3 pos = new Vector3(CurrentData.playerPosX, CurrentData.playerPosY, CurrentData.playerPosZ);
            growth.LoadState(CurrentData.playerSize, CurrentData.playerRadius, pos);
        }

        // Restore countdown
        var countdown = FindObjectOfType<CountdownUI>();
        if (countdown != null)
            countdown.SetRemainingTime(CurrentData.remainingTime);

        // Restore water level
        var citySink = FindObjectOfType<CitySink>();
        if (citySink != null)
            citySink.WaterLevelY = CurrentData.waterLevelY;

        // Enter playing state (skip if already playing)
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsPlaying())
            GameStateManager.Instance.SetState(GameState.Playing);

        Debug.Log($"Game loaded. {CurrentData.remainingEatableIds.Count} objects, " +
            $"size: {CurrentData.playerSize:F2}, time: {CurrentData.remainingTime:F0}s");
    }

    private static string GetObjectId(GameObject obj)
    {
        if (obj == null) return "";
        Transform t = obj.transform;
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
