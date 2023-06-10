using Microsoft.Maui.Devices;
using System.Text;

namespace SensorsMAUI;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

		DispalyInfoAboutDevice();
        DisplayInfoAboutScreen();
        UpdateInformationsAboutBatteryStatus(
            null,
            new BatteryInfoChangedEventArgs(
				Battery.Default.ChargeLevel, 
				Battery.Default.State,
				Battery.Default.PowerSource));
    }

    private void ContentPage_Loaded(object sender, EventArgs e)
    {
		DeviceDisplay.Current.MainDisplayInfoChanged += 
			(object sender, DisplayInfoChangedEventArgs e) => DisplayInfoAboutScreen();
    }

    private void DispalyInfoAboutDevice()
	{
		var sb = new StringBuilder();
		var deviceType = DeviceInfo.Current.DeviceType == DeviceType.Virtual ? "Virtual" : "Real";

        sb.AppendLine("Informations about device:");
		sb.AppendLine($"Model: {DeviceInfo.Current.Model}");
		sb.AppendLine($"Manufacturer: {DeviceInfo.Current.Manufacturer}");
		sb.AppendLine($"Name: {DeviceInfo.Name}");
		sb.AppendLine($"System version: {DeviceInfo.VersionString}");
		sb.AppendLine($"Idiom: {DeviceInfo.Current.Idiom}");
		sb.AppendLine($"Platform: {DeviceInfo.Current.Platform}");
		sb.AppendLine($"Device type: {deviceType}");

		labelDevice.Text = sb.ToString();
	}

	private void DisplayInfoAboutScreen()
	{
		var sb = new StringBuilder();
		var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

        sb.AppendLine("Informations about screen");
		sb.AppendLine($"Resolution: {displayInfo.Width} x {displayInfo.Height}");
		sb.AppendLine($"Density: {displayInfo.Density}");
		sb.AppendLine($"Orientation: {displayInfo.Orientation}");
		sb.AppendLine($"Rotation: {displayInfo.Rotation}");
		sb.AppendLine($"Refresh rate: {displayInfo.RefreshRate} Hz");

		labelScreen.Text = sb.ToString();
	}

	#region Battery
	private bool _areBatteryStateChangesObserved;

	private void BatterySwitch_Toggled ( object sender, ToggledEventArgs e )
	{
		if (!_areBatteryStateChangesObserved)
		{
			Battery.Default.BatteryInfoChanged += UpdateInformationsAboutBatteryStatus;
		}
		else
		{
			Battery.Default.BatteryInfoChanged -= UpdateInformationsAboutBatteryStatus;
		}

		_areBatteryStateChangesObserved = !_areBatteryStateChangesObserved;
    }

	private void UpdateInformationsAboutBatteryStatus (
		object sender, 
		BatteryInfoChangedEventArgs e)
	{
		labelBatteryStatus.Text = e.State switch
		{ 
			BatteryState.Charging => "Charging",
			BatteryState.Discharging => "Discharging (charger is not connected)",
			BatteryState.Full => "Full",
			BatteryState.NotCharging => "Not charging",
			BatteryState.NotPresent => "Battery is not present",
			BatteryState.Unknown => "Battery state is not known",
			_ => "Unrecognized status"
		};

		labelBatteryLevel.Text = $"Battery is charged in {e.ChargeLevel * 100}%";
		progressBarBatteryLevel.Progress = e.ChargeLevel;
	}
    #endregion
}

