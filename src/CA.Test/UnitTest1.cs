using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace CA.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var w = new World();

            var values = Enumerable.Range(0, 51)
                .Select(x => x - 25)
                .Select(x => $"({x},{x}) => " + 
                             $"({w.ConvertCellWorldCoordinateToChunkCoordinate(x, x).X}, " +
                             $"{w.ConvertCellWorldCoordinateToChunkCoordinate(x, x).Y})")
                .ToArray();
        }
    }
}
