using SQLite;

namespace eBuddy
{
    public class ServiceDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public ServiceDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Vehicle>().Wait();
            _database.CreateTableAsync<ServiceEntry>().Wait();
            _database.CreateTableAsync<ServiceFile>().Wait();
        }

        // Vehicle methods
        public Task<List<Vehicle>> GetVehiclesAsync() =>
            _database.Table<Vehicle>().OrderBy(v => v.Name).ToListAsync();

        public async Task<int> AddVehicleAsync(Vehicle vehicle)
        {
            return await _database.InsertAsync(vehicle);
        }

        public Task<int> UpdateVehicleAsync(Vehicle vehicle) =>
            _database.UpdateAsync(vehicle);

        public async Task DeleteVehicleCascadeAsync(Vehicle vehicle)
        {
            var services = await GetServicesAsync(vehicle.Id);
            foreach (var service in services)
            {
                await DeleteServiceAsync(service);
            }
            await _database.DeleteAsync(vehicle);
        }


        // Service methods
        public Task<List<ServiceEntry>> GetServicesAsync(int vehicleId)
        {
            return _database.Table<ServiceEntry>().Where(s => s.VehicleId == vehicleId)
                .OrderByDescending(s => s.Mileage).ToListAsync();
        }

        public Task<int> AddServiceAsync(ServiceEntry service) =>
            _database.InsertAsync(service);

        public Task<int> UpdateServiceAsync(ServiceEntry service) =>
            _database.UpdateAsync(service);

        public async Task<int> DeleteServiceAsync(ServiceEntry service)
        {
            // Delete all files associated with the service
            var files = await GetFilesForServiceAsync(service.Id);
            foreach (var file in files)
            {
                await DeleteFileAsync(file);
            }

            var serviceDir = Path.Combine(FileSystem.AppDataDirectory, service.Id.ToString());
            if (Directory.Exists(serviceDir))
                Directory.Delete(serviceDir, true);

            return await _database.DeleteAsync(service);
        }

        // File methods
        public Task<List<ServiceFile>> GetFilesForServiceAsync(int serviceId) =>
            _database.Table<ServiceFile>().Where(f => f.ServiceEntryId == serviceId).ToListAsync();

        public Task<int> SaveFileAsync(ServiceFile file) =>
            _database.InsertAsync(file);

        public Task<int> DeleteFileAsync(ServiceFile file)
        {
            try { File.Delete(file.FilePath); } catch { /* Ignore errors */ };
            return _database.DeleteAsync(file);
        }
    }
}
