using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using System;

public class DatabaseManager
{
    private static DatabaseManager instance;

    public static DatabaseManager Instance
    {
        get
        {
            if (instance == null)
                instance = new DatabaseManager();

            return instance;
        }
    }

    private DatabaseReference databaseReference;

    DatabaseManager()
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public async Task<FirebaseResult> CreateUserDatabase(string userId, UserInfo userInfo)
    {
        FirebaseResult result = new FirebaseResult();
        //string json = JsonUtility.ToJson(userInfo);

        try
        {
            await databaseReference.Child("users").Child(userId).SetValueAsync(userInfo);
            result.State = FirebaseState.success;
            result.Message = $"{userId}'s data was created successfully!";
            Debug.Log($"{userId}'s data was created successfully!");
        }
        catch(FirebaseException ex)
        {
            Debug.LogException(ex);
            result.State = FirebaseState.Failed;
            result.Message = ex.Message;
            result.ErrorCode = ex.ErrorCode;
        }

        return result;
    }

    public async Task<FirebaseResult> UpdateUserData(string userId, Dictionary<string, object> updates)
    {
        FirebaseResult result = new FirebaseResult();

        try
        {
            await databaseReference.Child("users").Child(userId).UpdateChildrenAsync(updates);
            result.Message = $"{userId}'s data updated successfully!";
            result.State = FirebaseState.success ;
            Debug.Log($"{userId}'s data updated successfully!");
        }
        catch (FirebaseException ex)
        {
            Debug.LogException(ex);
            result.State = FirebaseState.Failed;
            result.Message = ex.Message;
            result.ErrorCode = ex.ErrorCode;
        }

        return result;
    }

    public async Task<FirebaseResult> GetUserDataBase(string userId)
    {
        FirebaseResult result = new FirebaseResult();

        try
        {
            DataSnapshot snapshot = await databaseReference.Child("users").Child(userId).GetValueAsync();

            if (snapshot.Exists)
            {
                UserInfo userData = JsonUtility.FromJson<UserInfo>(snapshot.GetRawJsonValue());
                result.Result = userData;
                result.State = FirebaseState.success;
                result.Message = $"{userId}'s Data was loaded successfully";
            }
        }
        catch (FirebaseException ex)
        {
            Debug.LogException(ex);
            result.State = FirebaseState.Failed;
            result.Message = ex.Message;
            result.ErrorCode = ex.ErrorCode;
        }

        return result;
    }
}
