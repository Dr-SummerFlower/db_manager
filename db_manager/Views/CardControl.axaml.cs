using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace db_manager.Views;

public partial class CardControl : UserControl
{
    private static readonly StyledProperty<ICommand> ToggleCommandProperty =
        AvaloniaProperty.Register<CardControl, ICommand>(nameof(ToggleCommand));

    public ICommand ToggleCommand
    {
        get => GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }

    public CardControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}