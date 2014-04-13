﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Mercraft.Models.Buildings.Entities;
using Mercraft.Models.Buildings.Utils;
using UnityEngine;
using Texture = Mercraft.Models.Buildings.Entities.Texture;

namespace Mercraft.Models.Buildings
{
    public class BuildingGenerator
    {
        private static char[] filenameDelimiters = new[] {'\\', '/'};
        
        private static Texture wallTexture;
        private static Texture windowTexture;
        private static Texture doorTexture;
        private static Texture roofTexture;

        public static void Generate(Data data)
        {

            //GenerateConstraints constraints = data.GeneratorConstraints;

            //uint seed = (uint) (constraints.useSeed ? constraints.seed : Random.Range(0, int.MaxValue));
            //constraints.seed = (int) seed; //reassign value incase it's changed
            //constraints.rgen = new RandomGen(seed);
            //RandomGen rgen = constraints.rgen;

            GenerateFloorPlan(data);

            data.FloorHeight = 5;
            foreach (Volume volume in data.Plan.volumes)
            {
                volume.height = 5*5;
                volume.numberOfFloors = 5;
            }
            /*data.FloorHeight = rgen.OutputRange(constraints.minimumFloorHeight, constraints.maximumFloorHeight);
            float minBuildingSize = (constraints.constrainHeight)
                ? constraints.minimumHeight
                : GenerateConstraints.MINIMUM_BUILDING_HEIGHT;
            float maxBuildingSize = (constraints.constrainHeight)
                ? constraints.maximumHeight
                : GenerateConstraints.MAXIMUM_BUILDING_HEIGHT;
            foreach (Volume volume in data.Plan.volumes)
            {
                volume.height = rgen.OutputRange(minBuildingSize, maxBuildingSize);
                volume.numberOfFloors = Mathf.FloorToInt(volume.height/data.FloorHeight);
            }*/

            //texture generation
            GetTextures(data);

            //facade generation
            GenerateFacades(data);

            //roof generation
            GenerateRoof(data);

            // TODO generate details
        }

        private static void GenerateRoof(Data data)
        {
            GenerateConstraints constraints = data.GeneratorConstraints;
            RandomGen rgen = constraints.rgen;
            RoofDesign roofDesign = new RoofDesign("default");

            List<int> availableRoofStyles = new List<int>();
            if (constraints.roofStyleFlat) availableRoofStyles.Add(0);
            if (constraints.roofStyleMansard) availableRoofStyles.Add(1);
            if (constraints.roofStyleBarrel) availableRoofStyles.Add(2);
            if (constraints.roofStyleGabled) availableRoofStyles.Add(3);
            if (constraints.roofStyleHipped) availableRoofStyles.Add(4);
            if (constraints.roofStyleLeanto) availableRoofStyles.Add(5);
            if (constraints.roofStyleSteepled) availableRoofStyles.Add(6);
            if (constraints.roofStyleSawtooth) availableRoofStyles.Add(7);

            System.Array A = System.Enum.GetValues(typeof (RoofDesign.styles));
            roofDesign.style =
                (RoofDesign.styles)
                    A.GetValue(availableRoofStyles[rgen.OutputRange(0, availableRoofStyles.Count - 1)]);
            roofDesign.height = rgen.OutputRange(constraints.minimumRoofHeight, constraints.maximumRoofHeight);
            roofDesign.floorDepth = rgen.OutputRange(constraints.minimumRoofFloorDepth,
                constraints.maximumRoofFloorDepth);
            roofDesign.depth = rgen.OutputRange(Mathf.Min(constraints.minimumRoofDepth, roofDesign.floorDepth),
                constraints.maximumRoofDepth);

            roofDesign.hasDormers = (constraints.allowDormers) && (rgen.output <= constraints.dormerChance);
            roofDesign.dormerWidth = rgen.OutputRange(constraints.dormerMinimumWidth, constraints.dormerMaximumWidth);
            roofDesign.dormerHeight = rgen.OutputRange(constraints.dormerMinimumHeight,
                Mathf.Min(roofDesign.height, constraints.dormerMaximumHeight));
            roofDesign.dormerRoofHeight = rgen.OutputRange(constraints.dormerMinimumRoofHeight,
                constraints.dormerMaximumRoofHeight);
            roofDesign.minimumDormerSpacing = rgen.OutputRange(constraints.dormerMinimumSpacing,
                constraints.dormerMaximumSpacing);
            roofDesign.dormerHeightRatio = rgen.OutputRange(0.0f, 1.0f);

            roofDesign.parapet = (constraints.allowParapet) && (rgen.output <= constraints.parapetChance);
            roofDesign.parapetDesignWidth = rgen.OutputRange(constraints.parapetMinimumDesignWidth,
                constraints.parapetMaximumDesignWidth);
            roofDesign.parapetHeight = rgen.OutputRange(constraints.parapetMinimumHeight,
                constraints.parapetMaximumHeight);
            roofDesign.parapetFrontDepth = rgen.OutputRange(constraints.parapetMinimumFrontDepth,
                constraints.parapetMaximumFrontDepth);
            roofDesign.parapetBackDepth = rgen.OutputRange(constraints.parapetMinimumBackDepth,
                constraints.parapetMaximumBackDepth);

            if (roofDesign.style == RoofDesign.styles.sawtooth)
            {
                //make a new window texture for the sawtooth
            }

            data.Roofs.Add(roofDesign);
        }

