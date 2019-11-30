using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ObjParser;
using ObjParser.Types;

namespace ForestReco
{
	public static class CMaterialManager
	{
		public static Mtl materials;

		private static Dictionary<EMaterial, List<int>> materialSet;

		private static bool useTreeMaterial;

		public static void Init()
		{
			useTreeMaterial = CParameterSetter.GetBoolSettings(ESettings.colorTrees);

			materialSet = new Dictionary<EMaterial, List<int>>();

			materials = new Mtl("colors");
			AddMaterial("invalid", .3f, EMaterial.Invalid);
			AddMaterial("alarm", 1, 0, 0, EMaterial.Alarm);
			AddMaterial("fake", 1, 0, 1, EMaterial.Fake);
			AddMaterial("checkTree", 0, 0, 1, EMaterial.CheckTree);

			//https://www.december.com/html/spec/colorper.html
			AddMaterial("unassignedGold", .6f, .8f, .1f, EMaterial.UnassignedPoint);
			AddMaterial("groundBrown", .62f, .44f, .23f, EMaterial.GroundPoint);
			AddMaterial("vegeGreen", .6f, 1, .6f, EMaterial.VegePoint);
			AddMaterial("buildingRed", .63f, .32f, .18f, EMaterial.BuildingPoint);

			AddTreeMaterial("red", 1, 0, 0);
			AddTreeMaterial("orange", 1, .5f, 0);
			AddTreeMaterial("lightOrange", 1, .2f, 0);
			AddTreeMaterial("pink", 1, 0, .5f);
			AddTreeMaterial("purple", .5f, 0, .5f);

			AddTreeMaterial("green", 0, 1, 0);
			AddTreeMaterial("yellow", 1, 1, 0);

			AddTreeMaterial("azure", 0, 1, 1);
			AddTreeMaterial("lightBlue", 0, .5f, 1);
			AddTreeMaterial("darkBlue", 0, 0, .3f);
			AddTreeMaterial("mediumBlue", 0, 0, .7f);
		}

		private static void AddMaterial(string pName, float pColorIntensity, EMaterial pType = EMaterial.None)
		{
			AddMaterial(pName, pColorIntensity, pColorIntensity, pColorIntensity, pType);
		}

		private static void AddTreeMaterial(string pName, float pR, float pG, float pB)
		{
			AddMaterial(pName, pR, pG, pB, EMaterial.Tree);
		}


		private static void AddMaterial(string pName, float pR, float pG, float pB, EMaterial pType = EMaterial.None)
		{
			Material mat = new Material(pName);
			mat.DiffuseReflectivity = new Color(pR, pG, pB);
			materials.MaterialList.Add(mat);
			int newMatIndex = materials.MaterialList.Count - 1;
			if (pType != EMaterial.None)
			{
				if (materialSet.ContainsKey(pType))
				{
					materialSet[pType].Add(newMatIndex);
				}
				else
				{
					materialSet.Add(pType, new List<int> { newMatIndex });
				}
			}
		}

		/// <summary>
		/// Returns a material assigned to this tree
		/// </summary>
		public static Material GetTreeMaterial(CTree pTree)
		{
			int selectedIndex = pTree.treeIndex;

			List<CTree> neighbourTrees = CProjectData.Points.treeNormalArray.GetTreesInMaxStepsFrom(pTree.Center, 5);		
			List<Material> assignedMaterials = new List<Material>();
			foreach (CTree tree in neighbourTrees)
			{
				if(tree.Equals(pTree)){ continue; }
				assignedMaterials.Add(tree.assignedMaterial);
			}

			Material selectedMaterial = GetTreeMaterial(selectedIndex);
			for (int i = 0; i < materialSet[EMaterial.Tree].Count; i++)
			{
				selectedMaterial = GetTreeMaterial(selectedIndex + i);
				if (!assignedMaterials.Contains(selectedMaterial))
				{
					return selectedMaterial;
				}
			}
			//CDebug.Warning("No material left to assign. it will be same as neighbour");
			return selectedMaterial;
		}


		private static Material GetTreeMaterial(int pIndex)
		{
			if(!useTreeMaterial){ return null; }

			List<int> treeIndexes = materialSet[EMaterial.Tree];
			int matIndex = (pIndex % treeIndexes.Count + treeIndexes.Count) % treeIndexes.Count;
			if (matIndex < 0 || matIndex > treeIndexes.Count - 1)
			{
				CDebug.Error("matIndex OOR");
				matIndex = 0;
			}

			return materials.MaterialList[treeIndexes[matIndex]];
		}


		public static Material GetPointsMaterial(EClass pClass)
		{
			EMaterial material = GetClassMaterial(pClass);
			return materials.MaterialList[materialSet[material][0]];
		}

		private static EMaterial GetClassMaterial(EClass pClass)
		{
			switch(pClass)
			{
				case EClass.Unassigned:
					return EMaterial.UnassignedPoint;
				case EClass.Ground:
					return EMaterial.GroundPoint;
				case EClass.Vege:
					return EMaterial.VegePoint;
				case EClass.Building:
					return EMaterial.BuildingPoint;
			}

			return EMaterial.None;
		}

		public static Material GetInvalidMaterial()
		{
			return materials.MaterialList[materialSet[EMaterial.Invalid][0]];
		}

		public static Material GetFakeMaterial()
		{
			return materials.MaterialList[materialSet[EMaterial.Fake][0]];
		}

		public static Material GetAlarmMaterial()
		{
			return materials.MaterialList[materialSet[EMaterial.Alarm][0]];
		}

		public static Material GetCheckTreeMaterial()
		{
			return materials.MaterialList[materialSet[EMaterial.CheckTree][0]];
		}
	}

	public enum EMaterial
	{
		None,
		Tree,
		RefTree,
		CheckTree,
		Invalid,
		Fake,
		Alarm,
		VegePoint,
		UnassignedPoint,
		GroundPoint,
		BuildingPoint
	}
}