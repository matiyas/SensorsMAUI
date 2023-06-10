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

	#region Accelerometer
	private void AccelerometerSwitch_Toggled (object sender, ToggledEventArgs e)
	{
		if (!Accelerometer.Default.IsSupported) return;

		if (!Accelerometer.Default.IsMonitoring)
		{
			Accelerometer.Default.ReadingChanged += DisplayAccelerationReading;
			Accelerometer.Default.ShakeDetected += SignalShake;
			Accelerometer.Default.Start(SensorSpeed.UI);
		}
		else
		{
			Accelerometer.Stop();
			Accelerometer.Default.ShakeDetected -= SignalShake;
			Accelerometer.Default.ReadingChanged -= DisplayAccelerationReading;
        }
	}

	private void DisplayAccelerationReading(object sender, AccelerometerChangedEventArgs e)
	{
		var accelerationReading = new StringBuilder();
		accelerationReading.Append("Acceleration vector:\n");
		accelerationReading.Append($"\tX = {e.Reading.Acceleration.X}\n");
		accelerationReading.Append($"\tY = {e.Reading.Acceleration.Y}\n");
		accelerationReading.Append($"\tZ = {e.Reading.Acceleration.Z}\n");
		accelerationReading.Append($"Vector length: {9.81 * e.Reading.Acceleration.Length()}");


		labelAccelerometer.Text = accelerationReading.ToString();
		progressBarAcceleration.Progress = e.Reading.Acceleration.Length() / 10;

		if (DateTime.Now > _timeFromTheLastShake.AddSeconds(3))
			labelShaking.Text = "---";
	}

	private DateTime _timeFromTheLastShake;

	private void SignalShake (object sender, EventArgs e)
	{
		labelShaking.Text = $"Device shaking detected";
		_timeFromTheLastShake = DateTime.Now;

		ToggleFlashlightState();
		SignalWithVibrations();
	}
	#endregion

	#region Flashlight and vibrations
	private bool _flashlightTurnedOn = false;

	private async void ToggleFlashlightState ()
	{
		try
		{
			_flashlightTurnedOn = !_flashlightTurnedOn;
			if (_flashlightTurnedOn)
			{
				await Flashlight.Default.TurnOnAsync();
			}
			else
			{
				await Flashlight.Default.TurnOffAsync();
			} 
		}
		catch (FeatureNotSupportedException) 
		{
			await DisplayAlert(Title, "Device has no flashlight", "OK");
		}
		catch (PermissionException)
		{
			await DisplayAlert(Title, "Application has no permission to use flashlight", "OK");
		}
		catch (Exception)
		{
			await DisplayAlert(Title, "It's not possible to turn the flashlight on or off", "OK");
		}
	}

	private void FlashlightButton_Clicked (object sender, EventArgs e)
	{
		ToggleFlashlightState();
	}

	private void SignalWithVibrations ()
	{
		Vibration.Default.Vibrate(TimeSpan.FromSeconds(1.1));
	}
	#endregion

	#region Barometer
	private void BarometerSwitch_Toggled (object sender, ToggledEventArgs e)
	{
		if (!Barometer.Default.IsSupported)
		{
			labelBarometer.Text = "Device doesn't have barometer";
			return;
		}

		if (!Barometer.Default.IsMonitoring)
		{
			Barometer.Default.ReadingChanged += Barometer_ReadingChanged;
			Barometer.Default.Start(SensorSpeed.UI);
		}
		else
		{
			Barometer.Default.Stop();
			Barometer.Default.ReadingChanged -= Barometer_ReadingChanged;
		}
	}

	private void Barometer_ReadingChanged (object sender, BarometerChangedEventArgs e)
	{
		labelBarometer.Text = $"Barometer: {e.Reading.PressureInHectopascals} hPa";
		progressBarPressure.Progress = e.Reading.PressureInHectopascals / 2000;
    }
	#endregion

	#region Compass
	private void CompassSwitch_Toggled (Object sender, ToggledEventArgs e)
	{
		if (!Compass.Default.IsSupported)
		{
			labelCompass.Text = "Device doesn't have compass";
			return;
		}

        if (!Compass.Default.IsMonitoring)
        {
            Compass.Default.ReadingChanged += Compass_ReadingChanged;
            Compass.Default.Start(SensorSpeed.UI);
        }
        else
        {
            Compass.Default.Stop();
            Compass.Default.ReadingChanged -= Compass_ReadingChanged;
        }
    }

    private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
    {
        labelCompass.Text = $"Compass: {e.Reading.HeadingMagneticNorth}";
    }
    #endregion
}

