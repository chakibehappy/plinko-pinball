using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{

    public bool showDebug = true;
    public DomainType domainType = DomainType.DEV;
    public string developingDomain = "https://shgn22dev.saibara619.xyz/";
    public string stagingDomain = "https://shgn22dev.saibara619.xyz/";
    public string releaseDomain = "https://shgn22dev.saibara619.xyz/";
    public string dataUserURL = "api/auth/data";
    public string historyTransactionURL = "game/history";
    public string sendBetDataURL = "game/send_bet";
    public string encryptDataURL = "game/enc-result";
    public string settingPropertiesURL = "game/settings/properties";
    public string triggerLoginURL = "https://apin22dev.saibara619.xyz/api/operator/auth-game/?agent=meja-hoki&game=PLB00001&token=abcd123456";
    public string encryptionKey = "bce13cb9b55baa2409fa33259ad589319caa27eeb3fff94bf2c111aed1fc81e6";

    string encryptedJson;
    BetData decrypted;

    public string jsonFileName = "config.json";

    public enum DomainType
    {
        DEV,
        STAGING,
        RELEASE
    }

    UserDataResponse userDataResponse;

    [Serializable]
    public class ConfigData
    {
        public string api_url;
    }

    public IEnumerator GetAPIFromConfig(Action nextAction = null)
    {
        string url = Application.absoluteURL;
        if (url.EndsWith("index.html"))
        {
            url = url.Substring(0, url.Length - "index.html".Length);
        }
        url += jsonFileName;

        using UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        domainType = DomainType.RELEASE;

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error loading JSON: " + request.error);
        }
        else
        {
            string jsonData = request.downloadHandler.text;
            if(jsonData != string.Empty)
            {
                Log("JSON Loaded: " + jsonData);
                ConfigData config = JsonUtility.FromJson<ConfigData>(jsonData);
                Log("API URL: " + config.api_url);
                if(releaseDomain != "")
                {
                    releaseDomain = config.api_url;
                }
            }
            nextAction?.Invoke();
        }
    }

    public void ReceiveConfigDecryptedData(string decryptedData)
    {
        DecryptedConfigData data = JsonUtility.FromJson<DecryptedConfigData>(decryptedData);
        developingDomain = data.url_dev;
        stagingDomain = data.url_staging;
        releaseDomain = data.url_release;
        switch (data.env)
        {
            case "dev":
                domainType = DomainType.DEV;
                break;
            case "staging":
                domainType = DomainType.STAGING;
                break;
            case "release":
                domainType = DomainType.RELEASE;
                break;
        }
        Log(decryptedData);
    }

    public string GetDomain()
    {
        if (domainType == DomainType.DEV)
            return developingDomain;
        else if (domainType == DomainType.STAGING)
            return stagingDomain;
        else
            return releaseDomain;
    }

    public string GetDataUserAPI()
    {
        return GetDomain() + dataUserURL;
    }

    public string GetHistoryTransactionAPI()
    {
        return GetDomain() + historyTransactionURL;
    }

    public string GetSendBetDataAPI()
    {
        return GetDomain() + sendBetDataURL;
    }

    public string GetEncryptDataAPI()
    {
        //return GetDomain() + encryptDataURL;
        return "https://shgn22dev.saibara619.xyz/" + encryptDataURL;
    }

    public string GetSettingPropertiesAPI()
    {
        return GetDomain() + settingPropertiesURL;
    }

    public string TriggerLoginAPI()
    {
        return triggerLoginURL;
    }

    public IEnumerator TriggerLoginIE(Action<UserDataResponse> nextAction = null)
    {
        UnityWebRequest request = UnityWebRequest.Get(TriggerLoginAPI());
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Log("Error: " + request.error);
        }
        else
        {
            //Log(request.downloadHandler.text);
            yield return StartCoroutine(GetUserDataIE());
            nextAction?.Invoke(userDataResponse);
        }
    }

    public List<List<string>> ParseNestedList(string input)
    {
        string[] rows = input.Split("]],");
        List<List<string>> list = new();

        for (int i = 0; i < rows.Length; i++)
        {
            List<string> nestedList = new();
            string[] nestedRow = rows[i].Split(",[");
            for(int j = 0; j < nestedRow.Length; j++)
            {
                if (nestedRow[j].Contains("[")){
                    string[] models = nestedRow[j].Replace("[", "").Replace("]", "").Split(",");
                    foreach (var item in models)
                    {
                        nestedList.Add(item);
                    }
                }
                else
                {
                    nestedList.Add(nestedRow[j].Replace("[", "").Replace("]", ""));
                }
            }
            list.Add(nestedList);
        }

        
        return list;
    }

    IEnumerator GetUserDataIE(Action<UserDataResponse> nextAction = null)
    {
        UnityWebRequest request = UnityWebRequest.Get(GetDataUserAPI());
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Log("Error: " + request.error);
        }
        else
        {
            string responseJson = request.downloadHandler.text;
            string modeltexts = responseJson.Split("models")[1].Replace(":","")
                .Replace("{", "").Replace("}", "").Replace("\"", "").Replace("[[","[").Replace("]]]","]]");
            Log(responseJson);
            UserDataResponse response = JsonUtility.FromJson<UserDataResponse>(responseJson);
            userDataResponse = response;
            Log(modeltexts);
            userDataResponse.data.game.tutorial.models = ParseNestedList(modeltexts);
            
            nextAction?.Invoke(userDataResponse);
        }
    }

    public IEnumerator GetHistoryDataIE(Action<HistoryResponse> nextAction = null)
    {
        UnityWebRequest request = UnityWebRequest.Get(GetHistoryTransactionAPI());
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Log("Error: " + request.error);
        }
        else
        {
            string responseJson = request.downloadHandler.text;
            Log(responseJson);
            try
            {
                HistoryResponse response = JsonUtility.FromJson<HistoryResponse>(responseJson);
                nextAction?.Invoke(response);
            }
            catch (Exception ex)
            {
                Log("Failed to parse JSON: " + ex.Message);
                nextAction?.Invoke(null);
            }
        }
    }

    public IEnumerator SendBetRequest(float totalAmount, Action<BetResponse> nextAction = null)
    {
        DataToSend data = new()
        {
            data = JsonUtility.ToJson(new BetDataToSend
            {
                total_amount = totalAmount
            })
        };

        string jsonData = JsonUtility.ToJson(data);
        Log(jsonData);

#if UNITY_WEBGL && !UNITY_EDITOR
        jsonData = JsonUtility.ToJson(new BetDataToSend
        {
            total_amount = totalAmount
        });
        string jsCommand1 = $"encryptDataAndSendBack('{jsonData}', '{encryptionKey}', '{gameObject.name}', 'ReceiveEncryptedData')";
        Application.ExternalCall("eval", jsCommand1);
#else
        var encryptRequest = new UnityWebRequest(GetEncryptDataAPI(), "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        encryptRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        encryptRequest.downloadHandler = new DownloadHandlerBuffer();
        encryptRequest.SetRequestHeader("Content-Type", "application/json");

        yield return encryptRequest.SendWebRequest();

        if (encryptRequest.result == UnityWebRequest.Result.ConnectionError || encryptRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Log("Error: " + encryptRequest.error);
        }
        else
        {
            EncryptedResponse response = JsonUtility.FromJson<EncryptedResponse>(encryptRequest.downloadHandler.text);
            encryptedJson = $"{{\"data\":\"{response.encrypted_data}\"}}";
        }
#endif
        using UnityWebRequest betRequest = new(GetSendBetDataAPI(), "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(encryptedJson);
        betRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
        betRequest.downloadHandler = new DownloadHandlerBuffer();
        betRequest.SetRequestHeader("Content-Type", "application/json");
        yield return betRequest.SendWebRequest();

        if (betRequest.result == UnityWebRequest.Result.ConnectionError || betRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Log("Error: " + betRequest.error);
            string responseJson = betRequest.downloadHandler.text;
            BetResponse betResponse = JsonUtility.FromJson<BetResponse>(responseJson);
            nextAction?.Invoke(betResponse);

        }
        else
        {
            if (betRequest.result != UnityWebRequest.Result.Success)
            {
                string responseJson = betRequest.downloadHandler.text;
                BetResponse betResponse = JsonUtility.FromJson<BetResponse>(responseJson);
                Log("Error: " + betRequest.error);
                nextAction?.Invoke(betResponse);
            }
            else
            {
                string responseJson = betRequest.downloadHandler.text;
                BetResponse betResponse = JsonUtility.FromJson<BetResponse>(responseJson);
                Log(responseJson);

#if UNITY_WEBGL && !UNITY_EDITOR
                string jsCommand2 = $"decryptDataAndSendBack('{betResponse.data}', '{encryptionKey}', '{gameObject.name}', 'ReceiveDecryptedData')";
                Application.ExternalCall("eval", jsCommand2);
#else
                decrypted = betResponse.data;
#endif
                nextAction?.Invoke(betResponse);
            }
        }
    }

    public IEnumerator SendSoundSettingIE(Sounds soundSetting)
    {
        string jsonBody = "{";
        jsonBody += $"\"effect\":{soundSetting.effect.ToString().ToLower()},";
        jsonBody += $"\"music\":{soundSetting.music.ToString().ToLower()},";
        jsonBody += "\"language\":\"ID\"}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        UnityWebRequest request = new UnityWebRequest(GetSettingPropertiesAPI(), "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Log(request.error);
        }
        else
        {
            Log(request.downloadHandler.text);
        }
    }


    public void ReceiveEncryptedData(string encryptedData)
    {
        encryptedJson = $"{{\"data\":\"{encryptedData}\"}}";
    }

    public void ReceiveDecryptedData(string jsonString)
    {
        decrypted = JsonUtility.FromJson<BetData>(jsonString);
    }

    public void Log(string message)
    {
        if (showDebug)
            Debug.Log(message);
    }
}


