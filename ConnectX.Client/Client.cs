﻿using ConnectX.Client.Interfaces;
using ConnectX.Shared.Interfaces;
using ConnectX.Shared.Messages.Group;
using ConnectX.Shared.Models;
using Hive.Both.General.Dispatchers;
using Microsoft.Extensions.Logging;

namespace ConnectX.Client;

public delegate void GroupStateChangedHandler(GroupUserStates state, UserInfo? userInfo);

public class Client
{
    private bool _isInGroup;
    
    private readonly IDispatcher _dispatcher;
    private readonly IServerLinkHolder _serverLinkHolder;
    private readonly ILogger _logger;
    
    public event GroupStateChangedHandler? OnGroupStateChanged;
    
    public Client(
        IDispatcher dispatcher,
        IServerLinkHolder serverLinkHolder,
        ILogger<Client> logger)
    {
        _dispatcher = dispatcher;
        _serverLinkHolder = serverLinkHolder;
        _logger = logger;
        
        _dispatcher.AddHandler<GroupUserStateChanged>(OnGroupUserStateChanged);
    }
    
    private void OnGroupUserStateChanged(MessageContext<GroupUserStateChanged> ctx)
    {
        if (!_serverLinkHolder.IsConnected) return;
        if (!_serverLinkHolder.IsSignedIn) return;
        if (!_isInGroup) return;
        
        var state = ctx.Message.State;
        var userInfo = ctx.Message.UserInfo;
        
        if (state == GroupUserStates.Dismissed)
            _isInGroup = false;
        
        OnGroupStateChanged?.Invoke(state, userInfo);
    }
    
    private async Task<GroupInfo?> AcquireGroupInfoAsync(Guid groupId)
    {
        if (!_serverLinkHolder.IsConnected) return null;
        if (!_serverLinkHolder.IsSignedIn) return null;
        
        var message = new AcquireGroupInfo
        {
            GroupId = groupId,
            UserId = _serverLinkHolder.UserId
        };
        var groupInfo =
            await _dispatcher.SendAndListenOnce<AcquireGroupInfo, GroupInfo>(
                _serverLinkHolder.ServerSession!,
                message);

        if (groupInfo == null ||
            groupInfo == GroupInfo.Invalid ||
            groupInfo.Users.Length == 0)
        {
            _logger.LogError(
                "[CLIENT] Failed to acquire group info, group id: {groupId}",
                groupId);
            
            return null;
        }

        return groupInfo;
    }

    private async Task<(GroupOpResult?, string?)> PerformGroupOpAsync<T>(T message) where T : IRequireAssignedUserId
    {
        var result =
            await _dispatcher.SendAndListenOnce<T, GroupOpResult>(
                _serverLinkHolder.ServerSession!,
                message);

        if (!(result?.IsSucceeded ?? false))
        {
            _logger.LogError(
                "[CLIENT] Failed to create group, error message: {errorMessage}",
                result?.ErrorMessage ?? "-");
            return (null, result?.ErrorMessage ?? "-");
        }
        
        return (result, null);
    }

    private async Task<(GroupInfo?, string?)> PerformOpAndGetRoomInfoAsync<T>(T message) where T : IRequireAssignedUserId
    {
        var createResult = await PerformGroupOpAsync(message);

        if (createResult.Item1 == null)
            return (null, createResult.Item2);

        var groupInfo = await AcquireGroupInfoAsync(createResult.Item1!.GroupId);

        if (groupInfo == null)
            return (null, "Failed to acquire group info");
        
        _isInGroup = true;
        
        return (groupInfo, null);
    }

    public async Task<(GroupInfo? Info, string? Error)> CreateGroupAsync(CreateGroup createGroup)
    {
        if (!_serverLinkHolder.IsConnected) return (null, "Not connected to the server");
        if (!_serverLinkHolder.IsSignedIn) return (null, "Not signed in");

        return await PerformOpAndGetRoomInfoAsync(createGroup);
    }

    public async Task<(GroupInfo?, string?)> JoinGroupAsync(JoinGroup joinGroup)
    {
        if (!_serverLinkHolder.IsConnected) return (null, "Not connected to the server");
        if (!_serverLinkHolder.IsSignedIn) return (null, "Not signed in");

        return await PerformOpAndGetRoomInfoAsync(joinGroup);
    }
    
    public async Task<(bool, string?)> LeaveGroupAsync(LeaveGroup leaveGroup)
    {
        if (!_serverLinkHolder.IsConnected) return (false, "Not connected to the server");
        if (!_serverLinkHolder.IsSignedIn) return (false, "Not signed in");
        if (!_isInGroup) return (false, "Not in a group");

        var result = await PerformGroupOpAsync(leaveGroup);

        return (result.Item1?.IsSucceeded ?? false, result.Item2);
    }

    public async Task<(bool, string?)> KickUserAsync(KickUser kickUser)
    {
        if (!_serverLinkHolder.IsConnected) return (false, "Not connected to the server");
        if (!_serverLinkHolder.IsSignedIn) return (false, "Not signed in");
        if (!_isInGroup) return (false, "Not in a group");

        var result = await PerformGroupOpAsync(kickUser);

        return (result.Item1?.IsSucceeded ?? false, result.Item2);
    }
}