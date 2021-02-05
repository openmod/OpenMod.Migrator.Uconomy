using System;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;

[assembly: PluginMetadata("OpenMod.Migrator.Uconomy", Author = "OpenMod", Website = "https://github.com/openmod/OpenMod.Migrator.Uconomy")]

namespace OpenMod.Migrator.Uconomy
{
    public class UconomyToOpenMod : OpenModUnturnedPlugin
    {
        public UconomyToOpenMod(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}