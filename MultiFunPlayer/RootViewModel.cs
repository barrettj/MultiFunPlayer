using MultiFunPlayer.Common.Messages;
using MultiFunPlayer.Settings;
using MultiFunPlayer.UI;
using MultiFunPlayer.UI.Controls.ViewModels;
using Newtonsoft.Json.Linq;
using NLog;
using Stylet;
using StyletIoC;
using System.Windows;
using System.Windows.Input;

namespace MultiFunPlayer;

public class RootViewModel : Conductor<IScreen>.Collection.AllActive
{
    protected Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly IEventAggregator _eventAggregator;

    [Inject] public ScriptViewModel Script { get; set; }
    [Inject] public VideoSourceViewModel VideoSource { get; set; }
    [Inject] public OutputTargetViewModel OutputTarget { get; set; }
    [Inject] public ShortcutViewModel Shortcut { get; set; }
    [Inject] public ApplicationViewModel Application { get; set; }

    public RootViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }

    protected override void OnActivate()
    {
        Items.Add(Script);
        Items.Add(VideoSource);
        Items.Add(OutputTarget);

        ActivateAndSetParent(Items);
        base.OnActivate();

        var settings = SettingsHelper.ReadOrEmpty(SettingsType.Application);
        _eventAggregator.Publish(new AppSettingsMessage(settings, AppSettingsMessageType.Loading));
    }

    public void OnInformationClick() => _ = DialogHelper.ShowOnUIThreadAsync(new InformationMessageDialogViewModel(showCheckbox: false), "RootDialog");
    public void OnShortcutClick() => _ = DialogHelper.ShowOnUIThreadAsync(Shortcut, "RootDialog");
    public void OnSettingsClick() => _ = DialogHelper.ShowOnUIThreadAsync(Application, "RootDialog");

    public void OnLoaded(object sender, EventArgs e)
    {
        Execute.PostToUIThread(async () =>
        {
            var settings = SettingsHelper.ReadOrEmpty(SettingsType.Application);
            if (!settings.TryGetValue("DisablePopup", out var disablePopupToken) || !disablePopupToken.Value<bool>())
            {
                var result = await DialogHelper.ShowAsync(new InformationMessageDialogViewModel(showCheckbox: true), "RootDialog").ConfigureAwait(true);
                if (result is not bool disablePopup || !disablePopup)
                    return;

                settings["DisablePopup"] = true;
                SettingsHelper.Write(SettingsType.Application, settings);
            }
        });
    }

    public void OnClosing(object sender, EventArgs e)
    {
        var settings = SettingsHelper.ReadOrEmpty(SettingsType.Application);
        _eventAggregator.Publish(new AppSettingsMessage(settings, AppSettingsMessageType.Saving));
        SettingsHelper.Write(SettingsType.Application, settings);
    }

    public void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Window window)
            return;

        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        window.DragMove();
    }
}
