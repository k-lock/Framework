# Transition System

**Version**: 1.0  
**Last Updated**: 10/2025

--------------------------------------------------------------------------------------

A fluent API for composing and managing asynchronous operations in Unity using UniTask.

--------------------------------------------------------------------------------------

## 🌟 Features

- ✅ **Fluent API** - Chain operations naturally: `.And()`, `.Or()`, `.Then()`
- ✅ **UniTask Integration** - Seamlessly wrap any `UniTask` with `.WaitForComplete()`
- ✅ **Cancellation Support** - Full `CancellationToken` propagation
- ✅ **Timeout Support** - Add timeouts to any transition with `.WithTimeout()`
- ✅ **External Library Integration** - Built-in support for DOTween and UI Toolkit
- ✅ **Composable** - Combine simple transitions into complex workflows
- ✅ **Type-Safe** - Compile-time safety with `ITransition` interface
- ✅ **Performance Optimized** - Minimal overhead, GC-friendly
- ✅ **Comprehensive Tests** - 70+ test cases covering all scenarios

--------------------------------------------------------------------------------------

## 📦 Installation

### **Requirements**
- Unity 2021.3 or later
- UniTask package (com.cysharp.unitask)
- DOTween (optional, for tween integration)

### **Setup**
1. Copy the `Transitions` folder to your project
2. Ensure UniTask is installed via Package Manager
3. (Optional) Install DOTween for tween support | add UNITASK_DOTWEEN_SUPPORT define

--------------------------------------------------------------------------------------

## 🏗️ Architecture

### **Directory Structure**
```
Transitions/
├── Base/
│   ├── ITransition.cs              # Core interface
│   └── TransitionBase.cs           # Abstract base class
├── Implementations/
│   ├── DelayTransition.cs          # Delay implementation
│   ├── EmptyTransition.cs          # Empty implementation
│   ├── UniTaskTransition.cs        # UniTask wrapper
│   ├── CombinedAllTransition.cs    # And implementation
│   ├── CombinedAnyTransition.cs    # Or implementation
│   ├── SequentialTransition.cs     # Then implementation
│   ├── TimeoutTransition.cs        # Timeout wrapper
│   ├── DoTweenTransition.cs        # DOTween wrapper
│   └── VisualElementTransitionEndTransition.cs  # UI Toolkit wrapper
├── Extensions/
│   ├── TransitionExtensions.cs     # Core extensions
│   ├── UniTaskExtensions.cs        # UniTask extensions
│   ├── TweenTransitionExtensions.cs # DOTween extensions
│   └── VisualElementExtensions.cs  # UI Toolkit extensions
├── Tests/
│   ├── TransitionTests.cs          # Comprehensive test suite
├── Transition.cs                   # Static factory
├── README.md                       # This file
```

------------------------------------------------------------------------------------

### **Design Patterns**

#### **1. Fluent Interface**
Method chaining for readable code:
```csharp
await transition
    .And(other)
    .Then(next)
    .WithTimeout(5f)
    .WithCancellation(ct);
```

#### **2. Composite Pattern**
Combine simple transitions into complex ones:
```csharp
ITransition complex = simple1.And(simple2).Then(simple3);
```

#### **3. Decorator Pattern**
Add behavior without modifying original:
```csharp
ITransition withTimeout = transition.WithTimeout(5f);
```

#### **4. Factory Pattern**
Static factory methods for creation:
```csharp
ITransition delay = Transition.Delay(1f);
ITransition all = Transition.WhenAll(t1, t2, t3);
```

------------------------------------------------------------------------------------

## 🚀 Quick Start

### **Basic Delay**
```csharp
using Core.Utils.Transitions.Base;
using Core.Utils.Transitions.Extensions;

// Wait 2 seconds
await Transition.Delay(2f);
```

### **Wrap UniTask**
```csharp
UniTask myTask = LoadDataAsync();
await myTask.WaitForComplete();
```

### **Parallel Execution**
```csharp
// Wait for both to complete
await Transition.Delay(1f)
    .And(LoadDataAsync().WaitForComplete());
```

### **Sequential Execution**
```csharp
// Execute in order
await ShowLoadingScreen().WaitForComplete()
    .Then(LoadDataAsync().WaitForComplete())
    .Then(HideLoadingScreen().WaitForComplete());
```

### **With Timeout**
```csharp
try
{
    await LoadDataAsync()
        .WaitForComplete()
        .WithTimeout(5f);
}
catch (TimeoutException)
{
    Debug.LogError("Loading timed out!");
}
```

