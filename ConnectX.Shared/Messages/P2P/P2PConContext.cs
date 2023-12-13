﻿using System.Net;
using ConnectX.Shared.Models;
using MemoryPack;

namespace ConnectX.Shared.Messages.P2P;

public record P2PConContextInit
{
    public bool UseUdp { get; init; }
    public PortDeterminationMode PortDeterminationMode { get; init; } 
    public IPAddress? PublicAddress { get; init; } = IPAddress.Any;
    public ChangeLaws ChangeLaw { get; init; }
    public int Diff { get; init; }
    public bool SameIfNotConflict { get; init; }
    public ushort PublicPort { get; init; }
    public ushort PublicPortLower { get; init; }
    public ushort PublicPortUpper { get; init; }
    public ushort CurrentUsedPort { get; init; }
}

[MemoryPackable]
[MemoryPackUnion(0, typeof(P2PConAccept))]
[MemoryPackUnion(1, typeof(P2PConReady))]
[MemoryPackUnion(2, typeof(P2PConRequest))]
public abstract partial class P2PConContext
{
    [MemoryPackConstructor]
    public P2PConContext()
    {
    }
    
    protected P2PConContext(P2PConContext context)
    {
        UseUdp = context.UseUdp;
        PortDeterminationMode = context.PortDeterminationMode;
        PublicPort = context.PublicPort;
        PublicPortLower = context.PublicPortLower;
        PublicPortUpper = context.PublicPortUpper;
        ChangeLaw = context.ChangeLaw;
        Diff = context.Diff;
        SameIfNotConflict = context.SameIfNotConflict;
        CurrentUsedPort = context.CurrentUsedPort;
        PublicAddress = context.PublicAddress;
    }
    
    protected P2PConContext(P2PConContextInit context)
    {
        UseUdp = context.UseUdp;
        PortDeterminationMode = context.PortDeterminationMode;
        PublicPort = context.PublicPort;
        PublicPortLower = context.PublicPortLower;
        PublicPortUpper = context.PublicPortUpper;
        ChangeLaw = context.ChangeLaw;
        Diff = context.Diff;
        SameIfNotConflict = context.SameIfNotConflict;
        CurrentUsedPort = context.CurrentUsedPort;
        PublicAddress = context.PublicAddress;
    }
    
    public bool UseUdp { get; set; }
    public PortDeterminationMode PortDeterminationMode { get; set; } 
    
    [MemoryPackAllowSerialize]
    public IPAddress? PublicAddress { get; init; } = IPAddress.Any;
    
    public ChangeLaws ChangeLaw { get; init; }
    public int Diff { get; init; }
    public bool SameIfNotConflict { get; init; }
    
    public ushort PublicPort { get; set; }
    public ushort PublicPortLower { get; init; }
    public ushort PublicPortUpper { get; init; }
    public ushort CurrentUsedPort { get; init; }
}