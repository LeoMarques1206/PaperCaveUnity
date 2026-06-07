using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Detecta hover via Physics.Raycast (compatível com Canvas World Space).
/// Requer BoxCollider no mesmo GameObject e PhysicsRaycaster na câmera.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class TableRowHover : MonoBehaviour
{
    [Header("Hover Z")]
    public float hoverOffsetZ = -1.5f;   // negativo = em direção à câmera
    public float animSpeed    = 10f;

    [Header("Highlight")]
    public Color hoverColor = new Color(0.98f, 0.92f, 0.50f, 1f);

    // --- estado interno ---
    private Vector3 _originLocal;
    private Vector3 _targetLocal;
    private bool    _hovered;

    private List<Image> _cellImages    = new();
    private List<Color> _origColors    = new();

    private BoxCollider _col;

    void Awake()
    {
        _col = GetComponent<BoxCollider>();
        CacheImages();
    }

    void Start()
    {
        _originLocal = transform.localPosition;
        _targetLocal = _originLocal;
    }

    void Update()
    {
        // --- Raycast manual da câmera ---
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool hit = _col.Raycast(ray, out _, 500f);

        if (hit && !_hovered)  EnterHover();
        if (!hit && _hovered)  ExitHover();

        // --- Suaviza movimento ---
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            _targetLocal,
            Time.deltaTime * animSpeed);
    }

    // Chamado pelo TableBuilder logo após posicionar a linha
    public void SetOrigin(Vector3 localPos)
    {
        _originLocal = localPos;
        _targetLocal = localPos;
        transform.localPosition = localPos;
        ResizeCollider();
    }

    // ---- helpers ----

    private void EnterHover()
    {
        _hovered     = true;
        _targetLocal = _originLocal + new Vector3(0f, 0f, hoverOffsetZ);
        for (int i = 0; i < _cellImages.Count; i++)
            _cellImages[i].color = hoverColor;
    }

    private void ExitHover()
    {
        _hovered     = false;
        _targetLocal = _originLocal;
        for (int i = 0; i < _cellImages.Count; i++)
            _cellImages[i].color = _origColors[i];
    }

    private void CacheImages()
    {
        _cellImages.Clear();
        _origColors.Clear();
        foreach (Transform child in transform)
        {
            var img = child.GetComponent<Image>();
            if (img != null)
            {
                _cellImages.Add(img);
                _origColors.Add(img.color);
            }
        }
    }

    // Ajusta o BoxCollider para cobrir todas as células da linha
    private void ResizeCollider()
    {
        if (_col == null) return;

        // Calcula bounds das células filhas
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float depth = 0.1f;

        foreach (Transform child in transform)
        {
            var rt = child.GetComponent<RectTransform>();
            if (rt == null) continue;

            Vector3 pos  = rt.anchoredPosition;
            Vector2 size = rt.sizeDelta;

            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x + size.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y + size.y);
        }

        if (minX == float.MaxValue) return; // sem filhos ainda

        float w = maxX - minX;
        float h = maxY - minY;

        _col.size   = new Vector3(w, h, depth);
        _col.center = new Vector3(minX + w * 0.5f, minY + h * 0.5f, 0f);
        _col.isTrigger = true;
    }
}
