using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonFactory : GenericPool<Hexagon>
{
    public static HexagonFactory Instance { get; private set; } //Singleton Pattern

    public BombHexagon bombHexagonPrefab;
    public int bombInitialExplodeCount = 6;
    public ColorTable colorVariant;
    public float spawnDelay = 0.3f;

    public bool FactoryRunnable { get; set; }
    public Action OnFactoryStart { get; set; }
    public Action OnFactoryStop { get; set; }
    public bool IsSpawnFactoryRunning { get; private set;}
    public bool IsGridFilling { get; private set; }

    //Son olarak en üstte kalan boş node'lar eklediği için Stack veri yapısı
    private Stack<GridNode> m_EmptyNodeStack = new Stack<GridNode>();
    private float m_SpawnFactoryPositionY = 0f;
    private Transform m_SpawnedRoot;
    private GameManager m_GameManager;
    private int m_LastBombSpawnScore;

    protected override void Awake()
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

        //Color variant atanmamış ise Resources klasöründen al
        if (!colorVariant)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[{gameObject.name}] color variant is not assigned. Colors in the Resources folder will be used.");
#endif
            colorVariant = Resources.Load<ColorTable>("defaultColors");
        }
#if UNITY_EDITOR
        //Bomba prefabı eklenmemiş ise bildir
        if (!bombHexagonPrefab)
        {
            Debug.LogError($"[{gameObject.name}] BombHexagon prefab not assigned");
        }
