using Microsoft.Maui.Controls.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace eBuddy
{
    public partial class MainPage : ContentPage
    {
        private List<Vehicle> vehicles = new List<Vehicle>();
        private Vehicle? selectedVehicle;

        public MainPage()
        {
            InitializeComponent();
            LoadVehiclesAsync();
            LoadServices();
        }

        private async void LoadVehiclesAsync()
        {
            vehicles = await App.Database.GetVehiclesAsync();
            VehiclePicker.ItemsSource = vehicles.Select(v => v.Name).ToList();
            if (vehicles.Count > 0)
            {
                VehiclePicker.SelectedIndex = 0;
                selectedVehicle = vehicles[0];
            }
        }

        /// <summary>
        /// Loads the list of services from the database and populates the UI.
        /// </summary>
        private async void LoadServices()
        {
            ServiceList.Children.Clear();
            if (selectedVehicle == null) return;
            var services = await App.Database.GetServicesAsync(selectedVehicle.Id);
            foreach (var service in services)
            {
                AddServiceBox(service);
            }
        }

        private void OnVehicleSelected(object? sender, EventArgs e)
        {
            if (VehiclePicker.SelectedIndex < 0 || VehiclePicker.SelectedIndex >= vehicles.Count)
            {
                selectedVehicle = null;
                ServiceList.Children.Clear();
                return;
            }
            selectedVehicle = vehicles[VehiclePicker.SelectedIndex];
            LoadServices();
        }

        private async void OnAddVehicleClicked(object? sender, EventArgs e)
        {
            var name = await DisplayPromptAsync(AppResources.NewVehicle, AppResources.EnterVehicleName);
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                await App.Database.AddVehicleAsync(new Vehicle { Name = name.Trim() });
                LoadVehiclesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not add vehicle: {ex.Message}", "OK");
            }
        }

        private async void OnMoreOptionsClicked(object? sender, EventArgs e)
        {
            string action = await DisplayActionSheet(AppResources.VehicleMenu, AppResources.Cancel, null, AppResources.AddVehicle, AppResources.DeleteVehicle);
            if (action == AppResources.DeleteVehicle)
            {
                if (selectedVehicle == null)
                {
                    await DisplayAlert("No Vehicle Selected", "Please select a vehicle to delete.", "OK");
                    return;
                }
                bool confirm = await DisplayAlert(AppResources.ConfirmDelete, $"{AppResources.ConfirmDeleteText} '{selectedVehicle.Name}'?", AppResources.Yes, AppResources.No);
                if (confirm)
                {
                    try
                    {
                        await App.Database.DeleteVehicleCascadeAsync(selectedVehicle);
                        selectedVehicle = null;
                        LoadVehiclesAsync();
                        LoadServices();
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"Could not delete vehicle: {ex.Message}", "OK");
                    }
                }
            }
            else if (action == AppResources.AddVehicle)
            {
                OnAddVehicleClicked(sender, e);
            }
        }

        /// <summary>
        /// Opens a new page to add a service when the "Add Service" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnAddServiceClicked(object? sender, EventArgs e)
        {
            if (selectedVehicle == null)
            {
                await DisplayAlert("No Vehicle Selected", "Please select a vehicle before adding a service.", "OK");
                return;
            }
            await Navigation.PushAsync(new NewServicePage(OnServicesUpdated, null, selectedVehicle));
        }

        private void OnServicesUpdated()
        {
            LoadServices();
        }

        /// <summary>
        /// Adds a service box to the service list with the specified title, icon, date, and mileage.
        /// </summary>
        /// <param name="service">Service entry containing details about the service</param>
        public void AddServiceBox(ServiceEntry service)
        {
            var border = new Border
            {
                Padding = 5,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(10, 10, 10, 10)
                }
            };

            var outerGrid = new Grid
            {
                ColumnSpacing = 10,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                }
            };

            var iconImage = new Image
            {
                Source = EnumHelper.GetIcon(service.ServiceType),
                WidthRequest = 40,
                HeightRequest = 40,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetRow(iconImage, 0);
            Grid.SetColumn(iconImage, 0);

            var innerGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Star }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star }
                    //new ColumnDefinition { Width = GridLength.Star }
                }
            };
            Grid.SetRow(innerGrid, 0);
            Grid.SetColumn(innerGrid, 1);

            var titleLabel = new Label
            {
                Text = service.Title,
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };
            Grid.SetRow(titleLabel, 0);
            Grid.SetColumn(titleLabel, 0);

            var numberLabel = new Label
            {
                Text = service.Mileage.ToString("N0") + "km",
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.End
            };
            Grid.SetRow(numberLabel, 1);
            Grid.SetColumn(numberLabel, 0);

            outerGrid.Children.Add(iconImage);
            outerGrid.Children.Add(innerGrid);

            innerGrid.Children.Add(titleLabel);
            innerGrid.Children.Add(numberLabel);

            border.Content = outerGrid;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                await Navigation.PushAsync(new ServiceDetailsPage(service, OnServicesUpdated));
            };
            border.GestureRecognizers.Add(tapGesture);

            ServiceList.Children.Add(border);
        }
    }
}
