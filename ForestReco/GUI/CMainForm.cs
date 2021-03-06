﻿using ForestReco.GUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Windows.Forms;

// ReSharper disable ConditionIsAlwaysTrueOrFalse - TEST VALUES

namespace ForestReco
{
	public class CMainForm : Form
	{
		#region properties
		public TextBox textForestFileName;
		private Button btnStart;
		public TextBox textOutputFolder;
		private Button btnOutputFolder;
		public ProgressBar progressBar;
		public TextBox textProgress;
		private Button btnAbort;
		private Button btnToggleConsole;
		private TrackBar trackBarTreeExtent;
		private TextBox textTreeExtent;
		private Label labelTreeExtent;
		private TrackBar trackBarTreeExtentMultiply;
		private TextBox textTreeExtentMultiply;
		private Label labelTreeExtentMultiply;
		private TrackBar trackBarAvgTreeHeight;
		private TextBox textAvgTreeHeight;
		private Label labelAvgTreeHeight;
		private ToolTip myToolTip;
		private System.ComponentModel.IContainer components;
		private Button btnOpenResult;
		private CheckBox checkBoxAutoTreeHeight;
		private CheckBox checkBoxExportBitmap;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
		public TrackBar trackBarRangeXmin;
		public TextBox textRangeXmin;
		private Label label1;
		public TrackBar trackBarRangeXmax;
		public TrackBar trackBarRangeYmax;
		public TrackBar trackBarRangeYmin;
		public TextBox textRangeYmin;
		private Label label2;
		private Button btnSellectForest;

		private TextBox textLasTools;
		private Button btnLasTools;
		#endregion


		private TextBox textTmpFolder;
		private Button btnTmpFolder;

		private CUiRangeController rangeController;
		private CUiDetectionMethod detectionMethod;

		public TextBox textRangeYmax;
		public TextBox textRangeXmax;
		private Label label3;
		private Label label4;
		public Label labelRangeXval;
		public Label labelRangeYval;
		private ComboBox comboBoxSplitMode;
		public TextBox textShapefile;
		public Button btnShapefile;
		private Label label5;
		private TrackBar trackBarTileSize;
		private TextBox textTileSize;
		private Label label6;
		private CheckedListBox checkedListBoxBitmaps;
		private CheckBox checkBoxPreprocess;
		private CheckBox checkBoxDeleteTmp;
		private RichTextBox richTextDBH;
		private Label label7;
		private Label label8;
		private RichTextBox richTextAGB;
		private CheckedListBox checkedListBoxShape;
		private CheckBox checkBoxExportShape;
		private CheckBox checkBoxExportLas;
		private CheckBox checkBoxDBH;
		private CheckBox checkBoxAGB;
		private Label label9;
		public TextBox textStartTile;
		private Label label11;
		public RichTextBox richTextTreeRadius;
		public TextBox textForestFileFolder;
		public TextBox textForestFileExtension;
		private TrackBar trackBarMinTreeHeight;
		private TextBox textMinTreeHeight;
		private Label label12;
		public TrackBar trackBarMinTreePoints;
		private TextBox textMinTreePoints;
		private Label label17;
		private CUiPathSelection pathSelection;

		public CMainForm()
		{

			rangeController = new CUiRangeController(this);
			detectionMethod = new CUiDetectionMethod(this);
			pathSelection = new CUiPathSelection(this);

			InitializeComponent();
			InitializeValues();

			CProjectData.backgroundWorker = backgroundWorker1;
			backgroundWorker1.WorkerSupportsCancellation = true;
			backgroundWorker1.WorkerReportsProgress = true;

		}

		private void InitializeValues()
		{
			CParameterSetter.Init();
			//textForestFolder.Text = CParameterSetter.GetStringSettings(ESettings.forestFolderPath);

			textForestFileFolder.Text = CParameterSetter.GetStringSettings(ESettings.forestFileFolder);
			textForestFileName.Text = CParameterSetter.GetStringSettings(ESettings.forestFileName);
			textForestFileExtension.Text = CParameterSetter.GetStringSettings(ESettings.forestFileExtension);

			//textReftreeFolder.Text = CParameterSetter.GetStringSettings(ESettings.reftreeFolderPath);
			textOutputFolder.Text = CParameterSetter.GetStringSettings(ESettings.outputFolderPath);
			//textCheckTreePath.Text = CParameterSetter.GetStringSettings(ESettings.checkTreeFilePath);
			//textAnalyticsFile.Text = CParameterSetter.GetStringSettings(ESettings.analyticsFilePath);

			textLasTools.Text = CParameterSetter.GetStringSettings(ESettings.lasToolsFolderPath);
			textTmpFolder.Text = CParameterSetter.GetStringSettings(ESettings.tmpFilesFolderPath);


			//partition
			//trackBarPartition.Value = CParameterSetter.GetIntSettings(ESettings.partitionStep);
			//trackBarPartition_Scroll(null, null); //force text refresh

			trackBarTileSize.Value = CParameterSetter.GetIntSettings(ESettings.tileSize);
			trackBarTileSize_Scroll(null, null);


			comboBoxSplitMode.SelectedIndex = CParameterSetter.GetIntSettings(ESettings.currentSplitMode);

			//gorund array step
			float groundArrayStep = CParameterSetter.GetFloatSettings(ESettings.groundArrayStep);
			//trackBarGroundArrayStep.Value = (int)(groundArrayStep * 10f);
			//textGroundArrayStep.Text = groundArrayStep.ToString("0.0") + " m";

			//tree extent
			float treeExtent = CParameterSetter.GetFloatSettings(ESettings.treeExtent);
			trackBarTreeExtent.Value = (int)(treeExtent * 10f);
			textTreeExtent.Text = treeExtent.ToString("0.0") + " m";

			//tree extent multiply
			float treeExtentMultiply = CParameterSetter.GetFloatSettings(ESettings.treeExtentMultiply);
			trackBarTreeExtentMultiply.Value = (int)(treeExtentMultiply * 10f);
			textTreeExtentMultiply.Text = treeExtentMultiply.ToString("0.0");

			//average tree height
			textAvgTreeHeight.Text = CParameterSetter.GetIntSettings(ESettings.avgTreeHeigh) + " m";
			trackBarAvgTreeHeight.Value = CParameterSetter.GetIntSettings(ESettings.avgTreeHeigh);

			richTextDBH.Text = CParameterSetter.GetStringSettings(ESettings.dbh);
			richTextAGB.Text = CParameterSetter.GetStringSettings(ESettings.agb);
			richTextTreeRadius.Text = CParameterSetter.GetStringSettings(ESettings.treeRadius);

			textStartTile.Text = CParameterSetter.GetIntSettings(ESettings.startIndex).ToString();

			//int detectMethodIndex = CParameterSetter.GetIntSettings(ESettings.detectMethod);
			//if(detectMethodIndex < 0)
			//	detectMethodIndex = 0;
			//if(detectMethodIndex >= comboBoxDetectMethod.Items.Count)
			//	detectMethodIndex = comboBoxDetectMethod.Items.Count - 1;

			//comboBoxDetectMethod.SelectedIndex = detectMethodIndex;

			//min tree height
			int minTreeHeight = CParameterSetter.GetIntSettings(ESettings.minTreeHeight);
			trackBarMinTreeHeight.Value = minTreeHeight;
			TrackBarMinTreeHeight_Scroll(trackBarMinTreeHeight, EventArgs.Empty);


			/// 2D detection 
			
			float localMaxHeight = CParameterSetter.GetFloatSettings(ESettings.localMaxHeight);
			//trackBarLocalMaxHeight.Value = (int)(localMaxHeight * 10f);
			//trackBarLocalMaxHeight_Scroll(trackBarLocalMaxHeight.Value, EventArgs.Empty);

			float allowedDescend = CParameterSetter.GetFloatSettings(ESettings.allowedDescend);
			//trackBarAllowedDescend.Value = (int)(allowedDescend * 10f);
			//trackBarAllowedDescend_Scroll(trackBarAllowedDescend.Value, EventArgs.Empty);

			int minAscendSteps = CParameterSetter.GetIntSettings(ESettings.minAscendSteps);
			//trackBarMinAscendSteps.Value = minAscendSteps;
			//trackBarMinAscendSteps_Scroll(trackBarMinAscendSteps.Value, EventArgs.Empty);

			int minDescendSteps = CParameterSetter.GetIntSettings(ESettings.minDescendSteps);
			//trackBarMinDescendSteps.Value = minDescendSteps;
			//trackBarMinDescendSteps_Scroll(trackBarMinDescendSteps.Value, EventArgs.Empty);

			int minTreePoints = CParameterSetter.GetIntSettings(ESettings.minTreePoints);
			trackBarMinTreePoints.Value = minTreePoints;
			trackBarMinTreePoints_Scroll(trackBarMinTreePoints.Value, EventArgs.Empty);


			//bools
			CParameterSetter.SetParameter(ESettings.export3d, false);
			//checkBoxExport3d.Checked =
			//	CParameterSetter.GetBoolSettings(ESettings.export3d);
			//checkBoxExort3d_CheckedChanged(this, EventArgs.Empty); //force refresh

			//- bitmaps
			checkBoxExportBitmap.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportBitmap);
			checkBoxExportBitmap_CheckedChanged(this, EventArgs.Empty); //force refresh

			checkedListBoxBitmaps.SetItemChecked(0,
				CParameterSetter.GetBoolSettings(ESettings.ExportBMHeightmap));
			checkedListBoxBitmaps.SetItemChecked(1,
				CParameterSetter.GetBoolSettings(ESettings.ExportBMTreePositions));
			checkedListBoxBitmaps.SetItemChecked(2,
				CParameterSetter.GetBoolSettings(ESettings.ExportBMTreeBorders));

			//- shape
			checkBoxExportShape.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportShape);
			checkBoxExportShape_CheckedChanged(this, EventArgs.Empty); //force refresh
			checkedListBoxShape.SetItemChecked(0,
				CParameterSetter.GetBoolSettings(ESettings.exportShapeTreePositions));
			checkedListBoxShape.SetItemChecked(1,
				CParameterSetter.GetBoolSettings(ESettings.exportShapeTreeAreas));

			checkBoxExportLas.Checked =
				CParameterSetter.GetBoolSettings(ESettings.exportLas);

			checkBoxDBH.Checked = CParameterSetter.GetBoolSettings(ESettings.calculateDBH);
			checkBoxDBH_CheckedChanged(this, EventArgs.Empty); //force refresh
			checkBoxAGB.Checked = CParameterSetter.GetBoolSettings(ESettings.calculateAGB);
			checkBoxAGB_CheckedChanged(this, EventArgs.Empty); //force refresh

