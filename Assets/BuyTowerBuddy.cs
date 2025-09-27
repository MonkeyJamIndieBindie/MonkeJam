using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyTowerBuddy : MonoBehaviour
{
    [SerializeField] GameObject towerBuddy;
    [SerializeField] float buddyCost;
    GameManager gameManager;
    [SerializeField] Transform nextKale;
    private void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
    }

    public void BuyBuddy()
    {
        if(gameManager.money >= buddyCost)
        {
            GameObject yeniKale = Instantiate(towerBuddy, nextKale.position, Quaternion.identity) as GameObject;
            if (nextKale.position.y == -2.4) yeniKale.GetComponent<SpriteRenderer>().sortingOrder = -2;
            if (nextKale.position.y == .2) yeniKale.GetComponent<SpriteRenderer>().sortingOrder = -3;
            if (nextKale.position.y == 2.8) yeniKale.GetComponent<SpriteRenderer>().sortingOrder = -4;
            if (nextKale.position.y == 5.4) yeniKale.GetComponent<SpriteRenderer>().sortingOrder = -5;
            gameManager.buddyShooting.Add(yeniKale.GetComponentInChildren<Shooting>());
            nextKale.position = new Vector2(nextKale.position.x, nextKale.position.y + 2.6f);
            gameManager.money -= buddyCost;
            gameManager.UpdateMoney();
            this.gameObject.SetActive(false);
        }
    }
}
