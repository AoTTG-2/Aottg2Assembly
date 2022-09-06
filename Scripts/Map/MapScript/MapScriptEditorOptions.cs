using Utility;

namespace Map
{
    class MapScriptEditorOptions: BaseCSVRow
    {
        protected override bool NamedParams => true;
        public string CustomLogicName = string.Empty;
    }
}
