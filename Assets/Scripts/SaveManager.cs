using UnityEngine;

public static class SaveManager
{
    private static string currentUsername = "";
    
    // set which user is currently playing
    public static void setCurrentUser(string username)
    {
        currentUsername = username;
        Debug.Log("Current user set to: " + username);
    }
    
    // get current user
    public static string getCurrentUser()
    {
        return currentUsername;
    }
    
    // check if a username exists (has save data)
    public static bool userExists(string username)
    {
        string checkKey = username + "_exists";
        return PlayerPrefs.HasKey(checkKey);
    }
    
    // create a new user
    public static void createNewUser(string username)
    {
        string existsKey = username + "_exists";
        PlayerPrefs.SetString(existsKey, "true");
        
        // initialize default values for new user
        saveInt("CurrentSanity", 100);
        //saveInt("CareCycles", 0);
        
        PlayerPrefs.Save();
        Debug.Log("New user created: " + username);
    }
    
    // save functions (automatically add username prefix)
    public static void saveInt(string key, int value)
    {
        string fullKey = currentUsername + "_" + key;
        PlayerPrefs.SetInt(fullKey, value);
        PlayerPrefs.Save();
    }
    
    public static int loadInt(string key, int defaultValue = 0)
    {
        string fullKey = currentUsername + "_" + key;
        return PlayerPrefs.GetInt(fullKey, defaultValue);
    }
    
    public static void saveString(string key, string value)
    {
        string fullKey = currentUsername + "_" + key;
        PlayerPrefs.SetString(fullKey, value);
        PlayerPrefs.Save();
    }
    
    public static string loadString(string key, string defaultValue = "")
    {
        string fullKey = currentUsername + "_" + key;
        return PlayerPrefs.GetString(fullKey, defaultValue);
    }
}