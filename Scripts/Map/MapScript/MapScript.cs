using System.Collections.Generic;
using Utility;

namespace Map
{
    class MapScript
    {
        protected string HeaderPrefix = "/// ";
        protected char Delimiter = '\n';
        public MapScriptOptions Options  = new MapScriptOptions();
        public MapScriptObjects Objects = new MapScriptObjects();
        public MapScriptEditorOptions EditorOptions = new MapScriptEditorOptions();

        public virtual string Serialize()
        {
            List<string> items = new List<string>();
            items.Add(CreateHeader("Options"));
            items.Add(Options.Serialize());
            items.Add(CreateHeader("Objects"));
            items.Add(Objects.Serialize());
            items.Add(CreateHeader("EditorOptions"));
            items.Add(EditorOptions.Serialize());
            return string.Join(Delimiter.ToString(), items.ToArray());
        }

        private string CreateHeader(string name)
        {
            return HeaderPrefix + name;
        }

        public virtual void Deserialize(string csv)
        {
            if (MapConverter.IsLegacy(csv))
            {
                Objects = MapConverter.Convert(csv).Objects;
                return;
            }
            string[] items = csv.Split(Delimiter);
            List<string> currentSectionItems = new List<string>();
            string currentSection = "";
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].StartsWith(HeaderPrefix))
                {
                    DeserializeSection(currentSection, currentSectionItems);
                    currentSection = items[i].Substring(HeaderPrefix.Length);
                    currentSectionItems.Clear();
                }
                else
                    currentSectionItems.Add(items[i]);
            }
            DeserializeSection(currentSection, currentSectionItems);
        }

        private void DeserializeSection(string currentSection, List<string> currentSectionItems)
        {
            string currentSectionCSV = string.Join(Delimiter.ToString(), currentSectionItems.ToArray());
            if (currentSection == "Options")
                Options.Deserialize(currentSectionCSV);
            else if (currentSection == "Objects")
                Objects.Deserialize(currentSectionCSV);
            else if (currentSection == "EditorOptions")
                EditorOptions.Deserialize(currentSectionCSV);
        }
    }
}
