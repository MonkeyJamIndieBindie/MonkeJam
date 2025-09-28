using UnityEngine;

public class BuyTowerBuddy : MonoBehaviour
{
    [SerializeField] GameObject towerBuddy;
    [SerializeField] float buddyCost;
    [SerializeField] Transform nextKale;
    [SerializeField] string characterId;

    GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void BuyBuddy()
    {
        if (gameManager.money < buddyCost) return;

        GameObject yeniKale = Instantiate(towerBuddy, nextKale.position, Quaternion.identity);
        float y = nextKale.position.y;

        if (Mathf.Approximately(y, -2.4f)) yeniKale.GetComponent<SpriteRenderer>().sortingOrder = -2;
        if (Mathf.Approximately(y, 0.2f)) yeniKale.GetComponent<SpriteRenderer>().sortingOrder = -3;
        if (Mathf.Approximately(y, 2.8f)) yeniKale.GetComponent<SpriteRenderer>().sortingOrder = -4;
        if (Mathf.Approximately(y, 5.4f)) yeniKale.GetComponent<SpriteRenderer>().sortingOrder = -5;

        gameManager.buddyShooting.Add(yeniKale.GetComponentInChildren<Shooting>());
        nextKale.position = new Vector2(nextKale.position.x, nextKale.position.y + 2.6f);
        gameManager.money -= buddyCost;
        gameManager.UpdateMoney();

        if (NavbarUI.Instance != null) NavbarUI.Instance.AddCharacterById(characterId);

        gameObject.SetActive(false);
    }
}
