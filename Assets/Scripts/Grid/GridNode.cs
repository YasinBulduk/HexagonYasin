using System;
using System.Collections.Generic;
using UnityEngine;

public class GridNode
{
    public Vector2 Position => m_Position;
    public int Row { get; } //X düzlemi
    public int Col { get; } //Y düzlemi
    public Action<GridNode> OnHexagonChange { get; set; }

    public Hexagon CurrentHexagon { get; set; }

    //Private Variables
    private Vector2 m_Position;
    private HexGrid m_Grid;
    private List<Anchor> m_Anchors;
    private List<GridNode> m_Neighbors;

    public GridNode(int row, int col, Vector2 nodePosition, HexGrid grid)
    {
        Row = row;
        Col = col;
        m_Position = nodePosition;
        m_Grid = grid;
        m_Neighbors = new List<GridNode>();
        m_Anchors = new List<Anchor>();
    }

    //Hexagonu değişen node'ları kaydetmek için
    public void HexagonChanged()
    {
        OnHexagonChange(this);
    }

    public void GetConnectedAnchors(ref List<Anchor> listToFill)
    {
        listToFill.AddRange(m_Anchors);
    }

    public void GetConnectedNeighbors(ref List<GridNode> listToFill)
    {
        listToFill.AddRange(m_Neighbors);
    }

    public void AddAnchor(Anchor anchorToAdd)
    {
        m_Anchors.Add(anchorToAdd);
    }

    //Mesafe karşılaştırması. Bağlı olduğu anchorlardan en yakın olanı bul
    public Anchor FindClosestAnchor(Vector2 position)
    {
        Anchor closestAnchor = m_Anchors[0];
        float closestDistance = Mathf.Abs((position - closestAnchor.Position).sqrMagnitude);

        for (int i = 1; i < m_Anchors.Count; i++)
        {
            Anchor currentAnchor = m_Anchors[i];
            float currentDistance = Mathf.Abs((position - currentAnchor.Position).sqrMagnitude);

            if (currentDistance < closestDistance)
            {
                closestAnchor = currentAnchor;
                closestDistance = currentDistance;
            }
        }

        return closestAnchor;
    }

