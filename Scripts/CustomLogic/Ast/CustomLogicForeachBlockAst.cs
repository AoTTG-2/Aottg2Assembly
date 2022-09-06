using System.Collections.Generic;

namespace CustomLogic
{
    class CustomLogicForeachBlockAst: CustomLogicBlockAst
    {
        public CustomLogicVariableExpressionAst Variable;
        public CustomLogicBaseExpressionAst Iterable;

        public CustomLogicForeachBlockAst(int line): base(CustomLogicAstType.ForeachExpression, line)
        {
        }
    }
}
