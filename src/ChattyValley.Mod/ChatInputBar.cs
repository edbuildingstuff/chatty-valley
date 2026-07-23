using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace ChattyValley.Mod;

/// <summary>
/// The typing half of a free-chat conversation: a slim vanilla-styled bar at the bottom of the
/// screen where the player writes their next line. The villager's replies are NOT shown here;
/// they appear in the game's own <see cref="DialogueBox"/> (see <see cref="ChatSession"/>), which
/// is the point: everything the player reads looks exactly like vanilla dialogue. While the model
/// generates, the bar shows a small thinking indicator instead of the input.
/// </summary>
public sealed class ChatInputBar : IClickableMenu
{
    private readonly ChatSession _session;
    private readonly NPC _npc;
    private readonly TextBox _input;
    private int _ticks;

    private const int Width_ = 900;
    private const int Pad = 28;
    private const int InputH = 48;
    private const int HintGap = 10;

    public ChatInputBar(ChatSession session, NPC npc)
        : base(0, 0, Width_, 0)
    {
        _session = session;
        _npc = npc;

        var boxTexture = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
        _input = new TextBox(boxTexture, null, Game1.smallFont, Game1.textColor);
        _input.OnEnterPressed += _ => Submit();
        Game1.keyboardDispatcher.Subscriber = _input;
        _input.Selected = true;
        Relayout();
    }

    private void Submit()
    {
        if (_session.Thinking) return;
        string text = _input.Text?.Trim() ?? "";
        if (text.Length == 0) return;
        _input.Text = "";
        _session.Submit(text);
    }

    private void Relayout()
    {
        int hintH = (int)Game1.smallFont.MeasureString("Xy").Y;
        width = Width_;
        height = Pad + InputH + HintGap + hintH + Pad;
        xPositionOnScreen = (Game1.uiViewport.Width - width) / 2;
        yPositionOnScreen = Game1.uiViewport.Height - height - 96;   // above the toolbar

        _input.X = xPositionOnScreen + Pad;
        _input.Y = yPositionOnScreen + Pad;
        _input.Width = width - 2 * Pad;
        _input.Height = InputH;
    }

    public override void update(GameTime time)
    {
        base.update(time);
        _ticks++;
        _input.Update();
        _session.PumpPendingReply();   // swaps this bar for the vanilla DialogueBox when ready
    }

    public override void receiveKeyPress(Keys key)
    {
        // Only Escape closes (ends the conversation); letters go to the text box. Do not call
        // base, or the menu key ('e') would close the bar mid-word.
        if (key != Keys.Escape) return;
        _session.Abort();
        exitThisMenu();
    }

    protected override void cleanupBeforeExit()
    {
        base.cleanupBeforeExit();
        // Release the keyboard; the session decides separately whether the conversation is over
        // (Esc -> Abort above) or just moving to the reply box (no Abort).
        if (Game1.keyboardDispatcher.Subscriber == _input)
            Game1.keyboardDispatcher.Subscriber = null;
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        _input.Selected = true;
        Game1.keyboardDispatcher.Subscriber = _input;
    }

    public override void draw(SpriteBatch b)
    {
        Relayout();   // track window resizes; cheap
        drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60),
            xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 1f);

        int left = xPositionOnScreen + Pad;
        int hintY = _input.Y + InputH + HintGap;

        if (_session.Thinking)
        {
            string dots = new string('.', 1 + _ticks / 30 % 3);
            Utility.drawTextWithShadow(b, $"{_npc.displayName} is thinking{dots}", Game1.smallFont,
                new Vector2(left, _input.Y + 10), Game1.textColor * 0.8f);
        }
        else
        {
            _input.Draw(b);
        }

        Utility.drawTextWithShadow(b,
            $"Talking with {_npc.displayName}  ·  Enter to send  ·  Esc to leave",
            Game1.smallFont, new Vector2(left, hintY), Game1.textColor * 0.6f);

        drawMouse(b);
    }
}
