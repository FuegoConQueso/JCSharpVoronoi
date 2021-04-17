using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using JCSharpVoronoi;
using System;
using System.Collections.Generic;
using System.Text;

namespace JCSharpVoronoi.Tests
{
    [TestClass()]
    public class VoronoiTests
    {
        [TestMethod()]
        public void JCVDiagramGenerateTest()
        {
            JCVDiagram d = Voronoi.JCVDiagramGenerate();
            float[,] coords = new float[d.edges.Count, 4];
            for(int i = 0; i < d.edges.Count; i++)
            {
                coords[i, 0] = d.edges[i].Points[0].X;
                coords[i, 1] = d.edges[i].Points[0].Y;
                coords[i, 2] = d.edges[i].Points[1].X;
                coords[i, 3] = d.edges[i].Points[1].Y;
            }
            Assert.IsTrue(d.SiteCount == 4);
        }
    }
}