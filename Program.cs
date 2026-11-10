using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

internal static class Program
{
    private static readonly string DiscordRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Discord");
    private static readonly string LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BDGuardian", "guardian.log");
    private static string BdCli;

    [STAThread]
    private static void Main()
    {
        try
        {
            BdCli = PrepareBundledCli();
            if (String.IsNullOrEmpty(BdCli)) throw new InvalidOperationException("Extraction du composant BetterDiscord impossible.");
            if (!Directory.Exists(DiscordRoot)) throw new InvalidOperationException("Installation Discord Stable introuvable.");

            string latest = GetLatestVersion();
            if (String.IsNullOrEmpty(latest)) throw new InvalidOperationException("Aucune installation Discord Stable valide trouvée.");

            string installedVersion;
            bool injected;
            if (!TryGetStatus(out installedVersion, out injected) || !injected || installedVersion != latest)
            {
                Log("Installation/réparation demandée pour Discord " + latest + ".");
                string output;
                int code;
                if (!RunBdCli("--silent install --channel stable", out output, out code) || code != 0)
                    throw new InvalidOperationException("BetterDiscord n’a pas pu être installé. Consulte le journal dans %LOCALAPPDATA%\\BDGuardian\\guardian.log.");
                Log("BetterDiscord installé sur Discord Stable.");
            }
            else
            {
                Log("BetterDiscord est déjà présent sur Discord " + latest + ".");
            }

            latest = GetLatestVersion();
            if (String.IsNullOrEmpty(latest)) throw new InvalidOperationException("Discord.exe introuvable après vérification.");
            string exe = Path.Combine(DiscordRoot, "app-" + latest, "Discord.exe");
            Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true });
            Log("Discord lancé (version " + latest + "). BD Guardian se ferme.");
        }
        catch (Exception ex)
        {
            Log("Erreur : " + ex.Message);
            MessageBox.Show(ex.Message, "Discord avec BetterDiscord", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string PrepareBundledCli()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        byte[] payload;
        using (Stream input = assembly.GetManifestResourceStream("Bundled.bdcli.exe"))
        using (MemoryStream memory = new MemoryStream())
        {
            if (input == null) throw new InvalidOperationException("Composant BetterDiscord intégré introuvable.");
            input.CopyTo(memory);
            payload = memory.ToArray();
        }
        string digest;
        using (var sha = System.Security.Cryptography.SHA256.Create())
            digest = BitConverter.ToString(sha.ComputeHash(payload)).Replace("-", "").ToLowerInvariant();
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BDGuardian", "tools", digest);
        Directory.CreateDirectory(folder);
        string target = Path.Combine(folder, "bdcli.exe");
        bool valid = false;
        if (File.Exists(target))
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            using (var input = File.OpenRead(target))
                valid = BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "").ToLowerInvariant() == digest;
        }
        if (!valid)
        {
            string temporary = Path.Combine(folder, Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                File.WriteAllBytes(temporary, payload);
                if (File.Exists(target)) File.Replace(temporary, target, null);
                else File.Move(temporary, target);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
        using (Stream license = assembly.GetManifestResourceStream("Bundled.LICENSE"))
        using (Stream destination = File.Create(Path.Combine(folder, "LICENSE.txt")))
            license.CopyTo(destination);
        return target;
    }
    private static string GetLatestVersion()
    {
        if (!Directory.Exists(DiscordRoot)) return null;
        Version best = null;
        string bestText = null;
        foreach (string directory in Directory.GetDirectories(DiscordRoot, "app-*"))
        {
            string name = Path.GetFileName(directory);
            Version parsed;
            if (name.Length <= 4 || !Version.TryParse(name.Substring(4), out parsed)) continue;
            if (!File.Exists(Path.Combine(directory, "Discord.exe"))) continue;
            if (best == null || parsed > best) { best = parsed; bestText = name.Substring(4); }
        }
        return bestText;
    }

    private static bool TryGetStatus(out string version, out bool injected)
    {
        version = "";
        injected = false;
        string output;
        int code;
        if (!RunBdCli("discover installs", out output, out code) || code != 0) return false;
        Match match = Regex.Match(output, @"(?im)^\s*Discord\s+(?<version>\d+\.\d+\.\d+)\s+\S+\s+(?<status>yes|no|true|false)\s+");
        if (!match.Success) return false;
        version = match.Groups["version"].Value;
        injected = Regex.IsMatch(match.Groups["status"].Value, "^(yes|true)$", RegexOptions.IgnoreCase);
        return true;
    }

    private static bool RunBdCli(string arguments, out string output, out int exitCode)
    {
        output = "";
        exitCode = -1;
        try
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = BdCli;
            info.Arguments = arguments;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.StandardOutputEncoding = Encoding.UTF8;
            info.StandardErrorEncoding = Encoding.UTF8;
            using (Process process = Process.Start(info))
            {
                var stdout = process.StandardOutput.ReadToEndAsync();
                var stderr = process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                exitCode = process.ExitCode;
                output = stdout.Result + Environment.NewLine + stderr.Result;
                return true;
            }
        }
        catch (Exception ex)
        {
            output = ex.Message;
            return false;
        }
    }

    private static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
            File.AppendAllText(LogPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + message + Environment.NewLine, new UTF8Encoding(false));
        }
        catch { }
    }
}

