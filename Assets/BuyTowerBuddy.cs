using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyTowerBuddy : MonoBehaviour
{
    [SerializeField] GameObject towerBuddy;
    [SerializeField] float buddyCost;
    GameManager gameManager;
    private void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
    }

    public void BuyBuddy()
    {
        if(gameManager.money >= buddyCost)
        {
            towerBuddy.SetActive(true);
            gameManager.money -= buddyCost;
            gameManager.UpdateMoney();
            this.gameObject.SetActive(false);
        }
    }
}
