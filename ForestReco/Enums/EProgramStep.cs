﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForestReco
{
	public enum EProgramStep
	{
		LoadLines = 1,
		LoadReftrees = 2, //not part of tile processing
		ParseLines = 3,
		ProcessGroundPoints = 4,
		PreprocessVegePoints = 5,
		ProcessVegePoints = 6,
		ValidateTrees1 = 7,
		MergeTrees1 = 8,
		ValidateTrees2 = 9,
		MergeTrees2 = 10,
		ValidateTrees3 = 11,
		AssignReftrees = 12,
		LoadCheckTrees = 13,
		AssignCheckTrees = 14,
		Export = 15,
		Bitmap = 16,
		Dart = 17,
		Shp = 18,
		Las = 19,
		Analytics = 20,

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
