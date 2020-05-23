using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class HexMetrics
{
    //Köşe uzaklığı için
    public const float outerRadius = 0.5f;

    //Kenar uzaklığı için
    public const float innerRadius = outerRadius * 0.866025404f;
}

public class HexGrid : MonoBehaviour
{
    [Range(2, 100)] public int gridWidth = 8;
    [Range(2, 100)] public int gridHeight = 9;

    private GridNode[][] m_GridNodes;
    private List<Anchor> m_Anchors = new List<Anchor>();

    #region DEBUG
#if UNITY_EDITOR
    public bool DEBUG_NODES = false;
    public bool DEBUG_ANCHORS = false;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (DEBUG_ANCHORS)
            {
                m_Anchors.ForEach(a => a.DebugAnchor());
            }

            if (DEBUG_NODES)
            {
                for (int i = 0; i < m_GridNodes.Length; i++)
                {
                    for (int k = 0; k < m_GridNodes[i].Length; k++)
                    {
                        m_GridNodes[i][k].DebugNode();
                    }
                }
            }
        }
    }
#endif
    #endregion

    private void InitializeArrays()
    {
        m_GridNodes = new GridNode[gridWidth][];

        for (int i = 0; i < m_GridNodes.Length; i++)
        {
            m_GridNodes[i] = new GridNode[gridHeight];
        }
    }

    private GridNode CreateNode(int row, int col, Vector2 position)
    {
        GridNode result = new GridNode(row, col, position, this);

        return result;
    }

    private void CreateAnchors(GridNode gridNode, ref List<Anchor> createdAnchors)
    {
        int x = gridNode.Row;
        int y = gridNode.Col;

        //Grid soldan sağa oluştuğu için ilk satırlar için birşey yapma
        if (x == 0) return;

        GridNode connected1 = gridNode;
        GridNode connected2;
        GridNode connected3;

        Anchor anchor;

        //Çift satırlar
        if (x % 2 == 0)
        {
            //Anchor için minimum 3 node gerekli ilk sütünlar için birşey yapma
            if (y == 0) return;

            //Geriye kalan tüm çift satılar için 2 anchor (sol alt ve sol) bulunuyor.

            //Anchor 1 (sol alt)
            connected2 = m_GridNodes[x][y - 1]; //Alt
            connected3 = m_GridNodes[x - 1][y - 1]; //Sol Alt

            //Anchor oluştur ve bağlı olduğu node'ları aktar
            //Anchordan rotasyon için bağlı olan node'ları saat yönünde ekle.
            anchor = new Anchor(connected1, connected2, connected3);

            //Grid anchor listesine oluşturulan anchor'u ekle
            m_Anchors.Add(anchor);

            //Renk kontrolü için şuan oluşturulan anchor'ları ekle
            createdAnchors.Add(anchor);

            //Anchor 2 (sol)
            connected2 = m_GridNodes[x - 1][y - 1]; //Sol Alt
            connected3 = m_GridNodes[x - 1][y]; //Sol Yan

            //Anchordan rotasyon için bağlı olan node'ları saat yönünde ekle.
            anchor = new Anchor(connected1, connected2, connected3);

            m_Anchors.Add(anchor);
            createdAnchors.Add(anchor);
        }
        //Tek satılar
        else
        {
            //Tek satıların en alt ve en üst kolonlarında 1 adet Anchor bulunurken, (alt için sol / üst için sol alt)
            //orta kısımlarda 2 Adet Anchor bulunuyor.

            //En üst değil ise ekle
            //Sol Anchor
            if (y != gridHeight - 1)
            {
                connected2 = m_GridNodes[x - 1][y]; // Sol Yan
                connected3 = m_GridNodes[x - 1][y + 1]; // Sol Üst

                //Anchordan rotasyon için bağlı olan node'ları saat yönünde ekle.
                anchor = new Anchor(connected1, connected2, connected3);

                m_Anchors.Add(anchor);
                createdAnchors.Add(anchor);
            }

            //En alt değil ise ekle
            //Sol Alt Anchor
            if (y != 0)
            {
                connected2 = m_GridNodes[x][y - 1]; // Alt
                connected3 = m_GridNodes[x - 1][y]; // Sol Yan

                //Anchordan rotasyon için bağlı olan node'ları saat yönünde ekle.
                anchor = new Anchor(connected1, connected2, connected3);

                m_Anchors.Add(anchor);
                createdAnchors.Add(anchor);
            }
        }
    }

    private void CreateGrid()
    {
        //Gereksiz hafıza kullanımını engellemek için
        List<Anchor> currentAnchors = new List<Anchor>();
        HashSet<int> dualColors = new HashSet<int>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                //Pozisyonları hesapla
                float xOffset = x * (HexMetrics.outerRadius * 1.5f);
                float yOffset = (y + x * 0.5f - x / 2) * (HexMetrics.innerRadius * 2f);

                Vector2 nodePosition = new Vector2(xOffset, yOffset);

                //Node oluştur
                GridNode currentNode = m_GridNodes[x][y] = CreateNode(x, y, nodePosition);

                //Griddeki hexagon her değiştiğinde değişikliği kaydet
                currentNode.OnHexagonChange += ColorMatcher.RegisterChangedNode;

                //Eski oluşturulan anchorları temizle
                currentAnchors.Clear();

                //Node a göre anchor oluştur
                CreateAnchors(m_GridNodes[x][y], ref currentAnchors);

                //Önceki node için renk eşleşmesinde kullanılan listeyi temizle
                dualColors.Clear();

                //3 renk aynı olmayacak şekilde yeni hexagonu al
                Hexagon spawnedHexa = HexagonFactory.Instance.SpawnHexagonByConnectionColor(nodePosition, currentAnchors, ref dualColors);

                //Grid-Hexagon bağlantılarını sağla
                spawnedHexa.currentNode = currentNode;
                currentNode.CurrentHexagon = spawnedHexa;

                //Başlangıçta oyuncunun hamle hakkı varmı kontrolü için nodu hexagonu değişenler listesinde kaydet
                currentNode.HexagonChanged();
            }
        }

        //Her Node için komşu Node'u bul
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridNode currentNode = m_GridNodes[x][y];
                currentNode.FindNeighbors(m_GridNodes);
            }
        }

        //Oyuncunun oynama hamlesi olmadığı sürece gridi yeniden doldur
        while (!ColorMatcher.CheckPlayerHasAnyMove())
        {
            FillNodes();
        }
    }

    //Oyuncunun hamlesi olmadığında hexagonları yeniden oluşturmak için
    private void FillNodes()
    {
        List<Anchor> connectedAnchors = new List<Anchor>();
        HashSet<int> dualColors = new HashSet<int>();
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GridNode currentNode = m_GridNodes[x][y];

                //Ilk oluşturmadaki hexayı iade et
                Hexagon previousSpawnedHexa = currentNode.CurrentHexagon;
                HexagonFactory.Instance.AddHexagonToPool(previousSpawnedHexa);

                //Önceki döngüden kalanları temizle
                connectedAnchors.Clear();
                dualColors.Clear();
                //Bağlı anchorları al
                currentNode.GetConnectedAnchors(ref connectedAnchors);
                //Hexagon oluştur
                Hexagon spawnedHexa = HexagonFactory.Instance.SpawnHexagonByConnectionColor(currentNode.Position, connectedAnchors, ref dualColors);

                //Grid-Hexagon bağlantılarını sağla
                spawnedHexa.currentNode = currentNode;
                currentNode.CurrentHexagon = spawnedHexa;

                //Başlangıçta oyuncunun hamle hakkı varmı kontrolü için nodu hexagonu değişenler listesinde kaydet
                currentNode.HexagonChanged();
            }
        }
    }

    public void InitializeGrid()
    {
        //Arrayleri oluştur
        InitializeArrays();

        //Gridi oluştur
        CreateGrid();
    }

    public GridNode[][] GetGridNodes()
    {
        return m_GridNodes;
    }

    //Camera görüşü için köşe noktalar
    public void GetCorners(out Vector2 bottomLeft, out Vector2 upperRight)
    {
        bottomLeft = m_GridNodes[0][0].Position;
        upperRight = m_GridNodes[gridWidth - 1][gridHeight - 1].Position;
    }
}