### **With Cancellation**
```csharp
CancellationTokenSource cts = new();

try
{
    await LoadDataAsync()
        .WaitForComplete()
        .WithCancellation(cts.Token);
}
catch (OperationCanceledException)
{
    Debug.Log("Loading cancelled");
}

// Cancel from elsewhere
cts.Cancel();
```

------------------------------------------------------------------------------------

## 📚 Core Concepts

### **ITransition Interface**
All transitions implement `ITransition`:
```csharp
public interface ITransition
{
    UniTask WaitAsync(CancellationToken cancellationToken = default);
}
```

### **Transition Types**

#### **1. Empty Transition**
Completes immediately.
```csharp
await Transition.Create();
```

#### **2. Delay Transition**
Waits for a specified duration.
```csharp
await Transition.Delay(1.5f); // 1.5 seconds
```

#### **3. UniTask Transition**
Wraps any `UniTask`.
```csharp
UniTask task = MyAsyncOperation();
await task.WaitForComplete();
```

#### **4. DOTween Transition**
Wraps DOTween tweens.
```csharp
Tween tween = transform.DOMove(target, 1f);
await tween.WaitForComplete();
```

#### **5. VisualElement Transition**
Waits for UI Toolkit transition events.
```csharp
VisualElement element = new();
await element.WaitForComplete();
```

------------------------------------------------------------------------------------

## 🔗 Composition Operators - Detailed Guide

### **And (Parallel Execution)**

**What it does**: Executes multiple transitions **simultaneously** and waits for **all** to complete.

**When to use**:
- Loading multiple resources at once
- Running independent animations in parallel
- Performing multiple network requests simultaneously
- Any scenario where operations don't depend on each other

**How it works**:
- All transitions start **immediately**
- Completes when the **slowest** transition finishes
- Total time = **max(transition1, transition2, ...)**
- Cancellation propagates to **all** transitions

**Basic Example**:
```csharp
// Both delays start at the same time
await Transition.Delay(1f)
    .And(Transition.Delay(2f));
// Total time: 2 seconds (not 3!)
```

**Practical Example - Parallel Resource Loading**:
```csharp
async UniTask LoadGameResourcesAsync()
{
    // All three load operations run simultaneously
    await LoadTextures().WaitForComplete()
        .And(LoadAudio().WaitForComplete())
        .And(LoadPrefabs().WaitForComplete());
    
    Debug.Log("All resources loaded!");
}
```

**Chaining Multiple And Operations**:
```csharp
// Load 5 things in parallel
await Transition.Delay(1f)
    .And(Transition.Delay(2f))
    .And(Transition.Delay(3f))
    .And(Transition.Delay(4f))
    .And(Transition.Delay(5f));
// Total time: 5 seconds (longest delay)
```

**With Cancellation**:
```csharp
CancellationTokenSource cts = new();

try
{
    await LoadTextures().WaitForComplete()
        .And(LoadAudio().WaitForComplete())
        .WithCancellation(cts.Token);
}
catch (OperationCanceledException)
{
    Debug.Log("Loading cancelled - both operations stopped");
}

// Cancel from elsewhere (e.g., user clicks "Cancel" button)
cts.Cancel();
```

------------------------------------------------------------------------------------

### **Or (Race Condition)**

**What it does**: Executes multiple transitions **simultaneously** and completes when **any one** finishes.

**When to use**:
- Timeout scenarios (operation vs. delay)
- User input with timeout (wait for click OR 5 seconds)
- Fallback mechanisms (primary source OR backup source)
- "First to respond" scenarios

**How it works**:
- All transitions start **immediately**
- Completes when the **fastest** transition finishes
- Total time = **min(transition1, transition2, ...)**
- Other transitions are **cancelled** automatically

**Basic Example**:
```csharp
// Both delays start, but we only wait for the faster one
await Transition.Delay(5f)
    .Or(Transition.Delay(2f));
// Total time: 2 seconds (fastest wins)
```

**Practical Example - User Input with Timeout**:
```csharp
async UniTask<bool> WaitForUserClickAsync()
{
    bool clicked = false;
    
    // Wait for click OR 10 seconds
    await WaitForClickAsync()
        .Or(Transition.Delay(10f));
    
    return clicked;
}
```

**Practical Example - Network Request with Fallback**:
```csharp
async UniTask<string> FetchDataAsync()
{
    // Try primary server OR backup server (whichever responds first)
    return await FetchFromPrimaryServer().WaitForComplete()
        .Or(FetchFromBackupServer().WaitForComplete());
}
```

