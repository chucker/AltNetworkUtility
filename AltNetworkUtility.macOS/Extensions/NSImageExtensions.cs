using System;

using AppKit;

using CoreGraphics;

using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace AltNetworkUtility.macOS.Extensions
{
    public static class NSImageExtensions
    {
        public static NSImage WithTintColor(this NSImage image, Color color)
        {
            if (!image.Template)
                return image;

            var copiedImage = image.Copy() as NSImage;
            copiedImage.LockFocus();

            var tintColor = color.ToNSColor();
            tintColor.Set();

            var bounds = new CGRect(0, 0, copiedImage.Size.Width, copiedImage.Size.Height);
            NSGraphics.RectFill(bounds, NSCompositingOperation.SourceAtop);

            copiedImage.Template = false;
            copiedImage.UnlockFocus();

            return copiedImage;
        }

        public static NSImage WithChangedSize(this NSImage image, CGSize desiredSize)
        {
            // XamForms has ResizeTo, but it's not proportional (as of 5.0.0)

            if (image == null)
                return null;

            image.ResizingMode = NSImageResizingMode.Stretch;

            var ratioX = desiredSize.Width / image.Size.Width;
            var ratioY = desiredSize.Height / image.Size.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newSize = new CGSize(image.Size.Width * ratio, image.Size.Height * ratio);

            var resizedImage = new NSImage(newSize);

            resizedImage.LockFocus();
            image.Size = newSize;
            NSGraphicsContext.CurrentContext.ImageInterpolation = NSImageInterpolation.High;
            image.Draw(CGPoint.Empty, new CGRect(0, 0, newSize.Width, newSize.Height),
                NSCompositingOperation.Copy, 1.0f);
            resizedImage.UnlockFocus();

            return resizedImage;
        }
    }
}
