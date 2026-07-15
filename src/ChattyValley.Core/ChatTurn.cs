namespace ChattyValley.Core;

/// <summary>One turn of a free-chat conversation: the player's line or the villager's reply.</summary>
public readonly record struct ChatTurn(bool IsUser, string Content);
