#region

using System;
using Cysharp.Threading.Tasks;
using fr34kyn01535.Uconomy;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;

#endregion

[assembly:
    PluginMetadata("UconomyToOpenMod", Author = "Rube200", Website = "https://github.com/rube200/UconomyToOpenMod")]

namespace RG.UconomyToOpenMod
{
    // ReSharper disable once UnusedMember.Global
    public class UconomyToOpenMod : OpenModUnturnedPlugin
    {
        public UconomyToOpenMod(IServiceProvider serviceProvider) : base(
            serviceProvider)
        {
            if (Uconomy.Instance != null)
                return;

            // ReSharper disable once VirtualMemberCallInConstructor
            Uconomy.Instance = ActivatorUtilities.CreateInstance<Uconomy>(serviceProvider);
        }

        protected override UniTask OnLoadAsync()
        {
            return UniTask.CompletedTask;
        }

        protected override UniTask OnUnloadAsync()
        {
            return UniTask.CompletedTask;
        }
    }
}