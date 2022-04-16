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

    public static void MinMaxForm(this IMainForm mainForm)
    {
        if (mainForm.WindowState == FormWindowState.Maximized)
        {
            mainForm.BoxMinMax.SetMaximize();
            mainForm.WindowState = FormWindowState.Normal;
            mainForm.Options.WindowSettings.IsMaximized = false;
        }
        else if (mainForm.WindowState == FormWindowState.Normal)
        {
            mainForm.BoxMinMax.SetMaximized();
            mainForm.WindowState = FormWindowState.Maximized;
            mainForm.Options.WindowSettings.IsMaximized = true;
        }
    }

    public static void SetupBoxButtons(this IMainForm mainForm)
    {
        mainForm.BoxClose.Size = new(36, 32);
        mainForm.BoxClose.Location = new(mainForm.Right - 37, 1);
        mainForm.BoxClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        mainForm.BoxClose.Click += (o, e) => mainForm.Close();

        mainForm.BoxMinMax.Size = new(36, 32);
        mainForm.BoxMinMax.Location = new(mainForm.Right - 73, 1);
        mainForm.BoxMinMax.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        mainForm.BoxMinMax.Click += (o, e) => mainForm.MinMaxForm();

        mainForm.BoxIcon.Size = new(36, 32);
        mainForm.BoxIcon.Location = new(mainForm.Right - 109, 1);
        mainForm.BoxIcon.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        mainForm.BoxIcon.Click += (o, e) =>
        {
            if (mainForm.WindowState == FormWindowState.Maximized || mainForm.WindowState == FormWindowState.Normal)
                mainForm.WindowState = FormWindowState.Minimized;
        };

        mainForm.Controls.Add(mainForm.BoxClose);
        mainForm.Controls.Add(mainForm.BoxMinMax);
        mainForm.Controls.Add(mainForm.BoxIcon);
    }

    public static void SetupStatusBar(this IMainForm mainForm)
    {
        mainForm.Clock.Size = new(100, 26);
        mainForm.Clock.Location = new Point(mainForm.Right - 105, mainForm.Bottom - 26);
        mainForm.Clock.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        mainForm.Clock.TextAlign = ContentAlignment.MiddleRight;
        mainForm.Clock.BackColor = BorderAndStatus;
        mainForm.Clock.ForeColor = GrayWhite;
        mainForm.Clock.Font = new("Cascadia Code", 11.0f);

        mainForm.ClockTimer.Interval = 250;
        mainForm.ClockTimer.Tick += (o, e) => mainForm.Clock.Text = $"{DateTime.Now:HH:mm:ss}";
        mainForm.ClockTimer.Start();

        mainForm.Controls.Add(mainForm.Clock);

        mainForm.StatusLabel.Size = new(mainForm.Clock.Left - 10, 26);
        mainForm.StatusLabel.AutoSize = false;
        mainForm.StatusLabel.Location = new Point(5, mainForm.Bottom - 26); 
        mainForm.StatusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        mainForm.StatusLabel.TextAlign = ContentAlignment.MiddleLeft;
        mainForm.StatusLabel.BackColor = BorderAndStatus;
        mainForm.StatusLabel.ForeColor = GrayWhite;
        mainForm.StatusLabel.Font = new("Cascadia Code", 11.0f);
        mainForm.StatusLabel.Click += (o, e) => Clipboard.SetText(mainForm.StatusLabel.Text);

        mainForm.Controls.Add(mainForm.StatusLabel);
    }

    public static async Task SetupBrowser(this IMainForm mainForm, string startUrl)
    {
        void WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (sender is not CoreWebView2 cwv2)
                return;

            mainForm
                .Logger
                .InformationLog($"{cwv2.Source} requires a custom useragent (e.g. Google login)");

            cwv2.Settings.UserAgent = "Sedge1.0";

            var secChUa = e.Request.Headers.FirstOrDefault(x => x.Key.ToLower() == "sec-ch-ua");
            if (secChUa.Key is null || secChUa.Value is null)
                return;

            mainForm
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

            mainForm
                .Logger
                .InformationLog($"Amended sec-ch-ua header:\r\n{newValue}");
        }

        void NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            //e.GetDeferral();
        }

        var env = await CoreWebView2Environment
            .CreateAsync(
                null,
                mainForm.Options.UserDataPath);

        await mainForm.Browser.EnsureCoreWebView2Async(env);

        mainForm.Browser.Location = new(1, 33);
        mainForm.Browser.Size = new(mainForm.Width - 2, mainForm.Height - 59);
        mainForm.Browser.Source = new(startUrl);
        mainForm.Browser.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
        mainForm.Browser.SourceChanged += (o, e) => mainForm.StatusLabel.Text = mainForm.Browser.Source.AbsoluteUri;
        mainForm.Browser.CoreWebView2.NavigationCompleted += (o, e) => mainForm.Title = mainForm.Browser.CoreWebView2.DocumentTitle;
        mainForm.Browser.CoreWebView2.NewWindowRequested += NewWindowRequested;

        if (string.IsNullOrEmpty(mainForm.DefaultUserAgent))
        {
            mainForm.DefaultUserAgent = mainForm.Browser.CoreWebView2.Settings.UserAgent;

            mainForm
                .Logger
                .InformationLog($"Default user agent has been saved for future reference:\r\n{mainForm.DefaultUserAgent}");
        }

        foreach (var filter in mainForm.CustomUserAgentFilters)
            mainForm
                .Browser
                .CoreWebView2
                .AddWebResourceRequestedFilter(filter, CoreWebView2WebResourceContext.All);

        mainForm.Browser.CoreWebView2.WebResourceRequested += WebResourceRequested;
        mainForm.Controls.Add(mainForm.Browser);
    }

    public static void SetupUrlNavigation(this IMainForm mainForm)
    {
        mainForm.Navigation.Location = new(35, 16);
        mainForm.Navigation.Navigate += (o, e) =>
        {
            mainForm.Logger.InformationLog($"Navigating to: {e.Url}");
            if (!string.IsNullOrEmpty(mainForm.DefaultUserAgent))
            {
                if (mainForm.DefaultUserAgent != mainForm.Browser.CoreWebView2.Settings.UserAgent)
                {
                    mainForm.Browser.CoreWebView2.Settings.UserAgent = mainForm.DefaultUserAgent;
                    mainForm.Logger.InformationLog($"User agent rolled back to the default one: {mainForm.DefaultUserAgent}");
                }
            }

            mainForm.Browser.Source = e.Url;
        };

        mainForm.Controls.Add(mainForm.Navigation);
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

    public static void SetupShowNavigate(this IMainForm mainForm)
    {
        mainForm.ShowNavigate.Location = new(1, 1);
        mainForm.ShowNavigate.Size = new(32, 32);
        mainForm.ShowNavigate.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        mainForm.ShowNavigate.BackgroundImage = Properties.Resources.SendImg32;
        mainForm.ShowNavigate.BackgroundImageLayout = ImageLayout.Stretch;

        mainForm.ShowNavigate.FlatStyle = FlatStyle.Flat;
        mainForm.ShowNavigate.ForeColor = GrayWhite;
        mainForm.ShowNavigate.BackColor = DarkPanel;

        mainForm.ShowNavigate.FlatAppearance.BorderSize = 0;
        mainForm.ShowNavigate.FlatAppearance.MouseDownBackColor = BlueBar;
        mainForm.ShowNavigate.FlatAppearance.MouseOverBackColor = LightDark;

        mainForm.ShowNavigate.Click += (o, e) =>
        {
            mainForm.Navigation.Url = mainForm.StatusLabel.Text;
            mainForm.Navigation.Visible = !mainForm.Navigation.Visible;
        };

        mainForm.Controls.Add(mainForm.ShowNavigate);
    }
}
