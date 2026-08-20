namespace AlTayerERP.Mobile.Office;

public sealed class SearchableVoucherOptionPage : ContentPage
{
    private readonly List<VoucherOption> _all;
    private readonly TaskCompletionSource<VoucherOption?> _selection =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly CollectionView _list;

    public SearchableVoucherOptionPage(string title, IEnumerable<VoucherOption> options, bool allowClear = false)
    {
        Title = title;
        FlowDirection = FlowDirection.RightToLeft;
        BackgroundColor = Color.FromArgb("#F6F8FB");
        _all = options.OrderBy(x => x.DisplayName).ToList();

        var search = new SearchBar
        {
            Placeholder = "اكتب للبحث...",
            TextColor = Color.FromArgb("#17324D"),
            PlaceholderColor = Color.FromArgb("#7A8896"),
            BackgroundColor = Colors.White
        };
        search.TextChanged += (_, e) => ApplyFilter(e.NewTextValue);

        _list = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            ItemsSource = _all,
            EmptyView = new Label
            {
                Text = "لا توجد نتائج مطابقة.",
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromArgb("#7A8896"),
                Padding = 20
            },
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label
                {
                    FontSize = 16,
                    TextColor = Color.FromArgb("#17324D"),
                    VerticalTextAlignment = TextAlignment.Center
                };
                label.SetBinding(Label.TextProperty, nameof(VoucherOption.DisplayName));
                return new Border
                {
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = 14,
                    BackgroundColor = Colors.White,
                    Stroke = Color.FromArgb("#D9E2EC"),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                    Content = label
                };
            })
        };
        _list.SelectionChanged += OnSelectionChanged;

        var close = new Button
        {
            Text = "إلغاء",
            BackgroundColor = Color.FromArgb("#7A8896"),
            TextColor = Colors.White
        };
        close.Clicked += async (_, _) => await CloseAsync(null);

        var buttons = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star)] };
        buttons.Add(close);
        if (allowClear)
        {
            buttons.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var clear = new Button
            {
                Text = "بدون اختيار",
                BackgroundColor = Color.FromArgb("#B7791F"),
                TextColor = Colors.White,
                Margin = new Thickness(8, 0, 0, 0)
            };
            clear.Clicked += async (_, _) => await CloseAsync(new VoucherOption(string.Empty, "بدون اختيار"));
            buttons.Add(clear, 1);
        }

        Content = new Grid
        {
            Padding = 16,
            RowSpacing = 10,
            RowDefinitions =
            [
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            ],
            Children = { search, _list, buttons }
        };
        Grid.SetRow(_list, 1);
        Grid.SetRow(buttons, 2);
    }

    public Task<VoucherOption?> WaitForSelectionAsync() => _selection.Task;

    protected override bool OnBackButtonPressed()
    {
        _selection.TrySetResult(null);
        return base.OnBackButtonPressed();
    }

    private void ApplyFilter(string? text)
    {
        var query = text?.Trim();
        _list.ItemsSource = string.IsNullOrWhiteSpace(query)
            ? _all
            : _all.Where(x => x.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is VoucherOption option)
            await CloseAsync(option);
    }

    private async Task CloseAsync(VoucherOption? option)
    {
        _selection.TrySetResult(option);
        await Navigation.PopModalAsync();
    }
}

public sealed record VoucherOption(string Id, string DisplayName);
