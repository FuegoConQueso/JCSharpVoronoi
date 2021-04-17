using System;
using System.Collections.Generic;
using System.Drawing;

namespace JCSharpVoronoi
{
    public class Voronoi
    {
        //development only constants
        const float R_HEIGHT = 100;
        const float R_WIDTH = 100;
        public static JCVDiagram JCVDiagramGenerate(List<PointF> points, float width, float height)
        {
            RectangleF rect = new RectangleF(0, 0, width, height);

            JCVDiagram diagram = new JCVDiagram(ref points, rect);



            return diagram;
        }

    }
}
