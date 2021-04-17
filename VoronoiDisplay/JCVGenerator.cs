using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace VoronoiDisplay
{
    class JCVGenerator
    {

        public static List<PointF> GeneratePoints(int amount, float maxX, float maxY, int seedNumber = 0)
        {
            float MaxX = maxX;
            float MaxY = maxY;

            //List<PointF> superBox = new List<PointF>
            //{
            //    new PointF(-MaxX, 0),
            //    new PointF(-MaxX, MaxY),
            //    new PointF(2 * MaxX, MaxY),
            //    new PointF(2 * MaxX, 0)
            //};

            //var PointFs = new List<PointF>(superBox);

            var randPointFs = new List<PointF>();

            Random random;
            if (seedNumber == 0)
                random = new Random();
            else
                random = new Random(seedNumber);

            for (int i = 0; i < amount; i++)
            {
                var PointFX = (float)random.NextDouble() * MaxX;
                var PointFY = (float)random.NextDouble() * MaxY;
                randPointFs.Add(new PointF(PointFX, PointFY));
            }

            //duplicate PointFs to left and right
            //var duplicatePointFs = new List<PointF>();
            //foreach (PointF PointF in randPointFs)
            //{
            //    duplicatePointFs.Add(new PointF(PointF.X - MaxX, PointF.Y));
            //    duplicatePointFs.Add(new PointF(PointF.X + MaxX, PointF.Y));
            //}


            //PointFs.AddRange(randPointFs);
            //PointFs.AddRange(duplicatePointFs);

            return randPointFs;
        }
    }
}
