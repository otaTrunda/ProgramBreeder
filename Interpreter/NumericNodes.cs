using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpreter
{
    public abstract class NumericNode : Node
    {
        protected static Random r = new Random();
        public abstract int eval(Interpret interpreter);

		public override NodeClass getNodeClass()
		{
			return NodeClass.numeric;
		}

		public override string toSourceCode()
		{
			return ToString();
		}
	}

    public class NumericConstant : NumericNode
    {
        private int number = 0;

        public override NodeType type => NodeType.NumConst;

		public override object Clone()
        {
            return new NumericConstant(this.number);
        }

        public NumericConstant(int number)
        {
			this.successors = NodeSuccessors.empty;
            this.number = number;
        }

		public static implicit operator NumericConstant(int number)
		{
			return new NumericConstant(number);
		}

		public override int eval(Interpret interpreter)
        {
            return number;
        }

        public override string ToString()
        {
            return number.ToString();
        }

		public override string getLabel()
		{
			return number.ToString();
		}

		public override bool isEqual(Node n)
		{
			if (n.type == this.type)
			{
				NumericConstant other = n as NumericConstant;
				return other.number == this.number;
			}
			return false;
		}
	}

    public class InputNode : NumericNode
    {
        int inputIndex;

        public override int eval(Interpret interpreter)
        {
			return interpreter.getInput(inputIndex);
        }

        public override string ToString()
        {
            return "Input(" + inputIndex + ")";
        }

        public InputNode(int inputIndex)
        {
            this.inputIndex = inputIndex;
        }

		public override NodeType type => NodeType.NumInput;

        public override object Clone()
        {
            return new InputNode(this.inputIndex);
        }

		public override string getLabel()
		{
			return ToString();
		}

		public override bool isEqual(Node n)
		{
			if (n.type == this.type)
			{
				InputNode other = n as InputNode;
				return other.inputIndex == this.inputIndex;
			}
			return false;
		}

	}

    #region unary operators and math functions

    public abstract class NumericUnaryOperator : NumericNode
    {
		public NumericNode experssion => (NumericNode)successors.get(0);

        public override object Clone()
        {
            var result = (NumericUnaryOperator)NodeFactory.createNode(this.type);
			result.setSuccessor((NumericNode)this.experssion.Clone(), 0);
            return result;
        }

        public NumericUnaryOperator()
        {
        }

		public NumericUnaryOperator(NumericNode argument)
		{
			setSuccessor(argument, 0);
		}

		public override string ToString()
		{
			return getLabel() + "(" + experssion.ToString() + ")";
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[]
			{
				new Slot(NodeClass.numeric, "argument", 0)
			};
			successors = new NodeSuccessors(s, this);
		}
	}

	public class RandomGeneratorNode : NumericUnaryOperator
	{
		public override NodeType type => NodeType.Rand;

		public RandomGeneratorNode()
		{

		}

		public RandomGeneratorNode(NumericNode limit)
			:base(limit)
		{
		}

		public override int eval(Interpret interpreter)
		{
			int limit = experssion.eval(interpreter);
			if (limit <= 0)
				return 0;
			return r.Next(limit);
		}

		public override string ToString()
		{
			return "Random()";
		}
	}

	public class LogFunction : NumericUnaryOperator
    {
        public override NodeType type => NodeType.Log;

        public override int eval(Interpret interpreter)
        {
			int argument = experssion.eval(interpreter);
			if (argument <= 0)
				return 0;
			return (int)Math.Round(Math.Log(argument, 2));
        }

		public override string getLabel()
        {
			return "Log";
        }

        public LogFunction()
		{ 
        }

		public LogFunction(NumericNode experssion)
			:base(experssion)
		{
		}
	}

    public class SinFunction : NumericUnaryOperator
    {
        public override NodeType type => NodeType.Sin;

        public override int eval(Interpret interpreter)
        {
            return (int)Math.Round(Math.Sin(experssion.eval(interpreter)));
        }

		public override string getLabel()
		{
			return "Sin";
		}

		public SinFunction()
        {

        }

		public SinFunction(NumericNode experssion)
			:base(experssion)
		{
		}
	}

    public class TanFunction : NumericUnaryOperator
    {
        public override NodeType type => NodeType.Tan;

        public override int eval(Interpret interpreter)
        {
            return (int)Math.Round(Math.Tan(experssion.eval(interpreter)));
        }

		public override string getLabel()
		{
			return "Tan";
		}

		public TanFunction()
        {

        }

		public TanFunction(NumericNode experssion)
			:base(experssion)
		{
		}
	}

    public class SquareFunction : NumericUnaryOperator
    {
		public override NodeType type => NodeType.Sqr;

        public override int eval(Interpret interpreter)
        {
            int value = experssion.eval(interpreter);
            return value * value;
        }

        public override string ToString()
        {
            return "(" + experssion.ToString() + ")^2";
        }

        public SquareFunction()
        {
        }

		public override string getLabel()
		{
			return "^2";
		}

		public SquareFunction(NumericNode experssion)
			:base(experssion)
		{
		}
	}

    public class RootFunction : NumericUnaryOperator
    {
        public override NodeType type => NodeType.Sqrt;

        public override int eval(Interpret interpreter)
        {
			int argument = experssion.eval(interpreter);
			if (argument < 0)
				return 0;
			return (int)Math.Round(Math.Sqrt(argument));
        }

		public override string getLabel()
		{
			return "Sqrt";
		}

		public RootFunction()
        {
        }

		public RootFunction(NumericNode experssion)
			:base(experssion)
		{
		}
	}

    #endregion

    #region binary operators

    public abstract class NumericBinaryOperator : NumericNode
    {
		public NumericNode first => (NumericNode)successors.get(0);
		public NumericNode second => (NumericNode)successors.get(1);

        public override object Clone()
        {
            var result = (NumericBinaryOperator)NodeFactory.createNode(this.type);
			result.setSuccessor((NumericNode)first.Clone(), 0);
			result.setSuccessor((NumericNode)second.Clone(), 1);
            return result;
        }

		public NumericBinaryOperator()
		{
		}

		public NumericBinaryOperator(NumericNode first, NumericNode second)
		{
			setSuccessor(first, 0);
			setSuccessor(second, 1);
		}

		public override string ToString()
		{
			return "(" + first.ToString() + " " + getLabel() + " " + second.ToString() + ")";
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[]
			{
				new Slot(NodeClass.numeric, "first argument", 0),
				new Slot(NodeClass.numeric, "second argument", 1),
			};
			this.successors = new NodeSuccessors(s, this);
		}
	}

    public class PlusOperator : NumericBinaryOperator
    {
        public override NodeType type => NodeType.Plus;
        public PlusOperator(NumericNode first, NumericNode second)
			:base(first, second)
        {
        }

        public PlusOperator()
        {
        }

        public override int eval(Interpret interpreter)
        {
            return first.eval(interpreter) + second.eval(interpreter);
        }

		public override string getLabel()
		{
			return "+";
		}
	}

    public class MinusOperator : NumericBinaryOperator
    {
		public override NodeType type => NodeType.Minus;

        public MinusOperator(NumericNode first, NumericNode second)
			:base(first, second)
        {
        }

        public override int eval(Interpret interpreter)
        {
            return first.eval(interpreter) - second.eval(interpreter);
        }

		public override string getLabel()
		{
			return "-";
		}

		public MinusOperator()
        {

        }
    }

    public class DivOperator : NumericBinaryOperator
    {
        public override NodeType type => NodeType.Div;
        public DivOperator(NumericNode first, NumericNode second)
			:base(first, second)
		{
        }

        public override int eval(Interpret interpreter)
        {
			int secondVal = second.eval(interpreter);
			if (secondVal == 0)
				return 0;
			return first.eval(interpreter) / secondVal;
        }

		public override string getLabel()
		{
			return "/";
		}

		public DivOperator()
        {

        }
    }

    public class MultiplyOperator : NumericBinaryOperator
    {
        public override NodeType type => NodeType.Multiply;

        public MultiplyOperator(NumericNode first, NumericNode second)
			:base(first, second)
        {
        }

        public override int eval(Interpret interpreter)
        {
            return first.eval(interpreter) * second.eval(interpreter);
        }

		public override string getLabel()
		{
			return "*";
		}

		public MultiplyOperator()
        {

        }
    }

    public class ModOperator : NumericBinaryOperator
    {
        public override NodeType type => NodeType.Mod;
        public ModOperator(NumericNode first, NumericNode second)
			:base(first, second)
        {
        }

        public override int eval(Interpret interpreter)
        {
			int firstVal = first.eval(interpreter);
			if (firstVal == 0)
				return 0;
			int secondVal = second.eval(interpreter);
			if (secondVal == 0)
				return firstVal;

			return firstVal % secondVal;
        }

		public override string getLabel()
		{
			return "%";
		}

		public ModOperator()
        {

        }
    }

    #endregion
        
    public class BoolToNumber : NumericNode
    {
		public BoolNode expression => (BoolNode)successors.get(0);
        public override NodeType type => NodeType.Bool2Num;

        public override int eval(Interpret interpreter)
        {
            return expression.eval(interpreter) ? 1 : 0;
        }

        public override string ToString()
        {
            return "(" + expression.ToString() + ").asNumber";
        }

        public override object Clone()
        {
            var result = new BoolToNumber();
			result.setSuccessor((BoolNode)this.expression.Clone(), 0);
            return result;
        }

        public BoolToNumber()
        {
        }

		public BoolToNumber(BoolNode node)
		{
			setSuccessor(node, 0);
		}

		public override string getLabel()
		{
			return "ToNumber";
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[] { new Slot(NodeClass.boolean, "argument", 0) };
			successors = new NodeSuccessors(s, this);
		}
	}

    #region list manipulation nodes

    public abstract class ListManipulationNode : NumericNode
    {
        
    }

    public class ValueGetter : ListManipulationNode
    {
		public override NodeType type => NodeType.ValueGetter;

		private NumericNode variableIndex => (NumericNode)successors.get(0);

		public ValueGetter()
		{

		}

        public ValueGetter(NumericNode variableIndex)
        {
			setSuccessor(variableIndex, 0);
        }

        public override int eval(Interpret interpreter)
        {
            return interpreter.getNumVariable(variableIndex.eval(interpreter)).getValue();
        }

        public override string ToString()
        {
            return "Var(" + variableIndex.ToString() + ")";
        }

		public override string getLabel()
		{
			return "ReadVar";
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[] { new Slot(NodeClass.numeric, "argument", 0) };
			successors = new NodeSuccessors(s, this);
		}

	}

	public class ListValueGetter : ListManipulationNode
	{
		public override NodeType type => NodeType.ListGetValue;

		private NumericNode listIndex => (NumericNode)successors.get(0);
		private NumericNode itemIndex => (NumericNode)successors.get(1);

		public ListValueGetter()
		{

		}

		public ListValueGetter(NumericNode listIndex, NumericNode itemIndex)
		{
			setSuccessor(listIndex, 0);
			setSuccessor(itemIndex, 1);
		}

		public override int eval(Interpret interpreter)
		{
			return interpreter.getValueFromList(this.listIndex.eval(interpreter), this.itemIndex.eval(interpreter)).eval(interpreter);
		}

		public override string ToString()
		{
			return "List(" + listIndex.ToString() + ")[" + itemIndex.ToString() + "]";
		}

		public override string getLabel()
		{
			return "ReadFromList";
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[] 
			{
				new Slot(NodeClass.numeric, "listIndex", 0),
				new Slot(NodeClass.numeric, "itemIndex", 1),
			};
			successors = new NodeSuccessors(s, this);
		}
	}

	public class ListSizeGetter : ListManipulationNode
    {
		public override NodeType type => NodeType.ListSizeGetter;

		private NumericNode listIndex => (NumericNode)successors.get(0);

		public ListSizeGetter()
		{

		}

        public ListSizeGetter(NumericNode listIndex)
        {
			setSuccessor(listIndex, 0);
        }

        public override int eval(Interpret interpreter)
        {
            return interpreter.getList(listIndex.eval(interpreter)).Count;
        }

        public override string ToString()
        {
            return "List(" + listIndex.ToString() + ").Count";
        }

		public override string getLabel()
		{
			return "SizeOfList";
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[]
			{
				new Slot(NodeClass.numeric, "listIndex", 0),
			};
			successors = new NodeSuccessors(s, this);
		}
	}

    public class ListGetFirst : ListManipulationNode
    {
		public override NodeType type => NodeType.ListGetFirst;

		private NumericNode listIndex => (NumericNode)successors.get(0);

		public ListGetFirst()
		{

		}

        public ListGetFirst(NumericNode listIndex)
        {
			setSuccessor(listIndex, 0);
        }

        public override int eval(Interpret interpreter)
        {
            List<NumericVariable> list = interpreter.getList(listIndex.eval(interpreter));
            if (list.Count == 0)
                return 0;
            return list[0].getValue();
        }

        public override string ToString()
        {
            return "List(" + listIndex.ToString() + ").GetFirst()";
        }

		public override string getLabel()
		{
			return "ListGetFirst";
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[]
			{
				new Slot(NodeClass.numeric, "listIndex", 0),
			};
			successors = new NodeSuccessors(s, this);
		}
	}

    public class ListGetLast : ListManipulationNode
    {
		public override NodeType type => NodeType.ListGetLast;

		private NumericNode listIndex => (NumericNode)successors.get(0);

		public ListGetLast()
		{

		}

        public ListGetLast(NumericNode listIndex)
        {
			setSuccessor(listIndex, 0);
        }

        public override int eval(Interpret interpreter)
        {
            List<NumericVariable> list = interpreter.getList(listIndex.eval(interpreter));
            if (list.Count == 0)
                return 0;
            return list[list.Count - 1].getValue();
        }

        public override string ToString()
        {
            return "List(" + listIndex.ToString() + ").GetLast()";
        }

		public override string getLabel()
		{
			return "ListGetLast";
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[]
			{
				new Slot(NodeClass.numeric, "listIndex", 0),
			};
			successors = new NodeSuccessors(s, this);
		}
	}

#endregion
}
