using System.Diagnostics;

namespace BlackyCSharp;

internal static class Program
{
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main()
	{
		// To customize application configuration such as set high DPI settings or default font,
		// see https://aka.ms/applicationconfiguration.
		
		ApplicationConfiguration.Initialize();
		/*ApplicationContext context = new ApplicationContext(new MainForm());
		context.Tag = */
		Application.Run(new MainForm());
	}
}