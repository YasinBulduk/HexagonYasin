using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Hexagon : MonoBehaviour
{
    //Node tarafından harekete geçirilen hexagon sayısı
    public static int movingHexagonCount;

    public GridNode currentNode;
    public HexagonProperties properties = null;
    public ParticleSystem onDestroyParticle;
    public bool IsMatched;

    [HideInInspector] public int colorId;

    protected SpriteRenderer m_SpriteRenderer;
    protected Coroutine m_MoveRoutine = null;
    protected bool m_Moving = false;

    protected virtual void Awake()
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>();

        //Hexagon Properties atanmamış ise Resources klasöründen al
        if (!properties)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[{gameObject.name}] Properties is not assigned. Hexagon Properties in the Resources folder will be used.");
#endif
            properties = Resources.Load<HexagonProperties>("defaultHexagonProperties");
        }
    }

    protected IEnumerator RotateRoutine(GridNode nextNode, Vector2 center, Vector2 currentDir, Vector2 targetDir, float rotationSpeedDegrees)
    {
        while (true)
        {
            float angle = Vector2.Angle(currentDir, targetDir);

            currentDir = Vector3.RotateTowards(currentDir, targetDir, rotationSpeedDegrees * Mathf.Deg2Rad * Time.deltaTime, 0f);

            Vector2 wantedPos = center + currentDir.normalized * HexMetrics.outerRadius;

#if UNITY_EDITOR
            Debug.DrawRay(center, currentDir, Color.red);
            Debug.DrawRay(center, targetDir, Color.blue);
#endif

            transform.position = wantedPos;

            if (angle <= 0.5f)
            {
                //Varış anında varılan node'un hexagonuna ata
                nextNode.CurrentHexagon = this;

                yield break;
            }

            yield return null;
        }
    }

    //Recursive çağrı için
    protected IEnumerator MoveRoutine(GridNode targetNode, float delay)
    {
        //Hareket halinde değil ise bekle. Sırayla düşmesi için
        if(!m_Moving)
        {
            yield return new WaitForSeconds(delay);
        }

        m_Moving = true;
        while (true)
        {
            Vector3 nodePos = targetNode.Position;
            Vector3 newPosition = Vector3.MoveTowards(transform.position, nodePos, properties.moveSpeed * Time.deltaTime);

            transform.position = newPosition;

            if (Vector3.Distance(transform.position, nodePos) <= 0.01f)
            {
                break;
            }

            yield return null;
        }

        m_Moving = false;
        //Hareket tamamlandığında hareket sayacını eksilt
        movingHexagonCount--;
        //Kaydedilen coroutine'i null'a çek
        m_MoveRoutine = null;

        //Hareket sonunda bağlı olduğu nodu değişti olarak kaydet
        currentNode.HexagonChanged();
    }

    protected virtual void SpawnDestroyParticle()
    {
        ParticleSystem p = Instantiate(onDestroyParticle, transform.position, Quaternion.identity);
        ParticleSystem.MainModule main = p.main;
        Color color = m_SpriteRenderer.material.GetColor("_Color");
        color.a = .85f;
        main.startColor = color;
    }

    protected virtual IEnumerator DestroyRoutine()
    {
        while(true)
        {
            transform.localScale *= .8f;

            if(transform.localScale.x < .3f)
            {
                SpawnDestroyParticle();

                //Scale oynaması, Pozisyon ve Rotasyon değişiklikleri GenericPool tarafından düzeltiliyor.
                HexagonFactory.Instance.AddHexagonToPool(this);

                yield break;
            }

            yield return null;
        }
    }

    public virtual void Destroy()
    {
        StartCoroutine(DestroyRoutine());
    }

    //HexagonFactory için
    public IEnumerator MoveRoutine(GridNode targetNode)
    {
        currentNode = targetNode;
        targetNode.CurrentHexagon = this;

        while (true)
        {
            Vector3 nodePos = targetNode.Position;
            Vector3 newPosition = Vector3.MoveTowards(transform.position, nodePos, properties.moveSpeed * Time.deltaTime);

            transform.position = newPosition;

            if (Vector3.Distance(transform.position, nodePos) <= 0.01f)
            {
                break;
            }

            yield return null;
        }

        //Hareket sonunda bağlı olduğu nodu değişti olarak kaydet
        currentNode.HexagonChanged();
    }

    public void SetRendererOrder(int newOrder)
    {
        m_SpriteRenderer.sortingOrder = newOrder;
    }

    public void SetMaterial(Material newMaterial)
    {
        m_SpriteRenderer.material = newMaterial;
        colorId = newMaterial.GetInstanceID();
    }
    
    public IEnumerator RotateRoutine(GridNode targetNode, Anchor anchor)
    {
        Vector2 currentPosition = transform.position;
        Vector2 currentDirection = currentPosition - anchor.Position;

        Vector2 targetDirection = targetNode.Position - anchor.Position;

        //Method overloading
        yield return StartCoroutine(RotateRoutine(targetNode, anchor.Position, currentDirection, targetDirection, properties.rotationSpeed));
    }

    //Recursive çağrı için
    public void Move(GridNode targetNode, float delay)
    {
        //Daha önce hereke geçirilmiş ise önceden tetiklenen hareketi durdur ve yeni hareket hedefi ile tekrar başlat
        if(m_MoveRoutine != null)
        {
            StopCoroutine(m_MoveRoutine);
            //Durdurulan hareket için hareket sayacını eksilt
            movingHexagonCount--;
        }

        //Hareket sayacını arttır
        movingHexagonCount++;
        //Node bağlantısını sağla
        currentNode = targetNode;
        m_MoveRoutine = StartCoroutine(MoveRoutine(targetNode, delay));
    }
}
