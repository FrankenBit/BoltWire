using System;
using FrankenBit.BoltWire.Tools;
using UnityEngine;

namespace FrankenBit.BoltWire.Unity3D;

public abstract class LifetimeScope : MonoBehaviour, IDisposable
{
    private readonly CompositeDisposable _disposable = new();

    private readonly CompositeStartable _startable = new();
        
    public void Dispose() =>
        _disposable.Dispose();

    protected abstract void Configure(IServiceCollection services);
       
    private void Awake()
    {
        var services = new ServiceCollection();
        Configure(services);
            
        ServiceProvider provider = services.Build();

        _disposable.Add(provider);
        _startable.AddRange(provider.ResolveAll<IStartable>());
    }

    private void OnDestroy() =>
        Dispose();

    private void Start() =>
        _startable.Start();
}