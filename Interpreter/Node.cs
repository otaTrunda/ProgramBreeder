using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpreter
{
	public enum NodeType
	{
		NumInput,
		NumConst,
		Log,
		Sin,
		Tan,
		Sqr,
		Sqrt,
		Plus,
		Minus,
		Div,
		Mod,
		Multiply,
		Rand,
		ValueGetter,
		ListSizeGetter,
		ListGetValue,
		ListGetFirst,
		ListGetLast,
		Bool2Num,
		//BoolInput,	//boolean inputs are no longer supported. Use numeric inputs instead with 0 = false, 1 = true (or any other way you like)
		BoolConstant,
		Equals,
		Less,
		LessEq,
		AND,
		OR,
		XOR,
		NOT,
		dirFor,
		dirForeach,
		dirWhile,
		dirAddLast,
		dirAddFirst,
		dirRemoveFirst,
		dirRemoveLast,
		//dirSequence,	//directiveSequence is no longer necessary since every directive has a "nextDirective" as its sucessor
		dirIf,
		dirIfElse,
		dirAssign,
		dirIncrement,
		dirDecrement,
		dirSetOutput,
		dirTerminal,
		dirEntryPoint
	}

	public enum NodeClass
	{
		directive,
		numeric,
		boolean
	}

	#region obsolete parts

	[Obsolete]
	public enum FunctionTypeEnum
	{
		_00_10,
		_00_01,
		_10_10,
		_20_10,
		_20_01,
		_02_01,
		_01_01,
		_01_10
	}

	[Obsolete]
	public class FunctionType
	{
		public int numericInputsCount, logicInputsCount, numericOutputsCount, logicOutputsCount;
		public FunctionTypeEnum signature;

		public override bool Equals(object obj)
		{
			if (!(obj is FunctionType))
				return false;
			FunctionType other = (FunctionType)obj;

			return other.signature == this.signature;

			return other.logicInputsCount == this.logicInputsCount && other.numericInputsCount == this.numericInputsCount &&
				other.logicOutputsCount == this.logicOutputsCount && other.numericOutputsCount == this.numericOutputsCount;
		}

		/// <summary>
		/// Two types match if they have the same number of outputs of each type
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool matches(FunctionType other)
		{
			return this.logicOutputsCount == other.logicOutputsCount &&
				this.numericOutputsCount == other.numericOutputsCount;
		}

		public FunctionType(int numericInputsCount, int logicInputsCount,
			int numericOutputsCount, int logicOutputsCount, FunctionTypeEnum sig)
		{
			this.numericInputsCount = numericInputsCount;
			this.numericOutputsCount = numericOutputsCount;
			this.logicInputsCount = logicInputsCount;
			this.logicOutputsCount = logicOutputsCount;
			this.signature = sig;
		}

		public static FunctionType
			_00_10 = new FunctionType(0, 0, 1, 0, FunctionTypeEnum._00_10),
			_00_01 = new FunctionType(0, 0, 0, 1, FunctionTypeEnum._00_01),
			_10_10 = new FunctionType(1, 0, 1, 0, FunctionTypeEnum._10_10),
			_20_10 = new FunctionType(2, 0, 1, 0, FunctionTypeEnum._20_10),
			_20_01 = new FunctionType(2, 0, 0, 1, FunctionTypeEnum._20_01),
			_02_01 = new FunctionType(0, 2, 0, 1, FunctionTypeEnum._02_01),
			_01_01 = new FunctionType(0, 1, 0, 1, FunctionTypeEnum._01_01),
			_01_10 = new FunctionType(0, 1, 1, 0, FunctionTypeEnum._01_10);
	}

	[Obsolete]
	public class NodeSignature
	{
		public NodeClass[] inputs;
		public NodeClass type;

		public NodeSignature(NodeClass type, params NodeClass[] inputs)
		{
			this.type = type;
			this.inputs = inputs;
		}
	}

	#endregion

	public class Slot
	{
		public NodeClass argumentClass;
		public string argumentDescription;
		public Node nodeConnectedToSlot;
		public int index;

		public void setNodeConnectedToSlot(Node n)
		{
			if (n.getNodeClass() != this.argumentClass)
				throw new Exception();
			nodeConnectedToSlot = n;
		}

		public Slot(NodeClass argumentClass, string argumentDescription, int index)
		{
			this.argumentClass = argumentClass;
			this.argumentDescription = argumentDescription;
			this.index = index;
			if (argumentClass == NodeClass.directive)
				this.nodeConnectedToSlot = new directiveTerminal();
		}
	}

	public class NodeSuccessors
	{
		public static NodeSuccessors empty = new NodeSuccessors(new Slot[0]);

		private Slot[] slots;
		private Node parrent;
		public IEnumerable<Slot> getSlots()
		{
			return slots;
		}

		public int count => slots.Length;
		public Slot GetSlot(int i) => slots[i];

		public void getSuccessors(List<Node> result)
		{
			result.Clear();
			foreach (var item in slots)
			{
				result.Add(item.nodeConnectedToSlot);
			}
		}

		public void setSuccessor(Node successor, int index)
		{
			slots[index].setNodeConnectedToSlot(successor);
			successor.predecessor = this.parrent;
			successor.indexInParrentsSlot = index;
		}

		private NodeSuccessors(Slot[] slots)
		{
			this.slots = slots;
		}

		public NodeSuccessors(Slot[] slots, Node parrentNode)
		{
			this.slots = slots;
			this.parrent = parrentNode;
		}

		[Obsolete]
		public NodeSuccessors(FunctionType functionType)
		{
			//deprecated - not implemented. Not supported anymore.
			throw new NotImplementedException();
		}

		public Node get(int index) => GetSlot(index).nodeConnectedToSlot;
	}

	public class NodeFactory
	{
		//TODO.. bezparametricke konstruktory dat privatni??

		public static Node createNode(NodeType type)
		{
			switch (type)
			{
				case NodeType.NumInput:
					return new InputNode(0);
				case NodeType.NumConst:
					return new NumericConstant(0);
				case NodeType.Log:
					return new LogFunction();
				case NodeType.Sin:
					return new SinFunction();
				case NodeType.Tan:
					return new TanFunction();
				case NodeType.Sqr:
					return new SquareFunction();
				case NodeType.Sqrt:
					return new RootFunction();
				case NodeType.Plus:
					return new PlusOperator();
				case NodeType.Minus:
					return new MinusOperator();
				case NodeType.Div:
					return new DivOperator();
				case NodeType.Mod:
					return new ModOperator();
				case NodeType.Multiply:
					return new MultiplyOperator();
				case NodeType.Rand:
					return new RandomGeneratorNode();
				case NodeType.Bool2Num:
					return new BoolToNumber();
				//case NodeType.BoolInput:
				//	return new BoolInput(0);
				case NodeType.BoolConstant:
					return new BoolConstant(false);
				case NodeType.Equals:
					return new BoolEqualsOperator();
				case NodeType.Less:
					return new BoolLessOperator();
				case NodeType.LessEq:
					return new BoolLessEqualOperator();
				case NodeType.AND:
					return new BoolANDOperator();
				case NodeType.OR:
					return new BoolOROperator();
				case NodeType.XOR:
					return new BoolXOROperator();
				case NodeType.NOT:
					return new BoolLogicNotOperator();
				case NodeType.dirTerminal:
					return new directiveTerminal();
				case NodeType.dirAssign:
					return new directiveAssign();
				case NodeType.dirAddFirst:
					return new directiveAddFirst();
				case NodeType.dirAddLast:
					return new directiveAddLast();
				case NodeType.dirDecrement:
					return new directiveDecrement();
				case NodeType.dirFor:
					return new directiveFOR();
				case NodeType.dirForeach:
					return new directiveFOREACH();
				case NodeType.dirIf:
					return new directiveIF();
				case NodeType.dirIfElse:
					return new directiveIFELSE();
				case NodeType.dirIncrement:
					return new directiveIncrement();
				case NodeType.dirRemoveFirst:
					return new directiveRemoveFirst();
				case NodeType.dirRemoveLast:
					return new directiveRemoveLast();
				case NodeType.dirWhile:
					return new directiveWHILE();
				//case NodeType.dirSequence:
				//	return new directivesSequence();
				case NodeType.ValueGetter:
					return new ValueGetter();
				case NodeType.ListGetFirst:
					return new ListGetFirst();
				case NodeType.ListGetLast:
					return new ListGetLast();
				case NodeType.ListGetValue:
					return new ListValueGetter();
				case NodeType.ListSizeGetter:
					return new ListSizeGetter();
				case NodeType.dirSetOutput:
					return new directiveSetOutput();
				case NodeType.dirEntryPoint:
					return new directiveEntryPoint();
				default:
					throw new Exception();
			}
		}

		public static List<Node> prototypes = new List<Node>()
		{
			 new InputNode(0),
			 new NumericConstant(0),
			 new LogFunction(),
			 new SinFunction(),
			 new TanFunction(),
			 new SquareFunction(),
			 new RootFunction(),
			 new PlusOperator(),
			 new MinusOperator(),
			 new DivOperator(),
			 new ModOperator(),
			 new MultiplyOperator(),
			 new RandomGeneratorNode(),
             new BoolToNumber(),
			 new BoolInput(0),
			 new BoolConstant(true),
			 new BoolEqualsOperator(),
			 new BoolLessOperator(),
			 new BoolLessEqualOperator(),
			 new BoolANDOperator(),
			 new BoolOROperator(),
			 new BoolXOROperator(),
			 new BoolLogicNotOperator()
		};
	}

	/// <summary>
	/// Represents some constrains that has to hold during creation of nodes. E.g. that the node must not be a constant etc.
	/// </summary>
	public class NodeCreationConstrains : HashSet<NodeCreationContrainsOption>
	{
		public static NodeCreationConstrains empty = new NodeCreationConstrains();

		public static bool isConstant(NodeType t)
		{
			switch (t)
			{
				case NodeType.NumConst:
				case NodeType.BoolConstant:
				case NodeType.dirTerminal:
					return true;
				default:
					return false;
			}
		}

		public bool isOK()
		{
			return true;
		}

		private List<Node> allAddedNodes;

		public void logNodeAdded(Node n)
		{
			allAddedNodes.Add(n);
		}



	}

	public enum NodeCreationContrainsOption
	{
		hasToBeConstant,
		cantBeConstant
	}

	public abstract class Node
	{
		public NodeCreationConstrains contrains;

		public int indexInParrentsSlot;
		public Node predecessor;

		public static BoolConstant defaultBoolNode => new BoolConstant(true);

		public static NumericNode defaultNumericNode => new NumericConstant(0);

        /// <summary>
        /// Creates a shallow copy of the object. Should not be called directly.
        /// </summary>
        /// <returns></returns>
		public virtual object Clone()
        {
            return NodeFactory.createNode(this.type);
        }

		public abstract string toSourceCode();

        /// <summary>
        /// Clones the object without its successors. Successors of the new object will be null.
        /// </summary>
        /// <returns></returns>
        public Node createShallowCopy()
        {
            Node result = (Node)this.Clone();
            return result;
        }

        /// <summary>
        /// Clones the object together with all of its successors. I.e. recreates the whole subtree.
        /// </summary>
        /// <returns></returns>
        public Node createDeepCopy()
        {
            Node result = (Node)this.Clone();
            for (int i = 0; i < this.successors.count; i++)
            {
                result.setSuccessor(this.successors.get(i).createDeepCopy(), i);
            }
            return result;
        }

		public abstract NodeClass getNodeClass();

		public virtual NodeSignature signature => new NodeSignature(NodeClass.directive, NodeClass.directive);

		public abstract NodeType type
		{
			get;
		}

		public NodeSuccessors successors;

		/// <summary>
		/// Gets all successors of this node. Successors are returned in the given list.
		/// </summary>
		/// <param name="result"></param>
		public virtual void getSuccessors(List<Node> result)
		{
			successors.getSuccessors(result);
		}

		/// <summary>
		/// Sets the nextDirective for this node.
		/// </summary>
		/// <param name="succ"></param>
		/// <param name="succIndex"></param>
		public void setSuccessor(Node succ, int succIndex)
		{
			if (this.successors == null)
				createSlots();
			successors.setSuccessor(succ, succIndex);
			succ.predecessor = this;
			succ.indexInParrentsSlot = succIndex;
		}

		public virtual string getLabel()
		{
			return this.ToString();
		}

		/// <summary>
		/// Creates slots according to number and types of successors of this node.
		/// </summary>
		protected virtual void createSlots()
		{
			this.successors = NodeSuccessors.empty;
		}

		public Node()
		{
			this.contrains = new NodeCreationConstrains();
            //this is important. This constructor is called every time some inherited object is created.
			createSlots();
		}

		/// <summary>
		/// Returns true if the given node is equal to this one, i.e. if they have the same type and same inner parameters. Doesn't check the successors of the node!
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public virtual bool isEqual(Node n)
		{
			//implementation that works in case the node has no inner parameters (which is most of them. Only BoolConstant and NumericConstant have parameter)
			return n.type == this.type;
		}

		/// <summary>
		/// Returns true if subtree of this node contains subtree given by the root <paramref name="n"/>
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public bool containsSubtree(Node n)
		{
			if (this.isEqual(n))
			{
				bool allSuccessorsEquals = true;
				for (int i = 0; i < this.successors.count; i++)
				{
					if (!this.successors.get(i).containsSubtree(n.successors.get(i)))
					{
						allSuccessorsEquals = false;
						break;
					}
				}
				if (allSuccessorsEquals)
					return true;
			}
			else
			{
				for (int i = 0; i < this.successors.count; i++)
				{
					if (this.successors.get(i).containsSubtree(n))
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Adds all nodes from subtree of this node (including this) to the given list <paramref name="resultsPlaceholder"/>
		/// </summary>
		/// <param name="resultsPlaceholder"></param>
		public void gatherAllNodesInSubtree(List<Node> resultsPlaceholder)
		{
			resultsPlaceholder.Add(this);
			for (int i = 0; i < this.successors.count; i++)
			{
				this.successors.get(i).gatherAllNodesInSubtree(resultsPlaceholder);
			}
		}
	}

	public static class EnumUtils
	{
		private static Lazy<List<NodeType>> terminalTypes = new Lazy<List<NodeType>>(createTerminalTypes);
		private static Lazy<List<NodeType>> nonTerminalTypes = new Lazy<List<NodeType>>(createNonterminalTypes);
		private static Lazy<List<NodeType>> allTypes = new Lazy<List<NodeType>>(createAllTypes);
		private static Lazy<List<NodeClass>> allClases = new Lazy<List<NodeClass>>(createAllNodeClasses);
		private static Lazy<Dictionary<NodeClass, List<NodeType>>> typesByClasses = new Lazy<Dictionary<NodeClass, List<NodeType>>>(createTypesByClasses);
		private static Lazy<List<NodeType>> allWriteNodeTypes = new Lazy<List<NodeType>>(createWriteNodeTypes);
		private static Lazy<Dictionary<NodeClass, List<NodeType>>> writeTypesByClasses = new Lazy<Dictionary<NodeClass, List<NodeType>>>(createWriteTypesByClasses);

		public static NodeClass getClassFromType(NodeType t)
		{
			switch (t)
			{
				case NodeType.NumInput:
				case NodeType.NumConst:
				case NodeType.Log:
				case NodeType.Sin:
				case NodeType.Tan:
				case NodeType.Sqr:
				case NodeType.Sqrt:
				case NodeType.Plus:
				case NodeType.Minus:
				case NodeType.Div:
				case NodeType.Mod:
				case NodeType.Multiply:
				case NodeType.Rand:
				case NodeType.Bool2Num:
				case NodeType.ValueGetter:
				case NodeType.ListGetFirst:
				case NodeType.ListGetLast:
				case NodeType.ListGetValue:
				case NodeType.ListSizeGetter:
					return NodeClass.numeric;
				//case NodeType.BoolInput:
				case NodeType.BoolConstant:
				case NodeType.Equals:
				case NodeType.Less:
				case NodeType.LessEq:
				case NodeType.AND:
				case NodeType.OR:
				case NodeType.XOR:
				case NodeType.NOT:
					return NodeClass.boolean;
				case NodeType.dirFor:
				case NodeType.dirForeach:
				case NodeType.dirWhile:
				case NodeType.dirAddLast:
				case NodeType.dirAddFirst:
				case NodeType.dirRemoveFirst:
				case NodeType.dirRemoveLast:
				//case NodeType.dirSequence:
				case NodeType.dirIf:
				case NodeType.dirIfElse:
				case NodeType.dirAssign:
				case NodeType.dirIncrement:
				case NodeType.dirDecrement:
				case NodeType.dirTerminal:
				case NodeType.dirSetOutput:
				case NodeType.dirEntryPoint:
					return NodeClass.directive;
				default:
					throw new Exception();
			}
		}

		private static List<NodeType> createAllTypes()
		{
			return Enum.GetValues(typeof(NodeType)).Cast<NodeType>().OrderBy(t => NodeFactory.createNode(t).successors.count).ToList();
		}

		public static IEnumerable<NodeType> getAllTypes()
		{
			return allTypes.Value;
		}

		private static List<NodeClass> createAllNodeClasses()
		{
			return Enum.GetValues(typeof(NodeClass)).Cast<NodeClass>().ToList();
		}

		public static IEnumerable<NodeClass> getAllNodeClasses()
		{
			return allClases.Value;
		}

		private static Dictionary<NodeClass, List<NodeType>> createTypesByClasses()
		{
			return getAllNodeClasses().ToDictionary(c => c, c => getAllTypes().Where(t => t != NodeType.dirEntryPoint).Where(t => getClassFromType(t) == c).ToList());  
			//directive entry point is not considered here as it is a special symbol that occurs exactly once in each program
		}

		public static IEnumerable<NodeType> getTypesByClass(NodeClass c)
		{
			return typesByClasses.Value[c];
		}

		private static List<NodeType> createTerminalTypes()
		{
			return getAllTypes().Where(t => NodeFactory.createNode(t).successors.count == 0).ToList();
		}

		private static List<NodeType> createNonterminalTypes()
		{
			return getAllTypes().Where(t => t != NodeType.dirEntryPoint && NodeFactory.createNode(t).successors.count != 0).ToList();
		}

		/// <summary>
		/// Type is terminal, if it doesn't have any successors.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<NodeType> getTerminalTypes()
		{
			return terminalTypes.Value;
		}

		/// <summary>
		/// Type is terminal, if it doesn't have any successors.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<NodeType> getNonterminalTypes()
		{
			return nonTerminalTypes.Value;
		}

		/// <summary>
		/// Type is terminal, if it doesn't have any successors.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<NodeType> getTerminalTypes(NodeClass cl)
		{
			return terminalTypes.Value.Where(t => getClassFromType(t) == cl);
		}

		/// <summary>
		/// Type is terminal, if it doesn't have any successors.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<NodeType> getNonterminalTypes(NodeClass cl)
		{
			return nonTerminalTypes.Value.Where(t => getClassFromType(t) == cl);
		}

		private static List<NodeType> createWriteNodeTypes()
		{
			return new List<NodeType>()
			{
				NodeType.dirAddFirst,
				NodeType.dirAddLast,
				NodeType.dirAssign,
				NodeType.dirDecrement,
				NodeType.dirIncrement,
				NodeType.dirRemoveFirst,
				NodeType.dirRemoveLast,
				NodeType.dirSetOutput,
			};
		}

		private static Dictionary<NodeClass, List<NodeType>> createWriteTypesByClasses()
		{
			return getAllNodeClasses().ToDictionary(c => c, c => allWriteNodeTypes.Value.Where(t => getClassFromType(t) == c).ToList());
		}

		/// <summary>
		/// NodeType is considered "writeType" if it writes something, i.e. if it changes value of some variable.
		/// </summary>
		/// <param name="cl"></param>
		/// <returns></returns>
		public static IEnumerable<NodeType> getWriteNodeTypes(NodeClass cl)
		{
			return writeTypesByClasses.Value[cl];
		}

	}



}
