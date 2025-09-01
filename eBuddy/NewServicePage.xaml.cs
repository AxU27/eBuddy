namespace eBuddy;

public partial class NewServicePage : ContentPage
{
    private readonly Action _onServiceAdded;
    private readonly List<ServiceType> _serviceTypes;
    private ServiceEntry? editingEntry;
    private Vehicle? vehicle;

    private List<FileResult> _filePickerResults = new List<FileResult>();
    private List<ServiceFile> _existingFiles = new List<ServiceFile>();
    public NewServicePage(Action onServiceAdded, ServiceEntry? service = null, Vehicle? _vehicle = null)
	{
		InitializeComponent();
        editingEntry = service;
        _onServiceAdded = onServiceAdded;
        vehicle = _vehicle;

        // Set user friendly names for the service types in the picker
        _serviceTypes = Enum.GetValues(typeof(ServiceType)).Cast<ServiceType>().ToList();
        ServiceTypePicker.ItemsSource = _serviceTypes.Select(st => EnumHelper.GetLocalizedDisplayName(st)).ToList();

        if (editingEntry != null)
        {
            Title = AppResources.EditServicePageTitle;
            TitleEntry.Text = editingEntry.Title;
            DatePicker.Date = editingEntry.Date;
            MileageEntry.Text = editingEntry.Mileage.ToString();
            DesctriptionEntry.Text = editingEntry.Description;
            ServicedByEntry.Text = editingEntry.ServicedBy;
            CostEntry.Text = editingEntry.ServiceCost > 0 ? editingEntry.ServiceCost.ToString() : string.Empty;
            ServiceTypePicker.SelectedIndex = (int)editingEntry.ServiceType;

            _existingFiles = App.Database.GetFilesForServiceAsync(editingEntry.Id).Result;
        }

        RefreshFileList();
    }

