﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using ClassicalSharp.Gui.Widgets;

namespace ClassicalSharp.Gui.Screens {
	public class GuiOptionsScreen : ExtMenuOptionsScreen {
		
		public GuiOptionsScreen(Game game) : base(game) {
		}
		
		public override void Init() {
			base.Init();
			ContextRecreated();		
			validators = new MenuInputValidator[widgets.Length];
			defaultValues = new string[widgets.Length];
			
			validators[2]    = new RealValidator(0.25f, 4f);
			defaultValues[2] = "1";
			validators[3]    = new RealValidator(0.25f, 4f);
			defaultValues[3] = "1";
			validators[6]    = new RealValidator(0.25f, 4f);
			defaultValues[6] = "1";
			validators[7]    = new IntegerValidator(0, 30);
			defaultValues[7] = "10";
			validators[9]    = new StringValidator();
			defaultValues[9] = "Arial";
		}
		
		protected override void ContextRecreated() {
			ClickHandler onClick = OnButtonClick;
			ClickHandler onBool = OnBoolClick;
				
			widgets = new Widget[] {
				MakeOpt(-1, -150, "Black text shadows", onBool,  GetShadows,   SetShadows),
				MakeOpt(-1, -100, "Show FPS",           onBool,  GetShowFPS,   SetShowFPS),
				MakeOpt(-1,  -50, "Hotbar scale",       onClick, GetHotbar,    SetHotbar),
				MakeOpt(-1,    0, "Inventory scale",    onClick, GetInventory, SetInventory),
				MakeOpt(-1,   50, "Tab auto-complete",  onBool,  GetTabAuto,   SetTabAuto),

				MakeOpt(1, -150, "Clickable chat",      onBool,  GetClickable, SetClickable),
				MakeOpt(1, -100, "Chat scale",          onClick, GetChatScale, SetChatScale),
				MakeOpt(1,  -50, "Chat lines",          onClick, GetChatlines, SetChatlines),
				MakeOpt(1,    0, "Use system font",     onBool,  GetUseFont,   SetUseFont),
				MakeOpt(1,   50, "Font",                onClick, GetFont,      SetFont),
				
				MakeBack(false, titleFont, SwitchOptions),
				null, null, null,
			};
		}
		
		static string GetShadows(Game g) { return GetBool(g.Drawer2D.BlackTextShadows); }
		void SetShadows(Game g, string v) { 
			g.Drawer2D.BlackTextShadows = SetBool(v, OptionsKey.BlackText);
			HandleFontChange(); 
		}
		
		static string GetShowFPS(Game g) { return GetBool(g.ShowFPS); }
		static void SetShowFPS(Game g, string v) { g.ShowFPS = SetBool(v, OptionsKey.ShowFPS); }
		
		static void SetScale(Game g, string v, ref float target, string optKey) {
			target = Utils.ParseDecimal(v);
			Options.Set(optKey, v);
			g.Gui.RefreshHud();
		}
		
		static string GetHotbar(Game g) { return g.HotbarScale.ToString("F1"); }
		static void SetHotbar(Game g, string v) { SetScale(g, v, ref g.HotbarScale, OptionsKey.HotbarScale); }
		
		static string GetInventory(Game g) { return g.InventoryScale.ToString("F1"); }
		static void SetInventory(Game g, string v) { SetScale(g, v, ref g.InventoryScale, OptionsKey.InventoryScale); }
		
		static string GetTabAuto(Game g) { return GetBool(g.TabAutocomplete); }
		static void SetTabAuto(Game g, string v) { g.TabAutocomplete = SetBool(v, OptionsKey.TabAutocomplete); }
		
		static string GetClickable(Game g) { return GetBool(g.ClickableChat); }
		static void SetClickable(Game g, string v) { g.ClickableChat = SetBool(v, OptionsKey.ClickableChat); }
		
		static string GetChatScale(Game g) { return g.ChatScale.ToString("F1"); }
		static void SetChatScale(Game g, string v) { SetScale(g, v, ref g.ChatScale, OptionsKey.ChatScale); }
		
		static string GetChatlines(Game g) { return g.ChatLines.ToString(); }
		static void SetChatlines(Game g, string v) {
			g.ChatLines = Int32.Parse(v);
			Options.Set(OptionsKey.ChatLines, v);
			g.Gui.RefreshHud();
		}
		
		static string GetUseFont(Game g) { return GetBool(!g.Drawer2D.UseBitmappedChat); }
		void SetUseFont(Game g, string v) { 
			g.Drawer2D.UseBitmappedChat = !SetBool(v, OptionsKey.UseChatFont);
			HandleFontChange(); 
		}
		
		static string GetFont(Game g) { return g.FontName; }
		void SetFont(Game g, string v) {
			g.FontName = v;
			Options.Set(OptionsKey.FontName, v);
			HandleFontChange();
		}
		
		void HandleFontChange() {
			game.Events.RaiseChatFontChanged();
			Recreate();
			game.Gui.RefreshHud();
			selectedI = -1;
			HandlesMouseMove(game.Mouse.X, game.Mouse.Y);
		}
	}
}