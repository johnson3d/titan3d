using System.Text.Json.Serialization;

namespace BrickManager
{
    public class BrickNugetInfo
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("Version")]
        public string Version { get; set; } = string.Empty;
    }

    public class BrickInfo
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("Path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("FullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("SharedProjects")]
        public List<string> SharedProjects { get; set; } = new List<string>();

        [JsonPropertyName("Nugets")]
        public List<BrickNugetInfo> Nugets { get; set; } = new List<BrickNugetInfo>();

        [JsonPropertyName("DllModules")]
        public List<string> DllModules { get; set; } = new List<string>();

        /// <summary>
        /// The absolute file path of this .brick file on disk
        /// </summary>
        [JsonIgnore]
        public string AbsoluteFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Whether this brick is selected by the user in the UI
        /// </summary>
        [JsonIgnore]
        public bool IsSelected { get; set; } = false;
    }
}
