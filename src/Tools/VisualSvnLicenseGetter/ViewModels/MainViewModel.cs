#region FileHeader

// // Project:  VisualSvnLicenseGetter
// // File:  MainViewModel.cs
// // CreateTime:  2023-01-03 17:49
// // LastUpdateTime:  2023-01-05 9:42

#endregion

#region Nmaespaces

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;
using Stylet;
using Tool.Core;
using VisualSvnLicenseGetter.Properties;
using VisualSvnLicenseGetter.Views;

#endregion

namespace VisualSvnLicenseGetter.ViewModels
{
    internal class MainViewModel : ToolViewModelBase
    {
        #region Fields

        private string _baseUrl;
        private bool _canEdit;
        private string _code;
        private BitmapImage _codeImageSource;
        private string _email;
        private string _license;
        private string _name;
        private string _organization;
        private string _svnVersion;
        private ObservableCollection<string> _versionHistory;
        private WebView2 _webView;

        #endregion

        #region Properties

        /// <summary>
        /// 能否复制证书内容到剪贴板
        /// </summary>
        public bool CanCopyToClipboard => !string.IsNullOrWhiteSpace(License);

        /// <summary>
        /// 能否填写申请内容
        /// </summary>
        public bool CanEdit
        {
            get => _canEdit;
            set => SetAndNotify(ref _canEdit, value);
        }

        /// <summary>
        /// 能否加载申请信息
        /// </summary>
        public bool CanLoadInfo => !string.IsNullOrWhiteSpace(SvnVersion);

        /// <summary>
        /// 能否申请证书
        /// </summary>
        public bool CanRequestLicense => !string.IsNullOrWhiteSpace(Name)
                                         && !string.IsNullOrWhiteSpace(Email)
                                         && !string.IsNullOrWhiteSpace(Code) &&
                                         _webView != null &&
                                         _webView.CoreWebView2 != null &&
                                         _webView.CoreWebView2.Source ==
                                         "https://www.visualsvn.com/server/licensing/evaluation/" &&
                                         string.IsNullOrWhiteSpace(License);

        /// <summary>
        /// 能否保存到文件
        /// </summary>
        public bool CanSaveToFile => !string.IsNullOrWhiteSpace(License);

        /// <summary>
        /// 验证码
        /// </summary>
        public string Code
        {
            get => _code;
            set => SetAndNotify(ref _code, value);
        }

        /// <summary>
        /// 验证码图片
        /// </summary>
        public BitmapImage CodeImageSource
        {
            get => _codeImageSource;
            set => SetAndNotify(ref _codeImageSource, value);
        }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email
        {
            get => _email;
            set => SetAndNotify(ref _email, value);
        }

