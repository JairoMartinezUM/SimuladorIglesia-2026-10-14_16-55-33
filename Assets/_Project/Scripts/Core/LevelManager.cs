using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState { Briefing, Playing, Result }

[DefaultExecutionOrder(-100)]   // corre antes que los demás Start, para aplicar las fallas primero
public class LevelManager : MonoBehaviour
{
    public ProblemData problem;

    [Header("UI")]
    public GameObject briefingPanel;
    public TMP_Text briefingText;
    public TMP_Text timerText;
    public GameObject resultPanel;
    public TMP_Text resultText;

    public GameState State { get; private set; }

    SignalNode[] objectives;
    float timeLeft;

    void Start()
    {
        ApplyFaults();

        objectives = FindObjectsByType<SignalNode>(FindObjectsSortMode.None)
            .Where(n => problem.objectiveNodes.Contains(n.displayName))
            .ToArray();

        timeLeft = problem.timeLimitSeconds;
        resultPanel.SetActive(false);
        ShowBriefing();
    }

    void ApplyFaults()
    {
        var cables = FindObjectsByType<Cable>(FindObjectsSortMode.None);
        var nodes = FindObjectsByType<SignalNode>(FindObjectsSortMode.None);

        foreach (var f in problem.faults)
        {
            switch (f.kind)
            {
                case FaultKind.CableDisconnected:
                    foreach (var c in cables.Where(c => c.id == f.targetId)) c.SetConnected(false);
                    break;
                case FaultKind.CableDamaged:
                    foreach (var c in cables.Where(c => c.id == f.targetId)) c.isDamaged = true;
                    break;
                case FaultKind.NodeFault:
                    foreach (var n in nodes.Where(n => n.displayName == f.targetId)) n.SetFault(f.nodeFault);
                    break;
            }
        }
    }

    void Update()
    {
        var kb = Keyboard.current;
        switch (State)
        {
            case GameState.Briefing:
                if (kb != null && kb.enterKey.wasPressedThisFrame) BeginPlay();
                break;

            case GameState.Playing:
                timeLeft -= Time.deltaTime;
                UpdateTimerUI();
                if (AllObjectivesOk()) Finish(true);
                else if (timeLeft <= 0f) Finish(false);
                break;

            case GameState.Result:
                if (kb != null && kb.rKey.wasPressedThisFrame) Retry();
                break;
        }
    }

    bool AllObjectivesOk() => objectives.Length > 0 && objectives.All(o => o.HasSignal());

    void ShowBriefing()
    {
        State = GameState.Briefing;
        Time.timeScale = 0f;
        briefingPanel.SetActive(true);
        briefingText.text =
            $"<b>{problem.title}</b>\n\n{problem.briefing}\n\n<i>{problem.report}</i>\n\n" +
            $"Tiempo: {FormatTime(problem.timeLimitSeconds)}\n\nPresiona ENTER para comenzar";
        UpdateTimerUI();
    }

    void BeginPlay()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        briefingPanel.SetActive(false);
    }

    void Finish(bool won)
    {
        State = GameState.Result;
        Time.timeScale = 0f;
        resultPanel.SetActive(true);

        if (won)
        {
            int score = Mathf.RoundToInt(1000f * Mathf.Clamp01(timeLeft / problem.timeLimitSeconds));
            resultText.text = $"<b>¡Servicio salvado!</b>\n\nTiempo restante: {FormatTime(timeLeft)}\nPuntos: {score}\n\nR: reintentar";
        }
        else
        {
            resultText.text = "<b>Se acabó el tiempo</b>\n\nEl servicio comenzó con el problema sin resolver.\n\nR: reintentar";
        }
    }

    void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateTimerUI() => timerText.text = FormatTime(timeLeft);

    static string FormatTime(float t)
    {
        t = Mathf.Max(0f, t);
        return $"{(int)(t / 60):00}:{(int)(t % 60):00}";
    }
}