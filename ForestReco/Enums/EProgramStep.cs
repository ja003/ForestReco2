using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public enum EProgramStep
	{
		LoadReftrees, //not part of tile processing

		LoadLines,
		ParseLines,
		ProcessUnassignedPoints,
		ProcessBuildingPoints,
		ProcessGroundPoints,
		PreprocessVegePoints,
		ProcessVegePoints,
		ValidateTrees1,
		MergeNotTrees1,
		MergeTrees1,
		ValidateTrees2,
		MergeNotTrees2,
		MergeTrees2,
		ValidateTrees3,
		AssignReftrees,
		Export3D,
		Bitmap,
		Dart,
		Shp,
		Las,
		Analytics,
		ExportMainFiles,

		Done,

		Cancelled,
		Exception,

		Pre_Tile,
		Pre_Noise,
		Pre_LasGround,
		Pre_LasHeight,
		Pre_LasClassify,
		Pre_LasReverseTile,
		Pre_DeleteTmp,
		Pre_Split,
		Pre_LasToTxt,
	}
}