        private static void GenerateFacades(Data data)
        {
            GenerateConstraints constraints = data.GeneratorConstraints;
            RandomGen rgen = constraints.rgen;

            //generate bays
            //blank
            Bay blankBay = new Bay("Blank");
            blankBay.isOpening = false;
            data.Bays.Add(blankBay);
            //door
            Bay doorBay = new Bay("Door");
            doorBay.openingHeight = data.FloorHeight*0.9f;
            doorBay.openingHeightRatio = 0.0f;
            float doorWidth = (doorTexture.texture.width/(float) doorTexture.texture.height)*doorBay.openingHeight;
            doorBay.openingWidth = doorWidth;
            doorBay.openingDepth = rgen.OutputRange(0.0f, 0.3f);
            doorBay.SetTexture(Bay.TextureNames.OpeningBackTexture, data.Textures.IndexOf(doorTexture));
            data.Bays.Add(doorBay);
            //ground window
            Bay groundWindow = new Bay("Ground Window");
            groundWindow.openingWidth = rgen.OutputRange(constraints.openingMinimumWidth,
                constraints.openingMaximumWidth);
            groundWindow.openingHeight = rgen.OutputRange(constraints.openingMinimumHeight,
                Mathf.Min(data.FloorHeight, constraints.openingMinimumHeight));
            groundWindow.openingDepth = rgen.OutputRange(constraints.openingMinimumDepth,
                constraints.openingMaximumDepth);
            groundWindow.openingHeightRatio = 0.8f;
            data.Bays.Add(groundWindow);

            Texture groundFloorWindowTexture = windowTexture.Duplicate("groundWindowTexture");
            groundFloorWindowTexture.tiled = false;
            groundFloorWindowTexture.tiledX = Mathf.RoundToInt(groundWindow.openingWidth/groundWindow.openingHeight);
            int groundtextureIndex = data.Textures.Count;
            data.Textures.Add(groundFloorWindowTexture);
            groundWindow.SetTexture(Bay.TextureNames.OpeningBackTexture, groundtextureIndex);
            //other windows
            Bay windowBay = new Bay("Window");
            data.Bays.Add(windowBay);
            //util window
            Bay utilBay = new Bay("Utility Window");
            data.Bays.Add(utilBay);

            //generate facades
            FacadeDesign basicFacadeDesign = new FacadeDesign("default");
            basicFacadeDesign.simpleBay.openingWidth = rgen.OutputRange(constraints.openingMinimumWidth,
                constraints.openingMaximumWidth);
            basicFacadeDesign.simpleBay.openingHeight = rgen.OutputRange(constraints.openingMinimumHeight,
                Mathf.Min(data.FloorHeight, constraints.openingMinimumHeight));
            basicFacadeDesign.simpleBay.openingDepth = rgen.OutputRange(constraints.openingMinimumDepth,
                constraints.openingMaximumDepth);
            
            basicFacadeDesign.simpleBay.minimumBayWidth = rgen.OutputRange(constraints.minimumBayMaximumWidth,
                constraints.minimumBayMaximumWidth);
            data.Facades.Add(basicFacadeDesign);
            //ground floor with and without door
            FacadeDesign groundFloorDoor = new FacadeDesign("Ground Floor With Door");
            groundFloorDoor.type = FacadeDesign.types.patterned;
            int patternSize = rgen.OutputRange(1, 8);
            for (int i = 0; i < patternSize; i++)
                groundFloorDoor.bayPattern.Add(rgen.output > 0.2f ? 2 : 0);
            groundFloorDoor.bayPattern.Insert(rgen.OutputRange(0, patternSize), 1); //insert door into pattern
            data.Facades.Add(groundFloorDoor);
            //couple of main facades
            //utility/back wall facade
            //maybe attic version

            Plan plan = data.Plan;
            for (int v = 0; v < plan.numberOfVolumes; v++)
            {
                Volume volume = plan.volumes[v];
                int numberOfFloors = volume.numberOfFloors;
                volume.styles.Clear();
                for (int f = 0; f < volume.points.Count; f++)
                {
                    int facadeIndex = volume.points[f];
                    volume.styles.AddStyle(0, facadeIndex, numberOfFloors - 1);
                    volume.styles.AddStyle(1, facadeIndex, 1);
                }
            }
        }

