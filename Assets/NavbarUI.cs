using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NavbarUI : MonoBehaviour
{
    [System.Serializable]
    public struct CharacterEntry
    {
        public string id;        // "IronMouse", "Chris" ...
        public Sprite portrait;  // Üst sýra portre
        public Sprite skill;     // Alt sýra skill
    }

    public static NavbarUI Instance;

    [Header("Slots (Top = Portraits, Bottom = Skills)")]
    [SerializeField] Image[] portraitSlots;
    [SerializeField] Image[] skillSlots;

    [Header("Characters")]
    [SerializeField] CharacterEntry[] characters;

    [Header("Optional empty marker (kullanmazsan boþ býrak)")]
    [SerializeField] Sprite emptySprite;

    Dictionary<string, CharacterEntry> map;

    void Awake()
    {
        Instance = this;

        // Karakter haritasý
        map = new Dictionary<string, CharacterEntry>(characters.Length);
        for (int i = 0; i < characters.Length; i++)
            if (!map.ContainsKey(characters[i].id))
                map.Add(characters[i].id, characters[i]);

        // Baþlangýçta tüm slotlarý gizle
        ResetNavbar();
    }

    // Oyun baþý / yeni dalga için hepsini temizle ve gizle
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
            p.gameObject.SetActive(true); // sadece dolduðunda aç
            if (emptySprite != null && p.sprite == emptySprite) p.sprite = portrait; // güvenlik
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

            // Kapalýysa boþ kabul et
            if (!img.gameObject.activeSelf) return i;

            // Sprite yoksa boþ kabul et
            if (img.sprite == null) return i;

            // Boþ iþaretçisi kullanýyorsan ona eþitse boþ kabul et
            if (emptySprite != null && img.sprite == emptySprite) return i;
        }
        return -1;
    }

    void HideAll(Image[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            // tamamen gizle (beyaz kare görünmesin)
            slots[i].sprite = null;              // güvenlik için sprite'ý da sýfýrla
            slots[i].gameObject.SetActive(false);
        }
    }
}
