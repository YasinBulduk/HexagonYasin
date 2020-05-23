using System;
using System.Collections.Generic;
using UnityEngine;

public static class ColorMatcher
{
    public static Action<List<GridNode>> OnColorMatch { get; set; }
    public static Action OnPlayerHasNoMove { get; set; }

    //Değişiklik olan node'ların renk eşleşmesi için kaydı
    private static Queue<GridNode> changedNodes = new Queue<GridNode>();
    //Değişiklik olan node'ların bağlı olduğu anchorlarda çift renk eşleşmesi olduğunu kontrol için
    private static List<GridNode> changedNodeList = new List<GridNode>();
    //Çift renk eşleşmesi barındıran anchorlar. 
    //Listenin tüm elemanları çift renk eşleşmesini değişiklikten dolayı sağlamıyor olabilir. Bir sonraki kontrolünde listeden atılır.
    private static LinkedList<Anchor> twoColorAnhors = new LinkedList<Anchor>();

    //Anchora bağlı olan node'larda aynı renge sahip 2 tane hexagon varmı?
    private static bool IsAnchorConnectionsHaveDualColorMatch(Anchor anchor, out GridNode differentColoredNode, out List<GridNode> sameColoredNodes)
    {
        //Aynı olup olmadığını hashset ile kontrol
        HashSet<int> matcherHash = new HashSet<int>();
        //Anchor'un bağlı olduğu node'ları al
        List<GridNode> currentAnchorNodes = anchor.GetConnectedNodes();

        differentColoredNode = null;
        int dualColorId = -1;
        bool result = false;
        for (int i = 0; i < currentAnchorNodes.Count; i++)
        {
            Hexagon currentHexa = currentAnchorNodes[i].CurrentHexagon;

            //Eşleşmeyi kontrol et daha önce eklenmiş ise çık
            if (result = !matcherHash.Add(currentHexa.colorId))
            {
                //Son kontrol edilen rengi ata
                dualColorId = currentHexa.colorId;
                break;
            }
        }

        //Farklı olan node'u tespit et
        differentColoredNode = currentAnchorNodes.Find(n => n.CurrentHexagon.colorId != dualColorId);
        //Aynı olan node'ları tespit et
        sameColoredNodes = currentAnchorNodes.FindAll(n => n.CurrentHexagon.colorId == dualColorId);

        return result;
    }

    private static bool IsAnchorConnectionsHaveDualColorMatch(Anchor anchor, out int dualColorId)
    {
        HashSet<int> matcherHash = new HashSet<int>();
        //Anchor bağlantılarını al
        List<GridNode> currentAnchorNodes = anchor.GetConnectedNodes();

        dualColorId = -1;
        bool result = false;
        for (int i = 0; i < currentAnchorNodes.Count; i++)
        {
            Hexagon currentHexa = currentAnchorNodes[i].CurrentHexagon;

            //Başlangıçtaki eksik olan hexa'nın node'u ise devam et
            if (!currentHexa) continue;

            //Eşleşmeyi kontrol et var ise çık
            if (result = !matcherHash.Add(currentHexa.colorId))
            {
                //Son kontrol edilen rengi ata
                dualColorId = currentHexa.colorId;
                break;
            }
        }
        return result;
    }

    private static bool IsAnchorConnectionsHaveTripleColorMatch(Anchor anchor, out List<GridNode> connectedNodes)
    {
        connectedNodes = new List<GridNode>();
        //Anchor'un bağlı olduğu node'ları al
        anchor.GetConnectedNodes(ref connectedNodes);
        int matchCount = 0;

        int previousColorId = connectedNodes[0].CurrentHexagon.colorId;
        //Her node için renk aynımı kontrol et aynı ise sayacı arttır
        for (int i = 1; i < connectedNodes.Count; i++)
        {
            int currentHexagonColorId = connectedNodes[i].CurrentHexagon.colorId;

            if (previousColorId == currentHexagonColorId)
            {
                matchCount++;
            }
            else
            {
                //Ilk kontrol başarısız ise çık
                break;
            }
            previousColorId = connectedNodes[i].CurrentHexagon.colorId;
        }

        //
        bool result = matchCount == 2;

        //Eşleşme var ise hexagonları işaretle
        if (result)
        {
            connectedNodes.ForEach(n => n.CurrentHexagon.IsMatched = true);
        }

        return result;
    }

    //Verilen node'un bağlı olduğu anchorların bağlı olduğu tüm node'ların renk kontrolü
    private static void CheckColorMatch(GridNode changedNode, ref List<GridNode> matchedNodes)
    {
        List<Anchor> connectedAnchors = new List<Anchor>();
        //Bağlı olan anchor'ları al
        changedNode.GetConnectedAnchors(ref connectedAnchors);

        foreach (var anchor in connectedAnchors)
        {
            //Anchora aynı frame içinde bakılmış ise devam et
            if (anchor.IsUpdated()) continue;

            //Acnhoru güncelle
            anchor.UpdateAnchor();

            //Anchoru bağlı olduğu node'lardan renk eşleşmesi sağlayanları listeye ekle
            List<GridNode> connectedNodes;
            if (IsAnchorConnectionsHaveTripleColorMatch(anchor, out connectedNodes))
            {
                matchedNodes.AddRange(connectedNodes);
            }
        }
    }

