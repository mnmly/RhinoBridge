﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using bridge_c_sharp_plugin;
using Rhino;
using Rhino.UI;
using RhinoBridge.Converters;
using RhinoBridge.DataAccess;
using RhinoBridge.Errors;
using RhinoBridge.Settings;
using RhinoBridge.UI.Views;

namespace RhinoBridge
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class RhinoBridgePlugIn : Rhino.PlugIns.PlugIn
    {
        public RhinoBridgePlugIn()
        {
            Instance = this;
        }

        ///<summary>Gets the only instance of the RhinoBridgePlugIn plug-in.</summary>
        public static RhinoBridgePlugIn Instance
        {
            get; private set;
        }

        #region Settings

        /// <summary>
        /// The unit system used by quixel fbx files
        /// </summary>
        public const UnitSystem FBX_UNIT_SYSTEM = UnitSystem.Centimeters;

        #region Port
        private const int DEFAULT_PORT = 24981;
        private const string PORT_KEY = "PORT";
        /// <summary>
        /// The port the bridge server listens to
        /// </summary>
        public int Port => GetPort();

        /// <summary>
        /// Gets the port from the persistent settings
        /// </summary>
        /// <returns></returns>
        private int GetPort()
        {
            return Settings.TryGetInteger(PORT_KEY, out var port) ? port : DEFAULT_PORT;
        }

        /// <summary>
        /// Set a different port number for the bridge server
        /// </summary>
        /// <param name="port">The port number to use</param>
        public void SetPort(int port)
        {
            // check for bounds that make sense
            if (port < 0 | port > IPEndPoint.MaxPort)
                return;

            // listener might not be initialized if settings change before the server gets started
            if(Listener != null)
                Listener.MessageReceivingPort = port;

            // Store the port to the settings
            Settings.SetInteger(PORT_KEY, port);
        }

        #endregion

        #region Preview geometry

        private const TexturePreviewGeometryType DEFAULT_PREVIEW_TYPE = TexturePreviewGeometryType.Sphere;
        private const string PREVIEW_TYPE_KEY = "PREVIEWTYPE";
        /// <summary>
        /// The type of geometry to use on texture import as a preview
        /// </summary>
        public TexturePreviewGeometryType PreviewType => GetPreviewType();

        /// <summary>
        /// Gets the preview type from the persistent settings
        /// </summary>
        /// <returns></returns>
        private TexturePreviewGeometryType GetPreviewType()
        {
            return Settings.TryGetEnumValue<TexturePreviewGeometryType>(PREVIEW_TYPE_KEY, out var type)
                ? type
                : DEFAULT_PREVIEW_TYPE;
        }

        /// <summary>
        /// Sets a different type of preview geometry to use
        /// </summary>
        /// <param name="type"></param>
        public void SetPreviewType(TexturePreviewGeometryType type)
        {
            // Store the new preview type
            Settings.SetEnumValue(PREVIEW_TYPE_KEY, type);
        }

        #endregion

        #region Import scale

        private const bool DEFAULT_SCALE_BOOL = false;
        private const string DEFAULT_SCALE_KEY = "SHOULDSCALE";
        public bool ShouldScaleMaterials => GetShouldScale();

        private bool GetShouldScale()
        {
            return Settings.TryGetBool(DEFAULT_SCALE_KEY, out var shouldScale) ? shouldScale : DEFAULT_SCALE_BOOL;
        }

        public void SetShouldScale(bool shouldScale)
        {
            // Store the new value
            Settings.SetBool(DEFAULT_SCALE_KEY, shouldScale);
        }

        #endregion

        #region Import geometry flavor

        private const AssetImportGeometryFlavor DEFAULT_ASSET_GEOMETRY_TYPE = AssetImportGeometryFlavor.Block;
        private const string DEFAULT_ASSET_GEOMETRY_TYPE_KEY = "ASSETGEOMETRYTYPE";
        public AssetImportGeometryFlavor AssetGeometryType => GetAssetGeometryType();

        private AssetImportGeometryFlavor GetAssetGeometryType()
        {
            return Settings.TryGetEnumValue<AssetImportGeometryFlavor>(DEFAULT_ASSET_GEOMETRY_TYPE_KEY, out var flavor)
                ? flavor
                : DEFAULT_ASSET_GEOMETRY_TYPE;
        }

        public void SetAssetGeometryType(AssetImportGeometryFlavor type)
        {
            Settings.SetEnumValue(DEFAULT_ASSET_GEOMETRY_TYPE_KEY, type);
        }

        #endregion

        /// <summary>
        /// Restores all settings to their default values
        /// </summary>
        public void RestoreDefaultSettings()
        {
            SetPort(DEFAULT_PORT);
            SetPreviewType(DEFAULT_PREVIEW_TYPE);
            SetShouldScale(DEFAULT_SCALE_BOOL);
            SetAssetGeometryType(DEFAULT_ASSET_GEOMETRY_TYPE);
        }

        #endregion

        #region Server

        /// <summary>
        /// The <see cref="BridgeServer"/> that listens for export events
        /// of Bridge
        /// </summary>
        public BridgeServer Listener { get; set; }

        /// <summary>
        /// Starts the server
        /// </summary>
        public void StartServer()
        {
            // Check if we already have a server running
            if (Listener != null) return;

            // create a new server
            Listener = new BridgeServer
            {
                // use custom port from settings
                MessageReceivingPort = Port
            };

            // Starts the server in background.
            Listener.StartServer();

            // Subscribe to asset import events
            BridgeImporter.Instance.ImportsReceived += BridgeImporterOnRaiseAssetImport;

            // Subscribe to idle app events
            RhinoApp.Idle += RhinoAppOnIdle;
        }

        /// <summary>
        /// Ends the server
        /// </summary>
        public void EndServer()
        {
            // End the server
            Listener?.EndServer();

            // End subscription to asset imports
            BridgeImporter.Instance.ImportsReceived -= BridgeImporterOnRaiseAssetImport;

            // End subscription to idle events
            RhinoApp.Idle -= RhinoAppOnIdle;
        }

        #endregion

        private ImportEventMachine _eventMachine;

        /// <summary>
        /// Handle Asset export events coming from quixel bridge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BridgeImporterOnRaiseAssetImport(object sender, EventArgs e)
        {
            // empty the whole import queue
            while (BridgeImporter.Instance.CanExport)
            {
                try
                {
                    // create a new event machine and assign to backing field
                    _eventMachine = new ImportEventMachine(BridgeImporter.Instance.GetNextAsset());

                    // execute the machine
                    _eventMachine.Execute();
                }
                catch (Exception ex)
                {
                    if (ex is TextureTypeNotImplementedException || ex is AssetTypeNotImplementedException ||
                        ex is GeometryFormatNotImplementedException)
                    {
                        RhinoApp.WriteLine(ex.Message);
                        return;
                    }

                    throw;
                }

                // set machine back to null, so next thread can start working
                _eventMachine = null;
            }
        }

        private void RhinoAppOnIdle(object sender, EventArgs e)
        {
            // nothing to import?
            if (!AssetImportQueue.Instance.CanImport)
            {
                // update the currently active doc
                AssetImportQueue.Instance.UpdateDocument();

                return;
            }

            // import the next asset in the queue
            AssetImportQueue.Instance.ImportNext();
        }


        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and maintain plug-in wide options in a document.
        protected override void OnShutdown()
        {
            // make sure to disable the server
            EndServer();

            base.OnShutdown();
        }

        protected override void OptionsDialogPages(List<OptionsDialogPage> pages)
        {
            var optionsPage = new PluginSettingsPage("RhinoBridge settings");
            pages.Add(optionsPage);
        }
    }
}