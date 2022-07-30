namespace Sedge.Browser.Helpers;

internal static class HelperExtensions
{
    public static (Uri, string, bool) GetUrlAndUserData(this string[] args)
    {
        if (args.Length != 0 && args.Length != 2)
            throw new ApplicationException("Either zero or two arguments (`start URL`, `userdata`) are required");

        var startUrl = "https://www.google.com";
        var userData = "DefaultShared";
        var isShared = true;

        if (args.Length == 2)
        {
            startUrl = args[0];
            userData = args[1];
            isShared = false;
        }

        return (new Uri(startUrl), userData, isShared);
    }

    public static void MinMaxForm(this IBrowserForm bForm)
    {
        if (bForm.WindowState == FormWindowState.Maximized)
        {
            bForm.BoxMinMax.SetMaximize();
            bForm.WindowState = FormWindowState.Normal;
            bForm.Options.WindowSettings.IsMaximized = false;
        }
        else if (bForm.WindowState == FormWindowState.Normal)
        {
            bForm.BoxMinMax.SetMaximized();
            bForm.WindowState = FormWindowState.Maximized;
            bForm.Options.WindowSettings.IsMaximized = true;
        }
    }

    public static void SetupBoxButtons(this IBrowserForm bForm)
    {
        bForm.BoxClose.Size = new(36, 32);
        bForm.BoxClose.Location = new(bForm.Right - 37, 1);
        bForm.BoxClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        bForm.BoxClose.Click += (o, e) => bForm.Close();
        bForm.BoxClose.BackColor = bForm.CurrentBackColor;

        bForm.BoxMinMax.Size = new(36, 32);
        bForm.BoxMinMax.Location = new(bForm.Right - 73, 1);
        bForm.BoxMinMax.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        bForm.BoxMinMax.Click += (o, e) => bForm.MinMaxForm();
        bForm.BoxMinMax.BackColor = bForm.CurrentBackColor;

        bForm.BoxIcon.Size = new(36, 32);
        bForm.BoxIcon.Location = new(bForm.Right - 109, 1);
        bForm.BoxIcon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        bForm.BoxIcon.Click += (o, e) =>
        {
            if (bForm.WindowState == FormWindowState.Maximized || bForm.WindowState == FormWindowState.Normal)
                bForm.WindowState = FormWindowState.Minimized;
        };
        bForm.BoxIcon.BackColor = bForm.CurrentBackColor;

        bForm.Controls.Add(bForm.BoxClose);
        bForm.Controls.Add(bForm.BoxMinMax);
        bForm.Controls.Add(bForm.BoxIcon);
    }

    public static void SetupStatusBar(this IBrowserForm bForm)
    {
        bForm.Clock.Size = new(100, 26);
        bForm.Clock.Location = new Point(bForm.Right - 105, bForm.Bottom - 26);
        bForm.Clock.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        bForm.Clock.TextAlign = ContentAlignment.MiddleRight;
        bForm.Clock.BackColor = bForm.CurrentBorderColor;
        bForm.Clock.ForeColor = GrayWhite;
        bForm.Clock.Font = new("Cascadia Code", 11.0f);

        bForm.ClockTimer.Interval = 250;
        bForm.ClockTimer.Tick += (o, e) => bForm.Clock.Text = $"{DateTime.Now:HH:mm:ss}";
        bForm.ClockTimer.Start();

        bForm.Controls.Add(bForm.Clock);

        bForm.StatusLabel.Size = new(bForm.Clock.Left - 10, 26);
        bForm.StatusLabel.AutoSize = false;
        bForm.StatusLabel.Location = new Point(5, bForm.Bottom - 26); 
        bForm.StatusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        bForm.StatusLabel.TextAlign = ContentAlignment.MiddleLeft;
        bForm.StatusLabel.BackColor = bForm.CurrentBorderColor;
        bForm.StatusLabel.ForeColor = GrayWhite;
        bForm.StatusLabel.Font = new("Cascadia Code", 11.0f);
        bForm.StatusLabel.Click += (o, e) => Clipboard.SetText(bForm.StatusLabel.Text);

        bForm.Controls.Add(bForm.StatusLabel);
    }

