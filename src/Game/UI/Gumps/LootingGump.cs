using System;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps
{
	// Token: 0x0200026E RID: 622
	internal class LootingGump : Gump
	{
		// Token: 0x06000DE1 RID: 3553 RVA: 0x0007917C File Offset: 0x0007737C
		public LootingGump(Item item) : base(0U, 0U)
		{
			CanMove = false;
			AcceptMouseInput = false;
			string text;
			if (item != null)
			{
				text = item.Name;
			}
			else
			{
				text = string.Empty;
			}
            LayerOrder = UILayer.Over;
			if (text == string.Empty)
			{
				// _name = string.Empty;
                _name = "Waiting loot...";
                IsVisible = false;
            }
			else
			{
				if (item.IsCorpse)
				{
					_name = ResGeneral.TryOpen + text;
				}
				else
				{
					_name = ResGeneral.Looting + text;
				}
				IsVisible = true;
			}
			AlphaBlendControl alphaBlendControl = new AlphaBlendControl(0.3f);
			alphaBlendControl.Hue = 34;
			AlphaBlendControl c = alphaBlendControl;
			alpha = alphaBlendControl;
			Add(c, 0);
			Label label = new Label(_name , true, ushort.MaxValue, 0, 1, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT, false);
			label.X = 2;
			_renderedText = label;
			alpha.Width = _renderedText.Width + 5;
			alpha.Height = _renderedText.Height + 5;
			Width = alpha.Width;
			Height = alpha.Height;
			Add(_renderedText, 0);
            // X = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - Width / 2;
            // Y = ProfileManager.CurrentProfile.GameWindowSize.Y / 2 + 20;
			X = 0;
			Y = ProfileManager.CurrentProfile.GameWindowSize.Y + 50;
		}

		// Token: 0x06000DE2 RID: 3554 RVA: 0x00079304 File Offset: 0x00077504
		public void ChangeName(Item item)
		{
			string text;
			if (item != null)
			{
				text = item.Name;
			}
			else
			{
				text = string.Empty;
			}
			if (text == string.Empty)
			{
				_name = string.Empty;
				IsVisible = false;
				return;
			}
			if (item.IsCorpse)
			{
				_name = ResGeneral.TryOpen + text;
			}
			else
			{
				_name = ResGeneral.Looting + text;
			}
			IsVisible = true;
			_renderedText.Text = _name;
			alpha.Width = _renderedText.Width + 5;
			alpha.Height = _renderedText.Height + 5;
			Width = alpha.Width;
			Height = alpha.Height;
			Width = _renderedText.Width;
			Height = _renderedText.Height;
			// X = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - Width / 2;
			// Y = ProfileManager.CurrentProfile.GameWindowSize.Y / 2 + 20;
            X = 0;
            Y = ProfileManager.CurrentProfile.GameWindowSize.Y + 50;
		}

		private string _name;

		private readonly Label _renderedText;

		private readonly AlphaBlendControl alpha;
	}
}
