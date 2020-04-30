﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bridge_c_sharp_plugin;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;

namespace RhinoBridge.Converters
{
    /// <summary>
    /// Converts asset to a Rhino friendly format
    /// </summary>
    public static class AssetConverter
    {
        /// <summary>
        /// Converts an asset to a material 
        /// </summary>
        /// <param name="asset">asset to convert</param>
        public static Material Convert(Asset asset)
        {
            var mat = new Material
            {
                Name = asset.name
            };


            // this could be nicer with pbr materials in rhino 7
            foreach (var texture in asset.textures)
            {
                switch (GetTextureType(texture))
                {
                    case TextureType.Albedo:
                        mat.SetBitmapTexture(texture.path);
                        break;
                    case TextureType.AmbientOcclusion:
                        break;
                    case TextureType.Cavity:
                        break;
                    case TextureType.Displacement:
                        break;
                    case TextureType.Gloss:
                        break;
                    case TextureType.Normal:
                        mat.SetBumpTexture(texture.path);
                        break;
                    case TextureType.Roughness:
                        break;
                    case TextureType.Specular:
                        break;
                    case TextureType.Undefined:
                        break;
                }
            }

            mat.DiffuseColor = GetAverageColor(asset);

            return mat;

        }

        /// <summary>
        /// Gets the <see cref="TextureType"/> associated with a given <see cref="bridge_c_sharp_plugin.Texture"/>
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static TextureType GetTextureType(bridge_c_sharp_plugin.Texture texture)
        {
            if (texture.type == "albedo") return TextureType.Albedo;
            if (texture.type == "ao") return TextureType.AmbientOcclusion;
            if (texture.type == "cavity") return TextureType.Cavity;
            if (texture.type == "displacement") return TextureType.Displacement;
            if (texture.type == "gloss") return TextureType.Gloss;
            if (texture.type == "normal") return TextureType.Normal;
            if (texture.type == "roughness") return TextureType.Roughness;
            if (texture.type == "specular") return TextureType.Specular;

            return TextureType.Undefined;
        }

        /// <summary>
        /// Gets the average color of an asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static Color GetAverageColor(Asset asset)
        {
            return ColorTranslator.FromHtml(asset.averageColor);
        }
    }
}
