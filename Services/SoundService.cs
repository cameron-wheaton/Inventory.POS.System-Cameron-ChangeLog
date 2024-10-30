using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Views;

namespace INVApp.Services
{
    public class SoundService
    {

        private MediaElement _mediaElement;

        public SoundService()
        {
            _mediaElement = new MediaElement
            {
                IsVisible = false, // Don't display media controls
                ShouldAutoPlay = false   // Control playback manually
            };
        }

        public void Initialize(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
            PreloadSound("scan_beep.mp3");
        }

        public async Task PlaySoundAsync(string soundFileName)
        {
            if (_mediaElement != null)
            {
                try
                {
                    _mediaElement.Source = MediaSource.FromResource(soundFileName); 
                    _mediaElement.Play();
                    await Task.Delay(1000); 
                    _mediaElement.Stop();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing sound: {ex.Message}");
                }
            }
        }

        private void PreloadSound(string soundFileName)
        {
            if (_mediaElement != null)
            {
                try
                {
                    _mediaElement.Source = MediaSource.FromResource(soundFileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error preloading sound: {ex.Message}");
                }
            }
        }

        public void ApplyVolume(double volume)
        {
            double scaledVolume = volume / 100.0;

            if (scaledVolume < 0.0 || scaledVolume > 1.0)
            {
                Console.WriteLine($"Invalid volume value: {volume}. It must be between 0 and 100.");
                return; 
            }

            _mediaElement.Volume = scaledVolume;
        }
    }
}
