using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace Getools.Lib.Game.Asset.Model
{
    public static class ModelDataResolver
    {
        private static bool _bboxJsonLoaded = false;
        private static bool _scaleJsonLoaded = false;

        private static ModelBboxDataContainer? _modelBboxData = null;
        private static ModelScaleDataContainer? _modelScaleData = null;

        private static Dictionary<PropId, ModelData> _modelBboxLookup = new Dictionary<PropId, ModelData>();
        private static Dictionary<PropId, Single> _modelScaleLookup = new Dictionary<PropId, Single>();

        public static ModelData GetModelDataFromPropId(PropId propId)
        {
            if (_modelBboxLookup.ContainsKey(propId))
            {
                return _modelBboxLookup[propId].Clone();
            }

            var startName = propId.ToString().ToLower().Substring(5);

            // some special cases. These are not the same as model scale data.
            switch (startName)
            {
                case "icbm": startName = "ICBM"; break;
                case "icbm_nose": startName = "ICBM_nose"; break;
                case "card_box4_lg": startName = "card_box4"; break;
                case "card_box5_lg": startName = "card_box5"; break;
                case "card_box6_lg": startName = "card_box6"; break;
                case "console_sev_gea": startName = "console_sev_GEa"; break;
                case "console_sev_geb": startName = "console_sev_GEb"; break;
            }

            string modelFilename = "P" + startName + "Z";

            if (!_bboxJsonLoaded)
            {
                LoadDefaultBboxModelData();
            }

            var data = _modelBboxData!.Items.FirstOrDefault(x => x.Name == modelFilename);

            if (data == null)
            {
                throw new KeyNotFoundException($"Could not find \"{modelFilename}\" in model data container");
            }

            _modelBboxLookup[propId] = data;

            return data.Clone();
        }

        public static Single GetModelScaleFromPropId(PropId propId)
        {
            if (_modelScaleLookup.ContainsKey(propId))
            {
                return _modelScaleLookup[propId];
            }

            if (!_scaleJsonLoaded)
            {
                LoadDefaultScaleModelData();
            }

            string modelEnumName = propId.ToString().ToLower().Substring(5);

            // some special cases. These are not the same as model bbox data.
            switch (modelEnumName)
            {
                case "icbm": modelEnumName = "ICBM"; break;
                case "icbm_nose": modelEnumName = "ICBM_nose"; break;
                case "console_sev_gea": modelEnumName = "console_sev_GEa"; break;
                case "console_sev_geb": modelEnumName = "console_sev_GEb"; break;
            }

            var data = _modelScaleData!.Items.FirstOrDefault(x => x.Name == modelEnumName);

            if (data == null)
            {
                throw new KeyNotFoundException($"Could not find \"{modelEnumName}\" in model data container");
            }

            _modelScaleLookup[propId] = data.Scale;

            return data.Scale;
        }

        private static void LoadDefaultBboxModelData()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            string resourcePath = "Getools.Lib.Game.Asset.Model.ModelBboxDefinition.json";
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();

                _modelBboxData = JsonConvert.DeserializeObject<ModelBboxDataContainer>(json);
            }

            _bboxJsonLoaded = true;
        }

        private static void LoadDefaultScaleModelData()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Format: "{Namespace}.{Folder}.{filename}.{Extension}"
            string resourcePath = "Getools.Lib.Game.Asset.Model.ModelScaleDefinition.json";
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();

                _modelScaleData = JsonConvert.DeserializeObject<ModelScaleDataContainer>(json);
            }

            _scaleJsonLoaded = true;
        }

        private class ModelBboxDataContainer
        {
            [JsonProperty(PropertyName = "items")]
            public List<ModelData> Items = new List<ModelData>();

            public ModelBboxDataContainer()
            {
            }
        }

        private class ModelScaleDataContainer
        {
            [JsonProperty(PropertyName = "items")]
            public List<ModelScalJson> Items = new List<ModelScalJson>();

            public ModelScaleDataContainer()
            {
            }
        }

        private class ModelScalJson
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "scale")]
            public Single Scale { get; set; }
        }
    }
}
