#region FileHeader

// // Project:  VisualSvnLicenseGetter
// // File:  MainViewModel.cs
// // CreateTime:  2023-01-03 17:49
// // LastUpdateTime:  2023-01-04 17:23

#endregion

#region Nmaespaces

using System;
using System.IO;
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
using VisualSvnLicenseGetter.Views;

#endregion

namespace VisualSvnLicenseGetter.ViewModels
{
    internal class MainViewModel : ToolViewModelBase
    {
        private string _baseUrl;
        private bool _canEdit;
        private string _code;
        private BitmapImage _codeImageSource;
        private string _email;
        private string _license;
        private string _name;
        private string _organization;
        private string _svnVersion;
        private WebView2 _webView;

        public MainViewModel() : base("VisualSVN许可证获取")
        {
        }

        public string SvnVersion
        {
            get => _svnVersion;
            set => SetAndNotify(ref _svnVersion, value);
        }

        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        public string Organization
        {
            get => _organization;
            set => SetAndNotify(ref _organization, value);
        }

        public string Email
        {
            get => _email;
            set => SetAndNotify(ref _email, value);
        }

        public string Code
        {
            get => _code;
            set => SetAndNotify(ref _code, value);
        }

        public string License
        {
            get => _license;
            set => SetAndNotify(ref _license, value);
        }

        public bool CanEdit
        {
            get => _canEdit;
            set => SetAndNotify(ref _canEdit, value);
        }

        public bool CanLoadInfo => !string.IsNullOrWhiteSpace(SvnVersion);

        public BitmapImage CodeImageSource
        {
            get => _codeImageSource;
            set => SetAndNotify(ref _codeImageSource, value);
        }

        public bool CanRequestLicense => !string.IsNullOrWhiteSpace(Name)
                                         && !string.IsNullOrWhiteSpace(Email)
                                         && !string.IsNullOrWhiteSpace(Code) &&
                                         _webView != null &&
                                         _webView.CoreWebView2 != null &&
                                         _webView.CoreWebView2.Source ==
                                         "https://www.visualsvn.com/server/licensing/evaluation/" &&
                                         string.IsNullOrWhiteSpace(License);

        public bool CanSaveToFile => !string.IsNullOrWhiteSpace(License);

        public bool CanCopyToClipboard => !string.IsNullOrWhiteSpace(License);

        public void LoadInfo()
        {
            _baseUrl = $"https://www.visualsvn.com/go/2196/?v={SvnVersion}";
            _webView.CoreWebView2.Navigate(_baseUrl);
            License = string.Empty;
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
            //License = string.Empty;
        }

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
                    break;
            }
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            if (View is MainView view)
            {
                _webView = view.WebView;
                _webView.NavigationCompleted -= WebViewNavigationCompleted;
                _webView.NavigationCompleted += WebViewNavigationCompleted;
                InitializeWebViewAsync();
            }
        }

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
                if (e.Request.Uri == "https://www.visualsvn.com/server/licensing/evaluation/")
                {
                    //switch (e.Request.Method)
                    //{
                    //    case "GET":
                    //        License = string.Empty;
                    //        break;
                    //    case "POST":
                    //        if (await ServiceValidate())
                    //            GetLicense();
                    //        break;
                    //}
                }
                else if (e.Request.Uri.Contains("Captcha.aspx"))
                {
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
                }
                else
                {
                    CanEdit = false;
                }
            }
        }

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
    }
}