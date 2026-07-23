using ChattyValley.Core;

namespace ChattyValley.Tests;

/// <summary>
/// The sliding-window invariant that broke in the 2026-07-23 play-test: history always ends on the
/// player's latest message (odd count at ask time), so slicing an even window off the end opened
/// every post-slide prompt with an orphaned assistant turn (system -> assistant -> user...), a shape
/// the adapter never saw in training. The window must always open on a user turn.
/// </summary>
public class ConversationWindowTests
{
    private static List<ChatTurn> History(int messages)
    {
        // Alternating user/assistant starting on user, like a real chat: u a u a u ...
        var h = new List<ChatTurn>();
        for (int i = 0; i < messages; i++)
            h.Add(new ChatTurn(IsUser: i % 2 == 0, $"m{i}"));
        return h;
    }

    [Fact]
    public void ShortHistoryPassesThroughUntouched()
    {
        var h = History(5);
        var w = ConversationWindow.Apply(h, 12);
        Assert.Equal(5, w.Count);
        Assert.Equal("m0", w[0].Content);
    }

    [Fact]
    public void SlidWindowOpensOnUserTurn()
    {
        // 13 messages, window 12: the naive slice starts on the assistant turn m1. The fixed
        // window must drop that orphan and open on the user turn m2.
        var h = History(13);
        var w = ConversationWindow.Apply(h, 12);
        Assert.True(w[0].IsUser);
        Assert.Equal("m2", w[0].Content);
        Assert.Equal(11, w.Count);
    }

    [Fact]
    public void SlidWindowAlwaysEndsOnLatestUserMessage()
    {
        for (int len = 13; len <= 43; len += 2)
        {
            var h = History(len);
            var w = ConversationWindow.Apply(h, 12);
            Assert.True(w[^1].IsUser);
            Assert.Equal($"m{len - 1}", w[^1].Content);
        }
    }

    [Fact]
    public void SlidWindowAlternatesUserFirstThroughout()
    {
        var h = History(27);
        var w = ConversationWindow.Apply(h, 12);
        for (int i = 0; i < w.Count; i++)
            Assert.Equal(i % 2 == 0, w[i].IsUser);
    }

    [Fact]
    public void TinyMaxStillReturnsAtLeastTheLatestUserMessage()
    {
        var h = History(9);
        var w = ConversationWindow.Apply(h, 2);
        Assert.Single(w);
        Assert.True(w[0].IsUser);
        Assert.Equal("m8", w[0].Content);
    }
}
