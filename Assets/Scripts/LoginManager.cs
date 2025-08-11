using System;
using System.Collections;
using System.Data.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Anaglyph.Menu;

public class LoginManager : MonoBehaviour
{
    [SerializeField] private NavPagesParent navPagesParent;
    
    [SerializeField] private TMP_InputField id;
    [SerializeField] private TMP_InputField password;

    [SerializeField] private NavPage initialPage;
    [SerializeField] private NavPage successPage;
    [SerializeField] private NavPage failedPage;
    
    // 전송할 데이터 구조 정의
    [Serializable]
    public class PostData
    {
        // public bool success;
        public string user_id;
        public string password;
    }
    
    [Serializable]
    public class TokenResponse
    {
        public string access_token;
        public string token_type;
    }
    
    public void OnRegister()
    {
        PostData data = new PostData
        {
            user_id = id.text,
            password = password.text
        };

        StartCoroutine(PostRegisterData("http://221.163.19.142:58002/users/register", data));
    }

    public void OnLogin()
    {
        PostData data = new PostData
        {
            user_id = id.text,
            password = password.text
        };

        StartCoroutine(PostLoginData("http://221.163.19.142:58002/login/token", data));
    }
    
    public void InitPage()
    {
        navPagesParent.GoToPage(initialPage);
    }

    private void PageSwitch(NavPage goToPage)
    {
        navPagesParent.GoToPage(goToPage);
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
            GetComponent<ResetInputField>().OnClick();
            PageSwitch(successPage);
        }
        else
        {
            Debug.LogError("요청 실패: " + request.error);
            PageSwitch(failedPage);
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
                    PageSwitch(successPage);

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
            PageSwitch(failedPage);
        }
    }
}