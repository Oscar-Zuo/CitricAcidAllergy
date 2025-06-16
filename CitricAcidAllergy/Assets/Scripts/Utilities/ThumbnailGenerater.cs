using UnityEngine;

public class ThumbnailGenerater
{
    public static Texture2D GenerateThumbnail((Vector2Int position, Sprite sprite)[] spriteData, int spriteSize = 128)
    {
        if (spriteData == null || spriteData.Length == 0)
        {
            Debug.LogError("Sprite data is null or empty.");
            return null;
        }

        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (var (position, _) in spriteData)
        {
            if (position.x < minX) minX = position.x;
            if (position.y < minY) minY = position.y;
            if (position.x > maxX) maxX = position.x;
            if (position.y > maxY) maxY = position.y;
        }

        int gridWidth = maxX - minX + 1;
        int gridHeight = maxY - minY + 1;

        int gridSize = Mathf.Max(gridWidth, gridHeight);
        int textureSize = gridSize * spriteSize;

        int xOffset = (textureSize - (gridWidth * spriteSize)) / 2;
        int yOffset = (textureSize - (gridHeight * spriteSize)) / 2;


        Texture2D thumbnail = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

        foreach (var (position, sprite) in spriteData)
        {
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite at position {position} is null. Skipping.");
                continue;
            }

            Texture2D spriteTexture = sprite.texture;
            Rect rect = sprite.textureRect;

            Color[] pixels = spriteTexture.GetPixels(
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height
            );

            Texture2D tempTexture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            tempTexture.SetPixels(pixels);
            tempTexture.Apply();

            Texture2D resizedTexture = ResizeTexture(tempTexture, spriteSize, spriteSize);

            int xPos = xOffset + (position.x - minX) * spriteSize;
            int yPos = yOffset + (position.y - minY) * spriteSize;

            Color[] resizedPixels = resizedTexture.GetPixels();
            thumbnail.SetPixels(xPos, yPos, spriteSize, spriteSize, resizedPixels);
        }

        thumbnail.Apply();
        return thumbnail;
    }

    public static Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, false);
        float incX = 1.0f / targetWidth;
        float incY = 1.0f / targetHeight;

        for (int i = 0; i < targetHeight; ++i)
        {
            for (int j = 0; j < targetWidth; ++j)
            {
                Color newColor = source.GetPixelBilinear(j * incX, i * incY);
                result.SetPixel(j, i, newColor);
            }
        }

        result.Apply();
        return result;
    }
}
