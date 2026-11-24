# LOGIYGames Timers — полное руководство

## 1. Назначение

LOGIYGames Timers — библиотека таймеров для Unity. Она обновляет активные таймеры через Unity PlayerLoop и не требует отдельного MonoBehaviour или coroutine для каждого таймера.

Все публичные типы расположены в namespace `LOGIYGames.Timers`. Runtime assembly — `LOGIYGames.Timers`. Package manifest указывает Unity 2022.3.

| Тип | Назначение |
|---|---|
| `Timer` | Абстрактная база: состояние, callbacks, регистрация и IDisposable |
| `CountdownTimer` | Обратный отсчёт до нуля |
| `IntervalTimer` | Обратный отсчёт с событиями через заданный интервал |
| `FrequencyTimer` | Повторный callback N раз в секунду |
| `StopwatchTimer` | Накопление прошедшего времени |
| `TimerManager` | Статический реестр и обновление таймеров |
| `PlayerLoopUtils` | Редактирование и вывод дерева Unity PlayerLoop |

Для ИИ-агентов есть отдельный структурированный контракт `API.json` с сигнатурами, ограничениями, правилами жизненного цикла и машинно-читаемыми примерами.

## 2. Установка

1. Установите пакет в Unity-проект.
2. Проверьте, что целевая версия Unity соответствует `2022.3` или совместимой версии.
3. Убедитесь, что проект содержит assembly, указанный в исходном `Runtime/LOGIYGames.Timers.asmdef` как зависимость `Extensions`.
4. Используйте namespace `LOGIYGames.Timers` в скриптах.

Таймеры не требуют GameObject в сцене. `TimerBootstrapper` автоматически вставляет `TimerManager.UpdateTimers` в фазу `Update` PlayerLoop после загрузки assemblies. В Editor callback удаляется при выходе из Play Mode, после чего активные таймеры очищаются.

Импорт `UnityEditor` ограничен `#if UNITY_EDITOR`, поэтому runtime-код не требует UnityEditor API в Player-сборке.

## 3. Быстрый старт: CountdownTimer

```csharp
using LOGIYGames.Timers;
using UnityEngine;

public sealed class CountdownExample : MonoBehaviour
{
    private CountdownTimer timer;

    private void Start()
    {
        timer = new CountdownTimer(5f);
        timer.OnTimerStop += OnFinished;
        timer.Start();
    }

    private void OnFinished()
    {
        Debug.Log("Countdown finished");
    }

    private void OnDestroy()
    {
        timer?.Dispose();
    }
}
```

Таймер стартует с пяти секунд. Каждый кадр Unity передаёт ему scaled `Time.deltaTime`. Когда значение достигает нуля, таймер останавливается и вызывает `OnTimerStop`.

## 4. Модели времени и update loop

Все таймеры используют scaled game time (`Time.deltaTime`). При `Time.timeScale = 0` обновление времени прекращается; при `Time.timeScale = 0.5` таймеры идут вдвое медленнее относительно реального времени.

Обновление происходит в `UnityEngine.PlayerLoop.Update`. Bootstrapper вставляет callback в начало дочернего списка Update и перед вставкой удаляет существующие идентичные callback-системы. Это защищает от дублей при повторной инициализации с отключённым Domain Reload.

`TimerManager.UpdateTimers` обновляет снимок списка таймеров. Поэтому callback может безопасно вызвать Start, Stop, Pause, Resume или Dispose другого таймера во время обхода. Повторный вход в UpdateTimers игнорируется.

## 5. Жизненный цикл Timer

| Метод / свойство | Поведение |
|---|---|
| `Start()` | Сбрасывает CurrentTime к начальному значению и запускает таймер |
| `Stop()` | Останавливает и снимает с регистрации |
| `Pause()` | Приостанавливает и снимает с регистрации, сохраняя CurrentTime |
| `Resume()` | Возобновляет с сохранённого значения и регистрирует таймер |
| `Reset()` | Сбрасывает CurrentTime, не меняя running state |
| `Reset(float)` | Задаёт новое начальное значение и сбрасывает таймер |
| `Dispose()` | Окончательно снимает с регистрации и прекращает работу таймера |
| `IsRunning` | True, пока таймер зарегистрирован и обновляется |
| `CurrentTime` | Текущее значение в секундах; смысл зависит от типа таймера |
| `IsFinished` | Признак завершения, заданный конкретным таймером |

