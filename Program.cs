using Microsoft.Extensions.Configuration;
using Spectre.Console;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

try
{
    // --- 1. Load Configuration ---
    var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build()
        .Get<AppSettings>() ?? new AppSettings();

    var youtube = new YoutubeClient();


    Console.Write("Enter a YouTube video URL: ");
    string? videoUrl = Console.ReadLine();
    if (string.IsNullOrEmpty(videoUrl))
    {
        Console.WriteLine("Oops! You forgot to enter a YouTube link.");
        Console.ReadLine();
        return;
    }

    // --- 2. Get Video Information ---
    var video = await youtube.Videos.GetAsync(videoUrl);
    var outputFileName = SanitizeFilename(video.Title);

    AnsiConsole.MarkupLine($"Downloading: [bold cyan]{outputFileName}[/]");
    AnsiConsole.MarkupLine($"Duration: [bold cyan]{video.Duration}[/]");

    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);

    // --- 3. Select Video and Audio Streams ---
    var videoStreamInfo = streamManifest
        .GetVideoStreams()
        .Where(s => s.Container == Container.Mp4 && s.VideoQuality.MaxHeight == config.PreferredQuality)
        .OrderByDescending(s => s.VideoQuality.Framerate)
        .FirstOrDefault();

    // Fallback logic if preferred quality is not found
    if (videoStreamInfo == null)
    {
        AnsiConsole.MarkupLine($"[yellow]Video at {config.PreferredQuality}p not found. Looking for the best available alternative...[/]");

        videoStreamInfo = streamManifest
            .GetVideoStreams()
            .Where(s => s.Container == Container.Mp4 && s.VideoQuality.MaxHeight < config.PreferredQuality)
            .OrderByDescending(s => s.VideoQuality.MaxHeight + s.VideoQuality.Framerate)
            .FirstOrDefault();
        if (videoStreamInfo == null)
        {
            AnsiConsole.MarkupLine("[red]Error: No suitable MP4 video stream found.[/]");
            return;
        }
        AnsiConsole.MarkupLine($"[aqua]Found best alternative: {videoStreamInfo.VideoQuality.Label}[/]");
    }

    // Select the best audio stream
    var audioStreamInfo = streamManifest
        .GetAudioStreams()
        .Where(s => s.Container == Container.Mp4)
        .GetWithHighestBitrate();

    if (audioStreamInfo == null)
    {
        AnsiConsole.MarkupLine("[red]Error: No audio stream found.[/]");
        return;
    }

    // --- 4. Download Streams to Temporary Files ---

    //// Determine the output path. Default to user's "Downloads" folder for better UX.
    var outputPath = config.OutputPath ?? Environment.GetFolderPath(Environment.SpecialFolder.CommonVideos);
    var outputFile = Path.Combine(outputPath, $"{outputFileName}.mp4");

    await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
            var progressTask = ctx.AddTask($"[green]Downloading Video[/]");

            var progress = new Progress<double>(percent => progressTask.Increment(percent * 100 - progressTask.Percentage));

            await youtube.Videos.DownloadAsync(
                [audioStreamInfo, videoStreamInfo],
                new ConversionRequestBuilder(outputFile).Build(),
                progress
            );
        });

    AnsiConsole.MarkupLine($"[bold green]Video saved successfully![/]");
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine("[bold red]An error occurred:[/]");
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
}

Console.WriteLine("Press enter to exit..");
Console.ReadLine();

static string SanitizeFilename(string filename)
{
    if (string.IsNullOrWhiteSpace(filename))
    {
        return $"video{Random.Shared.Next()}";
    }

    foreach (var c in Path.GetInvalidFileNameChars())
    {
        filename = filename.Replace(c.ToString(), "");
    }

    if (string.IsNullOrWhiteSpace(filename))
    {
        return $"video{Random.Shared.Next()}";
    }

    const int MaxFileNameLength = 200;

    if (filename.Length > MaxFileNameLength)
    {
        filename = filename.Substring(0, MaxFileNameLength);
    }

    return filename;
}

public class AppSettings
{
    public int PreferredQuality { get; set; } = 480;
    public string? OutputPath { get; set; }
}
