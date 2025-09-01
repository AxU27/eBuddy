namespace eBuddy;

public partial class ServiceDetailsPage : ContentPage
{
	private readonly ServiceEntry _service;
	private readonly Action _onServiceDeletedOrEdited;
    public ServiceDetailsPage(ServiceEntry service, Action onServiceDeleted)
	{
		InitializeComponent();
		_service = service;
		_onServiceDeletedOrEdited = onServiceDeleted;

        // Bind the service details to the UI elements
        RefreshUI();
    }

    /// <summary>
    /// Opens the selected file when an item in the list view is selected.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnFileSelected(object sender, SelectedItemChangedEventArgs e)
	{
		if (e.SelectedItem is ServiceFile file)
		{
			try
			{
                var filePath = file.FilePath;
                if (filePath != null)
                {
                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(filePath)
                    });
                }
                filesListView.SelectedItem = null; // Deselect the item
            }
			catch (Exception ex)
			{
				await DisplayAlert("Error", "Could not open file: " + ex.Message, "OK");
            }
        }
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new NewServicePage(RefreshUI, _service));
    }

    private void RefreshUI()
    {
        // Refresh the service details
        TitleLabel.Text = _service.Title;
        DateLabel.Text = _service.Date.DayOfWeek.ToString() + " " + _service.Date.ToString("dd.MM.yyyy");
        MileageLabel.Text = _service.Mileage.ToString("N0") + "km";
        DescriptionLabel.Text = _service.Description;
        ServicedByLabel.Text = _service.ServicedBy;
        CostLabel.Text = _service.ServiceCost > 0 ? _service.ServiceCost.ToString() + "€" : "Free";
        TypeLabel.Text = EnumHelper.GetLocalizedDisplayName(_service.ServiceType);

        // Get the files associated with the service and bind them to the ListView
        var files = App.Database.GetFilesForServiceAsync(_service.Id).Result;
        filesListView.ItemsSource = null; // Clear the current binding
        filesListView.ItemsSource = files;

        _onServiceDeletedOrEdited?.Invoke();
    }

    /// <summary>
    /// Deletes the service and all associated files when the "Delete Service" button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnDeleteClicked(object? sender, EventArgs e)
	{
		bool confirm = await DisplayAlert(AppResources.ConfirmDelete, AppResources.ConfirmDeleteText + " this service?", AppResources.Yes, AppResources.No);
		if (confirm)
		{
            await App.Database.DeleteServiceAsync(_service);
			_onServiceDeletedOrEdited?.Invoke();
			await Navigation.PopAsync();
        }
    }
}