        /// <summary>
        /// 许可证内容
        /// </summary>
        public string License
        {
            get => _license;
            set => SetAndNotify(ref _license, value);
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        /// <summary>
        /// 组织
        /// </summary>
        public string Organization
        {
            get => _organization;
            set => SetAndNotify(ref _organization, value);
        }

        /// <summary>
        /// SVN版本
        /// </summary>
        public string SvnVersion
        {
            get => _svnVersion;
            set => SetAndNotify(ref _svnVersion, value);
        }

        /// <summary>
        /// SVN版本输入历史
        /// </summary>
        public ObservableCollection<string> VersionHistory
        {
            get => _versionHistory;
            set => SetAndNotify(ref _versionHistory, value);
        }

        #endregion

        #region Constructors

        public MainViewModel() : base("VisualSVN许可证获取")
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     加载信息
        /// </summary>
        public void LoadInfo()
        {
            _baseUrl = $"https://www.visualsvn.com/go/2196/?v={SvnVersion}";
            _webView.CoreWebView2.Navigate(_baseUrl);
            License = string.Empty;
        }

        /// <summary>
        ///     请求许可证
        /// </summary>
        public void RequestLicense()
        {
            _webView.CoreWebView2.ExecuteScriptAsync(
                $"document.getElementById('Content_Content_Content_ctlName').value='{Name}'");
            _webView.CoreWebView2.ExecuteScriptAsync(
                $"document.getElementById('Content_Content_Content_ctlCompany').value='{Organization}'");
            _webView.CoreWebView2.ExecuteScriptAsync(
                $"document.getElementById('Content_Content_Content_ctlEmail').value='{Email}'");
            _webView.CoreWebView2.ExecuteScriptAsync(
                $"document.querySelector('#Content_Content_Content_ctlCaptcha > div:nth-child(2) > input').value='{Code}'");
            _webView.CoreWebView2.ExecuteScriptAsync(
                "document.getElementById('Content_Content_Content_submit').click()");
            CanEdit = false;
        }

        /// <summary>
        ///     保存到文件
        /// </summary>
        public void SaveToFile()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "文本文件|*.txt";
            if (dialog.ShowDialog() ?? false)
            {
                File.WriteAllText(dialog.FileName, License, Encoding.Default);
                MessageBox.Show("保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        ///     拷贝到剪贴板
        /// </summary>
        public void CopyToClipboard()
        {
            Clipboard.SetText(License);
            MessageBox.Show("复制成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(SvnVersion):
                    NotifyOfPropertyChange(nameof(CanLoadInfo));
                    break;
                case nameof(Name):
                case nameof(Email):
                case nameof(Code):
                    NotifyOfPropertyChange(nameof(CanRequestLicense));
                    break;
                case nameof(License):
                    NotifyOfPropertyChange(nameof(CanRequestLicense));
                    NotifyOfPropertyChange(nameof(CanCopyToClipboard));
                    NotifyOfPropertyChange(nameof(CanSaveToFile));
                    if (!string.IsNullOrWhiteSpace(License))
                    {
                        var oldVersions = string.IsNullOrWhiteSpace(Settings.Default.Versions)
                            ? new List<string>()
                            : Settings.Default.Versions
                                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        oldVersions.Add(SvnVersion);
                        VersionHistory = new ObservableCollection<string>(oldVersions.Distinct());
                        Settings.Default.Versions =
                            string.Join(";", VersionHistory);
                        Settings.Default.Save();
                    }

                    break;
            }
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            VersionHistory = string.IsNullOrWhiteSpace(Settings.Default.Versions)
                ? new ObservableCollection<string>()
                : new ObservableCollection<string>(Settings.Default.Versions
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            if (View is MainView view)
                if (_webView == null)
                {
                    _webView = view.WebView;
                    _webView.NavigationCompleted += WebViewNavigationCompleted;
                    InitializeWebViewAsync();
                }
        }

        private async void WebViewNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(CanRequestLicense));
            if (_webView.CoreWebView2 != null)
                switch (_webView.CoreWebView2.Source)
                {
                    case "https://www.visualsvn.com/server/licensing/evaluation/":
                        if (await ServiceValidate())
                            GetLicense();
                        else
                            License = string.Empty;
                        break;
                }
        }

        /// <summary>
        /// 获取许可证内容
        /// </summary>
        private async void GetLicense()
        {
            var licenseDom = await _webView.CoreWebView2.ExecuteScriptAsync(
                "document.getElementById('Content_Content_Content_evaluationKey')");
            if (licenseDom != "null")
            {
                var license =
                    await _webView.CoreWebView2.ExecuteScriptAsync(
                        "document.getElementById('Content_Content_Content_evaluationKey').innerHTML");
                License = license == "null"
                    ? string.Empty
                    : string.Join(Environment.NewLine,
                        license.Trim('"').Split(new[] { "\\r", "\\n" }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        /// <summary>
        /// 初始化WebView
        /// </summary>
        private async void InitializeWebViewAsync()
        {
            await _webView.EnsureCoreWebView2Async(null);
            _webView.CoreWebView2.AddWebResourceRequestedFilter(
                "https://www.visualsvn.com/*", CoreWebView2WebResourceContext.Image);
            _webView.CoreWebView2.WebResourceResponseReceived += WebView_WebResourceResponseReceived;
        }

        private async void WebView_WebResourceResponseReceived(object sender,
            CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            var status = e.Response.StatusCode;
            if (status == 200)
            {
                if (e.Request.Uri.Contains("Captcha.aspx"))
                    try
                    {
                        using (var stream = new MemoryStream())
                        {
                            await (await e.Response.GetContentAsync()).CopyToAsync(stream);
                            var image = new BitmapImage();
                            stream.Position = 0;
                            image.BeginInit();
                            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.UriSource = null;
                            image.StreamSource = stream;
                            image.EndInit();
                            image.Freeze();
                            CodeImageSource = image;
                        }

                        CanEdit = true;
                    }
                    catch (COMException ex)
                    {
                        CanEdit = false;
                        // A COMException will be thrown if the content failed to load.
                    }
                else
                    CanEdit = false;
            }
        }

        /// <summary>
        /// 获取站点验证内容
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ServiceValidate()
        {
            var error = string.Empty;
            var selector = @"#Content_Content_Content_ctl03";
            var validateDom = await _webView.CoreWebView2.ExecuteScriptAsync(
                $"document.querySelector('{selector}')");
            if (validateDom != "null")
            {
                var display =
                    await _webView.CoreWebView2.ExecuteScriptAsync(
                        $@"document.querySelector('{selector}').style.display");
                if (display.Trim('\"') != "none")
                    error = "名称不能为空";
            }

            if (string.IsNullOrWhiteSpace(error))
            {
                selector = @"#Content_Content_Content_ctl06";
                validateDom = await _webView.CoreWebView2.ExecuteScriptAsync(
                    $"document.querySelector('{selector}')");
                if (validateDom != "null")
                {
                    var display =
                        await _webView.CoreWebView2.ExecuteScriptAsync(
                            $@"document.querySelector('{selector}').style.display");
                    if (display.Trim('\"') != "none")
                        error = "邮箱不能为空";
                }
            }

            if (string.IsNullOrWhiteSpace(error))
            {
                selector = @"#Content_Content_Content_EMailValidator";
                validateDom = await _webView.CoreWebView2.ExecuteScriptAsync(
                    $"document.querySelector('{selector}')");
                if (validateDom != "null")
                {
                    var display =
                        await _webView.CoreWebView2.ExecuteScriptAsync(
                            $@"document.querySelector('{selector}').style.display");
                    if (display.Trim('\"') != "none")
                        error = "邮箱验证失败";
                }
            }

            if (string.IsNullOrWhiteSpace(error))
            {
                selector = @"#Content_Content_Content_ctlRecaptchaResult";
                validateDom = await _webView.CoreWebView2.ExecuteScriptAsync(
                    $"document.querySelector('{selector}')");
                if (validateDom != "null")
                {
                    var display =
                        await _webView.CoreWebView2.ExecuteScriptAsync(
                            $@"document.querySelector('{selector}').style.display");
                    if (display.Trim('\"') != "none")
                        error = "验证码输入错误";
                }
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                await Execute.OnUIThreadAsync(() =>
                {
                    MessageBox.Show(error.Trim('\"'), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                    }
                });
                return false;
            }

            return true;
        }

        #endregion
    }
}