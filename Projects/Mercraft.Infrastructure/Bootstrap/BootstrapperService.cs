﻿using System.Collections.Generic;
using System.Linq;
using Mercraft.Infrastructure.Config;
using Mercraft.Infrastructure.Dependencies;

namespace Mercraft.Infrastructure.Bootstrap
{
    /// <summary>
    /// Provides default functionality to execute startup plugins
    /// </summary>
    public class BootstrapperService: IBootstrapperService
    {
        private readonly IEnumerable<IBootstrapperPlugin> _plugins;
        private readonly IConfigSection _configSection;
        
        [Dependency]
        public IContainer Container { get; set; }

        [Dependency]
        public BootstrapperService(IConfigSection configSection)
        {
            _configSection = configSection;
        }
        
        public BootstrapperService(Container container, IBootstrapperPlugin[] plugins)
        {
            Container = container;
            _plugins = plugins;
        }

        public bool IsInitialized { get; private set; }

        private void Initialze()
        {
            var bootstrappers = _configSection.GetSections("bootstrappers/bootstrapper");
            foreach (var bootsrapperSection in bootstrappers)
            {
                var name = bootsrapperSection.GetString("@name");
                var type = bootsrapperSection.GetType("@type");
                Container.Register(Component.For<IBootstrapperPlugin>().Use(type, bootsrapperSection).Named(name).Singleton());
            }
            IsInitialized = true;
        }


        #region IBootstrapperService members

        /// <summary>
        /// Run all registred bootstrappers
        /// </summary>
        /// <returns></returns>
        public bool Run()
        {
            if(!IsInitialized && _plugins == null)
                Initialze();
            
            var plugins = _plugins ?? Container.ResolveAll<IBootstrapperPlugin>();
            return plugins
                .ToList().Aggregate(true, (current, task) => current & task.Run());
        }

        /// <summary>
        /// Updates all registred bootstrappers
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            var plugins = _plugins ?? Container.ResolveAll<IBootstrapperPlugin>();
            return plugins
                .ToList().Aggregate(true, (current, task) => current & task.Update());
        }

        /// <summary>
        /// Updates all registred bootstrappers
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            var plugins = _plugins ?? Container.ResolveAll<IBootstrapperPlugin>();
            return plugins
                .ToList().Aggregate(true, (current, task) => current & task.Stop());
        }

        #endregion
    }
}
