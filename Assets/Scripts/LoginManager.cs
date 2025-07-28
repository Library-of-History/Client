using System;
using System.Collections;
using System.Data.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    // public TextMeshProUGUI Id;
    // public TextMeshProUGUI Password;
    
    public string Id;
    public string Password;
    
    // 전송할 데이터 구조 정의
    [System.Serializable]
    public class PostData
    {
        // public bool success;
        public string user_id;
        public string password;
    }
    
    [System.Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string token_type;
    }

    private void Start()
    {
        OnLogin();
    }
    
    public void OnRegister()
    {
        PostData data = new PostData
        {
            // user_id = Id.text,
            // password = Password.text
            user_id = Id,
            password = Password
        };

        StartCoroutine(PostRegisterData("http://221.163.19.142:58026/users/register", data));
    }

    public void OnLogin()
    {
        PostData data = new PostData
        {
            // user_id = Id.text,
            // password = Password.text
            user_id = Id,
            password = Password
        };

        StartCoroutine(PostLoginData("http://221.163.19.142:58026/login/token", data));
    }
    
    IEnumerator PostRegisterData(string url, PostData data)
    {
        // 1. JSON 문자열로 변환
        string jsonData = JsonUtility.ToJson(data);
        
        // 2. 바이트 배열로 변환
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // 3. UnityWebRequest 객체 구성
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        // 4. 요청 전송
        yield return request.SendWebRequest();
        
        Debug.Log(request.downloadHandler.text);
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("요청 성공: " + request.downloadHandler.text);
            
            var jsonText = request.downloadHandler.text;
            // PostData postData = JsonUtility.FromJson<PostData>(jsonText);
        }
        else
        {
            Debug.LogError("요청 실패: " + request.error);
        }
    }

    IEnumerator PostLoginData(string url, PostData data)
    {
        WWWForm form = new WWWForm();
        form.AddField("grant_type", "password");
        form.AddField("username", data.user_id);
        form.AddField("password", data.password);
        
        UnityWebRequest request = UnityWebRequest.Post(url, form);

        // 4. 요청 전송
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            // 응답 본문: "string" 형태의 JSON 문자열
            string jsonResponse = request.downloadHandler.text;
            
            try
            {
                TokenResponse tokenData = JsonUtility.FromJson<TokenResponse>(jsonResponse);
                if (tokenData != null && !string.IsNullOrEmpty(tokenData.access_token))
                {
                    string token = tokenData.access_token;
                    string type = tokenData.token_type;

                    Debug.Log($"Access Token: {token}");
                    Debug.Log($"Token Type: {type}");

                    // 예: SystemManager.Inst.Token 저장
                    SystemManager.Inst.Token = token;

                    // 필요 시 토큰 타입을 함께 저장하거나 Authorization 헤더 생성 시 활용 가능
                    // 예: "Bearer " + token
                }
                else
                {
                    Debug.LogWarning("토큰 응답이 유효하지 않습니다.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("JSON 파싱 오류: " + e.Message);
            }
        }
        else
        {
            Debug.LogError($"HTTP Error: {request.responseCode}, {request.error}");
        }
    }
}