#endif

        //GameManager referansını al
        m_GameManager = GameManager.Instance;

        //Spawn pozisyonunun Y eksenindeki yüksekliğini hesapla
        m_SpawnFactoryPositionY = Camera.main.orthographicSize * 2f;

        //Aktif olan hexagonlar için parent oluştur
        m_SpawnedRoot = new GameObject("Active " + typeof(Hexagon).Name + "s Root").transform;

        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (m_SpawnedRoot) Destroy(m_SpawnedRoot.gameObject);
    }

    private IEnumerator SpawnFactoryRoutine()
    {
        //Spawner çalışamaz durumda olduğu sürece bekle
        while (!FactoryRunnable)
        {
            yield return null;
        }

        OnFactoryStart?.Invoke();

        //Ilk hexagonların hareket geçmesi için bekle
        yield return new WaitForSeconds(.3f);
        while (m_EmptyNodeStack.Count > 0)
        {
            //Hareket halinde hexagon olduğu sürece bekle
            while (Hexagon.movingHexagonCount > 0)
            {
                yield return null;
            }

            //Kayıt edilmiş olan boş nodu al
            GridNode emptyNode = m_EmptyNodeStack.Pop();

            Vector2 emptyGridPosition = emptyNode.Position;

            emptyGridPosition.y = m_SpawnFactoryPositionY;

            Hexagon hexa;
            //Her 1000 skorda bomba oluştur. CurrentScore küsuratlı olarak atandığı için rastgelelik ekliyor.
            int currentScore = m_GameManager.scoreCounter.CurrentScore;
            if (currentScore > 1000 && m_LastBombSpawnScore - currentScore < -1000)
            {
                hexa = GetBombHexagonWithPosAndRot(emptyGridPosition, Quaternion.identity);
                m_LastBombSpawnScore = currentScore;
            }
            else //Bomba zamanı değil ise hexagon oluştur
            {
                hexa = GetHexagonWithPosAndRot(emptyGridPosition, Quaternion.identity);
            }

            //Eğer boş node'lar bittiyse sonlandırma işlemine geç
            if (m_EmptyNodeStack.Count == 0)
            {
                //Son oluşturulan hexagonun hareketini tamamlamasını bekle
                yield return StartCoroutine(hexa.MoveRoutine(emptyNode));

                //Bekleme sırasında yeni boş node eklendi ise devam et
                if (m_EmptyNodeStack.Count == 0)
                {
                    //m_IsSpawnFactoryRunning = false; //Yeni değişiklik

                    //Yerleşim tamamlandıktan sonra ufak bir süre bekledikten sonra renk eşleşimi varmı kontrol et
                    yield return new WaitForSeconds(.1f);
                    ColorMatcher.CheckRegisteredNodesColorMatches();

                    //Renk eşleşmesi kontrolü sonrasında eşleşme bulunup boş node'lar factorye eklenmiş ise devam et
                    if (m_EmptyNodeStack.Count == 0)
                    {
                        //Yeni eşleşme yok ise spawneri durdur
                        IsSpawnFactoryRunning = false;

                        //Oyuncunun hamlesi olup olmadığını kontrol et
                        ColorMatcher.CheckPlayerHasAnyMove();

                        //Spawnerin durduğunu bildir
                        OnFactoryStop?.Invoke();
                        yield break;
                    }
                }
            }
            else
            {
                StartCoroutine(hexa.MoveRoutine(emptyNode));
                yield return new WaitForSeconds(spawnDelay);
            }
            yield return null;
        }

    }

    //Spawneri çalıştır
    private void RunSpawnFactory()
    {
        IsSpawnFactoryRunning = true;
        StartCoroutine(SpawnFactoryRoutine());
    }

    //Satırı doldur
    private IEnumerator FillRowRoutine(GridNode[] gridRow)
    {
        HashSet<int> dualColors = new HashSet<int>();
        for (int i = 0; i < gridRow.Length; i++)
        {
            GridNode currentNode = gridRow[i];

            List<Anchor> connectedAnchors = new List<Anchor>();

            currentNode.GetConnectedAnchors(ref connectedAnchors);

            //Önceki node için renk eşleşmesinde kullanılan listeyi temizle
            dualColors.Clear();

            //3 renk aynı olmayacak şekilde yeni hexagonu al
            Hexagon spawned = SpawnHexagonByConnectionColor(gridRow[i].Position, connectedAnchors, ref dualColors);

            //Hexagonu spawn pozisyonuna taşı
            Vector2 emptyGridPosition = currentNode.Position;
            emptyGridPosition.y = m_SpawnFactoryPositionY;
            spawned.transform.position = emptyGridPosition;

            //Hexagonun node pozisyonuna ilerlemesini sağla
            StartCoroutine(spawned.MoveRoutine(currentNode));

            //Bir sonrakine geçmeden önce belirtilen süre kadar bekle
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    //Node'ları soldan sağa doldur
    public IEnumerator FillGridRoutine(GridNode[][] gridNodes)
    {
        IsGridFilling = true;
        for (int i = 0; i < gridNodes.Length; i++)
        {
            //Son satır ise bitene kadar bekle
            if(i == gridNodes.Length - 1)
            {
                yield return StartCoroutine(FillRowRoutine(gridNodes[i]));
            }
            else
            {
                StartCoroutine(FillRowRoutine(gridNodes[i]));
                yield return new WaitForSeconds(.6f);
            }
        }
        IsGridFilling = false;
    }

    public void AddEmptyNode(GridNode emptyNode)
    {
        m_EmptyNodeStack.Push(emptyNode);

        //Eğer spawner çalışmıyor ise çalıştır
        if (!IsSpawnFactoryRunning)
        {
            RunSpawnFactory();
        }
    }

    //Komşu 3 renk aynı olmayacak şekilde hexagon oluştur
    public Hexagon SpawnHexagonByConnectionColor(Vector3 nodePosition, List<Anchor> connectedAnchors, ref HashSet<int> cleanSetForCalculation)
    {
        //Renk eşleşmelerini al
        ColorMatcher.GetDualColorsFromAnchors(connectedAnchors, ref cleanSetForCalculation);

        //Hexagon oluştur
        Hexagon spawnedHexa = null;
        do
        {
            //Renk eşleşmesi durumunda önceki alınan hexagonu iade et
            if (spawnedHexa != null) AddHexagonToPool(spawnedHexa);

            //Yeni bir hexa al
            spawnedHexa = GetHexagonWithPosAndRot(nodePosition, Quaternion.identity);

            //Renk eşleşmesi olduğu sürece işlemi tekrarla
        } while (cleanSetForCalculation.Contains(spawnedHexa.colorId));

        return spawnedHexa;
    }

    /// <summary>
    /// Do not call on Awake. Pool is initializing on Awake
    /// </summary>
    /// <returns>Hexagon instance</returns>
    public Hexagon GetHexagon()
    {
        return GetHexagonWithPosAndRot(Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// Do not call on Awake. Pool is initializing on Awake
    /// </summary>
    /// <returns>Hexagon instance</returns>
    public Hexagon GetHexagonWithPosAndRot(Vector3 position, Quaternion rotation)
    {
        Hexagon spawned = GetObjectWithPosAndRot(position, rotation);
        spawned.transform.parent = m_SpawnedRoot;
        Material randomMaterial = colorVariant.GetRandomMaterial();
        spawned.SetMaterial(randomMaterial);

        return spawned;
    }

    //Pozisyon ve rotasyon bilgisi ile bomba oluştur
    public Hexagon GetBombHexagonWithPosAndRot(Vector3 position, Quaternion rotation)
    {
        BombHexagon spawned = Instantiate(bombHexagonPrefab, position, rotation);
        spawned.ExplodeCount = bombInitialExplodeCount;
        m_GameManager.roundCounter.OnRoundCountChanged += spawned.DecreaseExplodeCount;
        spawned.onBombExplode += m_GameManager.OnBombExplode;

        spawned.transform.parent = m_SpawnedRoot;
        Material randomMaterial = colorVariant.GetRandomMaterial();
        spawned.SetMaterial(randomMaterial);

        return spawned;
    }

    //Oluşturulan bombeyı yok et
    public void DestroyBombHexagon(BombHexagon bombHexagon)
    {
        m_GameManager.roundCounter.OnRoundCountChanged -= bombHexagon.DecreaseExplodeCount;

        Destroy(bombHexagon.gameObject);
    }

    //Hexagonu havuza geri aktar
    public void AddHexagonToPool(Hexagon hexagonToAdd)
    {
        hexagonToAdd.currentNode = null;
        hexagonToAdd.SetRendererOrder(0);
        hexagonToAdd.IsMatched = false;
        AddObjectToPool(hexagonToAdd);
    }

    public void ResetFactory()
    {
        m_LastBombSpawnScore = 0;
    }
}