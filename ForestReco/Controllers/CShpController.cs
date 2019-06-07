using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ProjNet.CoordinateSystems;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace ForestReco
{
	public static class CShpController
	{
		private const string TREE_POS_FILE = "tree_positions";
		private const string TREE_BORDER_FILE = "tree_borders";
		private const string MAIN = "_main";

		private const string ATTR_ID = "id";
		private const string ATTR_X = "X";
		private const string ATTR_Y = "Y";
		private const string ATTR_AREA = "area";
		private const string ATTR_HEIGHT = "height";
		private const string ATTR_DBG = "DBH";
		private const string ATTR_AGB = "AGB";
		private const string ATTR_TYPE = "type";
		private const string NUM_FORMAT = "0.00";
		private static List<IFeature> treePositionsAll;
		private static List<IFeature> treeAreasAll;

		private static GeometryFactory factory = new GeometryFactory();

		private static bool exportShape => CParameterSetter.GetBoolSettings(ESettings.exportShape) &&
			(exportShapeTreePos || exportShapeTreeAreas);
		private static bool exportShapeTreePos => CParameterSetter.GetBoolSettings(ESettings.exportShapeTreePositions);
		private static bool exportShapeTreeAreas => CParameterSetter.GetBoolSettings(ESettings.exportShapeTreeAreas);


		public static void Init()
		{
			treePositionsAll = new List<IFeature>();
			treeAreasAll = new List<IFeature>();
		}

		/// <summary>
		/// Exports all features as one which were previously exported separated
		/// </summary>
		public static void ExportMain()
		{
			if(!exportShape)
				return;

			if(exportShapeTreePos)
				ExportFeatures(treePositionsAll, TREE_POS_FILE + MAIN, true);
			if(exportShapeTreeAreas)
				ExportFeatures(treeAreasAll, TREE_BORDER_FILE + MAIN, true);
		}

		/// <summary>
		/// Export file using data from currently loaded trees
		/// </summary>
		public static void ExportCurrent()
		{
			if(!exportShape)
				return;

			List<IFeature> treePositions = new List<IFeature>();
			List<IFeature> treeBorders = new List<IFeature>();

			foreach(CTree tree in CTreeManager.Trees)
			{
				//tree positions
				Feature f = GetTreePosition(tree);
				treePositions.Add(f);
				treePositionsAll.Add(f);

				//tree borders
				f = GetTreeBorder(tree);
				treeBorders.Add(f);
				treeAreasAll.Add(f);
			}

			if(exportShapeTreePos)
				ExportFeatures(treePositions, TREE_POS_FILE, false);
			if(exportShapeTreeAreas)
				ExportFeatures(treeBorders, TREE_BORDER_FILE, false);
		}

		/// <summary>
		/// Generates point feature representing position of the tree.
		/// Attributes:
		/// id, X, Y, height, DBH, AGB, type
		/// </summary>
		private static Feature GetTreePosition(CTree pTree)
		{
			Vector3 globalTreepos = CUtils.GetGlobalPosition(pTree.peak.Center);
			IPoint myPoint = factory.CreatePoint(new Coordinate(globalTreepos.X, globalTreepos.Z));

			AttributesTable attributesTable = new AttributesTable();
			attributesTable.Add(ATTR_ID, pTree.treeIndex);

			attributesTable.Add(ATTR_X, globalTreepos.X.ToString(NUM_FORMAT));
			attributesTable.Add(ATTR_Y, globalTreepos.Z.ToString(NUM_FORMAT));

			float treeHeight = pTree.GetTreeHeight();
			attributesTable.Add(ATTR_HEIGHT, treeHeight.ToString(NUM_FORMAT));

			if(CParameterSetter.GetBoolSettings(ESettings.calculateDBH))
			{
				double stemDiameter = CBiomassController.GetTreeStemDiameter(treeHeight);
				attributesTable.Add(ATTR_DBG, stemDiameter.ToString(NUM_FORMAT));

				if(CParameterSetter.GetBoolSettings(ESettings.calculateAGB))
				{
					double biomass = CBiomassController.GetTreeBiomass(stemDiameter, treeHeight);
					attributesTable.Add(ATTR_AGB, biomass.ToString(NUM_FORMAT));
				}
			}

			attributesTable.Add(ATTR_TYPE, pTree.assignedRefTree.RefTreeTypeName);

			Feature feature = new Feature(myPoint, attributesTable);

			return feature;
		}

		/// <summary>
		/// 
		/// </summary>
		private static Feature GetTreeBorder(CTree pTree)
		{
			List<Vector3> furthestPoints = pTree.GetFurthestPoints();
			List<Coordinate> pointsCoords = new List<Coordinate>();
			foreach(Vector3 p in furthestPoints)
			{
				Vector3 globalP = CUtils.GetGlobalPosition(p);
				pointsCoords.Add(new Coordinate(globalP.X, globalP.Z));
			}
			pointsCoords.Add(pointsCoords[0]); //to close polygon

			IPolygon polygon = factory.CreatePolygon(pointsCoords.ToArray());

			//id
			AttributesTable attributesTable = new AttributesTable();
			attributesTable.Add(ATTR_ID, pTree.treeIndex);

			//position
			Vector3 globalTreepos = CUtils.GetGlobalPosition(pTree.peak.Center);
			attributesTable.Add(ATTR_X, globalTreepos.X.ToString(NUM_FORMAT));
			attributesTable.Add(ATTR_Y, globalTreepos.Z.ToString(NUM_FORMAT));

			//area
			attributesTable.Add(ATTR_AREA, pTree.GetArea());

			//tree height
			float treeHeight = pTree.GetTreeHeight();
			attributesTable.Add(ATTR_HEIGHT, treeHeight.ToString(NUM_FORMAT));

			//reftree type
			attributesTable.Add(ATTR_TYPE, pTree.assignedRefTree.RefTreeTypeName);

			Feature feature = new Feature(polygon, attributesTable);

			return feature;
		}

		private static void ExportFeatures(List<IFeature> pFeatures, string pFileName, bool pIsMainFile)
		{
			if(pFeatures.Count == 0)
			{
				CDebug.Warning("You are trying to export 0 features");
				return;
			}

			string shapeFileFolder = pIsMainFile ? CProjectData.outputFolder : CProjectData.outputTileSubfolder;
			shapeFileFolder += "/shp_" + pFileName;
			Directory.CreateDirectory(shapeFileFolder);//create subfolder

			// Construct the shapefile name. Don't add the .shp extension or the ShapefileDataWriter will 
			// produce an unwanted shapefile
			string shapeFileName = Path.Combine(shapeFileFolder, pFileName);
			string shapeFilePrjName = Path.Combine(shapeFileFolder, $"{pFileName}.prj");

			// Create the shapefile
			var outGeomFactory = GeometryFactory.Default;
			var writer = new ShapefileDataWriter(shapeFileName, outGeomFactory);
			var outDbaseHeader = ShapefileDataWriter.GetHeader(pFeatures[0], pFeatures.Count);
			writer.Header = outDbaseHeader;
			writer.Write(pFeatures);

			//Create the projection file
			using(var streamWriter = new StreamWriter(shapeFilePrjName))
			{
				streamWriter.Write(GeographicCoordinateSystem.WGS84.WKT);
			}
		}
	}
}
