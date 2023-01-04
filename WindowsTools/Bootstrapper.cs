using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Stylet;
using StyletIoC;
using Tool.Core;
using WindowsTools.ViewModels;
using WindowsTools.Views;

namespace WindowsTools
{
    internal class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        private static readonly string PluginsFolder;

        static Bootstrapper()
        {
            PluginsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            if (!Directory.Exists(PluginsFolder))
            {
                Directory.CreateDirectory(PluginsFolder);
            }
        }

        protected override void ConfigureCatalog(AggregateCatalog catalog)
        {
            base.ConfigureCatalog(catalog);
            var directories = Directory.GetDirectories(PluginsFolder);
            foreach (var directory in directories)
            {
                catalog.Catalogs.Add(new DirectoryCatalog(directory));
            }
        }

        protected override void DisplayRootView(object rootViewModel)
        {
            var baseWindow = new ShellView
            {
                DataContext = rootViewModel
            };
            baseWindow.Show();
        }
    }

    internal class Bootstrapper<TRootViewModel> : MEFBootstrapperBase where TRootViewModel : class
    {
        private TRootViewModel _rootViewModel;

        /// <summary>
        ///     Gets the root ViewModel, creating it first if necessary
        /// </summary>
        protected virtual TRootViewModel RootViewModel =>
            _rootViewModel ?? (_rootViewModel = (TRootViewModel)GetInstance(typeof(TRootViewModel)));

        /// <summary>
        ///     Called when the application is launched. Displays the root view.
        /// </summary>
        protected override void Launch()
        {
            DisplayRootView(RootViewModel);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            // Don't create the root ViewModel if it doesn't already exist...
            ScreenExtensions.TryDispose(_rootViewModel);

            // Dispose the container last
            base.Dispose();
        }
    }

    internal abstract class MEFBootstrapperBase : BootstrapperBase
    {
        protected CompositionContainer Container;

        private Func<IMessageBoxViewModel> MessageBoxViewModelFactory =>
            () => (IMessageBoxViewModel)GetInstance(typeof(IMessageBoxViewModel));

        /// <summary>
        ///     Overridden from BootstrapperBase, this sets up the IoC container
        /// </summary>
        protected sealed override void ConfigureBootstrapper()
        {
            var catalog = new AggregateCatalog();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                catalog.Catalogs.Add(new AssemblyCatalog(assembly));
            ConfigureCatalog(catalog);
            Container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            DefaultConfigureBatch(batch);
            ConfigureBatch(batch);
            batch.AddExportedValue(Container);
            Container.Compose(batch);
        }

        protected virtual void DefaultConfigureBatch(CompositionBatch batch)
        {
            var viewManagerConfig = new ViewManagerConfig
            {
                ViewFactory = GetInstance,
                ViewAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList()
            };
            batch.AddExportedValue(viewManagerConfig);
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue<IMessageBoxViewModel>(new MessageBoxViewModel());
            batch.AddExportedValue<IWindowManagerConfig>(this);
            batch.AddExportedValue(new MessageBoxView());
            batch.AddExportedValue(MessageBoxViewModelFactory);
        }

        protected virtual void ConfigureBatch(CompositionBatch batch)
        {
        }

        protected virtual void ConfigureCatalog(AggregateCatalog catalog)
        {
        }

        /// <summary>
        ///     Given a type, use the IoC container to fetch an instance of it
        /// </summary>
        /// <param name="type">Type to fetch</param>
        /// <returns>Fetched instance</returns>
        public override object GetInstance(Type type)
        {
            var contract = AttributedModelServices.GetContractName(type);
            var export = Container.GetExportedValues<object>(contract).FirstOrDefault();
            if (export != null)
                return export;
            throw new Exception($"Could not find the {contract}");
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            // Dispose the container last
            Container?.Dispose();
        }
    }
}