using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Python.Runtime;

namespace DemoPlatform
{
    public static class DllHelper
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);
    }
    public partial class MainWindow : Window
    {
        private const string PythonHome = @"D:\conda_envs\pyQt";

        [System.Runtime.InteropServices.DllImport("HelloWorld.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public static extern IntPtr SayHelloCpp();

        public MainWindow()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                MessageBox.Show($"Unhandled Exception: {e.ExceptionObject}");
            };

            InitializeComponent();
            try
            {
                InitializePython39();
                LoadMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Init Error: {ex}");
            }
        }

        private void LoadMessages()
        {
            // === 调用 C++ ===
            try
            {
                IntPtr ptr = SayHelloCpp();
                string cppMsg = Marshal.PtrToStringAnsi(ptr);
                txtCpp.Text = cppMsg;
            }
            catch (Exception ex)
            {
                txtCpp.Text = "C++ Error: " + ex.Message;
            }

            try
            {
                using (Py.GIL())
                {
                    string pythonFile = @"D:\Project\DemoPlatform\HelloWorld.py"; ;

                    if (!File.Exists(pythonFile))
                    {
                        Dispatcher.Invoke(() => txtPython.Text = $"找不到文件: {pythonFile}");
                        return;
                    }

                    string pythonDir = Path.GetDirectoryName(pythonFile);

                    dynamic sys = Py.Import("sys");
                    sys.path.append(pythonDir);

                    dynamic hello = Py.Import("HelloWorld");
                    string pyMsg = hello.say_hello_python();
                    txtPython.Text = pyMsg;
                }
            }
            catch (Exception ex)
            {
                txtPython.Text = "Python Error: " + ex.Message;
            }
        }

        public static void InitializePython39()
        {
            string envPath = @"D:\conda_envs\CSharpEnv";

            string pythonDll = Path.Combine(envPath, "python312.dll");

            if (!File.Exists(pythonDll))
            {
                throw new FileNotFoundException($"找不到Python DLL: {pythonDll}");
            }

            Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDll);
            Environment.SetEnvironmentVariable("PYTHONHOME", envPath);

            string currentPath = Environment.GetEnvironmentVariable("PATH");
            string newPath = $"{envPath};{envPath}\\Library\\bin;{currentPath}";
            Environment.SetEnvironmentVariable("PATH", newPath);

            string pythonPath = string.Join(";",
                Path.Combine(envPath, "Lib"),
                Path.Combine(envPath, "Lib", "site-packages"),
                Path.Combine(envPath, "DLLs"),
                "."
            );

            Environment.SetEnvironmentVariable("PYTHONPATH", pythonPath);

            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    PythonEngine.Initialize();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Python初始化失败: {ex.Message}");
                throw;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (PythonEngine.IsInitialized)
                PythonEngine.Shutdown();
        }
    }
}