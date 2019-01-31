using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interpreter;
using Utils;

namespace ProgramBreeder
{
	class Program
	{
		public static Random r = new Random(1234);

		//TODO 
		//dodelat spravne odsazovani u metody tostring u Nodu

		//do profilu (nebo jinam) dat pravidla co kde muze a nesmi byt. napr. v tele forcyklu se musi vyskytovat ridici promenna, u operatoru nesmi byt jen konstanty
		//to se da udelat tak, ze prvni argument operatoru nesmi byt konstanta (a to by se propagovalo hloubs)
		//v celem programu se musi prirazovat do vysledku a cist vstupy atd..

		//pri generovani podstromu by hloubka nemela byt pouze fixni, ale mely by tam byt vsechny stromy s hloubkou mensi rovnou danemu limitu

		//profil nebude uniformni, ale podmineny, tzn. nebude dana "globalni pravdepodobnost vyskytu typu uzlu", ale "pravdepodobnost vyskytu typu uzlu v zavislosti na typu rodice"
		//To by melo omezit vyskyt ReadVar -> ReadVar a podobne.	-- DONE

		//pri hodnoceni uspesnosti programu je nutne brat v uvahu nejen vysledek, ale i neco dalsiho. Napr. hodnoty vnitrnich promennych na konci, posloupnost hodnot vnitrnich promennych v prubehu vypoctu, nebo neco podobneho
		//obecne by to mely byt kriteria, ktera bud zada uzivatel, nebo se najdou automaticky (pomoci "korelaci" vstupu s pozadovanymi vystupy, resp. kriterium je "uzitecne" pokud pro vsechny x: korelace(krit(x), target(x)) je velka.
		//Pricemz "korelace" je spis v Kolmogorovskem smyslu - tzn. {a_i} a {b_i} maji velkou korelaci pokud program, ktery pro vsechny "i" prevadi a_i na b_i je kratky)

		//prohledavani prostoru programu:
		//A* s heuristikou, kde heuristika je "velikost programu, ktery prevadi vystup toho, co mame ted, na pozadovany vystup" (pomoci aproximace kolmogorovske slozitosti)
		//MCTS - podobne. program se bude stavet pomoci MCTS, heuristika bude ohodnoceni, pravdepodobne pouzit AMAF, v listech delat samplovani (vyhodnoti se vzdycky jen na malem mnozstvi samplu pri kazde navsteve)


		/// <summary>
		/// Enumerates all trees of given depth
		/// </summary>
		/// <param name="args"></param>
		static void Main5(string[] args)
        {
            NodeTypeFrequencyProfile p = new NodeTypeFrequencyProfile();
            for (int depth = 3; depth < 5; depth++)
            {
                //Console.WriteLine("number of graphs of depth " + depth + ": " + TreeIterator.enumerateAllTrees(NodeClass.directive, depth, p).Count());
                //continue;

                foreach (var tree in TreeIterator.enumerateAllTrees(NodeClass.directive, depth, p))
                {
                    new GraphVisualizer().visualizeDialog(ProgramTreeDrawer.createGraph(new SolutionProgram((DirectiveNode)(tree))));
                }
            }
        }

		/// <summary>
		/// To test the primality testing program and Erratic sequence program
		/// </summary>
		/// <param name="args"></param>
        static void Main4(string[] args)
		{
			runEvaluation(new PrimalityTestingSamplesGenerator(), getProgram2(), 10000, quiet: true);
			new GraphVisualizer().visualizeDialog(ProgramTreeDrawer.createGraph(getProgram2()));

			//applies a random mutation to the program and evaluates it. Repeates 10-times.
			NodeTypeRelativizedFrequencyProfile prof = NodeTypeRelativizedFrequencyProfile.createProfile(new List<SolutionProgram> { getProgram0(), getProgram1(), getProgram2() });
			PointMutation m = new PointMutation(prof, 1);
			for (int i = 0; i < 10; i++)
			{
				var p = getProgram2();
				m.modify(p);
				new GraphVisualizer().visualizeDialog(ProgramTreeDrawer.createGraph(p));
				runEvaluation(new PrimalityTestingSamplesGenerator(), p, 10000, quiet: true);
			}
			//runEvaluation(new ErraticSequenceLengthSamplesGenerator(), getProgram1(), 10000);

		}

