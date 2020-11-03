using LibDeltaSystem.WebFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.MasterControl.WebInterface
{
    /// <summary>
    /// This type is used as a type that'll be displayed on the top bar in the admin interface
    /// </summary>
    public abstract class AdminInterfaceHeaderDefinition : DeltaWebServiceDefinition
    {
        public abstract string GetTitle();
    }
}