**Manual Timeout Pattern**:
```csharp
try
{
    // Wait for operation OR 5 second timeout
    await LongRunningOperation().WaitForComplete()
        .Or(Transition.Delay(5f));
    
    Debug.Log("Operation completed (or timed out)");
}
catch (TimeoutException)
{
    Debug.LogError("Operation took too long!");
}
```

------------------------------------------------------------------------------------

### **Then (Sequential Execution)**

**What it does**: Executes transitions **one after another** in order.

**When to use**:
- Step-by-step animations (fade out, then fade in)
- Sequential loading (load config, then load data)
- Ordered operations (show UI, wait for input, hide UI)
- Any scenario where operations depend on each other

**How it works**:
- First transition completes **fully** before second starts
- Total time = **sum(transition1, transition2, ...)**
- Cancellation stops at current step

**Basic Example**:
```csharp
// First delay completes, THEN second delay starts
await Transition.Delay(1f)
    .Then(Transition.Delay(2f));
// Total time: 3 seconds (1 + 2)
```

**Practical Example - UI Animation Sequence**:
```csharp
async UniTask ShowDialogAsync()
{
    // Step 1: Fade in background
    await background.DOFade(0.8f, 0.3f).WaitForComplete()
    
    // Step 2: Scale in dialog box
    .Then(dialogBox.DOScale(1f, 0.2f).WaitForComplete())
    
    // Step 3: Slide in buttons
    .Then(buttonsPanel.DOLocalMoveY(0, 0.2f).WaitForComplete());
    
    Debug.Log("Dialog fully visible!");
}
```

**Practical Example - Loading Sequence**:
```csharp
async UniTask InitializeGameAsync()
{
    await LoadConfig().WaitForComplete()
        .Then(ConnectToServer().WaitForComplete())
        .Then(LoadPlayerData().WaitForComplete())
        .Then(LoadGameWorld().WaitForComplete());
    
    Debug.Log("Game initialized!");
}
```

**Long Chain Example**:
```csharp
// Create a complex sequence
await ShowLoadingScreen().WaitForComplete()
    .Then(Transition.Delay(0.5f))
    .Then(LoadAssets().WaitForComplete())
    .Then(Transition.Delay(0.5f))
    .Then(InitializeSystems().WaitForComplete())
    .Then(Transition.Delay(0.5f))
    .Then(HideLoadingScreen().WaitForComplete());
```

**With Cancellation**:
```csharp
CancellationTokenSource cts = new();

try
{
    await Step1().WaitForComplete()
        .Then(Step2().WaitForComplete())
        .Then(Step3().WaitForComplete())
        .WithCancellation(cts.Token);
}
catch (OperationCanceledException)
{
    Debug.Log("Sequence cancelled at current step");
}

// Cancel from elsewhere
cts.Cancel();
```

------------------------------------------------------------------------------------

### **WhenAll (Factory Method for Parallel)**

**What it does**: Same as `.And()` but accepts **multiple transitions** as parameters.

**When to use**:
- When you have **many** transitions to run in parallel
- When transitions are in an array or collection
- More readable than chaining multiple `.And()` calls

**How it works**:
- Accepts `params ITransition[]` (variadic arguments)
- All transitions start **immediately**
- Completes when **all** finish
- Total time = **max(all transitions)**

**Basic Example**:
```csharp
await Transition.WhenAll(
    Transition.Delay(1f),
    Transition.Delay(2f),
    Transition.Delay(3f)
);
// Total time: 3 seconds (longest delay)
```

**Practical Example - Dynamic Array**:
```csharp
async UniTask LoadAllLevelsAsync(string[] levelNames)
{
    // Create array of transitions
    ITransition[] loadOperations = levelNames
        .Select(name => LoadLevel(name).WaitForComplete())
        .ToArray();
    
    // Wait for all to complete
    await Transition.WhenAll(loadOperations);
    
    Debug.Log($"Loaded {levelNames.Length} levels!");
}
```

**Practical Example - Mixed Operations**:
```csharp
await Transition.WhenAll(
    // Load textures
    LoadTextures().WaitForComplete(),
    
    // Load audio
    LoadAudio().WaitForComplete(),
    
    // Animate loading bar
    loadingBar.DOFillAmount(1f, 2f).WaitForComplete(),
    
    // Wait minimum time (for branding)
    Transition.Delay(2f)
);
```

