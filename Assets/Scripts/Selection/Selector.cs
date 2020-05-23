using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour, IObservable
{
    public AbstractInput input;
    public SelectionVisual selectionVisualPrefab;

    private bool m_PlayerControl;
    private SelectionVisual m_Visual;
    private Anchor m_SelectedAnchor;
    private List<GridNode> m_Selection = new List<GridNode>();
    private bool m_RotationDone = true;
    private Action m_OnPlayerFoundMatch;
    private Vector2 rayCenter;

    void Start()
    {
        //Pixel hesaplamaları için inputu başlat
        input.InitializeInput();

        CreateSelectionVisual();
        m_PlayerControl = true;
    }

    void Update()
    {
        //Input al
        input.UpdateInput();

        //Player kontrolü yok ise dön
        if (!m_PlayerControl) return;
        //Dönüş tamamlanmadı ise dön
        if (!m_RotationDone) return;

        if (input.HasInput)
        {
            Ray screenToWorldRay = Camera.main.ScreenPointToRay(input.Position);
            rayCenter = screenToWorldRay.origin;

            RaycastHit2D hit = Physics2D.GetRayIntersection(screenToWorldRay, Mathf.Infinity);

            if (hit.collider != null)
            {
                Hexagon selectedHexa = hit.collider.GetComponent<Hexagon>();

                m_SelectedAnchor = selectedHexa.currentNode.FindClosestAnchor(rayCenter);

                SelectHexagons(m_SelectedAnchor);
            }
        }

        //Anchor seçilmemiş ise girme
        if(m_SelectedAnchor != null)
        {
            if (input.HasDrag)
            {
                m_RotationDone = false;
                Vector2 inputDirection = input.DragDirection;
                Vector2 inputWorldPosition = Camera.main.ScreenToWorldPoint(input.Position);

                //Çevir
                RotateSelection(inputWorldPosition, inputDirection);
            }
        }
    }

    //Çevirme yönünü bul
    private void RotateSelection(Vector2 inputWorldPosition, Vector2 inputDirection)
    {
        if (inputWorldPosition.x > m_SelectedAnchor.Position.x) //Sağ
        {
            if (inputWorldPosition.y > m_SelectedAnchor.Position.y) //Sağ üst
            {
                if (inputDirection.x > .5f)
                {
                    StartCoroutine(RotateClockwiseRoutine());
                }
                else if (inputDirection.x < -.5f)
                {
                    StartCoroutine(RotateCounterClockwiseRoutine());
                }
                else if (inputDirection.y > .5f)
                {
                    StartCoroutine(RotateCounterClockwiseRoutine());
                }
                else if (inputDirection.y < -.5f)
                {
                    StartCoroutine(RotateClockwiseRoutine());
                }
            }
            else // Sağ alt
            {
                if (inputDirection.x > .5f)
                {
                    StartCoroutine(RotateCounterClockwiseRoutine());
                }
                else if (inputDirection.x < -.5f)
                {
                    StartCoroutine(RotateClockwiseRoutine());
                }
                else if (inputDirection.y > .5f)
                {
                    StartCoroutine(RotateCounterClockwiseRoutine());
                }
                else if (inputDirection.y < -.5f)
                {
                    StartCoroutine(RotateClockwiseRoutine());
                }
            }
        }
        else //Sol
        {
            if (inputWorldPosition.y > m_SelectedAnchor.Position.y) //Sol üst
            {
                if (inputDirection.x > .5f)
                {
                    StartCoroutine(RotateClockwiseRoutine());
                }
                else if (inputDirection.x < -.5f)
                {
                    StartCoroutine(RotateCounterClockwiseRoutine());
                }
                else if (inputDirection.y > .5f)
                {
                    StartCoroutine(RotateClockwiseRoutine());
                }
                else if (inputDirection.y < -.5f)
                {
                    StartCoroutine(RotateCounterClockwiseRoutine());
                }
            }
            else // Sol alt
            {
                if (inputDirection.x > .5f)
                {
                    StartCoroutine(RotateCounterClockwiseRoutine());
                }
                else if (inputDirection.x < -.5f)
                {
                    StartCoroutine(RotateClockwiseRoutine());
                }
                else if (inputDirection.y > .5f)
                {
                    StartCoroutine(RotateClockwiseRoutine());
                }
                else if (inputDirection.y < -.5f)
                {
                    StartCoroutine(RotateCounterClockwiseRoutine());
                }
            }
        }
    }

    private void CreateSelectionVisual()
    {
#if UNITY_EDITOR
        if(!selectionVisualPrefab)
        {
            Debug.LogError($"[{gameObject.name}] Visual prefab is null. Must be not null.");
            return;
        }
#endif
        m_Visual = Instantiate(selectionVisualPrefab, Vector3.zero, Quaternion.identity);

        m_Visual.Deactivate();
    }

    private IEnumerator RotateClockwiseRoutine()
    {
        int rotateCount = 3;
        while (rotateCount-- > 0)
        {
            StartCoroutine(HexagonRotateRoutine(0, 1));
            StartCoroutine(HexagonRotateRoutine(1, 2));
            StartCoroutine(m_Visual.RotateClockwiseRoutine(m_Selection[0].CurrentHexagon.properties.rotationSpeed));

            //Son hexagonun dönüşü tamamlanana kadar bekle
            yield return StartCoroutine(HexagonRotateRoutine(2, 0));

            //Node'ları değişti olarak kaydet
            foreach (var selectedNode in m_Selection)
            {
                selectedNode.HexagonChanged();
            }

            //Eşleşme var ise terket
            foreach (var selectedNode in m_Selection)
            {
                if (ColorMatcher.CheckColorMatch(selectedNode))
                {
                    //Anchorlar bulunduğumuz frame'de 1 kez kontrol edildiği için 1 frame bekle
                    yield return null;

                    DeactivatePlayer();
                    Notify();
                    ColorMatcher.CheckRegisteredNodesColorMatches();
                    goto BREAK_POINT;
                }
            }
        }
    BREAK_POINT:

        m_RotationDone = true;
    }

    private IEnumerator RotateCounterClockwiseRoutine()
    {
        int rotateCount = 3;
        while (rotateCount-- > 0)
        {
            StartCoroutine(HexagonRotateRoutine(0, 2));
            StartCoroutine(HexagonRotateRoutine(1, 0));
            StartCoroutine(m_Visual.RotateCounterClockwiseRoutine(m_Selection[0].CurrentHexagon.properties.rotationSpeed));

            //Son hexagonun dönüşü tamamlanana kadar bekle
            yield return StartCoroutine(HexagonRotateRoutine(2, 1));

            //Node'ları değişti olarak kaydet
            foreach (var selectedNode in m_Selection)
            {
                selectedNode.HexagonChanged();
            }

            //Eşleşme var ise terket
            foreach (var selectedGrid in m_Selection)
            {
                if (ColorMatcher.CheckColorMatch(selectedGrid))
                {
                    yield return null;
                    DeactivatePlayer();
                    //Playerin eşleşme bulduğunu bildir
                    Notify();
                    //Kaydedilen node'ları kontrol et
                    ColorMatcher.CheckRegisteredNodesColorMatches();
                    goto BREAK_POINT;
                }
            }
        }
    BREAK_POINT:

        m_RotationDone = true;
    }

    private IEnumerator HexagonRotateRoutine(int rotatedGridIndex, int targetGridIndex)
    {
        yield return StartCoroutine(m_Selection[rotatedGridIndex].CurrentHexagon.RotateRoutine(m_Selection[targetGridIndex], m_SelectedAnchor));
    }

    private void SelectHexagons(Anchor selectedAnchor)
    {
        //Önceki hexagonlara verilen render order sıralamasını geri al
        ClearSelectionOrder();
        m_Selection.Clear();

        //Seçilen anchor'a bağlı olan nodeları al
        selectedAnchor.GetConnectedNodes(ref m_Selection);

        //Seçilen hexagonlara render order sıralaması ver
        SetSelectionOrder();

        Vector2 selectionDirection = m_Selection[0].Position - m_SelectedAnchor.Position;

        //Seçim görselini aktif et
        m_Visual.Activate(selectedAnchor.Position, selectionDirection);
    }

    private void ClearSelectionOrder()
    {
        for (int i = 0; i < m_Selection.Count; i++)
        {
            m_Selection[i].CurrentHexagon?.SetRendererOrder(0);
        }
    }

    private void SetSelectionOrder()
    {
        for (int i = 0; i < m_Selection.Count; i++)
        {
            m_Selection[i].CurrentHexagon.SetRendererOrder(1);
        }
    }

    #region DEBUG
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(rayCenter, .3f);

            if (m_Selection.Count > 0)
            {
                Gizmos.color = Color.yellow;
                m_Selection.ForEach(h => Gizmos.DrawWireSphere(h.Position, .5f));
            }
        }
    }
#endif
    #endregion

    //Oyuncuyu devre dışı bırak
    public void DeactivatePlayer()
    {
        m_PlayerControl = false;
        ClearSelectionOrder();
        m_Visual.Deactivate();
    }

    //Oyuncuyu etkinleştir
    public void ActivatePlayer()
    {
        m_PlayerControl = true;
        SetSelectionOrder();
        m_Visual.Activate(m_SelectedAnchor.Position, m_Selection[0].Position - m_SelectedAnchor.Position);
    }

    //Oyuncuyu sıfırla
    public void ResetPlayer()
    {
        m_PlayerControl = true;
    }

    public void Register(Action action)
    {
        m_OnPlayerFoundMatch += action;
    }

    public void Unregister(Action action)
    {
        m_OnPlayerFoundMatch -= action;
    }

    public void Notify()
    {
        m_OnPlayerFoundMatch?.Invoke();
    }
}
