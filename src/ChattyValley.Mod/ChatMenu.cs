using ChattyValley.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace ChattyValley.Mod;

/// <summary>
/// Open-ended typed chat with a villager: the player types, the local model replies, multi-turn. This
/// is a self-contained mod menu; it reads no game state beyond the context captured when it opened and
/// writes nothing back, so canonical gameplay is untouched. Closing it (Esc) ends the conversation.
///
/// The panel height is flexible: it is recomputed from the wrapped reply each time the text changes, so
/// a long reply is never clipped by the input box.
/// </summary>
public sealed class ChatMenu : IClickableMenu
{
    private readonly NPC _npc;
    private readonly Func<IReadOnlyList<ChatTurn>, Task<string?>> _ask; // build prompt from history + generate
    private readonly List<ChatTurn> _history = new();
    private readonly TextBox _input;

    private string _linusLine = "";
    private bool _thinking;
    private volatile string? _pendingReply;

    // Layout, recomputed by Relayout().
    private const int Width_ = 900;
    private const int Pad = 40;
    private const int PortraitPx = 64 * 3;   // 192
    private const int Gap = 24;
    private const int InputH = 48;
    private const int HintGap = 12;
    private string _wrapped = "";
    private int _textY;
    private int _hintY;

    public ChatMenu(NPC npc, Func<IReadOnlyList<ChatTurn>, Task<string?>> ask)
        : base((Game1.uiViewport.Width - Width_) / 2, 0, Width_, 360)
    {
        _npc = npc;
        _ask = ask;

        var boxTexture = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
        _input = new TextBox(boxTexture, null, Game1.smallFont, Game1.textColor);
        _input.OnEnterPressed += _ => Submit();
        Game1.keyboardDispatcher.Subscriber = _input;
        _input.Selected = true;

        // Kick off Linus's opening line (an implicit greeting), then the player types replies.
        _history.Add(new ChatTurn(true, "Hello, Linus."));
        Generate();
    }

    private void Submit()
    {
        if (_thinking) return;
        string text = _input.Text?.Trim() ?? "";
        if (text.Length == 0) return;
        _input.Text = "";
        _history.Add(new ChatTurn(true, text));
        Generate();
    }

    private void Generate()
    {
        _thinking = true;
        _pendingReply = null;
        Relayout();
        var snapshot = _history.ToArray();  // stable copy for the background task
        _ = Task.Run(async () =>
        {
            string? reply = await _ask(snapshot);
            _pendingReply = string.IsNullOrWhiteSpace(reply) ? "..." : reply!.Trim();
        });
    }

    /// <summary>Recompute panel height + element positions from the current text, and re-center.</summary>
    private void Relayout()
    {
        string body = _thinking ? $"{_npc.displayName} is thinking..." : (_linusLine.Length > 0 ? _linusLine : "...");
        _wrapped = Game1.parseText(body, Game1.dialogueFont, Width_ - 2 * Pad);
        int textH = (int)Game1.dialogueFont.MeasureString(_wrapped).Y;
        int hintH = (int)Game1.smallFont.MeasureString("Xy").Y;

        int headerH = PortraitPx; // portrait (name sits beside it) sets the header height
        height = Pad + headerH + Gap + textH + Gap + InputH + HintGap + hintH + Pad;
        width = Width_;
        xPositionOnScreen = (Game1.uiViewport.Width - width) / 2;
        yPositionOnScreen = System.Math.Max(24, (Game1.uiViewport.Height - height) / 2);

        _textY = yPositionOnScreen + Pad + headerH + Gap;
        _input.X = xPositionOnScreen + Pad;
        _input.Y = _textY + textH + Gap;
        _input.Width = width - 2 * Pad;
        _input.Height = InputH;
        _hintY = _input.Y + InputH + HintGap;
    }

    public override void update(GameTime time)
    {
        base.update(time);
        _input.Update();
        if (_pendingReply is { } reply)
        {
            _pendingReply = null;
            _thinking = false;
            _linusLine = reply;
            _history.Add(new ChatTurn(false, reply));
            Relayout();
        }
    }

    public override void receiveKeyPress(Keys key)
    {
        // Only Escape closes; letter keys are typed into the box (do not call base, or the menu button
        // key, e.g. 'e', would close the menu while typing).
        if (key == Keys.Escape) exitThisMenu();
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        _input.Selected = true;
        Game1.keyboardDispatcher.Subscriber = _input;
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
        drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
            xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 1f);

        int left = xPositionOnScreen + Pad;
        int top = yPositionOnScreen + Pad;

        if (_npc.Portrait != null)
            b.Draw(_npc.Portrait, new Vector2(left, top), new Rectangle(0, 0, 64, 64),
                Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.88f);
        Utility.drawTextWithShadow(b, _npc.displayName, Game1.dialogueFont,
            new Vector2(left + PortraitPx + 24, top + 24), Game1.textColor);

        Utility.drawTextWithShadow(b, _wrapped, Game1.dialogueFont, new Vector2(left, _textY), Game1.textColor);

        _input.Draw(b);
        Utility.drawTextWithShadow(b, "Type and press Enter  ·  Esc to leave", Game1.smallFont,
            new Vector2(left, _hintY), Game1.textColor * 0.7f);

        drawMouse(b);
    }
}
