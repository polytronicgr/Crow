﻿//
//  ImlVisualEditor.cs
//
//  Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
//  Copyright (c) 2016 jp
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using Crow;
using System.Threading;
using System.Xml.Serialization;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using Crow.IML;

namespace CrowIDE
{
	public class ImlVisualEditor : GraphicObject
	{
		#region CTOR
		public ImlVisualEditor () : base()
		{
			imlVE = new DesignInterface ();
			Thread t = new Thread (interfaceThread);
			t.IsBackground = true;
			t.Start ();
		}
		#endregion

		DesignInterface imlVE;
		Instantiator itor;
		GraphicObject selectedItem;
		string imlSource;
		ProjectItem projectItem;

		bool drawGrid;
		int gridSpacing;


		[XmlAttributeAttribute][DefaultValue(true)]
		public bool DrawGrid {
			get { return drawGrid; }
			set {
				if (drawGrid == value)
					return;
				drawGrid = value;
				NotifyValueChanged ("DrawGrid", drawGrid);
				RegisterForRedraw ();
			}
		}
		[XmlAttributeAttribute][DefaultValue(10)]
		public int GridSpacing {
			get { return gridSpacing; }
			set {
				if (gridSpacing == value)
					return;
				gridSpacing = value;
				NotifyValueChanged ("GridSpacing", gridSpacing);
				RegisterForRedraw ();
			}
		}
		[XmlAttributeAttribute]public GraphicObject SelectedItem {
			get { return selectedItem; }
			set {
				if (selectedItem == value)
					return;
				selectedItem = value;
				NotifyValueChanged ("SelectedItem", selectedItem);
				RegisterForRedraw ();
			}
		}
		/// <summary>Pointer is over the widget</summary>
		public virtual GraphicObject HoverWidget
		{
			get { return imlVE.HoverWidget; }
			set {
				if (HoverWidget == value)
					return;
				imlVE.HoverWidget = value;
				NotifyValueChanged ("HoverWidget", HoverWidget);
			}
		}
		[XmlIgnore]public List<LQIList> LQIs {
			get { return imlVE.LQIs; }
		}
		[XmlIgnore]public string ImlSource {
			get { return imlSource; }
			set {
				if (imlSource == value)
					return;
				imlSource = value;

				NotifyValueChanged ("ImlSource", ImlSource);

				reloadFromSource ();
			}
		}
		[XmlAttributeAttribute][DefaultValue("")]
		public string ImlPath {
			get { return projectItem?.Path; }
		}


		[XmlAttributeAttribute]
		public ProjectNode ProjectNode {
			get { return projectItem; }
			set {
				if (projectItem == value)
					return;

				if (!(value is ProjectItem))
					return;

				projectItem = value as ProjectItem;

				NotifyValueChanged ("ImlPath", ImlPath);

				reloadFromPath ();
			}
		}
		public List<GraphicObject> GraphicTree {
			get { return imlVE.GraphicTree; }
		}

		void reloadFromSource(){
			if (string.IsNullOrEmpty (imlSource)) {
				reload_iTor (null);
				return;
			}

			Instantiator iTmp;
			try {
				iTmp = Instantiator.CreateFromImlFragment (imlSource);
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine (ex.ToString());
				return;
			}

			reload_iTor (iTmp);
		}
		void reloadFromPath(){
			string path = Path.Combine (projectItem?.Project.RootDir,projectItem?.Path);
			if (!File.Exists (path)){
				System.Diagnostics.Debug.WriteLine ("Path not found: " + path);
				reload_iTor (null);
				return;
			}
			using (StreamReader sr = new StreamReader (path)) {
				ImlSource = sr.ReadToEnd ();
			}
			NotifyValueChanged ("GraphicTree", null);
			NotifyValueChanged ("GraphicTree", GraphicTree);
			SelectedItem = null;
		}
		void reload_iTor(Instantiator new_iTot){
			itor = new_iTot;
			lock (imlVE.UpdateMutex) {
				try {
					imlVE.ClearInterface ();
					if (itor != null)
						imlVE.AddWidget(itor.CreateInstance(imlVE));

				} catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine (ex.ToString());
				}
			}
		}

