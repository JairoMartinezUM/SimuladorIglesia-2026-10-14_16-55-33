using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState { Briefing, Playing, Result }

[DefaultExecutionOrder(-100)]   // corre antes que los demás Start, para aplicar las fallas primero
public class LevelManager : MonoBehaviour
{
    public ProblemData[] problems;

    [Header("UI")]
    public GameObject briefingPanel;
    public TMP_Text briefingText;
    public TMP_Text timerText;
    public GameObject resultPanel;
    public TMP_Text resultText;

    [Header("Puntuación")]
    public int mistakePenalty = 100;       // error: romper algo que estaba bien
    public int replacePenalty = 150;       // mayordomía: reemplazar en vez de reparar
    public int teamBonus = 100;            // trabajo en equipo: pedir ayuda
    public float secondsPerNotice = 10f;   // cada tanto, un hermano nota el problema

    public static LevelManager Instance { get; private set; }
    public GameState State { get; private set; }

    // Persisten al recargar la escena
    static int currentLevel;
    static int totalScore;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { currentLevel = 0; totalScore = 0; }

    public static void ResetProgress() { currentLevel = 0; totalScore = 0; }

    ProblemData problem;
    SignalNode[] objectives;
    float timeLeft;
    float elapsed;
    bool won;
    int mistakes;
    int replacements;
    int helpCount;

    bool HasNext => currentLevel < problems.Length - 1;

    void Awake() { Instance = this; }

    public void RegisterMistake()
    {
        if (State != GameState.Playing) return;
        mistakes++;
        AudioManager.Error();
    }

    public void RegisterReplacement()
    {
        if (State == GameState.Playing) replacements++;
    }

    // Trabajo en equipo: el diácono da una pista; cada pedido da una más específica
    public string AskForHelp()
    {
        if (State != GameState.Playing) return null;

        helpCount++;
        if (problem.hints == null || problem.hints.Length == 0)
            return "No sé qué decirte, hermano, pero cuenta conmigo.";

        return problem.hints[Mathf.Min(helpCount - 1, problem.hints.Length - 1)];
    }

    void Start()
    {
        if (problems == null || problems.Length == 0)
        {
            Debug.LogError("LevelManager: la lista 'problems' está vacía.");
            enabled = false;
            return;
        }

        currentLevel = Mathf.Clamp(currentLevel, 0, problems.Length - 1);
        problem = problems[currentLevel];

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
                elapsed += Time.deltaTime;
                UpdateTimerUI();
                if (AllObjectivesOk()) Finish(true);
                else if (timeLeft <= 0f) Finish(false);
                break;

            case GameState.Result:
                if (kb == null) break;
                if (won && kb.enterKey.wasPressedThisFrame) GoToNext();
                else if (!won && kb.rKey.wasPressedThisFrame) Reload();
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
            $"<size=70%>Nivel {currentLevel + 1} de {problems.Length}</size>\n" +
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

    void Finish(bool success)
    {
        won = success;
        State = GameState.Result;
        Time.timeScale = 0f;
        resultPanel.SetActive(true);
        if (success) AudioManager.Success(); else AudioManager.Fail();

        if (success)
        {
            int timeScore = Mathf.RoundToInt(1000f * Mathf.Clamp01(timeLeft / problem.timeLimitSeconds));
            int mistakeCost = mistakes * mistakePenalty;
            int replaceCost = replacements * replacePenalty;
            int team = helpCount > 0 ? teamBonus : 0;
            int levelScore = Mathf.Max(0, timeScore - mistakeCost - replaceCost + team);
            totalScore += levelScore;

            int noticed = Mathf.FloorToInt(elapsed / secondsPerNotice);
            string teamLine = helpCount > 0 ? "pediste ayuda" : "sin pedir ayuda";
            string reflection = "";
            if (!string.IsNullOrEmpty(problem.reflection))
                reflection += $"<i>{problem.reflection}</i>\n";
            if (!string.IsNullOrEmpty(problem.verseText))
                reflection += $"<size=80%><color=#D0D0D0>“{problem.verseText}”\n— {problem.verseRef}</color></size>\n";
            if (reflection != "") reflection += "\n";
            string next = HasNext ? "ENTER: siguiente nivel" : "¡Completaste todos los niveles!\n\nENTER: volver a empezar";

            resultText.text =
                "<b>¡Servicio salvado!</b>\n\n" +
                $"Tiempo restante: {FormatTime(timeLeft)}  (+{timeScore})\n" +
                $"Errores: {mistakes}  (-{mistakeCost})\n" +
                $"Mayordomía, reemplazos: {replacements}  (-{replaceCost})\n" +
                $"Trabajo en equipo: {teamLine}  (+{team})\n" +
                $"Hermanos que lo notaron: {noticed}\n\n" +
                $"Puntos del nivel: {levelScore}\nPuntos totales: {totalScore}\n\n" +
                reflection + next;
        }
        else
        {
            resultText.text = "<b>Se acabó el tiempo</b>\n\nEl servicio comenzó con el problema sin resolver.\n\nR: reintentar";
        }
    }

    void GoToNext()
    {
        if (HasNext) currentLevel++;
        else { currentLevel = 0; totalScore = 0; }
        Reload();
    }

    void Reload()
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