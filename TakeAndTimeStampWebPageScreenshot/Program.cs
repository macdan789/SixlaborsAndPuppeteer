// See https://aka.ms/new-console-template for more information
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using PuppeteerSharp;
using System.Globalization;

const string TIME_STAMP_FORMAT = "dddd, dd MMMM yyyy HH:mm:ss tt";
const string FILE_NAME_FORMAT = "{0}_{2:yyyy-MM-dd_HH-mm-ss-fff}";
const string FONT_NAME = "Arial";
const string CULTURE_NAME = "en";
const int FONT_SIZE = 30;

DateTime _dateTimeNow = DateTime.Now;


var result = await TakeScreenshot("https://www.google.com/");
Console.WriteLine(result);


async Task<string> TakeScreenshot(string targetUrl)
{
    using var browserFetcher = new BrowserFetcher();
    await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

    string fileName = string.Format(FILE_NAME_FORMAT, "Screenshot", _dateTimeNow);

    var launchOptions = new LaunchOptions() { Headless = false };

    using var browser = await Puppeteer.LaunchAsync(launchOptions);
    using var page = await browser.NewPageAsync();
    await page.GoToAsync(targetUrl);
    await page.SetViewportAsync(new ViewPortOptions { Height = 1080, Width = 1920 });

    var screenshotStream = await page.ScreenshotStreamAsync(new ScreenshotOptions { Clip = new PuppeteerSharp.Media.Clip { Height = 1080, Width = 1920 } });
    
    using var resultStream = new MemoryStream();
    await TimeStampScreenshot(screenshotStream, resultStream);

    await File.WriteAllBytesAsync(@$"{fileName}.png", resultStream.ToArray());

    return "success";
}

async Task TimeStampScreenshot(Stream inputStream, Stream outputStream)
{
    Image inputImage = await Image.LoadAsync(inputStream);

    using Image<Rgba32> outputImage = new(inputImage.Width, inputImage.Height + FONT_SIZE + 5);

    string timestamp = _dateTimeNow.ToString(TIME_STAMP_FORMAT, CultureInfo.GetCultureInfo(CULTURE_NAME));

    Font font = SystemFonts.CreateFont(FONT_NAME, FONT_SIZE);

    var textOptions = new TextOptions(font)
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top
    };

    outputImage.Mutate(img => img
        .DrawImage(inputImage, new SixLabors.ImageSharp.Point(0, FONT_SIZE + 5), 1f)
        .DrawText(textOptions, timestamp, Color.Red)
    );

    await outputImage.SaveAsPngAsync(outputStream);
}