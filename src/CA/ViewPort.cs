using System.Drawing;

namespace CA
{
    public class ViewPort
    {
        public int XMin { get; private set; }
        public int XMax { get; private set; }
        public int YMin { get; private set; }
        public int YMax { get; private set; }

        public int Width
        {
            get { return XMax - XMin; }
        }

        public int Height
        {
            get { return YMax - YMin; }
        }

        public ViewPort(int xMin, int yMin, int xMax, int yMax)
        {
            XMin = xMin;
            YMin = yMin;
            XMax = xMax;
            YMax = yMax;
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle(XMin, YMin, Width, YMax - YMin);
        }

        public void Translate(int xTranslation, int yTranslation)
        {
            XMin += xTranslation;
            XMax += xTranslation;
            YMin += yTranslation;
            YMax += yTranslation;
        }

        public void Grow(int xSize, int ySize)
        {
            var xDim = (1 + XMax - XMin) * xSize - 1;
            XMin *= xSize;
            XMax = XMin + xDim;
            var yDim = YMax = (1 + YMax - YMin) * ySize - 1;
            YMin *= ySize;
            YMax = YMin + yDim;
        }

        public override string ToString()
        {
            return $"({XMin}, {YMin}) -> ({XMax}, {YMax})";
        }
    }
}