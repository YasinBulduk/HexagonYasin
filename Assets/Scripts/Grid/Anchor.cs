using System.Collections.Generic;
using UnityEngine;

public class Anchor
{
    public Vector2 Position => m_Position;

    private Vector2 m_Position;
    private List<GridNode> m_ConnectedNodes;
    private float m_LastUpdateFrame;

    public Anchor(GridNode connected1, GridNode connected2, GridNode connected3)
    {
        m_ConnectedNodes = new List<GridNode> { connected1, connected2, connected3 };

        //Bağlantıları sağla
        connected1.AddAnchor(this);
        connected2.AddAnchor(this);
        connected3.AddAnchor(this);

        CalculatePosition();
    }

    //Bağlı olduğu nodeların merkezini bul
    private void CalculatePosition()
    {
        Vector2 avaragePosition = Vector2.zero;

        m_ConnectedNodes.ForEach(g => avaragePosition += g.Position);

        avaragePosition /= m_ConnectedNodes.Count;
        
        m_Position = avaragePosition;
    }

    public List<GridNode> GetConnectedNodes()
    {
        return new List<GridNode>(m_ConnectedNodes);
    }

    public void GetConnectedNodes(ref List<GridNode> listToFill)
    {
        listToFill.AddRange(m_ConnectedNodes);
    }

    //Renk kontrolünde ortak anchorların 2 kez kontrol edilmesini önlemek için
    //kontrol edilen frame'i kaydet;
    public void UpdateAnchor()
    {
        m_LastUpdateFrame = Time.frameCount;
    }

    public bool IsUpdated()
    {
        return m_LastUpdateFrame == Time.frameCount;
    }

    #region DEBUG
#if UNITY_EDITOR
    public void DebugAnchor()
    {
        m_ConnectedNodes.ForEach(c => Debug.DrawLine(Position, Position + (c.Position - Position) * .5f, Color.black));

        if(m_ConnectedNodes.Count < 3)
        {
            Gizmos.color = Color.blue;
        }
        else
        {
            if(m_LastUpdateFrame == Time.frameCount)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
        }

        Gizmos.DrawWireSphere(m_Position, .1f);
    }
#endif
#endregion
}
