using Noxy.NET.UI.Enums;
using Noxy.NET.UI.Interfaces;
using Noxy.NET.UI.Models;

namespace Noxy.NET.UI.Services;

public sealed class ToastService(TimeProvider timeProvider) : IToastService, IDisposable
{
    private readonly Lock _lock = new();
    private readonly List<Toast> _visibleList = [];
    private readonly List<Toast> _waitingList = [];
    private readonly Dictionary<Guid, ITimer> _timerMap = [];

    private int _maxVisible = 3;
    private TimeSpan _defaultDuration = TimeSpan.FromSeconds(5);

    public event Action? Changed;

    public int MaxVisible
    {
        get
        {
            lock (_lock) return _maxVisible;
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);

            lock (_lock)
            {
                if (_maxVisible == value) return;

                _maxVisible = value;
                MoveWaitingToVisible();
            }

            Changed?.Invoke();
        }
    }

    public TimeSpan DefaultDuration
    {
        get
        {
            lock (_lock) return _defaultDuration;
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, TimeSpan.Zero);

            lock (_lock) _defaultDuration = value;
        }
    }

    public List<Toast> GetVisibleList()
    {
        lock (_lock) return [.. _visibleList];
    }

    public List<Toast> GetWaitingList()
    {
        lock (_lock) return [.. _waitingList];
    }

    public Toast Show(string message, StatusEnum status, bool isPermanent, TimeSpan? duration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (duration is { } value) ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, TimeSpan.Zero);

        Toast toast = new() { Message = message, Status = status, IsPermanent = isPermanent, Duration = duration };
        lock (_lock)
        {
            // The same message raised again while it is still shown or waiting would only repeat itself.
            Toast? existing = _visibleList.Concat(_waitingList).FirstOrDefault(x => x.Message == message && x.Status == status);
            if (existing is not null) return existing;

            _waitingList.Add(toast);
            MoveWaitingToVisible();
        }

        Changed?.Invoke();

        return toast;
    }

    public Toast ShowError(string message, bool isPermanent = true, TimeSpan? duration = null)
    {
        return Show(message, StatusEnum.Error, isPermanent, duration);
    }

    public Toast ShowWarning(string message, bool isPermanent = false, TimeSpan? duration = null)
    {
        return Show(message, StatusEnum.Warning, isPermanent, duration);
    }

    public Toast ShowInfo(string message, bool isPermanent = false, TimeSpan? duration = null)
    {
        return Show(message, StatusEnum.Info, isPermanent, duration);
    }

    public Toast ShowSuccess(string message, bool isPermanent = false, TimeSpan? duration = null)
    {
        return Show(message, StatusEnum.Success, isPermanent, duration);
    }

    public void Dismiss(Guid id)
    {
        lock (_lock)
        {
            bool removed = _visibleList.RemoveAll(x => x.ID == id) > 0 || _waitingList.RemoveAll(x => x.ID == id) > 0;
            if (!removed) return;

            StopTimer(id);
            MoveWaitingToVisible();
        }

        Changed?.Invoke();
    }

    public void Clear()
    {
        lock (_lock)
        {
            if (_visibleList.Count == 0 && _waitingList.Count == 0) return;

            foreach (Toast toast in _visibleList) StopTimer(toast.ID);

            _visibleList.Clear();
            _waitingList.Clear();
        }

        Changed?.Invoke();
    }

    public void Dispose()
    {
        lock (_lock)
        {
            foreach (ITimer timer in _timerMap.Values) timer.Dispose();

            _timerMap.Clear();
        }
    }

    // Called with the lock held: shows the waiting toasts, oldest first, while there is room.
    private void MoveWaitingToVisible()
    {
        while (_visibleList.Count < _maxVisible && _waitingList.Count > 0)
        {
            Toast toast = _waitingList[0];
            _waitingList.RemoveAt(0);
            _visibleList.Add(toast);
            StartTimer(toast);
        }
    }

    private void StartTimer(Toast toast)
    {
        if (toast.IsPermanent) return;

        Guid id = toast.ID;
        _timerMap[id] = timeProvider.CreateTimer(_ => Dismiss(id), null, toast.Duration ?? _defaultDuration, Timeout.InfiniteTimeSpan);
    }

    private void StopTimer(Guid id)
    {
        if (_timerMap.Remove(id, out ITimer? timer))
        {
            timer.Dispose();
        }
    }
}
