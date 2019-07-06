using System.ServiceProcess;

namespace IPFilter
{
    partial class FilterService : ServiceBase
    {
        public FilterService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {}

        protected override void OnStop()  {}
    }
}
