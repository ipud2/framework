﻿using Mercraft.Core;
using Mercraft.Infrastructure.Bootstrap;
using Mercraft.Infrastructure.Config;
using Mercraft.Infrastructure.Dependencies;
using UnityEngine;
using Component = Mercraft.Infrastructure.Dependencies.Component;

namespace Mercraft.Explorer
{
    /// <summary>
    /// Represents application component root
    /// </summary>
    public class ComponentRoot : IGameRunner, IPositionListener
    {
        /// <summary>
        /// Holds config reference
        /// </summary>
        private readonly ConfigSettings _config;

        /// <summary>
        /// DI container
        /// </summary>
        private readonly IContainer _container = new Container();
        
        /// <summary>
        /// Actual zone loader
        /// </summary>
        private IPositionListener _positionListener;

        public ComponentRoot(string configPath)
        {
            var configSettings = new ConfigSettings(configPath);
            InitializeFromConfig(configSettings);
        }

        public ComponentRoot(ConfigSettings configSettings)
        {
            _config = configSettings;
            InitializeFromConfig(configSettings);
        }

        private void InitializeFromConfig(ConfigSettings configSettings)
        {
            _container.RegisterInstance(configSettings);

            // register bootstrapping service which will register all dependencies using configuration
            var bootSection = configSettings.GetSection("system/bootstrapping");
            var bootServiceType = bootSection.GetType("@type");

            _container.Register(Component.For<IBootstrapperService>()
                .Use(bootServiceType, bootSection).Singleton());
            
            // run bootstrappers
            _container.Resolve<IBootstrapperService>().Run();
        }

        public void RunGame(GeoCoordinate coordinate)
        {
            // TODO register position here
            _positionListener = _container.Resolve<IPositionListener>();
        }

        public void OnPositionChanged(Vector2 position)
        {
            _positionListener.OnPositionChanged(position);
        }
    }
}
