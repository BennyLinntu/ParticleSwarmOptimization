using COP;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

namespace r10546039YiDaLinAss10
{
    public delegate double COPObjectiveFunction(double[] solution);

    public class Particle_Swamp_Optimizer_Solver
    {
        // data fields

        Random rnd = new Random();
        COP.OptimizationType optimization_Type = OptimizationType.Minimization;
        int numberOfVariables;
        int numberOfParticles = 20;
        int iteration = 0;
        int iteration_Limit = 300;
        double soFartheBestObjective;
        double initialBestOBJ;
        double selfFactor = 0.5;
        double socialFactor = 0.5;
        double iterationAverage;
        double iterationBest;
        double[] lowerBound;
        double[] upperBound;
        double[] selfBestObjectives;
        double[] soFartheBestPosition;
        double[] objectives;
        double[][] positions;
        double[][] selfBestPosition;
        COPObjectiveFunction theObjectiveFunction;
        COPBenchmark theCP = null;
        Series seriessoFarTheBestObjective;
        Series seriesiterationAverageObjective;
        Series seriesiterationTheBestObjective;

        // define public properties for those modifiable parameters
        
        [Category("Parameters")]
        public int NumberOfParticles { get => numberOfParticles; set => numberOfParticles = value; }
        [Category("Parameters")]
        public double SelfFactor { get => selfFactor; set => selfFactor = value; }
        [Category("Parameters")]
        public double SocialFactor { get => socialFactor; set => socialFactor = value; }
        [Category("Iteration")]
        public int CurrentIteration { get => iteration; set => iteration = value; }
        [Category("Iteration")]
        public int IterationLimit { get => iteration_Limit; set => iteration_Limit = value; }
        [Browsable(false)]
        public Series SeriessoFarTheBestObjective { get => seriessoFarTheBestObjective; set => seriessoFarTheBestObjective = value; }
        [Browsable(false)]
        public Series SeriesiterationAverageObjective { get => seriesiterationAverageObjective; set => seriesiterationAverageObjective = value; }
        [Browsable(false)]
        public Series SeriesiterationTheBestObjective { get => seriesiterationTheBestObjective; set => seriesiterationTheBestObjective = value; }
        [Browsable(false)]
        public double[] SoFartheBestPosition { get => soFartheBestPosition; set => soFartheBestPosition = value; }
        [Browsable(false)]
        public double SoFartheBestObjective { get => soFartheBestObjective; set => soFartheBestObjective = value; }
        [Browsable(false)]
        public double InitialBestOBJ { get => initialBestOBJ; set => initialBestOBJ = value; }
        [Browsable(false)]
        public double[][] Positions { get => positions; set => positions = value; }

        public Particle_Swamp_Optimizer_Solver(COP.COPBenchmark Cp)
        {
            theCP = Cp;
            numberOfVariables = Cp.Dimension;
            lowerBound = Cp.LowerBound;
            upperBound = Cp.UpperBound;
            optimization_Type = Cp.OptimizationGoal;
            theObjectiveFunction = Cp.GetObjectiveValue;

            soFartheBestPosition = new double[numberOfVariables];
            selfBestObjectives = new double[numberOfParticles];

            // series
            seriesiterationAverageObjective = new Series();
            seriessoFarTheBestObjective = new Series();
            seriesiterationTheBestObjective = new Series();
            seriessoFarTheBestObjective.ChartType = SeriesChartType.Line;
            seriesiterationAverageObjective.ChartType = SeriesChartType.Line;
            seriesiterationTheBestObjective.ChartType = SeriesChartType.Line;
        }

   

        private void Move_All_Particles_to_New_Positions()
        {
            double alpha;
            double beta;
            double diffself;
            double diffgroup;
            double velocity;
            double newPos;
            for (int p = 0; p < NumberOfParticles; p++)
            {
                for (int i = 0; i < numberOfVariables; i++)
                {
                    alpha = socialFactor * rnd.NextDouble();
                    beta = selfFactor * rnd.NextDouble();
                    diffself = selfBestPosition[p][i] - positions[p][i];
                    diffgroup = soFartheBestPosition[i] - positions[p][i];
                    velocity = alpha * (diffgroup) + beta * (diffself);
                    newPos = positions[p][i] + velocity;

                    // check constrains
                    if (newPos > upperBound[i])
                        newPos = upperBound[i];
                    else if (newPos < lowerBound[i])
                        newPos = lowerBound[i];

                    // move point
                    positions[p][i] = newPos;
                }

                // update objective
                objectives[p] = theObjectiveFunction(positions[p]);
            }

            iteration++;
        }

