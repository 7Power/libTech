﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ultraviolet;
using Ultraviolet.Content;
using Ultraviolet.Core;
using Ultraviolet.Core.Text;
using Ultraviolet.Graphics;
using Ultraviolet.Graphics.Graphics2D;
using Ultraviolet.Graphics.Graphics2D.Text;
using Ultraviolet.OpenGL;
using Ultraviolet.Platform;
using Ultraviolet.OpenGL.Bindings;

using libTech.Reflection;
using libTech.Importer;
using System.Reflection;
using CARP;
using System.IO;

using NuklearDotNet;
using System.Runtime.InteropServices;
using Ultraviolet.Input;
using libTech.Renderer;

namespace libTech {
	public class Engine : UltravioletApplication {
		internal RenderDevice RenderDevice;

		public static bool HUE_FRAME = true;

		static void Main(string[] args) {
			Console.Title = "libTech";

			CVar.InitMode = true;
			CVar.Register("game", "basegame", CVarType.Replicated | CVarType.Init, (This, Old, New) => This.Value = Path.GetFullPath((string)New));

			CVar.Register("width", 800, CVarType.Archive);
			CVar.Register("height", 600, CVarType.Archive);
			CVar.Register("borderless", false, CVarType.Archive);
			CVar.Register("resizable", false, CVarType.Archive);

			// Parse all arguments and set CVars
			foreach (var Arg in ArgumentParser.All) {
				CVar CVar = CVar.Find(Arg.Key);

				if (CVar != null)
					CVar.Value = Arg.Value.LastOrDefault();
				else
					CVar.Register(Arg.Key, Arg.Value.LastOrDefault());
			}

			CVar.InitMode = false;
			foreach (var CVar in CVar.GetAll())
				Console.WriteLine(CVar);

			foreach (var Type in Reflect.GetAllTypes(Reflect.GetExeAssembly()))
				if (!Type.IsAbstract && Reflect.Inherits(Type, typeof(Importer.Importer)))
					Importers.Register(Type);

			using (Engine P = new Engine()) {
				P.Run();
			}
		}

		public Engine() : base("Carpmanium", "libTech") {
			RenderDevice = new RenderDevice(this);
		}

		protected override UltravioletContext OnCreatingUltravioletContext() {
			OpenGLUltravioletConfiguration Cfg = new OpenGLUltravioletConfiguration();
			Cfg.MinimumOpenGLVersion = new Version(3, 2);
			Cfg.BackBufferRenderTargetUsage = RenderTargetUsage.PreserveContents;

			Cfg.WindowIsResizable = CVar.GetBool("resizable");
			Cfg.WindowIsBorderless = CVar.GetBool("borderless");

#if DEBUG
			Cfg.Debug = true;
			Cfg.DebugLevels = DebugLevels.Warning | DebugLevels.Error;

			Cfg.DebugCallback = (UV, Lvl, Msg) => Console.WriteLine("{0}: {1}", Lvl, Msg);
#endif

			OpenGLUltravioletContext Ctx = new OpenGLUltravioletContext(this, Cfg);
			Console.WriteLine("{0}", gl.GetString(gl.GL_VERSION));
			return Ctx;
		}

		protected override void OnInitialized() {
			base.OnInitialized();

			IUltravioletPlatform Platform = Ultraviolet.GetPlatform();
			IUltravioletWindow Window = Platform.Windows.GetPrimary();

			if (Window != null) {
				Window.Caption = "libTech";
				Window.ClientSize = new Size2(CVar.GetInt("width", 800), CVar.GetInt("height", 600));
				Window.SetWindowedClientSizeCentered(Window.ClientSize);
			}

			IUltravioletInput Input = Ultraviolet.GetInput();
			MouseDevice Mouse = Input.GetMouse();
			KeyboardDevice Keyboard = Input.GetKeyboard();

			Mouse.Moved += (IUltravioletWindow W, MouseDevice Dev, int X, int Y, int DX, int DY) => {
				RenderDevice.OnMouseMove(X, Y);
			};

			Mouse.WheelScrolled += (W, Dev, X, Y) => {
				RenderDevice.OnScroll(X, Y);
			};

			Mouse.ButtonPressed += (W, Dev, Button) => {
				Point2 Pos = Dev.Position;
				if (Button == MouseButton.Left || Button == MouseButton.Middle || Button == MouseButton.Right)
					RenderDevice.OnMouseButton((NuklearEvent.MouseButton)(Button - 1), Pos.X, Pos.Y, true);
			};

			Mouse.ButtonReleased += (W, Dev, Button) => {
				Point2 Pos = Dev.Position;
				if (Button == MouseButton.Left || Button == MouseButton.Middle || Button == MouseButton.Right)
					RenderDevice.OnMouseButton((NuklearEvent.MouseButton)(Button - 1), Pos.X, Pos.Y, false);
			};

			Keyboard.ButtonPressed += (W, Dev, Button) => {
				if (Button == Scancode.Escape)
					HUE_FRAME = true;
			};

			NuklearAPI.Init(RenderDevice);
		}

		protected override void OnUpdating(UltravioletTime time) {
			base.OnUpdating(time);
		}

		protected override void OnDrawing(UltravioletTime time) {
			base.OnDrawing(time);
			if (HUE_FRAME)
				Console.WriteLine("BEGIN_DRAWING");

			IUltravioletGraphics UVGfx = Ultraviolet.GetGraphics();
			Gfx.UVGfx = UVGfx;
			UVGfx.Clear(new Color(70, 90, 190));

			NuklearAPI.Frame(() => {
				NuklearAPI.Window("Ultraviolet", 100, 100, 200, 200, NkPanelFlags.BorderTitle | NkPanelFlags.MovableScalable | NkPanelFlags.Minimizable, () => {
					NuklearAPI.LayoutRowDynamic(35);

					for (int i = 0; i < 5; i++)
						NuklearAPI.ButtonLabel("Some Button " + i);

					if (NuklearAPI.ButtonLabel("Exit"))
						Environment.Exit(0);
				});
			});

			if (HUE_FRAME)
				Console.WriteLine("END_DRAWING");
			HUE_FRAME = false;
		}
	}
}