Важные различия:
- `Start()` всегда сбрасывает значение к начальному. Повторный Start во время работы перезапускает отсчёт, но не добавляет дубликат в реестр и не повторяет OnTimerStart.
- `Pause()` не вызывает OnTimerStop. Это пауза, а не завершение.
- `Resume()` не вызывает OnTimerStart и не сбрасывает время.
- `Stop()` вызывает OnTimerStop только при переходе из running в stopped.
- `Dispose()` запрещает дальнейшие Start, Resume и Reset; они выбрасывают ObjectDisposedException.
- `Reset` не запускает и не останавливает таймер.

Пример pause/resume:

```csharp
timer.Pause();
// CurrentTime сохранён, обновление не выполняется.
timer.Resume();
// Таймер продолжает отсчёт с сохранённого значения.
```

## 6. CountdownTimer

`CountdownTimer(float value = 0f)` уменьшает CurrentTime до нуля. Значение constructor и Reset(float) должно быть конечным и неотрицательным.

`Progress` возвращает долю прошедшего времени от 0 до 1. Для таймера с нулевой длительностью Progress сразу возвращает 1, а не NaN.

```csharp
var timer = new CountdownTimer(8f);
timer.OnTimerStop += () => Debug.Log("Time is up");
timer.Start();

// Можно читать в UI:
float progress = timer.Progress;
float secondsLeft = timer.CurrentTime;
```

При длительности 0 таймер считается завершённым по свойству IsFinished. Если его запустить, он зарегистрируется и будет остановлен на ближайшем update tick.

## 7. IntervalTimer

`IntervalTimer(float totalTime, float intervalSeconds)` считает от totalTime до нуля и вызывает `OnInterval` при каждом пересечении интервала. intervalSeconds должен быть конечным числом больше нуля.

```csharp
var timer = new IntervalTimer(10f, 2f);
timer.OnInterval += () => Debug.Log("Two seconds elapsed");
timer.OnTimerStop += () => Debug.Log("Finished");
timer.Start();
```

Для totalTime = 10 и intervalSeconds = 2 события происходят примерно при оставшемся времени 8, 6, 4, 2 и 0 секунд. Если один кадр пересёк несколько порогов, все соответствующие OnInterval callback выполняются в этом кадре. Если интервал длиннее общей длительности, interval callback не будет.

В `Tick` используется именно переданный deltaTime. Нулевой или отрицательный интервал не допускается, чтобы исключить бесконечный цикл.

## 8. FrequencyTimer

`FrequencyTimer(int ticksPerSecond)` вызывает OnTick с заданной частотой. Частота должна быть положительным целым числом.

```csharp
var timer = new FrequencyTimer(4); // 4 раза в секунду
timer.OnTick += () => Debug.Log("Tick");
timer.Start();
```

Внутренний аккумулятор сохраняет остаток времени. Если кадр задержался и deltaTime пересекла несколько интервалов, OnTick вызывается для каждого пропущенного такта, а остаток переносится на следующий кадр. Это обеспечивает среднюю заданную частоту, но очень длинный кадр может вызвать несколько callback подряд.

`Reset(int newTicksPerSecond)` меняет частоту и очищает аккумулятор. Значение должно быть больше нуля. `IsFinished` у FrequencyTimer возвращает true, когда таймер не запущен; таймер не имеет естественного конца.

## 9. StopwatchTimer

`StopwatchTimer` накапливает время от нуля. Он не завершает работу сам и всегда возвращает false для IsFinished.

```csharp
var stopwatch = new StopwatchTimer();
stopwatch.Start();

// Через некоторое время:
Debug.Log($"Elapsed: {stopwatch.CurrentTime:F2}s");
```

Для паузы используйте Pause, для продолжения — Resume, для завершения — Stop. Значение `CurrentTime` не сбрасывается при Stop; Start начинает новое измерение с начального значения (0).

## 10. События и безопасный disposal