        private void UpdatesoFartheBestPosition()
        {
            // update self best 
            for (int p = 0; p < NumberOfParticles; p++)
            {
                bool selfupdate = false;
                bool sofarthebestupdate = false;
                switch (optimization_Type)
                {
                    case OptimizationType.Minimization:
                        if (objectives[p] < selfBestObjectives[p])
                            selfupdate = true;
                        if (objectives[p] < soFartheBestObjective)
                            sofarthebestupdate = true;
                        break;
                    case OptimizationType.Maximization:
                        if (objectives[p] > selfBestObjectives[p])
                            selfupdate = true;
                        if (objectives[p] > soFartheBestObjective)
                            sofarthebestupdate = true;
                        break;
                    case OptimizationType.GoalMatching:
                        break;
                    default:
                        break;
                }
                if (selfupdate)
                {
                    selfBestObjectives[p] = objectives[p];
                    for (int d = 0; d < numberOfVariables; d++)
                        selfBestPosition[p][d] = positions[p][d];
                }

                if (sofarthebestupdate)
                {
                    soFartheBestObjective = objectives[p];
                    for (int d = 0; d < numberOfVariables; d++)
                        soFartheBestPosition[d] = positions[p][d];
                }
            }

            // update series point
            double sum = 0;
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int p = 0; p < NumberOfParticles; p++)
            {
                sum += objectives[p];
                //bool update = false;
                if (optimization_Type == OptimizationType.Minimization)
                    if (min > objectives[p])
                        min = objectives[p];

                    else
                    if (max < objectives[p])
                        max = objectives[p];
            }

            iterationAverage = objectives.Sum() / (double)NumberOfParticles;
            if (optimization_Type == OptimizationType.Minimization)
                iterationBest = min;
            else
                iterationBest = max;

        }


        public void Reset()
        {
            soFartheBestPosition = new double[numberOfVariables];
            selfBestObjectives = new double[numberOfParticles];

            // optimization type
            if (optimization_Type == OptimizationType.Maximization) soFartheBestObjective = double.MinValue;
            else soFartheBestObjective = double.MaxValue;

            // allocate or reallocate memory
            positions = new double[numberOfParticles][];
            objectives = new double[NumberOfParticles];
            selfBestPosition = new double[NumberOfParticles][];
            for (int i = 0; i < numberOfParticles; i++)
            {
                positions[i] = new double[numberOfVariables];
                selfBestPosition[i] = new double[numberOfVariables];
            }


            // randomly assign initial positions & calculate the objectives 
            for (int p = 0; p < numberOfParticles; p++)
            {
                for (int i = 0; i < numberOfVariables; i++)
                    positions[p][i] = lowerBound[i] + rnd.NextDouble() * (upperBound[i] - lowerBound[i]);
                selfBestObjectives[p] = soFartheBestObjective;
                objectives[p] = theObjectiveFunction(positions[p]);
            }

            iteration = 0;


            // series
            seriesiterationAverageObjective.Points.Clear();
            seriessoFarTheBestObjective.Points.Clear();
            seriesiterationTheBestObjective.Points.Clear();
            seriesiterationAverageObjective.Name = "Iteration_Average";
            seriessoFarTheBestObjective.Name = "So_Far_The_Best";
            seriesiterationTheBestObjective.Name = "Iteration_The_Best";
            seriesiterationAverageObjective.Color = Color.Green;
            seriessoFarTheBestObjective.Color = Color.Red;
            seriesiterationTheBestObjective.Color = Color.Blue;
            seriesiterationTheBestObjective.BorderWidth = 2;
            seriesiterationAverageObjective.BorderWidth = 2;
            seriessoFarTheBestObjective.BorderWidth = 2;

            UpdatesoFartheBestPosition();
            initialBestOBJ = soFartheBestObjective;
        }
        public void RunOne()
        {
            Move_All_Particles_to_New_Positions();
            UpdatesoFartheBestPosition();
            Update_Series();
        }
        public void RunToEnd()
        {
            for (int i = iteration; i < iteration_Limit; i++)
                RunOne();
        }


        public void Set_Chart_Area(ChartArea ca)
        {
            seriesiterationAverageObjective.ChartArea = ca.Name;
            seriesiterationTheBestObjective.ChartArea = ca.Name;
            seriessoFarTheBestObjective.ChartArea = ca.Name;
        }
        private void Update_Series()
        {
            seriesiterationAverageObjective.Points.AddXY(iteration, iterationAverage);
            seriessoFarTheBestObjective.Points.AddXY(iteration, soFartheBestObjective);
            seriesiterationTheBestObjective.Points.AddXY(iteration, iterationBest);
        }
        public string Flatten_Position(double[] pos)
        {
            string str = string.Empty;
            for (int d = 0; d < numberOfVariables; d++)
            {
                str += Math.Round(pos[d], 3).ToString() + ", ";
            }
            return str;
        }
    }

}
