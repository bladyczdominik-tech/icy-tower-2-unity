using UnityEngine;

public static class ProceduralSpriteFactory
{
    public static Sprite CreatePlatformSprite(float width, float height, Color fill, Color edge)
    {
        int texW = 64;
        int texH = 32;
        var tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < texH; y++)
        for (int x = 0; x < texW; x++)
        {
            var color = new Color(0f, 0f, 0f, 0f);
            bool inside = x > 4 && x < texW - 5 && y > 4 && y < texH - 5;
            if (inside)
            {
                color = fill;
            }
            else
            {
                color = edge;
            }

            if (y > 6 && y < texH - 7 && x > 7 && x < texW - 8)
            {
                color = fill;
            }

            tex.SetPixel(x, y, color);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), 32f);
    }

    public static Sprite CreatePlayerSprite()
    {
        int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color skin = new Color(1f, 0.74f, 0.45f);
        Color blue = new Color(0.13f, 0.39f, 0.95f);
        Color green = new Color(0.2f, 0.75f, 0.35f);
        Color red = new Color(0.9f, 0.36f, 0.2f);
        Color dark = new Color(0.13f, 0.11f, 0.25f);
        Color white = Color.white;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            tex.SetPixel(x, y, new Color(0, 0, 0, 0));
        }

        for (int y = 10; y < 28; y++)
        for (int x = 7; x < 25; x++)
        {
            tex.SetPixel(x, y, blue);
        }

        for (int y = 20; y < 29; y++)
        for (int x = 7; x < 25; x++)
        {
            tex.SetPixel(x, y, green);
        }

        for (int i = 0; i < 6; i++)
        {
            tex.SetPixel(7 + i, 24, white);
        }

        for (int y = 7; y < 16; y++)
        for (int x = 10; x < 22; x++)
        {
            tex.SetPixel(x, y, skin);
        }

        for (int y = 16; y < 21; y++)
        for (int x = 8; x < 24; x++)
        {
            tex.SetPixel(x, y, red);
        }

        for (int x = 11; x < 15; x++)
            tex.SetPixel(x, 12, dark);

        for (int x = 17; x < 21; x++)
            tex.SetPixel(x, 12, dark);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    public static Sprite CreateSkySprite()
    {
        int width = 128;
        int height = 128;
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            float t = (float)x / width;
            Color c = new Color(0.5f + t * 0.2f, 0.7f + t * 0.2f, 0.96f, 1f);
            tex.SetPixel(x, y, c);
        }

        for (int i = 0; i < 18; i++)
        {
            int cx = Random.Range(10, width - 10);
            int cy = Random.Range(10, height - 10);
            int r = Random.Range(10, 28);
            for (int y = cy - r; y <= cy + r; y++)
            for (int x = cx - r; x <= cx + r; x++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist < r)
                    {
                        var c = tex.GetPixel(x, y);
                        tex.SetPixel(x, y, new Color(c.r, c.g, c.b, 0.18f));
                    }
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16f);
    }
}
