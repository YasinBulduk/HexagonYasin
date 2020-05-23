using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    //Sistem Prefabları
    [Header("Systems")]
    public HexagonFactory hexagonFactoryPrefab;
    public HexGrid gridPrefab;
    public UIController uiControllerPrefab;
    public Selector selectorPrefab;
    public RoundCounter roundCounterPrefab;
    public ScoreCounter scoreCounterPrefab;

    //Sistemler
    [HideInInspector] public HexagonFactory hexagonFactory;
    [HideInInspector] public HexGrid grid;
    [HideInInspector] public Selector selector;
    [HideInInspector] public CameraAligner cameraAligner;
    [HideInInspector] public RoundCounter roundCounter;
    [HideInInspector] public ScoreCounter scoreCounter;
    [HideInInspector] public UIController uiController;

    private bool m_BombExplode;
    private bool m_PlayerNoMovesLeft;
    private bool m_GameRestarted;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        InitializeSystems();
        SetDependencies();

        StartCoroutine(GameLoop());
    }

    private void SetDependencies()
    {
        ColorMatcher.OnColorMatch += ColorMatch;
        ColorMatcher.OnPlayerHasNoMove += () => m_PlayerNoMovesLeft = true;
        cameraAligner.Initialize(grid);
        selector.Register(roundCounter.IncreaseCount);
        scoreCounter.OnCurrentScoreChanged += uiController.scoreDisplayer.UpdateCurrentScore;
        scoreCounter.OnHighScoreChanged += uiController.scoreDisplayer.UpdateHighScore;
        roundCounter.OnRoundCountChangedInt += uiController.roundDisplayer.UpdateRoundCount;
        uiController.dropdownMenu.RegisterEventToButton(0, RestartGame);

        hexagonFactory.FactoryRunnable = true;
        hexagonFactory.OnFactoryStop += selector.ActivatePlayer;
        hexagonFactory.OnFactoryStart += selector.DeactivatePlayer;
    }

    private void InitializeSystems()
    {
        hexagonFactory = Instantiate(hexagonFactoryPrefab, Vector3.zero, Quaternion.identity);
        grid = Instantiate(gridPrefab, Vector3.zero, Quaternion.identity);
        grid.InitializeGrid();
        roundCounter = Instantiate(roundCounterPrefab, Vector3.zero, Quaternion.identity);
        scoreCounter = Instantiate(scoreCounterPrefab, Vector3.zero, Quaternion.identity);
        uiController = Instantiate(uiControllerPrefab);
        selector = Instantiate(selectorPrefab, Vector3.zero, Quaternion.identity);
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
#if UNITY_EDITOR
            Debug.Log("[GameManager] GameLoop Updating.");
#endif
            while (!m_BombExplode && !m_PlayerNoMovesLeft && !m_GameRestarted)
            {
#if UNITY_EDITOR
                Debug.Log("[GameManager] Waiting player to finish game.");
#endif
                yield return null;
            }

            selector.DeactivatePlayer();
            hexagonFactory.FactoryRunnable = false;

            if (m_BombExplode)
            {
#if UNITY_EDITOR
                Debug.Log("[GameManager] Bomb Explode.");
#endif
                yield return new WaitForSeconds(1f);

                DestroyAllHexagons();

                uiController.messageDisplayer.RegisterMessage("Bomb Exploded");

                uiController.DisplayReplayButton();

                while(!m_GameRestarted)
                {
                    yield return null;
                }
            }
            else if(m_PlayerNoMovesLeft)
            {
#if UNITY_EDITOR
                Debug.Log("[GameManager] Player Has No Move.");
#endif
                uiController.messageDisplayer.RegisterMessage("No More Moves Left");

                uiController.DisplayReplayButton();

                while (!m_GameRestarted)
                {
                    yield return null;
                }

                DestroyAllHexagons();
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("[GameManager] Game Restarted.");
#endif
                DestroyAllHexagons();
            }

            yield return new WaitForSeconds(0.3f);

            ResetGame();

            yield return StartCoroutine(FillGrid());

            selector.ResetPlayer();
            hexagonFactory.FactoryRunnable = true;
        }
    }

    private void ColorMatch(List<GridNode> matchedNodes)
    {
        //Eğer match yapılan roundda renk patlamış ise birşey yapma
        if (m_BombExplode) return;

#if UNITY_EDITOR
        Debug.Log("[GameManager] There's a MATCH!");
#endif
        matchedNodes.Sort((GridNode a, GridNode b) => b.Col - a.Col);

        HashSet<GridNode> matchedNodeSet = new HashSet<GridNode>(matchedNodes);

        List<Hexagon> destroyList = new List<Hexagon>();
        foreach (var node in matchedNodeSet)
        {
            destroyList.Add(node.CurrentHexagon);

            hexagonFactory.AddEmptyNode(node.MakeNodeEmpty(.3f));
        }

        foreach (var hexa in destroyList)
        {
            hexa.Destroy();
            scoreCounter.IncreaseScore(5);
        }
    }

    private IEnumerator FillGrid()
    {
        GridNode[][] gridNodes = grid.GetGridNodes();

        yield return StartCoroutine(HexagonFactory.Instance.FillGridRoutine(gridNodes));
    }

    private void DestroyAllHexagons()
    {
        GridNode[][] gridNodes = grid.GetGridNodes();

        for (int i = 0; i < gridNodes[0].Length; i++)
        {
            for (int k = 0; k < gridNodes.Length; k++)
            {
                GridNode currentNode = gridNodes[k][i];
                currentNode.CurrentHexagon.Destroy();
            }
        }
    }

    private void RestartGame()
    {
        if(hexagonFactory.IsSpawnFactoryRunning || hexagonFactory.IsGridFilling)
        {
            return;
        }

        m_GameRestarted = true;
    }

    private void ResetGame()
    {
        m_BombExplode = false;
        m_PlayerNoMovesLeft = false;
        m_GameRestarted = false;
        roundCounter.ResetCount();
        scoreCounter.ResetScore();
    }

    public void OnBombExplode()
    {
        m_BombExplode = true;
    }
}
