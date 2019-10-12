using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace ForestReco
{
	public static class CBitmapExporter
	{
		private static bool exportHeightmap =>
			CParameterSetter.GetBoolSettings(ESettings.ExportBMHeightmap);
		private static bool exportPositions =>
			CParameterSetter.GetBoolSettings(ESettings.ExportBMTreePositions);
		private static bool exportBorders =>
			CParameterSetter.GetBoolSettings(ESettings.ExportBMTreeBorders);

		public static bool FILTER_MAIN_MAP = true; //slows export
		private static bool exportMain = true;

		public const int TILE_WIDTH = 400; //todo: set from GUI?
		public const int MAIN_WIDTH = 1000; //todo: set from GUI?
		private const float MAX_COLOR_VALUE = 255f;

		private static Bitmap mainMap;
		private static float mainMapStepSize;

		private static Color treeColor = Color.Blue;
		private static Color invalidTreeColor = Color.DarkSlateGray;
		private static Color treeBorderColor = Color.FromArgb(255, 0, 255);
		private static Color branchColor = Color.Yellow;

		private static int treeMarkerSize;

		private static SolidBrush treeBorderBrush = new SolidBrush(treeBorderColor);
		private static SolidBrush branchBrush = new SolidBrush(branchColor);
		private static SolidBrush treeBrush = new SolidBrush(treeColor);
		private static SolidBrush invalidTreeBrush = new SolidBrush(invalidTreeColor);
		private static Pen treeBorderPen = new Pen(treeBorderBrush);
		private static Pen branchPen = new Pen(branchBrush);

		private static bool exportBitmap => CParameterSetter.GetBoolSettings(ESettings.exportBitmap) && !CRxpParser.IsRxp;

		/// <summary>
		/// Create a bitmap big enough to store all results together
		/// Warning: can be inited only after main header is set
		/// </summary>
		public static void Init()
		{		
			if(!exportBitmap)
				return;

			float mainArrayWidth = CProjectData.mainHeader.Width;
			float mainArrayHeight = CProjectData.mainHeader.Height;
			mainMapStepSize = CGroundArray.GetStepSizeForWidth(MAIN_WIDTH, mainArrayWidth);
			mainMap = new Bitmap((int)(mainArrayWidth / mainMapStepSize),
				(int)(mainArrayHeight / mainMapStepSize));
		}

		public static void ExportMain()
		{
			if(!exportMain || !exportBitmap)
				return;

			//mainMap.SetPixel(0, 0, treeColor);

			//main bitmap has a lot of missing fields - apply filter
			//todo: make radius dependent on bitmap width?
			if(FILTER_MAIN_MAP)
			{
				//looks good enough on .3f, higher npt necessary, just slows down rapidly
				const float kernelRadius = .3f;
				int kernelSize = GetKernelSize(mainMapStepSize, kernelRadius);
				FilterBitmap(ref mainMap, kernelSize, EFilter.ColorOrMax);
			}

			ExportBitmap(mainMap, "tree_positions_main", -1);
		}

		public static void Export(int pTileIndex)
		{
			if(!exportBitmap)
				return;

			//init for each tile
			treeMarkerSize = GetTreeBrushSize(false);

			DateTime bitmapStart = DateTime.Now;

			CVegeArray array = CProjectData.Points.vegeDetailArray;
			Bitmap bitmap = new Bitmap(array.arrayXRange, array.arrayYRange);

			int maxValue = 0;
			for(int x = 0; x < array.arrayXRange; x++)
			{
				for(int y = 0; y < array.arrayYRange; y++)
				{

					CVegeField element = array.GetField(x, y);
					int? colorVal = element.GetColorValue(); //from detailed array

					if(colorVal == null)
						continue;

					int colorVaInt = (int)colorVal;
					if(colorVaInt > maxValue)
						maxValue = colorVaInt;

					int rVal = colorVaInt;
					//highlight buffer zone
					bool isAtBufferZone = CTreeMath.IsAtBufferZone(element.Center);
					if(isAtBufferZone)
						rVal = Math.Min(rVal + 30, 255);

					Color color = Color.FromArgb(rVal, colorVaInt, colorVaInt);
					//CDebug.WriteLine($"{x},{y} = {color}");

					bitmap.SetPixel(x, y, color);


					if(exportMain && !isAtBufferZone)
					{
						Tuple<int, int> posInMain = GetIndexInMainBitmap(element.Center);

						if(posInMain == null)
							continue;

						if(color.R > 255)
							CDebug.Error("color.R = " + color.R);
						mainMap.SetPixel(posInMain.Item1, posInMain.Item2, color);
					}
				}
			}

			//StretchColorRange(ref bitmap, maxValue);

			//FilterBitmap(ref bitmap, GetKernelSize(array.stepSize, .2f), EFilter.Max);

			if(exportHeightmap)
				ExportBitmap(bitmap, "heightmap", pTileIndex);

			int bitmapsCount = 3;
			bool useCheckTree = CParameterSetter.GetBoolSettings(ESettings.useCheckTreeFile);
			if(useCheckTree)
			{ bitmapsCount++; }

			CDebug.Progress(1, bitmapsCount, 1, ref bitmapStart, bitmapStart, "bitmap: ");

			if(exportPositions)
			{
				Bitmap bitmapTreePos = new Bitmap(bitmap);
				AddTreesToBitmap(array, bitmapTreePos, true, false);
				ExportBitmap(bitmapTreePos, "tree_positions", pTileIndex);

				if(useCheckTree)
				{
					Bitmap bitmapChecktree = new Bitmap(bitmapTreePos);
					ExportBitmap(bitmapChecktree, "tree_check", pTileIndex);
					CDebug.Progress(bitmapsCount - 1, bitmapsCount, 1, ref bitmapStart, bitmapStart, "bitmap: ");
				}
			}

			CDebug.Progress(2, bitmapsCount, 1, ref bitmapStart, bitmapStart, "bitmap: ");

			if(exportBorders)
			{
				Bitmap bitmapTreeBorder = new Bitmap(bitmap);
				AddTreesToBitmap(array, bitmapTreeBorder, true, true);
				ExportBitmap(bitmapTreeBorder, "tree_borders", pTileIndex);
			}

			CDebug.Progress(bitmapsCount, bitmapsCount, 1, ref bitmapStart, bitmapStart, "bitmap: ");

			CAnalytics.bitmapExportDuration = CAnalytics.GetDuration(bitmapStart);
			CDebug.Duration("bitmap export", bitmapStart);
		}

		/// <summary>
		/// Transforms given point from world coordinates to index in main bitmap
		/// </summary>
		private static Tuple<int, int> GetIndexInMainBitmap(Vector3 pPoint)
		{
			Tuple<int, int> posInMain = CGroundArray.GetIndexInArray(
										pPoint, CProjectData.mainHeader.TopLeftCorner, mainMapStepSize);

			//todo: error posInMain is OOB
			CUtils.TransformArrayIndexToBitmapIndex(ref posInMain,
				CProjectData.mainHeader, mainMapStepSize, mainMap);

			if(!CUtils.IsInBitmap(posInMain, mainMap))
			{
				CDebug.Error($"cant write to bitmap {posInMain.Item1}, {posInMain.Item2}");
				return null;
			}

			return posInMain;
		}

		private static bool IsOOB(int pX, int pY, Bitmap pBitmap)
		{
			return pX < 0 || pX >= pBitmap.Width || pY < 0 || pY >= pBitmap.Height;
		}

		private static void AddTreesToBitmap(CVegeArray pArray, Bitmap pBitmap, bool pTreePostition, bool pTreeBorder)
		{
			List<CTree> allTrees = new List<CTree>();
			allTrees.AddRange(CTreeManager.Trees);
			allTrees.AddRange(CTreeManager.InvalidTrees);

			foreach(CTree tree in allTrees)
			{
				try
				{
					CVegeField fieldWithTree = pArray.GetFieldContainingPoint(tree.peak.Center);
					if(fieldWithTree == null)
					{
						CDebug.Error($"tree {tree.treeIndex} field = null");
						continue;
					}

					int x = fieldWithTree.indexInField.Item1;
					int y = fieldWithTree.indexInField.Item2;

					if(IsOOB(x, y, pBitmap))
					{
						CDebug.Error($"{x},{y} is OOB {pBitmap.Width}x{pBitmap.Height}");
						continue;
					}

					//draw branch extents
					if(pTreeBorder && tree.isValid)
					{
						List<Vector3> furthestPoints = tree.GetFurthestPoints();
						//	new List<Vector3>();
						//foreach(CBranch branch in tree.Branches)
						//{
						//	furthestPoints.Add(branch.furthestPoint);
						//}
						for(int i = 0; i < furthestPoints.Count; i++)
						{
							Vector3 furthestPoint = furthestPoints[i];
							Vector3 nextFurthestPoint = furthestPoints[(i + 1) % furthestPoints.Count];

							CVegeField fieldWithFP1 = pArray.GetFieldContainingPoint(furthestPoint);
							CVegeField fieldWithFP2 = pArray.GetFieldContainingPoint(nextFurthestPoint);
							if(fieldWithFP1 == null || fieldWithFP2 == null)
							{
								CDebug.Error($"futhest points {furthestPoint} + {nextFurthestPoint} - no field assigned");
								continue;
							}

							int x1 = fieldWithFP1.indexInField.Item1;
							int y1 = fieldWithFP1.indexInField.Item2;
							int x2 = fieldWithFP2.indexInField.Item1;
							int y2 = fieldWithFP2.indexInField.Item2;

							using(Graphics g = Graphics.FromImage(pBitmap))
							{
								g.DrawLine(treeBorderPen, x1, y1, x2, y2);
							}
						}

						foreach(CBranch branch in tree.Branches)
						{
							CVegeField fieldWithBranch = pArray.GetFieldContainingPoint(branch.furthestPoint);
							if(fieldWithBranch == null)
							{
								CDebug.Error($"branch {branch} is OOB");
								continue;
							}

							int _x = fieldWithBranch.indexInField.Item1;
							int _y = fieldWithBranch.indexInField.Item2;

							using(Graphics g = Graphics.FromImage(pBitmap))
							{
								g.DrawLine(branchPen, x, y, _x, _y);
							}
						}
					}
					//mark tree position
					if(pTreePostition)
					{
						DrawTreeOnBitmap(pBitmap, tree, x, y);

						bool isAtBufferZone = CTreeMath.IsAtBufferZone(tree);
						if(exportMain && !isAtBufferZone)
						{
							//Tuple<int, int> posInMain = CGroundArray.GetPositionInArray(
							//	tree.peak.Center, CProjectData.mainHeader.TopLeftCorner, mainMapStepSize);

							//CUtils.TransformArrayIndexToBitmapIndex(ref posInMain,
							//	CProjectData.mainHeader, mainMapStepSize, mainMap);
							Tuple<int, int> posInMain = GetIndexInMainBitmap(tree.peak.Center);
							x = posInMain.Item1;
							y = posInMain.Item2;
							if(posInMain == null)
								continue;

							Color color = mainMap.GetPixel(x, y);
							if(!IsTreeColoured(color))
								DrawTreeOnBitmap(mainMap, tree, x, y);
						}
					}
				}
				catch(Exception e)
				{
					CDebug.Error(e.Message);
				}
			}
		}

		private static void DrawTreeOnBitmap(Bitmap pBitmap, CTree tree, int x, int y)
		{
			using(Graphics g = Graphics.FromImage(pBitmap))
			{
				int _x = x - treeMarkerSize / 2;
				if(_x < 0)
					_x = x;

				int _y = y - treeMarkerSize / 2;
				if(_y < 0)
					_y = y;

				g.FillRectangle(tree.isValid ? treeBrush : invalidTreeBrush, _x, _y, treeMarkerSize, treeMarkerSize);
			}
		}

		public enum EFilter
		{
			Blur,
			Max,
			Min,
			ColorOrBlur, //prefer colored value from neighbourhood 
			ColorOrMax
		}

		private static void FilterBitmap(ref Bitmap pBitmap, int pKernelSize, EFilter pFilter)
		{
			Bitmap copyBitmap = new Bitmap(pBitmap);
			for(int x = 0; x < pBitmap.Width; x++)
			{
				for(int y = 0; y < pBitmap.Height; y++)
				{
					Color color = pBitmap.GetPixel(x, y);
					if(pFilter == EFilter.ColorOrMax || pFilter == EFilter.ColorOrBlur)
						if(IsTreeColoured(color))
							continue;

					int origVal = color.R;
					if(origVal > 0)
					{ continue; }
					int definedCount = 0;
					int valueSum = 0;
					int maxValue = 0;
					int minValue = 0;
					for(int i = -pKernelSize; i < pKernelSize; i++)
					{
						for(int j = -pKernelSize; j < pKernelSize; j++)
						{
							int _x = x + i;
							int _y = y + j;
							if(IsOOB(_x, _y, pBitmap))
							{ continue; }
							int val = pBitmap.GetPixel(_x, _y).R;

							if(val > 0)
							{
								valueSum += val;
								definedCount++;
								if(val > maxValue)
								{ maxValue = val; }
								if(val < minValue)
								{ minValue = val; }
							}
						}
					}

					int newVal = 0;
					switch(pFilter)
					{
						case EFilter.Blur:
						case EFilter.ColorOrBlur:
							newVal = definedCount > 0 ? valueSum / definedCount : 0;
							break;
						case EFilter.Max:
						case EFilter.ColorOrMax:
							newVal = maxValue;
							break;
						case EFilter.Min:
							newVal = minValue;
							break;
					}

					Color newColor = Color.FromArgb(newVal, newVal, newVal);

					if(newColor.R > 255)
						CDebug.Error("color.R = " + newColor.R);
					copyBitmap.SetPixel(x, y, newColor);
				}
			}
			pBitmap = copyBitmap;
		}

		private static bool IsTreeColoured(Color color)
		{
			//return color.Equals(treeColor); //doesnt work
			return color.R == treeColor.R && color.G == treeColor.G && color.B == treeColor.B;
			//return //color == invalidTreeColor ||
			//	(color.R != color.G || color.R != color.B || color.G != color.B);
		}

		/// <summary>
		/// Scale¨color values in bitmap so that max value = 255 (MAX_COLOR_VALUE)
		/// </summary>
		private static void StretchColorRange(ref Bitmap pBitmap, int pMaxValue)
		{
			//if no value was not assigned (its error, but just to prevent exception)
			pMaxValue = Math.Max(1, pMaxValue);
			float scale = MAX_COLOR_VALUE / pMaxValue;

			//skip process if change is not significant
			if(scale - 1 < 0.1f)
				return;

			for(int x = 0; x < pBitmap.Width; x++)
			{
				for(int y = 0; y < pBitmap.Height; y++)
				{
					Color color = pBitmap.GetPixel(x, y);
					int origVal = color.R;
					int scaledVal = Math.Min((int)(origVal * scale), 255);
					Color newColor = Color.FromArgb(scaledVal, scaledVal, scaledVal);

					if(newColor.R > 255)
						CDebug.Error("color.R = " + newColor.R);
					pBitmap.SetPixel(x, y, newColor);
				}
			}
		}

		private static int GetTreeBrushSize(bool pSmall)
		{
			float width = CProjectData.currentTileHeader.Width;
			bool isArrayLarge = width > 150;
			const int smallSize = 3;
			int size = smallSize;
			if(!pSmall)
			{ size *= 2; }
			if(isArrayLarge)
			{ size /= 2; }

			size = Math.Max(pSmall ? 1 : 2, size);

			return size;
		}

		private static int GetKernelSize(float pArrayStepSize, float pRadius)
		{
			int size = (int)(pRadius / pArrayStepSize);
			size = Math.Max(1, size);
			return size;
		}

		private static void ResizeBitmap(ref Bitmap pBitmap, int pMaxWidth)
		{
			bool isBitmapWidthSmaller = pBitmap.Width < pMaxWidth;
			if(isBitmapWidthSmaller)
				return;

			float scale = (float)pMaxWidth / pBitmap.Width;
			Bitmap resized = new Bitmap(pBitmap, new Size((int)(pBitmap.Width * scale), (int)(pBitmap.Height * scale)));
			pBitmap = resized;
		}

		/// <summary>
		/// pTileIndex -1 = main bitmap
		/// </summary>
		private static void ExportBitmap(Bitmap pBitmap, string pName, int pTileIndex)
		{
			bool pIsMain = pTileIndex == -1;
			ResizeBitmap(ref pBitmap, pIsMain ? MAIN_WIDTH : TILE_WIDTH);

			string fileName = pName + (pIsMain ? "" : "_" + pTileIndex) + ".jpg";
			string folder = pIsMain ? CProjectData.outputFolder : CProjectData.outputTileSubfolder;
			//string folder = CProjectData.outputFolder;
			string filePath = folder + "/" + fileName;
			pBitmap.Save(filePath, ImageFormat.Jpeg);
		}
	}
}