[Serializable]
public class BetDataToSend
{
    public float total_amount;
}

[Serializable]
public class ButtonBet
{
    public string button;
    public string type;
    public float amount;
}

[Serializable]
public class DataToSend
{
    public string data;
}


[Serializable]
public class EncryptedResponse
{
    public string encrypted_data;
}

[Serializable]
public class EncryptedConfigResponse
{
    public string data;
}

[Serializable]
public class DecryptedConfigData
{
    public string env;
    public string url_dev;
    public string url_staging;
    public string url_release;
}

[System.Serializable]
public class UserDataResponse
{
    public bool status;
    public string message;
    public Data data;
}

[System.Serializable]
public class Data
{
    public Player player;
    public Game game;
}

[System.Serializable]
public class Player
{
    public string player_id;
    public string agent_id;
    public string player_name;
    public string player_balance;
    public string player_currency;
    public string player_language;
    public string player_session;
    public string player_last_active;
    public int[] last_bet;
}


[System.Serializable]
public class Game
{
    public string lobby_url;
    public LimitBet limit_bet;
    public long[] chip_base;
    public string[] running_text;
    public Sounds sounds;
    public Tutorial tutorial;
}


[System.Serializable]
public class LimitBet
{
    public float minimal;
    public float maximal;
    public float minimal_50;
    public float maximal_50;
    public float multiplication;
    public float multiplication_50;
}

