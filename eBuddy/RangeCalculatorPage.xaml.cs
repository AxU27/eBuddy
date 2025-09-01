namespace eBuddy;

public partial class RangeCalculatorPage : ContentPage
{
	private double batteryCapacity = 75;
	private double kwhPer100Km = 20;
	private double distance = 0;
	private double batteryPercentage = 100;

    public RangeCalculatorPage()
	{
		InitializeComponent();
	}

	private void KwhStepSmall(object? sender, ValueChangedEventArgs e)
	{
        kwhStepperSmall.SetValue(Stepper.ValueProperty, batteryCapacity);
        batteryCapacity = e.NewValue;
        kwhLabel.Text = $"{batteryCapacity} kWh";
    }

    private void KwhStepBig(object? sender, ValueChangedEventArgs e)
    {
        kwhStepperBig.SetValue(Stepper.ValueProperty, batteryCapacity);
        batteryCapacity = e.NewValue;
        kwhLabel.Text = $"{batteryCapacity} kWh";
    }
}