		void interfaceThread()
		{
			while (true) {
				try {
					imlVE.Update ();
				} catch (Exception ex) {
					System.Diagnostics.Debug.WriteLine (ex.ToString ());
					if (Monitor.IsEntered(imlVE.UpdateMutex))
						Monitor.Exit (imlVE.UpdateMutex);
				}


				bool isDirty = false;

				lock (imlVE.RenderMutex)
					isDirty = imlVE.IsDirty;

				if (isDirty) {
					lock (CurrentInterface.UpdateMutex)
						RegisterForRedraw ();
				}

				Thread.Sleep (2);
			}
		}

		#region GraphicObject overrides
		public override void OnLayoutChanges (LayoutingType layoutType)
		{
			base.OnLayoutChanges (layoutType);
			switch (layoutType) {
			case LayoutingType.Width:
			case LayoutingType.Height:
				imlVE.ProcessResize (this.ClientRectangle.Size);
				break;
			}
		}
		public override void onMouseMove (object sender, MouseMoveEventArgs e)
		{
			//base.onMouseMove (sender, e);
			GraphicObject oldHW = HoverWidget;
			Rectangle scr = this.ScreenCoordinates (this.getSlot ());
			ProcessMouseMove (e.X - scr.X, e.Y - scr.Y);
			if (oldHW == HoverWidget)
				return;
			RegisterForRedraw ();

		}
		public override void onMouseDown (object sender, MouseButtonEventArgs e)
		{
			//base.onMouseDown (sender, e);
			SelectedItem = HoverWidget;
		}
		protected override void onDraw (Cairo.Context gr)
		{
			base.onDraw (gr);
			if (!drawGrid)
				return;


			Rectangle cb = ClientRectangle;
			const double gridLineWidth = 0.1;
			double glhw = gridLineWidth / 2.0;
			int nbLines = cb.Width / gridSpacing ;
			double d = cb.Left + gridSpacing;
			for (int i = 0; i < nbLines; i++) {
				gr.MoveTo (d-glhw, cb.Y);
				gr.LineTo (d-glhw, cb.Bottom);
				d += gridSpacing;
			}
			nbLines = cb.Height / gridSpacing;
			d = cb.Top + gridSpacing;
			for (int i = 0; i < nbLines; i++) {
				gr.MoveTo (cb.X, d - glhw);
				gr.LineTo (cb.Right, d -glhw);
				d += gridSpacing;
			}
			gr.LineWidth = gridLineWidth;
			Foreground.SetAsSource (gr, cb);
			gr.Stroke ();

			lock (imlVE.RenderMutex) {
				using (Cairo.Surface surf = new Cairo.ImageSurface (imlVE.bmp, Cairo.Format.Argb32,
					imlVE.ClientRectangle.Width, imlVE.ClientRectangle.Height, imlVE.ClientRectangle.Width * 4)) {
					gr.SetSourceSurface (surf, cb.Left, cb.Top);
					gr.Paint ();
				}
				imlVE.IsDirty = false;
			}

			Rectangle hr;
			if (HoverWidget != null) {
				hr = HoverWidget.ScreenCoordinates (HoverWidget.getSlot ());
//			gr.SetSourceColor (Color.LightGray);
//			gr.DrawCote (new Cairo.PointD (hr.X, hr.Center.Y), new Cairo.PointD (hr.Right, hr.Center.Y));
//			gr.DrawCote (new Cairo.PointD (hr.Center.X, hr.Y), new Cairo.PointD (hr.Center.X, hr.Bottom));
				//hr.Inflate (2);
				gr.SetSourceColor (Color.LightGray);
				gr.SetDash (new double[]{ 3.0, 3.0 }, 0.0);
				gr.Rectangle (hr, 1.0);
			}

			if (SelectedItem == null)
				return;
			hr = SelectedItem.ScreenCoordinates(SelectedItem.getSlot ());
			hr.Inflate (1);
			gr.SetSourceColor (Color.Yellow);
			gr.SetDash (new double[]{ 5.0, 3.0 },0.0);
			gr.Rectangle (hr, 1.0);
		}
		#endregion

