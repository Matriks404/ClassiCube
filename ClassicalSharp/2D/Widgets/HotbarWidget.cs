﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Drawing;
using ClassicalSharp.GraphicsAPI;
using ClassicalSharp.Gui.Screens;
using OpenTK.Input;
using BlockID = System.UInt16;

namespace ClassicalSharp.Gui.Widgets {
	public class HotbarWidget : Widget {
		
		public HotbarWidget(Game game) : base(game) {
			HorizontalAnchor = Anchor.Centre;
			VerticalAnchor = Anchor.Max;
		}
		
		Texture selTex, backTex;
		protected float barHeight, selBlockSize, elemSize;
		protected float barXOffset, borderSize;
		IsometricBlockDrawer drawer = new IsometricBlockDrawer();
		
		public override void Init() { Reposition(); }
		public override void Dispose() { }
		public override void Render(double delta) {
			RenderHotbarOutline();
			RenderHotbarBlocks();
		}

		public override void Reposition() {
			float scale = game.GuiHotbarScale;
			selBlockSize = (float)Math.Ceiling(24 * scale);
			barHeight = (int)(22 * scale);
			Width = (int)(182 * scale);
			Height = (int)barHeight;
			
			elemSize = 16 * scale;
			barXOffset = 3.1f * scale;
			borderSize = 4 * scale;

			base.Reposition();
			RepositonBackgroundTexture();
			RepositionSelectionTexture();
		}
		
		
		void RenderHotbarOutline() {
			int texId = game.UseClassicGui ? game.Gui.GuiClassicTex : game.Gui.GuiTex;
			backTex.ID = texId;
			backTex.Render(game.Graphics);
			
			int i = game.Inventory.SelectedIndex;
			int x = (int)(X + barXOffset + (elemSize + borderSize) * i + elemSize / 2);
			
			selTex.ID = texId;
			selTex.X1 = (int)(x - selBlockSize / 2);
			game.Graphics.Draw2DTexture(ref selTex, FastColour.White);
		}
		
		void RenderHotbarBlocks() {
			Model.ModelCache cache = game.ModelCache;
			drawer.BeginBatch(game, cache.vertices, cache.vb);
			
			for (int i = 0; i < Inventory.BlocksPerRow; i++) {
				BlockID block = game.Inventory[i];
				int x = (int)(X + barXOffset + (elemSize + borderSize) * i + elemSize / 2);
				int y = (int)(Y + (Height - barHeight / 2));
				
				float scale = (elemSize * 13.5f/16f) / 2f;
				drawer.DrawBatch(block, scale, x, y);
			}
			drawer.EndBatch();
		}
		
		void RepositonBackgroundTexture() {
			TextureRec rec = new TextureRec(0, 0, 182/256f, 22/256f);
			backTex = new Texture(0, X, Y, Width, Height, rec);
		}
		
		void RepositionSelectionTexture() {
			int hSize = (int)selBlockSize;
			
			float scale = game.GuiHotbarScale;
			int vSize = (int)(22 * scale);
			int y = Y + (Height - (int)(23 * scale));
			
			TextureRec rec = new TextureRec(0, 22/256f, 24/256f, 22/256f);
			selTex = new Texture(0, 0, y, hSize, vSize, rec);
		}
		
		bool altHandled;
		public override bool HandlesKeyDown(Key key) {
			if (key >= Key.Number1 && key <= Key.Number9) {
				int index = (int)key - (int)Key.Number1;
				
				if (game.IsKeyDown(KeyBind.HotbarSwitching)) {
					// Pick from first to ninth row
					game.Inventory.Offset = index * Inventory.BlocksPerRow;
					altHandled = true;
				} else {
					game.Inventory.SelectedIndex = index;
				}
				return true;
			}
			return false;
		}
		
		public override bool HandlesKeyUp(Key key) {
			// We need to handle these cases:
			// a) user presses alt then number
			// b) user presses alt
			// thus we only do case b) if case a) did not happen
			if (key != game.Input.Keys[KeyBind.HotbarSwitching]) return false;
			if (altHandled) { altHandled = false; return true; } // handled already
			
			// Don't switch hotbar when alt+tab
			if (!game.window.Focused) return true;
			
			// Alternate between first and second row
			int index = game.Inventory.Offset == 0 ? 1 : 0;
			game.Inventory.Offset = index * Inventory.BlocksPerRow;
			return true;
		}
		
		public override bool HandlesMouseClick(int mouseX, int mouseY, MouseButton button) {
			if (button != MouseButton.Left || !Bounds.Contains(mouseX, mouseY))
				return false;
			InventoryScreen screen = game.Gui.ActiveScreen as InventoryScreen;
			if (screen == null) return false;
			
			for (int i = 0; i < Inventory.BlocksPerRow; i++) {
				int x = (int)(X + (elemSize + borderSize) * i);
				int y = (int)(Y + (Height - barHeight));
				Rectangle bounds = new Rectangle(x, y, (int)(elemSize + borderSize), (int)barHeight);
				
				if (bounds.Contains(mouseX, mouseY)) {
					game.Inventory.SelectedIndex = i;
					return true;
				}
			}
			return false;
		}
		
		float deltaAcc;
		public override bool HandlesMouseScroll(float delta) {
			if (game.Input.IsKeyDown(KeyBind.HotbarSwitching)) {
				int index = game.Inventory.Offset / Inventory.BlocksPerRow;
				game.Inventory.Offset = ScrolledIndex(delta, index, 1) * Inventory.BlocksPerRow;
				altHandled = true;
			} else {
				game.Inventory.SelectedIndex = ScrolledIndex(delta, game.Inventory.SelectedIndex, -1);
			}
			return true;
		}
		
		int ScrolledIndex(float delta, int index, int dir) {
			int steps = Utils.AccumulateWheelDelta(ref deltaAcc, delta);
			const int blocksPerRow = Inventory.BlocksPerRow;
			
			index += (dir * steps) % blocksPerRow;
			if (index < 0) index += blocksPerRow;
			if (index >= blocksPerRow) index -= blocksPerRow;
			return index;
		}
	}
}