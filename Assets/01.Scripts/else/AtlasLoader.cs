using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEditor;
using Util;

public class AtlasLoader : MonoBehaviour
{
    [SerializeField] private string _spriteName;

    private void Awake()
    {
        LoadSprite();
    }

    private string GetSpriteName()
    {
        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sp))
        {
            if (!string.IsNullOrEmpty(sp.sprite.name))
                return sp.sprite.name;
        }
        else if (TryGetComponent<Image>(out Image image))
        {
            if (!string.IsNullOrEmpty(image.sprite.name))
                return image.sprite.name;
        }

        return string.Empty;
    }

    [ContextMenu("LoadSprite")]
    private void LoadSprite()
    {
        SpriteAtlas atlas = Resources.Load<SpriteAtlas>("Ko/Atlas_UI");

        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sp))
        {
            sp.sprite = atlas.GetSprite(_spriteName);
        }
        else if (TryGetComponent<Image>(out Image image))
        {
            image.sprite = atlas.GetSprite(_spriteName);
        }
    }

    [ContextMenu("UnLoadSprite")]
    private void UnLoadSprite()
    {
        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sp))
        {
            sp.sprite = null;
        }
        else if (TryGetComponent<Image>(out Image image))
        {
            image.sprite = null;
        }
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_spriteName))
        {
            _spriteName = GetSpriteName();
            _spriteName = _spriteName.Replace("(Clone)", "");
            LoadSprite();
        }
        else
        {
            LoadSprite();
        }
    }

    private void Reset()
    {
        string spriteName = GetSpriteName();

        if (string.IsNullOrEmpty(_spriteName))
        {
            _spriteName = spriteName;
        }
        LoadSprite();
    }

    [MenuItem("Atlas/UnLoadSprites %&u")]
    public static void UnLoadSprites()
    {
        List<AtlasLoader> _atlas = Util.Util.FindAllObjects<AtlasLoader>();

        foreach (AtlasLoader item in _atlas)
        {
            item.UnLoadSprite();
        }

        Debug.Log($"{_atlas.Count} sprites were unloaded");
    }

    [MenuItem("Atlas/LoadSprites %&l")]
    public static void LoadSprites()
    {
        List<AtlasLoader> _atlas = Util.Util.FindAllObjects<AtlasLoader>();

        foreach (AtlasLoader item in _atlas)
        {
            item.LoadSprite();
        }

        Debug.Log($"{_atlas.Count} sprites were loaded");
    }
}