    //Yanında bulunan tüm node'ları bul
    //Bulunan node'lar listeye saat yönünde eklenir.
    public void FindNeighbors(GridNode[][] gridNodes)
    {
        if (Row % 2 == 0)
        {
            if (Row == 0)
            {
                if (Col == 0)
                {
                    //sol alt köşe
                    //1 yan, 1 üst
                    m_Neighbors.Add(gridNodes[Row][Col + 1]);//üst
                    m_Neighbors.Add(gridNodes[Row + 1][Col]);//yan
                }
                else if (Col == gridNodes[0].Length - 1)
                {
                    //sol üst köşe
                    //1 yan, 2 alt
                    m_Neighbors.Add(gridNodes[Row + 1][Col]);//yan
                    m_Neighbors.Add(gridNodes[Row + 1][Col - 1]);//alt
                    m_Neighbors.Add(gridNodes[Row][Col - 1]);//alt
                }
                else
                {
                    //sol orta
                    //1 yan, 2alt, 1 üst
                    m_Neighbors.Add(gridNodes[Row][Col + 1]);//üst
                    m_Neighbors.Add(gridNodes[Row + 1][Col]);//yan
                    m_Neighbors.Add(gridNodes[Row + 1][Col - 1]);//alt
                    m_Neighbors.Add(gridNodes[Row][Col - 1]);//alt
                }
            }
            else if (Row == gridNodes.Length - 1)
            {
                if (Col == 0)
                {
                    //sağ alt köşe
                    //1 yan, 1 üst
                    m_Neighbors.Add(gridNodes[Row][Col + 1]);//üst
                    m_Neighbors.Add(gridNodes[Row - 1][Col]);//yan
                }
                else if (Col == gridNodes[0].Length - 1)
                {
                    //sağ üst köşe
                    //1 yan, 2 alt
                    m_Neighbors.Add(gridNodes[Row][Col - 1]);//alt
                    m_Neighbors.Add(gridNodes[Row - 1][Col - 1]);//alt
                    m_Neighbors.Add(gridNodes[Row - 1][Col]);//yan
                }
                else
                {
                    //sağ orta
                    //1 yan, 2alt, 1 üst
                    m_Neighbors.Add(gridNodes[Row][Col + 1]);//üst
                    m_Neighbors.Add(gridNodes[Row][Col - 1]);//alt
                    m_Neighbors.Add(gridNodes[Row - 1][Col - 1]);//alt
                    m_Neighbors.Add(gridNodes[Row - 1][Col]);//yan
                }
            }
            else if (Col == 0)
            {
                //alt orta
                //2 yan, 1 üst
                m_Neighbors.Add(gridNodes[Row][Col + 1]);//üst
                m_Neighbors.Add(gridNodes[Row + 1][Col]);//yan
                m_Neighbors.Add(gridNodes[Row - 1][Col]);//yan
            }
            else if (Col == gridNodes[0].Length - 1)
            {
                //üst orta
                //2yan, 3 alt
                m_Neighbors.Add(gridNodes[Row + 1][Col]);//yan
                m_Neighbors.Add(gridNodes[Row + 1][Col - 1]);//alt
                m_Neighbors.Add(gridNodes[Row][Col - 1]);//alt
                m_Neighbors.Add(gridNodes[Row - 1][Col - 1]);//alt
                m_Neighbors.Add(gridNodes[Row - 1][Col]);//yan
            }
            else
            {
                //merkez
                //2 yan, 1 üst, 3 alt
                m_Neighbors.Add(gridNodes[Row][Col + 1]);//üst
                m_Neighbors.Add(gridNodes[Row + 1][Col]);//yan
                m_Neighbors.Add(gridNodes[Row + 1][Col - 1]);//alt
                m_Neighbors.Add(gridNodes[Row][Col - 1]);//alt
                m_Neighbors.Add(gridNodes[Row - 1][Col - 1]);//alt
                m_Neighbors.Add(gridNodes[Row - 1][Col]);//yan
            }
        }
        else
        {
            if (Row == gridNodes.Length - 1)
            {
                if (Col == 0)
                {
                    //sağ alt
                    //1 yan, 2 üst
                    m_Neighbors.Add(gridNodes[Row][Col + 1]);//üst
                    m_Neighbors.Add(gridNodes[Row - 1][Col]);//yan
                    m_Neighbors.Add(gridNodes[Row - 1][Col + 1]);//üst
                }
                else if (Col == gridNodes[0].Length - 1)
                {
                    //sağ üst
                    //1 yan 1 alt
                    m_Neighbors.Add(gridNodes[Row][Col - 1]);//alt
                    m_Neighbors.Add(gridNodes[Row - 1][Col]);//yan
                }
                else
                {
                    //sağ orta
                    //1 yan 1 alt 2 üst
                    m_Neighbors.Add(gridNodes[Row][Col + 1]);//üst
                    m_Neighbors.Add(gridNodes[Row][Col - 1]);//alt
                    m_Neighbors.Add(gridNodes[Row - 1][Col]);//yan
                    m_Neighbors.Add(gridNodes[Row - 1][Col + 1]);//üst
                }
            }
            else if (Col == 0)
            {
                //alt orta
                //2 yan 3 üst
                m_Neighbors.Add(gridNodes[Row][Col + 1]);//üst
                m_Neighbors.Add(gridNodes[Row + 1][Col + 1]);//üst
                m_Neighbors.Add(gridNodes[Row + 1][Col]);//yan
                m_Neighbors.Add(gridNodes[Row - 1][Col]);//yan
                m_Neighbors.Add(gridNodes[Row - 1][Col + 1]);//üst
            }
            else if (Col == gridNodes[0].Length - 1)
            {
                //üst orta
                //2 yan 1 alt
                m_Neighbors.Add(gridNodes[Row + 1][Col]);//yan
                m_Neighbors.Add(gridNodes[Row][Col - 1]);//alt
                m_Neighbors.Add(gridNodes[Row - 1][Col]);//yan
            }
            else
            {
                //merkez
                //2 yan, 3 üst, 1 alt
                m_Neighbors.Add(gridNodes[Row][Col + 1]); //üst
                m_Neighbors.Add(gridNodes[Row + 1][Col + 1]); //üst
                m_Neighbors.Add(gridNodes[Row + 1][Col]); //yan
                m_Neighbors.Add(gridNodes[Row][Col - 1]); //alt
                m_Neighbors.Add(gridNodes[Row - 1][Col]); //yan
                m_Neighbors.Add(gridNodes[Row - 1][Col + 1]); //üst
            }
        }
    }

    //Recursive
    //Düşme hissi için delay. Bir sonraki node'un harekete geçmeden önce beklemesi gereken süre.
    public GridNode MakeNodeEmpty(float delay)
    {
        //Bağlı olduğu hexagonu null'a çek
        CurrentHexagon = null;

        //En üstteki node ise veya daha önce kaydırma işlemine uğramış ise kendisini dön
        if (Col == m_Grid.gridHeight - 1 || !m_Neighbors[0].CurrentHexagon)
        {
            return this;
        }

        //Üst node'un hexagonunu al ve hareket ettir
        GridNode upperNode = m_Neighbors[0];
        CurrentHexagon = upperNode.CurrentHexagon;
        CurrentHexagon.Move(this, delay);

        //Üst node'a recursive çağrı yap
        return upperNode.MakeNodeEmpty(delay += .1f);
    }

    public override string ToString()
    {
        return $"[GridNode] Row:{Row} Col:{Col} ConnectedNodeCount:{m_Neighbors.Count} ConnectedAnchorCount{m_Anchors.Count}";
    }

    #region DEBUG
#if UNITY_EDITOR
    public void DebugNode()
    {
        if (CurrentHexagon == null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(Position, .3f);
        }

        int count = 0;
        foreach (GridNode neig in m_Neighbors)
        {
            switch (count)
            {
                case 0:
                    Gizmos.color = Color.red;
                    break;
                case 1:
                    Gizmos.color = Color.green;
                    break;
                case 2:
                    Gizmos.color = Color.blue;
                    break;
                case 3:
                    Gizmos.color = Color.cyan;
                    break;
                case 4:
                    Gizmos.color = Color.magenta;
                    break;
                default:
                    Gizmos.color = Color.yellow;
                    count = 0;
                    break;
            }

            float lineLength = .25f;
            Gizmos.DrawLine(Position, Position + (neig.Position - Position) * lineLength);
            count++;
        }
    }
#endif
#endregion
}