У базового Timer есть делегаты `OnTimerStart` и `OnTimerStop`. У IntervalTimer есть `OnInterval`, у FrequencyTimer — `OnTick`. Их можно подписывать через `+=` и отписывать через `-=`.

```csharp
timer.OnTimerStart += HandleStart;
timer.OnTimerStop += HandleStop;

// Когда таймер больше не нужен:
timer.OnTimerStart -= HandleStart;
timer.OnTimerStop -= HandleStop;
timer.Dispose();
```

Всегда вызывайте Dispose для таймеров, срок жизни которых завершён. Это освобождает ссылку из TimerManager и предотвращает дальнейшие обновления. Dispose идемпотентен. Если таймер был активен, Dispose сначала переводит его в остановленное состояние и вызывает OnTimerStop.

## 11. TimerManager

TimerManager — статический класс. Основной пользовательский сценарий не требует прямого вызова его методов: Start/Stop/Pause/Resume автоматически управляют регистрацией.

| Метод | Назначение |
|---|---|
| `RegisterTimer(Timer)` | Регистрирует таймер; повторная регистрация того же экземпляра игнорируется |
| `DeregisterTimer(Timer)` | Удаляет таймер, если он зарегистрирован |
| `UpdateTimers()` | Обновляет снимок активных таймеров через Time.deltaTime |
| `Clear()` | Disposes все таймеры из текущего реестра и очищает его |

`Clear()` преимущественно вызывается bootstrapper при выходе из Play Mode в Editor. Для обычного завершения конкретного таймера используйте Stop или Dispose.

## 12. PlayerLoopUtils

Утилиты предназначены для расширенных сценариев работы с Unity PlayerLoop.

- `RemoveSystem<T>(ref PlayerLoopSystem, in PlayerLoopSystem)` удаляет все вложенные системы, у которых совпадают `type` и `updateDelegate`.
- `InsertSystem<T>(ref PlayerLoopSystem, in PlayerLoopSystem, int)` вставляет систему в первый найденный parent типа T. Возвращает false, если parent не найден. Некорректный индекс приводит к ArgumentOutOfRangeException.
- `PrintPlayerLoop(PlayerLoopSystem)` выводит дерево PlayerLoop в Unity Console и корректно обрабатывает пустые дочерние списки.

Для обычного использования таймеров напрямую вызывать эти методы не требуется.

## 13. Ограничения и диагностика

| Симптом | Причина / решение |
|---|---|
| Таймер не обновляется при Time.timeScale = 0 | Используется scaled Time.deltaTime; верните timeScale или реализуйте отдельный unscaled timer |
| Resume не продолжает таймер после Dispose | После Dispose объект окончательно закрыт; создайте новый экземпляр |
| ArgumentOutOfRangeException у CountdownTimer | Значение времени должно быть конечным и неотрицательным |
| ArgumentOutOfRangeException у IntervalTimer | intervalSeconds должен быть > 0 и конечным |
| ArgumentOutOfRangeException у FrequencyTimer | ticksPerSecond должен быть > 0 |
| Таймер зарегистрирован дважды | Текущий TimerManager предотвращает дубликаты по ссылке на экземпляр |
| Editor API not found in Player build | UnityEditor импортируется только под UNITY_EDITOR; используйте актуальный refactor |
| Не найдена Extensions assembly | Подключите зависимость, на которую ссылается исходный asmdef |

## 14. Совместимость и проверка

Package manifest указывает Unity 2022.3. Runtime assembly сохраняет существующий GUID reference на проектную Extensions assembly. Публичные имена классов, основной конструкторный API и методы таймеров сохранены.

Перед публикацией проверьте в целевой версии Unity:
1. Компиляцию в Editor и в Player.
2. CountdownTimer: положительная, нулевая и дробная длительность.
3. IntervalTimer: несколько порогов в одном кадре и интервал больше общей длительности.
4. FrequencyTimer: нормальный и искусственно большой deltaTime.
5. Pause/Resume, Start повторно, Stop, Dispose.
6. Enter Play Mode с отключённым Domain Reload.

Автоматические Unity EditMode/PlayMode tests в этом изменении не добавлены; компиляция в Unity Editor должна быть выполнена в целевом проекте.
