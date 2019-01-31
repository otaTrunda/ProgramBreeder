using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interpreter;
using System.Diagnostics;

namespace ProgramBreeder
{
	abstract class SearchAlgorithm
	{
		protected void printMSG(string message, bool quiet = false)
		{
			if (quiet)
				return;
			Console.WriteLine(message);
		}

		protected TrainingSamplesGenerator generator;
		protected Interpret i;
		protected bool quiet = false;
		//protected TimeSpan timeLimit = TimeSpan.FromMinutes(5);
		protected Stopwatch watch;

		public abstract SolutionProgram search(TimeSpan timeLimit);

		/// <summary>
		/// Returns the percentage of correctly answered tests
		/// </summary>
		/// <param name="batchSize"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		protected virtual double evaluate(int batchSize, SolutionProgram p)
		{
			int currectCount = 0;
			//var samples = generator.generateSamples(batchSize).ToList();
			foreach (var sample in generator.generateSamples(batchSize))
			{
				p.evaluate(sample, i);
				bool isCorrect = sample.isCorrectlyAnswered;
				printMSG(sample.ToString() + "\treal output: " + sample.realOutputs2String + " " + (isCorrect ? "OK" : "WRONG"), true);
				if (isCorrect)
					currectCount++;
			}
			return ((double)(currectCount * 100) / batchSize);
		}

		protected SearchAlgorithm(TrainingSamplesGenerator generator)
		{
			this.watch = new Stopwatch();
			this.generator = generator;
			this.i = new Interpret();
		}

		/// <summary>
		/// Adds evaluations to each program in the sequence
		/// </summary>
		/// <param name="batchSize"></param>
		/// <param name="programs"></param>
		protected virtual void addEvaluation(int batchSize, List<EvaluatedEntity<SolutionProgram>> programs)
		{
			for (int i = 0; i < programs.Count; i++)
			{
				double evaluation = evaluate(batchSize, programs[i].item);
				programs[i].value = evaluation;
			}
		}
	}

	/// <summary>
	/// Creates an initial randomProgram and then modifies it by series of random mutations. In each step, the best performing mutated program is selected and others are discarted. 
	/// This goes on until time runs out or until 100% performance is achieved.
	/// </summary>
	class RandomMutationSearch : SearchAlgorithm
	{
		private int mutationsInStep;
		private List<EvaluatedEntity<SolutionProgram>> mutatedPrograms;
		private int batchSize = 100;
		private NodeTypeFrequencyProfile profile;
		PointMutation mutator;
		private Selector<SolutionProgram> selector = new RouletteSelector<SolutionProgram>();

		public RandomMutationSearch(TrainingSamplesGenerator generator, int mutationsInEachStep, NodeTypeFrequencyProfile profile)
			:base(generator)
		{
			this.mutationsInStep = mutationsInEachStep;
			this.profile = profile;
			this. mutator = new PointMutation(profile, 3);
			this.mutatedPrograms = new List<EvaluatedEntity<SolutionProgram>>();
		}

		public override SolutionProgram search(TimeSpan timeLimit)
		{
			watch.Start();

			SolutionProgram current = new SolutionProgram((DirectiveNode)SearchMethodsSupport.createRandomTree(NodeClass.directive, profile, 3));
			int steps = 0;
			while (watch.Elapsed < timeLimit)
			{
				steps++;
				mutatedPrograms.Clear();
				for (int i = 0; i < mutationsInStep; i++)
				{
                    SolutionProgram mutated = (SolutionProgram)current.Clone();
					mutator.modify(mutated);
					mutatedPrograms.Add(new EvaluatedEntity<SolutionProgram>(mutated, 0));
				}
				mutatedPrograms.Add(new EvaluatedEntity<SolutionProgram>(new SolutionProgram((DirectiveNode)SearchMethodsSupport.createRandomTree(NodeClass.directive, profile, 3)), 0));
				addEvaluation(batchSize, mutatedPrograms);
				var selected = selector.select(mutatedPrograms);
				if (selected.value >= 99)	//more than 99% accuracy reached -> ending the search.
				{
					printMSG(selected.value.ToString());
					return selected.item;
				}

				printMSG("Steps: " + steps);
				printMSG("Best evaluation: " + mutatedPrograms.Max(p => p.value));
				printMSG("Best program: " + mutatedPrograms.Where(p => p.value >= mutatedPrograms.Max(q => q.value)).First().item.ToString());
			}
			watch.Stop();
			return mutatedPrograms.Where(p => p.value >= mutatedPrograms.Max(q => q.value)).First().item;
		}
	}

	/// <summary>
	/// Keeps creating random programs, evaluating them and storing the one with best performance
	/// </summary>
	class RandomSearch : SearchAlgorithm
	{
		NodeTypeFrequencyProfile profile;
		int maxDepth;
		int batchSize;

		public override SolutionProgram search(TimeSpan timeLimit)
		{
			watch = Stopwatch.StartNew();
			var bestProgram = new SolutionProgram((DirectiveNode)SearchMethodsSupport.createRandomTree(NodeClass.directive, profile, maxDepth));
			var bestPerformance = evaluate(batchSize, bestProgram);
			Console.WriteLine("best program: ");
			Console.WriteLine(bestProgram.ToString());
			Console.WriteLine($"performance: {bestPerformance}");

			while (watch.Elapsed < timeLimit)
			{
				var program = new SolutionProgram((DirectiveNode)SearchMethodsSupport.createRandomTree(NodeClass.directive, profile, maxDepth));
				var performance = evaluate(batchSize, program);
				if (performance > bestPerformance)
				{
					bestPerformance = performance;
					bestProgram = program;
					Console.WriteLine("best program: ");
					Console.WriteLine(bestProgram.ToString());
					Console.WriteLine($"performance: {bestPerformance}");
				}
			}
			return bestProgram;
		}

		public RandomSearch(TrainingSamplesGenerator gen, int maxDepth, NodeTypeFrequencyProfile profile, int batchSize = 128)
			:base(gen)
		{
			this.profile = profile;
			this.maxDepth = maxDepth;
			this.batchSize = batchSize;
		}
	}

	class FullEnumerationSearch : SearchAlgorithm
	{
		int batchSize;

		public FullEnumerationSearch(TrainingSamplesGenerator generator, int batchSize = 128)
			: base(generator)
		{
			this.batchSize = batchSize;
		}

		public override SolutionProgram search(TimeSpan timeLimit)
		{
			watch = Stopwatch.StartNew();
			SolutionProgram bestProgram = null;
			double bestPerformance = -1d;

			int depth = 0;
			while(true)
			{
				depth++;
				Console.WriteLine($"Depth: {depth}");
				//Console.WriteLine(SearchMethodsSupport.enumerateAllTrees(NodeClass.directive, depth).Count());
				foreach (var item in SearchMethodsSupport.enumerateAllTrees(NodeClass.directive, depth))
				{
					if (watch.Elapsed > timeLimit)
						return bestProgram;

					var program = new SolutionProgram((DirectiveNode)item);
					var performance = evaluate(batchSize, program);
					//Console.WriteLine(program.ToString());
					//Console.WriteLine($"performance: {performance}");

					if (performance > bestPerformance)
					{
						bestPerformance = performance;
						bestProgram = program;
						Console.WriteLine("best program: ");
						Console.Write(bestProgram.ToString());
						Console.WriteLine($"performance: {bestPerformance} %");
						Console.WriteLine();
						if (performance >= 100)
						{
							Console.WriteLine("solution found");
							return bestProgram;
						}
					}
				}
			}		
		}
	}
}
