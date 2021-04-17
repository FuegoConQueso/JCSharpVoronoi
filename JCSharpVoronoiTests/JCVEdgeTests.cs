using Microsoft.VisualStudio.TestTools.UnitTesting;
using JCSharpVoronoi;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace JCSharpVoronoi.Tests
{
    [TestClass()]
    public class JCVEdgeTests
    {
        [TestMethod()]
        public void lineEqTest()
        {
            PointF p1 = new PointF(20, 80);
            PointF p2 = new PointF(25, 25);

            float[] lineEq = JCVEdge.lineEq(p1, p2);

            Assert.Fail();
        }
    }
}