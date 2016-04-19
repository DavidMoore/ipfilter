namespace IPFilter.Setup.CustomActions
{
    using System;
    using System.Collections.Generic;

    public interface IUninstallStep : IDisposable
    {
        void Prepare(List<string> componentsToRemove);

        void PrintDebugInformation();

        void Execute();
    }
}
