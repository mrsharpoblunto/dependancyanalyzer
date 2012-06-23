using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using dependancyAnalyzer.Model;
using dependancyAnalyzer.View;
using Physics;
using Physics.ForceDissipationFunction;

namespace dependancyAnalyzer
{
    public partial class Form1 : Form
    {
        private ParticleSimulation<NodeRenderer, LineRenderer> _simulation=null;
        private ProjectManager _projectManager;
        private Vector mousePosition;
        private int selectedParticleIndex = -1;

        //constants
        private static double MAX_RADIUS = 15;
        private static double MIN_RADIUS = 5;

        //default values
        private double DEFAULT_SPRING_CONSTANT = 30;
        private double DEFAULT_REPELLANT_FORCE = 200;
        private double DEFAULT_MAX_MASS = 10;
        private double DEFAULT_FRICTION_CONSTANT = 0.9;
        private double DEFAULT_GRAVITY = 0;
        private double DEFAULT_STATIC_FRICTION = 15;
        private double DEFAULT_SHORT_RESTING_LENGTH = 200;

        public Form1()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult result = FolderDialog.ShowDialog();
            if (result==System.Windows.Forms.DialogResult.OK)
            {
                _projectManager = new ProjectManager(FolderDialog.SelectedPath);
                _simulation =
                    new ParticleSimulation<NodeRenderer, LineRenderer>(0, 0, RenderPanel.Width
                              , RenderPanel.Height
                              , DEFAULT_GRAVITY
                              , DEFAULT_FRICTION_CONSTANT
                              , DEFAULT_STATIC_FRICTION
                              , DEFAULT_SHORT_RESTING_LENGTH
                              , DEFAULT_SPRING_CONSTANT
                              , ForceDissipationFunctionFactory.LinearDissipationFunction(DEFAULT_REPELLANT_FORCE));
                BuildParticles();
                RenderTimer.Enabled = true;
                RenderTimer.Start();
            }
        }

        private void BuildParticles()
        {
            double maxProjectFileCount = 0;

            foreach (Project p in _projectManager.Projects)
            {
                if (p.Files > maxProjectFileCount)
                {
                    maxProjectFileCount = p.Files;
                }
            }

            Random random = new Random(DateTime.Now.Millisecond);

            Dictionary<string, Particle<NodeRenderer, LineRenderer>> particleDictionary = new Dictionary<string, Particle<NodeRenderer, LineRenderer>>();

            //create each particle
            foreach (Project p in _projectManager.Projects)
            {
                NodeRenderer renderer = new NodeRenderer(p.Name, ((p.Files / maxProjectFileCount) * MAX_RADIUS) + MIN_RADIUS);
                Particle<NodeRenderer, LineRenderer> particle =
                    new Particle<NodeRenderer, LineRenderer>(DEFAULT_MAX_MASS, DEFAULT_REPELLANT_FORCE,
                                                             _simulation.ForceDissipationFunction);
                particle.InitPosition(random.Next(RenderPanel.Width), random.Next(RenderPanel.Height));
                _simulation.AddParticle(particle, renderer);

                particleDictionary.Add(p.Name,particle);
            }

            //connect the particle dependancies
            foreach (Project p in _projectManager.Projects)
            {
                foreach (string projectName in p.Dependancies)
                {
                    //if this has any projects that are not loaded, create a defualt project to link to
                    if (!particleDictionary.ContainsKey(projectName))
                    {
                        NodeRenderer renderer = new NodeRenderer(projectName, MIN_RADIUS);
                        Particle<NodeRenderer, LineRenderer> particle =
                            new Particle<NodeRenderer, LineRenderer>(DEFAULT_MAX_MASS, DEFAULT_REPELLANT_FORCE,
                                                                     _simulation.ForceDissipationFunction);
                        particle.InitPosition(random.Next(RenderPanel.Width), random.Next(RenderPanel.Height));
                        _simulation.AddParticle(particle, renderer);

                        particleDictionary.Add(projectName, particle);
                    }

                    particleDictionary[
                        p.Name].AddConnection(particleDictionary[projectName], 
                        new LineRenderer(particleDictionary[projectName].MetaData.Radius), 
                        DEFAULT_SHORT_RESTING_LENGTH, 
                        DEFAULT_SPRING_CONSTANT);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            _simulation.RunSimulation(0.1);

            if (selectedParticleIndex != -1)
            {
                _simulation.Particles[selectedParticleIndex].InitPosition(mousePosition.X, mousePosition.Y);
            }

            Bitmap offScreenBmp;
            Graphics offScreenDC;
            offScreenBmp = new Bitmap(RenderPanel.Width, RenderPanel.Height);
            offScreenDC = Graphics.FromImage(offScreenBmp);
            offScreenDC.SmoothingMode = SmoothingMode.AntiAlias;
            Graphics clientDC = RenderPanel.CreateGraphics();


            offScreenDC.FillRectangle(Brushes.White, 0, 0, Width, Height);
            foreach (Particle<NodeRenderer, LineRenderer> particle in _simulation.Particles)
            {
                particle.MetaData.Render(offScreenDC, particle.Position.X, particle.Position.Y);
                foreach (ParticleConnector<NodeRenderer, LineRenderer> connector in particle.Connectors)
                {
                    if (connector.Particle1 == particle)
                    {
                        connector.MetaData.Render(offScreenDC,
                            connector.Particle1.Position.X
                            , connector.Particle1.Position.Y
                            , connector.Particle2.Position.X
                            , connector.Particle2.Position.Y
                        );
                    }
                }
            }

            clientDC.DrawImage(offScreenBmp, 0, 0);

            offScreenDC.Dispose();
            clientDC.Dispose();
            offScreenBmp.Dispose();
        }

        private void RenderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            mousePosition = new Vector(e.X, e.Y);
            for (int i = 0; i < _simulation.Particles.Count; ++i)
            {
                Vector temp = mousePosition - _simulation.Particles[i].Position;
                if (Vector.Length(temp) <= _simulation.Particles[i].MetaData.Radius)
                {
                    selectedParticleIndex = i;
                    break;
                }
            }
        }

        private void RenderPanel_MouseLeave(object sender, EventArgs e)
        {
            selectedParticleIndex = -1;
        }

        private void RenderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedParticleIndex != -1)
            {
                mousePosition = new Vector(e.X, e.Y);
            }
        }

        private void RenderPanel_MouseUp(object sender, MouseEventArgs e)
        {
            //if a block has been selected
            if (selectedParticleIndex != -1)
            {
                //remove the selected particle
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    _simulation.Particles[selectedParticleIndex].DisconnectParticle();
                    _simulation.Particles.Remove(_simulation.Particles[selectedParticleIndex]);
                }
                selectedParticleIndex = -1;
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            BrowseButton.Top = Height - 70;
            RenderPanel.Width = Width - 30;
            RenderPanel.Height = Height - 90;

            if (_simulation != null)
            {
                _simulation.Height = RenderPanel.Height;
                _simulation.Width = RenderPanel.Width;
            }
        }
    }
}