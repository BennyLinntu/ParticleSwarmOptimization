using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using COP;
using r10546039YiDaLinAss10;

namespace r10546039YiDaLinAss10
{
    public partial class MainForm : Form
    {
        // data fileds
        Particle_Swamp_Optimizer_Solver PSOSolver;
        COPBenchmark the_Problem;



        public MainForm()
        {
            InitializeComponent();
        }
        // open file
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            the_Problem = COPBenchmark.LoadAProblemFromAFile();
            if (the_Problem == null) return;
            the_Problem.DisplayOnPanel(tabPageShow);
            the_Problem.DisplayObjectiveGraphics(splitContainer2.Panel2);
            btnReset.Enabled = true;
            btnRunOne.Enabled = false;
            btnRuntoEnd.Enabled = false;
            chtShow.Series.Clear();
            rtbShow.Clear();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            PSOSolver = new Particle_Swamp_Optimizer_Solver(the_Problem);
            ppgShow.SelectedObject = PSOSolver;
            PSOSolver.Reset();
            chtShow.ChartAreas[0].RecalculateAxesScale();
            the_Problem.DisplaySolutionsOnGraphics(PSOSolver.Positions);
            rtbShow.AppendText("The Best Show\n");
            rtbShow.AppendText($"Solution: {PSOSolver.Flatten_Position(PSOSolver.SoFartheBestPosition)}\n");
            rtbShow.AppendText($"Objective: {Math.Round(PSOSolver.SoFartheBestObjective, 3).ToString()}\n");

            chtShow.Series.Clear();
            chtShow.Series.Add(PSOSolver.SeriesiterationAverageObjective);
            chtShow.Series.Add(PSOSolver.SeriesiterationTheBestObjective);
            chtShow.Series.Add(PSOSolver.SeriessoFarTheBestObjective);
            btnRunOne.Enabled = true;
            btnRuntoEnd.Enabled = true;
        }

        private void btnRunOne_Click(object sender, EventArgs e)
        {
            bool reach = true;

            if (PSOSolver.CurrentIteration < PSOSolver.IterationLimit)
            {
                PSOSolver.RunOne();
                reach = false;
            }
            if (reach)
            {
                
                btnReset.Enabled = false;
                btnRunOne.Enabled = false;
                btnRuntoEnd.Enabled = false;
            }
            else
            {

                btnReset.Enabled = false;
            }
            rtbShow.AppendText("The Best Show\n");
            rtbShow.AppendText($"Solution: {PSOSolver.Flatten_Position(PSOSolver.SoFartheBestPosition)}\n");
            rtbShow.AppendText($"Objective: {Math.Round(PSOSolver.SoFartheBestObjective, 3).ToString()}\n");
        }

        private void btnRuntoEnd_Click(object sender, EventArgs e)
        {
            PSOSolver.RunToEnd();
            rtbShow.AppendText("The Best Show\n");
            rtbShow.AppendText($"Solution: {PSOSolver.Flatten_Position(PSOSolver.SoFartheBestPosition)}\n");
            rtbShow.AppendText($"Objective: {Math.Round(PSOSolver.SoFartheBestObjective, 3).ToString()}\n");
            btnReset.Enabled = false;
            btnRunOne.Enabled = false;
            btnRuntoEnd.Enabled = false;
        }
    }
}
