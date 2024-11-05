using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;
using Unity.Services.CloudSave.Models.Data.Player;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;
using Newtonsoft.Json;
using System;

public class CloudManager
{
    private static CloudManager instance = null;
    public static CloudManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CloudManager();
            }

            return instance;
        }
    }

    #region PlayerData

    public async Task SavePlayerData<T>(string key, T value)
    {
        var data = new Dictionary<string, object>
        {
            { key, value }
        };

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(data, new SaveOptions(new PublicWriteAccessClassOptions()));
            Debug.Log("Player data saved successfully.");
        }
        catch (CloudSaveException ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task<T> LoadPlayerData<T>(string key)
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key }, new LoadOptions(new PublicReadAccessClassOptions()));
           
            if(savedData.TryGetValue(key, out var item))
            {
                Debug.Log($"Player data loaded: {key} = {savedData[key]}");
                return item.Value.GetAs<T>();
            }
                            
            //if (savedData.ContainsKey(key))
            //{
            //    string jsonData = savedData[key].ToString();

            //    //T data = JsonUtility.FromJson<T>(jsonData);
            //    //T data = JsonConvert.DeserializeObject<T>(jsonData);
            //    return JsonUtility.FromJson<string>(jsonData);
            //}
            //else
            //{
            //    Debug.Log("No data found for key: " + key);
            //}
        }
        catch (CloudSaveException ex)
        {
            Debug.LogException(ex);
        }

        return default;
    }

    public async void DeletePlayerData(string key)
    {
        try
        {
            await CloudSaveService.Instance.Data.Player.DeleteAsync(key, new Unity.Services.CloudSave.Models.Data.Player.DeleteOptions(new PublicWriteAccessClassOptions()));
            Debug.Log("Player data deleted successfully.");
        }
        catch (CloudSaveException ex)
        {
            Debug.LogException(ex);
        }
    }

    #endregion
}