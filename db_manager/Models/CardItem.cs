using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using db_manager.Utilities;

namespace db_manager.Models;

public sealed class CardItem : INotifyPropertyChanged
{
  private ButtonState _state;
  private string? _status;

  public DatabaseConfig? Config { get; init; }
  public Bitmap? LogoPath { get; set; }
  public string? Name { get; set; }

  public string? Status
  {
    get => _status;
    set => SetField(ref _status, value);
  }

  public ButtonState State
  {
    get => _state;
    set => SetField(ref _state, value);
  }

  [JsonIgnore] public ICommand ToggleCommand { get; set; } = new RelayCommand(() => { });

  public event PropertyChangedEventHandler? PropertyChanged;

  private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
  {
    if (EqualityComparer<T>.Default.Equals(field, value)) return;
    field = value;
    OnPropertyChanged(propertyName);
  }
}

public enum ButtonState
{
  Start,
  Stop,
  Loading
}
