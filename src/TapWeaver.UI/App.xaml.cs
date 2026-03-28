using System.Windows;
using System.Windows.Threading;
using System.IO;

namespace TapWeaver.UI;

public partial class App : Application
{
	public App()
	{
		DispatcherUnhandledException += OnDispatcherUnhandledException;
		AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
	}

	private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		ReportFatal("UI thread exception", e.Exception);
		e.Handled = true;
		Shutdown(1);
	}

	private void OnCurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is Exception ex)
			ReportFatal("Unhandled app-domain exception", ex);
		else
			ReportFatal("Unhandled app-domain exception", new Exception("Unknown non-exception error object"));
	}

	private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		ReportFatal("Unobserved task exception", e.Exception);
		e.SetObserved();
	}

	private static void ReportFatal(string context, Exception ex)
	{
		try
		{
			var logDir = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"TapWeaver",
				"logs");

			Directory.CreateDirectory(logDir);
			var logPath = Path.Combine(logDir, "crash.log");
			var entry = $"[{DateTime.UtcNow:O}] {context}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}";
			File.AppendAllText(logPath, entry);

			MessageBox.Show(
				"TapWeaver encountered an error and had to close.\n\n" +
				$"A crash log was written to:\n{logPath}\n\n" +
				"Please send this file for debugging.",
				"TapWeaver Error",
				MessageBoxButton.OK,
				MessageBoxImage.Error);
		}
		catch
		{
			// Avoid throwing from exception handler.
		}
	}
}