    public static async Task SetupBrowser(this IBrowserForm bForm, string startUrl)
    {
        void WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (sender is not CoreWebView2 cwv2)
                return;

            bForm
                .Logger
                .InformationLog($"{cwv2.Source} requires a custom useragent (e.g. Google login)");

            cwv2.Settings.UserAgent = "Sedge1.0";

            var secChUa = e.Request.Headers.FirstOrDefault(x => x.Key.ToLower() == "sec-ch-ua");
            if (secChUa.Key is null || secChUa.Value is null)
                return;

            bForm
                .Logger
                .InformationLog($"Original sec-ch-ua header:\r\n{secChUa.Value}");

            e.Request.Headers.RemoveHeader(secChUa.Key);
            var list = secChUa.Value.Split(",");

            var newValue = "";
            foreach (var item in list)
            {
                if (item.Contains("WebView2"))
                    continue;

                newValue += $"{item},";
            }

            if (newValue.Length >= 1)
                newValue = newValue.Remove(newValue.Length - 1);

            e.Request.Headers.SetHeader(secChUa.Key, newValue);

            bForm
                .Logger
                .InformationLog($"Amended sec-ch-ua header:\r\n{newValue}");
        }

        void NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            var f = bForm.BrowserForms.AppendNew();
            f.NewWindowArgs = e;
            f.Deferral = e.GetDeferral();

