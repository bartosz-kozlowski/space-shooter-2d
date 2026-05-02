using UnityEngine;
using UnityEngine.SceneManagement;

public class CharSelectManager : MonoBehaviour
{
    enum Ship{
        Blue = 0,
        Orange = 1
    }

    int currentShipIndex;
    static CharSelectManager instance;

    void Awake()
    {
        ManageSingleton();
    }

    void ManageSingleton()
    {
        if (instance != null)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public int GetCurrentShipIndex()
    {
        return currentShipIndex;
    }

    public void SetBluePlayer()
    {
        currentShipIndex = (int)Ship.Blue;
    }

    public void SetOrangePlayer()
    {
        currentShipIndex = (int)Ship.Orange;
    }

    public void ResetCurrentPlayer()
    {
        currentShipIndex = -1;
    }
}
