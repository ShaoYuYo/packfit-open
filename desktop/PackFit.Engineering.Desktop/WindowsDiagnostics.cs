using System.Diagnostics;
using System.Text;

namespace PackFit.Engineering.Desktop;

/// <summary>Allowlisted, local Windows diagnostics: no user-supplied commands or silent privilege elevation.</summary>
public static class WindowsDiagnostics
{
    public sealed record Command(string Id,string Label,string Description,string[] Args,int TimeoutSeconds);
    public static IReadOnlyList<Command> BuiltIn {get;}=[
        new("sleep","可用睡眠狀態","確認 S3/Modern Standby。",["/a"],20),
        new("requests","電力要求","找阻止睡眠的程式與驅動。",["/requests"],20),
        new("wake","上次喚醒來源","查看最近喚醒原因。",["/lastwake"],20),
        new("timers","喚醒計時器","找定時喚醒工作。",["/waketimers"],20),
        new("armed","可喚醒裝置","列出允許喚醒系統的硬體。",["/devicequery","wake_armed"],20)
    ];
    public static async Task<string> ExecuteAsync(string id,CancellationToken cancellation=default)
    {
        var cmd=BuiltIn.FirstOrDefault(c=>c.Id==id) ??
            throw new ArgumentException("Unsupported diagnostic request.");
        return await RunAsync(cmd.Args,cmd.TimeoutSeconds,cancellation);
    }
    public static async Task<string> CreateHtmlReportAsync(string kind,string output,
        CancellationToken cancellation=default)
    {
        if(!Path.IsPathFullyQualified(output)||
            !output.EndsWith(".html",StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Choose a valid HTML report filename.");
        // FileSavePicker often creates a zero-byte placeholder. Only delete that
        // placeholder, never replace an existing report containing user data.
        if(File.Exists(output))
        {
            if(new FileInfo(output).Length!=0)
                throw new IOException("Output file already contains data. Choose another filename.");
            File.Delete(output);
        }
        string[] args=kind switch {
            "energy"=>["/energy","/duration","30","/output",output],
            "sleepstudy"=>["/sleepstudy","/output",output],
            _=>throw new ArgumentException("Unsupported report kind.")
        };
        await RunAsync(args,kind=="energy"?90:50,cancellation);
        if(!File.Exists(output))throw new IOException(
            "Windows did not create the report. This tool may require administrator rights or Modern Standby.");
        return output;
    }
    private static async Task<string> RunAsync(IReadOnlyList<string> args,int seconds,CancellationToken cancellation)
    {
        using var linked=CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        linked.CancelAfter(TimeSpan.FromSeconds(seconds));
        var psi=new ProcessStartInfo("powercfg.exe"){
            UseShellExecute=false,CreateNoWindow=true,
            RedirectStandardOutput=true,RedirectStandardError=true,
            StandardOutputEncoding=Encoding.UTF8,StandardErrorEncoding=Encoding.UTF8
        };
        foreach(var arg in args)psi.ArgumentList.Add(arg);
        using var process=Process.Start(psi)??throw new IOException("Could not launch Windows powercfg.");
        try{
            var stdout=process.StandardOutput.ReadToEndAsync(linked.Token);
            var stderr=process.StandardError.ReadToEndAsync(linked.Token);
            await process.WaitForExitAsync(linked.Token);
            string output=await stdout, error=await stderr;
            if(process.ExitCode!=0)throw new IOException(
                $"powercfg returned {process.ExitCode}: {error.Trim()} {output.Trim()}");
            return string.IsNullOrWhiteSpace(output)?error:output;
        }catch(OperationCanceledException){
            if(!process.HasExited)process.Kill(entireProcessTree:true);
            throw new TimeoutException($"Windows diagnostic exceeded {seconds} seconds.");
        }
    }
    public static bool IsAvailable(string exe){
        if(exe is not ("wpr.exe" or "wpa.exe" or "procmon.exe"))return false;
        foreach(var folder in (Environment.GetEnvironmentVariable("PATH")??"")
            .Split(Path.PathSeparator,StringSplitOptions.RemoveEmptyEntries))
        {
            try{if(File.Exists(Path.Combine(folder,exe)))return true;}
            catch{ /* Invalid PATH entry */ }
        }
        return File.Exists(Path.Combine(Environment.SystemDirectory,exe));
    }
}
