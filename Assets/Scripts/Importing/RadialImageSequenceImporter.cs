using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    ///     Converts a directory of image slices into a VolumeDataset for volumetric rendering.
    /// </summary>
    public class RadialImageSequenceImporter : DatasetImporterBase
    {
        private readonly string directoryPath;

        private readonly string[] supportedImageTypes =
        {
            "*.png",
            "*.jpg",
            "*.bmp"
        };

        private List<double> imageAngles;

        public RadialImageSequenceImporter(string directoryPath)
        {
            this.directoryPath = directoryPath;
        }

        public override VolumeDataset Import()
        {
            if (!Directory.Exists(directoryPath))
                throw new NullReferenceException("No directory found: " + directoryPath);

            var imagePaths = GetSortedImagePaths();
            // imageAngles = GetAngles();

            if (!ImageSetHasUniformDimensions(imagePaths))
                throw new IndexOutOfRangeException("Image sequence has non-uniform dimensions");

            var dimensions = GetVolumeDimensions(imagePaths);
            var data = FillSequentialData(dimensions, imagePaths);
            var dataset = FillVolumeDataset(data, dimensions);

            return dataset;
        }

        private List<double> GetAngles()
        {
            var angles = new List<double>();
            //TODO: Add try-catch
            var file = File.ReadAllLines(directoryPath + "/CaptSave.tag");
            foreach (var line in file)
            {
                var splitted = line.Split(';');
                angles.Add(double.Parse(splitted[10], CultureInfo.InvariantCulture));
            }

            return angles;
        }

        /// <summary>
        ///     Gets every file path in the directory with a supported suffix.
        /// </summary>
        /// <returns>A sorted list of image file paths.</returns>
        private List<string> GetSortedImagePaths()
        {
            var imagePaths = new List<string>();

            foreach (var type in supportedImageTypes) imagePaths.AddRange(Directory.GetFiles(directoryPath, type));

            imagePaths.Sort();

            return imagePaths;
        }

        /// <summary>
        ///     Checks if every image in the set has the same XY dimensions.
        /// </summary>
        /// <param name="imagePaths">The list of image paths to check.</param>
        /// <returns>True if at least one image differs from another.</returns>
        private bool ImageSetHasUniformDimensions(List<string> imagePaths)
        {
            var hasUniformDimension = true;

            Vector2Int previous, current;
            previous = GetImageDimensions(imagePaths[0]);

            foreach (var path in imagePaths)
            {
                current = GetImageDimensions(path);

                if (current.x != previous.x || current.y != previous.y)
                {
                    hasUniformDimension = false;
                    break;
                }

                previous = current;
            }

            return hasUniformDimension;
        }

        /// <summary>
        ///     Gets the XY dimensions of an image at the path.
        /// </summary>
        /// <param name="path">The image path to check.</param>
        /// <returns>The XY dimensions of the image.</returns>
        private Vector2Int GetImageDimensions(string path)
        {
            var bytes = File.ReadAllBytes(path);

            var texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);

            var dimensions = new Vector2Int
            {
                x = texture.width,
                y = texture.height
            };

            return dimensions;
        }

        /// <summary>
        ///     Adds a depth value Z to the XY dimensions of the first image.
        /// </summary>
        /// <param name="paths">The set of image paths comprising the volume.</param>
        /// <returns>The dimensions of the volume.</returns>
        private Vector3Int GetVolumeDimensions(List<string> paths)
        {
            var twoDimensional = GetImageDimensions(paths[0]);
            var threeDimensional = new Vector3Int
            {
                x = twoDimensional.x,
                y = twoDimensional.y,
                z = paths.Count
            };
            return threeDimensional;
        }

        /// <summary>
        ///     Converts a volume set of images into a sequential series of values.
        /// </summary>
        /// <param name="dimensions">The XYZ dimensions of the volume.</param>
        /// <param name="paths">The set of image paths comprising the volume.</param>
        /// <returns>The set of sequential values for the volume.</returns>
        private int[] FillSequentialData(Vector3Int dimensions, List<string> paths)
        {
            var data = new List<int>(dimensions.x * dimensions.y * dimensions.z);
            var texture = new Texture2D(1, 1);

            foreach (var path in paths)
            {
                var bytes = File.ReadAllBytes(path);
                texture.LoadImage(bytes);
                var pixels = texture.GetPixels(); // Order priority: X -> Y -> Z
                var imageData = DensityHelper.ConvertColorsToDensities(pixels);

                data.AddRange(imageData);
            }

            return data.ToArray();
        }

        /// <summary>
        ///     Wraps volume data into a VolumeDataset.
        /// </summary>
        /// <param name="data">Sequential value data for a volume.</param>
        /// <param name="dimensions">The XYZ dimensions of the volume.</param>
        /// <returns>The wrapped volume data.</returns>
        private VolumeDataset FillVolumeDataset(int[] data, Vector3Int dimensions)
        {
            var name = Path.GetFileName(directoryPath);

            var dataset = ScriptableObject.CreateInstance<VolumeDataset>();
            dataset.name = name;
            dataset.datasetName = name;
            dataset.dimX = dimensions.x;
            dataset.dimY = dimensions.y;
            dataset.dimZ = dimensions.z;
            dataset.scaleX = 1f;
            dataset.scaleY = (float) dimensions.y / dimensions.x;
            dataset.scaleZ = (float) (dimensions.z / (float) dimensions.x);
            dataset.data = data;

            return dataset;
        }
    }
}