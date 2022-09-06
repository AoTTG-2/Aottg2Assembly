using System.Collections.Generic;

namespace CustomLogic
{
    class CustomLogicForBlockAst: CustomLogicBlockAst
    {
        public CustomLogicAssignmentExpressionAst Initial;
        public CustomLogicBaseExpressionAst Conditional;
        public CustomLogicAssignmentExpressionAst Assignment;

        public CustomLogicForBlockAst(int line): base(CustomLogicAstType.ForExpression, line)
        {
        }
    }
}
