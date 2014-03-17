﻿using System;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Windows;

namespace LightBuzz.Vitruvius.WPF
{
    /// <summary>
    /// Provides some common functionality for manipulating depth frames.
    /// </summary>
    public static class DepthExtensions
    {
        #region Members

        /// <summary>
        /// The bitmap source.
        /// </summary>
        static WriteableBitmap _bitmap = null;

        /// <summary>
        /// The depth values.
        /// </summary>
        static ushort[] _depthData = null;

        /// <summary>
        /// The body index values.
        /// </summary>
        static byte[] _bodyData = null;

        /// <summary>
        /// The RGB pixel values.
        /// </summary>
        static byte[] _pixels = null;

        #endregion

        #region Public methods

        /// <summary>
        /// Converts a depth frame to the corresponding System.Windows.Media.Imaging.BitmapSource.
        /// </summary>
        /// <param name="frame">The specified depth frame.</param>
        /// <returns>The corresponding System.Windows.Media.Imaging.BitmapSource representation of the depth frame.</returns>
        public static BitmapSource ToBitmap(this DepthFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;

            ushort minDepth = frame.DepthMinReliableDistance;
            ushort maxDepth = frame.DepthMaxReliableDistance;

            if (_bitmap == null)
            {
                _depthData = new ushort[width * height];
                _pixels = new byte[width * height * Constants.BYTES_PER_PIXEL];
                _bitmap = new WriteableBitmap(width, height, 96.0, 96.0, Constants.FORMAT, null);
            }

            frame.CopyFrameDataToArray(_depthData);

            // Convert the depth to RGB.
            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < _depthData.Length; ++depthIndex)
            {
                // Get the depth for this pixel
                ushort depth = _depthData[depthIndex];

                // To convert to a byte, we're discarding the most-significant
                // rather than least-significant bits.
                // We're preserving detail, although the intensity will "wrap."
                // Values outside the reliable depth range are mapped to 0 (black).
                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                _pixels[colorIndex++] = intensity; // Blue
                _pixels[colorIndex++] = intensity; // Green
                _pixels[colorIndex++] = intensity; // Red

                // We're outputting BGR, the last byte in the 32 bits is unused so skip it
                // If we were outputting BGRA, we would write alpha here.
                ++colorIndex;
            }

            _bitmap.WritePixels(new Int32Rect(0, 0, width, height), _pixels, width * Constants.BYTES_PER_PIXEL, 0);

            return _bitmap;
        }

        /// <summary>
        /// Converts a depth frame to the corresponding System.Windows.Media.Imaging.BitmapSource with the players highlighted.
        /// </summary>
        /// <param name="depthFrame">The specified depth frame.</param>
        /// <param name="bodyIndexFrame">The specified body index frame.</param>
        /// <returns>The corresponding System.Windows.Media.Imaging.BitmapSource representation of the depth frame.</returns>
        public static BitmapSource ToBitmap(this DepthFrame depthFrame, BodyIndexFrame bodyIndexFrame)
        {
            int width = depthFrame.FrameDescription.Width;
            int height = depthFrame.FrameDescription.Height;

            ushort minDepth = depthFrame.DepthMinReliableDistance;
            ushort maxDepth = depthFrame.DepthMaxReliableDistance;

            if (_bodyData == null)
            {
                _depthData = new ushort[width * height];
                _bodyData = new byte[width * height];
                _pixels = new byte[width * height * Constants.BYTES_PER_PIXEL];
                _bitmap = new WriteableBitmap(width, height, 96.0, 96.0, Constants.FORMAT, null);
            }

            depthFrame.CopyFrameDataToArray(_depthData);
            bodyIndexFrame.CopyFrameDataToArray(_bodyData);

            // Convert the depth to RGB
            for (int depthIndex = 0, colorPixelIndex = 0; depthIndex < _depthData.Length && colorPixelIndex < _pixels.Length; depthIndex++, colorPixelIndex += 4)
            {
                // Get the depth for this pixel
                ushort depth = _depthData[depthIndex];
                byte player = _bodyData[depthIndex];

                // To convert to a byte, we're discarding the most-significant
                // rather than least-significant bits.
                // We're preserving detail, although the intensity will "wrap."
                // Values outside the reliable depth range are mapped to 0 (black).
                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                if (player != 0xff)
                {
                    // Color player gold.
                    _pixels[colorPixelIndex + 0] = Colors.Gold.B; // B
                    _pixels[colorPixelIndex + 1] = Colors.Gold.G; // G
                    _pixels[colorPixelIndex + 2] = Colors.Gold.R; // R
                }
                else
                {
                    // Color the rest of the image in grayscale.
                    _pixels[colorPixelIndex + 0] = intensity; // B
                    _pixels[colorPixelIndex + 1] = intensity; // G
                    _pixels[colorPixelIndex + 2] = intensity; // R
                }
            }

            _bitmap.WritePixels(new Int32Rect(0, 0, width, height), _pixels, width * Constants.BYTES_PER_PIXEL, 0);

            return _bitmap;
        }

        #endregion
    }
}
