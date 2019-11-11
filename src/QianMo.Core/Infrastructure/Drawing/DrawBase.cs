using System;
using System.Drawing;

namespace QianMo.Core.Infrastructure.Drawing
{
    public class DrawBase
    {
        private readonly int _w;
        private readonly int _h;
        public float AutoFitScale;
        public float MinX;
        public float MinY;
        public Image Image { get; }

        public DrawBase(int w, int h)
        {
            _w = w;
            _h = h;
            Image = new Bitmap(w, h);
        }

        public DrawBase(Image original)
        {
            Image = original;
            _w = original.Width;
            _h = original.Height;
        }

        public DrawBase CloseAutoFit()
        {
            AutoFitScale = 1;
            return this;
        }

        public DrawBase OpenAutoFit(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            AutoFitScale = GetScaleOfBitmap(minX, minY, maxX, maxY);
            return this;
        }

        private float GetScaleOfBitmap(float minX, float minY, float maxX, float maxY)
        {
            var diffY = maxX - minX;
            var diffX = maxY - minY;
            var scaleY = _h / diffY;
            var scaleX = _w / diffX;
            var scale = Math.Min(scaleY, scaleX);
            return scale;
        }
    }
}