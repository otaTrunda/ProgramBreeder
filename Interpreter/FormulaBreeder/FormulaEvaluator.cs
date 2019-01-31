using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpreter;

namespace FormulaBreeder
{
    public class FormulaEvaluator
    {
        /// <summary>
        /// Evaluates the furmula. The result should be within <0, 1> where 0 means the worst result and 1 means perfect result.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public virtual double evaluate(Formula f)
        {
            return 0;
        }
    }

    public class DataSetEvaluator : FormulaEvaluator
    {
        public DataSet data;
        public int outputIndex = 0;
        public Interpret interpreter;

        public override double evaluate(Formula f)
        {
            int correct = 0, total = 0;
            foreach (var item in data.records)
            {
                total++;
                if (isCorrect(item, f))
                    correct++;
            }
            return correct / total;
        }

        private bool isCorrect(DataPoint p, Formula f)
        {
            interpreter.numericInputs = p.integerData;
            interpreter.logicalInputs = p.logicData;

            switch(data.schema.outputsType[outputIndex])
            {
                case DataType.numerical:
                    NumericNode n = (NumericNode)f.entryNode;
                    return n.eval(interpreter) == p.integerData[p.integerData.Count - 1];
                case DataType.logic:
                    BoolNode m = (BoolNode)f.entryNode;
                    return m.eval(interpreter) == p.logicData[p.logicData.Count - 1];
                default: return false;
            }
        }
    }
}
