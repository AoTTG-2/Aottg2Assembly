using UnityEngine;
using Utility;

namespace Map
{
    class MapScriptOptions: BaseCSVRow
    {
        protected override bool NamedParams => true;
        public string Version = "1.0";
        public string Author = string.Empty;
        public string Description = string.Empty;
    }
}
