#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Linq;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
    internal class GridContainerGump : TextContainerGump
    {
        private const int MAX_WIDTH = 480;
        private const int MAX_HEIGHT = 320;

        private static int _lastX = 200;
        private static int _lastY = 100;
        private readonly AlphaBlendControl _background;
        // private bool _sort;
        // private byte _dr;
        // private static byte _nextdr;
        // private bool _hide;
        // private static bool _nexthide;

        private int _currentPage = 1;
        private int _pagesCount;
        private readonly Label _nameLabel, _amountLabel, _currentPageLabel;
        private readonly NiceButton _buttonPrev, _buttonNext, _buttonSort, _buttonHorizontal, _buttonVertical;
        
        private byte _horizontal;
        private static byte _nextHorizontal = 6;

        private byte _vertical;
        private static byte _nextVertical = 3;

        // private long _corpseEyeTicks;
        private ContainerData _data;
        // private int _eyeCorspeOffset;
        // private GumpPic _eyeGumpPic;
        // private readonly bool _hideIfEmpty;
        // private HitBox _hitBox;
        // private bool _isMinimized;

        private readonly Item _container;
        
        public GridContainerGump(uint local) : base(local, 0)
        {
            _container = World.Items.Get(local);

            if (_container == null)
            {
                Dispose();

                return;
            }

            //GridContainerGump gridContainerGump = UIManager.Gumps.OfType<GridContainerGump>().FirstOrDefault((GridContainerGump s) => s.LocalSerial == base.LocalSerial);
            //if (GridContainerGump == null)
            //{
            //    X = (int)((float)gridContainerGump._lastX + 10f * UIManager.ContainerScale);
            //    Y = (int)((float)gridContainerGump._lastY + 10f * UIManager.ContainerScale);
            //}
            //else
            //{
            //    X = gridContainerGump.X;
            //    Y = gridContainerGump.Y;
            //}
            X = _lastX;
            Y = _lastY;

            CanMove = true;
            AcceptMouseInput = true;
            WantUpdateSize = true;
            CanCloseWithRightClick = true;

            float scale = GetScale();

            _background = new AlphaBlendControl();
            // _background.Width = MAX_WIDTH;
            // _background.Height = MAX_HEIGHT;
            Add(_background);

            Width = _background.Width;
            Height = _background.Height;

            _horizontal = _nextHorizontal;

            _vertical = _nextVertical;

            //Locate initial position
            int width = Client.Game.Window.ClientBounds.Width;
            int height = Client.Game.Window.ClientBounds.Height;

            if (X > width - Width)
            {
                X = 0;
            }
            if (Y > height - Height)
            {
                Y = 0;
            }
            _lastX = X;
            _lastY = Y;

            Add
            (
                _nameLabel = new Label(_container.Name, true, 999, 120, byte.MaxValue, FontStyle.None, TEXT_ALIGN_TYPE.TS_RIGHT, false)
                {
                    X = 100,
                    Y = 5
                }

            );
            Add
            (
                _amountLabel = new Label("", true, 53, 0, 1, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, false)
            );
            _buttonPrev = new NiceButton
                (
                    Width - 70,
                    Height - 20,
                    20,
                    20,
                    ButtonAction.Activate,
                    "<<"
                )
                { ButtonParameter = 0, IsSelectable = false };


            _buttonNext = new NiceButton
                (Width - 20,
                Height - 20,
                20, 20,
                ButtonAction.Activate,
                ">>",
                0,
                TEXT_ALIGN_TYPE.TS_CENTER)
            {
                ButtonParameter = 1,
                IsSelectable = false
            };

            _buttonNext.IsVisible = _buttonPrev.IsVisible = false;


            Add(_buttonPrev);
            Add(_buttonNext);

            //_buttonSort = new NiceButton
            //    (base.Width - 110,
            //    base.Height - 20,
            //    40,
            //    20,
            //    ButtonAction.Activate,
            //    "Sort",
            //    0,
            //    TEXT_ALIGN_TYPE.TS_CENTER)
            //{
            //    ButtonParameter = 2,
            //    IsSelected = _sort
            //};

            //Add(_buttonNext);

            _buttonHorizontal = new NiceButton
                (5,
                0,
                40,
                30,
                ButtonAction.Activate,
                "←→",
                0,
                TEXT_ALIGN_TYPE.TS_CENTER)
            {
                ButtonParameter = 2,
                IsSelectable = false
            };

            Add(_buttonHorizontal);

            _buttonVertical = new NiceButton
                (45,
                0,
                40,
                30,
                ButtonAction.Activate,
                "↑↓",
                0,
                TEXT_ALIGN_TYPE.TS_CENTER)
            {
                ButtonParameter = 3,
                IsSelected = false
            };

            Add(_buttonVertical);

            Add
            (
                _currentPageLabel = new Label("1", true, 999, 0, byte.MaxValue, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER, false)
                {
                    X = Width / 2 - 5,
                    Y = Height - 20
                }
            );
            
        }

        public override void OnButtonClick(int buttonID)
        {
            if (buttonID == 0)
            {
                _currentPage--;
                if (_currentPage <= 1)
                {
                    _currentPage = 1;
                    //_buttonPrev.IsEnabled = false;
                    _buttonPrev.IsVisible = false;
                }
                //_buttonNext.IsEnabled = true;
                _buttonNext.IsVisible = true;
                
                ChangePage(_currentPage);
                _currentPageLabel.Text = ActivePage.ToString();
                _currentPageLabel.X = Width / 2 - _currentPageLabel.Width / 2;
            }
            else if (buttonID == 1)
            {
                _currentPage++;

                if (_currentPage >= _pagesCount)
                {
                    _currentPage = _pagesCount;
                    _buttonNext.IsVisible = false;
                }

                _buttonPrev.IsVisible = true;
                
                ChangePage(_currentPage);

                _currentPageLabel.Text = ActivePage.ToString();
                _currentPageLabel.X = Width / 2 - _currentPageLabel.Width / 2;
                
                
            }
            else if (buttonID == 2)
            {
                if (_horizontal == 5)
                {
                    _horizontal = (_nextHorizontal = 6);
                }
                else if (_horizontal == 6)
                {
                    _horizontal = (_nextHorizontal = 7);
                }
                else if (_horizontal == 7)
                {
                    _horizontal = (_nextHorizontal = 8);
                }
                else if (_horizontal == 8)
                {
                    _horizontal = (_nextHorizontal = 9);
                }
                else
                {
                    _horizontal = (_nextHorizontal = 5);
                }
                UIManager.GetGump<GridContainerGump>(_container.Serial)?.RequestUpdateContents();
            }
            else if (buttonID == 3)
            {
                if (_vertical == 3)
                {
                    _vertical = (_nextVertical = 4);
                }
                else if (_vertical == 4)
                {
                    _vertical = (_nextVertical = 5);
                }
                else if (_vertical == 5)
                {
                    _vertical = (_nextVertical = 6);
                }
                else
                {
                    _vertical = (_nextVertical = 3);
                }
                UIManager.GetGump<GridContainerGump>(_container.Serial)?.RequestUpdateContents();
            }
            else
            {
                base.OnButtonClick(buttonID);
            }
        }

        protected override void UpdateContents()
        {
            const int GRID_ITEM_SIZE = 40;

            float scale = GetScale();
            if (scale < 0.8)
            {
                scale = 0.8f;
            }
            
            int hnum = _horizontal;
            int vnum = _vertical;
            
            _background.Width = (int)(5 * (hnum + 3) + hnum * GRID_ITEM_SIZE * scale);
            _background.Height = (int)(5 * (vnum - 1) + GRID_ITEM_SIZE * vnum * scale +  30 + 20);
            
            Width = _background.Width;
            Height = _background.Height;

            _buttonPrev.X = Width - 70;
            _buttonPrev.Y = Height - 20;
            _buttonNext.X = Width - 20;
            _buttonNext.Y = Height - 20;
            _currentPageLabel.X = Width / 2 - 5;
            _currentPageLabel.Y = Height - 20;
            _amountLabel.X = _nameLabel.X + _nameLabel.Width + 20;
            _amountLabel.Y = 5;

            int count = 0;
            int line = 1;
            
            _pagesCount = 1;
            
            int x = 10;
            int y = 30;
            
            foreach (InGridItem inGridItem in Children.OfType<InGridItem>())
            {
                inGridItem.Dispose();
            }
            //foreach (Item item in list)
            
            for (LinkedObject i = _container.Items; i != null; i = i.Next)
            {
                Item it = (Item) i;
            
                InGridItem gridItem = new InGridItem(it, (int) (GRID_ITEM_SIZE * scale));
            
                if (x >= Width - 20)
                {
                    x = 10;
                    ++line;
            
                    y += gridItem.Height + 5;
            
                    if (y >= Height - 40)
                    {
                        _pagesCount++;
                        
                        y = 30;
                        //line = 1;
                    }
                }
            
                gridItem.X = x;
                gridItem.Y = y;
                
                Add(gridItem, _pagesCount);
            
                x += gridItem.Width + 5;
                
                ++count;
            }

            _amountLabel.Text = "[" + count + "]";

            if (ActivePage <= 1)
            {
                ActivePage = 1;
                _buttonNext.IsVisible = _pagesCount > 1;
                _buttonPrev.IsVisible = false;
            }
            else if (ActivePage >= _pagesCount)
            {
                ActivePage = _pagesCount;
                _buttonNext.IsVisible = false;
                _buttonPrev.IsVisible = _pagesCount > 1;
            }
            else if (ActivePage > 1 && ActivePage < _pagesCount)
            {
                _buttonNext.IsVisible = true;
                _buttonPrev.IsVisible = true;
            }

        }

        public ushort Graphic { get; }

        public override GumpType GumpType => GumpType.Container;

        public bool IsChessboard => Graphic == 0x091A || Graphic == 0x092E;


        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            // foreach (Control control in base.Children)
            // {
            //     if (control is InGridItem && control.MouseIsOver)
            //     {
            //         return;
            //     }
            // }
            // if (button == MouseButtonType.Left)
            // {
            //     GameScene scene = Client.Game.GetScene<GameScene>();
            //     if (!ItemHold.Enabled || !scene.IsMouseOverUI)
            //     {
            //         return;
            //     }
            //     scene.DropHeldItemToContainer(_container, (int)ItemHold.X, (int)ItemHold.Y);
            // }
            if (button != MouseButtonType.Left || UIManager.IsMouseOverWorld)
            {
                return;
            }
            
            Entity it = SelectedObject.Object as Entity;
            uint serial = it != null ? it.Serial : 0;
            uint dropcontainer = LocalSerial;
            
            if (TargetManager.IsTargeting && !ItemHold.Enabled && SerialHelper.IsValid(serial))
            {
                TargetManager.Target(serial);
                Mouse.CancelDoubleClick = true;
            
                if (TargetManager.TargetingState == CursorTarget.SetTargetClientSide)
                {
                    UIManager.Add(new InspectorGump(World.Get(serial)));
                }
            }
            else
            {
                Entity thisCont = World.Items.Get(dropcontainer);
            
                if (thisCont == null)
                {
                    return;
                }
            
                thisCont = World.Get(((Item) thisCont).RootContainer);
            
                if (thisCont == null)
                {
                    return;
                }
            
                bool candrop = thisCont.Distance <= Constants.DRAG_ITEMS_DISTANCE;
            
                if (candrop && SerialHelper.IsValid(serial))
                {
                    candrop = false;
            
                    if (ItemHold.Enabled && !ItemHold.IsFixedPosition)
                    {
                        candrop = true;
            
                        Item target = World.Items.Get(serial);
            
                        if (target != null)
                        {
                            if (target.ItemData.IsContainer)
                            {
                                dropcontainer = target.Serial;
                                x = 0xFFFF;
                                y = 0xFFFF;
                            }
                            else if (target.ItemData.IsStackable && target.Graphic == ItemHold.Graphic)
                            {
                                dropcontainer = target.Serial;
                                x = target.X;
                                y = target.Y;
                            }
                            else
                            {
                                switch (target.Graphic)
                                {
                                    case 0x0EFA:
                                    case 0x2253:
                                    case 0x2252:
                                    case 0x238C:
                                    case 0x23A0:
                                    case 0x2D50:
                                    {
                                        dropcontainer = target.Serial;
                                        x = target.X;
                                        y = target.Y;
            
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            
                if (!candrop && ItemHold.Enabled && !ItemHold.IsFixedPosition)
                {
                    Client.Game.Scene.Audio.PlaySound(0x0051);
                }
            
                if (candrop && ItemHold.Enabled && !ItemHold.IsFixedPosition)
                {
                    ContainerGump gump = UIManager.GetGump<ContainerGump>(dropcontainer);
            
                    if (gump != null && (it == null || it.Serial != dropcontainer && it is Item item && !item.ItemData.IsContainer))
                    {
                        if (gump.IsChessboard)
                        {
                            y += 20;
                        }
            
                        Rectangle bounds = ContainerManager.Get(gump.Graphic).Bounds;
            
                        UOTexture texture = gump.IsChessboard ? GumpsLoader.Instance.GetTexture((ushort) (ItemHold.DisplayedGraphic - Constants.ITEM_GUMP_TEXTURE_OFFSET)) : ArtLoader.Instance.GetTexture(ItemHold.DisplayedGraphic);
            
                        float scale = GetScale();
            
                        bounds.X = (int) (bounds.X * scale);
                        bounds.Y = (int) (bounds.Y * scale);
                        bounds.Width = (int) (bounds.Width * scale);
                        bounds.Height = (int) ((bounds.Height + (gump.IsChessboard ? 20 : 0)) * scale);
            
                        if (texture != null && !texture.IsDisposed)
                        {
                            int textureW, textureH;
            
                            if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ScaleItemsInsideContainers)
                            {
                                textureW = (int) (texture.Width * scale);
                                textureH = (int) (texture.Height * scale);
                            }
                            else
                            {
                                textureW = texture.Width;
                                textureH = texture.Height;
                            }
            
                            if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.RelativeDragAndDropItems)
                            {
                                x += ItemHold.MouseOffset.X;
                                y += ItemHold.MouseOffset.Y;
                            }
            
                            x -= textureW >> 1;
                            y -= textureH >> 1;
            
                            if (x + textureW > bounds.Width)
                            {
                                x = bounds.Width - textureW;
                            }
            
                            if (y + textureH > bounds.Height)
                            {
                                y = bounds.Height - textureH;
                            }
                        }
            
                        if (x < bounds.X)
                        {
                            x = bounds.X;
                        }
            
                        if (y < bounds.Y)
                        {
                            y = bounds.Y;
                        }
            
                        x = (int) (x / scale);
                        y = (int) (y / scale);
                    }
            
                    GameActions.DropItem
                    (
                        ItemHold.Serial,
                        x,
                        y,
                        0,
                        dropcontainer
                    );
            
                    Mouse.CancelDoubleClick = true;
                }
                else if (!ItemHold.Enabled && SerialHelper.IsValid(serial))
                {
                    if (!DelayedObjectClickManager.IsEnabled)
                    {
                        Point off = Mouse.LDragOffset;
            
                        DelayedObjectClickManager.Set(serial, Mouse.Position.X - off.X - ScreenCoordinateX, Mouse.Position.Y - off.Y - ScreenCoordinateY, Time.Ticks + Mouse.MOUSE_DELAY_DOUBLE_CLICK);
                    }
                }
            }
        }

        public override void Update(double totalTime, double frameTime)
        {
            base.Update(totalTime, frameTime);

            if (IsDisposed)
            {
                return;
            }

            if (_background.Width < 100)
            {
                _background.Width = 100;
            }

            if (_background.Height < 100)
            {
                _background.Height = 100;
            }

            Width = _background.Width;
            Height = _background.Height;

            _buttonPrev.X = Width - 80;
            _buttonPrev.Y = Height - 23;
            _buttonNext.X = Width - 40;
            _buttonNext.Y = Height - 20;

            _currentPageLabel.X = Width / 2 - 5;
            _currentPageLabel.Y = Height - 20;

            WantUpdateSize = true;

            Item item = World.Items.Get(LocalSerial);

            if (item == null || item.IsDestroyed)
            {
                Dispose();

                return;
            }

            if ((item.IsCorpse || item.OnGround) && item.Distance > 3)
            {
                Dispose();

                return;
            }

            if (!item.IsDestroyed && UIManager.MouseOverControl != null && (UIManager.MouseOverControl == this || UIManager.MouseOverControl.RootParent == this))
            {
                SelectedObject.Object = item;
                SelectedObject.LastObject = item;
                SelectedObject.CorpseObject = item;
            }
        }



        public override void Restore(XmlElement xml)
        {
            base.Restore(xml);
            // skip loading
        
            Client.Game.GetScene<GameScene>()?.DoubleClickDelayed(LocalSerial);
        
            Dispose();
        }


        private float GetScale()
        {
            return IsChessboard ? 1f : UIManager.ContainerScale;
        }
        
        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            // base.Draw(batcher, x, y);
            //
            // ResetHueVector();
            // base.Draw(batcher, x, y);
            // ResetHueVector();
            // batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Gray), x, y, Width, Height, ref HueVector);
            // return true;
            
            if (!IsVisible || IsDisposed)
            {
                return false;
            }

            ResetHueVector();
            base.Draw(batcher, x, y);
            ResetHueVector();

            batcher.DrawRectangle
            (
                SolidColorTextureCache.GetTexture(Color.Gray),
                x,
                y,
                Width,
                Height,
                ref HueVector
            );

            return true;

            // if (CUOEnviroment.Debug && !IsMinimized)
            // {
            //     Rectangle bounds = _data.Bounds;
            //     float scale = GetScale();
            //     ushort boundX = (ushort) (bounds.X * scale);
            //     ushort boundY = (ushort) (bounds.Y * scale);
            //     ushort boundWidth = (ushort) (bounds.Width * scale);
            //     ushort boundHeight = (ushort) (bounds.Height * scale);
            //
            //     ResetHueVector();
            //
            //     batcher.DrawRectangle
            //     (
            //         SolidColorTextureCache.GetTexture(Color.Red),
            //         x + boundX,
            //         y + boundY,
            //         boundWidth - boundX,
            //         boundHeight - boundY,
            //         ref HueVector
            //     );
            // }
            //
            // return true;
        }


        public override void Dispose()
        {
            // Item item = World.Items.Get(LocalSerial);
            
            ContainerData containerData = default(ContainerData);
            if (_container != null)
            {
                containerData = ContainerManager.Get(_container.Graphic);
            }

            if (_container != null)
            {
                if (World.Player != null && ProfileManager.CurrentProfile?.OverrideContainerLocationSetting == 3)
                {
                    UIManager.SavePosition(_container, Location);
                }

                for (LinkedObject i = _container.Items; i != null; i = i.Next)
                {
                    Item child = (Item) i;

                    if (child.Container == _container)
                    {
                        UIManager.GetGump<GridContainerGump>(child)?.Dispose();
                    }
                }
            }

            base.Dispose();
            // ContainerData containerData = default(ContainerData);
            // if (this._container != null)
            // {
            //     containerData = ContainerManager.Get(this._container.Graphic);
            // }
            // if (this._container != null)
            // {
            //     this._container.Items.Added -= this.Items_Added;
            //     this._container.Items.Removed -= this.Items_Removed;
            //     foreach (Item item in this._container.Items)
            //     {
            //         if (item.Container == this._container)
            //         {
            //             AdvanceContainerGump gump = UIManager.GetGump<AdvanceContainerGump>(new uint?(item));
            //             if (gump != null)
            //             {
            //                 gump.Dispose();
            //             }
            //         }
            //     }
            //     if (containerData.ClosedSound != 0)
            //     {
            //         Client.Game.Scene.Audio.PlaySound((int)containerData.ClosedSound, AudioEffects.None);
            //     }
            // }
            // base.Dispose();
        }

        protected override void CloseWithRightClick()
        {
            base.CloseWithRightClick();

            if (_data.ClosedSound != 0)
            {
                Client.Game.Scene.Audio.PlaySound(_data.ClosedSound);
            }
        }

        protected override void OnDragEnd(int x, int y)
        {
            // if (ProfileManager.CurrentProfile.OverrideContainerLocation && ProfileManager.CurrentProfile.OverrideContainerLocationSetting >= 2)
            // {
            //     Point gumpCenter = new Point(X + (Width >> 1), Y + (Height >> 1));
            //     ProfileManager.CurrentProfile.OverrideContainerLocationPosition = gumpCenter;
            // }
            //
            // base.OnDragEnd(x, y);
            
            if (X + Width > Client.Game.Window.ClientBounds.Width)
            {
                X = Client.Game.Window.ClientBounds.Width - Width;
            }
            if (Y + Height > Client.Game.Window.ClientBounds.Height)
            {
                Y = Client.Game.Window.ClientBounds.Height - Height;
            }
            _lastX = X;
            _lastY = Y;
            base.OnDragEnd(x, y);
        }

        private class InGridItem : ItemGump
        {
            //public ushort Hue { get; set; }
            public bool IsPartial { get; set; }

            public InGridItem(Item item, int size) : base(item.Serial, item.Graphic, item.Hue, item.X, item.Y)
            {
                if (item == null)
                {
                    Dispose();
                    return;
                }
                HighlightOnMouseOver = true;
                string text = (item.Amount > 1) ? StringHelper.IntToAbbreviatedString((int)item.Amount) : "";
                _name = new Label(text, true, 53, 0, 1, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT, false);
                _weight = new Label(item.ItemData.Weight.ToString(), true, 999, 0, 1, FontStyle.None, TEXT_ALIGN_TYPE.TS_RIGHT, false);
                Add(_name, 0);
                Add(_weight, 0);
                _background = new AlphaBlendControl(0.5f);
                //Hue = item.Hue;
                Add(_background, 0);
                Control background = _background;
                Width = size;
                background.Width = size;
                Control background2 = _background;
                Height = size;
                background2.Height = size;
                IsPartial = item.ItemData.IsPartialHue;
            }

            // Token: 0x06000E3E RID: 3646 RVA: 0x0007CACC File Offset: 0x0007ACCC
            public override bool Contains(int x, int y)
            {
                if (_background.Contains(x, y))
                {
                    return true;
                }
                Item item = World.Items.Get(LocalSerial);
                if (item == null || item.IsDestroyed)
                {
                    this.Dispose();
                }
                return !base.IsDisposed && (!item.IsCoin && item.Amount > 1 && item.ItemData.IsStackable && ArtLoader.Instance.GetTexture(Graphic).Contains(x - 5, y - 5, true));
            }

            // Token: 0x06000E3F RID: 3647 RVA: 0x0007CB5C File Offset: 0x0007AD5C
            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                base.OnMouseUp(x, y, button);
                if (button == MouseButtonType.Left && Keyboard.Shift)
                {
                    //Item item = World.Items.Get(LocalSerial);
                    //GameActions.PickUp(item, new int?((int)item.Amount), null);
                    UOTexture texture = ArtLoader.Instance.GetTexture(Graphic);

                    Rectangle bounds = texture.Bounds;
                    int centerX = bounds.Width >> 1;
                    int centerY = bounds.Height >> 1;

                    if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ScaleItemsInsideContainers)
                    {
                        float scale = UIManager.ContainerScale;
                        centerX = (int)(centerX * scale);
                        centerY = (int)(centerY * scale);
                    }

                    if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.RelativeDragAndDropItems)
                    {
                        Point p = new Point(centerX - (Mouse.Position.X - ScreenCoordinateX), centerY - (Mouse.Position.Y - ScreenCoordinateY));

                        GameActions.PickUp
                        (
                            LocalSerial,
                            centerX,
                            centerY,
                            offset: p
                        );
                    }
                    else
                    {
                        GameActions.PickUp(LocalSerial, centerX, centerY);
                    }
                }
            }

            // Token: 0x06000E40 RID: 3648 RVA: 0x0007CBA8 File Offset: 0x0007ADA8
            public override bool Draw(UltimaBatcher2D batcher, int x, int y)
            {
                ResetHueVector();
                int num = x;
                int num2 = y;
                batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Gray), x, y, _background.Width, _background.Height, ref HueVector);
                if (MouseIsOver)
                {
                    ResetHueVector();
                    HueVector.Z = 0.7f;
                    batcher.Draw2D(SolidColorTextureCache.GetTexture(Color.Yellow), x + 1, y + 1, (float)_background.Width, (float)_background.Height, ref HueVector);
                    HueVector.Z = 0f;
                }
                ResetHueVector();
                ShaderHueTranslator.GetHueVector(ref HueVector, Hue, IsPartial, Alpha, false);

                ArtTexture artTexture = ArtLoader.Instance.GetTexture(Graphic) as ArtTexture;
                //ArtTexture artTexture = Texture as ArtTexture;
                if (artTexture != null)
                {
                    int width = base.Width;
                    int height = base.Height;
                    Rectangle imageRectangle = artTexture.ImageRectangle;
                    if (imageRectangle.Width < base.Width)
                    {
                        width = imageRectangle.Width;
                        x += (base.Width >> 1) - (width >> 1);
                    }
                    if (imageRectangle.Height < base.Height)
                    {
                        height = imageRectangle.Height;
                        y += (base.Height >> 1) - (height >> 1);
                    }
                    batcher.Draw2D(ArtLoader.Instance.GetTexture(Graphic), x, y, width, height, imageRectangle.X, imageRectangle.Y, imageRectangle.Width, imageRectangle.Height, ref HueVector, 0f);
                }
                else
                {
                    batcher.Draw2D(ArtLoader.Instance.GetTexture(Graphic), x, y, Width, Height, 0, 0, ArtLoader.Instance.GetTexture(Graphic).Width, ArtLoader.Instance.GetTexture(Graphic).Height, ref HueVector, 0f);
                }
                ResetHueVector();
                _name.Draw(batcher, num, num2 + _background.Height - _name.Height);

                Item item = World.Items.Get(LocalSerial);
                if (item.ItemData.Weight >= 5)
                {
                    _weight.Draw(batcher, num + _background.Width - _weight.Width, num2);
                }
                return true;
            }

            private Label _name;

            private Label _weight;

            private AlphaBlendControl _background;

            private readonly TextureControl _texture;
        }
    }
}