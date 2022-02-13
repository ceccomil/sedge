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

        mainForm.Controls.Add(mainForm.StatusLabel);
    }

    public static void SetupBrowser(this IMainForm mainForm, string startUrl)
    {
        mainForm.Browser.Location = new(1, 33);
        mainForm.Browser.Size = new(mainForm.Width - 2, mainForm.Height - 59);
        mainForm.Browser.Source = new(startUrl);
        mainForm.Browser.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
        mainForm.Browser.SourceChanged += (o, e) => mainForm.StatusLabel.Text = mainForm.Browser.Source.AbsoluteUri;

        mainForm.Controls.Add(mainForm.Browser);
    }
}
