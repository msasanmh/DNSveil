using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace CypherRSA;

// CypherRSA For Text Files MSasanMH
internal class Program
{
    [STAThread]
    static async Task Main()
    {
        try
        {
            // Commands
            // Generate
            // Encrypt FileToEncrypt PublicKey
            // Decrypt FileToDecrypt PrivateKey

            // Title
            string title = $"Cypher Tool v{Assembly.GetExecutingAssembly().GetName().Version}";
            if (OperatingSystem.IsWindows()) Console.Title = title;

            // Invariant Culture
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            string generate = "generate", encrypt = "encrypt", decrypt = "decrypt";
            bool isGenerate = false, isEncrypt = false, isDecrypt = false;
            string fileToEncrypt = string.Empty, publicKeyPath = string.Empty;
            string fileToDecrypt = string.Empty, privateKeyPath = string.Empty;

            string[] args = Environment.GetCommandLineArgs();
            
            if (args.Length > 0)
            {
                if (args.Length == 2)
                {
                    string arg = args[1].Trim();
                    if (arg.Equals(generate, StringComparison.OrdinalIgnoreCase)) isGenerate = true;
                }

                if (!isGenerate)
                {
                    if (args.Length == 4)
                    {
                        string arg1 = args[1].Trim();
                        string arg2 = args[2].Trim();
                        string arg3 = args[3].Trim();
                        if (arg1.Equals(encrypt, StringComparison.OrdinalIgnoreCase))
                        {
                            isEncrypt = true;
                            fileToEncrypt = Path.GetFullPath(arg2);
                            publicKeyPath = Path.GetFullPath(arg3);
                        }
                        else if (arg1.Equals(decrypt, StringComparison.OrdinalIgnoreCase))
                        {
                            isDecrypt = true;
                            fileToDecrypt = Path.GetFullPath(arg2);
                            privateKeyPath = Path.GetFullPath(arg3);
                        }
                    }
                }
            }

            if (isGenerate) // Generate
            {
                CypherRSA.GenerateKeys(out byte[] publicKey, out byte[] privateKey);
                
                string? pubKeyPath = GetPath("PublicKey.bin");
                if (!string.IsNullOrEmpty(pubKeyPath))
                {
                    await File.WriteAllBytesAsync(pubKeyPath, publicKey);
                    Console.WriteLine($"Public Key Generated:{Environment.NewLine}{pubKeyPath}");
                }

                string? privKeyPath = GetPath("PrivateKey.bin");
                if (!string.IsNullOrEmpty(privKeyPath))
                {
                    await File.WriteAllBytesAsync(privKeyPath, privateKey);
                    Console.WriteLine($"Private Key Generated:{Environment.NewLine}{privKeyPath}");
                }
            }
            else if (isEncrypt) // Encrypt
            {
                if (!File.Exists(fileToEncrypt))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"File Not Exist:{Environment.NewLine}{fileToEncrypt}");
                    Console.ResetColor();
                    Environment.Exit(0);
                }

                if (!File.Exists(publicKeyPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Public Key File Not Exist:{Environment.NewLine}{publicKeyPath}");
                    Console.ResetColor();
                    Environment.Exit(0);
                }

                string contentToEncrypt = await File.ReadAllTextAsync(fileToEncrypt);
                List<string> linesToEncrypt = contentToEncrypt.ReplaceLineEndings().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                if (linesToEncrypt.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"File Is Empty:{Environment.NewLine}{fileToEncrypt}");
                    Console.ResetColor();
                    Environment.Exit(0);
                }

                byte[] publicKeyBytes = await File.ReadAllBytesAsync(publicKeyPath);

                if (publicKeyBytes.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Public Key File Is Empty:{Environment.NewLine}{publicKeyPath}");
                    Console.ResetColor();
                    Environment.Exit(0);
                }

                List<string> encryptedLines = new();
                for (int n = 0; n < linesToEncrypt.Count; n++)
                {
                    string lineToEncrypt = linesToEncrypt[n].Trim();
                    if (string.IsNullOrEmpty(lineToEncrypt)) continue;
                    bool isEncryptionSuccess = CypherRSA.TryEncrypt(lineToEncrypt, publicKeyBytes, out _, out string encryptedLine);
                    if (isEncryptionSuccess)
                    {
                        encryptedLines.Add(encryptedLine);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Failed To Encrypt Line:{Environment.NewLine}{lineToEncrypt}{Environment.NewLine}");
                        Console.WriteLine($"Line Length: {lineToEncrypt.Length}{Environment.NewLine}");
                        Console.ResetColor();
                    }
                }

                string fileNameOut = Path.GetFileNameWithoutExtension(fileToEncrypt);
                string fileExtOut = Path.GetExtension(fileToEncrypt);
                string? fileOut = GetPath($"{fileNameOut}_Encrypted{fileExtOut}");

                if (!string.IsNullOrEmpty(fileOut) && encryptedLines.Count > 0)
                {
                    string encryptedContent = string.Join(Environment.NewLine, encryptedLines);
                    await File.WriteAllTextAsync(fileOut, encryptedContent);
                    Console.WriteLine("Encryption Success.");
                    Environment.Exit(0);
                }
            }
            else if (isDecrypt) // Decrypt
            {
                if (!File.Exists(fileToDecrypt))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"File Not Exist:{Environment.NewLine}{fileToDecrypt}");
                    Console.ResetColor();
                    Environment.Exit(0);
                }

                if (!File.Exists(privateKeyPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Private Key File Not Exist:{Environment.NewLine}{privateKeyPath}");
                    Console.ResetColor();
                    Environment.Exit(0);
                }

                string contentToDecrypt = await File.ReadAllTextAsync(fileToDecrypt);
                List<string> linesToDecrypt = contentToDecrypt.ReplaceLineEndings().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                if (linesToDecrypt.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"File Is Empty:{Environment.NewLine}{fileToDecrypt}");
                    Console.ResetColor();
                    Environment.Exit(0);
                }

                byte[] privateKeyBytes = await File.ReadAllBytesAsync(privateKeyPath);

                if (privateKeyBytes.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Private Key File Is Empty:{Environment.NewLine}{privateKeyPath}");
                    Console.ResetColor();
                    Environment.Exit(0);
                }

                List<string> decryptedLines = new();
                for (int n = 0; n < linesToDecrypt.Count; n++)
                {
                    string lineToDecrypt = linesToDecrypt[n].Trim();
                    if (string.IsNullOrEmpty(lineToDecrypt)) continue;
                    bool isDecryptionSuccess = CypherRSA.TryDecrypt(lineToDecrypt, privateKeyBytes, out string decryptedLine);
                    if (isDecryptionSuccess)
                    {
                        decryptedLines.Add(decryptedLine);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Failed To Decrypt Line:{Environment.NewLine}{lineToDecrypt}{Environment.NewLine}");
                        Console.WriteLine($"Line Length: {lineToDecrypt.Length}{Environment.NewLine}");
                        Console.ResetColor();
                    }
                }

                string fileNameOut = Path.GetFileNameWithoutExtension(fileToDecrypt);
                string fileExtOut = Path.GetExtension(fileToDecrypt);
                string? fileOut = GetPath($"{fileNameOut}_Decrypted{fileExtOut}");

                if (!string.IsNullOrEmpty(fileOut) && decryptedLines.Count > 0)
                {
                    string decryptedContent = string.Join(Environment.NewLine, decryptedLines);
                    await File.WriteAllTextAsync(fileOut, decryptedContent);
                    Console.WriteLine("Decryption Success.");
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wrong Command.");
                Console.ResetColor();
                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
            Environment.Exit(0);
        }
    }

    public static string? GetPath(string fileNameWithExt)
    {
        try
        {
            using Process currentProcess = Process.GetCurrentProcess();
            string? dir = Path.GetDirectoryName(Path.GetFullPath(currentProcess.ProcessName));
            Debug.WriteLine("====== " + dir);
            return !string.IsNullOrEmpty(dir) ? Path.GetFullPath(Path.Combine(dir, fileNameWithExt)) : null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("GetPath: " + ex.Message);
            return null;
        }
    }
}