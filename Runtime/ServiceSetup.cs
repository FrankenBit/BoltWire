namespace FrankenBit.BoltWire;

public delegate TServices ServiceSetup<TServices>(TServices services) where TServices : IServiceCollection;