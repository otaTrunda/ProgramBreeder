using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interpreter;

namespace ProgramBreeder
{
    public enum NumberType
    {
        SimpleNumber,
        operatorPlus,
        operatorMinus,
        operatorDiv,
        operatorTimes,
        operatorMod,
        random,
        valueGetter,
        listSizeGetter,
        listGetFirst,
        listGetLast
    }

    public enum BoolExpType
    {
        boolExpSimple,
        boolExpEqual,
        boolExpLessEqual,
        boolExpLess,
        boolExpNOT,
        boolExpAND,
        boolExpOR,
        boolExpXOR
    }

    public enum DirectiveType
    {
        DirectiveIF,
        DirectiveIfElse,
        DirectiveASSIGN,
        DirectiveINCREMENT,
        DirectiveDECREMENT,
        DirectiveADDLAST,
        DirectiveADDFIRST,
        DirectiveREMOVELAST,
        DirectiveREMOVEFIRST,
        DirectiveFOR,
        DirectiveFOREACH,
        DirectiveWHILE
    }

    public class Factory
    {
        private static Random r = new Random();

        #region Number creation

        public static NumericNode createNumericConstant(int number)
        {
            return new NumericConstant(number);
        }

        public static NumericNode createNumericExpression(NumberType type, NumericNode parram)
        {
            switch (type)
            {
                case NumberType.listGetFirst:
                    return new ListGetFirst(parram);
                case NumberType.listGetLast:
                    return new ListGetLast(parram);
                case NumberType.listSizeGetter:
                    return new ListSizeGetter(parram);
                case NumberType.random:
                    return new RandomGeneratorNode(parram);
                case NumberType.valueGetter:
                    return new ValueGetter(parram);
                default:
                    throw new Exception();
            }
        }

        public static NumericNode createNumericOperator(NumberType type, NumericNode parram1, NumericNode parram2)
        {
            switch (type)
            {
                case NumberType.operatorDiv:
                    return new DivOperator(parram1, parram2);
                case NumberType.operatorMinus:
                    return new MinusOperator(parram1, parram2);
                case NumberType.operatorMod:
                    return new ModOperator(parram1, parram2);
                case NumberType.operatorPlus:
                    return new PlusOperator(parram1, parram2);
                case NumberType.operatorTimes:
                    return new MultiplyOperator(parram1, parram2);
                default:
                    throw new Exception();
            }
        }

        private static NumericNode createRandomNumericConstant()
        {
            if (r.NextDouble() <= 0.8)
                return new NumericConstant(r.Next(10));
            return new NumericConstant(r.Next(100));
        }

        #endregion

        #region Boolean expressions creation

        public static BoolNode createBoolConstant(bool value)
        {
            return new BoolConstant(value);
        }

        public static BoolNode createBoolExpNot(BoolNode expression)
        {
            return new BoolLogicNotOperator(expression);
        }

        public static BoolNode createBoolRelationOperator(BoolExpType type, NumericNode parram1, NumericNode parram2)
        {
            switch (type)
            {
                case BoolExpType.boolExpEqual:
                    return new BoolEqualsOperator(parram1, parram2);

                case BoolExpType.boolExpLess:
                    return new BoolLessOperator(parram1, parram2);
                case BoolExpType.boolExpLessEqual:
                    return new BoolLessEqualOperator(parram1, parram2);
                default:
                    throw new Exception();
            }
        }

        public static BoolNode createBoolLogicOperator(BoolExpType type, BoolNode parram1,
            BoolNode parram2)
        {
            switch (type)
            {
                case BoolExpType.boolExpAND:
                    return new BoolANDOperator(parram1, parram2);
                case BoolExpType.boolExpOR:
                    return new BoolOROperator(parram1, parram2);
                case BoolExpType.boolExpXOR:
                    return new BoolXOROperator(parram1, parram2);
                default:
                    throw new Exception();
            }
        }

        #endregion

        #region Directive creation

        public static DirectiveNode createDirectiveListAdd(DirectiveType type, NumericNode listIndex, NumericNode toAdd)
        {
            switch(type)
            {
                case DirectiveType.DirectiveADDFIRST:
                    return new directiveAddFirst(listIndex, toAdd);
                case DirectiveType.DirectiveADDLAST:
                    return new directiveAddLast(listIndex, toAdd);
                default:
                    throw new Exception();
            }
        }
        public static DirectiveNode createDirectiveListRemove(DirectiveType type, NumericNode listIndex)
        {
            switch (type)
            {
                case DirectiveType.DirectiveREMOVEFIRST:
                    return new directiveRemoveFirst(listIndex);
                case DirectiveType.DirectiveREMOVELAST:
                    return new directiveRemoveLast(listIndex);
                default:
                    throw new Exception();
            }
        }

        public static DirectiveNode createDirectiveVariableModify(DirectiveType type, NumericNode varIndex)
        {
            switch (type)
            {
                case DirectiveType.DirectiveINCREMENT:
                    return new directiveIncrement(varIndex);
                case DirectiveType.DirectiveDECREMENT:
                    return new directiveDecrement(varIndex);
                default:
                    throw new Exception();
            }
        }

        public static DirectiveNode createDirectiveVariableAssign(NumericNode varIndex, NumericNode value)
        {
            return new directiveAssign(varIndex, value);
        }

        public static DirectiveNode createDirectiveIF(BoolNode condition, DirectiveNode thenBranch)
        {
            return new directiveIF(condition, thenBranch);
        }

        public static DirectiveNode createDirectiveIfELSE(BoolNode condition, DirectiveNode thenBranch, DirectiveNode elseBranch)
        {
            return new directiveIFELSE(condition, thenBranch, elseBranch);
        }

        public static DirectiveNode createDirectiveFOR(NumericNode varIndex, NumericNode upperBound, DirectiveNode body)
        {
            return new directiveFOR(varIndex, upperBound, body);
        }

        public static DirectiveNode createDirectiveWHILE(BoolNode condition, DirectiveNode body)
        {
            return new directiveWHILE(condition, body);
        }

        public static DirectiveNode createDirectiveFOREACH(NumericNode varIndex, NumericNode listIndex, DirectiveNode body)
        {
            return new directiveFOREACH(varIndex, listIndex, body);
        }

        #endregion


    }
}
