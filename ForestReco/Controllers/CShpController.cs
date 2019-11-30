using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ProjNet.CoordinateSystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

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
		private const int DEBUG_FREQUENCY = 100;

		private static GeometryFactory factory = new GeometryFactory();

		public static bool exportShape => CParameterSetter.GetBoolSettings(ESettings.exportShape) &&
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

		const string SEP = ",";

		/// <summary>
		/// Export file using data from currently loaded trees
		/// </summary>
		public static void ExportCurrent()
		{
			if(!exportShape)
				return;

			DateTime start = DateTime.Now;
			DateTime lastDebug = DateTime.Now;

			List<IFeature> treePositions = new List<IFeature>();
			List<IFeature> treeBorders = new List<IFeature>();
			StringBuilder shpInfo = new StringBuilder();

			shpInfo.Append(ATTR_ID + SEP);
			shpInfo.Append(ATTR_X + SEP);
			shpInfo.Append(ATTR_Y + SEP);
			//shpInfo.Append(ATTR_AREA + SEP);
			shpInfo.Append(ATTR_HEIGHT + SEP);
			shpInfo.Append(ATTR_DBG + SEP);
			shpInfo.Append(ATTR_AGB);
			shpInfo.AppendLine();

			for(int i = 0; i < CTreeManager.Trees.Count; i++)
			{
				CDebug.Progress(i, CTreeManager.Trees.Count, DEBUG_FREQUENCY, ref lastDebug, start, "Export shp (trees)");
				CTree tree = CTreeManager.Trees[i];
				//tree positions
				Feature f = GetTreePosition(tree, ref shpInfo);
				treePositions.Add(f);
				treePositionsAll.Add(f);

				//tree borders
				f = GetTreeBorder(tree);
				treeBorders.Add(f);
				treeAreasAll.Add(f);
			}
			CLasExporter.WriteToFile(shpInfo, CProjectData.outputTileSubfolder + "shape_info.csv");

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
		private static Feature GetTreePosition(CTree pTree, ref StringBuilder pString)
		{
			CVector3D globalTreepos = CUtils.GetGlobalPosition(pTree.peak.Center);
			IPoint myPoint = factory.CreatePoint(new Coordinate(globalTreepos.X, globalTreepos.Y));

			AttributesTable attributesTable = new AttributesTable();
			attributesTable.Add(ATTR_ID, pTree.treeIndex);
			pString.Append(pTree.treeIndex + SEP);

			attributesTable.Add(ATTR_X, globalTreepos.X.ToString(NUM_FORMAT));
			attributesTable.Add(ATTR_Y, globalTreepos.Y.ToString(NUM_FORMAT));
			pString.Append(globalTreepos.X.ToString(NUM_FORMAT) + SEP);
			pString.Append(globalTreepos.Y.ToString(NUM_FORMAT) + SEP);

			float treeHeight = pTree.GetTreeHeight();
			attributesTable.Add(ATTR_HEIGHT, treeHeight.ToString(NUM_FORMAT));
			pString.Append(treeHeight.ToString(NUM_FORMAT) + SEP);

			if(CParameterSetter.GetBoolSettings(ESettings.calculateDBH))
			{
				double stemDiameter = CBiomassController.GetTreeStemDiameter(treeHeight);
				attributesTable.Add(ATTR_DBG, stemDiameter.ToString(NUM_FORMAT));
				pString.Append(stemDiameter.ToString(NUM_FORMAT) + SEP);

				if(CParameterSetter.GetBoolSettings(ESettings.calculateAGB))
				{
					double biomass = CBiomassController.GetTreeBiomass(stemDiameter, treeHeight);
					attributesTable.Add(ATTR_AGB, biomass.ToString(NUM_FORMAT));
					pString.Append(biomass.ToString(NUM_FORMAT) + SEP);
				}
			}

			//251 - Finalizace produktu
			//attributesTable.Add(ATTR_TYPE, pTree.assignedRefTree.RefTreeTypeName);
			//pString.Append(pTree.assignedRefTree.RefTreeTypeName + SEP);

			Feature feature = new Feature(myPoint, attributesTable);
			pString.AppendLine();

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
				CVector3D globalP = CUtils.GetGlobalPosition(p);
				pointsCoords.Add(new Coordinate(globalP.X, globalP.Y));
			}
			pointsCoords.Add(pointsCoords[0]); //to close polygon

			IPolygon polygon = factory.CreatePolygon(pointsCoords.ToArray());

			//id
			AttributesTable attributesTable = new AttributesTable();
			attributesTable.Add(ATTR_ID, pTree.treeIndex);

			//position
			CVector3D globalTreepos = CUtils.GetGlobalPosition(pTree.peak.Center);
			attributesTable.Add(ATTR_X, globalTreepos.X.ToString(NUM_FORMAT));
			attributesTable.Add(ATTR_Y, globalTreepos.Y.ToString(NUM_FORMAT));

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
