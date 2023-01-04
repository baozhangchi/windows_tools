using Stylet;

namespace ReleaseUWPApplicationLoopbackProxyRestriction.Models
{
    internal class AppxPackageInfo : PropertyChangedBase
    {
        private string _name;
        private string _packageFamilyName;
        private string _packageFullName;
        private string _publisher;
        private bool _released;

        public string Publisher
        {
            get => _publisher;
            set => SetAndNotify(ref _publisher, value);
        }

        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        public string PackageFullName
        {
            get => _packageFullName;
            set => SetAndNotify(ref _packageFullName, value);
        }

        public string PackageFamilyName
        {
            get => _packageFamilyName;
            set => SetAndNotify(ref _packageFamilyName, value);
        }

        public bool Released
        {
            get => _released;
            set => SetAndNotify(ref _released, value);
        }
    }
}