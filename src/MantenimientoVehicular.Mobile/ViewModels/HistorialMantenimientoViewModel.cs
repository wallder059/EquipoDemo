using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MantenimientoVehicular.Mobile.Models;
using MantenimientoVehicular.Mobile.Services;

namespace MantenimientoVehicular.Mobile.ViewModels;

public sealed class HistorialMantenimientoViewModel : INotifyPropertyChanged
{
    private readonly IVehiculoApiService _apiService;
    private Vehiculo? _vehiculoSeleccionado;
    private ProximoMantenimiento? _proximoMantenimiento;
    private bool _isBusy;
    private bool _hasError;
    private string _errorMessage = string.Empty;
    private bool _loaded;
    private bool _suppressVehicleLoad;

    public HistorialMantenimientoViewModel(IVehiculoApiService apiService)
    {
        _apiService = apiService;
        ReloadCommand = new Command(async () => await LoadAsync(force: true), () => !IsBusy);
    }

    public ObservableCollection<Vehiculo> Vehiculos { get; } = [];
    public ObservableCollection<Mantenimiento> Mantenimientos { get; } = [];
    public ICommand ReloadCommand { get; }

    public Vehiculo? VehiculoSeleccionado
    {
        get => _vehiculoSeleccionado;
        set
        {
            if (!SetProperty(ref _vehiculoSeleccionado, value) || value is null)
            {
                return;
            }
            if (!_suppressVehicleLoad)
            {
                _ = LoadHistorialAsync(value);
            }
        }
    }

    public ProximoMantenimiento? ProximoMantenimiento
    {
        get => _proximoMantenimiento;
        private set
        {
            if (SetProperty(ref _proximoMantenimiento, value))
            {
                OnPropertyChanged(nameof(HasSuggestion));
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value) && ReloadCommand is Command command)
            {
                command.ChangeCanExecute();
            }
        }
    }

    public bool HasError { get => _hasError; private set => SetProperty(ref _hasError, value); }
    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public bool HasNoVehicles => !IsBusy && Vehiculos.Count == 0 && !HasError;
    public string VehiculosMessage => "No hay vehículos registrados para este usuario.";
    public bool HasHistory => Mantenimientos.Count > 0;
    public bool HasSuggestion => ProximoMantenimiento is not null;
    public bool ShowEmptyState => !IsBusy && !HasError && VehiculoSeleccionado is not null && Mantenimientos.Count == 0;

    public async Task LoadAsync(bool force = false)
    {
        if (_loaded && !force)
        {
            return;
        }
        IsBusy = true;
        ClearError();
        try
        {
            var vehicles = await _apiService.GetVehiculosAsync();
            Vehiculos.Clear();
            foreach (var vehiculo in vehicles)
            {
                Vehiculos.Add(vehiculo);
            }
            _loaded = true;
            OnPropertyChanged(nameof(HasNoVehicles));
            if (VehiculoSeleccionado is null && Vehiculos.Count > 0)
            {
                _suppressVehicleLoad = true;
                VehiculoSeleccionado = Vehiculos[0];
                _suppressVehicleLoad = false;
            }
            if (VehiculoSeleccionado is not null)
            {
                await LoadHistorialAsync(VehiculoSeleccionado);
            }
        }
        catch (Exception exception)
        {
            ShowError(exception);
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasNoVehicles));
        }
    }

    private async Task LoadHistorialAsync(Vehiculo vehicle)
    {
        IsBusy = true;
        ClearError();
        try
        {
            var history = await _apiService.GetHistorialAsync(vehicle.Id);
            if (vehicle != VehiculoSeleccionado)
            {
                return;
            }
            Mantenimientos.Clear();
            foreach (var item in history.Registros)
            {
                Mantenimientos.Add(item);
            }
            ProximoMantenimiento = history.ProximoMantenimiento;
            OnPropertyChanged(nameof(HasHistory));
            OnPropertyChanged(nameof(ShowEmptyState));
        }
        catch (Exception exception)
        {
            ShowError(exception);
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(ShowEmptyState));
        }
    }

    private void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private void ShowError(Exception exception)
    {
        Mantenimientos.Clear();
        ProximoMantenimiento = null;
        HasError = true;
        ErrorMessage = exception is HttpRequestException
            ? "No se pudo conectar con la API. Verifica la dirección del servidor y la red."
            : exception.Message;
        OnPropertyChanged(nameof(HasHistory));
        OnPropertyChanged(nameof(ShowEmptyState));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
