# Framework Overview

## Summary
This framework provides modular building blocks for orchestrating asynchronous gameplay logic, UI flows, and data access in Unity projects using UniTask.

## Composition
- **Addressables**: Helpers for loading labelled Addressable assets with UniTask, progress reporting, automatic handle release, and cancellation safeguards.
- **Observables**: Lightweight observable properties, subjects, and disposables for reactive state updates without external dependencies.
- **Services**: Interfaces, abstract bases, and a manager for lifecycle-controlled singleton services that can be lazily resolved across gameplay systems.
- **StateMachine**: Async-aware state machine with fluent configuration, auto-transitions, rollback handling, and observable current-state exposure.
- [**Transitions**](https://github.com/k-lock/Framework/blob/master/Transitions/README.md): Awaitable transition primitives that sequence or combine effects, with extension methods for visual elements, tweens, and UniTask integration.
- **UI**: Presenter-driven UI layer with events, models, and managers that decouple view logic from domain services.
- **Utils**: Shared utilities such as async locks that support the other modules.

## Example Usage
1. Register a presenter service and load assets.
2. Configure a state machine and transitions.
3. React to observable state from the UI layer.

```csharp
using Cysharp.Threading.Tasks;
using Framework.Addressables;
using Framework.Observable;
using Framework.Services;
using Framework.StateMachine;
using Framework.Transitions.Implementations;
using Framework.UI.Presenter;
using UnityEngine;

public enum FlowState { Loading, Ready, Error }

public sealed class GameBootstrap : MonoBehaviour, IRequiresPresenterService
{
    [SerializeField] private string addressableLabel;
    private IStateMachine<FlowState> stateMachine;
    private readonly ObservableProperty<string> status = new();

    public IPresenterService PresenterService { get; set; }

    private async void Start()
    {
        Services.Register<IPresenterService>(PresenterService);

        var assets = await AddressableLoader.LoadAssetsByLabelAsync<GameObject>(addressableLabel);
        if (!assets.Success)
        {
            status.Value = assets.Error;
            return;
        }

        stateMachine = new StateMachine<FlowState>(
            StateConfigBuilder.CreateFluentForEnum<FlowState>()
                .For(FlowState.Loading)
                    .AllowTransitionsTo(FlowState.Ready, FlowState.Error)
                    .Async(() => UniTask.Delay(1000))
                    .Done()
                .For(FlowState.Ready)
                    .Done()
                .For(FlowState.Error)
                    .Done()
                .Build(),
            FlowState.Loading);

        status.Value = "Loading";
        await new SequentialTransition(
            new DelayTransition(500),
            new UniTaskTransition(stateMachine.TransitionToAsync(FlowState.Ready))
        ).WaitAsync();
        status.Value = "Ready";
    }
}
```
