using ObjParser;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ForestReco
{
	public static class CObjPartition
	{
		public static List<Obj>[,] objPartition;
		private static int partitionXRange;
		private static int partitionYRange;

		private static int partitionStepSize;

		/// <summary>
		/// Calculate partition step size in proportion to used ground array step
		/// </summary>
		private static void SetPartitionStepSize()
		{
			float scaledPartitionStep =
				CParameterSetter.GetIntSettings(ESettings.partitionStep) /
				CParameterSetter.GetFloatSettings(ESettings.groundArrayStep);
			partitionStepSize = Math.Max(1, (int)scaledPartitionStep);
		}

		public static void Init()
		{
			SetPartitionStepSize();

			partitionXRange = CProjectData.Points.groundArray.arrayXRange / partitionStepSize + 1; //has to be +1
			partitionYRange = CProjectData.Points.groundArray.arrayYRange / partitionStepSize + 1;

			objPartition = new List<Obj>[partitionXRange, partitionYRange];
			Console.WriteLine($"objPartition: {partitionXRange} x {partitionYRange}");
			for(int x = 0; x < partitionXRange; x++)
			{
				for(int y = 0; y < partitionYRange; y++)
				{
					objPartition[x, y] = new List<Obj>();
				}
			}
		}

		public static void AddGroundArrayObj()
		{
			for(int x = 0; x < CProjectData.Points.groundArray.arrayXRange; x += partitionStepSize)
			{
				for(int y = 0; y < CProjectData.Points.groundArray.arrayYRange; y += partitionStepSize)
				{
					Obj groundArrayPartObj = CGroundFieldExporter.ExportToObj("array_[" + x + "," + y + "]",
						EExportStrategy.ZeroAroundDefined, true,
						new Tuple<int, int>(x, y), new Tuple<int, int>(x + partitionStepSize, y + partitionStepSize));

					AddObj(x, y, groundArrayPartObj);
				}
			}
		}

		/// <summary>
		/// Add points of given class to the partitions
		/// </summary>
		public static void AddPoints(EClass pClass)
		{
			for(int x = 0; x < CProjectData.Points.groundArray.arrayXRange; x += partitionStepSize)
			{
				for(int y = 0; y < CProjectData.Points.groundArray.arrayYRange; y += partitionStepSize)
				{
					List<Vector3> points = new List<Vector3>();
					for(int _x = x; _x < x + partitionStepSize; _x++)
					{
						for(int _y = y; _y < y + partitionStepSize; _y++)
						{
							CField element = CProjectData.Points.GetField(pClass, _x, _y);
							//CField element = CProjectData.Points.groundArray.GetField(_x, _y);
							if(element != null)
							{
								points.AddRange(element.points);
							}
						}
					}
					Obj pointsObj = new Obj(GetPointsObjName(pClass));
					CObjExporter.AddPointsToObj(ref pointsObj, points);
					pointsObj.UseMtl = CMaterialManager.GetPointsMaterial(pClass).Name;

					AddObj(x, y, pointsObj);
				}
			}
		}

		private static string GetPointsObjName(EClass pClass)
		{
			string prefix = pClass.ToString();
			string suffix = "Points";

			return prefix + suffix;
		}

		public static void AddTrees(bool pValid)//, bool pFake)
		{
			List<Tuple<Tuple<int, int>, CTree>> treesToExport = new List<Tuple<Tuple<int, int>, CTree>>();

			bool exportTreeStrucure = CParameterSetter.GetBoolSettings(ESettings.exportTreeStructures);
			bool exportBoxes = CParameterSetter.GetBoolSettings(ESettings.exportTreeBoxes);

			if(!exportBoxes && !exportTreeStrucure) { return; }

			foreach(CTree tree in pValid ? CTreeManager.Trees : CTreeManager.InvalidTrees)
			{
				treesToExport.Add(new Tuple<Tuple<int, int>, CTree>(tree.peakNormalField.indexInField, tree));
			}

			//special: add not-trees
			if(!pValid)
			{
				foreach(CTree tree in CTreeManager.NotTrees)
				{
					treesToExport.Add(new Tuple<Tuple<int, int>, CTree>(tree.peakNormalField.indexInField, tree));
				}
			}

			treesToExport.Sort((x, y) => x.Item2.treeIndex.CompareTo(y.Item2.treeIndex));
			foreach(Tuple<Tuple<int, int>, CTree> exportTree in treesToExport)
			{
				Obj obj = exportTree.Item2.GetObj(exportTreeStrucure, false, exportBoxes);
				if(!pValid) { obj.UseMtl = CMaterialManager.GetInvalidMaterial().Name; }

				AddObj(exportTree.Item1, obj);
			}
		}

		public static void AddRefTrees()
		{
			List<CTree> trees = new List<CTree>();
			foreach(CTreeField f in CProjectData.Points.treeNormalArray.fields)
			{
				foreach(CTree tree in f.DetectedTrees)
				{
					if(!tree.isValid)
						continue;
					if(!trees.Contains(tree))
						trees.Add(tree);
				}
			}
			foreach(CTree tree in trees)
			{
				if(tree.assignedRefTreeObj == null)
				{
					//not error if reftrees were not loaded
					if(CReftreeManager.Trees.Count > 0)
					{
						CDebug.Error($"{tree} has no reftree assigned");
					}
					return;
				}
				AddObj(CProjectData.Points.treeNormalArray.GetIndexInArray(tree.peak.Center), tree.assignedRefTreeObj);
			}
		}

		public static void AddObj(Tuple<int, int> pArrayIndex, Obj pObj)
		{
			AddObj(pArrayIndex.Item1, pArrayIndex.Item2, pObj);
		}

		public static void AddObj(int pArrayIndexX, int pArrayIndexY, Obj pObj)
		{
			if(pObj == null)
			{
				CDebug.Error("AddObj is null!");
			}
			Tuple<int, int> index = GetIndexInArray(pArrayIndexX, pArrayIndexY);
			AddToPartition(pObj, index);
		}

		private static void AddToPartition(Obj pObj, Tuple<int, int> pIndex)
		{
			//TODO: error, incorrect partition
			if(pIndex.Item1 >= partitionXRange)
			{
				CDebug.Error($"obj {pObj.Name} has partition index {pIndex} OOB");
				pIndex = new Tuple<int, int>(partitionXRange - 1, pIndex.Item2);
			}
			if(pIndex.Item2 >= partitionYRange)
			{
				CDebug.Error($"obj {pObj.Name} has partition index {pIndex} OOB");
				pIndex = new Tuple<int, int>(pIndex.Item1, partitionYRange - 1);
			}
			objPartition[pIndex.Item1, pIndex.Item2].Add(pObj);
		}

		public static void ExportPartition(string pFileSuffix = "", string pIndexPrefix = "")
		{
			//folderPath = CObjExporter.CreateFolderIn(
			//	CProjectData.saveFileName, CProjectData.outputTileSubfolder);

			//just creates a folder (for analytics etc)
			if(!CParameterSetter.GetBoolSettings(ESettings.export3d) || CRxpParser.IsRxp)
			{
				CDebug.WriteLine("Skipping export");
				return;
			}

			int counter = 0;
			DateTime exportPartitionStart = DateTime.Now;
			DateTime previousDebugStart = DateTime.Now;
			int partsCount = partitionXRange * partitionYRange;

			int debugFrequency = 1;
			for(int x = 0; x < partitionXRange; x++)
			{
				for(int y = 0; y < partitionYRange; y++)
				{
					if(CProjectData.backgroundWorker.CancellationPending) { return; }

					counter++;
					List<Obj> objsInPartition = objPartition[x, y];
					//export only if partition contains some objects (doesn't have to)
					if(objsInPartition.Count > 0)
					{
						CObjExporter.ExportObjs(objsInPartition,
							$"{CProjectData.saveFileName}_{pIndexPrefix}[{x},{y}]{pFileSuffix}", CProjectData.outputTileSubfolder);
					}

					CDebug.Progress(counter, partsCount, debugFrequency, ref previousDebugStart, exportPartitionStart, "Export of part");
				}
			}
		}

		private static Tuple<int, int> GetIndexInArray(int pArrayIndexX, int pArrayIndexY)
		{
			return new Tuple<int, int>(pArrayIndexX / partitionStepSize, pArrayIndexY / partitionStepSize);
		}
	}
}