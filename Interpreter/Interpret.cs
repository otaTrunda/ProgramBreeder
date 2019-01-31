using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpreter
{
	public class Interpret
	{
		#region constants

		public static readonly int variablesLimit = 1000,
			listsLimit = 100;

		#endregion

		#region Data

		public NumericVariable[] numVariables;
		[Obsolete]
		public BooleanVariable[] boolVariables;

		public List<List<NumericVariable>> lists;
		public List<int> numericInputs;
		public int[] outputs;

		[Obsolete]
		public List<bool> logicalInputs;
		
		#endregion

		#region Getters and other methods for accessing the data

		public NumericVariable getNumVariable(int variableIndex)
		{
			if (variableIndex < 0)
				return getNumVariable(0);
			if (variableIndex >= variablesLimit)
				return getNumVariable(variableIndex % variablesLimit);
			if (numVariables[variableIndex] == null)
			{
				numVariables[variableIndex] = new NumericVariable();
			}
			return numVariables[variableIndex];
		}

		[Obsolete]
		public BooleanVariable getBoolVariable(int variableIndex)
		{
			if (variableIndex >= variablesLimit)
				return getBoolVariable(variableIndex % variablesLimit);
			if (boolVariables[variableIndex] == null)
			{
				boolVariables[variableIndex] = new BooleanVariable();
			}
			return boolVariables[variableIndex];
		}

		public List<NumericVariable> getList(int listIndex)
		{
			while (listIndex < 0)
				listIndex += listsLimit;
			if (listIndex >= listsLimit)
				return lists[listIndex % listsLimit];
			return lists[listIndex];
		}
		public void setOutput(int outputIndex, int value)
		{
			while (outputIndex < 0)
				outputIndex += variablesLimit;
			outputs[outputIndex % variablesLimit] = value;
		}

		/// <summary>
		/// Returns value from list. Values in lists are read-only. List may be modified by addFirst/Last and removeFirst/Last but direct access only allows to read the data.
		/// TODO.. why??
		/// </summary>
		/// <param name="listIndex"></param>
		/// <param name="itemIndex"></param>
		/// <returns></returns>
		public NumericNode getValueFromList(int listIndex, int itemIndex)
		{
			var list = getList(listIndex);
			if (list.Count == 0)
				return NumericNode.defaultNumericNode;
			if (itemIndex < 0)
				return new NumericConstant(0);
			return new NumericConstant(list[itemIndex % list.Count].getValue());
		}

		public int getInput(int inputIndex)
		{
			if (numericInputs.Count == 0)
				return 0;
			while (inputIndex < 0)
				inputIndex += numericInputs.Count;
			return numericInputs[inputIndex % numericInputs.Count];
		}

        #endregion

        #region Constructors

        public Interpret()
        {
            this.numVariables = new NumericVariable[variablesLimit];
			this.outputs = new int[variablesLimit];
            this.lists = new List<List<NumericVariable>>();
            for (int i = 0; i < listsLimit; i++)
            {
                lists.Add(new List<NumericVariable>());
            }
        }

		#endregion

		public void reset()
		{
			this.numVariables = new NumericVariable[variablesLimit];
			this.outputs = new int[variablesLimit];
			for (int i = 0; i < listsLimit; i++)
			{
				lists[i].Clear();
			}
		}

	}



}
