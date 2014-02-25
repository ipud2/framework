﻿
using System.IO;
using System.Linq;
using Mercraft.Maps.Osm;
using Mercraft.Maps.Osm.Data;
using Mercraft.Maps.Osm.Visitors;
using Mercraft.Models;
using Mercraft.Models.Scene;
using NUnit.Framework;

namespace Mercraft.Maps.UnitTests.Osm
{
    [TestFixture]
    public class BuildingTests
    {
        [Test]
        public void CanProcessBuildings()
        {
            using (Stream stream = new FileInfo(TestHelper.TestXmlFilePath).OpenRead())
            {
                var dataSource = MemoryDataSource.CreateFromXmlStream(stream);

                var bbox = BoundingBox.CreateBoundingBox(new GeoCoordinate(52.529814, 13.388015), 200);

                var scene = new CountableScene();

                var elementManager = new ElementManager();

                elementManager.VisitBoundingBox(dataSource, bbox, new BuildingVisitor(scene));

                Assert.AreEqual(30, scene.Buildings.Count());

                DumpScene(scene);

            }
        }

        [Test]
        public void CanProcessLargerArea()
        {
            using (Stream stream = new FileInfo(TestHelper.TestPbfFilePath).OpenRead())
            {
                var dataSource = MemoryDataSource.CreateFromPbfStream(stream);

                var bbox = BoundingBox.CreateBoundingBox(new GeoCoordinate(51.26371, 4.7854), 1000);

                var scene = new CountableScene();

                var elementManager = new ElementManager();

                elementManager.VisitBoundingBox(dataSource, bbox, new BuildingVisitor(scene));

                Assert.AreEqual(1453, scene.Buildings.Count());
            }
        }

        private void DumpScene(CountableScene scene)
        {
            using (var file = File.CreateText(@"f:\scene.txt"))
            {
                file.WriteLine("BUILDINGS:");
                var buildings = scene.Buildings.ToList();
                for (int i = 0; i < buildings.Count; i++)
                {
                    var building = buildings[i];
                    file.WriteLine("\tBuilding {0}", (i+1));
                    var lineOffset = "\t\t";
                    file.WriteLine("{0}Tags:", lineOffset);
                    foreach (var tag in building.Tags)
                    {
                        file.WriteLine("{0}\t{1}:{2}",lineOffset, tag.Key, tag.Value);
                    }
                    file.WriteLine("{0}Points:", lineOffset);
                    foreach (var point in building.Points)
                    {
                        file.WriteLine("{0}\t{1},{2}", lineOffset, point.Latitude, point.Longitude);
                    }
                }
            }
        }
    }
}
