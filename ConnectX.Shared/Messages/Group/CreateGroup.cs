﻿using ConnectX.Shared.Interfaces;
using Hive.Codec.Shared;
using MemoryPack;

namespace ConnectX.Shared.Messages.Group;

[MessageDefine]
[MemoryPackable]
public partial class CreateGroup : IRequireAssignedUserId
{
    public bool IsPrivate { get; init; }
    public required string RoomName { get; init; }
    public string? RoomDescription { get; init; }
    public string? RoomPassword { get; init; }
    public required int MaxUserCount { get; init; }
    public required Guid UserId { get; init; }
    public required bool UseRelayServer { get; init; }
}