**With Cancellation**:
```csharp
CancellationTokenSource cts = new();

try
{
    await Transition.WhenAll(
        Operation1().WaitForComplete(),
        Operation2().WaitForComplete(),
        Operation3().WaitForComplete()
    ).WithCancellation(cts.Token);
}
catch (OperationCanceledException)
{
    Debug.Log("All operations cancelled");
}
```

------------------------------------------------------------------------------------

### **WhenAny (Factory Method for Race)**

**What it does**: Same as `.Or()` but accepts **multiple transitions** as parameters.

**When to use**:
- When you have **many** transitions racing
- When transitions are in an array or collection
- More readable than chaining multiple `.Or()` calls

**How it works**:
- Accepts `params ITransition[]` (variadic arguments)
- All transitions start **immediately**
- Completes when **any one** finishes
- Total time = **min(all transitions)**

**Basic Example**:
```csharp
await Transition.WhenAny(
    Transition.Delay(5f),
    Transition.Delay(2f),
    Transition.Delay(8f)
);
// Total time: 2 seconds (fastest wins)
```

**Practical Example - Multiple Input Sources**:
```csharp
async UniTask<InputType> WaitForAnyInputAsync()
{
    await Transition.WhenAny(
        WaitForKeyboard().WaitForComplete(),
        WaitForMouse().WaitForComplete(),
        WaitForGamepad().WaitForComplete(),
        WaitForTouch().WaitForComplete()
    );
    
    return detectedInputType;
}
```

**Practical Example - Redundant Servers**:
```csharp
async UniTask<string> FetchFromFastestServerAsync()
{
    string[] servers = { "server1.com", "server2.com", "server3.com" };
    
    ITransition[] requests = servers
        .Select(url => FetchData(url).WaitForComplete())
        .ToArray();
    
    // First server to respond wins
    await Transition.WhenAny(requests);
    
    return cachedResult;
}
```

------------------------------------------------------------------------------------

### **WithTimeout (Decorator)**

**What it does**: Adds a **timeout** to any transition. Throws `TimeoutException` if exceeded.

**When to use**:
- Network requests that might hang
- User input that shouldn't wait forever
- Any operation with a maximum acceptable duration
- Preventing infinite waits

**How it works**:
- Wraps the transition in a timeout check
- Uses `.Or()` internally (operation OR timeout delay)
- Throws `TimeoutException` if timeout wins
- Cancels the original operation on timeout

**Basic Example**:
```csharp
try
{
    await LongOperation().WaitForComplete()
        .WithTimeout(5f); // Max 5 seconds
}
catch (TimeoutException)
{
    Debug.LogError("Operation timed out!");
}
```

**Practical Example - Network Request**:
```csharp
async UniTask<string> FetchDataSafelyAsync()
{
    try
    {
        return await FetchDataFromServer()
            .WaitForComplete()
            .WithTimeout(10f); // 10 second timeout
    }
    catch (TimeoutException)
    {
        Debug.LogWarning("Server didn't respond in time");
        return "default_data";
    }
}
```

**Practical Example - User Input**:
```csharp
async UniTask<bool> WaitForConfirmationAsync()
{
    try
    {
        await WaitForButtonClick()
            .WaitForComplete()
            .WithTimeout(30f); // 30 second timeout
        
        return true; // User clicked
    }
    catch (TimeoutException)
    {
        Debug.Log("User didn't respond in time");
        return false; // Timeout
    }
}
```

**Combining with Cancellation**:
```csharp
CancellationTokenSource cts = new();

try
{
    await LoadData()
        .WaitForComplete()
        .WithTimeout(10f)
        .WithCancellation(cts.Token);
}
catch (TimeoutException)
{
    Debug.LogError("Timeout!");
}
catch (OperationCanceledException)
{
    Debug.Log("Cancelled!");
}
```

------------------------------------------------------------------------------------

### **WithCancellation (Decorator)**

**What it does**: Adds **cancellation support** to any transition using a `CancellationToken`.

**When to use**:
- User-cancellable operations (loading screens with "Cancel" button)
- Scene transitions that can be interrupted
- Long-running operations that might need to stop
- Cleanup on application quit

**How it works**:
- Passes `CancellationToken` to the transition
- Throws `OperationCanceledException` when cancelled
- Stops the operation immediately
- Allows cleanup in `catch` block

**Basic Example**:
```csharp
CancellationTokenSource cts = new();

try
{
    await Transition.Delay(10f)
        .WithCancellation(cts.Token);
}
catch (OperationCanceledException)
{
    Debug.Log("Delay was cancelled");
}

// Cancel from elsewhere
cts.Cancel();
```

