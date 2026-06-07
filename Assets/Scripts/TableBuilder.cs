using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Constrói uma tabela UI dinamicamente. Cada linha de dados
/// recebe um TableRowHover para animação ao passar o mouse.
/// </summary>
public class TableBuilder : MonoBehaviour
{
    [Header("Configuração Visual")]
    public Color headerColor     = new Color(0.15f, 0.35f, 0.60f, 1f);
    public Color rowColorA       = new Color(0.95f, 0.95f, 0.95f, 1f);
    public Color rowColorB       = new Color(0.85f, 0.88f, 0.92f, 1f);
    public Color borderColor     = new Color(0.40f, 0.40f, 0.40f, 1f);
    public Color headerTextColor = Color.white;
    public Color cellTextColor   = new Color(0.10f, 0.10f, 0.10f, 1f);
    public float cellWidth       = 10f;
    public float cellHeight      = 6f;
    public float fontSize        = 4.5f;
    public float headerFontSize  = 5f;

public void Build(int rows, int columns, List<string> headers, List<List<string>> rowData)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) rt = gameObject.AddComponent<RectTransform>();

        float totalWidth  = 30f;
        float totalHeight = 30f;
        rt.sizeDelta = new Vector2(totalWidth, totalHeight);

        int totalRows = rows + 1;
        cellWidth  = totalWidth  / columns;
        cellHeight = totalHeight / totalRows;

        headerFontSize = 5f;
        fontSize       = 4.5f;

        Image bg = GetComponent<Image>();
        if (bg == null) bg = gameObject.AddComponent<Image>();
        bg.color = borderColor;

        // Linha de header (sem hover)
        GameObject headerRow = CreateRowObject("Row_Header", 0, totalRows);
        for (int c = 0; c < columns; c++)
        {
            string headerText = (headers != null && c < headers.Count) ? headers[c] : $"Col {c + 1}";
            CreateCell(headerRow, c, headerText, headerColor, headerTextColor, true);
        }

        // Linhas de dados (com hover)
        for (int r = 0; r < rows; r++)
        {
            Color rowColor = (r % 2 == 0) ? rowColorA : rowColorB;
            GameObject rowGO = CreateRowObject($"Row_{r + 1}", r + 1, totalRows);

            for (int c = 0; c < columns; c++)
            {
                string cellText = "";
                if (rowData != null && r < rowData.Count && rowData[r] != null && c < rowData[r].Count)
                    cellText = rowData[r][c];
                CreateCell(rowGO, c, cellText, rowColor, cellTextColor, false);
            }

            // Hover: adicionado DEPOIS das celulas para que ResizeCollider encontre os filhos
            TableRowHover hover = rowGO.AddComponent<TableRowHover>();
            hover.SetOrigin(rowGO.transform.localPosition);
        }
    }

    private GameObject CreateRowObject(string rowName, int rowIndex, int totalRows)
    {
        GameObject rowGO = new GameObject(rowName);
        rowGO.transform.SetParent(transform, false);

        RectTransform rowRt = rowGO.AddComponent<RectTransform>();
        rowRt.anchorMin = Vector2.zero;
        rowRt.anchorMax = Vector2.zero;
        rowRt.pivot     = Vector2.zero;

        float totalHeight = totalRows * cellHeight;
        float y = totalHeight - (rowIndex + 1) * cellHeight;
        rowRt.anchoredPosition = new Vector2(0f, y);
        rowRt.sizeDelta        = Vector2.zero;

        return rowGO;
    }

    private void CreateCell(GameObject parent, int col, string content, Color bgColor, Color textColor, bool isHeader)
    {
        GameObject cell = new GameObject($"Cell_{col}");
        cell.transform.SetParent(parent.transform, false);

        RectTransform cellRt = cell.AddComponent<RectTransform>();
        cellRt.anchorMin        = Vector2.zero;
        cellRt.anchorMax        = Vector2.zero;
        cellRt.pivot            = Vector2.zero;
        cellRt.anchoredPosition = new Vector2(col * cellWidth, 0f);
        cellRt.sizeDelta        = new Vector2(cellWidth, cellHeight);

        Image cellBg = cell.AddComponent<Image>();
        cellBg.color = bgColor;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(cell.transform, false);

        RectTransform textRt = textGO.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(0.5f, 0.5f);
        textRt.offsetMax = new Vector2(-0.5f, -0.5f);

        TMP_Text tmp         = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text             = content;
        tmp.color            = textColor;
        tmp.fontStyle        = isHeader ? FontStyles.Bold : FontStyles.Normal;
        tmp.alignment        = TextAlignmentOptions.Center;
        tmp.overflowMode     = TextOverflowModes.Ellipsis;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin      = 1f;
        tmp.fontSizeMax      = isHeader ? headerFontSize : fontSize;
    }
}