		void WidgetCheckOver (GraphicObject go, MouseMoveEventArgs e){
			Type tGo = go.GetType();
			if (typeof(TemplatedGroup).IsAssignableFrom (tGo)) {
				
			} else if (typeof(TemplatedContainer).IsAssignableFrom (tGo)) {
				TemplatedContainer c = go as TemplatedContainer;
				if (c.Content?.MouseIsIn (e.Position) == true) {					
					WidgetCheckOver (c.Content, e);
					return;
				}
			} else if (typeof(TemplatedControl).IsAssignableFrom (tGo)) {
			} else if (typeof(Group).IsAssignableFrom (tGo)) {
				Group c = go as Group;
				for (int i = c.Children.Count -1; i >= 0; i--) {
					if (c.Children[i].MouseIsIn (e.Position)) {					
						WidgetCheckOver (c.Children[i], e);
						return;
					}
				}
			} else if (typeof(Crow.Container).IsAssignableFrom (tGo)) {
				Crow.Container c = go as Crow.Container;
				if (c.Child?.MouseIsIn (e.Position)==true) {					
					WidgetCheckOver (c.Child, e);
					return;
				}
			}
			HoverWidget = go;
			WidgetMouseEnter (go, e);
		}
		void WidgetMouseLeave (GraphicObject go, MouseMoveEventArgs e){

		}
		void WidgetMouseEnter (GraphicObject go, MouseMoveEventArgs e){

		}
		void WidgetMouseMove (GraphicObject go, MouseMoveEventArgs e){}
		public bool ProcessMouseMove(int x, int y)
		{
			int deltaX = x - imlVE.Mouse.X;
			int deltaY = y - imlVE.Mouse.Y;
			imlVE.Mouse.X = x;
			imlVE.Mouse.Y = y;
			MouseMoveEventArgs e = new MouseMoveEventArgs (x, y, deltaX, deltaY);
			e.Mouse = imlVE.Mouse;

			if (imlVE.ActiveWidget != null) {
				//TODO, ensure object is still in the graphic tree
				//send move evt even if mouse move outside bounds
				WidgetMouseMove (imlVE.ActiveWidget, e);
				return true;
			}

			if (HoverWidget != null) {
				//TODO, ensure object is still in the graphic tree
				//check topmost graphicobject first
				GraphicObject tmp = HoverWidget;
				GraphicObject topc = null;
				while (tmp is GraphicObject) {
					topc = tmp;
					tmp = tmp.LogicalParent as GraphicObject;
				}
				int idxhw = imlVE.GraphicTree.IndexOf (topc);
				if (idxhw != 0) {
					int i = 0;
					while (i < idxhw) {
						if (imlVE.GraphicTree [i].LogicalParent == imlVE.GraphicTree [i].Parent) {
							if (imlVE.GraphicTree [i].MouseIsIn (e.Position)) {
								while (imlVE.HoverWidget != null) {
									WidgetMouseLeave (imlVE.HoverWidget, e);
									imlVE.HoverWidget = imlVE.HoverWidget.LogicalParent as GraphicObject;
								}

								WidgetCheckOver (GraphicTree [i], e);
								return true;
							}
						}
						i++;
					}
				}


				if (imlVE.HoverWidget.MouseIsIn (e.Position)) {
					WidgetCheckOver (imlVE.HoverWidget, (e));
					return true;
				} else {
					WidgetMouseLeave (imlVE.HoverWidget, e);
					//seek upward from last focused graph obj's
					while (imlVE.HoverWidget.LogicalParent as GraphicObject != null) {
						imlVE.HoverWidget = imlVE.HoverWidget.LogicalParent as GraphicObject;
						if (imlVE.HoverWidget.MouseIsIn (e.Position)) {
							WidgetCheckOver (imlVE.HoverWidget, e);
							return true;
						} else
							WidgetMouseLeave (imlVE.HoverWidget, e);
					}
				}
			}

			//top level graphic obj's parsing
			lock (imlVE.GraphicTree) {
				for (int i = 0; i < imlVE.GraphicTree.Count; i++) {
					GraphicObject g = imlVE.GraphicTree [i];
					if (g.MouseIsIn (e.Position)) {
						WidgetCheckOver (g, e);
						return true;
					}
				}
			}
			imlVE.HoverWidget = null;
			return false;

		}

		void GTView_SelectedItemChanged (object sender, SelectionChangeEventArgs e)
		{
			SelectedItem = e.NewValue as GraphicObject;
		}
	}
}