**Practical Example - Cancellable Loading**:
```csharp
public class LoadingScreen : MonoBehaviour
{
    private CancellationTokenSource loadingCts;
    
    async UniTask LoadGameAsync()
    {
        loadingCts = new CancellationTokenSource();
        
        try
        {
            await LoadAllResources()
                .WaitForComplete()
                .WithCancellation(loadingCts.Token);
            
            Debug.Log("Loading complete!");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Loading cancelled by user");
        }
    }
    
    public void OnCancelButtonClicked()
    {
        loadingCts?.Cancel(); // User clicked "Cancel"
    }
}
```

**Practical Example - Scene Cleanup**:
```csharp
public class GameManager : MonoBehaviour
{
    private CancellationTokenSource gameCts;
    
    void Start()
    {
        gameCts = new CancellationTokenSource();
        RunGameLoopAsync(gameCts.Token).Forget();
    }
    
    async UniTask RunGameLoopAsync(CancellationToken ct)
    {
        try
        {
            while (true)
            {
                await GameTick()
                    .WaitForComplete()
                    .WithCancellation(ct);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Game loop stopped");
        }
    }
    
    void OnDestroy()
    {
        gameCts?.Cancel(); // Stop game loop on destroy
        gameCts?.Dispose();
    }
}
```

**Combining Multiple Cancellation Tokens**:
```csharp
async UniTask LoadWithMultipleCancellationAsync()
{
    // Create linked token source
    CancellationTokenSource userCts = new();
    CancellationTokenSource timeoutCts = new();
    CancellationTokenSource linkedCts = CancellationTokenSource
        .CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);
    
    try
    {
        await LoadData()
            .WaitForComplete()
            .WithCancellation(linkedCts.Token);
    }
    catch (OperationCanceledException)
    {
        if (userCts.Token.IsCancellationRequested)
            Debug.Log("User cancelled");
        else if (timeoutCts.Token.IsCancellationRequested)
            Debug.Log("Timeout");
    }
    finally
    {
        linkedCts.Dispose();
    }
}
```

------------------------------------------------------------------------------------

### **Combining Operators**

You can **chain multiple operators** to create complex workflows:

**Example 1: Parallel then Sequential**
```csharp
// Load resources in parallel, THEN show UI
await Transition.WhenAll(
        LoadTextures().WaitForComplete(),
        LoadAudio().WaitForComplete()
    )
    .Then(ShowMainMenu().WaitForComplete());
```

**Example 2: Sequential with Timeout**
```csharp
// Load step-by-step with timeout
await LoadConfig().WaitForComplete()
    .Then(LoadData().WaitForComplete())
    .WithTimeout(30f);
```

**Example 3: Parallel with Cancellation**
```csharp
// Load in parallel with cancellation
await Transition.WhenAll(
        Operation1().WaitForComplete(),
        Operation2().WaitForComplete()
    )
    .WithCancellation(cts.Token);
```

**Example 4: Complex Chain**
```csharp
// Parallel → Sequential → Timeout → Cancellation
await Transition.WhenAll(
        LoadA().WaitForComplete(),
        LoadB().WaitForComplete()
    )
    .Then(ProcessData().WaitForComplete())
    .Then(SaveResults().WaitForComplete())
    .WithTimeout(60f)
    .WithCancellation(cts.Token);
```

------------------------------------------------------------------------------------

## 🎯 Real-World Examples

### **Loading Screen**
```csharp
async UniTask LoadGameAsync(CancellationToken ct)
{
    // Show loading screen
    await ShowLoadingScreen().WaitForComplete();

    // Load resources in parallel
    await Transition.WhenAll(
        LoadTextures().WaitForComplete(),
        LoadAudio().WaitForComplete(),
        LoadPrefabs().WaitForComplete()
    ).WithTimeout(30f)
     .WithCancellation(ct);

    // Hide loading screen
    await HideLoadingScreen().WaitForComplete();
}
```

### **UI Animation Sequence**
```csharp
async UniTask ShowMenuAsync()
{
    // Fade in background
    await backgroundImage.DOFade(1f, 0.5f).WaitForComplete();

    // Slide in buttons (parallel)
    await Transition.WhenAll(
        playButton.DOLocalMoveX(0, 0.3f).WaitForComplete(),
        settingsButton.DOLocalMoveX(0, 0.3f).WaitForComplete(),
        quitButton.DOLocalMoveX(0, 0.3f).WaitForComplete()
    );

    // Pulse title
    await titleText.DOScale(1.1f, 0.2f)
        .SetLoops(2, LoopType.Yoyo)
        .WaitForComplete();
}
```

