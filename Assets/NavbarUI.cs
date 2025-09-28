using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NavbarUI : MonoBehaviour
{
    [System.Serializable]
    public struct CharacterEntry
    {
        public string id;        // "IronMouse", "Chris" ...
        public Sprite portrait;  // �st s�ra portre
        public Sprite skill;     // Alt s�ra skill
    }

    public static NavbarUI Instance;

    [Header("Slots (Top = Portraits, Bottom = Skills)")]
    [SerializeField] Image[] portraitSlots;
    [SerializeField] Image[] skillSlots;

    [Header("Characters")]
    [SerializeField] CharacterEntry[] characters;

    [Header("Optional empty marker (kullanmazsan bo� b�rak)")]
    [SerializeField] Sprite emptySprite;

    Dictionary<string, CharacterEntry> map;

    void Awake()
    {
        Instance = this;

        // Karakter haritas�
        map = new Dictionary<string, CharacterEntry>(characters.Length);
        for (int i = 0; i < characters.Length; i++)
            if (!map.ContainsKey(characters[i].id))
                map.Add(characters[i].id, characters[i]);

        // Ba�lang��ta t�m slotlar� gizle
        ResetNavbar();
    }

    // Oyun ba�� / yeni dalga i�in hepsini temizle ve gizle
    public void ResetNavbar()
    {
        HideAll(portraitSlots);
        HideAll(skillSlots);
    }

    public void AddCharacterById(string id)
    {
        if (!map.TryGetValue(id, out var def)) return;
        AddCharacter(def.portrait, def.skill);
    }

    public void AddCharacter(Sprite portrait, Sprite skill)
    {
        int slot = FirstEmptyIndex(portraitSlots);
        if (slot < 0 || slot >= portraitSlots.Length || slot >= skillSlots.Length) return;

        // Portre
        var p = portraitSlots[slot];
        if (p != null)
        {
            p.sprite = portrait;
            p.gameObject.SetActive(true); // sadece doldu�unda a�
            if (emptySprite != null && p.sprite == emptySprite) p.sprite = portrait; // g�venlik
        }

        // Skill
        var s = skillSlots[slot];
        if (s != null)
        {
            s.sprite = skill;
            s.gameObject.SetActive(true);
            if (emptySprite != null && s.sprite == emptySprite) s.sprite = skill;
        }
    }

    // ========== Helpers ==========
    int FirstEmptyIndex(Image[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var img = slots[i];
            if (img == null) continue;

            // Kapal�ysa bo� kabul et
            if (!img.gameObject.activeSelf) return i;

            // Sprite yoksa bo� kabul et
            if (img.sprite == null) return i;

            // Bo� i�aret�isi kullan�yorsan ona e�itse bo� kabul et
            if (emptySprite != null && img.sprite == emptySprite) return i;
        }
        return -1;
    }

    void HideAll(Image[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            // tamamen gizle (beyaz kare g�r�nmesin)
            slots[i].sprite = null;              // g�venlik i�in sprite'� da s�f�rla
            slots[i].gameObject.SetActive(false);
        }
    }
}
