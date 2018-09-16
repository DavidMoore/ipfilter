namespace IPFilter.Services.Deployment
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Win32;

    public class ClickOnceRegistry
    {
        public const string ComponentsRegistryPath = @"Software\Classes\Software\Microsoft\Windows\CurrentVersion\Deployment\SideBySide\2.0\Components";
        public const string MarksRegistryPath = @"Software\Classes\Software\Microsoft\Windows\CurrentVersion\Deployment\SideBySide\2.0\Marks";
        
        public ClickOnceRegistry()
        {
            ReadComponents();
            ReadMarks();
        }

        private void ReadComponents()
        {
            Components = new List<Component>();

            var components = Registry.CurrentUser.OpenSubKey(ComponentsRegistryPath);
            if (components == null) return;

            foreach (var keyName in components.GetSubKeyNames())
            {
                var componentKey = components.OpenSubKey(keyName);
                if (componentKey == null) continue;

                var component = new Component { Key = keyName };
                Components.Add(component);

                component.Dependencies = new List<string>();
                foreach (var dependencyName in componentKey.GetSubKeyNames().Where(n => n != "Files"))
                {
                    component.Dependencies.Add(dependencyName);
                }
            }
        }

        private void ReadMarks()
        {
            Marks = new List<Mark>();

            var marks = Registry.CurrentUser.OpenSubKey(MarksRegistryPath);
            if (marks == null) return;

            foreach (var keyName in marks.GetSubKeyNames())
            {
                var markKey = marks.OpenSubKey(keyName);
                if (markKey == null) continue;

                var mark = new Mark { Key = keyName };
                Marks.Add(mark);

                var appid = markKey.GetValue("appid") as byte[];
                if (appid != null) mark.AppId = Encoding.ASCII.GetString(appid);

                var identity = markKey.GetValue("identity") as byte[];
                if (identity != null) mark.Identity = Encoding.ASCII.GetString(identity);

                mark.Implications = new List<Implication>();
                var implications = markKey.GetValueNames().Where(n => n.StartsWith("implication"));
                foreach (var implicationName in implications)
                {
                    var implication = markKey.GetValue(implicationName) as byte[];
                    if (implication != null)
                        mark.Implications.Add(new Implication
                                                  {
                                                      Key = implicationName,
                                                      Name = implicationName.Substring(12),
                                                      Value = Encoding.ASCII.GetString(implication)
                                                  });
                }
            }
        }

        public class RegistryKey
        {
            public string Key { get; set; }

            public override string ToString()
            {
                return Key ?? base.ToString();
            }
        }

        public class Component : RegistryKey
        {
            public List<string> Dependencies { get; set; }
        }

        public class Mark : RegistryKey
        {
            public string AppId { get; set; }

            public string Identity { get; set; }

            public List<Implication> Implications { get; set; }
        }

        public class Implication : RegistryKey
        {
            public string Name { get; set; }

            public string Value { get; set; }
        }

        public List<Component> Components { get; set; }

        public List<Mark> Marks { get; set; }
    }
}