### **Network Request with Retry**
```csharp
async UniTask<string> FetchDataWithRetryAsync(CancellationToken ct)
{
    for (int i = 0; i < 3; i++)
    {
        try
        {
            return await FetchDataAsync()
                .WaitForComplete()
                .WithTimeout(5f)
                .WithCancellation(ct);
        }
        catch (TimeoutException)
        {
            Debug.LogWarning($"Attempt {i + 1}/3 timed out");
            if (i < 2)
                await Transition.Delay(2f); // Wait before retry
        }
    }
    throw new Exception("Failed after 3 attempts");
}
```

### **Cancellable Animation**
```csharp
CancellationTokenSource animationCts = new();

async UniTask PlayIdleAnimationAsync()
{
    try
    {
        while (true)
        {
            await character.DOLocalMoveY(0.5f, 1f)
                .SetEase(Ease.InOutSine)
                .WaitForComplete()
                .WithCancellation(animationCts.Token);

            await character.DOLocalMoveY(0f, 1f)
                .SetEase(Ease.InOutSine)
                .WaitForComplete()
                .WithCancellation(animationCts.Token);
        }
    }
    catch (OperationCanceledException)
    {
        // Animation stopped
    }
}

// Stop animation
animationCts.Cancel();
```

### **Parallel Scene Loading**
```csharp
async UniTask LoadMultipleScenesAsync()
{
    ITransition[] sceneLoads = new[]
    {
        LoadSceneAsync("Environment").WaitForComplete(),
        LoadSceneAsync("Characters").WaitForComplete(),
        LoadSceneAsync("UI").WaitForComplete()
    };

    await Transition.WhenAll(sceneLoads)
        .WithTimeout(60f);

    Debug.Log("All scenes loaded!");
}
```
------------------------------------------------------------------------------------

## 🧪 Testing

### **Run Tests**
1. Attach `TransitionTests` component to a GameObject
2. Enter Play Mode
3. Check Console for results

### **Test Coverage**
- ✅ 70+ test cases
- ✅ All transition types
- ✅ All composition operators
- ✅ Cancellation scenarios
- ✅ Timeout scenarios
- ✅ Edge cases
- ✅ Performance benchmarks

------------------------------------------------------------------------------------

## 🔧 Advanced Usage

### **Custom Transitions**
Create your own transition types:

```csharp
public class CustomTransition : TransitionBase
{
    public override async UniTask WaitAsync(CancellationToken cancellationToken)
    {
        // Your custom logic here
        await UniTask.Delay(1000, cancellationToken: cancellationToken);
    }
}
```

### **Custom Extensions**
Add extension methods for your types:

```csharp
public static class MyExtensions
{
    public static ITransition WaitForComplete(this MyAsyncType obj)
    {
        return new MyAsyncTypeTransition(obj);
    }
}
```

------------------------------------------------------------------------------------

## ⚡ Performance

### **Benchmarks**
- Empty transition: **<1ms** overhead
- 1000 empty transitions: **~45ms** total
- WhenAll(100): **O(max)** not O(sum) - truly parallel
- Memory allocation: **Minimal GC pressure**

### **Optimization Tips**
1. Reuse transitions when possible
2. Prefer `WhenAll` over chained `.And()`
3. Avoid deep nesting
4. Use `CancellationToken.None` if no cancellation needed

------------------------------------------------------------------------------------

## 📝 License

This package is licensed under the MIT License.

------------------------------------------------------------------------------------

## 🗺️ Roadmap

### **Planned Features**
- [ ] Progress reporting for long-running transitions
- [ ] Transition groups with shared cancellation
- [ ] Transition pooling for reduced allocations
- [ ] Visual debugging tools
- [ ] More external library integrations (Animator, Timeline, etc.)

### **Completed**
- [x] Core transition system
- [x] UniTask integration
- [x] DOTween integration
- [x] UI Toolkit integration
- [x] Cancellation support
- [x] Timeout support
- [x] Comprehensive test suite
- [x] Documentation

------------------------------------------------------------------------------------

## 🙏 Credits

- **UniTask** by Cysharp - Async/await foundation
- **DOTween** by Demigiant - Tween integration
- **Unity UI Toolkit** - UI transition support

------------------------------------------------------------------------------------