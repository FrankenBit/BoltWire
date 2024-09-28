using System;
using UnityEngine;

namespace FrankenBit.BoltWire;

public abstract class LifetimeScope : MonoBehaviour, IDisposable
{
    private readonly CompositeDisposable _disposable = new();

    private readonly CompositeStartable _startable = new();
        
    public void Dispose() =>
        _disposable.Dispose();

    protected abstract void Configure(IContainerBuilder builder);
       
    private void Awake()
    {
        var builder = new ContainerBuilder();
        Configure(builder);
            
        Container container = builder.Build();

        _disposable.Add(container);
        _startable.AddRange(container.ResolveAll<IStartable>());
    }

    private void OnDestroy() =>
        Dispose();

    private void Start() =>
        _startable.Start();
}