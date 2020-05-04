﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using bridge_c_sharp_plugin;
using Rhino;
using Rhino.Commands;
using RhinoBridge.Data;
using RhinoBridge.DataAccess;
using RhinoBridge.Factories;

namespace RhinoBridge.Commands
{
    [CommandStyle(Style.ScriptRunner)]
    public class RhinoBridgeImport3dAsset : Command
    {
        private GeometryInformation[] _geometryInfos;
        private Asset _asset;

        public void SetInfos(IEnumerable<GeometryInformation> infos)
        {
            _geometryInfos = infos.ToArray();
        }

        public void SetAsset(Asset asset)
        {
            _asset = asset;
        }

        static RhinoBridgeImport3dAsset _instance;
        public RhinoBridgeImport3dAsset()
        {
            _instance = this;
        }

        ///<summary>The only instance of the RhinoBridgeImport3dAsset command.</summary>
        public static RhinoBridgeImport3dAsset Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "RhinoBridgeImport3dAsset"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            doc.Views.RedrawEnabled = false;

            var propAccess = new PropData(doc);
            var matAccess = new MaterialData(doc);

            var mat = RenderContentFactory.CreateMaterial(_asset, doc, RhinoBridgePlugIn.FBX_UNIT_SYSTEM);
            matAccess.AddRenderMaterial(mat);

            foreach (var geometryInformation in _geometryInfos)
            {
                propAccess.AddTexturedGeometry(geometryInformation, mat);
            }

            doc.Views.RedrawEnabled = true;

            doc.Views.Redraw();

            return Result.Success;
        }
    }
}