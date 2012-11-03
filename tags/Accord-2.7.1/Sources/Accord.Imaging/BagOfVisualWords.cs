﻿// Accord Imaging Library
// The Accord.NET Framework
// http://accord.googlecode.com
//
// Copyright © César Souza, 2009-2012
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

namespace Accord.Imaging
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading;
    using System.Threading.Tasks;
    using Accord.MachineLearning;
    using Accord.Math;
    using AForge.Imaging;

    /// <summary>
    ///   Bag of Visual Words
    /// </summary>
    /// 
    /// <remarks>
    ///   The bag-of-words (BoW) model can be used to extract finite
    ///   length features from otherwise varying length representations.
    ///   This class uses the <see cref="SpeededUpRobustFeaturesDetector">
    ///   SURF features detector</see> to determine a coded representation
    ///   for a given image.
    /// </remarks>
    /// 
    /// <example>
    /// <code>
    ///   int numberOfWords = 32;
    ///   
    ///   // Create bag-of-words (BoW) with the given number of words
    ///   BagOfVisualWords bow = new BagOfVisualWords(numberOfWords);
    ///   
    ///   // Create the BoW codebook using a set of training images
    ///   bow.Compute(imageArray);
    ///   
    ///   // Create a fixed-length feature vector for a new image
    ///   bow.GetFeatureVector(image);
    /// </code>
    /// </example>
    /// 
    [Serializable]
    public class BagOfVisualWords
    {

        /// <summary>
        ///   Gets the number of words in this codebook.
        /// </summary>
        public int NumberOfWords { get; private set; }

        /// <summary>
        ///   Gets the K-Means algorithm used to create this model.
        /// </summary>
        /// 
        public IClusteringAlgorithm<double[]> Clustering { get; private set; }

        /// <summary>
        ///   Gets the <see cref="SpeededUpRobustFeaturesDetector">SURF</see>
        ///   feature point detector used to identify visual features in images.
        /// </summary>
        /// 
        public SpeededUpRobustFeaturesDetector Surf { get; private set; }


        /// <summary>
        ///   Constructs a new <see cref="BagOfVisualWords"/>.
        /// </summary>
        /// 
        /// <param name="numberOfWords">The number of codewords.</param>
        /// 
        public BagOfVisualWords(int numberOfWords)
        {
            this.NumberOfWords = numberOfWords;
            this.Clustering = new KMeans(numberOfWords);
            this.Surf = new SpeededUpRobustFeaturesDetector();
        }

        /// <summary>
        ///   Constructs a new <see cref="BagOfVisualWords"/>.
        /// </summary>
        /// 
        /// <param name="algorithm">The clustering algorithm to use.</param>
        /// 
        public BagOfVisualWords(IClusteringAlgorithm<double[]> algorithm)
        {
            this.NumberOfWords = algorithm.Clusters.Count;
            this.Clustering = algorithm;
            this.Surf = new SpeededUpRobustFeaturesDetector();
        }

        /// <summary>
        ///   Computes the Bag of Words model.
        /// </summary>
        /// 
        /// <param name="images">The set of images to initialize the model.</param>
        /// <param name="threshold">Convergence rate for the k-means algorithm. Default is 1e-5.</param>
        /// 
        /// <returns>The list of feature points detected in all images.</returns>
        /// 
        public List<SpeededUpRobustFeaturePoint>[] Compute(Bitmap[] images, double threshold = 1e-5)
        {

            var descriptors = new List<double[]>();
            var imagePoints = new List<SpeededUpRobustFeaturePoint>[images.Length];

            // For all images
            for (int i = 0; i < images.Length; i++)
            {
                Bitmap image = images[i];

                // Compute the feature points
                var points = Surf.ProcessImage(image);

                foreach (var point in points)
                    descriptors.Add(point.Descriptor);

                imagePoints[i] = points;
            }

            // Compute K-Means of the descriptors
            double[][] data = descriptors.ToArray();

            Clustering.Compute(data, threshold);

            return imagePoints;
        }

        /// <summary>
        ///   Gets the codeword representation of a given image.
        /// </summary>
        /// 
        /// <param name="image">The image to be processed.</param>
        /// 
        /// <returns>A double vector with the same length as words
        /// in the code book.</returns>
        /// 
        public double[] GetFeatureVector(Bitmap image)
        {
            // lock source image
            BitmapData imageData = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, image.PixelFormat);

            double[] features;

            try
            {
                // process the image
                features = GetFeatureVector(new UnmanagedImage(imageData));
            }
            finally
            {
                // unlock image
                image.UnlockBits(imageData);
            }

            return features;
        }

        /// <summary>
        ///   Gets the codeword representation of a given image.
        /// </summary>
        /// 
        /// <param name="image">The image to be processed.</param>
        /// 
        /// <returns>A double vector with the same length as words
        /// in the code book.</returns>
        /// 
        public double[] GetFeatureVector(UnmanagedImage image)
        {
            // Detect feature points in image
            List<SpeededUpRobustFeaturePoint> points = Surf.ProcessImage(image);

            return GetFeatureVector(points);
        }

        /// <summary>
        ///   Gets the codeword representation of a given image.
        /// </summary>
        /// 
        /// <param name="points">The interest points of the image.</param>
        /// 
        /// <returns>A double vector with the same length as words
        /// in the code book.</returns>
        /// 
        public double[] GetFeatureVector(List<SpeededUpRobustFeaturePoint> points)
        {
            int[] features = new int[NumberOfWords];

            // Detect all activation centroids
            Parallel.For(0, points.Count, i =>
            {
                int j = Clustering.Clusters.Nearest(points[i].Descriptor);

                // Form feature vector
                Interlocked.Increment(ref features[j]);
            });

            return features.ToDouble();
        }

    }
}
