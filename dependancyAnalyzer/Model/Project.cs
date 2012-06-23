using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace dependancyAnalyzer.Model
{
    class Project
    {
        private string _name;
        private int _files;
        private List<string> _dependancies = new List<string>();
        private List<Project> _resolvedDependancies = new List<Project>();

        public Project(string filename)
        {
            XPathDocument document = new XPathDocument(filename);

            FileInfo fi = new FileInfo(filename);
            _name = fi.Name.Replace(fi.Extension, string.Empty);

            XPathNavigator navigator = document.CreateNavigator();
            XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
            manager.AddNamespace("msbuild", "http://schemas.microsoft.com/developer/msbuild/2003");

            string dependancyExpression = "/msbuild:Project/msbuild:ItemGroup/msbuild:ProjectReference/msbuild:Name";
            XPathNodeIterator dependancyIterator = navigator.Select(dependancyExpression, manager);
            while (dependancyIterator.MoveNext())
            {
                _dependancies.Add(dependancyIterator.Current.InnerXml);
            }

            string compileableFilesExpression = "/msbuild:Project/msbuild:ItemGroup/msbuild:Compile";
            XPathNodeIterator compileableFilesIterator = navigator.Select(compileableFilesExpression, manager);
            _files = 0;
            while (compileableFilesIterator.MoveNext())
            {
                _files++;
            }

            string contentFilesExpression = "/msbuild:Project/msbuild:ItemGroup/msbuild:Content";
            XPathNodeIterator contentFilesIterator = navigator.Select(contentFilesExpression, manager);
            while (contentFilesIterator.MoveNext())
            {
                _files++;
            }
        }

        public void ResolveDependancies(ProjectManager pm)
        {
            foreach (string s in _dependancies)
            {
                Project p = pm.GetProject(s);
                if (p!=null)
                {
                    _resolvedDependancies.Add(p);
                }
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        internal List<Project> ResolvedDependancies
        {
            get { return _resolvedDependancies; }
            set { _resolvedDependancies = value; }
        }

        public int Files
        {
            get { return _files; }
            set { _files = value; }
        }

        public List<string> Dependancies
        {
            get { return _dependancies; }
        }
    }
}
