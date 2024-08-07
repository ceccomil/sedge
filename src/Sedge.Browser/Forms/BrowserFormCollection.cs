﻿using System.Collections;

namespace Sedge.Browser.Forms;

public class BrowserFormCollection : IBrowserFormCollection
{
    private readonly List<BrowserForm> _forms = new();

    public IBrowserForm MainForm { get; private set; } = null!;

    public IYesNoDialogForm YesNoDialogForm { get; }

    public int Count => _forms.Count;

    public bool IsReadOnly { get; } = false;

    public SearchEngines SearchEngine { get; }

    public FileInfo? ExternalBrowser { get; }

    private readonly IConfiguration _conf;
    private readonly ICaptainLogger _logger;
    private readonly ICaptainLogger<BrowserForm> _browserLogger;
    private readonly ICaptainLogger<UrlNavigation> _urlNavigationLogger;
    private readonly SedgeBrowserOptions _options;
    private readonly IBrowserEnv _envService;
    private readonly IDrawBorders _drawBorders;
    private readonly IProcessHooks _hooks;
    private readonly IEnumerable<string> _customUserAgentFilters;
    private readonly IBrowsersList _browsers;

    public BrowserFormCollection(
        ICaptainLogger<BrowserFormCollection> logger,
        ICaptainLogger<BrowserForm> browserLogger,
        ICaptainLogger<UrlNavigation> urlNavigationLogger,
        IOptions<SedgeBrowserOptions> opts,
        IDrawBorders drawBorders,
        IConfiguration conf,
        IProcessHooks hooks,
        IBrowserEnv browserEnv,
        IYesNoDialogForm yesNoDialogForm,
        IBrowsersList browsers)
    {
        _conf = conf;
        _logger = logger;
        _browserLogger = browserLogger;
        _urlNavigationLogger = urlNavigationLogger;
        _options = opts.Value;
        _drawBorders = drawBorders;
        _envService = browserEnv;
        _browsers = browsers;

        YesNoDialogForm = yesNoDialogForm;

        _customUserAgentFilters = conf
            .GetSection("CustomUserAgentRequired")
            .Get<IEnumerable<string>>()
            ?? Array.Empty<string>();

        _hooks = hooks;

        var se = conf["SearchEngine"];

        if (string.IsNullOrWhiteSpace(se))
        {
            return;
        }

        try
        {
            SearchEngine = (SearchEngines)Convert.ToInt32(se);
        }
        catch (Exception ex)
        {
            _logger
                .ErrorLog(
                    $"SearchEngine value from appsettings `{se}` is not valid!",
                    ex);
        }

        _logger
            .InformationLog($"Selected search engine {SearchEngine}");


        ExternalBrowser = GetExternalBrowser();
    }

    private FileInfo? GetExternalBrowser()
    {
        var path = _conf["ExternalBrowser"];
        if (File.Exists(path))
        {
            _logger
                .InformationLog(
                    $"External browser path: {path}");

            return new(path);
        }

        return null;
    }

    public IBrowserForm AppendNew()
    {
        var form = new BrowserForm(
                _browserLogger,
                _drawBorders,
                _options,
                _envService,
                _customUserAgentFilters,
                _hooks,
                this,
                _urlNavigationLogger,
                _browsers);

        if (!_forms.Any())
            MainForm = form;

        Add(form);

        return form;
    }

    public void Add(BrowserForm item)
    {
        if (Contains(item))
            throw new NotSupportedException(
                "Item already added to the collection");

        _forms.Add(item);

        item.FormClosed += (o, e) => FormClosed(item);
        item.FormClosing += (o, e) => FormClosing(item, e);

        _logger
            .InformationLog(
                "New form added to the collection");
    }

    private void FormClosed(BrowserForm bForm)
    {
        bForm.Dispose();
        Remove(bForm);
    }

    private void FormClosing(BrowserForm bForm, FormClosingEventArgs e)
    {
        if (bForm == MainForm && _forms.Count != 1)
        {
            var result = YesNoDialogForm
                .ShowDialog(
                    bForm,
                    "This is the main form\r\n" +
                    "Closing it will close all others opened\r\n" +
                    "Do you want to proceed?");

            if (result != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }

            foreach (var f in _forms.Where(x => x != MainForm).ToList())
            {
                f.Close();
            }

            bForm.Close();
        }
    }

    public void Clear() => _forms.Clear();

    public bool Contains(BrowserForm item) => _forms
        .Any(x => x.Handle == item.Handle);

    public void CopyTo(BrowserForm[] array, int arrayIndex) => _forms.CopyTo(array, arrayIndex);

    public IEnumerator<BrowserForm> GetEnumerator() => _forms.GetEnumerator();

    public bool Remove(BrowserForm item) => _forms.Remove(item);

    IEnumerator IEnumerable.GetEnumerator() => _forms.GetEnumerator();
}
