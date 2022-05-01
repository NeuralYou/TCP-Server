using System;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

public class Population
{
	private List<NeuralNetwork> pop;
	public float m_MutationRate;
	int m_PopulationSize;
	public float AverageFitness
	{
		get
		{
			float sum = 0;
			foreach(NeuralNetwork n in Elements)
			{
				sum += n.Fitness;
			}

			return sum / Elements.Count;
		}
	}

	public List<NeuralNetwork> Elements
	{
		get { return pop; }
		private set { pop = value; }
	}

	public Population(float i_MutationRate, int i_PopulationSize, int i_Inputs, int i_Outputs)
	{
		m_MutationRate = i_MutationRate;
		m_PopulationSize = i_PopulationSize;

		initPopulation(i_Inputs, i_Outputs);
	}

	public Population(NeuralNetwork[] networks, float i_MutationRate)
	{
		pop = new List<NeuralNetwork>(networks);
		m_PopulationSize = pop.Count;
		m_MutationRate = i_MutationRate;
	}

	private void initPopulation(int i_Inputs, int i_Outputs)
	{
		pop = new List<NeuralNetwork>();
		for(int i = 0; i < m_PopulationSize; i++)
		{
			NeuralNetwork n = new NeuralNetwork(i_Inputs, i_Outputs);
			pop.Add(n);
		}
	}

	public int Size()
	{
		return pop.Count;
	}

	public void ApplyGeneticOperators()
	{
		List<NeuralNetwork> newGeneration = new List<NeuralNetwork>();
		int elitistsAmount = Elitists(newGeneration);
		Select(newGeneration);
		Crossover(elitistsAmount);
		Mutate(elitistsAmount);
		ResetFitness();
	}
	private int Elitists(List<NeuralNetwork> newGeneration)
	{
		Elements.Sort();
		int fivePrecent = (int) (pop.Count * 0.05f);
		newGeneration.AddRange(Elements.GetRange(Elements.Count - fivePrecent, fivePrecent));
		Console.WriteLine("Added " + fivePrecent);
		return fivePrecent;
	}

	public void Select(List<NeuralNetwork> newGeneration)
	{
		int cloneLimit = 3;
		List<int> cloneCounters = new int[Elements.Count].ToList();
		List<NeuralNetwork> tempPop = new List<NeuralNetwork>(Elements);
		
		for(int i = newGeneration.Count; i < m_PopulationSize; i++)
		{
			NeuralNetwork winner = ThreeWayTournement(tempPop);
			int winnerIndex = tempPop.FindIndex((NeuralNetwork other) => winner.Equals(other));
			cloneCounters[winnerIndex]++;
			newGeneration.Add(tempPop[winnerIndex]);

			if (cloneCounters[winnerIndex] >= cloneLimit)
			{
				tempPop.RemoveAt(winnerIndex);
				cloneCounters.RemoveAt(winnerIndex);
			}
		}

		Elements = newGeneration;

		for(int i = 0; i < Elements.Count; i++)
		{
			Elements[i] = Elements[i].Clone();
		}

		//for(int i = newGeneration.Count; i < m_PopulationSize; i++)
		//{
		//	newGeneration.Add(ThreeWayTournement());
		//}
	}

	//Crossover type is 1-point crossover
	public void Crossover(int elitistAmount)
	{
		for(int i = elitistAmount; i < Elements.Count; i++)
		{
			if(RandomUtils.RollOdds(0.4f))
			{
				NeuralNetwork other = Elements[RandomUtils.RandomRange(elitistAmount, Elements.Count)];

				float[] genome1 = Elements[i].Genome;
				float[] genome2 = other.Genome;

				int middle = genome1.Length / 2;
				List<float> newGenome1 = new List<float>(genome1[0..middle]);
				newGenome1.AddRange(genome2[middle..]);

				List<float> newGenome2 = new List<float>(genome2[0..middle]);
				newGenome2.AddRange(genome1[middle..]);

				Elements[i].Genome = newGenome1.ToArray();
				other.Genome = newGenome2.ToArray();
			}
		}
	}

	public void Mutate(int elitistsAmount)
	{
		Random rand = new Random();
		float average = AverageFitness;

		for(int i = elitistsAmount; i < Elements.Count; i++)
		{
			if (RandomUtils.RollOdds(m_MutationRate))
			{
				bool aboveAverage = Elements[i].Fitness >= average;
				Elements[i].MutateNetwork(aboveAverage);
			}
		}
	}
	private void ResetFitness()
	{
		foreach(NeuralNetwork n in Elements)
		{
			n.Fitness = 0;
		}
	}

	public string[] SerializeNetworks()
	{ 
		List<string> list = new List<string>();

		foreach(NeuralNetwork n in Elements)
		{
			string json = JsonConvert.SerializeObject(n, Formatting.Indented);
			list.Add(json);
		}

		return list.ToArray();
	}

	private NeuralNetwork ThreeWayTournement()
	{
		NeuralNetwork p1 = RandomUtils.RandomElement(Elements);
		NeuralNetwork p2 = RandomUtils.RandomElement(Elements);
		NeuralNetwork p3 = RandomUtils.RandomElement(Elements);

		return Fitter(Fitter(p1, p2), p3);
	}
	
	private NeuralNetwork ThreeWayTournement(List<NeuralNetwork> elements)
	{
		NeuralNetwork p1 = RandomUtils.RandomElement(elements);
		NeuralNetwork p2 = RandomUtils.RandomElement(elements);
		NeuralNetwork p3 = RandomUtils.RandomElement(elements);

		return Fitter(Fitter(p1, p2), p3);
	}

	private NeuralNetwork Fitter(NeuralNetwork network1, NeuralNetwork network2)
	{
		return (network1.CompareTo(network2) > 0 ? network1 : network2);
	}

	public NeuralNetwork GetFittest()
	{
		List<NeuralNetwork> sorted = new List<NeuralNetwork>(Elements);
		sorted.Sort();
		return sorted.Last();
	}
}