		/// <summary>
		/// Creates and draws examples of randomly generated programs according to some profile.
		/// </summary>
		/// <param name="args"></param>
		static void Main1(string[] args)
		{
			/*
			//NodeTypeFrequencyProfile p = NodeTypeFrequencyProfile.createProfile(new List<SolutionProgram> { getProgram0(), getProgram1(), getProgram2() }, atLeastOneOfEach: true);
			NodeTypeRelativizedFrequencyProfile p = NodeTypeRelativizedFrequencyProfile.createProfile(new List<SolutionProgram> { getProgram0(), getProgram1(), getProgram2() });

			for (int i = 0; i < 50; i++)
			{
				//SolutionProgram pr = new SolutionProgram((DirectiveNode)SearchMethodsSupport.createRandomTree(NodeClass.directive, p, 10));
				SolutionProgram pr = new SolutionProgram((DirectiveNode)SearchMethodsSupport.createRandomTree(NodeClass.directive, p, 10, null));
				Console.WriteLine(pr.ToString());
				new GraphVisualizer().visualizeDialog(ProgramTreeDrawer.createGraph(pr));
				Console.WriteLine();
			}
			*/
			

			
			//enumerates and visualizes all trees with depth <= given number
			int depth = 2;
			foreach (var item in SearchMethodsSupport.enumerateAllTrees(NodeClass.numeric, depth))
			{
				new GraphVisualizer().visualizeDialog(ProgramTreeDrawer.createGraph(item));
			}
			
		}

		static void Main2(string[] args)
		{
			//SolutionProgram p = getProgram0();
			//SolutionProgram p = getProgram1();
			SolutionProgram p = getProgram2();

			Interpret i = new Interpret();
			i.numericInputs = new List<int>() { 97 };
			Console.WriteLine(p.ToString());
			p.execute(i);
			new GraphVisualizer().visualizeDialog(ProgramTreeDrawer.createGraph(p));
			Console.WriteLine("inputs: " + string.Join(" ", i.numericInputs));
			Console.WriteLine("outputs: " + string.Join(" ", i.outputs));
		}

		/// <summary>
		/// Main function for testing the search.
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			//NodeTypeFrequencyProfile p = NodeTypeFrequencyProfile.createProfile(new List<SolutionProgram> { getProgram0(), getProgram1(), getProgram2() }, true);
			NodeTypeRelativizedFrequencyProfile p = NodeTypeRelativizedFrequencyProfile.createProfile(new List<SolutionProgram> { getProgram0(), getProgram1(), getProgram2() });

			//TrainingSamplesGenerator g = new SimpleCopySamplesGenerator();
			TrainingSamplesGenerator g = new MaximumOfTwo_SamplesGenerator();
			//TrainingSamplesGenerator g = new AlwaysOne_SamplesGenerator();

			//RandomMutationSearch s = new RandomMutationSearch(generator: g, mutationsInEachStep: 5, profile: p);
			SearchAlgorithm s = new FullEnumerationSearch(generator: g);

			var res = s.search(TimeSpan.FromMinutes(10));
			new GraphVisualizer().visualizeDialog(ProgramTreeDrawer.createGraph(res));
		}

		/// <summary>
		/// To test the Clone() method
		/// </summary>
		/// <param name="args"></param>
		static void Main3(string[] args)
		{
			NodeTypeFrequencyProfile p = NodeTypeFrequencyProfile.createProfile(new List<SolutionProgram> { getProgram0(), getProgram1(), getProgram2() }, true);

			for (int i = 0; i < 50; i++)
			{
				Console.WriteLine("New random program:");
				SolutionProgram pr = new SolutionProgram((DirectiveNode)SearchMethodsSupport.createRandomTree(NodeClass.directive, p, 10));
				Console.WriteLine(pr.ToString());
				new GraphVisualizer().visualizeDialog(ProgramTreeDrawer.createGraph(pr));
				Console.WriteLine();
				Console.WriteLine("CLONE:");
				SolutionProgram clon = (SolutionProgram)pr.Clone();
				Console.WriteLine(clon.ToString());
				new GraphVisualizer().visualizeDialog(ProgramTreeDrawer.createGraph(clon));
				Console.WriteLine();
			}
		}

		/// <summary>
		/// Program enumerates numbers from 0 to given number and sums every second number in such list. Returns the sum.
		/// </summary>
		/// <returns></returns>
		static SolutionProgram getProgram0()
		{
			SolutionProgram p = new SolutionProgram(new List<DirectiveNode>()
			{
				new directiveFOR(new NumericConstant(0), new InputNode(0), new directiveAddLast(
					new NumericConstant(0), new ValueGetter(new NumericConstant(0)))),

				new directiveAssign(new NumericConstant(1), new NumericConstant(0)),
				new directiveAssign(new NumericConstant(2), new NumericConstant(0)),
				new directiveWHILE(new BoolLessOperator(new ValueGetter(new NumericConstant(1)), new ListSizeGetter(new NumericConstant(0))),
					new directivesSequence(new List<DirectiveNode>()
					{
						new directiveAssign(new NumericConstant(2), new PlusOperator(new ValueGetter(new NumericConstant(2)), new ListValueGetter(new NumericConstant(0), new ValueGetter(new NumericConstant(1))))),
						new directiveIncrement(new NumericConstant(1)),
						new directiveIncrement(new NumericConstant(1)),
					}
					)),
				new directiveSetOutput(new NumericConstant(0), new ValueGetter(new NumericConstant(2)))
			});
			return p;
		}

