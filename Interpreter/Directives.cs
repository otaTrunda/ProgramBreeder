using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpreter
{
    public abstract class DirectiveNode : Node
    {
		protected static int indent = 0;
		protected List<Node> placeHolder = new List<Node>();
        public const int LoopLimit = 1000;

		/// <summary>
		/// Executes the directive WITHOUT executing other directives conected to it through "nextDirective". BEWARE of this behaviour especially in bodies of cycles!
		/// </summary>
		/// <param name="interpreter"></param>
		public abstract void execute(Interpret interpreter);

		/// <summary>
		/// Executes this directive together with all other directives that are conected to it by "nextDirective".
		/// </summary>
		/// <param name="interpreter"></param>
		public virtual void executeAll(Interpret interpreter)
		{
			execute(interpreter);
			var nextDirective = getNextDirective();
			if (nextDirective != null)
				nextDirective.executeAll(interpreter);
		}

		public virtual DirectiveNode getNextDirective()
		{
			getSuccessors(placeHolder);
			return (DirectiveNode)placeHolder[placeHolder.Count - 1];
		}

		public override NodeClass getNodeClass()
		{
			return NodeClass.directive;
		}

		public virtual void setNextDirective(DirectiveNode d)
		{
			this.setSuccessor(d, this.successors.count - 1);
		}

		public override string toSourceCode()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(ToString());
			var next = getNextDirective();
			if(next != null)
			{
				sb.AppendLine(next.toSourceCode());
			}
			return sb.ToString();
		}

		public DirectiveNode()
		{
			createSlots();
		}
	}

	[Obsolete]
    public class directivesSequence : DirectiveNode
    {
        private List<DirectiveNode> sequence;

		public directivesSequence()
		{

		}

        public directivesSequence(List<DirectiveNode> directives)
        {
            this.sequence = new List<DirectiveNode>(directives);
			createSlots();
			this.setNextDirective(directives[0]);

			for (int i = 0; i < directives.Count - 1; i++)
				directives[i].setNextDirective(directives[i + 1]);

			directives[directives.Count - 1].setNextDirective(new directiveTerminal());
		}

        public override void execute(Interpret interpreter)
        {
            foreach (var item in sequence)
            {
                item.execute(interpreter);
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
			for (int i = 0; i < indent; i++)
				builder.Append("\t");
            builder.AppendLine("{");
			indent++;
			if (sequence != null)
				foreach (var item in sequence)
				{
					builder.AppendLine(item.ToString());
				}
			indent--;
			for (int i = 0; i < indent; i++)
				builder.Append("\t");
			builder.Append("}");
            return builder.ToString();
        }

		public override string getLabel()
		{
			return "{ ... }";
		}

		public override NodeType type => NodeType.dirTerminal;	

		protected override void createSlots()
		{
			Slot[] slots = new Slot[] { new Slot(NodeClass.directive, "next directive", 0) };
			this.successors = new NodeSuccessors(slots, this);
		}
    }

    public class directiveIF : DirectiveNode
    {
		private BoolNode expression => (BoolNode)successors.GetSlot(0).nodeConnectedToSlot;
        private DirectiveNode directive => (DirectiveNode)successors.GetSlot(1).nodeConnectedToSlot;

		public directiveIF()
		{

		}

		public directiveIF(BoolNode exp, DirectiveNode dir)
        {
			createSlots();
			this.successors.setSuccessor(exp, 0);
			this.successors.setSuccessor(dir, 1);
        }

        public override void execute(Interpret interpreter)
        {
            if (expression.eval(interpreter))
                directive.executeAll(interpreter);
        }

        public override string ToString()
        {
			string r = "";
			for (int i = 0; i < indent; i++)
				r = r + "\t";
			return r + "IF " + expression.ToString() + " THEN " + directive.ToString();
        }

		public override string getLabel()
		{
			return "if";
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[3];
			s[0] = new Slot(NodeClass.boolean, "condition", 0);
			s[1] = new Slot(NodeClass.directive, "body", 1);
			s[2] = new Slot(NodeClass.directive, "next directive", 2);
			this.successors = new NodeSuccessors(s, this);
		}

        public override NodeType type => NodeType.dirIf;
	}

    public class directiveIFELSE : DirectiveNode
    {
		private BoolNode expression => (BoolNode)successors.GetSlot(0).nodeConnectedToSlot;
        private DirectiveNode directiveIF => (DirectiveNode)successors.GetSlot(1).nodeConnectedToSlot;
		private DirectiveNode directiveELSE => (DirectiveNode)successors.GetSlot(2).nodeConnectedToSlot;

		public directiveIFELSE()
		{

		}

        public directiveIFELSE(BoolNode exp, DirectiveNode directiveIF, DirectiveNode directiveELSE)
        {
			createSlots();
			this.successors.setSuccessor(exp, 0);
			this.successors.setSuccessor(directiveIF, 1);
			this.successors.setSuccessor(directiveELSE, 2);
		}

        public override void execute(Interpret interpreter)
        {
            if (expression.eval(interpreter))
                directiveIF.executeAll(interpreter);
            else directiveELSE.executeAll(interpreter);
        }

        public override string ToString()
        {
            return "IF " + expression.ToString() + " THEN " + directiveIF.ToString() +
                "\n\tELSE " + directiveELSE.ToString();
        }

		public override string getLabel()
		{
			return "if-else";
		}

		public override NodeType type => NodeType.dirIfElse;

		protected override void createSlots()
		{
			Slot[] s = new Slot[4];
			s[0] = new Slot(NodeClass.boolean, "condition", 0);
			s[1] = new Slot(NodeClass.directive, "if-branch", 1);
			s[2] = new Slot(NodeClass.directive, "else-branch", 2);
			s[3] = new Slot(NodeClass.directive, "next directive", 3);
			this.successors = new NodeSuccessors(s, this);
		}
	}

    public class directiveAssign : DirectiveNode
    {
		private NumericNode varIndex => (NumericNode)successors.GetSlot(0).nodeConnectedToSlot;
        private NumericNode value => (NumericNode)successors.GetSlot(1).nodeConnectedToSlot;

		public directiveAssign(NumericNode varIndex, NumericNode value)
        {
			createSlots();
			this.successors.setSuccessor(varIndex, 0);
			this.successors.setSuccessor(value, 1);
		}

        public override void execute(Interpret interpreter)
        {
            interpreter.getNumVariable(varIndex.eval(interpreter)).assign(value.eval(interpreter));
        }

        public override string ToString()
        {
            return "Var(" + varIndex.ToString() + ")" + " := " + value.ToString();
        }

		public override string getLabel()
		{
			return ":=";
		}

		public override NodeType type => NodeType.dirAssign;

		public directiveAssign()
		{

		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[3];
			s[0] = new Slot(NodeClass.numeric, "variable index", 0);
			s[1] = new Slot(NodeClass.numeric, "new value", 1);
			s[2] = new Slot(NodeClass.directive, "next directive", 2);
			this.successors = new NodeSuccessors(s, this);
		}

	}

    public class directiveIncrement : DirectiveNode
    {
		private NumericNode variableIndex => (NumericNode)successors.GetSlot(0).nodeConnectedToSlot;

		public directiveIncrement()
		{

		}

        public directiveIncrement(NumericNode variableIndex)
        {
			createSlots();
			successors.setSuccessor(variableIndex, 0);
        }

        public override void execute(Interpret interpreter)
        {
            NumericVariable v = interpreter.getNumVariable(variableIndex.eval(interpreter));
            v.increment();
        }

        public override string ToString()
        {
            return "Var(" + variableIndex.ToString() + ")" + "++";
        }

		public override string getLabel()
		{
			return "increment";
		}

		public override NodeType type => NodeType.dirIncrement;

		protected override void createSlots()
		{
			Slot[] s = new Slot[2];
			s[0] = new Slot(NodeClass.numeric, "variable index", 0);
			s[1] = new Slot(NodeClass.directive, "next directive", 1);
			this.successors = new NodeSuccessors(s, this);
		}
	}

    public class directiveDecrement : DirectiveNode
    {
		private NumericNode variableIndex => (NumericNode)successors.GetSlot(0).nodeConnectedToSlot;

		public directiveDecrement()
		{

		}

        public directiveDecrement(NumericNode variableIndex)
        {
			createSlots();
			successors.setSuccessor(variableIndex, 0);
		}

        public override void execute(Interpret interpreter)
        {
            NumericVariable v = interpreter.getNumVariable(variableIndex.eval(interpreter));
            v.decrement();
        }

        public override string ToString()
        {
            return "Var(" + variableIndex.ToString() + ")" + "--";
        }

		public override string getLabel()
		{
			return "decrement";
		}

		public override NodeType type => NodeType.dirDecrement;

		protected override void createSlots()
		{
			Slot[] s = new Slot[2];
			s[0] = new Slot(NodeClass.numeric, "variable index", 0);
			s[1] = new Slot(NodeClass.directive, "next directive", 1);
			this.successors = new NodeSuccessors(s, this);
		}
	}

    public class directiveAddLast : DirectiveNode
    {
		private NumericNode listIndex => (NumericNode)successors.GetSlot(0).nodeConnectedToSlot;
		private NumericNode value => (NumericNode)successors.GetSlot(1).nodeConnectedToSlot;

		public directiveAddLast()
		{

		}

		public directiveAddLast(NumericNode listIndex, NumericNode valueToAdd)
        {
			createSlots();
			successors.setSuccessor(listIndex, 0);
			successors.setSuccessor(valueToAdd, 1);
        }

        public override void execute(Interpret interpreter)
        {
            List<NumericVariable> list = interpreter.getList(listIndex.eval(interpreter));
            list.Add(new NumericVariable(value.eval(interpreter)));
        }

        public override string ToString()
        {
            return "List(" + listIndex.ToString() + ").AddLast(" + value.ToString() + ")";
        }

		public override string getLabel()
		{
			return "addLast";
		}

		public override NodeType type => NodeType.dirAddLast;

		protected override void createSlots()
		{
			Slot[] s = new Slot[3];
			s[0] = new Slot(NodeClass.numeric, "list index", 0);
			s[1] = new Slot(NodeClass.numeric, "new value", 1);
			s[2] = new Slot(NodeClass.directive, "next directive", 2);
			this.successors = new NodeSuccessors(s, this);
		}

	}

    public class directiveAddFirst : DirectiveNode
    {
		private NumericNode listIndex => (NumericNode)successors.GetSlot(0).nodeConnectedToSlot;
		private NumericNode value => (NumericNode)successors.GetSlot(1).nodeConnectedToSlot;

		public directiveAddFirst()
		{

		}

		public directiveAddFirst(NumericNode index, NumericNode value)
        {
			createSlots();
			successors.setSuccessor(listIndex, 0);
			successors.setSuccessor(value, 1);
		}

        public override void execute(Interpret interpreter)
        {
            List<NumericVariable> list = interpreter.getList(listIndex.eval(interpreter));
            list.Insert(0, new NumericVariable(value.eval(interpreter)));
        }

        public override string ToString()
        {
            return "List(" + listIndex.ToString() + ").AddFirst(" + value.ToString() + ")";
        }

		public override string getLabel()
		{
			return "AddFirst";
		}

		public override NodeType type => NodeType.dirAddFirst;

		protected override void createSlots()
		{
			Slot[] s = new Slot[3];
			s[0] = new Slot(NodeClass.numeric, "list index", 0);
			s[1] = new Slot(NodeClass.numeric, "new value", 1);
			s[2] = new Slot(NodeClass.directive, "next directive", 2);
			this.successors = new NodeSuccessors(s, this);
		}

	}

	public class directiveRemoveFirst : DirectiveNode
    {
		private NumericNode listIndex => (NumericNode)successors.get(0);

		public directiveRemoveFirst()
		{

		}

        public directiveRemoveFirst(NumericNode index)
        {
			createSlots();
			successors.setSuccessor(index, 0);
        }

        public override void execute(Interpret interpreter)
        {
            List<NumericVariable> list = interpreter.getList(listIndex.eval(interpreter));
			if (list.Count > 0)
				list.RemoveAt(0);
        }

        public override string ToString()
        {
            return "List(" + listIndex.ToString() + ").RemoveFirst()";
        }

		public override string getLabel()
		{
			return "RemoveFirst";
		}

		public override NodeType type => NodeType.dirRemoveFirst;

		protected override void createSlots()
		{
			Slot[] s = new Slot[2];
			s[0] = new Slot(NodeClass.numeric, "list index", 0);
			s[1] = new Slot(NodeClass.directive, "next directive", 2);
			this.successors = new NodeSuccessors(s, this);
		}
	}

    public class directiveRemoveLast : DirectiveNode
    {
		private NumericNode listIndex => (NumericNode)successors.get(0);

		public directiveRemoveLast()
		{

		}

		public directiveRemoveLast(NumericNode index)
        {
			createSlots();
			successors.setSuccessor(index, 0);
		}

        public override void execute(Interpret interpreter)
        {
            List<NumericVariable> list = interpreter.getList(listIndex.eval(interpreter));
			if (list.Count > 0)
				list.RemoveAt(list.Count - 1);
        }

        public override string ToString()
        {
            return "List(" + listIndex.ToString() + ").RemoveLast()";
        }

		public override string getLabel()
		{
			return "RemoveLast";
		}

		public override NodeType type => NodeType.dirRemoveLast;

		protected override void createSlots()
		{
			Slot[] s = new Slot[2];
			s[0] = new Slot(NodeClass.numeric, "list index", 0);
			s[1] = new Slot(NodeClass.directive, "next directive", 2);
			this.successors = new NodeSuccessors(s, this);
		}
	}

    public class directiveFOR : DirectiveNode
    {
		private NumericNode varIndex => (NumericNode)successors.get(0);
        private NumericNode limitVal => (NumericNode)successors.get(1);
		private DirectiveNode directive => (DirectiveNode)successors.get(2);

		public directiveFOR()
		{

		}

		public directiveFOR(NumericNode varIndex, NumericNode value, DirectiveNode directive)
        {
			createSlots();
			successors.setSuccessor(varIndex, 0);
			successors.setSuccessor(value, 1);
			successors.setSuccessor(directive, 2);
        }

        public override void execute(Interpret interpreter)
        {
            int limit = limitVal.eval(interpreter),
                counter = 0;
            NumericVariable v = interpreter.getNumVariable(varIndex.eval(interpreter));
            while (v.getValue() < limit && counter < LoopLimit)
            {
                directive.executeAll(interpreter);
                v.increment();
                counter++;
            }
        }

        public override string ToString()
        {
			string r = "";
			for (int i = 0; i < indent; i++)
				r = r + "\t";
			r = r + "FOR(;" + "Var(" + varIndex.ToString() + ")" + " < " + limitVal.ToString() + "; " + "Var(" + varIndex.ToString() + ")" + "++)\n";
			indent++;
			r = r + directive.ToString();
			indent--;
			return r;
        }

		public override string getLabel()
		{
			return "for";
		}

		public override NodeType type => NodeType.dirFor;

		protected override void createSlots()
		{
			Slot[] s = new Slot[4];
			s[0] = new Slot(NodeClass.numeric, "iterator variable index", 0);
			s[1] = new Slot(NodeClass.numeric, "limit value", 1);
			s[2] = new Slot(NodeClass.directive, "body", 2);
			s[3] = new Slot(NodeClass.directive, "next directive", 3);
			this.successors = new NodeSuccessors(s, this);
		}
	}

    public class directiveFOREACH : DirectiveNode
    {
		private NumericNode varIndex => (NumericNode)successors.get(0);
		private NumericNode listIndex => (NumericNode)successors.get(1);
		private DirectiveNode directive => (DirectiveNode)successors.get(2);

		public directiveFOREACH()
		{

		}

        public directiveFOREACH(NumericNode varIndex, NumericNode listIndex, DirectiveNode directive)
        {
			createSlots();
			successors.setSuccessor(varIndex, 0);
			successors.setSuccessor(listIndex, 1);
			successors.setSuccessor(directive, 2);
		}

        public override void execute(Interpret interpreter)
        {
            NumericVariable v = interpreter.getNumVariable(varIndex.eval(interpreter));
            int listInd = interpreter.getNumVariable(listIndex.eval(interpreter)).getValue();
			var list = interpreter.getList(listInd);
            int counter = 0;
			int index = 0;
            while(index < list.Count)
            {
                v.assign(list[index].getValue());
                directive.executeAll(interpreter);
				index++;
				counter++;
                if (counter > LoopLimit)
                    break;
            }
        }

        public override string ToString()
        {
            return "FOREACH(var Var(" + varIndex.ToString() + ")" + " in List(" + listIndex.ToString() + ")\n" +
                directive.ToString();
        }

		public override string getLabel()
		{
			return "foreach";
		}

		public override NodeType type => NodeType.dirForeach;

		protected override void createSlots()
		{
			Slot[] s = new Slot[4];
			s[0] = new Slot(NodeClass.numeric, "iterator variable index", 0);
			s[1] = new Slot(NodeClass.numeric, "list index", 1);
			s[2] = new Slot(NodeClass.directive, "body", 2);
			s[3] = new Slot(NodeClass.directive, "next directive", 3);
			this.successors = new NodeSuccessors(s, this);
		}
	}

    public class directiveWHILE : DirectiveNode
    {
		private BoolNode expression => (BoolNode)successors.get(0);
        private DirectiveNode dir => (DirectiveNode)successors.get(1);

		public directiveWHILE()
		{

		}

		public directiveWHILE(BoolNode exp, DirectiveNode dir)
        {
			createSlots();
			successors.setSuccessor(exp, 0);
			successors.setSuccessor(dir, 1);
		}

        public override void execute(Interpret interpreter)
        {
            int counter = 0;
            while (expression.eval(interpreter) && counter < LoopLimit)
            {
                dir.executeAll(interpreter);
                counter++;
            }
        }

        public override string ToString()
        {
            return "WHILE(" + expression.ToString() + ")\n" + dir.ToString();
        }

		public override string getLabel()
		{
			return "while";
		}

		public override NodeType type => NodeType.dirWhile;

		protected override void createSlots()
		{
			Slot[] s = new Slot[3];
			s[0] = new Slot(NodeClass.boolean, "condition", 0);
			s[1] = new Slot(NodeClass.directive, "body", 1);
			s[2] = new Slot(NodeClass.directive, "next directive", 2);
			this.successors = new NodeSuccessors(s, this);
		}
	}

	public class directiveSetOutput : DirectiveNode
	{
		private NumericNode outpuIndex => (NumericNode)successors.GetSlot(0).nodeConnectedToSlot;
		private NumericNode value => (NumericNode)successors.GetSlot(1).nodeConnectedToSlot;

		public directiveSetOutput(NumericNode outpuIndex, NumericNode value)
		{
			createSlots();
			this.successors.setSuccessor(outpuIndex, 0);
			this.successors.setSuccessor(value, 1);
		}

		public override void execute(Interpret interpreter)
		{
			interpreter.setOutput(outpuIndex.eval(interpreter), value.eval(interpreter));
		}

		public override NodeType type => NodeType.dirSetOutput;

		public directiveSetOutput()
		{

		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[3];
			s[0] = new Slot(NodeClass.numeric, "output index", 0);
			s[1] = new Slot(NodeClass.numeric, "value", 1);
			s[2] = new Slot(NodeClass.directive, "next directive", 2);
			this.successors = new NodeSuccessors(s, this);
		}

		public override string ToString()
		{
			return "result(" + outpuIndex.ToString() + ") := " + value.ToString();
		}

		public override string getLabel()
		{
			return "setResult";
		}
	}

	public class directiveTerminal : DirectiveNode
	{
		public override void execute(Interpret interpreter)
		{
			//nothing here
		}

		public override void executeAll(Interpret interpreter)
		{
			//nothing here
		}

		public override DirectiveNode getNextDirective()
		{
			return null;
		}

		public override void getSuccessors(List<Node> result)
		{
			return;
		}

		public override string getLabel()
		{
			return "END";
		}

		public override NodeType type => NodeType.dirTerminal;

		protected override void createSlots()
		{
			this.successors = NodeSuccessors.empty;
		}

		public directiveTerminal()
		{
			createSlots();
		}
	}

	/// <summary>
	/// Doesn't do anything but allows to impose constrains on the whole program
	/// </summary>
	public class directiveEntryPoint : DirectiveNode
	{
		public override NodeType type => NodeType.dirEntryPoint;

		public override void execute(Interpret interpreter)
		{
			//nothing here. Just go on the the next directive
		}

		protected override void createSlots()
		{
			Slot[] s = new Slot[1];
			s[0] = new Slot(NodeClass.directive, "next directive", 0);
			this.successors = new NodeSuccessors(s, this);
		}

		public directiveEntryPoint()
		{
		}

		public directiveEntryPoint(DirectiveNode nextDir)
		{
			createSlots();
			this.setSuccessor(nextDir, 0);
		}

		public override string getLabel()
		{
			return "START";
		}
	}
}

