#define MONO_CAIRO_DEBUG_DISPOSE


using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using System.Diagnostics;

//using GGL;
using go;
using System.Threading;
using System.Collections.Generic;


namespace test
{
	public class ClsItem
	{
//		#region IValueChange implementation
//
//		public event EventHandler<ValueChangeEventArgs> ValueChanged;
//
//		#endregion

		public string field;

		public string Field {
			get {
				return field;
			}
			set {
				field = value;
				//ValueChanged.Raise(this, new ValueChangeEventArgs ("Field", null, field));
			}
		}

		public ClsItem(){
		}
		public ClsItem(string str){
			Field = str;
		}
	}
	
	class GOLIBTest_Listbox : OpenTKGameWindow, IValueChange
	{
		#region IValueChange implementation

		public event EventHandler<ValueChangeEventArgs> ValueChanged;

		#endregion
	
		public GOLIBTest_Listbox ()
			: base(1024, 600,"test")
		{}

//		public List<ClsItem> TestList = new List<ClsItem>(new ClsItem[] 
//			{
//				new ClsItem("string 1"),
//				new ClsItem("string 2"),
//				new ClsItem("string 3")
//			});
		public List<string> TestList = new List<string>(new string[] 
			{
				"string 1",
				"string 2",
//				"string 3",
//				"string 4",
//				"string 5",
//				"string 6",
//				"string 7",
//				"string 8",
				"string 9"
			});		
//		string[] TestList = new string[] 
//			{
//				"string 1",
//				"string 2",
//				"string 3"
//			};	
		ListBox g;

		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			LoadInterface("Interfaces/test_Listbox.goml", out g);

//			TestList [1].Field = "test string";
//			ValueChanged.Raise(this, new ValueChangeEventArgs ("TestList", null, TestList));


		}
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			GL.Clear (ClearBufferMask.ColorBufferBit);
			base.OnRenderFrame (e);
			SwapBuffers ();
		}

		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			base.OnUpdateFrame (e);
		}

		[STAThread]
		static void Main ()
		{
			Console.WriteLine ("starting example");

			using (GOLIBTest_Listbox win = new GOLIBTest_Listbox( )) {
				win.Run (30.0);
			}
		}
	}
}