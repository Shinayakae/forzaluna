using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ForzaHorizon6AutoshowUnlocker
{
    public sealed partial class MainForm
    {
        private static readonly List<Stream> AnimatedCreditImageStreams = new List<Stream>();

        private void BuildCreditsPage()
        {
            _creditsPage.AutoScroll = true;
            AddPageHeader(_creditsPage, "Credits", "The people and tools that helped Luna become possible.");

            var card = MakeCard(_creditsPage, 0, 72, ContentWidth, 622, "Project Credits", "Thank you for the work, testing, ideas, and support.");
            card.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            const int gap = 18;
            const int left = 18;
            const int top = 72;
            const int rowHeight = 118;
            var bannerWidth = (ContentWidth - 36 - gap) / 2;
            AddCreditBanner(card, left, top, bannerWidth, "Shina", "The Creator", "Shina Profile Picture.png", Color.FromArgb(240, 101, 149));
            AddCreditBanner(card, left + bannerWidth + gap, top, bannerWidth, "Forza Mods", "AIO reference", "forza mods profile.gif", Color.FromArgb(77, 171, 247));
            AddCreditBanner(card, left, top + rowHeight + gap, bannerWidth, "Matkhl", "Database", "Matkhl Profile Picture.png", Color.FromArgb(255, 146, 43));
            AddCreditBanner(card, left + bannerWidth + gap, top + rowHeight + gap, bannerWidth, "Ariza", "Emotional Support", "Ariza Profile Picture.png", Color.FromArgb(81, 207, 102));
            AddCreditBanner(card, left, top + ((rowHeight + gap) * 2), bannerWidth, "Ken", "Teleport routes and community location work.", "Ken Profile Picture.png", Color.FromArgb(32, 201, 151));
            AddCreditBanner(card, left + bannerWidth + gap, top + ((rowHeight + gap) * 2), bannerWidth, "Patch", "Teleport routes and community location work.", "Patchy Profile Picture.png", Color.FromArgb(255, 212, 59));
            AddCreditBanner(card, left, top + ((rowHeight + gap) * 3), bannerWidth, "Merika", "Reference", "Merika Profile Picture.png", Color.FromArgb(255, 107, 129));

            _creditsPage.AutoScrollMinSize = new Size(ContentWidth, card.Bottom + 30);
        }

        private void AddCreditBanner(Control parent, int x, int y, int width, string name, string subtitle, string imageFile, Color accent)
        {
            var banner = new ModernPanel();
            banner.Location = new Point(x, y);
            banner.Size = new Size(width, 118);
            banner.FillColor = Blend(SurfaceAlt, accent, 0.06F);
            banner.BorderColor = Blend(Border, accent, 0.34F);
            banner.CornerRadius = 10;
            banner.BorderWidth = 1F;
            banner.Tag = "CreditCard";
            parent.Controls.Add(banner);

            var picture = new CircularPictureBox();
            picture.Location = new Point(20, 23);
            picture.Size = new Size(72, 72);
            picture.BackColor = Color.Transparent;
            picture.BorderColor = accent;
            picture.BorderWidth = 2F;
            picture.SizeMode = PictureBoxSizeMode.Zoom;
            picture.Image = LoadLocalAssetImage(imageFile);
            banner.Controls.Add(picture);

            var title = new Label();
            title.Text = name;
            title.Location = new Point(112, 32);
            title.Size = new Size(width - 134, 26);
            title.BackColor = Color.Transparent;
            title.ForeColor = TextPrimary;
            title.Font = new Font("Segoe UI Semibold", 11F);
            banner.Controls.Add(title);

            var sub = new Label();
            sub.Text = subtitle;
            sub.Location = new Point(112, 62);
            sub.Size = new Size(width - 134, 40);
            sub.BackColor = Color.Transparent;
            sub.ForeColor = TextMuted;
            sub.Font = new Font("Segoe UI", 8.75F);
            banner.Controls.Add(sub);
        }

        private static Image LoadEmbeddedImage(string resourceName)
        {
            try
            {
                using (var stream = typeof(Program).Assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        return null;

                    if (IsAnimatedGifName(resourceName))
                    {
                        var memory = new MemoryStream();
                        stream.CopyTo(memory);
                        memory.Position = 0;
                        try
                        {
                            var animated = Image.FromStream(memory);
                            lock (AnimatedCreditImageStreams)
                            {
                                AnimatedCreditImageStreams.Add(memory);
                            }
                            return animated;
                        }
                        catch
                        {
                            memory.Dispose();
                            throw;
                        }
                    }

                    using (var image = Image.FromStream(stream))
                        return new Bitmap(image);
                }
            }
            catch
            {
                return null;
            }
        }

        private static Image LoadLocalAssetImage(string fileName)
        {
            var embedded = LoadEmbeddedImage(fileName);
            if (embedded != null)
                return embedded;

            try
            {
                foreach (var path in GetLocalAssetCandidates(fileName))
                {
                    if (!File.Exists(path))
                        continue;

                    if (IsAnimatedGifName(path))
                        return Image.FromFile(path);

                    using (var image = Image.FromFile(path))
                        return new Bitmap(image);
                }
            }
            catch
            {
            }

            return null;
        }

        private static bool IsAnimatedGifName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) &&
                fileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);
        }

        private static Image LoadLocalAssetIconImage(string fileName, Color tint)
        {
            using (var image = LoadEmbeddedImage(fileName) ?? LoadLocalAssetImage(fileName))
            {
                if (image == null)
                    return null;

                return TintIconImage(image, tint);
            }
        }

        private static Image TintIconImage(Image image, Color tint)
        {
            var source = new Bitmap(image);
            var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            for (var y = 0; y < source.Height; y++)
            {
                for (var x = 0; x < source.Width; x++)
                {
                    var pixel = source.GetPixel(x, y);
                    if (pixel.A == 0)
                    {
                        result.SetPixel(x, y, Color.Transparent);
                        continue;
                    }

                    var luminance = (pixel.R * 0.299F) + (pixel.G * 0.587F) + (pixel.B * 0.114F);
                    var alpha = (int)Math.Round(pixel.A * Math.Max(0.0F, Math.Min(1.0F, (255F - luminance) / 255F)));
                    if (alpha < 10)
                    {
                        result.SetPixel(x, y, Color.Transparent);
                        continue;
                    }

                    result.SetPixel(x, y, Color.FromArgb(alpha, tint));
                }
            }

            source.Dispose();
            return result;
        }

        private static IEnumerable<string> GetLocalAssetCandidates(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                yield break;

            var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            yield return Path.Combine(downloads, fileName);
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots", fileName);
            yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credits", fileName);
            yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "credits", fileName);
            yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", fileName);
            yield return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        private void ShowCreditsPage()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ShowCreditsPage);
                return;
            }
            HidePages();
            _creditsPage.Visible = true;
            _creditsPage.BringToFront();
            SetStatus("Credits");
            UpdateNavigationState(_navCredits);
        }
    }
}