            f.Show();
        }

        if (bForm.EnvService.Environment is null)
        {
            await bForm.EnvService.SetupEnvironment();
        }

        await bForm.Browser.EnsureCoreWebView2Async(bForm.EnvService.Environment);

        bForm.Browser.Location = new(1, 33);
        bForm.Browser.Size = new(bForm.Width - 2, bForm.Height - 59);
        bForm.Browser.Source = new(startUrl);
        bForm.Browser.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
        bForm.Browser.SourceChanged += (o, e) => bForm.StatusLabel.Text = bForm.Browser.Source.AbsoluteUri;
        bForm.Browser.CoreWebView2.NavigationCompleted += (o, e) => bForm.Title = bForm.Browser.CoreWebView2.DocumentTitle;
        bForm.Browser.CoreWebView2.NewWindowRequested += NewWindowRequested;
        bForm.Browser.CoreWebView2.WebResourceRequested += WebResourceRequested;

        if (bForm.BrowserForms.ExternalBrowser is not null)
        {
            bForm.SetupExternalBrowser();
        }

        if (string.IsNullOrEmpty(bForm.DefaultUserAgent))
        {
            bForm.DefaultUserAgent = bForm.Browser.CoreWebView2.Settings.UserAgent;

            bForm
                .Logger
                .InformationLog($"Default user agent has been saved for future reference:\r\n{bForm.DefaultUserAgent}");
        }

        foreach (var filter in bForm.CustomUserAgentFilters)
            bForm
                .Browser
                .CoreWebView2
                .AddWebResourceRequestedFilter(filter, CoreWebView2WebResourceContext.All);

        OnStart(bForm);

        bForm.Controls.Add(bForm.Browser);
    }

    private static void SetupExternalBrowser(
        this IBrowserForm bForm)
    {
        var openExternal = bForm
            .Browser
            .CoreWebView2
            .Environment
            .CreateContextMenuItem(
                "Open with default browser",
                null,
                CoreWebView2ContextMenuItemKind.Command);

        CoreWebView2ContextMenuRequestedEventArgs? eArgs = null;

        void CustomItemSelected(object? sender, object e1)
        {
            if (eArgs is null)
            {
                return;
            }

            var fInfo = bForm
                        .BrowserForms
                        .ExternalBrowser
                        ?? throw new NullReferenceException();

            fInfo
                .RunUrlOnExternalBrowser(
                    eArgs.ContextMenuTarget.LinkUri.ToString());

            openExternal
                .CustomItemSelected -= CustomItemSelected;
        }

        bForm
            .Browser
            .CoreWebView2
            .ContextMenuRequested += ContextMenuRequested;

        void ContextMenuRequested(
            object? sender,
            CoreWebView2ContextMenuRequestedEventArgs e)
        {
            eArgs = e;

            if (!eArgs.ContextMenuTarget.HasLinkUri)
            {
                return;
            }

            eArgs.MenuItems.Add(openExternal);

            openExternal
                .CustomItemSelected += CustomItemSelected;
        }
    }

    public static void SetLocation(this IBrowserForm bForm)
    {
        var form = bForm as Form
            ?? throw new NotSupportedException(
                $"{nameof(BrowserForm)} implements {nameof(Form)}");

        form.Location = new(bForm.Options.WindowSettings.X, bForm.Options.WindowSettings.Y);
        form.ClientSize = new(bForm.Options.WindowSettings.Width, bForm.Options.WindowSettings.Height);

        if (!bForm.IsMainForm)
            form.Location = new(form.Left + 30, form.Top + 30);

        if (bForm.Options.WindowSettings.IsMaximized)
            bForm.MinMaxForm();
    }

    private static void OnStart(IBrowserForm bForm)
    {
        if (bForm.Deferral is null)
            return;

        if (bForm.NewWindowArgs is null)
            throw new NullReferenceException(
                $"{nameof(bForm.NewWindowArgs)} should not be null!");

        bForm.NewWindowArgs.NewWindow = bForm.Browser.CoreWebView2;
        bForm.Deferral.Complete();
        bForm.Browser.Source = new Uri(bForm.NewWindowArgs.Uri);
    }

    public static void SetupUrlNavigation(this IBrowserForm bForm)
    {
        bForm.Navigation.Location = new(35, 16);
        bForm.Navigation.Navigate += (o, e) =>
        {
            bForm.Logger.InformationLog($"Navigating to: {e.Url}");
            if (!string.IsNullOrEmpty(bForm.DefaultUserAgent))
            {
                if (bForm.DefaultUserAgent != bForm.Browser.CoreWebView2.Settings.UserAgent)
                {
                    bForm.Browser.CoreWebView2.Settings.UserAgent = bForm.DefaultUserAgent;
                    bForm.Logger.InformationLog($"User agent rolled back to the default one: {bForm.DefaultUserAgent}");
                }
            }

            if (e.NewWindow)
            {
                bForm
                    .Browser
                    .CoreWebView2
                    .ExecuteScriptAsync($"window.open('{e.Url}');");

                bForm
                    .Logger
                    .InformationLog(
                        $"Js window open invoked for url {e.Url}");

                return;
            }

            bForm.Browser.Source = e.Url;
        };

        bForm.Controls.Add(bForm.Navigation as UrlNavigation);
    }

    public static bool IsAMatch(this IEnumerable<string> urls, string url)
    {
        foreach (var u in urls)
        {
            var pattern = u
                .Replace(".", "\\.")
                .Replace("*", ".*");

            var regx = new Regex(pattern);

            if (regx.IsMatch(url))
                return true;
        }

        return false;
    }

    public static void SetupShowNavigate(this IBrowserForm bForm)
    {
        bForm.ShowNavigate.Location = new(1, 1);
        bForm.ShowNavigate.Size = new(32, 32);
        bForm.ShowNavigate.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        bForm.ShowNavigate.BackgroundImage = Properties.Resources.SendImg32;
        bForm.ShowNavigate.BackgroundImageLayout = ImageLayout.Stretch;

        bForm.ShowNavigate.FlatStyle = FlatStyle.Flat;
        bForm.ShowNavigate.ForeColor = GrayWhite;
        bForm.ShowNavigate.BackColor = DarkPanel;

        bForm.ShowNavigate.FlatAppearance.BorderSize = 0;
        bForm.ShowNavigate.FlatAppearance.MouseDownBackColor = BlueBar;
        bForm.ShowNavigate.FlatAppearance.MouseOverBackColor = LightDark;

        bForm.ShowNavigate.Click += (o, e) =>
        {
            bForm.Navigation.Url = bForm.StatusLabel.Text;
            bForm.Navigation.ToggleShow();
        };

        bForm.Controls.Add(bForm.ShowNavigate);
    }

    public static string GetSearchUrl(
        this IBrowserFormCollection formCollection, 
        string search)
    {
        var pattern = formCollection.SearchEngine switch
        {
            SearchEngines.Bing => "https://www.bing.com/search?q={0}",
            SearchEngines.Yahoo => "https://search.yahoo.com/search?p={0}",
            _ => "https://www.google.com/search?q={0}",
        };

        return string.Format(pattern, Uri.EscapeDataString(search));
    }

    public static void RunUrlOnExternalBrowser(
        this FileInfo browser,
        string url)
    {
        using var proc = new Process();
        proc.StartInfo = new(browser.FullName, url);

        _ = proc.Start();
    }
}
