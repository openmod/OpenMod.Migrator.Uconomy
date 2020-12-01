#region

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using fr34kyn01535.Uconomy;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Extensions.Economy.Abstractions;
using OpenMod.Unturned.Plugins;
using Rocket.Core.Plugins;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

[assembly:
    PluginMetadata("UconomyToOpenMod", Author = "Rube200", Website = "https://github.com/rube200/UconomyToOpenMod")]

namespace RG.UconomyToOpenMod
{
    // ReSharper disable once UnusedMember.Global
    public class UconomyToOpenMod : OpenModUnturnedPlugin
    {
        private readonly ILogger<UconomyToOpenMod> m_Logger;

        public UconomyToOpenMod(IEconomyProvider economyProvider, ILogger<UconomyToOpenMod> logger, IServiceProvider serviceProvider) : base(
            serviceProvider)
        {
            Uconomy.EconomyProvider = economyProvider;
            m_Logger = logger;
        }

        protected override UniTask OnLoadAsync()
        {
            Harmony.Patch(m_OriginalLoadPlugins, m_TargetLoadPlugins);
            HarmonyPatchesToUconomy.OnPluginsLoaded += OnPluginsLoaded;
            if (m_RocketPluginsManagerAssemblies.GetValue(null) != null)
                OnPluginsLoaded(null, null);//LoaderAlreadyCalled

            return base.OnLoadAsync();
        }

        protected override UniTask OnUnloadAsync()
        {
            HarmonyPatchesToUconomy.OnPluginsLoaded -= OnPluginsLoaded;
            return base.OnUnloadAsync();
        }

        private void OnPluginsLoaded(object s, EventArgs e)
        {
            try
            {
                var plugins = (IList)m_RocketPluginsManagerPlugins.GetValue(null);
                if (plugins.Cast<GameObject>().Any(go => go.name == m_UconomyType.Name))
                    return;

                var pluginGameObject = new GameObject(m_UconomyType.Name, m_UconomyType);
                Object.DontDestroyOnLoad(pluginGameObject);
                plugins.Add(pluginGameObject);
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Fail to inject modded version of Uconomy");
            }
        }

        #region Relection

        private readonly Type m_UconomyType = typeof(Uconomy);

        private readonly MethodInfo m_OriginalLoadPlugins =
            typeof(RocketPluginManager).GetMethod("loadPlugins",
                BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly HarmonyMethod m_TargetLoadPlugins = new HarmonyMethod(typeof(HarmonyPatchesToUconomy),
            nameof(HarmonyPatchesToUconomy.LoadPluginPath));

        private readonly FieldInfo m_RocketPluginsManagerAssemblies =
            typeof(RocketPluginManager).GetField("pluginAssemblies", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly FieldInfo m_RocketPluginsManagerPlugins =
            typeof(RocketPluginManager).GetField("plugins", BindingFlags.Static | BindingFlags.NonPublic);

        #endregion
    }

    public static class HarmonyPatchesToUconomy
    {
        internal static event EventHandler OnPluginsLoaded;

        internal static void LoadPluginPath()
        {
            OnPluginsLoaded?.Invoke(null, null);
        }
    }
}