    /// <summary>
    /// Open the file picker to select multiple files when the "Pick File" button is clicked
    /// and add the selected files to the list view and the file picker results.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnPickFileClicked(object? sender, EventArgs e)
    {
        try
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.iOS, new[] { "public.image", "com.adobe.pdf" } },
                { DevicePlatform.Android, new[] { "image/*", "application/pdf" } },
                { DevicePlatform.WinUI, new[] { ".jpg", ".jpeg", ".png", ".pdf" } },
                { DevicePlatform.MacCatalyst, new[] { "public.image", "com.adobe.pdf" } }
            });

            var results = await FilePicker.PickMultipleAsync(new PickOptions
            {
                FileTypes = customFileType,
                PickerTitle = "Select Files"
            });

            if (results == null) return; // User canceled the picking

            foreach (var result in results)
            {
                _filePickerResults.Add(result);
            }

            RefreshFileList();
        }
        catch (OperationCanceledException)
        {
            // User canceled the picking operation, no action needed
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"File picking failed: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Confirm and remove a selected file from the list when the user taps on it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnFileSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is FileResult file)
        {
            bool confirm = await DisplayAlert(AppResources.DeleteFile, AppResources.DeleteFileText +  $" '{file.FileName}' ?", AppResources.Yes, AppResources.No);
            if (confirm)
            {
                _filePickerResults.Remove(file);
                RefreshFileList();
            }
        }
        else if (e.SelectedItem is ServiceFile existingFile)
        {
            bool confirm = await DisplayAlert(AppResources.DeleteFile, AppResources.DeleteFileText + $" '{existingFile.FileName}' ?", AppResources.Yes, AppResources.No);
            
            if (confirm)
            {
                if (!string.IsNullOrWhiteSpace(existingFile.FilePath) && File.Exists(existingFile.FilePath))
                {
                    try
                    {
                        File.Delete(existingFile.FilePath);
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"Could not delete file: {ex.Message}", "OK");
                    }
                }
                await App.Database.DeleteFileAsync(existingFile);
                _existingFiles.Remove(existingFile);
                RefreshFileList();
            }
        }
        filesListView.SelectedItem = null; // Deselect the item after handling
    }

    /// <summary>
    /// Refreshes the file list displayed in the user interface by combining existing files and newly selected
    /// files, and updates the data binding for the file list view.
    /// </summary>
    private void RefreshFileList()
    {
        var combined = new List<Object>();
        combined.AddRange(_existingFiles);
        combined.AddRange(_filePickerResults);

        filesListView.ItemsSource = null; // Clear the current binding
        filesListView.ItemsSource = combined; // Rebind the updated list
    }

    /// <summary>
    /// Saves the service entry and associated files to the database when the "Save" button is clicked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnSaveClicked(object? sender, EventArgs e)
	{
        var selectedIndex = ServiceTypePicker.SelectedIndex;
        if (selectedIndex < 0)
        {
            await DisplayAlert(AppResources.InvalidInput, AppResources.SelectServiceType, "OK");
            return;
        }
        var selectedType = _serviceTypes[selectedIndex];

        if (string.IsNullOrWhiteSpace(MileageEntry.Text) || !int.TryParse(MileageEntry.Text, out int mileage))
        {
            await DisplayAlert(AppResources.InvalidInput, AppResources.EnterValidMileage, "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(CostEntry.Text) || !int.TryParse(CostEntry.Text, out int cost))
        {
            await DisplayAlert(AppResources.InvalidInput, AppResources.EnterValidCost, "OK");
            return;
        }

        if (editingEntry != null)
        {
            editingEntry.Title = TitleEntry.Text;
            editingEntry.Date = DatePicker.Date;
            editingEntry.Mileage = mileage;
            editingEntry.Description = DesctriptionEntry.Text;
            editingEntry.ServicedBy = ServicedByEntry.Text;
            editingEntry.ServiceCost = cost;
            editingEntry.ServiceType = selectedType;

            await App.Database.UpdateServiceAsync(editingEntry);

            var serviceDir = Path.Combine(FileSystem.AppDataDirectory, editingEntry.Id.ToString());
            Directory.CreateDirectory(serviceDir);

            foreach (var result in _filePickerResults)
            {
                var destPath = Path.Combine(serviceDir, result.FileName);
                if (!File.Exists(destPath))
                {
                    // If the file doesn't exist, copy it to the service directory
                    using var stream = await result.OpenReadAsync();
                    using var newStream = File.OpenWrite(destPath);
                    await stream.CopyToAsync(newStream);

                    var serviceFile = new ServiceFile
                    {
                        FilePath = destPath,
                        FileName = result.FileName,
                        ServiceEntryId = editingEntry.Id
                    };
                    await App.Database.SaveFileAsync(serviceFile);
                }
            }
        }
        else
        {
            var service = new ServiceEntry
            {
                Title = TitleEntry.Text,
                Date = DatePicker.Date,
                Mileage = mileage,
                Description = DesctriptionEntry.Text,
                ServicedBy = ServicedByEntry.Text,
                ServiceCost = cost,
                ServiceType = selectedType,
                VehicleId = vehicle != null ? vehicle.Id : 0
            };
            await App.Database.AddServiceAsync(service);

            // Create a directory for the service entry
            var serviceDir = Path.Combine(FileSystem.AppDataDirectory, service.VehicleId.ToString());
            serviceDir = Path.Combine(serviceDir, service.Id.ToString());
            Directory.CreateDirectory(serviceDir);

            // Save the files in the directory associated with the service entry
            foreach (var result in _filePickerResults)
            {
                var destPath = Path.Combine(serviceDir, result.FileName);
                using var stream = await result.OpenReadAsync();
                using var newStream = File.OpenWrite(destPath);
                await stream.CopyToAsync(newStream);

                var serviceFile = new ServiceFile
                {
                    FilePath = destPath,
                    FileName = result.FileName,
                    ServiceEntryId = service.Id
                };
                await App.Database.SaveFileAsync(serviceFile);
            }
        }


        _onServiceAdded?.Invoke();
        await Navigation.PopAsync();
    }
}