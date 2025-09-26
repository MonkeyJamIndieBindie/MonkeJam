using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject[] towerBuddies;

    public void OpenIronMouse()
    {
        towerBuddies[0].SetActive(true);
    }
    public void OpenChris()
    {
        towerBuddies[1].SetActive(true);
    }
    public void OpenGarnt()
    {
        towerBuddies[2].SetActive(true);
    }
    public void OpenJoey()
    {
        towerBuddies[3].SetActive(true);
    }
}
