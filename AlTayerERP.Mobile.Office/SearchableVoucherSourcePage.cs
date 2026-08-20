using AlTayerERP.Mobile.Office.DTOs;

namespace AlTayerERP.Mobile.Office;

public sealed class SearchableVoucherSourcePage : ContentPage
{
    private readonly List<VoucherEntrySourceDto> _allItems;
    private readonly TaskCompletionSource<VoucherEntrySourceDto?> _completion = new();
    private readonly CollectionView _results;
    private readonly Label _emptyLabel;

    public SearchableVoucherSourcePage(IEnumerable<VoucherEntrySourceDto> items)
    {
        Title = "اختيار الصندوق أو البنك";
        FlowDirection = FlowDirection.RightToLeft;
        BackgroundColor = Color.FromArgb("#F6F8FB");
        _allItems = items.Where(x => !string.IsNullOrWhiteSpace(x.AccountId))
            .OrderBy(x => x.DisplayName)
            .ToList();

        var searchEntry = new Entry
        {
            Placeholder = "ابحث باسم الصندوق أو البنك أو الكود",
            BackgroundColor = Colors.White,
            TextColor = Color.FromArgb("#17324D"),
            PlaceholderColor = Color.FromArgb("#7A8896"),
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing
        };
        searchEntry.TextChanged += (_, _) => ApplyFilter(searchEntry.Text);

        _emptyLabel = new Label
        {
            Text = "لا توجد صناديق أو بنوك مرتبطة بحساب مالي لهذا الفرع.",
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = Color.FromArgb("#B42318"),
            Padding = 20,
            IsVisible = _allItems.Count == 0
        };

        _results = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            ItemsSource = _allItems,
            ItemTemplate = new DataTemplate(() =>
            {
                var title = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#17324D")
                };
                title.SetBinding(Label.TextProperty, nameof(VoucherEntrySourceDto.DisplayName));

                var type = new Label
                {
                    FontSize = 12,
                    TextColor = Color.FromArgb("#7A8896")
                };
                type.SetBinding(Label.TextProperty, new Binding(nameof(VoucherEntrySourceDto.SourceType),
                    stringFormat: "النوع: {0}"));

                return new Border
                {
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = 14,
                    BackgroundColor = Colors.White,
                    Stroke = Color.FromArgb("#D9E2EC"),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                    Content = new VerticalStackLayout
                    {
                        Spacing = 4,
                        Children = { title, type }
                    }
                };
            })
        };
        _results.SelectionChanged += OnSelectionChanged;

        var cancelButton = new Button
        {
            Text = "إلغاء",
            BackgroundColor = Color.FromArgb("#6B7280"),
            TextColor = Colors.White,
            CornerRadius = 10
        };
        cancelButton.Clicked += async (_, _) => await CloseAsync(null);

        Content = new Grid
        {
            Padding = 16,
            RowSpacing = 10,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            Children =
            {
                searchEntry,
                new Grid
                {
                    RowDefinitions = { new RowDefinition(GridLength.Star) },
                    Children = { _results, _emptyLabel }
                }.Row(1),
                cancelButton.Row(2)
            }
        };
    }

    public Task<VoucherEntrySourceDto?> WaitForSelectionAsync() => _completion.Task;

    protected override bool OnBackButtonPressed()
    {
        _ = CloseAsync(null);
        return true;
    }

    private void ApplyFilter(string? text)
    {
        var query = text?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allItems
            : _allItems.Where(x => x.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                   x.SourceType.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

        _results.ItemsSource = filtered;
        _emptyLabel.IsVisible = filtered.Count == 0;
        _emptyLabel.Text = _allItems.Count == 0
            ? "لا توجد صناديق أو بنوك مرتبطة بحساب مالي لهذا الفرع."
            : "لا توجد نتائج مطابقة للبحث.";
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not VoucherEntrySourceDto selected)
            return;

        _results.SelectedItem = null;
        await CloseAsync(selected);
    }

    private async Task CloseAsync(VoucherEntrySourceDto? selected)
    {
        if (_completion.Task.IsCompleted)
            return;

        _completion.TrySetResult(selected);
        await Navigation.PopModalAsync();
    }
}

internal static class SearchableVoucherSourcePageGridExtensions
{
    public static T Row<T>(this T view, int row) where T : View
    {
        Grid.SetRow(view, row);
        return view;
    }
}
