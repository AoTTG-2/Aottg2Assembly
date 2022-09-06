using System.Collections.Generic;

namespace CustomLogic
{
    class CustomLogicPrimitiveExpressionAst: CustomLogicBaseExpressionAst
    {
        public CustomLogicPrimitiveType PrimitiveType;
        public object Value;

        public CustomLogicPrimitiveExpressionAst(object value, int line): base(CustomLogicAstType.PrimitiveExpression, line)
        {
            Value = value;
            if (value is int)
                PrimitiveType = CustomLogicPrimitiveType.Int;
            else if (value is float)
                PrimitiveType = CustomLogicPrimitiveType.Float;
            else if (value is string)
                PrimitiveType = CustomLogicPrimitiveType.String;
            else if (value is bool)
                PrimitiveType = CustomLogicPrimitiveType.Bool;
        }
    }

    public enum CustomLogicPrimitiveType
    {
        Int,
        Float,
        String,
        Bool
    }
}
