using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Msagl.Drawing;
using Interpreter;
using graphNode = Microsoft.Msagl.Drawing.Node;
using Node = Interpreter.Node;
using System.Runtime.Serialization;

namespace ProgramBreeder
{
	class ProgramTreeDrawer
	{
		private static ObjectIDGenerator IDgen = new ObjectIDGenerator();

		public static Graph createGraph(Node n)
		{
			Graph g = new Graph();
			graphNode previous = createStartNode();
			g.AddNode(previous);

			graphNode next = generateSubtree(n, g);
			g.AddEdge(previous.Id, next.Id);
			/*
			while (d != null)
			{
				graphNode next = generateSubtree(d, g);
				g.AddNode(next);
				g.AddEdge(previous.Id, next.Id);
				previous = next;
				d = d.getNextDirective();
			}
			*/
			return g;
		}

		public static Graph createGraph(SolutionProgram p)
		{
			Graph g = new Graph();
			graphNode previous = createStartNode();
			g.AddNode(previous);

			DirectiveNode d = p.getEntryPoint();
			graphNode next = generateSubtree(d, g);
			g.AddEdge(previous.Id, next.Id);
			/*
			while (d != null)
			{
				graphNode next = generateSubtree(d, g);
				g.AddNode(next);
				g.AddEdge(previous.Id, next.Id);
				previous = next;
				d = d.getNextDirective();
			}
			*/
			return g;
		}

		private static graphNode createGraphNode(Node n)
		{
			bool dummyVar;
			long id = IDgen.GetId(n, out dummyVar);
			graphNode no = new graphNode(id.ToString());
			setNodeAttributes(no.Attr, n.type);
			no.LabelText = n.getLabel();

			return no;
		}

		private static void setNodeAttributes(NodeAttr attr, NodeType type)
		{
			NodeClass c = EnumUtils.getClassFromType(type);
			attr.Shape = Shape.Circle;
			switch (c)
			{
				case NodeClass.directive:
					attr.Color=Color.Black;
					attr.Shape = Shape.Box;
					break;
				case NodeClass.numeric:
					attr.Color = Color.Red;
					break;
				case NodeClass.boolean:
					attr.Color = Color.Blue;
					break;
				default:
					attr.Color = Color.Gray;
					break;
			}
			switch (type)
			{
				case NodeType.dirTerminal:
					attr.Shape = Shape.Diamond;
					break;
				case NodeType.NumInput:
					//attr.FillColor = Color.DarkGoldenrod;
					attr.Shape = Shape.DoubleCircle;
					break;
			}

		}

		private static graphNode createStartNode()
		{
			graphNode no = new graphNode("start");
			no.Attr.Color = Color.Green;
			//no.Attr.FillColor = Color.GreenYellow;
			no.Attr.Shape = Shape.Diamond;
			return no;
		}

		private static graphNode generateSubtree(Node n, Graph g)
		{
			graphNode no = createGraphNode(n);
			g.AddNode(no);
			List<Node> successors = new List<Node>();
			n.getSuccessors(successors);
			int succIndex = 0;
			foreach (var succ in successors)
			{
				graphNode succNode = generateSubtree(succ, g);
				Edge e = g.AddEdge(no.Id, n.successors.GetSlot(succIndex).argumentDescription, succNode.Id);
				if (n.successors.GetSlot(succIndex).argumentDescription == "next directive") //is next successor edge
				{
					e.Attr.Color = Color.Brown;
					e.Attr.ArrowheadAtTarget = ArrowStyle.Diamond;
				}

				succIndex++;
			}
			return no;
		}

		private static Shape GetShapeByType(NodeType type)
		{
			switch(type)
			{
				case NodeType.dirTerminal:
					return Shape.Diamond;
				case NodeType.dirAddFirst:
				case NodeType.dirAddLast:
				case NodeType.dirAssign:
				case NodeType.dirDecrement:
				case NodeType.dirFor:
				case NodeType.dirForeach:
				case NodeType.dirIf:
				case NodeType.dirIfElse:
				case NodeType.dirIncrement:
				case NodeType.dirRemoveFirst:
				case NodeType.dirRemoveLast:
				//case NodeType.dirSequence:
				case NodeType.dirWhile:
					return Shape.Box;
				default:
					return Shape.Circle;
			}
		}

	}

	/*
	partial class GraphVisualizer : Form
	{
		Microsoft.Msagl.GraphViewerGdi.GViewer viewer;
		//Graph MSAGL_graph;
		TextBox articleURL_TextBox;
		private bool useAsDialog = true;

		public GraphVisualizer(bool useAsDialog = true)
		{
			InitializeComponent();

			if (!useAsDialog)
			{
				this.AllowDrop = true;
				this.DragEnter += new DragEventHandler(dragEnterHandler);
				this.DragDrop += new DragEventHandler(dropItemHandler);
			}

			viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
			viewer.CurrentLayoutMethod = Microsoft.Msagl.GraphViewerGdi.LayoutMethod.IcrementalLayout;
			articleURL_TextBox = new TextBox();
			articleURL_TextBox.Dock = DockStyle.Bottom;
			this.Controls.Add(articleURL_TextBox);
			viewer.Dock = DockStyle.Fill;
			this.Controls.Add(viewer);
		}

		void dragEnterHandler(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
		}

		void dropItemHandler(object sender, DragEventArgs e)
		{
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			//visualize(EntityGraph.createEntityGraph(DatabaseOperator.deserialize(files[0])));
			visualize(SemanticRolesGraph.createSemanticRolesGraph(Serialization.Deserialize<ParsedWatson>(files[0])));
		}

		public void visualizeDialog(GraphVisualizable visualizableEntity)
		{
			viewer.Graph = visualizableEntity.toMSAGL_Graph();
			this.Text = "GraphVisualizer " + visualizableEntity.getSourceURL();
			this.articleURL_TextBox.Text = visualizableEntity.getSourceURL();
			this.ShowDialog();
		}

		public void visualize(GraphVisualizable visualizableEntity)
		{
			viewer.Graph = visualizableEntity.toMSAGL_Graph();
			this.Text = "GraphVisualizer " + visualizableEntity.getSourceURL();
			this.articleURL_TextBox.Text = visualizableEntity.getSourceURL();
		}
	}*/
}