			//checkBoxExportTreeStructures.Checked =
			//	CParameterSetter.GetBoolSettings(ESettings.exportTreeStructures);
			//checkBoxExportInvalidTrees.Checked =
			//	CParameterSetter.GetBoolSettings(ESettings.exportInvalidTrees);
			//checkBoxExportRefTrees.Checked =
			//	CParameterSetter.GetBoolSettings(ESettings.exportRefTrees);
			//checkBoxAssignRefTreesRandom.Checked =
				//CParameterSetter.GetBoolSettings(ESettings.assignRefTreesRandom);
			//checkBoxUseCheckTree.Checked =
				//CParameterSetter.GetBoolSettings(ESettings.useCheckTreeFile);
			//checkBoxUseCheckTree_CheckedChanged(this, EventArgs.Empty); //force refresh
			checkBoxDeleteTmp.Checked =
				CParameterSetter.GetBoolSettings(ESettings.deleteTmp);
			checkBoxPreprocess.Checked =
				CParameterSetter.GetBoolSettings(ESettings.preprocess);

			//checkBoxExportCheckTrees.Checked =
			//	CParameterSetter.GetBoolSettings(ESettings.exportCheckTrees);
			//checkBoxExportTreeBoxes.Checked =
			//	CParameterSetter.GetBoolSettings(ESettings.exportTreeBoxes);
			//checkBoxColorTrees.Checked =
			//CParameterSetter.GetBoolSettings(ESettings.colorTrees);
			//checkBoxReducedReftrees.Checked =
				//CParameterSetter.GetBoolSettings(ESettings.useReducedReftreeModels);
			//checkBoxFilterPoints.Checked =
				//CParameterSetter.GetBoolSettings(ESettings.filterPoints);
			//checkBoxExportPoints.Checked =
				//CParameterSetter.GetBoolSettings(ESettings.exportPoints);
			checkBoxAutoTreeHeight.Checked =
				CParameterSetter.GetBoolSettings(ESettings.autoAverageTreeHeight);

			SetStartBtnEnabled(true);

			CDebug.Init(this);

			#region tooltip
			CTooltipManager.AssignTooltip(myToolTip, btnSellectForest, ESettings.forestFileName);
			//CTooltipManager.AssignTooltip(myToolTip, btnSequence, ETooltip.sequenceFile);
			//CTooltipManager.AssignTooltip(myToolTip, btnReftreesFolder, ESettings.reftreeFolderPath);
			CTooltipManager.AssignTooltip(myToolTip, btnOutputFolder, ESettings.outputFolderPath);
			//CTooltipManager.AssignTooltip(myToolTip, btnAnalytics, ESettings.analyticsFilePath);
			//CTooltipManager.AssignTooltip(myToolTip, btnSelectCheckTree, ESettings.checkTreeFilePath);

			CTooltipManager.AssignTooltip(myToolTip, btnToggleConsole, ETooltip.toggleConsole);
			CTooltipManager.AssignTooltip(myToolTip, btnOpenResult, ETooltip.openResult);


			//CTooltipManager.AssignTooltip(myToolTip, checkBoxExport3d, ESettings.export3d);
			//CTooltipManager.AssignTooltip(myToolTip, checkBoxExportBitmap, ESettings.exportBitmap);
			////CTooltipManager.AssignTooltip(myToolTip, checkBoxAssignRefTreesRandom, ESettings.assignRefTreesRandom);
			//CTooltipManager.AssignTooltip(myToolTip, checkBoxExportInvalidTrees, ESettings.exportInvalidTrees);
			//CTooltipManager.AssignTooltip(myToolTip, checkBoxExportRefTrees, ESettings.exportRefTrees);
			//CTooltipManager.AssignTooltip(myToolTip, checkBoxExportTreeStructures, ESettings.exportTreeStructures);
			//CTooltipManager.AssignTooltip(myToolTip, checkBoxColorTrees, ESettings.colorTrees);
			//CTooltipManager.AssignTooltip(myToolTip, checkBoxReducedReftrees, ESettings.useReducedReftreeModels);
			//CTooltipManager.AssignTooltip(myToolTip, checkBoxExportTreeBoxes, ESettings.exportTreeBoxes);
			////CTooltipManager.AssignTooltip(myToolTip, checkBoxUseCheckTree, ESettings.useCheckTreeFile);
			//CTooltipManager.AssignTooltip(myToolTip, checkBoxExportCheckTrees, ESettings.exportCheckTrees);
			////CTooltipManager.AssignTooltip(myToolTip, checkBoxFilterPoints, ESettings.filterPoints);
			//CTooltipManager.AssignTooltip(myToolTip, checkBoxExportPoints, ESettings.exportPoints);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxAutoTreeHeight, ESettings.autoAverageTreeHeight);

			//CTooltipManager.AssignTooltip(myToolTip, labelPartition, ESettings.partitionStep);
			CTooltipManager.AssignTooltip(myToolTip, labelAvgTreeHeight, ESettings.avgTreeHeigh);
			//CTooltipManager.AssignTooltip(myToolTip, labelGroundArrayStep, ESettings.groundArrayStep);
			CTooltipManager.AssignTooltip(myToolTip, labelTreeExtent, ESettings.treeExtent);
			CTooltipManager.AssignTooltip(myToolTip, labelTreeExtentMultiply, ESettings.treeExtentMultiply);

			//CTooltipManager.AssignTooltip(myToolTip, labelEstimatedTotalSize, /ETooltip.EstimatedTotalSize);
			//CTooltipManager.AssignTooltip(myToolTip, labelEstimatedPartitionSize, ETooltip.EstimatedPartitionSize);
			CTooltipManager.AssignTooltip(myToolTip, trackBarAvgTreeHeight, ETooltip.avgTreeHeighSlider);

			CTooltipManager.AssignTooltip(myToolTip, richTextTreeRadius, ESettings.treeRadius);



