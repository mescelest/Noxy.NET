using Noxy.NET.EntityManagement.Application.Interfaces.Services;

namespace Noxy.NET.EntityManagement.Application.Services;

public class TaskBundlingService : ITaskBundlingService
{
    public async Task<(T1, T2)> WhenAll<T1, T2>(Task<T1> task1, Task<T2> task2)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);

        await Task.WhenAll(task1, task2).ConfigureAwait(false);
        return (task1.Result, task2.Result);
    }

    public async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);

        await Task.WhenAll(task1, task2, task3).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result);
    }

    public async Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);

        await Task.WhenAll(task1, task2, task3, task4).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result, task4.Result);
    }

    public async Task<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);

        await Task.WhenAll(task1, task2, task3, task4, task5).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result);
    }

    public async Task<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);

        await Task.WhenAll(task1, task2, task3, task4, task5, task6).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result);
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);

        await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result);
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);

        await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result);
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8, Task<T9> task9)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);
        ArgumentNullException.ThrowIfNull(task9);

        await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result, task9.Result);
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4, Task<T5> task5, Task<T6> task6, Task<T7> task7, Task<T8> task8,
        Task<T9> task9, Task<T10> task10)
    {
        ArgumentNullException.ThrowIfNull(task1);
        ArgumentNullException.ThrowIfNull(task2);
        ArgumentNullException.ThrowIfNull(task3);
        ArgumentNullException.ThrowIfNull(task4);
        ArgumentNullException.ThrowIfNull(task5);
        ArgumentNullException.ThrowIfNull(task6);
        ArgumentNullException.ThrowIfNull(task7);
        ArgumentNullException.ThrowIfNull(task8);
        ArgumentNullException.ThrowIfNull(task9);
        ArgumentNullException.ThrowIfNull(task10);

        await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10).ConfigureAwait(false);
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result, task9.Result, task10.Result);
    }
}
