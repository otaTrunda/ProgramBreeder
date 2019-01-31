using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpreter
{
	public class NumericVariable
	{
		private int value = 0;

		public void assign(int value)
		{
			this.value = value;
		}

		public int getValue()
		{
			return value;
		}

		public void increment()
		{
			value++;
		}

		public void decrement()
		{
			value--;
		}

		public NumericVariable()
		{
		}

		public NumericVariable(int value)
		{
			this.value = value;
		}

		public override string ToString()
		{
			return $"var:{value}";
		}
	}

	[Obsolete]
	public class BooleanVariable
	{
		private bool value = false;

		public void assign(bool value)
		{
			this.value = value;
		}

		public bool getValue()
		{
			return value;
		}

		public BooleanVariable()
		{
		}

		public BooleanVariable(bool value)
		{
			this.value = value;
		}

		public override string ToString()
		{
			return $"var:{value}";
		}
	}

	[Obsolete]
    public class NumVariableWrapper
    {
        public int variableIndex;

        public NumVariableWrapper(int variableIndex)
        {
            this.variableIndex = variableIndex;
        }

        public void assign(int value, Interpret interpreter)
        {
            NumericVariable var = interpreter.getNumVariable(variableIndex);
            var.assign(value);
        }

        public NumericVariable getVariable(Interpret interpreter)
        {
            return interpreter.getNumVariable(variableIndex);
        }

        public override string ToString()
        {
            return "Var(" + variableIndex.ToString() + ")";
        }
    }

	[Obsolete]
	public class BoolVariableWrapper
    {
        public int variableIndex;

        public BoolVariableWrapper(int variableIndex)
        {
            this.variableIndex = variableIndex;
        }

        public void assign(bool value, Interpret interpreter)
        {
            BooleanVariable var = interpreter.getBoolVariable(variableIndex);
            var.assign(value);
        }

        public BooleanVariable getVariable(Interpret interpreter)
        {
            return interpreter.getBoolVariable(variableIndex);
        }

        public override string ToString()
        {
            return "Var(" + variableIndex.ToString() + ")";
        }
    }
}
