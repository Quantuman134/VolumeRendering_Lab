using UnityEngine;
using System.Collections.Generic;
using System;

namespace UnityVolumeRendering
{
    [Serializable]
    public class TransferFunction : ScriptableObject
    {
        [SerializeField]
        public List<TFColourControlPoint> colourControlPoints = new List<TFColourControlPoint>();
        [SerializeField]
        public List<TFAlphaControlPoint> alphaControlPoints = new List<TFAlphaControlPoint>();

        //2*512texture
        private Texture2D texture = null;
        //与texture等大小的颜色数组
        private Color[] tfCols;

        private const int TEXTURE_WIDTH = 512;
        private const int TEXTURE_HEIGHT = 2;

        //返回纹理尺寸
        static public int[] GetTextureSize()
        {
            int[] temp = { TEXTURE_HEIGHT, TEXTURE_WIDTH};
            return temp;
        }
        //返回纹理对应的数组
        public Color[] GetColors()
        {
            return tfCols;
        }

        public void AddControlPoint(TFColourControlPoint ctrlPoint)
        {
            colourControlPoints.Add(ctrlPoint);
        }

        public void AddControlPoint(TFAlphaControlPoint ctrlPoint)
        {
            alphaControlPoints.Add(ctrlPoint);
        }

        public Texture2D GetTexture()
        {
            if (texture == null)
                GenerateTexture();

            return texture;
        }

        public void GenerateTexture()
        {
            if (texture == null)
                CreateTexture();

            List<TFColourControlPoint> cols = new List<TFColourControlPoint>(colourControlPoints);
            List<TFAlphaControlPoint> alphas = new List<TFAlphaControlPoint>(alphaControlPoints);

            // Sort lists of control points
            cols.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));
            alphas.Sort((a, b) => (a.dataValue.CompareTo(b.dataValue)));

            // Add colour points at beginning and end
            if (cols.Count == 0 || cols[cols.Count - 1].dataValue < 1.0f)
                cols.Add(new TFColourControlPoint(1.0f, Color.white));
            if (cols[0].dataValue > 0.0f)
                cols.Insert(0, new TFColourControlPoint(0.0f, Color.white));

            // Add alpha points at beginning and end
            if (alphas.Count == 0 || alphas[alphas.Count - 1].dataValue < 1.0f)
                alphas.Add(new TFAlphaControlPoint(1.0f, 1.0f));
            if (alphas[0].dataValue > 0.0f)
                alphas.Insert(0, new TFAlphaControlPoint(0.0f, 0.0f));

            int numColours = cols.Count;
            int numAlphas = alphas.Count;
            //当前纹理点向左遇到的第一个控制点
            int iCurrColour = 0;
            int iCurrAlpha = 0;

            for (int iX = 0; iX < TEXTURE_WIDTH; iX++)
            {
                float t = iX / (float)(TEXTURE_WIDTH - 1);//归一化到control point的datavalue
                while (iCurrColour < numColours - 2 && cols[iCurrColour + 1].dataValue < t)
                    iCurrColour++;
                while (iCurrAlpha < numAlphas - 2 && alphas[iCurrAlpha + 1].dataValue < t)
                    iCurrAlpha++;

                //以当前纹理点为参考
                TFColourControlPoint leftCol = cols[iCurrColour];
                TFColourControlPoint rightCol = cols[iCurrColour + 1];
                TFAlphaControlPoint leftAlpha = alphas[iCurrAlpha];
                TFAlphaControlPoint rightAlpha = alphas[iCurrAlpha + 1];

                //当前左右控制点为参考的归一化值t
                float tCol = (Mathf.Clamp(t, leftCol.dataValue, rightCol.dataValue) - leftCol.dataValue) / (rightCol.dataValue - leftCol.dataValue);
                float tAlpha = (Mathf.Clamp(t, leftAlpha.dataValue, rightAlpha.dataValue) - leftAlpha.dataValue) / (rightAlpha.dataValue - leftAlpha.dataValue);

                //控制点的线性插值
                Color pixCol = rightCol.colourValue * tCol + leftCol.colourValue * (1.0f - tCol);
                pixCol.a = rightAlpha.alphaValue * tAlpha + leftAlpha.alphaValue * (1.0f - tAlpha);

                for (int iY = 0; iY < TEXTURE_HEIGHT; iY++)
                {
                    tfCols[iX + iY * TEXTURE_WIDTH] = pixCol;
                }
            }

            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(tfCols);
            texture.Apply();
        }

        private void CreateTexture()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, texformat, false);
            tfCols = new Color[TEXTURE_WIDTH * TEXTURE_HEIGHT];
        }
    }
}
