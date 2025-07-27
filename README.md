# Youtube Downloader Console

A simple yet powerful console application for downloading YouTube videos. This tool allows you to download videos at a preferred quality, automatically fetches the best available audio, and converts them directly into a single, ready-to-watch MP4 file.

## Features

- **Download from URL**: Simply provide a YouTube video URL to start.
- **Configurable Quality**: Set your preferred video quality (e.g., 1080p, 720p, 480p) via a configuration file.
- **Smart Fallback**: If the preferred quality isn't available, the app automatically finds the next best available quality.
- **Best Audio**: Automatically selects the best compatible audio stream.
- **Direct MP4 Conversion**: Uses `YoutubeExplode.Converter` to download and convert the streams into a final MP4 file in one seamless step.
- **User-Friendly Interface**: Displays a clean progress bar for the download and conversion process, powered by Spectre.Console.
- **Custom Output Path**: Configure a specific folder for your downloads.
- **Safe Filenames**: Automatically sanitizes video titles to create valid filenames.

## Getting Started

Follow these instructions to get the project up and running on your local machine.

### Prerequisites

1.  **.NET SDK**: This project is built with .NET 9 but should be compatible with .NET 8 or newer.
    - [Download .NET SDK](https://dotnet.microsoft.com/download)

2.  **FFmpeg**: This tool is required by `YoutubeExplode.Converter` to combine the video and audio streams into a single MP4 file.
    - **You must place `ffmpeg.exe` in the same directory as the application's executable.** This project is already configured to do this if you place it in the project's root folder.
    - [Download FFmpeg](https://ffmpeg.org/download.html) (Get a static build for your OS from the official site or a trusted source like BtbN/Gyan.dev).

### Installation & Setup

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/DNsam/YoutubeDownloader.git
    ```

2.  **Place FFmpeg**: Download `ffmpeg.exe` and place it in the root directory of the cloned project.

3.  **Configure the application:**
    - Change a file named `appsettings.json` in the root of the project.
    - Copy the following structure into it and adjust the values as needed.

    **`appsettings.json`**
    ```json
    {
        "PreferredQuality": 720,
        "OutputPath": ""
    }
    ```

4.  **Run the application:**
    ```sh
    dotnet run
    ```
    The application will then prompt you to enter a YouTube video URL.

## Configuration Details

The `appsettings.json` file controls the application's behavior.

-   **`PreferredQuality`** (integer): The desired video height in pixels (e.g., `1080`, `720`, `480`). The application will first try to find a stream matching this quality. If not found, it will look for the best available quality *below* the preferred one.
-   **`OutputPath`** (string): The full path to the directory where the final video file will be saved.
    - If left empty (`""`), it defaults to the system's "Public Videos" folder (`C:\Users\Public\Videos` on Windows).
    - **Windows Example**: `"C:\\Users\\YourUser\\Videos"` (note the double backslashes).
    - **Linux/macOS Example**: `"/home/youruser/Videos"`

## Technologies Used

This project is built with the help of several fantastic open-source libraries:

-   [**YoutubeExplode**](https://github.com/Tyrrrz/YoutubeExplode) - For fetching video metadata and stream information from YouTube.
-   [**YoutubeExplode.Converter**](https://github.com/Tyrrrz/YoutubeExplode#converter-integration) - A companion library for YoutubeExplode that handles downloading and muxing streams using FFmpeg.
-   [**Spectre.Console**](https://github.com/spectreconsole/spectre.console) - For creating beautiful and user-friendly console interfaces.
-   [**Microsoft.Extensions.Configuration**](https://www.nuget.org/packages/Microsoft.Extensions.Configuration/) - For easy handling of the `appsettings.json` configuration.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.
