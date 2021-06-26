using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
	internal class LootListGump : Gump
	{
		public LootListGump() : base(0U, 0U)
		{
			if (ProfileManager.CurrentProfile.LootList == null)
			{
				ProfileManager.CurrentProfile.LootList = new List<ushort[]>();
			}
			_items = ProfileManager.CurrentProfile.LootList;
			X = _lastX;
			Y = _lastY;
			CanMove = true;
			AcceptMouseInput = true;
			_background = new AlphaBlendControl(0.5f);
			_background.Width = 120 * ProfileManager.CurrentProfile.LootListScale;
			_background.Height = 160 * ProfileManager.CurrentProfile.LootListRowsNum;
			Add(_background, 0);
			Width = _background.Width;
			Height = _background.Height;
			NiceButton c = new NiceButton(3, base.Height - 23, 50 * ProfileManager.CurrentProfile.LootListScale, 10 * ProfileManager.CurrentProfile.LootListScale, ButtonAction.Activate, ResGumps.AddItem, 0, TEXT_ALIGN_TYPE.TS_CENTER)
			{
				ButtonParameter = 2,
				IsSelectable = false
			};
			Add(c, 0);
			_buttonPrev = new NiceButton(base.Width - 50, base.Height - 20, 20, 20, ButtonAction.Activate, "<<", 0, TEXT_ALIGN_TYPE.TS_CENTER)
			{
				ButtonParameter = 0,
				IsSelectable = false
			};
			_buttonNext = new NiceButton(base.Width - 20, base.Height - 20, 20, 20, ButtonAction.Activate, ">>", 0, TEXT_ALIGN_TYPE.TS_CENTER)
			{
				ButtonParameter = 1,
				IsSelectable = false
			};
			_buttonNext.IsEnabled = (_buttonPrev.IsEnabled = false);
			_buttonNext.IsVisible = (_buttonPrev.IsVisible = false);
			Add(_buttonPrev, 0);
			Add(_buttonNext, 0);
			if (_items != null)
			{
				RedrawItems();
			}
		}
		public override void OnButtonClick(int buttonID)
		{
			if (buttonID == 0)
			{
				_currentPage--;
				if (_currentPage <= 1)
				{
					_currentPage = 1;
					_buttonPrev.IsEnabled = false;
					_buttonNext.IsEnabled = true;
					_buttonPrev.IsVisible = false;
					_buttonNext.IsVisible = true;
				}
				ChangePage(_currentPage);
			}
			else if (buttonID == 1)
			{
				_currentPage++;
				if (_currentPage >= _pagesCount)
				{
					_currentPage = _pagesCount;
					_buttonPrev.IsEnabled = true;
					_buttonNext.IsEnabled = false;
					_buttonPrev.IsVisible = true;
					_buttonNext.IsVisible = false;
				}
				ChangePage(_currentPage);
			}
			else if (buttonID == 2)
			{
				GameActions.Print(ResGumps.AddItem, 946, MessageType.Regular, 3, true);
				TargetManager.SetTargeting(CursorTarget.AddToLootlist, 0U, TargetType.Neutral);
			}
            else
            {
                base.OnButtonClick(buttonID);
            }
		}

		public void RedrawItems()
		{
			int num = 10 * ProfileManager.CurrentProfile.LootListScale;
			int size = (Width - 10 * ProfileManager.CurrentProfile.LootListScale) / ProfileManager.CurrentProfile.LootListRowsNum - 10 * ProfileManager.CurrentProfile.LootListScale;
			int num2 = num;
			int num3 = num;
			foreach (LootListItem lootListItem in Children.OfType<LootListItem>())
			{
				lootListItem.Dispose();
			}
			int num4 = 0;
			_pagesCount = 1;
			foreach (ushort[] item in _items)
			{
				LootListItem lootListItem2 = new LootListItem(item, size);
				if (num2 > _background.Width - lootListItem2.Width - num)
				{
					num2 = num;
					num3 += lootListItem2.Height + num;
					if (num3 > _background.Height - lootListItem2.Width - 2 * num - 20)
					{
						_pagesCount++;
						num3 = num;
						_buttonNext.IsEnabled = true;
						_buttonNext.IsVisible = true;
					}
				}
				lootListItem2.X = num2;
				lootListItem2.Y = num3;
				Add(lootListItem2, _pagesCount);
				num2 += lootListItem2.Width + num;
				num4++;
			}
		}

		public override void Dispose()
		{
			_lastX = X;
			_lastY = Y;
			base.Dispose();
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			ResetHueVector();
			base.Draw(batcher, x, y);
			batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Gray), x, y, base.Width, base.Height, ref HueVector);
			return true;
		}

		private readonly AlphaBlendControl _background;

		private readonly NiceButton _buttonPrev;

		private readonly NiceButton _buttonNext;

		private List<ushort[]> _items;

		private ushort _item;

		private static int _lastX = 100;

		private static int _lastY = 100;

		private int _currentPage = 1;

		private int _pagesCount;

		private class LootListItem : Control
		{
			public LootListItem(ushort[] item, int size) : base(null)
			{
				if (item == null)
				{
					Dispose();
					return;
				}
				CanMove = false;
				AlphaBlendControl alphaBlendControl = new AlphaBlendControl(0.5f);
				alphaBlendControl.Y = 15;
				alphaBlendControl.Width = size;
				alphaBlendControl.Height = size;
				Add(alphaBlendControl, 0);
				TextureControl textureControl = new TextureControl();
				textureControl.IsPartial = false;
				textureControl.ScaleTexture = true;
				textureControl.Hue = item[1];
				textureControl.Texture = ArtLoader.Instance.GetTexture((uint)item[0]);
				textureControl.Y = 15;
				textureControl.Width = size;
				textureControl.Height = size;
				textureControl.CanMove = false;
				_texture = textureControl;
				Add(_texture, 0);
				_texture.MouseUp += delegate(object sender, MouseEventArgs e)
				{
					if (e.Button == MouseButtonType.Left)
					{
						ProfileManager.CurrentProfile.LootList.Remove(item);
						LootListGump gump = UIManager.GetGump<LootListGump>(null);
						if (gump != null)
						{
							gump.Dispose();
						}
						UIManager.Add(new LootListGump());
					}
				};
				Width = alphaBlendControl.Width;
				Height = alphaBlendControl.Height + 15;
				WantUpdateSize = false;
			}

			public override bool Draw(UltimaBatcher2D batcher, int x, int y)
			{
				ResetHueVector();
				base.Draw(batcher, x, y);
				batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Gray), x, y + 15, base.Width, base.Height - 15, ref HueVector);
				if (_texture.MouseIsOver)
				{
					HueVector.Z = 0.7f;
					batcher.Draw2D(SolidColorTextureCache.GetTexture(Color.Yellow), x + 1, y + 15, (float)(base.Width - 1), (float)(base.Height - 15), ref HueVector);
					HueVector.Z = 0f;
				}
				return true;
			}

			private readonly TextureControl _texture;
		}
	}
}
