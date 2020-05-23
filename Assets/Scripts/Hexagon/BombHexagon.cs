using System;
using System.Collections;
using TMPro;

public class BombHexagon : Hexagon
{
    public int ExplodeCount { get { return m_ExplodeCount; } set { SetCountText(value); m_ExplodeCount = value; } }
    public Action onBombExplode;

    //Patlama sayacı
    private int m_ExplodeCount;
    private TextMeshPro m_CountText;

    protected override void Awake()
    {
        base.Awake();

        m_CountText = GetComponentInChildren<TextMeshPro>();
    }

    private void SetCountText(int newCount)
    {
        m_CountText.text = newCount.ToString();
    }

    protected override IEnumerator DestroyRoutine()
    {
        while (true)
        {
            transform.localScale *= .8f;

            if (transform.localScale.x < .3f)
            {
                SpawnDestroyParticle();

                currentNode = null;
                HexagonFactory.Instance.DestroyBombHexagon(this);

                yield break;
            }

            yield return null;
        }
    }

    public void DecreaseExplodeCount()
    {
        ExplodeCount--;

        //Eşleşmemiş ve sayacı 0 olmuş ise patlat
        if (!IsMatched && ExplodeCount <= 0)
        {
            Explode();
        }
    }

    public void Explode()
    {
        onBombExplode?.Invoke();
    }
}
