#region FileHeader

// // Project:  ReleaseUWPApplicationLoopbackProxyRestriction
// // File:  MainViewModel.cs
// // CreateTime:  2022-12-30 14:51
// // LastUpdateTime:  2023-01-05 9:23

#endregion

#region Nmaespaces

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using ReleaseUWPApplicationLoopbackProxyRestriction.Models;
using ReleaseUWPApplicationLoopbackProxyRestriction.Views;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Primitives;
using Tool.Core;

#endregion

namespace ReleaseUWPApplicationLoopbackProxyRestriction.ViewModels
{
    internal class MainViewModel : ToolViewModelBase
    {
        #region Fields

        private ObservableCollection<AppxPackageInfo> _appxPackages;
        private RadListBox _appxPackagesView;
        private bool _isUpdating;
        private CheckBox _selectAllCheckBox;
        private List<AppxPackageInfo> _selectedAppxPackages;

        #endregion

        #region Properties

        public ObservableCollection<AppxPackageInfo> AppxPackages
        {
            get => _appxPackages;
            set => SetAndNotify(ref _appxPackages, value);
        }

        public bool CanCancelLiftRestrictions => _appxPackagesView?.SelectedItems.Count > 0;

        public bool CanLiftRestrictions => _appxPackagesView?.SelectedItems.Count > 0;

        public List<AppxPackageInfo> SelectedAppxPackages
        {
            get => _selectedAppxPackages;
            set => SetAndNotify(ref _selectedAppxPackages, value);
        }

        #endregion

        #region Constructors

        public MainViewModel() : base("解除UWP应用回环代理限制")
        {
            AppxPackages = new ObservableCollection<AppxPackageInfo>(
                PowerShell.RunScriptList<AppxPackageInfo>(
                    "Get-AppxPackage | Select Publisher, Name, PackageFullName, PackageFamilyName"));
            var releasedAppxPackagesContent = PowerShell.RunScript("CheckNetIsolation.exe LoopbackExempt -s");
            var regex = new Regex(@"\s+名称:\s+(?<name>\S+)");
            foreach (var line in releasedAppxPackagesContent.Split(new[] { '\r', '\n' },
                         StringSplitOptions.RemoveEmptyEntries))
            {
                var match = regex.Match(line);
                if (match.Success)
                {
                    var name = match.Groups["name"].Value;
                    var item = AppxPackages.FirstOrDefault(x =>
                        x.PackageFamilyName.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                    if (item != null) item.Released = true;
                }
            }
        }

        #endregion

        #region Methods

        public void SelectAllCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;
            ListControl.SelectAllCommand.Execute(null, _appxPackagesView);
            NotifyOfPropertyChange(nameof(CanCancelLiftRestrictions));
            NotifyOfPropertyChange(nameof(CanLiftRestrictions));
            _isUpdating = false;
        }

        public void AppxPackagesView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;
            if (_appxPackagesView.SelectedItems.Count == 0)
                _selectAllCheckBox.IsChecked = false;
            else if (_appxPackagesView.SelectedItems.Count == _appxPackagesView.Items.Count)
                _selectAllCheckBox.IsChecked = true;
            else
                _selectAllCheckBox.IsChecked = null;
            _isUpdating = false;

            NotifyOfPropertyChange(nameof(CanCancelLiftRestrictions));
            NotifyOfPropertyChange(nameof(CanLiftRestrictions));
        }

        public void SelectAllCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;
            _appxPackagesView.SelectedItems.Clear();
            NotifyOfPropertyChange(nameof(CanCancelLiftRestrictions));
            NotifyOfPropertyChange(nameof(CanLiftRestrictions));
            _isUpdating = false;
        }

        public void LiftRestrictions()
        {
            foreach (var packageInfo in _appxPackagesView.SelectedItems.Cast<AppxPackageInfo>().ToList())
                PowerShell.RunScript($"CheckNetIsolation LoopbackExempt -a -n=\"{packageInfo.PackageFamilyName}\"");
        }

        public void CancelLiftRestrictions()
        {
            foreach (var packageInfo in _appxPackagesView.SelectedItems.Cast<AppxPackageInfo>().ToList())
                PowerShell.RunScript($"CheckNetIsolation LoopbackExempt -d -n=\"{packageInfo.PackageFamilyName}\"");
        }

        public void UpdateAppxPackages()
        {
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            if (View is MainView view)
            {
                _selectAllCheckBox = view.SelectAllCheckBox;
                _appxPackagesView = view.AppxPackagesView;
            }
        }

        #endregion
    }
}