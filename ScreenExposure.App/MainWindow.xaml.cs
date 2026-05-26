using System.ComponentModel;
using System.IO;
using System.Windows;
using ScreenExposure.Core;
using WpfOpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ScreenExposure.App;

public partial class MainWindow : Window
{
    private readonly DisplayGammaService gammaService = new();
    private readonly Dictionary<Channel, ToneCurve> curves = new()
    {
        [Channel.Master] = ToneCurve.Linear,
        [Channel.Red] = ToneCurve.Linear,
        [Channel.Green] = ToneCurve.Linear,
        [Channel.Blue] = ToneCurve.Linear
    };

    private Channel activeChannel = Channel.Master;
    private bool loaded;
    private bool suppressUpdates;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        DisplayComboBox.ItemsSource = DisplayDeviceService.GetDisplays();
        DisplayComboBox.SelectedIndex = DisplayComboBox.Items.Count > 0 ? 0 : -1;
        loaded = true;
        RefreshValueLabels();
        SetStatus("就绪，尚未写入屏幕曲线", false);
    }

    private void OnAdjustmentChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!loaded || suppressUpdates)
        {
            return;
        }

        RefreshValueLabels();
        ApplyCurrentAdjustment();
    }

    private void OnCurveChanged(object sender, ToneCurve curve)
    {
        if (!loaded || suppressUpdates)
        {
            return;
        }

        curves[activeChannel] = curve;
        ApplyCurrentAdjustment();
    }

    private void OnChannelChanged(object sender, RoutedEventArgs e)
    {
        if (!loaded)
        {
            return;
        }

        activeChannel = GetSelectedChannel();
        suppressUpdates = true;
        CurveEditor.Curve = curves[activeChannel];
        suppressUpdates = false;
    }

    private void OnApplyClicked(object sender, RoutedEventArgs e)
    {
        ApplyCurrentAdjustment();
    }

    private void OnRestoreClicked(object sender, RoutedEventArgs e)
    {
        TryRun("已恢复原始屏幕曲线", () => gammaService.Restore());
    }

    private void OnResetClicked(object sender, RoutedEventArgs e)
    {
        ApplyPreset(AdjustmentPresets.Default);
    }

    private void OnOutdoorPresetClicked(object sender, RoutedEventArgs e)
    {
        ApplyPreset(AdjustmentPresets.OutdoorDim);
    }

    private void OnNightPresetClicked(object sender, RoutedEventArgs e)
    {
        ApplyPreset(AdjustmentPresets.Night);
    }

    private void ApplyPreset(AdjustmentPreset preset)
    {
        suppressUpdates = true;
        ExposureSlider.Value = preset.ExposureStops;
        ContrastSlider.Value = preset.Contrast;
        GammaSlider.Value = preset.Gamma;
        curves[Channel.Master] = preset.MasterCurve;
        curves[Channel.Red] = preset.RedCurve;
        curves[Channel.Green] = preset.GreenCurve;
        curves[Channel.Blue] = preset.BlueCurve;
        CurveEditor.Curve = curves[activeChannel];
        suppressUpdates = false;
        RefreshValueLabels();
        ApplyCurrentAdjustment();
    }

    private void OnDefaultPresetClicked(object sender, RoutedEventArgs e)
    {
        OnResetClicked(sender, e);
    }

    private void OnSaveIccClicked(object sender, RoutedEventArgs e)
    {
        if (DisplayComboBox.SelectedItem is not DisplayDevice display)
        {
            SetStatus("没有可用显示器", true);
            return;
        }

        TryRun("ICC 已生成并关联到当前显示器", () =>
        {
            var ramp = GammaRamp.Generate(CreateAdjustment());
            var profilePath = IccProfileService.ExportVcgtProfile(ramp, $"ScreenExposure-{DateTime.Now:yyyyMMdd-HHmmss}");
            IccProfileService.InstallAndAssociate(profilePath, display.DeviceName);
        });
    }

    private void OnImportIccClicked(object sender, RoutedEventArgs e)
    {
        if (DisplayComboBox.SelectedItem is not DisplayDevice display)
        {
            SetStatus("没有可用显示器", true);
            return;
        }

        var dialog = new WpfOpenFileDialog
        {
            Title = "选择 ICC 滤镜文件",
            Filter = "ICC profiles (*.icc;*.icm)|*.icc;*.icm",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        TryRun("ICC 滤镜已导入并关联到当前显示器", () =>
        {
            IccProfileService.ImportAndAssociate(dialog.FileName, display.DeviceName);
        });
    }

    private void ApplyCurrentAdjustment()
    {
        TryRun("已应用到实时 Gamma Ramp", () =>
        {
            var ramp = GammaRamp.Generate(CreateAdjustment());
            gammaService.Apply(ramp);
        });
    }

    private ColorAdjustment CreateAdjustment()
    {
        return new ColorAdjustment(
            ExposureSlider.Value,
            ContrastSlider.Value,
            GammaSlider.Value,
            curves[Channel.Master],
            curves[Channel.Red],
            curves[Channel.Green],
            curves[Channel.Blue]);
    }

    private Channel GetSelectedChannel()
    {
        if (RedChannelButton.IsChecked == true)
        {
            return Channel.Red;
        }

        if (GreenChannelButton.IsChecked == true)
        {
            return Channel.Green;
        }

        return BlueChannelButton.IsChecked == true ? Channel.Blue : Channel.Master;
    }

    private void RefreshValueLabels()
    {
        ExposureValueText.Text = ExposureSlider.Value.ToString("+0.0;-0.0;0.0");
        ContrastValueText.Text = ContrastSlider.Value.ToString("+0.00;-0.00;0.00");
        GammaValueText.Text = GammaSlider.Value.ToString("0.00");
    }

    private void TryRun(string successMessage, Action action)
    {
        try
        {
            action();
            SetStatus(successMessage, false);
        }
        catch (Win32Exception ex)
        {
            SetStatus(ex.Message, true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            SetStatus(ex.Message, true);
        }
    }

    private void SetStatus(string message, bool isError)
    {
        StatusText.Text = message;
        StatusText.Foreground = isError
            ? System.Windows.Media.Brushes.LightCoral
            : System.Windows.Media.Brushes.LightGreen;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        gammaService.Dispose();
    }
}
