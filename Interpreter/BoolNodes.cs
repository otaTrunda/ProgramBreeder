using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpreter
{
    public abstract class BoolNode : Node
    {
        public abstract bool eval(Interpret interpreter);

		public override NodeClass getNodeClass()
		{
			return NodeClass.boolean;
		}

		public override string toSourceCode()
		{
			return ToString();
		}
	}

	#region constants

	[Obsolete]
	public class BoolConstant :BoolNode
    {
        private bool value;

        public override bool eval(Interpret interpreter)
        {
            return value;
        }

        public BoolConstant(bool value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return (value ? "TRUE" : "FALSE");
        }

        public override NodeType type => NodeType.BoolConstant;

        public override object Clone()
        {
            return new BoolConstant(this.value);
        }

		protected override void createSlots()
		{
			this.successors = NodeSuccessors.empty;
		}

		public override bool isEqual(Node n)
		{
			if(n.type == this.type)
			{
				BoolConstant other = n as BoolConstant;
				return other.value == this.value;
			}
			return false;
		}
	}

	[Obsolete]
    public class BoolInput : BoolNode
    {
        int inputIndex;

		public override NodeType type => throw new Exception();

        public override bool eval(Interpret interpreter)
        {
            return interpreter.logicalInputs[inputIndex];
        }

        public override string ToString()
        {
            return "BoolInput(" + inputIndex + ")";
        }

        public BoolInput(int inputIndex)
        {
            this.inputIndex = inputIndex;
        }

        public override object Clone()
        {
            return new BoolInput(this.inputIndex);
        }
    }

    #endregion

    #region numeric relational operators
    public abstract class BoolRelationOperator : BoolNode
    {
		public NumericNode first => (NumericNode)successors.get(0);
		public NumericNode second => (NumericNode)successors.get(1);

        public override object Clone()
        {
            var result = (BoolRelationOperator)NodeFactory.createNode(this.type);
			result.successors.setSuccessor((NumericNode)first.Clone(), 0);
			result.successors.setSuccessor((NumericNode)second.Clone(), 1);
			return result;
        }

        public BoolRelationOperator()
        {

        }

		public BoolRelationOperator(NumericNode first, NumericNode second)
		{
			createSlots();
			successors.setSuccessor(first, 0);
			successors.setSuccessor(second, 1);
		}

		public override string ToString()
		{
			return "(" + first.ToString() + getLabel() + second.ToString() + ")";
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[]
			{
				new Slot(NodeClass.numeric, "firstArgument", 0),
				new Slot(NodeClass.numeric, "secondArgument", 1),
			};
			this.successors = new NodeSuccessors(s, this);
		}
	}

    public class BoolEqualsOperator : BoolRelationOperator
    {
        public BoolEqualsOperator(NumericNode first, NumericNode second)
			: base(first, second)
		{
        }

        public override bool eval(Interpret interpreter)
        {
            return first.eval(interpreter) == second.eval(interpreter);
        }

		public override string getLabel()
        {
            return "==";
        }

        public override NodeType type => NodeType.Equals;

        public BoolEqualsOperator()
        {
        }
    }

    public class BoolLessEqualOperator : BoolRelationOperator
    {
        public BoolLessEqualOperator(NumericNode first, NumericNode second)
			: base(first, second)
		{
        }

        public override bool eval(Interpret interpreter)
        {
            return first.eval(interpreter) <= second.eval(interpreter);
        }

		public override string getLabel()
		{
			return "<=";
		}

		public override NodeType type => NodeType.LessEq;

        public BoolLessEqualOperator()
        {
        }
    }

    public class BoolLessOperator : BoolRelationOperator
    {
        public BoolLessOperator(NumericNode first, NumericNode second)
			: base(first, second)
		{
		}

		public override bool eval(Interpret interpreter)
        {
            return first.eval(interpreter) < second.eval(interpreter);
        }

		public override string getLabel()
		{
			return "<";
		}

		public override NodeType type => NodeType.Less;

        public BoolLessOperator()
        {
        }
    }

    #endregion

    #region unary operators

    public abstract class BoolUnaryLogicOperator : BoolNode
    {
		public BoolNode exp => (BoolNode)successors.get(0);

        public BoolUnaryLogicOperator()
        {
        }

		public BoolUnaryLogicOperator(BoolNode exp)
		{
			createSlots();
			this.setSuccessor(exp, 0);
		}

		public override object Clone()
        {
            var result = (BoolUnaryLogicOperator)NodeFactory.createNode(this.type);
			result.successors.setSuccessor((BoolNode)exp.Clone(), 0);
            return result;
        }

		protected override void createSlots()
		{
			base.createSlots();
			Slot[] s = new Slot[] { new Slot(NodeClass.boolean, "argument", 0)};
			successors = new NodeSuccessors(s, this);
		}
	}

    public class BoolLogicNotOperator : BoolUnaryLogicOperator
    {
        public BoolLogicNotOperator(BoolNode exp)
			:base(exp)
        {
        }

        public override bool eval(Interpret interpreter)
        {
            return !exp.eval(interpreter);
        }

        public override string ToString()
        {
            return "!" + exp.ToString();
        }

        public override NodeType type => NodeType.NOT;
        public BoolLogicNotOperator()
        {
        }

		public override string getLabel()
		{
			return "NOT";
		}
	}

    #endregion

    #region binary operators

    public abstract class BoolLogicBinaryOperator : BoolNode
    {
		public BoolNode first => (BoolNode)successors.get(0);
		public BoolNode second => (BoolNode)successors.get(1);

		public override object Clone()
        {
            var result = (BoolLogicBinaryOperator)NodeFactory.createNode(this.type);
			result.setSuccessor((BoolNode)first.Clone(), 0);
			result.setSuccessor((BoolNode)second.Clone(), 1);
            return result;
        }

        public BoolLogicBinaryOperator()
        {
        }

		public override string ToString()
		{
			//return "(" + first.ToString() + getLabel() + second.ToString() + ")";
			return first.ToString() + getLabel() + second.ToString();
		}

		public BoolLogicBinaryOperator(BoolNode first, BoolNode second)
		{
			setSuccessor(first, 0);
			setSuccessor(second, 1);
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[]
			{
				new Slot(NodeClass.boolean, "first argument", 0),
				new Slot(NodeClass.boolean, "second argument", 1),
			};
			successors = new NodeSuccessors(s, this);
		}
	}

    public class BoolANDOperator : BoolLogicBinaryOperator
    {
        public BoolANDOperator(BoolNode first, BoolNode second)
			:base(first, second)
        {
        }

        public override bool eval(Interpret interpreter)
        {
            return first.eval(interpreter) && second.eval(interpreter);
        }

		public override string getLabel()
		{
			return "AND";
		}

		public override NodeType type => NodeType.AND;
        public BoolANDOperator()
        {
        }
    }

    public class BoolOROperator : BoolLogicBinaryOperator
    {
        public BoolOROperator(BoolNode first, BoolNode second)
			:base(first, second)
        {
        }

        public override bool eval(Interpret interpreter)
        {
            return first.eval(interpreter) || second.eval(interpreter);
        }

		public override string getLabel()
		{
			return "OR";
		}

		public override NodeType type => NodeType.OR;

        public BoolOROperator()
        {
        }
    }

    public class BoolXOROperator : BoolLogicBinaryOperator
    {
        public BoolXOROperator(BoolNode first, BoolNode second)
			:base(first, second)
        {
        }

        public override bool eval(Interpret interpreter)
        {
            return first.eval(interpreter) ^ second.eval(interpreter);
        }

		public override string getLabel()
		{
			return "XOR";
		}

		public override NodeType type => NodeType.XOR;

        public BoolXOROperator()
        {
        }
    }

    #endregion
}