[System.Serializable]
public class Sounds
{
    public bool effect;
    public bool music;
}

[Serializable]
public class Tutorial
{
    public string rtp;
    public List<List<string>> models;
}



[Serializable]
public class BetResponse
{
    public bool status;
    public string message;
    public BetData data;
    public string type;
}

[Serializable]
public class BetData
{
    public int round_id;
    public GameResult game_result;
    public float total_win;
    public float total_amount;
    public string balance;
}


[Serializable]
public class GameResult
{
    public GameResultDetail result;
}


[System.Serializable]
public class GameResultDetail
{
    public float[] multipliers;
    public List<JackpotMultiplier> jackpot_multiplier;
    public List<GameResultEntry> game_results;
    public float total_win;
    public float total_amount;
}

[Serializable]
public class JackpotMultiplier
{
    public int box;
    public float multiplier;
}

[Serializable]
public class GameResultEntry
{
    public int box;
    public float multiplier;
    public float amount;
}


[System.Serializable]
public class HistoryResponse
{
    public bool status;
    public string message;
    public List<HistoryData> data;
}

[System.Serializable]
public class HistoryData
{
    public int round_id;
    public GameResultDetail result;
    public string player_id;
    public string agent_id;
    public string created_date;
    public History data;
}

[System.Serializable]
public class History
{
    public List<HistoryDetailBet> detail_bet;
    public float total_amount;
    public float total_win;
    public string last_balance;
}


[System.Serializable]
public class HistoryDetailBet
{
    public string balls;
    public string type;
    public float amount;
}