        private static void GenerateFloorPlan(Data data)
        {
            var plan = ScriptableObject.CreateInstance<Plan>();
            var bounds = data.Footprint;

            // TODO if a lot of points - split to volumes?
            plan.AddVolume(bounds.ToArray());
            data.Plan = plan;
        }

        private static void GetTextures(Data data)
        {
            List<Texture> walltextures = new List<Texture>();
            List<Texture> windowtextures = new List<Texture>();
            List<Texture> doortextures = new List<Texture>();
            List<Texture> rooftextures = new List<Texture>();
            XmlNodeList xmlTextures = null;
            string textureFilePath = data.GeneratorConstraints.texturePackXML;

            if (File.Exists(textureFilePath))
            {
                XmlDocument xml = new XmlDocument();
                StreamReader sr = new StreamReader(textureFilePath);
                xml.LoadXml(sr.ReadToEnd());
                sr.Close();
                xmlTextures = xml.SelectNodes("data/textures/texture");

                if (xmlTextures == null)
                    return;

                foreach (XmlNode node in xmlTextures)
                {
                    string filepath = node["filepath"].FirstChild.Value;
                    string[] splits = filepath.Split(filenameDelimiters);
                    Texture bTexture = new Texture(splits[splits.Length - 1]);


                    var texture = Resources.Load<Texture2D>(filepath);
                    Debug.Log(filepath + (texture == null? " <null>":" loaded!"));

                    bTexture.texture = texture;
                    bTexture.tiled = node["tiled"].FirstChild.Value == "True";
                    bTexture.patterned = node["patterned"].FirstChild.Value == "True";
                    Vector2 tileUnitUV;
                    tileUnitUV.x = float.Parse(node["tileUnitUVX"].FirstChild.Value);
                    tileUnitUV.y = float.Parse(node["tileUnitUVY"].FirstChild.Value);
                    bTexture.tileUnitUV = tileUnitUV;

                    Vector2 textureUnitSize;
                    textureUnitSize.x = float.Parse(node["textureUnitSizeX"].FirstChild.Value);
                    textureUnitSize.y = float.Parse(node["textureUnitSizeY"].FirstChild.Value);
                    bTexture.textureUnitSize = textureUnitSize;

                    bTexture.tiledX = int.Parse(node["tiledX"].FirstChild.Value);
                    bTexture.tiledY = int.Parse(node["tiledY"].FirstChild.Value);

                    bTexture.door = node["door"].FirstChild.Value == "True";
                    bTexture.window = node["window"].FirstChild.Value == "True";
                    bTexture.wall = node["wall"].FirstChild.Value == "True";
                    bTexture.roof = node["roof"].FirstChild.Value == "True";

                    if (bTexture.wall) walltextures.Add(bTexture);
                    if (bTexture.window) windowtextures.Add(bTexture);
                    if (bTexture.door) doortextures.Add(bTexture);
                    if (bTexture.roof) rooftextures.Add(bTexture);
                }
            }

            RandomGen rgen = data.GeneratorConstraints.rgen;
            wallTexture = walltextures[rgen.OutputRange(0, walltextures.Count - 1)]; //wall
            data.Textures.Add(wallTexture);

            windowTexture = windowtextures[rgen.OutputRange(0, windowtextures.Count - 1)]; //window
            data.Textures.Add(windowTexture);

            roofTexture = rooftextures[rgen.OutputRange(0, rooftextures.Count - 1)]; //roof
            data.Textures.Add(roofTexture);

            doorTexture = doortextures[rgen.OutputRange(0, doortextures.Count - 1)]; //door
            data.Textures.Add(doorTexture);
        }
    }
}