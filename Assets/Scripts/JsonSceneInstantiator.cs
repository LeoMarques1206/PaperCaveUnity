using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class GameObjectData
{
    public string suggestedName;

    public string conceptualOrigin;
    public string category;
    public string visualMetaphor;
    public string behaviourHint;
    public string interactionType;
    public string whyThisFive;

    public string relatedImage;
    public string displayType;
}

[System.Serializable]
public class SceneData
{
    public string paperTitle;
    public string centralContribution;

    public List<GameObjectData> gameObjects;
}

public enum DisplayType
{
    Text,
    Image,
    Graph,
    Table
}

public class JsonSceneInstantiator : MonoBehaviour
{
    [Header("JSON Input")]
    [TextArea(10, 30)]
    public string jsonInput;

    [Header("Prefabs por Tipo")]
    public GameObject prefabTexto;
    public GameObject prefabImagem;
    public GameObject prefabTabela;

    [Header("Prefabs de Gráfico (Aleatórios)")]
    public GameObject[] prefabsGrafico;

    [Header("Título")]
    public GameObject prefabTitle;

    [Header("Âncoras")]
    public Transform anchorTitle;
    public Transform[] anchorSlots;

    [Header("Layout")]
    public float spacingX = 3f;

    public float spacingZ = 3f;

    private readonly List<GameObject> _spawnedObjects = new();

    [ContextMenu("Instanciar da JSON")]
    public void InstantiateFromJson()
    {
        ClearSpawned();

        if (string.IsNullOrWhiteSpace(jsonInput))
        {
            Debug.LogWarning("[JsonSceneInstantiator] jsonInput está vazio.");
            return;
        }

        SceneData data;

        try
        {
            data = JsonUtility.FromJson<SceneData>(jsonInput);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[JsonSceneInstantiator] Erro ao parsear JSON: {e.Message}");
            return;
        }

        if (data == null)
        {
            Debug.LogError("[JsonSceneInstantiator] JSON inválido.");
            return;
        }

        //----------------------------------
        // TÍTULO
        //----------------------------------

        if (prefabTitle != null)
        {
            Vector3 titlePos =
                anchorTitle != null
                    ? anchorTitle.position
                    : Vector3.zero;

            GameObject titleInstance =
                Instantiate(
                    prefabTitle,
                    titlePos,
                    Quaternion.identity);

            titleInstance.name = "Title";

            TMP_Text tmp =
                titleInstance.GetComponent<TMP_Text>();

            if (tmp == null)
                tmp = titleInstance.GetComponentInChildren<TMP_Text>(true);

            if (tmp != null)
            {
                tmp.text = data.paperTitle;
            }
            else
            {
                Debug.LogWarning(
                    "[JsonSceneInstantiator] prefabTitle não possui TMP_Text.");
            }

            _spawnedObjects.Add(titleInstance);

            Debug.Log(
                $"[JsonSceneInstantiator] ✔ Título instanciado: {data.paperTitle}");
        }

        //----------------------------------
        // GAME OBJECTS
        //----------------------------------

        if (data.gameObjects == null ||
            data.gameObjects.Count == 0)
        {
            Debug.LogWarning(
                "[JsonSceneInstantiator] Nenhum gameObject encontrado.");
            return;
        }

        Debug.Log(
            $"[JsonSceneInstantiator] Instanciando {data.gameObjects.Count} objetos...");

        for (int i = 0; i < data.gameObjects.Count; i++)
        {
            GameObjectData goData =
                data.gameObjects[i];

            GameObject prefab =
                GetPrefabForDisplayType(goData.displayType);

            if (prefab == null)
            {
                Debug.LogWarning(
                    $"[JsonSceneInstantiator] Nenhum prefab encontrado para displayType '{goData.displayType}'.");
                continue;
            }

            if (anchorSlots == null ||
                i >= anchorSlots.Length ||
                anchorSlots[i] == null)
            {
                Debug.LogWarning(
                    $"[JsonSceneInstantiator] Âncora [{i}] não configurada.");
                continue;
            }

            Vector3 position =
                anchorSlots[i].position;

            GameObject instance =
                Instantiate(
                    prefab,
                    position,
                    Quaternion.identity);

            instance.name =
                goData.suggestedName;

            //----------------------------------
            // TEXTO AUTOMÁTICO
            //----------------------------------
            if (goData.displayType?.ToLower() == "text")
            {
                string textContent = goData.visualMetaphor ?? "";

                // Remove "Texto: " (7 caracteres)
                if (textContent.Length > 7)
                {
                    textContent = textContent.Substring(7);
                }

                // Remove aspas extras caso existam
                textContent = textContent.Trim().Trim('"', '\'');

                TMP_Text[] texts =
                    instance.GetComponentsInChildren<TMP_Text>(true);

                foreach (TMP_Text text in texts)
                {
                    text.text = textContent;
                }
            }

            //----------------------------------
            // METADADOS
            //----------------------------------

            GameObjectMeta meta =
                instance.GetComponent<GameObjectMeta>();

            if (meta == null)
                meta = instance.AddComponent<GameObjectMeta>();

            meta.suggestedName =
                goData.suggestedName;

            meta.type =
                goData.displayType;

            meta.conceptualOrigin =
                goData.conceptualOrigin;

            meta.category =
                goData.category;

            meta.visualMetaphor =
                goData.visualMetaphor;

            meta.behaviourHint =
                goData.behaviourHint;

            meta.interactionType =
                goData.interactionType;

            meta.whyThisFive =
                goData.whyThisFive;

            _spawnedObjects.Add(instance);

            Debug.Log(
                $"[JsonSceneInstantiator] ✔ '{instance.name}' ({goData.displayType}) instanciado.");
        }

        Debug.Log(
            $"[JsonSceneInstantiator] Finalizado. {_spawnedObjects.Count} objeto(s) criados.");
    }

    [ContextMenu("Limpar Objetos Instanciados")]
    public void ClearSpawned()
    {
        foreach (GameObject go in _spawnedObjects)
        {
            if (go != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(go);
                else
                    Destroy(go);
#else
                Destroy(go);
#endif
            }
        }

        _spawnedObjects.Clear();
    }

    private GameObject GetPrefabForDisplayType(string displayType)
    {
        switch (displayType?.ToLower().Trim())
        {
            case "text":
                return prefabTexto;

            case "image":
                return prefabImagem;

            case "table":
                return prefabTabela;

            case "graph":
            case "chart":
                return GetRandomGraphPrefab();

            default:
                return null;
        }
    }

    private GameObject GetRandomGraphPrefab()
    {
        if (prefabsGrafico == null ||
            prefabsGrafico.Length == 0)
        {
            Debug.LogWarning(
                "[JsonSceneInstantiator] Nenhum prefab de gráfico configurado.");

            return null;
        }

        int randomIndex =
            Random.Range(
                0,
                prefabsGrafico.Length);

        return prefabsGrafico[randomIndex];
    }

    private void Start()
    {
        InstantiateFromJson();
    }
}