		/// <summary>
		/// Program that repeatedly transforms given number by n % 2 == 0 ? n = n/2 : n = n*3 + 1, and count the number of steps required before n reaches value 1.
		/// </summary>
		/// <returns></returns>
		static SolutionProgram getProgram1()
		{
			SolutionProgram p = new SolutionProgram(new List<DirectiveNode>()
			{
				new directiveAssign(new NumericConstant(0), new InputNode(0)),
				new directiveAssign(new NumericConstant(1), new NumericConstant(0)),
				new directiveWHILE(
					new BoolLogicNotOperator(
						new BoolEqualsOperator(
							new ValueGetter(new NumericConstant(0)),
							new NumericConstant(1))),
					new directivesSequence(new List<DirectiveNode>()
					{
						new directiveIFELSE(
							new BoolEqualsOperator(new ModOperator(new ValueGetter(new NumericConstant(0)), new NumericConstant(2)), new NumericConstant(0)),
							new directiveAssign(new NumericConstant(0), new DivOperator(new ValueGetter(new NumericConstant(0)), new NumericConstant(2))),
							new directiveAssign(new NumericConstant(0), new PlusOperator(new MultiplyOperator(new ValueGetter(new NumericConstant(0)), new NumericConstant(3)), new NumericConstant(1)))),
						new directiveIncrement(new NumericConstant(1))
					})),
					new directiveSetOutput(new NumericConstant(0), new ValueGetter(new NumericConstant(1)))
			});
			return p;
		}

		/// <summary>
		/// Simple program for testing whether given number is a prime number
		/// </summary>
		/// <returns></returns>
		static SolutionProgram getProgram2()
		{
			SolutionProgram p = new SolutionProgram(new List<DirectiveNode>()
			{
				new directiveAssign(new NumericConstant(0), new InputNode(0)),
				new directiveAssign(new NumericConstant(1), new NumericConstant(2)),
				new directiveAssign(new NumericConstant(2), new NumericConstant(1)),
				new directiveWHILE(
					new BoolANDOperator(
						new BoolLessEqualOperator(
							new ValueGetter(new NumericConstant(1)),
							new DivOperator(new ValueGetter(new NumericConstant(0)), new ValueGetter(new NumericConstant(1)))

							),
						new BoolEqualsOperator(
							new ValueGetter(new NumericConstant(2)),
							new NumericConstant(1))
							),
					new directivesSequence(new List<DirectiveNode>()
					{
						new directiveIF(
							new BoolEqualsOperator(new ModOperator(new ValueGetter(new NumericConstant(0)), new ValueGetter(new NumericConstant(1))), new NumericConstant(0)),
							new directiveAssign(new NumericConstant(2), new NumericConstant(0))),
						new directiveIncrement(new NumericConstant(1))
					})),
				new directiveSetOutput(new NumericConstant(0), new ValueGetter(new NumericConstant(2)))
			});
			return p;
		}

		/// <summary>
		/// Evaluates given solution program using given samplesGenerator. Prints a comparison of desiredOutputs to realOutputs and computes success rate.
		/// </summary>
		/// <param name="generator"></param>
		/// <param name="p"></param>
		/// <param name="samplesCount"></param>
		public static void runEvaluation(TrainingSamplesGenerator generator, SolutionProgram p, int samplesCount, bool quiet = false)
		{
			Interpret i = new Interpret();
			int currectCount = 0;
			foreach (var sample in generator.generateSamples(samplesCount))
			{
				p.evaluate(sample, i);
				Printing.PrintMsg(sample.ToString() + "\treal output: " + sample.realOutputs2String + " " + (sample.isCorrectlyAnswered ? "OK" : "WRONG"), quiet);
				if (sample.isCorrectlyAnswered)
					currectCount++;
			}
			Console.WriteLine($"Success rate: {currectCount} out of {samplesCount} ({((double)(currectCount*100) / samplesCount).ToString("0.##")}%)");
		}



	}
}
