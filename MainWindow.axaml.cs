using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace minutnik;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
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
                // Break session finished -> 1 cycle (work + break) finished
                _completedCycles++;
                _isWorkSession = true;
                int workMinutes = (int)(WorkDurationNumeric.Value ?? 25);
                _remainingTime = TimeSpan.FromMinutes(workMinutes);

                PauseTimer();
                UpdateSessionVisuals();
                UpdateDisplay();
            }
        }
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