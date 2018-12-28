using System.ServiceProcess;

namespace IPFilter
{
    partial class FilterService : ServiceBase
    {
        public FilterService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }


    }
}