    //Değişiklik listesine kaydedilmiş node'ların bağlı olduğu anchorların
    //bağlı olduğu node'lardan en az 2 si aynı renk ise listeye ekle
    private static void FindTwoColorAnchors()
    {
        List<Anchor> anchors = new List<Anchor>();

        //Hexagon değişiklikleri kaydedilmiş node listesi
        for (int i = 0; i < changedNodeList.Count; i++)
        {
            GridNode currentNode = changedNodeList[i];

            anchors.Clear();
            //Seçilen node'un bağlı olduğu anchorları al
            currentNode.GetConnectedAnchors(ref anchors);

            for (int k = 0; k < anchors.Count; k++)
            {
                Anchor currentAnchor = anchors[k];

                GridNode differentNode;
                List<GridNode> sameColorNodes;
                //Seçilen anchor'un bağlı olduğu node'ların herhangi ikisi aynı renk ise listeye ekle
                if (IsAnchorConnectionsHaveDualColorMatch(currentAnchor, out differentNode, out sameColorNodes))
                {
                    twoColorAnhors.AddFirst(currentAnchor);
                }
            }
        }

        //Hexagon değişikliği kaydedilen listeyi bir sonraki kontrol için temizle
        changedNodeList.Clear();
    }

    //Daha önce tespit edilen aynı iki renge sahip hexagonlara bağlı anchorlar için
    //Yanyana olan aynı renk iki hexagonun, aynı renk olmayan üçüncüsünün bağlı olduğu node'larda aynı renkten hexagon olup olmadığı
    private static bool CheckTwoColorAnchors()
    {
        List<GridNode> connectedNodes = new List<GridNode>();
        //Artık bağlı olduğu node'lardan en az ikisi aynı renge sahip olmayan anchorlar
        List<Anchor> oldAchors = new List<Anchor>();

        //Komşuları içinde iki tane aynı renk node olan anchorlar
        foreach (Anchor currentAnchor in twoColorAnhors)
        {
            GridNode differentNode;
            List<GridNode> sameColorNodes;
            //Hala komşularından ikisi aynı renk ise
            if (IsAnchorConnectionsHaveDualColorMatch(currentAnchor, out differentNode, out sameColorNodes))
            {
                connectedNodes.Clear();
                //Rengi farklı olan node'un bağlı olduğu node'ları al
                differentNode.GetConnectedNeighbors(ref connectedNodes);

                //Aynı renge sahip komşuları listeden çıkar
                GridNode sameColor0 = sameColorNodes[0];
                GridNode sameColor1 = sameColorNodes[1];
                connectedNodes.Remove(sameColor0);
                connectedNodes.Remove(sameColor1);

                //Kalan komşularda aynı renkten hexagon var ise oyuncu hala hamle yapabilir.
                if(connectedNodes.Exists(g => g.CurrentHexagon.colorId == sameColor0.CurrentHexagon.colorId))
                {
                    //Biriktirilmiş eski anchorları listeden çıkar
                    if(oldAchors.Count > 0)
                    {
                        oldAchors.ForEach(a => twoColorAnhors.Remove(a));
                    }

                    return true;
                }
            }
            else //Artık komşuları aynı renk değil ise biriktir
            {
                oldAchors.Add(currentAnchor);
            }
        }

        //Hiç eşleşme kalmadı ise Oyuncunun hamle hakkı yok
        //Biriktirilmiş eski anchorları listeden çıkar
        if (oldAchors.Count > 0)
        {
            oldAchors.ForEach(a => twoColorAnhors.Remove(a));
        }

        return false;
    }

    public static bool CheckPlayerHasAnyMove()
    {
        //Bağlı olduğu node'lardan en az ikisi aynı renk olan anchorları bul
        FindTwoColorAnchors();
        //Bulunan anchorların aynı renk olmayan node'unun etrafındaki node'larda aynı renk varmı bak
        bool result = CheckTwoColorAnchors();

        //Yok ise Oyunucunun hamlesi kalmadığını bildir
        if(!result)
        {
            OnPlayerHasNoMove?.Invoke();
        }
#if UNITY_EDITOR
        Debug.Log("[ColorMatcher] PLAYERHASMOVE: " + result);
#endif
        return result;
    }

    public static bool CheckColorMatch(GridNode changedNode)
    {
        List<Anchor> connectedAnchors = new List<Anchor>();

        changedNode.GetConnectedAnchors(ref connectedAnchors);

        foreach (var anchor in connectedAnchors)
        {
            //Ortak anchorları tekrar update etmemek için geç
            if (anchor.IsUpdated()) continue;

            //Anchoru güncelle
            anchor.UpdateAnchor();

            //Bağlı olduğu anchorlardan herhangi biri 3 aynı renk node'a bağlı ise eşleşme var
            List<GridNode> connectedNodes;
            if (IsAnchorConnectionsHaveTripleColorMatch(anchor, out connectedNodes))
            {
                return true;
            }
        }

        return false;
    }

    public static void GetDualColorsFromAnchors(List<Anchor> currentAnchors, ref HashSet<int> result)
    {
        if (currentAnchors.Count <= 0) return;

        foreach (var anchor in currentAnchors)
        {
            int dualColorId;

            if (IsAnchorConnectionsHaveDualColorMatch(anchor, out dualColorId))
            {
                result.Add(dualColorId);
            }
        }
    }

    //Değişiklik kaydedilmiş tüm node'ları renk eşleşmesi için kontrol et
    public static void CheckRegisteredNodesColorMatches()
    {
        List<GridNode> matchedNodes = new List<GridNode>();
        while (changedNodes.Count > 0)
        {
            CheckColorMatch(changedNodes.Dequeue(), ref matchedNodes);
        }

        if (matchedNodes.Count > 0)
        {
            OnColorMatch?.Invoke(matchedNodes);
        }
    }

    //Hexagonu değişmiş node'ları kaydet
    public static void RegisterChangedNode(GridNode changedNode)
    {
        changedNodes.Enqueue(changedNode);
        changedNodeList.Add(changedNode);
    }
}