			CTooltipManager.AssignTooltip(myToolTip, checkBoxDBH, ESettings.calculateDBH);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxAGB, ESettings.calculateAGB);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxPreprocess, ESettings.preprocess);
			CTooltipManager.AssignTooltip(myToolTip, checkBoxDeleteTmp, ESettings.deleteTmp);
			CTooltipManager.AssignTooltip(myToolTip, trackBarMinTreeHeight, ESettings.minTreeHeight);
			CTooltipManager.AssignTooltip(myToolTip, trackBarMinTreePoints, ESettings.minTreePoints);


			#endregion
		}



		private void btnStart_Click(object sender, EventArgs e)
		{
			if(!CUiInputCheck.CheckProblems())
			{
				return;
			}
			CProgramStarter.PrepareSequence();
			SetStartBtnEnabled(false);

			if(backgroundWorker1.IsBusy != true)
			{
				// Start the asynchronous operation.
				backgroundWorker1.RunWorkerAsync();
			}

			//CProgramStarter.Start();
		}

		private void btnAbort_Click(object sender, EventArgs e)
		{
			//CProjectData.backgroundWorker.CancellationPending();
			if(backgroundWorker1.WorkerSupportsCancellation)
			{
				// Cancel the asynchronous operation.
				backgroundWorker1.CancelAsync();
			}
		}

		private void btnToggleConsole_Click(object sender, EventArgs e)
		{
			CParameterSetter.ToggleConsoleVisibility();
		}

		#region INIT
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CMainForm));
			this.btnSellectForest = new System.Windows.Forms.Button();
			this.textForestFileName = new System.Windows.Forms.TextBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.textOutputFolder = new System.Windows.Forms.TextBox();
			this.btnOutputFolder = new System.Windows.Forms.Button();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.textProgress = new System.Windows.Forms.TextBox();
			this.btnAbort = new System.Windows.Forms.Button();
			this.btnToggleConsole = new System.Windows.Forms.Button();
			this.trackBarTreeExtent = new System.Windows.Forms.TrackBar();
			this.textTreeExtent = new System.Windows.Forms.TextBox();
			this.labelTreeExtent = new System.Windows.Forms.Label();
			this.trackBarTreeExtentMultiply = new System.Windows.Forms.TrackBar();
			this.textTreeExtentMultiply = new System.Windows.Forms.TextBox();
			this.labelTreeExtentMultiply = new System.Windows.Forms.Label();
			this.trackBarAvgTreeHeight = new System.Windows.Forms.TrackBar();
			this.textAvgTreeHeight = new System.Windows.Forms.TextBox();
			this.labelAvgTreeHeight = new System.Windows.Forms.Label();
			this.myToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.checkBoxAutoTreeHeight = new System.Windows.Forms.CheckBox();
			this.checkBoxExportBitmap = new System.Windows.Forms.CheckBox();
			this.checkBoxPreprocess = new System.Windows.Forms.CheckBox();
			this.checkBoxDeleteTmp = new System.Windows.Forms.CheckBox();
			this.checkBoxExportShape = new System.Windows.Forms.CheckBox();
			this.checkBoxExportLas = new System.Windows.Forms.CheckBox();
			this.checkBoxDBH = new System.Windows.Forms.CheckBox();
			this.checkBoxAGB = new System.Windows.Forms.CheckBox();
			this.btnOpenResult = new System.Windows.Forms.Button();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.trackBarRangeXmin = new System.Windows.Forms.TrackBar();
			this.textRangeXmin = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.trackBarRangeXmax = new System.Windows.Forms.TrackBar();
			this.trackBarRangeYmax = new System.Windows.Forms.TrackBar();
			this.trackBarRangeYmin = new System.Windows.Forms.TrackBar();
			this.textRangeYmin = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textLasTools = new System.Windows.Forms.TextBox();
			this.btnLasTools = new System.Windows.Forms.Button();
			this.textTmpFolder = new System.Windows.Forms.TextBox();
			this.btnTmpFolder = new System.Windows.Forms.Button();
			this.textRangeYmax = new System.Windows.Forms.TextBox();
			this.textRangeXmax = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.labelRangeXval = new System.Windows.Forms.Label();
			this.labelRangeYval = new System.Windows.Forms.Label();
			this.comboBoxSplitMode = new System.Windows.Forms.ComboBox();
			this.textShapefile = new System.Windows.Forms.TextBox();
			this.btnShapefile = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.trackBarTileSize = new System.Windows.Forms.TrackBar();
			this.textTileSize = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.checkedListBoxBitmaps = new System.Windows.Forms.CheckedListBox();
			this.richTextDBH = new System.Windows.Forms.RichTextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.richTextAGB = new System.Windows.Forms.RichTextBox();
			this.checkedListBoxShape = new System.Windows.Forms.CheckedListBox();
			this.label9 = new System.Windows.Forms.Label();
			this.textStartTile = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.richTextTreeRadius = new System.Windows.Forms.RichTextBox();
			this.textForestFileFolder = new System.Windows.Forms.TextBox();
			this.textForestFileExtension = new System.Windows.Forms.TextBox();
			this.trackBarMinTreeHeight = new System.Windows.Forms.TrackBar();
			this.textMinTreeHeight = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.trackBarMinTreePoints = new System.Windows.Forms.TrackBar();
			this.textMinTreePoints = new System.Windows.Forms.TextBox();
			this.label17 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtent)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtentMultiply)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarAvgTreeHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeXmin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeXmax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeYmax)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeYmin)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTileSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarMinTreeHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarMinTreePoints)).BeginInit();
			this.SuspendLayout();
			// 
			// btnSellectForest
			// 
			this.btnSellectForest.Location = new System.Drawing.Point(12, 16);
			this.btnSellectForest.Name = "btnSellectForest";
			this.btnSellectForest.Size = new System.Drawing.Size(121, 31);
			this.btnSellectForest.TabIndex = 0;
			this.btnSellectForest.Text = "forest file";
			this.btnSellectForest.UseVisualStyleBackColor = true;
			this.btnSellectForest.Click += new System.EventHandler(this.btnSellectForest_Click);
			// 
			// textForestFileName
			// 
			this.textForestFileName.BackColor = System.Drawing.SystemColors.Window;
			this.textForestFileName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textForestFileName.Location = new System.Drawing.Point(520, 20);
			this.textForestFileName.Name = "textForestFileName";
			this.textForestFileName.Size = new System.Drawing.Size(126, 19);
			this.textForestFileName.TabIndex = 1;
			this.textForestFileName.Text = "forest file name";
			this.textForestFileName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textForestFileName.TextChanged += new System.EventHandler(this.textForestFileName_TextChanged);
			// 
			// btnStart
			// 
			this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(113)))), ((int)(((byte)(237)))), ((int)(((byte)(124)))));
			this.btnStart.Location = new System.Drawing.Point(375, 226);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(234, 48);
			this.btnStart.TabIndex = 5;
			this.btnStart.Text = "START";
			this.btnStart.UseVisualStyleBackColor = false;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// textOutputFolder
			// 
			this.textOutputFolder.Location = new System.Drawing.Point(147, 57);
			this.textOutputFolder.Name = "textOutputFolder";
			this.textOutputFolder.Size = new System.Drawing.Size(558, 20);
			this.textOutputFolder.TabIndex = 7;
			this.textOutputFolder.TextChanged += new System.EventHandler(this.textOutputFolder_TextChanged);
			// 
			// btnOutputFolder
			// 
			this.btnOutputFolder.Location = new System.Drawing.Point(12, 53);
			this.btnOutputFolder.Name = "btnOutputFolder";
			this.btnOutputFolder.Size = new System.Drawing.Size(121, 31);
			this.btnOutputFolder.TabIndex = 6;
			this.btnOutputFolder.Text = "output folder";
			this.btnOutputFolder.UseVisualStyleBackColor = true;
			this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
			// 
			// progressBar
			// 
			this.progressBar.Location = new System.Drawing.Point(203, 406);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(501, 23);
			this.progressBar.TabIndex = 9;
			// 
			// textProgress
			// 
			this.textProgress.Location = new System.Drawing.Point(204, 282);
			this.textProgress.Multiline = true;
			this.textProgress.Name = "textProgress";
			this.textProgress.ReadOnly = true;
			this.textProgress.Size = new System.Drawing.Size(501, 118);
			this.textProgress.TabIndex = 10;
			// 
			// btnAbort
			// 
			this.btnAbort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(124)))), ((int)(((byte)(112)))));
			this.btnAbort.Location = new System.Drawing.Point(615, 226);
			this.btnAbort.Name = "btnAbort";
			this.btnAbort.Size = new System.Drawing.Size(89, 48);
			this.btnAbort.TabIndex = 11;
			this.btnAbort.Text = "ABORT";
			this.btnAbort.UseVisualStyleBackColor = false;
			this.btnAbort.Click += new System.EventHandler(this.btnAbort_Click);
			// 
			// btnToggleConsole
			// 
			this.btnToggleConsole.Location = new System.Drawing.Point(469, 435);
			this.btnToggleConsole.Name = "btnToggleConsole";
			this.btnToggleConsole.Size = new System.Drawing.Size(109, 32);
			this.btnToggleConsole.TabIndex = 12;
			this.btnToggleConsole.Text = "toggle console";
			this.btnToggleConsole.UseVisualStyleBackColor = true;
			this.btnToggleConsole.Click += new System.EventHandler(this.btnToggleConsole_Click);
			// 
			// trackBarTreeExtent
			// 
			this.trackBarTreeExtent.AutoSize = false;
			this.trackBarTreeExtent.LargeChange = 10;
			this.trackBarTreeExtent.Location = new System.Drawing.Point(202, 120);
			this.trackBarTreeExtent.Maximum = 30;
			this.trackBarTreeExtent.Minimum = 5;
			this.trackBarTreeExtent.Name = "trackBarTreeExtent";
			this.trackBarTreeExtent.Size = new System.Drawing.Size(159, 30);
			this.trackBarTreeExtent.TabIndex = 29;
			this.trackBarTreeExtent.TickFrequency = 2;
			this.trackBarTreeExtent.Value = 10;
			this.trackBarTreeExtent.Scroll += new System.EventHandler(this.trackBarTreeExtent_Scroll);
			// 
			// textTreeExtent
			// 
			this.textTreeExtent.Location = new System.Drawing.Point(321, 95);
			this.textTreeExtent.Name = "textTreeExtent";
			this.textTreeExtent.ReadOnly = true;
			this.textTreeExtent.Size = new System.Drawing.Size(40, 20);
			this.textTreeExtent.TabIndex = 28;
			this.textTreeExtent.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// labelTreeExtent
			// 
			this.labelTreeExtent.AutoSize = true;
			this.labelTreeExtent.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.labelTreeExtent.Location = new System.Drawing.Point(209, 96);
			this.labelTreeExtent.Name = "labelTreeExtent";
			this.labelTreeExtent.Size = new System.Drawing.Size(68, 13);
			this.labelTreeExtent.TabIndex = 27;
			this.labelTreeExtent.Text = "tree extent";
			// 
			// trackBarTreeExtentMultiply
			// 
			this.trackBarTreeExtentMultiply.AutoSize = false;
			this.trackBarTreeExtentMultiply.LargeChange = 1;
			this.trackBarTreeExtentMultiply.Location = new System.Drawing.Point(366, 120);
			this.trackBarTreeExtentMultiply.Maximum = 30;
			this.trackBarTreeExtentMultiply.Minimum = 10;
			this.trackBarTreeExtentMultiply.Name = "trackBarTreeExtentMultiply";
			this.trackBarTreeExtentMultiply.Size = new System.Drawing.Size(159, 30);
			this.trackBarTreeExtentMultiply.TabIndex = 33;
			this.trackBarTreeExtentMultiply.TickFrequency = 2;
			this.trackBarTreeExtentMultiply.Value = 10;
			this.trackBarTreeExtentMultiply.Scroll += new System.EventHandler(this.trackBarTreeExtentMultiply_Scroll);
			// 
			// textTreeExtentMultiply
			// 
			this.textTreeExtentMultiply.Location = new System.Drawing.Point(492, 95);
			this.textTreeExtentMultiply.Name = "textTreeExtentMultiply";
			this.textTreeExtentMultiply.ReadOnly = true;
			this.textTreeExtentMultiply.Size = new System.Drawing.Size(40, 20);
			this.textTreeExtentMultiply.TabIndex = 32;
			this.textTreeExtentMultiply.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// labelTreeExtentMultiply
			// 
			this.labelTreeExtentMultiply.AutoSize = true;
			this.labelTreeExtentMultiply.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.labelTreeExtentMultiply.Location = new System.Drawing.Point(373, 96);
			this.labelTreeExtentMultiply.Name = "labelTreeExtentMultiply";
			this.labelTreeExtentMultiply.Size = new System.Drawing.Size(88, 13);
			this.labelTreeExtentMultiply.TabIndex = 31;
			this.labelTreeExtentMultiply.Text = "extent multiply";
			// 
			// trackBarAvgTreeHeight
			// 
			this.trackBarAvgTreeHeight.AutoSize = false;
			this.trackBarAvgTreeHeight.BackColor = System.Drawing.SystemColors.Control;
			this.trackBarAvgTreeHeight.LargeChange = 10;
			this.trackBarAvgTreeHeight.Location = new System.Drawing.Point(12, 120);
			this.trackBarAvgTreeHeight.Maximum = 50;
			this.trackBarAvgTreeHeight.Minimum = 5;
			this.trackBarAvgTreeHeight.Name = "trackBarAvgTreeHeight";
			this.trackBarAvgTreeHeight.Size = new System.Drawing.Size(192, 30);
			this.trackBarAvgTreeHeight.TabIndex = 37;
			this.trackBarAvgTreeHeight.TickFrequency = 3;
			this.trackBarAvgTreeHeight.Value = 15;
			this.trackBarAvgTreeHeight.Scroll += new System.EventHandler(this.trackBarAvgTreeHeight_Scroll);
			// 
			// textAvgTreeHeight
			// 
			this.textAvgTreeHeight.Location = new System.Drawing.Point(164, 95);
			this.textAvgTreeHeight.Name = "textAvgTreeHeight";
			this.textAvgTreeHeight.ReadOnly = true;
			this.textAvgTreeHeight.Size = new System.Drawing.Size(40, 20);
			this.textAvgTreeHeight.TabIndex = 36;
			this.textAvgTreeHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// labelAvgTreeHeight
			// 
			this.labelAvgTreeHeight.AutoSize = true;
			this.labelAvgTreeHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.labelAvgTreeHeight.Location = new System.Drawing.Point(15, 96);
			this.labelAvgTreeHeight.Name = "labelAvgTreeHeight";
			this.labelAvgTreeHeight.Size = new System.Drawing.Size(118, 13);
			this.labelAvgTreeHeight.TabIndex = 35;
			this.labelAvgTreeHeight.Text = "average tree height";
			// 
			// myToolTip
			// 
			this.myToolTip.AutoPopDelay = 32767;
			this.myToolTip.InitialDelay = 500;
			this.myToolTip.ReshowDelay = 100;
			this.myToolTip.ShowAlways = true;
			this.myToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			// 
			// checkBoxAutoTreeHeight
			// 
			this.checkBoxAutoTreeHeight.AutoSize = true;
			this.checkBoxAutoTreeHeight.Location = new System.Drawing.Point(12, 161);
			this.checkBoxAutoTreeHeight.Name = "checkBoxAutoTreeHeight";
			this.checkBoxAutoTreeHeight.Size = new System.Drawing.Size(125, 17);
			this.checkBoxAutoTreeHeight.TabIndex = 48;
			this.checkBoxAutoTreeHeight.Text = "automatic tree height";
			this.myToolTip.SetToolTip(this.checkBoxAutoTreeHeight, "include all points into final export file");
			this.checkBoxAutoTreeHeight.UseVisualStyleBackColor = true;
			this.checkBoxAutoTreeHeight.CheckedChanged += new System.EventHandler(this.checkBoxAutoTreeHeight_CheckedChanged);
			// 
			// checkBoxExportBitmap
			// 
			this.checkBoxExportBitmap.AutoSize = true;
			this.checkBoxExportBitmap.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.checkBoxExportBitmap.Location = new System.Drawing.Point(13, 336);
			this.checkBoxExportBitmap.Name = "checkBoxExportBitmap";
			this.checkBoxExportBitmap.Size = new System.Drawing.Size(126, 17);
			this.checkBoxExportBitmap.TabIndex = 59;
			this.checkBoxExportBitmap.Text = "EXPORT BITMAP";
			this.checkBoxExportBitmap.UseVisualStyleBackColor = true;
			this.checkBoxExportBitmap.CheckedChanged += new System.EventHandler(this.checkBoxExportBitmap_CheckedChanged);
			// 
			// checkBoxPreprocess
			// 
			this.checkBoxPreprocess.AutoSize = true;
			this.checkBoxPreprocess.Location = new System.Drawing.Point(204, 226);
			this.checkBoxPreprocess.Name = "checkBoxPreprocess";
			this.checkBoxPreprocess.Size = new System.Drawing.Size(78, 17);
			this.checkBoxPreprocess.TabIndex = 93;
			this.checkBoxPreprocess.Text = "preprocess";
			this.checkBoxPreprocess.UseVisualStyleBackColor = true;
			this.checkBoxPreprocess.CheckedChanged += new System.EventHandler(this.checkBoxPreprocess_CheckedChanged);
			// 
			// checkBoxDeleteTmp
			// 
			this.checkBoxDeleteTmp.AutoSize = true;
			this.checkBoxDeleteTmp.Location = new System.Drawing.Point(204, 250);
			this.checkBoxDeleteTmp.Name = "checkBoxDeleteTmp";
			this.checkBoxDeleteTmp.Size = new System.Drawing.Size(96, 17);
			this.checkBoxDeleteTmp.TabIndex = 94;
			this.checkBoxDeleteTmp.Text = "delete tmp files";
			this.checkBoxDeleteTmp.UseVisualStyleBackColor = true;
			this.checkBoxDeleteTmp.CheckedChanged += new System.EventHandler(this.checkBoxDeleteTmp_CheckedChanged);
			// 
			// checkBoxExportShape
			// 
			this.checkBoxExportShape.AutoSize = true;
			this.checkBoxExportShape.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.checkBoxExportShape.Location = new System.Drawing.Point(13, 267);
			this.checkBoxExportShape.Name = "checkBoxExportShape";
			this.checkBoxExportShape.Size = new System.Drawing.Size(105, 17);
			this.checkBoxExportShape.TabIndex = 99;
			this.checkBoxExportShape.Text = "EXPORT SHP";
			this.checkBoxExportShape.UseVisualStyleBackColor = true;
			this.checkBoxExportShape.CheckedChanged += new System.EventHandler(this.checkBoxExportShape_CheckedChanged);
			// 
			// checkBoxExportLas
			// 
			this.checkBoxExportLas.AutoSize = true;
			this.checkBoxExportLas.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.checkBoxExportLas.Location = new System.Drawing.Point(13, 416);
			this.checkBoxExportLas.Name = "checkBoxExportLas";
			this.checkBoxExportLas.Size = new System.Drawing.Size(81, 17);
			this.checkBoxExportLas.TabIndex = 101;
			this.checkBoxExportLas.Text = "export las";
			this.checkBoxExportLas.UseVisualStyleBackColor = true;
			this.checkBoxExportLas.CheckedChanged += new System.EventHandler(this.checkBoxExportLas_CheckedChanged);
			// 
			// checkBoxDBH
			// 
			this.checkBoxDBH.AutoSize = true;
			this.checkBoxDBH.Location = new System.Drawing.Point(742, 16);
			this.checkBoxDBH.Name = "checkBoxDBH";
			this.checkBoxDBH.Size = new System.Drawing.Size(15, 14);
			this.checkBoxDBH.TabIndex = 102;
			this.checkBoxDBH.UseVisualStyleBackColor = true;
			this.checkBoxDBH.CheckedChanged += new System.EventHandler(this.checkBoxDBH_CheckedChanged);
			// 
			// checkBoxAGB
			// 
			this.checkBoxAGB.AutoSize = true;
			this.checkBoxAGB.Location = new System.Drawing.Point(742, 54);
			this.checkBoxAGB.Name = "checkBoxAGB";
			this.checkBoxAGB.Size = new System.Drawing.Size(15, 14);
			this.checkBoxAGB.TabIndex = 103;
			this.checkBoxAGB.UseVisualStyleBackColor = true;
			this.checkBoxAGB.CheckedChanged += new System.EventHandler(this.checkBoxAGB_CheckedChanged);
			// 
			// btnOpenResult
			// 
			this.btnOpenResult.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(161)))), ((int)(((byte)(212)))));
			this.btnOpenResult.Location = new System.Drawing.Point(595, 435);
			this.btnOpenResult.Name = "btnOpenResult";
			this.btnOpenResult.Size = new System.Drawing.Size(109, 32);
			this.btnOpenResult.TabIndex = 47;
			this.btnOpenResult.Text = "open result";
			this.btnOpenResult.UseVisualStyleBackColor = false;
			this.btnOpenResult.Click += new System.EventHandler(this.btnOpenResult_Click);
			// 
			// backgroundWorker1
			// 
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
			this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
			this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
			// 
			// trackBarRangeXmin
			// 
			this.trackBarRangeXmin.AutoSize = false;
			this.trackBarRangeXmin.LargeChange = 10;
			this.trackBarRangeXmin.Location = new System.Drawing.Point(818, 296);
			this.trackBarRangeXmin.Maximum = 30;
			this.trackBarRangeXmin.Minimum = 5;
			this.trackBarRangeXmin.Name = "trackBarRangeXmin";
			this.trackBarRangeXmin.Size = new System.Drawing.Size(92, 30);
			this.trackBarRangeXmin.SmallChange = 5;
			this.trackBarRangeXmin.TabIndex = 63;
			this.trackBarRangeXmin.TickFrequency = 5;
			this.trackBarRangeXmin.Value = 10;
			this.trackBarRangeXmin.Scroll += new System.EventHandler(this.trackBarRangeXmin_Scroll);
			// 
			// textRangeXmin
			// 
			this.textRangeXmin.Location = new System.Drawing.Point(822, 271);
			this.textRangeXmin.Name = "textRangeXmin";
			this.textRangeXmin.Size = new System.Drawing.Size(85, 20);
			this.textRangeXmin.TabIndex = 100;
			this.textRangeXmin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textRangeXmin.LostFocus += new System.EventHandler(this.textRangeXmin_LostFocus);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(759, 274);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(44, 13);
			this.label1.TabIndex = 61;
			this.label1.Text = "range X";
			// 
			// trackBarRangeXmax
			// 
			this.trackBarRangeXmax.AutoSize = false;
			this.trackBarRangeXmax.LargeChange = 10;
			this.trackBarRangeXmax.Location = new System.Drawing.Point(923, 296);
			this.trackBarRangeXmax.Maximum = 30;
			this.trackBarRangeXmax.Minimum = 5;
			this.trackBarRangeXmax.Name = "trackBarRangeXmax";
			this.trackBarRangeXmax.Size = new System.Drawing.Size(92, 30);
			this.trackBarRangeXmax.SmallChange = 5;
			this.trackBarRangeXmax.TabIndex = 64;
			this.trackBarRangeXmax.TickFrequency = 5;
			this.trackBarRangeXmax.Value = 10;
			this.trackBarRangeXmax.Scroll += new System.EventHandler(this.trackBarRangeXmax_Scroll);
			// 
			// trackBarRangeYmax
			// 
			this.trackBarRangeYmax.AutoSize = false;
			this.trackBarRangeYmax.LargeChange = 10;
			this.trackBarRangeYmax.Location = new System.Drawing.Point(923, 359);
			this.trackBarRangeYmax.Maximum = 30;
			this.trackBarRangeYmax.Minimum = 5;
			this.trackBarRangeYmax.Name = "trackBarRangeYmax";
			this.trackBarRangeYmax.Size = new System.Drawing.Size(92, 30);
			this.trackBarRangeYmax.SmallChange = 5;
			this.trackBarRangeYmax.TabIndex = 68;
			this.trackBarRangeYmax.TickFrequency = 5;
			this.trackBarRangeYmax.Value = 10;
			this.trackBarRangeYmax.Scroll += new System.EventHandler(this.trackBarRangeYmax_Scroll);
			// 
			// trackBarRangeYmin
			// 
			this.trackBarRangeYmin.AutoSize = false;
			this.trackBarRangeYmin.LargeChange = 10;
			this.trackBarRangeYmin.Location = new System.Drawing.Point(818, 359);
			this.trackBarRangeYmin.Maximum = 30;
			this.trackBarRangeYmin.Minimum = 5;
			this.trackBarRangeYmin.Name = "trackBarRangeYmin";
			this.trackBarRangeYmin.Size = new System.Drawing.Size(92, 30);
			this.trackBarRangeYmin.SmallChange = 5;
			this.trackBarRangeYmin.TabIndex = 67;
			this.trackBarRangeYmin.TickFrequency = 5;
			this.trackBarRangeYmin.Value = 10;
			this.trackBarRangeYmin.Scroll += new System.EventHandler(this.trackBarRangeYmin_Scroll);
			// 
			// textRangeYmin
			// 
			this.textRangeYmin.Location = new System.Drawing.Point(822, 334);
			this.textRangeYmin.Name = "textRangeYmin";
			this.textRangeYmin.Size = new System.Drawing.Size(85, 20);
			this.textRangeYmin.TabIndex = 102;
			this.textRangeYmin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textRangeYmin.LostFocus += new System.EventHandler(this.textRangeYmin_LostFocus);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(759, 337);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(44, 13);
			this.label2.TabIndex = 65;
			this.label2.Text = "range Y";
			// 
			// textLasTools
			// 
			this.textLasTools.Location = new System.Drawing.Point(869, 400);
			this.textLasTools.Name = "textLasTools";
			this.textLasTools.Size = new System.Drawing.Size(149, 20);
			this.textLasTools.TabIndex = 70;
			this.textLasTools.TextChanged += new System.EventHandler(this.textLasTools_TextChanged);
			// 
			// btnLasTools
			// 
			this.btnLasTools.Location = new System.Drawing.Point(732, 396);
			this.btnLasTools.Name = "btnLasTools";
			this.btnLasTools.Size = new System.Drawing.Size(121, 31);
			this.btnLasTools.TabIndex = 69;
			this.btnLasTools.Text = "LAStools";
			this.btnLasTools.UseVisualStyleBackColor = true;
			this.btnLasTools.Click += new System.EventHandler(this.btnLasTools_Click);
			// 
			// textTmpFolder
			// 
			this.textTmpFolder.Location = new System.Drawing.Point(869, 438);
			this.textTmpFolder.Name = "textTmpFolder";
			this.textTmpFolder.Size = new System.Drawing.Size(149, 20);
			this.textTmpFolder.TabIndex = 72;
			this.textTmpFolder.TextChanged += new System.EventHandler(this.textTmpFolder_TextChanged);
			// 
			// btnTmpFolder
			// 
			this.btnTmpFolder.Location = new System.Drawing.Point(732, 434);
			this.btnTmpFolder.Name = "btnTmpFolder";
			this.btnTmpFolder.Size = new System.Drawing.Size(121, 31);
			this.btnTmpFolder.TabIndex = 71;
			this.btnTmpFolder.Text = "TMP folder";
			this.btnTmpFolder.UseVisualStyleBackColor = true;
			this.btnTmpFolder.Click += new System.EventHandler(this.btnTmpFolder_Click);
			// 
			// textRangeYmax
			// 
			this.textRangeYmax.Location = new System.Drawing.Point(927, 334);
			this.textRangeYmax.Name = "textRangeYmax";
			this.textRangeYmax.Size = new System.Drawing.Size(85, 20);
			this.textRangeYmax.TabIndex = 103;
			this.textRangeYmax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textRangeYmax.LostFocus += new System.EventHandler(this.textRangeYmax_LostFocus);
			// 
			// textRangeXmax
			// 
			this.textRangeXmax.Location = new System.Drawing.Point(927, 271);
			this.textRangeXmax.Name = "textRangeXmax";
			this.textRangeXmax.Size = new System.Drawing.Size(85, 20);
			this.textRangeXmax.TabIndex = 101;
			this.textRangeXmax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textRangeXmax.TextChanged += new System.EventHandler(this.textRangeXmax_TextChanged);
			this.textRangeXmax.LostFocus += new System.EventHandler(this.textRangeXmax_LostFocus);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(911, 274);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(10, 13);
			this.label3.TabIndex = 78;
			this.label3.Text = "-";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(911, 337);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(10, 13);
			this.label4.TabIndex = 79;
			this.label4.Text = "-";
			// 
			// labelRangeXval
			// 
			this.labelRangeXval.AutoSize = true;
			this.labelRangeXval.Location = new System.Drawing.Point(765, 303);
			this.labelRangeXval.Name = "labelRangeXval";
			this.labelRangeXval.Size = new System.Drawing.Size(36, 13);
			this.labelRangeXval.TabIndex = 80;
			this.labelRangeXval.Text = "100 m";
			// 
			// labelRangeYval
			// 
			this.labelRangeYval.AutoSize = true;
			this.labelRangeYval.Location = new System.Drawing.Point(765, 366);
			this.labelRangeYval.Name = "labelRangeYval";
			this.labelRangeYval.Size = new System.Drawing.Size(36, 13);
			this.labelRangeYval.TabIndex = 81;
			this.labelRangeYval.Text = "100 m";
			// 
			// comboBoxSplitMode
			// 
			this.comboBoxSplitMode.FormattingEnabled = true;
			this.comboBoxSplitMode.Items.AddRange(new object[] {
            "None",
            "Manual",
            "Shapefile"});
			this.comboBoxSplitMode.Location = new System.Drawing.Point(867, 196);
			this.comboBoxSplitMode.Name = "comboBoxSplitMode";
			this.comboBoxSplitMode.Size = new System.Drawing.Size(121, 21);
			this.comboBoxSplitMode.TabIndex = 85;
			this.comboBoxSplitMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxSplitMode_SelectedIndexChanged);
			// 
			// textShapefile
			// 
			this.textShapefile.Location = new System.Drawing.Point(867, 227);
			this.textShapefile.Name = "textShapefile";
			this.textShapefile.Size = new System.Drawing.Size(151, 20);
			this.textShapefile.TabIndex = 87;
			this.textShapefile.TextChanged += new System.EventHandler(this.textShapefile_TextChanged);
			// 
			// btnShapefile
			// 
			this.btnShapefile.Location = new System.Drawing.Point(732, 223);
			this.btnShapefile.Name = "btnShapefile";
			this.btnShapefile.Size = new System.Drawing.Size(121, 31);
			this.btnShapefile.TabIndex = 86;
			this.btnShapefile.Text = "shapefile";
			this.btnShapefile.UseVisualStyleBackColor = true;
			this.btnShapefile.Click += new System.EventHandler(this.btnShapefile_Click);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(765, 199);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(54, 13);
			this.label5.TabIndex = 88;
			this.label5.Text = "split mode";
			// 
			// trackBarTileSize
			// 
			this.trackBarTileSize.AutoSize = false;
			this.trackBarTileSize.LargeChange = 10;
			this.trackBarTileSize.Location = new System.Drawing.Point(732, 160);
			this.trackBarTileSize.Maximum = 250;
			this.trackBarTileSize.Minimum = 20;
			this.trackBarTileSize.Name = "trackBarTileSize";
			this.trackBarTileSize.Size = new System.Drawing.Size(140, 30);
			this.trackBarTileSize.SmallChange = 5;
			this.trackBarTileSize.TabIndex = 91;
			this.trackBarTileSize.TickFrequency = 10;
			this.trackBarTileSize.Value = 30;
			this.trackBarTileSize.Scroll += new System.EventHandler(this.trackBarTileSize_Scroll);
			// 
			// textTileSize
			// 
			this.textTileSize.Location = new System.Drawing.Point(830, 135);
			this.textTileSize.Name = "textTileSize";
			this.textTileSize.ReadOnly = true;
			this.textTileSize.Size = new System.Drawing.Size(40, 20);
			this.textTileSize.TabIndex = 90;
			this.textTileSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(739, 137);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(41, 13);
			this.label6.TabIndex = 89;
			this.label6.Text = "tile size";
			// 
			// checkedListBoxBitmaps
			// 
			this.checkedListBoxBitmaps.CheckOnClick = true;
			this.checkedListBoxBitmaps.FormattingEnabled = true;
			this.checkedListBoxBitmaps.Items.AddRange(new object[] {
            "heightmap",
            "tree positions",
            "tree borders"});
			this.checkedListBoxBitmaps.Location = new System.Drawing.Point(41, 356);
			this.checkedListBoxBitmaps.Margin = new System.Windows.Forms.Padding(15);
			this.checkedListBoxBitmaps.Name = "checkedListBoxBitmaps";
			this.checkedListBoxBitmaps.Size = new System.Drawing.Size(126, 49);
			this.checkedListBoxBitmaps.TabIndex = 92;
			this.checkedListBoxBitmaps.SelectedIndexChanged += new System.EventHandler(this.checkedListBox1_SelectedIndexChanged);
			// 
			// richTextDBH
			// 
			this.richTextDBH.Location = new System.Drawing.Point(823, 11);
			this.richTextDBH.Name = "richTextDBH";
			this.richTextDBH.Size = new System.Drawing.Size(195, 27);
			this.richTextDBH.TabIndex = 95;
			this.richTextDBH.Text = "";
			this.richTextDBH.TextChanged += new System.EventHandler(this.richTextDBH_TextChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(769, 16);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(39, 13);
			this.label7.TabIndex = 96;
			this.label7.Text = "DBH =";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(769, 54);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(38, 13);
			this.label8.TabIndex = 98;
			this.label8.Text = "AGB =";
			// 
			// richTextAGB
			// 
			this.richTextAGB.Location = new System.Drawing.Point(823, 49);
			this.richTextAGB.Name = "richTextAGB";
			this.richTextAGB.Size = new System.Drawing.Size(195, 27);
			this.richTextAGB.TabIndex = 97;
			this.richTextAGB.Text = "";
			this.richTextAGB.TextChanged += new System.EventHandler(this.richTextAGB_TextChanged);
			// 
			// checkedListBoxShape
			// 
			this.checkedListBoxShape.CheckOnClick = true;
			this.checkedListBoxShape.FormattingEnabled = true;
			this.checkedListBoxShape.Items.AddRange(new object[] {
            "tree positions",
            "tree areas"});
			this.checkedListBoxShape.Location = new System.Drawing.Point(41, 287);
			this.checkedListBoxShape.Margin = new System.Windows.Forms.Padding(15);
			this.checkedListBoxShape.Name = "checkedListBoxShape";
			this.checkedListBoxShape.Size = new System.Drawing.Size(126, 34);
			this.checkedListBoxShape.TabIndex = 100;
			this.checkedListBoxShape.SelectedIndexChanged += new System.EventHandler(this.checkedListBoxShape_SelectedIndexChanged);
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(876, 140);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(55, 13);
			this.label9.TabIndex = 105;
			this.label9.Text = "start at tile";
			// 
			// textStartTile
			// 
			this.textStartTile.Location = new System.Drawing.Point(956, 137);
			this.textStartTile.Name = "textStartTile";
			this.textStartTile.Size = new System.Drawing.Size(56, 20);
			this.textStartTile.TabIndex = 106;
			this.textStartTile.Text = "0";
			this.textStartTile.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textStartTile.TextChanged += new System.EventHandler(this.textStartTile_TextChanged);
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(736, 85);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(65, 13);
			this.label11.TabIndex = 110;
			this.label11.Text = "tree radius =";
			// 
			// richTextTreeRadius
			// 
			this.richTextTreeRadius.Location = new System.Drawing.Point(823, 82);
			this.richTextTreeRadius.Name = "richTextTreeRadius";
			this.richTextTreeRadius.Size = new System.Drawing.Size(195, 27);
			this.richTextTreeRadius.TabIndex = 109;
			this.richTextTreeRadius.Text = "";
			this.richTextTreeRadius.TextChanged += new System.EventHandler(this.richTextBoxTreeRadius_TextChanged);
			// 
			// textForestFileFolder
			// 
			this.textForestFileFolder.BackColor = System.Drawing.SystemColors.Window;
			this.textForestFileFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textForestFileFolder.Location = new System.Drawing.Point(147, 20);
			this.textForestFileFolder.Name = "textForestFileFolder";
			this.textForestFileFolder.Size = new System.Drawing.Size(360, 19);
			this.textForestFileFolder.TabIndex = 111;
			this.textForestFileFolder.Text = "forest file - folder path";
			this.textForestFileFolder.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textForestFileFolder.TextChanged += new System.EventHandler(this.TextForestFileFolder_TextChanged);
			// 
			// textForestFileExtension
			// 
			this.textForestFileExtension.BackColor = System.Drawing.SystemColors.Window;
			this.textForestFileExtension.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textForestFileExtension.Location = new System.Drawing.Point(652, 20);
			this.textForestFileExtension.Name = "textForestFileExtension";
			this.textForestFileExtension.Size = new System.Drawing.Size(53, 19);
			this.textForestFileExtension.TabIndex = 112;
			this.textForestFileExtension.Text = ".las";
			this.textForestFileExtension.TextChanged += new System.EventHandler(this.TextForestFileExtension_TextChanged);
			// 
			// trackBarMinTreeHeight
			// 
			this.trackBarMinTreeHeight.AutoSize = false;
			this.trackBarMinTreeHeight.LargeChange = 1;
			this.trackBarMinTreeHeight.Location = new System.Drawing.Point(548, 176);
			this.trackBarMinTreeHeight.Name = "trackBarMinTreeHeight";
			this.trackBarMinTreeHeight.Size = new System.Drawing.Size(140, 30);
			this.trackBarMinTreeHeight.TabIndex = 115;
			this.trackBarMinTreeHeight.Scroll += new System.EventHandler(this.TrackBarMinTreeHeight_Scroll);
			// 
			// textMinTreeHeight
			// 
			this.textMinTreeHeight.Location = new System.Drawing.Point(652, 151);
			this.textMinTreeHeight.Name = "textMinTreeHeight";
			this.textMinTreeHeight.ReadOnly = true;
			this.textMinTreeHeight.Size = new System.Drawing.Size(40, 20);
			this.textMinTreeHeight.TabIndex = 114;
			this.textMinTreeHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label12.Location = new System.Drawing.Point(555, 153);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(91, 13);
			this.label12.TabIndex = 113;
			this.label12.Text = "min tree height";
			// 
			// trackBarMinTreePoints
			// 
			this.trackBarMinTreePoints.AutoSize = false;
			this.trackBarMinTreePoints.LargeChange = 1;
			this.trackBarMinTreePoints.Location = new System.Drawing.Point(545, 120);
			this.trackBarMinTreePoints.Maximum = 200;
			this.trackBarMinTreePoints.Minimum = 1;
			this.trackBarMinTreePoints.Name = "trackBarMinTreePoints";
			this.trackBarMinTreePoints.Size = new System.Drawing.Size(143, 30);
			this.trackBarMinTreePoints.SmallChange = 5;
			this.trackBarMinTreePoints.TabIndex = 131;
			this.trackBarMinTreePoints.TickFrequency = 15;
			this.trackBarMinTreePoints.Value = 2;
			this.trackBarMinTreePoints.Scroll += new System.EventHandler(this.trackBarMinTreePoints_Scroll);
			// 
			// textMinTreePoints
			// 
			this.textMinTreePoints.Location = new System.Drawing.Point(652, 95);
			this.textMinTreePoints.Name = "textMinTreePoints";
			this.textMinTreePoints.ReadOnly = true;
			this.textMinTreePoints.Size = new System.Drawing.Size(40, 20);
			this.textMinTreePoints.TabIndex = 130;
			this.textMinTreePoints.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label17.Location = new System.Drawing.Point(552, 96);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(90, 13);
			this.label17.TabIndex = 129;
			this.label17.Text = "min tree points";
			// 
			// CMainForm
			// 
			this.BackColor = System.Drawing.SystemColors.MenuBar;
			this.ClientSize = new System.Drawing.Size(1034, 481);
			this.Controls.Add(this.trackBarMinTreePoints);
			this.Controls.Add(this.textMinTreePoints);
			this.Controls.Add(this.label17);
			this.Controls.Add(this.trackBarMinTreeHeight);
			this.Controls.Add(this.textMinTreeHeight);
			this.Controls.Add(this.label12);
			this.Controls.Add(this.textForestFileExtension);
			this.Controls.Add(this.textForestFileFolder);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.richTextTreeRadius);
			this.Controls.Add(this.textStartTile);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.checkBoxAGB);
			this.Controls.Add(this.checkBoxDBH);
			this.Controls.Add(this.checkBoxExportLas);
			this.Controls.Add(this.checkedListBoxShape);
			this.Controls.Add(this.checkBoxExportShape);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.richTextAGB);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.richTextDBH);
			this.Controls.Add(this.checkBoxDeleteTmp);
			this.Controls.Add(this.checkBoxPreprocess);
			this.Controls.Add(this.checkedListBoxBitmaps);
			this.Controls.Add(this.trackBarTileSize);
			this.Controls.Add(this.textTileSize);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.textShapefile);
			this.Controls.Add(this.btnShapefile);
			this.Controls.Add(this.comboBoxSplitMode);
			this.Controls.Add(this.labelRangeYval);
			this.Controls.Add(this.labelRangeXval);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textRangeYmax);
			this.Controls.Add(this.textRangeXmax);
			this.Controls.Add(this.textTmpFolder);
			this.Controls.Add(this.btnTmpFolder);
			this.Controls.Add(this.textLasTools);
			this.Controls.Add(this.btnLasTools);
			this.Controls.Add(this.trackBarRangeYmax);
			this.Controls.Add(this.trackBarRangeYmin);
			this.Controls.Add(this.textRangeYmin);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.trackBarRangeXmax);
			this.Controls.Add(this.trackBarRangeXmin);
			this.Controls.Add(this.textRangeXmin);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.checkBoxExportBitmap);
			this.Controls.Add(this.checkBoxAutoTreeHeight);
			this.Controls.Add(this.btnOpenResult);
			this.Controls.Add(this.trackBarAvgTreeHeight);
			this.Controls.Add(this.textAvgTreeHeight);
			this.Controls.Add(this.labelAvgTreeHeight);
			this.Controls.Add(this.trackBarTreeExtentMultiply);
			this.Controls.Add(this.textTreeExtentMultiply);
			this.Controls.Add(this.labelTreeExtentMultiply);
			this.Controls.Add(this.trackBarTreeExtent);
			this.Controls.Add(this.textTreeExtent);
			this.Controls.Add(this.labelTreeExtent);
			this.Controls.Add(this.btnToggleConsole);
			this.Controls.Add(this.btnAbort);
			this.Controls.Add(this.textProgress);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.textOutputFolder);
			this.Controls.Add(this.btnOutputFolder);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.textForestFileName);
			this.Controls.Add(this.btnSellectForest);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "CMainForm";
			this.Text = "ForestReco";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CMainForm_FormClosing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtent)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTreeExtentMultiply)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarAvgTreeHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeXmin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeXmax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeYmax)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarRangeYmin)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarTileSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarMinTreeHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBarMinTreePoints)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}


		#endregion

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			//without this the console is not closed and process remains alive in task manager
			Environment.Exit(0);
		}

		#region path selection
		private void textOutputFolder_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.outputFolderPath, textOutputFolder.Text);
		}

		private void btnOutputFolder_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFolder(textOutputFolder);
		}

		//private void btnSellectReftreeFodlers_Click(object sender, EventArgs e)
		//{
		//	pathSelection.SelectFolder(textReftreeFolder);
		//}

		//private void textReftreeFolder_TextChanged(object sender, EventArgs e)
		//{
		//	CParameterSetter.SetParameter(ESettings.reftreeFolderPath, textReftreeFolder.Text);
		//}

		private void btnSellectForest_Click(object sender, EventArgs e)
		{
			//pathSelection.SelectFile(textForestFileName, "Select forest file", new List<string>() { "las", "laz" }, "forest");

			pathSelection.SelectFile(textForestFileFolder, textForestFileName, textForestFileExtension,
				"Select forest file", new List<string>() { "las", "laz" }, "forest");
		}

		//private void btnSequence_Click(object sender, EventArgs e)
		//{
		//	pathSelection.SelectFile(textForestFileName, "Select sequence config", "seq", "sequence");
		//}

		private void textForestFileName_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.forestFileName, textForestFileName.Text);
			UpdateForestFile();
		}

		private void TextForestFileFolder_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.forestFileFolder, textForestFileFolder.Text);
			UpdateForestFile();
		}

		private void TextForestFileExtension_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.forestFileExtension, textForestFileExtension.Text);
			UpdateForestFile();
		}

		private void UpdateForestFile()
		{
			string folder = CParameterSetter.GetStringSettings(ESettings.forestFileFolder) + "\\";
			string name = CParameterSetter.GetStringSettings(ESettings.forestFileName);
			string extension = CParameterSetter.GetStringSettings(ESettings.forestFileExtension);

			string fullFilePath = folder + name + extension;
			CParameterSetter.SetParameter(ESettings.forestFileFullName, fullFilePath);

			string infoFileName = CUtils.GetFileName(fullFilePath) + "_i.txt";
			string infoFilePath = CPreprocessController.currentTmpFolder + infoFileName;

			string[] lines = CPreprocessController.GetHeaderLines(fullFilePath, infoFilePath);

			if(lines == null)
				return;

			if(CSequenceController.IsSequence())
				return;

			CProjectData.sourceFileHeader = new CHeaderInfo(lines);
			RefreshEstimatedSize();

			//dont update if not inited yet
			rangeController?.UpdateRangeBounds();
		}


		//private void buttonAnalytics_Click(object sender, EventArgs e)
		//{
		//	pathSelection.SelectFile(textForestFileName, "Select analytics file (CSV)", "csv", "csv");
		//}

		//private void textAnalyticsFile_TextChanged(object sender, EventArgs e)
		//{
		//	CParameterSetter.SetParameter(ESettings.analyticsFilePath, textAnalyticsFile.Text);
		//}

		
		private void btnLasTools_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFolder(textLasTools);
		}

		private void textLasTools_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.lasToolsFolderPath, textLasTools.Text);
		}

		private void btnTmpFolder_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFolder(textTmpFolder);
		}

		private void textTmpFolder_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.tmpFilesFolderPath, textTmpFolder.Text);
		}


		#endregion

		private void RefreshEstimatedSize()
		{
			//CResultSize.WriteEstimatedSize(textBoxEstimatedSize, textBoxPartitionSize);
		}

		private void trackBarPartition_Scroll(object sender, EventArgs e)
		{
			if(blockRecursion)
			{ return; }
			//trackValue = trackBarPartition.Value;
			if(trackValue % smallChangeValue != 0)
			{
				trackValue = trackValue / smallChangeValue * smallChangeValue;

				blockRecursion = true;
				//trackBarPartition.Value = trackValue;
				blockRecursion = false;
			}

			//textPartition.Text = trackBarPartition.Value + " m";
			//CParameterSetter.SetParameter(ESettings.partitionStep, trackBarPartition.Value);
			RefreshEstimatedSize();
		}

		//snap to multiply of 5 implementation
		private bool blockRecursion;
		private int smallChangeValue = 5;
		private int trackValue;
		private void trackBarGroundArrayStep_Scroll(object sender, EventArgs e)
		{
			if(blockRecursion)
			{ return; }
			//trackValue = trackBarGroundArrayStep.Value;
			if(trackValue % smallChangeValue != 0)
			{
				trackValue = trackValue / smallChangeValue * smallChangeValue;

				blockRecursion = true;
				//trackBarGroundArrayStep.Value = trackValue;
				blockRecursion = false;
			}

			//float value = trackBarGroundArrayStep.Value / 10f;

			//textGroundArrayStep.Text = value.ToString("0.0") + " m";
			//CParameterSetter.SetParameter(ESettings.groundArrayStep, value);
		}

		private void trackBarTreeExtent_Scroll(object sender, EventArgs e)
		{
			float value = trackBarTreeExtent.Value / 10f;
			textTreeExtent.Text = value.ToString("0.0") + " m";
			CParameterSetter.SetParameter(ESettings.treeExtent, value);
		}

		private void trackBarTreeExtentMultiply_Scroll(object sender, EventArgs e)
		{
			float value = trackBarTreeExtentMultiply.Value / 10f;
			textTreeExtentMultiply.Text = value.ToString("0.0");
			CParameterSetter.SetParameter(ESettings.treeExtentMultiply, value);
		}

		private void trackBarAvgTreeHeight_Scroll(object sender, EventArgs e)
		{
			textAvgTreeHeight.Text = trackBarAvgTreeHeight.Value + " m";
			CParameterSetter.SetParameter(
				ESettings.avgTreeHeigh, trackBarAvgTreeHeight.Value);
		}

		#region checkboxes
		private void checkBoxExportTreeStructures_CheckedChanged(object sender, EventArgs e)
		{
			//CParameterSetter.SetParameter(ESettings.exportTreeStructures,
				//checkBoxExportTreeStructures.Checked);

			RefreshEstimatedSize();
		}

		private void checkBoxExportInvalidTrees_CheckedChanged(object sender, EventArgs e)
		{
			//CParameterSetter.SetParameter(ESettings.exportInvalidTrees,
				//checkBoxExportInvalidTrees.Checked);
		}

		private void checkBoxExportRefTrees_CheckedChanged(object sender, EventArgs e)
		{
			//CParameterSetter.SetParameter(ESettings.exportRefTrees, checkBoxExportRefTrees.Checked);
			RefreshEstimatedSize();
		}

		private void checkBoxExportCheckTrees_CheckedChanged(object sender, EventArgs e)
		{
			//CParameterSetter.SetParameter(ESettings.exportCheckTrees,
				//checkBoxExportCheckTrees.Checked);
		}

		

		private void checkBoxAssignRefTreesRandom_CheckedChanged(object sender, EventArgs e)
		{
			//CParameterSetter.SetParameter(ESettings.assignRefTreesRandom,
				//checkBoxAssignRefTreesRandom.Checked);
		}

		private void checkBoxReducedReftrees_CheckedChanged(object sender, EventArgs e)
		{
			//CParameterSetter.SetParameter(ESettings.useReducedReftreeModels,
				//checkBoxReducedReftrees.Checked);
		}

		private void checkBoxFilterPoints_CheckedChanged(object sender, EventArgs e)
		{
			//CParameterSetter.SetParameter(ESettings.filterPoints,
				//checkBoxFilterPoints.Checked);
		}

		//private void checkBoxExportPoints_CheckedChanged(object sender, EventArgs e)
		//{
		//	CParameterSetter.SetParameter(ESettings.exportPoints, checkBoxExportPoints.Checked);
		//}

		//private void checkBoxExportTreeBoxes_CheckedChanged(object sender, EventArgs e)
		//{
		//	CParameterSetter.SetParameter(ESettings.exportTreeBoxes, checkBoxExportTreeBoxes.Checked);
		//	RefreshEstimatedSize();
		//}


		private void checkBoxAutoTreeHeight_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.autoAverageTreeHeight, checkBoxAutoTreeHeight.Checked);

			trackBarAvgTreeHeight.Enabled = !checkBoxAutoTreeHeight.Checked;
			trackBarAvgTreeHeight.BackColor = checkBoxAutoTreeHeight.Checked ?
				System.Drawing.Color.Gray : trackBarMinTreeHeight.BackColor; //dont know color code of 'enabled color'
		}

		private void checkBoxColorTrees_CheckedChanged(object sender, EventArgs e)
		{
			//CParameterSetter.SetParameter(ESettings.colorTrees, checkBoxColorTrees.Checked);

		}

		private void checkBoxExort3d_CheckedChanged(object sender, EventArgs e)
		{
			//CParameterSetter.SetParameter(ESettings.export3d, checkBoxExport3d.Checked);

			//SetExport3DchekboxesEnabled(checkBoxExport3d.Checked);
		}

		#endregion

		//private void SetExport3DchekboxesEnabled(bool pValue)
		//{
		//	checkBoxExportTreeStructures.Enabled = pValue;
		//	checkBoxExportTreeBoxes.Enabled = pValue;
		//	checkBoxExportInvalidTrees.Enabled = pValue;
		//	checkBoxExportRefTrees.Enabled = pValue;
		//	checkBoxExportPoints.Enabled = pValue;
		//	checkBoxExportCheckTrees.Enabled = pValue;
		//}

		private void checkBoxExportBitmap_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(
				ESettings.exportBitmap, checkBoxExportBitmap.Checked);
			checkedListBoxBitmaps.Enabled = checkBoxExportBitmap.Checked;
		}

		public void SetStartBtnEnabled(bool pValue)
		{
			btnStart.Enabled = pValue;
			btnAbort.Enabled = !pValue;
			//btnClearTmpFolder.Enabled = pValue;
		}

		private void btnOpenResult_Click(object sender, EventArgs e)
		{
			string folderPath = CProjectData.outputFolder;

			if(string.IsNullOrEmpty(folderPath))
				return;
			if(!Directory.Exists(folderPath))
				return;

			Process.Start(folderPath);
		}


		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			EProcessResult processResult = CProgramStarter.Start();
			switch(processResult)
			{
				//case EProcessResult.Exception:
				case EProcessResult.Cancelled:
					e.Cancel = true;
					break;
			}
		}

		// This event handler updates the progress.
		private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			//happened once...dont know how, cant go back in call stack
			if(e.ProgressPercentage < 0)
				return;

			progressBar.Value = e.ProgressPercentage;

			string[] results = null;
			try
			{
				results = (string[])e.UserState;
			}
			catch(Exception ex)
			{
				CDebug.Error("backgroundWorker exception. " + ex.Message);
				results = new string[1] { ex.Message };
			}

			if(results == null)
			{ return; }

			string resultText = "";
			for(int i = 0; i < results.Length; i++)
			{
				resultText += results[i] + Environment.NewLine;
			}
			if(resultText.Length > 0)
			{
				textProgress.Text = resultText;
			}
		}

		// This event handler deals with the results of the background operation.
		private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			SetStartBtnEnabled(true);

			if(e.Cancelled)
			{
				//CDebug.Step(EProgramStep.Cancelled); //cant call from this thread!
				textProgress.Text = "CANCELLED";
			}

			//ERROR and DONE messages should be handelend during the process. no need to write to textProgress
			else if(e.Error != null)
			{
				CDebug.WriteLine("Error: " + e.Error.Message);
			}
			else
			{
				CDebug.WriteLine("Done!");
			}
		}

		private void buttonTest1_Click(object sender, EventArgs e)
		{
			CDebug.WriteLine("TEST", true, true);
			CRigidTransform rigTransform;
			List<Vector3> setA = new List<Vector3>()
			{
				new Vector3(1,0,0),
				new Vector3(0,1,0),
				new Vector3(0,0,1),
			};
			List<Vector3> setB = new List<Vector3>()
			{
				new Vector3(-1,0,0),
				new Vector3(0,1,0),
				new Vector3(0,0,-1),
			};
			//CBallsTransformator.GetRigidTransform(setA, setB);

			//////////////

			//works in python - ok
			setA = new List<Vector3>()
			{
				new Vector3(0,2,0),
				new Vector3(1,0,0),
				new Vector3(0,0,1),
			};
			setB = new List<Vector3>()
			{
				new Vector3(2,-1,0),
				new Vector3(0,-2,0),
				new Vector3(0,-1,1),
			};
			//CBallsTransformator.GetRigidTransform(setA, setB);		
			//
			
			setA = new List<Vector3>()
			{
				new Vector3(1,0,0),
				new Vector3(0,1,0),
				new Vector3(0,0,1),
			};
			setB = new List<Vector3>()
			{
				new Vector3(1,0,0),
				new Vector3(0,1,0),
				new Vector3(0,0,1),
			};
			CUtils.MovePointsBy(ref setB, Vector3.One);
			//CBallsTransformator.GetRigidTransform(setA, setB);

			//////////////

			setA = new List<Vector3>()
			{
				new Vector3(1,3,0),
				new Vector3(2,1,0),
				new Vector3(1,1,1),
				new Vector3(0,1,0),
			};
			setB = new List<Vector3>()
			{
				new Vector3(3,-1,0),
				new Vector3(1,-1,1),
				new Vector3(1,0,0),
				new Vector3(1,-1,-1),
			};
			CBallsTransformator.GetRigidTransform(setA, setB);

			setA = new List<Vector3>()
			{
				new Vector3(1,3,0),
				new Vector3(2,1,0),
				new Vector3(1,1,1),
				new Vector3(0,1,0),
			};
			setB = new List<Vector3>()
			{
				new Vector3(1,-1,-1),
				new Vector3(3,-1,0),
				new Vector3(1,-1,1),
				new Vector3(1,0,0),
			};
			CBallsTransformator.GetRigidTransform(setA, setB);
			return;


			//////////////

			setA = new List<Vector3>()
			{
				new Vector3(1,3,0),
				new Vector3(2,1,0),
				new Vector3(1,1,1),
			};
			setB = new List<Vector3>()
			{
				new Vector3(3,-1,0),
				new Vector3(1,-1,1),
				new Vector3(1,0,0),
			};
			rigTransform = CBallsTransformator.GetRigidTransform(setA, setB);
			return;
			//////////////
			setA = new List<Vector3>()
			{
				new Vector3(1,3,0),
				new Vector3(2,1,0),
				new Vector3(1,1,1),
			};
			setB = new List<Vector3>()
			{
				new Vector3(3,-1,0),
				new Vector3(1,-1,1),
				new Vector3(1,0,0),
			};
			CBallsTransformator.GetRigidTransform(setA, setB);
			//////////////
			setA = new List<Vector3>()
			{
				new Vector3(1,3,0),
				new Vector3(2,1,0),
				new Vector3(1,1,1),
			};
			setB = new List<Vector3>()
			{
				new Vector3(1,-1,1),
				new Vector3(3,-1,0),
				new Vector3(1,0,0),
			};
			CBallsTransformator.GetRigidTransform(setA, setB);
			//////////////
			setA = new List<Vector3>()
			{
				new Vector3(1,3,0),
				new Vector3(2,1,0),
				new Vector3(1,1,1),
			};
			setB = new List<Vector3>()
			{
				new Vector3(1,-1,1),
				new Vector3(1,0,0),
				new Vector3(3,-1,0),
			};
			CBallsTransformator.GetRigidTransform(setA, setB);

		}

		#region range
		private void trackBarRangeXmin_Scroll(object sender, EventArgs e)
		{
			rangeController.trackBarRangeXmin_Scroll();
		}

		private void trackBarRangeXmax_Scroll(object sender, EventArgs e)
		{
			rangeController.trackBarRangeXmax_Scroll();
		}

		private void trackBarRangeYmin_Scroll(object sender, EventArgs e)
		{
			rangeController.trackBarRangeYmin_Scroll();
		}

		private void trackBarRangeYmax_Scroll(object sender, EventArgs e)
		{
			rangeController.trackBarRangeYmax_Scroll();
		}

		private void textRangeXmin_LostFocus(object sender, EventArgs e)
		{
			rangeController.textRange_LostFocus(trackBarRangeXmin, ESettings.rangeXmax, textRangeXmin.Text);
		}

		private void textRangeXmax_LostFocus(object sender, EventArgs e)
		{
			rangeController.textRange_LostFocus(trackBarRangeXmax, ESettings.rangeXmin, textRangeXmax.Text);
		}

		private void textRangeYmin_LostFocus(object sender, EventArgs e)
		{
			rangeController.textRange_LostFocus(trackBarRangeYmin, ESettings.rangeYmax, textRangeYmin.Text);
		}

		private void textRangeYmax_LostFocus(object sender, EventArgs e)
		{
			rangeController.textRange_LostFocus(trackBarRangeYmax, ESettings.rangeYmin, textRangeYmax.Text);
		}


		private void comboBoxSplitMode_SelectedIndexChanged(object sender, EventArgs e)
		{
			rangeController.comboBoxSplitMode_SelectedIndexChanged(comboBoxSplitMode.SelectedItem.ToString());
			RefreshEstimatedSize();
		}

		private void btnShapefile_Click(object sender, EventArgs e)
		{
			pathSelection.SelectFile(textShapefile, "Select shape file", new List<string>() { "shp" }, "shapefile");
		}

		private void textShapefile_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.shapeFilePath, textShapefile.Text);
		}

		#endregion

		//private void btnForestFolder_Click(object sender, EventArgs e)
		//{
		//	pathSelection.SelectFolder(textForestFolder);
		//}
		//private void textForestFolder_TextChanged(object sender, EventArgs e)
		//{
		//	CParameterSetter.SetParameter(ESettings.forestFolderPath, textForestFolder.Text);
		//}

		//private void btnMergeForestFolder_Click(object sender, EventArgs e)
		//{
		//	DateTime start = DateTime.Now;
		//	string mergedFilePath = "";
		//	textProgress.Text = "Merge started. please wait";

		//	try
		//	{
		//		mergedFilePath = CPreprocessController.ProcessForestFolder();
		//		textProgress.Text = "merge complete";
		//	}
		//	catch(Exception ex)
		//	{
		//		CDebug.Error($"merged file not created. {ex}");
		//	}


		//	if(File.Exists(mergedFilePath))
		//	{
		//		textForestFileName.Text = mergedFilePath;
		//		CDebug.WriteLine($"Merge successful. forest file set to: {mergedFilePath}");
		//	}
		//	else
		//	{
		//		CDebug.Error($"Error. forest file: {mergedFilePath} not created");
		//	}
		//}

		private void trackBarTileSize_Scroll(object sender, EventArgs e)
		{
			if(blockRecursion)
				return;

			trackValue = trackBarTileSize.Value;
			if(trackValue % smallChangeValue != 0)
			{
				trackValue = trackValue / smallChangeValue * smallChangeValue;

				blockRecursion = true;
				trackBarTileSize.Value = trackValue;
				blockRecursion = false;
			}

			textTileSize.Text = trackBarTileSize.Value + " m";
			CParameterSetter.SetParameter(ESettings.tileSize, trackBarTileSize.Value);
		}

		private void textRangeXmax_TextChanged(object sender, EventArgs e)
		{

		}

		private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.ExportBMHeightmap,
				checkedListBoxBitmaps.GetItemCheckState(0) == CheckState.Checked);
			CParameterSetter.SetParameter(ESettings.ExportBMTreePositions,
				checkedListBoxBitmaps.GetItemCheckState(1) == CheckState.Checked);
			CParameterSetter.SetParameter(ESettings.ExportBMTreeBorders,
				checkedListBoxBitmaps.GetItemCheckState(2) == CheckState.Checked);
		}

		private void checkBoxPreprocess_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.preprocess, checkBoxPreprocess.Checked);
		}

		private void checkBoxDeleteTmp_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.deleteTmp, checkBoxDeleteTmp.Checked);

		}

		private void richTextDBH_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.dbh, richTextDBH.Text);
		}

		private void richTextAGB_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.agb, richTextAGB.Text);
		}

		private void checkBoxExportShape_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportShape, checkBoxExportShape.Checked);
			checkedListBoxShape.Enabled = checkBoxExportShape.Checked;
		}

		private void checkedListBoxShape_SelectedIndexChanged(object sender, EventArgs e)
		{
			CheckState treePosState = checkedListBoxShape.GetItemCheckState(0);
			CDebug.WriteLine($"treePosState = {treePosState}");
			CParameterSetter.SetParameter(ESettings.exportShapeTreePositions, treePosState == CheckState.Checked);

			CheckState treeAreasState = checkedListBoxShape.GetItemCheckState(1);
			CDebug.WriteLine($"treeAreasState = {treeAreasState}");
			CParameterSetter.SetParameter(ESettings.exportShapeTreeAreas, treeAreasState == CheckState.Checked);
		}

		private void checkBoxExportLas_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.exportLas, checkBoxExportLas.Checked);
		}

		private void checkBoxDBH_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.calculateDBH, checkBoxDBH.Checked);
			richTextDBH.Enabled = checkBoxDBH.Checked;
		}

		private void checkBoxAGB_CheckedChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.calculateAGB, checkBoxAGB.Checked);
			richTextAGB.Enabled = checkBoxAGB.Checked;
		}

		/// <summary>
		/// Deleted all files and folders in tmp folder and creates the folder again.
		/// </summary>
		private void btnClearTmpFolder_Click(object sender, EventArgs e)
		{
			string tmpFolderPath = CParameterSetter.GetStringSettings(ESettings.tmpFilesFolderPath);
			if(Directory.Exists(tmpFolderPath))
			{
				try
				{
					textProgress.Text = "Tmp files cleared";
					Directory.Delete(tmpFolderPath, true);
					Directory.CreateDirectory(tmpFolderPath);
				}
				catch(Exception)
				{

				}
			}
			else if(tmpFolderPath.Length > 0)
			{
				Directory.CreateDirectory(tmpFolderPath);
			}
		}

		private void textStartTile_TextChanged(object sender, EventArgs e)
		{
			int val = 0;
			int.TryParse(textStartTile.Text, out val);
			textStartTile.Text = val.ToString();
			CParameterSetter.SetParameter(ESettings.startIndex, val);
		}

		//private void comboBoxDetectMethod_SelectedIndexChanged(object sender, EventArgs e)
		//{
		//	CParameterSetter.SetParameter(ESettings.detectMethod, comboBoxDetectMethod.SelectedIndex);
		//	detectionMethod.OnSelectDetectMethod(CTreeManager.GetDetectMethod());
		//}

		private void richTextBoxTreeRadius_TextChanged(object sender, EventArgs e)
		{
			CParameterSetter.SetParameter(ESettings.treeRadius, richTextTreeRadius.Text);
			CTreeRadiusCalculator.NeedsReinit = true;
			CTooltipManager.AssignTooltip(myToolTip, richTextTreeRadius, ESettings.treeRadius);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			//the global settings is stored in:
			//C:\Users\ja004\AppData\Local\ForestReco\ForestReco.exe_Url_wod4dcapztryfivsknzgz2tg0wxqxsmy\1.0.0.0
			//local in:
			//C:\Coding\ForestReco2\ForestReco\bin\x64\Debug
			//needs to upgrade
			//https://stackoverflow.com/questions/1054422/why-are-my-application-settings-not-getting-persisted
			Properties.Settings.Default.Upgrade();
			Location = Properties.Settings.Default.formLocation;
		}

		private void CMainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Properties.Settings.Default.formLocation = Location;
			Properties.Settings.Default.Save();
		}

		private void TrackBarMinTreeHeight_Scroll(object sender, EventArgs e)
		{
			int value = trackBarMinTreeHeight.Value;
			textMinTreeHeight.Text = value + " m";
			CParameterSetter.SetParameter(ESettings.minTreeHeight, value);
		}


		private void RefreshTrackBarValue(TrackBar pTrackBar, TextBox pText, ESettings pSettings, bool pFloat)
		{
			float value = pFloat ? pTrackBar.Value / 10f : pTrackBar.Value;
			pText.Text = pFloat ? value.ToString("0.0") + " m" : value.ToString();

			//for some reason cant use ? notation
			if(!pFloat)
				CParameterSetter.SetParameter(pSettings, (int)value);
			else
				CParameterSetter.SetParameter(pSettings, value);
		}


		private void trackBarMinTreePoints_Scroll(object sender, EventArgs e)
		{
			RefreshTrackBarValue(trackBarMinTreePoints, textMinTreePoints, 
				ESettings.minTreePoints, false);
		}
	}
}
