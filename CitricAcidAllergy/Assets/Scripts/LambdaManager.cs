using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class LambdaManager
{
    public static string submitURL = "";
    public static string getURL = "";

    public static void SubmitJson(string json)
    {
        GameManager.Instance.StartCoroutine(PostJson(submitURL, json));
    }

    private static IEnumerator PostJson(string url, string jsonBody)
    {
        using UnityWebRequest request = new UnityWebRequest(url, "POST");

        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Response: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
        }
    }

    public static void FetchJson(int level, System.Action<ReinfocementSerializeStruct?> onSuccess)
    {
        GameManager.Instance.StartCoroutine(GetJson(level, getURL, onSuccess));
    }

    [Serializable]
    private struct ReinfocementReturnJsonObject
    {
        [SerializeField]
        public ReinfocementSerializeStruct item;
    }

    private static IEnumerator GetJson(int level, string url, System.Action<ReinfocementSerializeStruct?> onSuccess)
    {
        string json = $"{{\"level\":{level}}}";

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var response = JsonUtility.FromJson<ReinfocementReturnJsonObject>(request.downloadHandler.text);
            onSuccess?.Invoke(response.item);
        }
        else
        {
            Debug.LogError("POST request failed: " + request.error);
            onSuccess?.Invoke(null);
        }
    }
}
