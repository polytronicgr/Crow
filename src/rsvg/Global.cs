// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace Rsvg {

	using System;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public class Global {

		[DllImport(Handle.librsvg)]
		static extern void rsvg_set_default_dpi_x_y(double dpi_x, double dpi_y);

		public static void SetDefaultDpiXY(double dpi_x, double dpi_y) {
			rsvg_set_default_dpi_x_y(dpi_x, dpi_y);
		}

		[DllImport(Handle.librsvg)]
		static extern int rsvg_error_quark();

		public static int ErrorQuark {
			get {
				int raw_ret = rsvg_error_quark();
				int ret = raw_ret;
				return ret;
			}
		}

		[DllImport(Handle.librsvg)]
		static extern void rsvg_set_default_dpi(double dpi);

		public static double DefaultDpi {
			set {
				rsvg_set_default_dpi(value);
			}
		}

#endregion
	}
}
