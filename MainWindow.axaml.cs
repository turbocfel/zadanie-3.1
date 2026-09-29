using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace minutnik;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private readonly ObservableCollection<string> _cycleHistory = new();
    private TimeSpan _remainingTime;
    private bool _isRunning;
    private bool _isWorkSession = true;
    private int _completedCycles;

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;

        CycleHistoryItemsControl.ItemsSource = _cycleHistory;

        ResetState();
    }

    private void StartPauseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            PauseTimer();
        }
        else
        {
            StartTimer();
        }
    }

    private void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        ResetAll();
    }

    private void DurationNumeric_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (TimeTextBlock == null || WorkDurationNumeric == null || BreakDurationNumeric == null)
        {
            return;
        }

        if (!_isRunning)
        {
            if (_isWorkSession)
            {
                int workMinutes = (int)(WorkDurationNumeric.Value ?? 25);
                _remainingTime = TimeSpan.FromMinutes(workMinutes);
            }
            else
            {
                int breakMinutes = (int)(BreakDurationNumeric.Value ?? 5);
                _remainingTime = TimeSpan.FromMinutes(breakMinutes);
            }
            UpdateDisplay();
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_remainingTime.TotalSeconds > 0)
        {
            _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
            UpdateDisplay();
        }

        if (_remainingTime <= TimeSpan.Zero)
        {
            if (_isWorkSession)
            {
                // Work session finished -> automatically start break session
                _isWorkSession = false;
                int breakMinutes = (int)(BreakDurationNumeric.Value ?? 5);
                _remainingTime = TimeSpan.FromMinutes(breakMinutes);

                UpdateSessionVisuals();
                UpdateDisplay();
                // Break timer keeps running automatically
            }
            else
            {
                // Break session finished -> 1 full cycle (work + break) finished!
                _completedCycles++;
                int workMinutes = (int)(WorkDurationNumeric.Value ?? 25);
                int breakMinutes = (int)(BreakDurationNumeric.Value ?? 5);

                AddCycleToHistory(workMinutes, breakMinutes);

                _isWorkSession = true;
                _remainingTime = TimeSpan.FromMinutes(workMinutes);

                PauseTimer();
                UpdateSessionVisuals();
                UpdateDisplay();
            }
        }
    }

    private void AddCycleToHistory(int workMinutes, int breakMinutes)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        _cycleHistory.Insert(0, $"Cykl #{_completedCycles}: {workMinutes} min pracy + {breakMinutes} min przerwy ({timestamp})");

        EmptyHistoryTextBlock.IsVisible = false;
        HistoryCountTextBlock.Text = $"{_completedCycles} {GetCycleNounPlural(_completedCycles)}";
    }

    private static string GetCycleNounPlural(int count)
    {
        if (count == 1) return "cykl";
        if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20)) return "cykle";
        return "cykli";
    }

    private void StartTimer()
    {
        _isRunning = true;
        _timer.Start();

        StartPauseButton.Content = "Pauza";
        StartPauseButton.Background = Brush.Parse("#E11D48");

        WorkDurationNumeric.IsEnabled = false;
        BreakDurationNumeric.IsEnabled = false;
    }

    private void PauseTimer()
    {
        _isRunning = false;
        _timer.Stop();

        StartPauseButton.Content = "Start";
        StartPauseButton.Background = Brush.Parse("#2563EB");

        WorkDurationNumeric.IsEnabled = true;
        BreakDurationNumeric.IsEnabled = true;
    }

    private void ResetState()
    {
        PauseTimer();
        _isWorkSession = true;
        int workMinutes = (int)(WorkDurationNumeric?.Value ?? 25);
        _remainingTime = TimeSpan.FromMinutes(workMinutes);
        UpdateSessionVisuals();
        UpdateDisplay();
    }

    private void ResetAll()
    {
        _completedCycles = 0;
        _cycleHistory.Clear();
        EmptyHistoryTextBlock.IsVisible = true;
        HistoryCountTextBlock.Text = "0 cykli";
        ResetState();
    }

    private void UpdateSessionVisuals()
    {
        if (_isWorkSession)
        {
            SessionBadge.Text = "SESJA PRACY";
            SessionBadgeBorder.Background = Brush.Parse("#3B1E2B");
            SessionBadgeBorder.BorderBrush = Brush.Parse("#F43F5E");
            SessionBadge.Foreground = Brush.Parse("#FDA4AF");
            StatusIndicatorDot.Fill = Brush.Parse("#F43F5E");
        }
        else
        {
            SessionBadge.Text = "PRZERWA";
            SessionBadgeBorder.Background = Brush.Parse("#14382B");
            SessionBadgeBorder.BorderBrush = Brush.Parse("#10B981");
            SessionBadge.Foreground = Brush.Parse("#6EE7B7");
            StatusIndicatorDot.Fill = Brush.Parse("#10B981");
        }
    }

    private void UpdateDisplay()
    {
        int totalMinutes = (int)_remainingTime.TotalMinutes;
        int seconds = _remainingTime.Seconds;
        TimeTextBlock.Text = $"{totalMinutes:D2}:{seconds:D2}";
        CyclesTextBlock.Text = _completedCycles.ToString();
    }
}