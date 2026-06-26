using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // Persistent stats
    public int highScore;
    public int totalObjectsEaten;
    public float totalPlayTime;

    // Game progress (snapshot)
    public float playerPosX, playerPosY, playerPosZ;
    public float playerSize;
    public float playerRadius;
    public float remainingTime;
    public float waterLevelY;
    public List<string> remainingEatableIds = new List<string>();

    public bool HasGameProgress()
    {
        // Valid save must have player position data
        return !(playerPosX == 0 && playerPosY == 0 && playerPosZ == 0);
    }
}
