﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ForestReco.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D:\\Resources\\ForestReco\\podklady\\data-small\\TXT\\BK_1000AGL_59_72_97_x90_y62.txt")]
        public string forestFilePath {
            get {
                return ((string)(this["forestFilePath"]));
            }
            set {
                this["forestFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string reftreeFolderPath {
            get {
                return ((string)(this["reftreeFolderPath"]));
            }
            set {
                this["reftreeFolderPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string outputFolderPath {
            get {
                return ((string)(this["outputFolderPath"]));
            }
            set {
                this["outputFolderPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool consoleVisible {
            get {
                return ((bool)(this["consoleVisible"]));
            }
            set {
                this["consoleVisible"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("30")]
        public int partitionStep {
            get {
                return ((int)(this["partitionStep"]));
            }
            set {
                this["partitionStep"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string checkTreeFilePath {
            get {
                return ((string)(this["checkTreeFilePath"]));
            }
            set {
                this["checkTreeFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public float groundArrayStep {
            get {
                return ((float)(this["groundArrayStep"]));
            }
            set {
                this["groundArrayStep"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public float treeExtent {
            get {
                return ((float)(this["treeExtent"]));
            }
            set {
                this["treeExtent"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1.5")]
        public float treeExtentMultiply {
            get {
                return ((float)(this["treeExtentMultiply"]));
            }
            set {
                this["treeExtentMultiply"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("15")]
        public int avgTreeHeigh {
            get {
                return ((int)(this["avgTreeHeigh"]));
            }
            set {
                this["avgTreeHeigh"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool exportTreeStructures {
            get {
                return ((bool)(this["exportTreeStructures"]));
            }
            set {
                this["exportTreeStructures"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool exportInvalidTrees {
            get {
                return ((bool)(this["exportInvalidTrees"]));
            }
            set {
                this["exportInvalidTrees"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool exportRefTrees {
            get {
                return ((bool)(this["exportRefTrees"]));
            }
            set {
                this["exportRefTrees"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool assignRefTreesRandom {
            get {
                return ((bool)(this["assignRefTreesRandom"]));
            }
            set {
                this["assignRefTreesRandom"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool useCheckTreeFile {
            get {
                return ((bool)(this["useCheckTreeFile"]));
            }
            set {
                this["useCheckTreeFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool exportCheckTrees {
            get {
                return ((bool)(this["exportCheckTrees"]));
            }
            set {
                this["exportCheckTrees"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool useReducedReftreeModels {
            get {
                return ((bool)(this["useReducedReftreeModels"]));
            }
            set {
                this["useReducedReftreeModels"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool filterPoints {
            get {
                return ((bool)(this["filterPoints"]));
            }
            set {
                this["filterPoints"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool exportPoints {
            get {
                return ((bool)(this["exportPoints"]));
            }
            set {
                this["exportPoints"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool autoAverageTreeHeight {
            get {
                return ((bool)(this["autoAverageTreeHeight"]));
            }
            set {
                this["autoAverageTreeHeight"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool exportTreeBoxes {
            get {
                return ((bool)(this["exportTreeBoxes"]));
            }
            set {
                this["exportTreeBoxes"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool colorTrees {
            get {
                return ((bool)(this["colorTrees"]));
            }
            set {
                this["colorTrees"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool export3d {
            get {
                return ((bool)(this["export3d"]));
            }
            set {
                this["export3d"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string analyticsFilePath {
            get {
                return ((string)(this["analyticsFilePath"]));
            }
            set {
                this["analyticsFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool exportBitmap {
            get {
                return ((bool)(this["exportBitmap"]));
            }
            set {
                this["exportBitmap"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D:\\Resources\\ForestReco\\podklady\\data-small\\tmpFiles")]
        public string tmpFilesFolderPath {
            get {
                return ((string)(this["tmpFilesFolderPath"]));
            }
            set {
                this["tmpFilesFolderPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int rangeXmin {
            get {
                return ((int)(this["rangeXmin"]));
            }
            set {
                this["rangeXmin"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int rangeXmax {
            get {
                return ((int)(this["rangeXmax"]));
            }
            set {
                this["rangeXmax"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int rangeYmin {
            get {
                return ((int)(this["rangeYmin"]));
            }
            set {
                this["rangeYmin"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int rangeYmax {
            get {
                return ((int)(this["rangeYmax"]));
            }
            set {
                this["rangeYmax"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string lasToolsFolderPath {
            get {
                return ((string)(this["lasToolsFolderPath"]));
            }
            set {
                this["lasToolsFolderPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string forestFolderPath {
            get {
                return ((string)(this["forestFolderPath"]));
            }
            set {
                this["forestFolderPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int currentSplitMode {
            get {
                return ((int)(this["currentSplitMode"]));
            }
            set {
                this["currentSplitMode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string shapeFilePath {
            get {
                return ((string)(this["shapeFilePath"]));
            }
            set {
                this["shapeFilePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("50")]
        public int tileSize {
            get {
                return ((int)(this["tileSize"]));
            }
            set {
                this["tileSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ExportBMTreePositions {
            get {
                return ((bool)(this["ExportBMTreePositions"]));
            }
            set {
                this["ExportBMTreePositions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ExportBMHeightmap {
            get {
                return ((bool)(this["ExportBMHeightmap"]));
            }
            set {
                this["ExportBMHeightmap"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ExportBMTreeBorders {
            get {
                return ((bool)(this["ExportBMTreeBorders"]));
            }
            set {
                this["ExportBMTreeBorders"] = value;
            }
        }
    }
}
