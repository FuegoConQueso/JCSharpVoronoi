using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace JCSharpVoronoi
{
    public abstract class JCVClipper
    {
        public RectangleF boundingBox;

        public JCVClipper(RectangleF rect)
        {
            boundingBox = rect;
        }

        public abstract bool TestPoint(PointF point);
        public abstract bool ClipEdge(JCVEdge edge);
        public abstract bool FillGaps(JCVSite site, JCVDiagram diagram);

    }
}
