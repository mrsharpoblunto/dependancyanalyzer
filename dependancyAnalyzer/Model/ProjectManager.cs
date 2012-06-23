using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dependancyAnalyzer.Model
{
    class ProjectManager
    {
        private Dictionary<string, Project> _projects = new Dictionary<string, Project>();

        public IEnumerable<Project> Projects
        {
            get { return _projects.Values; }
        }

        public Project GetProject(string s)
        {
            if (_projects.ContainsKey(s))
            {
                return _projects[s];
            }
            else
            {
                return null;
            }
        }

        public ProjectManager(string directoryName)
        {
            RecursiveLoad(directoryName);
            foreach (Project value in _projects.Values)
            {
                value.ResolveDependancies(this);
            }
        }

        private void RecursiveLoad(string name)
        {
            DirectoryInfo di = new DirectoryInfo(name);

            foreach (FileInfo file in di.GetFiles("*.csproj"))
            {
                Project p = new Project(file.FullName);
                if (!_projects.ContainsKey(p.Name))
                {
                    _projects.Add(p.Name, p);
                }
            }

            foreach (DirectoryInfo info in di.GetDirectories())
            {
                RecursiveLoad(info.FullName);
            }
        }
    }
}
